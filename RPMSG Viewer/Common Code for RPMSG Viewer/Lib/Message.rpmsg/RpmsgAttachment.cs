using System;
using OpenMcdf;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SI.Mobile.RPMSGViewer.Lib;
using System.Linq;
using System.Security;

#if __ANDROID__
using Android.Webkit;
#endif

#if __IOS__
using Foundation;
#endif

namespace SI.Mobile.RPMSGViewer.Lib
{
	public class RpmsgAttachment
	{
		private enum AttachType
		{
			afByBalue = 1,
			afEmbeddedMessage = 5
		}

		//rpmgs consts
		public const string ATTACHMENT_LIST_STREAM_NAME = "Attachment List";
		public const string ATTACHMENT_INFO_STREAM_NAME = "Attachment Info";
		public const string ATTACHMENT_DECRIPTION_STREAM_NAME = "AttachDesc";

		//embedded msg consts
		private const string FIRST_MSG_ATTACHMENT_STORAGE_NAME = "__attach_version1.0_#00000000";
		private const string PidTagAttachDataBinary = "__substg1.0_37010102";
		private const string PidTagAttachLongFilename  = "__substg1.0_3707001F";
		private const string MIME_TYPE_STREAM_NAME = "__substg1.0_370E001F";
		private const string RPMSG_ATTACHMENT_NAME = "message.rpmsg";

		public string Name { get; private set;}
		public string Extension { get; private set;}
		public string MimeType { get; private set;}
		public byte[] Content { get; private set;}
		public string ContentID { get; private set; }
		private AttachType AttachmentMethod { get; set; }

		private string _UniqueId = null;
		public string UniqueID 
		{ 
			get 
			{
				if (_UniqueId == null)
					_UniqueId = Guid.NewGuid().ToString();

				return _UniqueId;
			}
		}

		[SecuritySafeCritical]
		public static List<RpmsgAttachment> CreateAttachmentsFromStorage(CompoundFile rpmsg)
		{
			var attachments = new List<RpmsgAttachment>();

			try
			{
				byte[] attachInfoBytes = rpmsg.RootStorage.GetStorage (ATTACHMENT_LIST_STREAM_NAME).GetStream(ATTACHMENT_INFO_STREAM_NAME).GetData();

				List<string> attachmentsList;
				using (MemoryStream stream = new MemoryStream (attachInfoBytes))
				{
					attachmentsList = ReadAttachmentsInfoStream(stream);
				}
					
				foreach (string attachment in attachmentsList) 
				{
					try
					{
						CFStorage storage = rpmsg.RootStorage.GetStorage(ATTACHMENT_LIST_STREAM_NAME).GetStorage (attachment);
						attachments.Add(new RpmsgAttachment(storage));
					}
					catch (Exception ex) 
					{
						LogUtils.Error("Error while trying to load attachment", ex);
					}
				}
			}
			catch(CFItemNotFound) 
			{
				//No attachments, no need to do anything
			}

			return attachments;
		}

		public RpmsgAttachment(CFStorage attachmentStorage)
		{
			LoadAttachmentProps(attachmentStorage);
		}

		protected static List<string> ReadAttachmentsInfoStream(Stream infoStream)
		{
			StreamUtils.ReadBytes (infoStream, 4);
			ushort fifthByte = StreamUtils.ReadBytes(infoStream, 1)[0];
			ushort attachmentNamesLength;
			if (fifthByte == 0xff)
			{
				attachmentNamesLength = BitConverter.ToUInt16(StreamUtils.ReadBytes(infoStream, 2), 0);
			}
			else
			{
				attachmentNamesLength = fifthByte;
			}

			string attachmentsNames = Encoding.Unicode.GetString(StreamUtils.ReadBytes(infoStream, attachmentNamesLength * 2));
			return attachmentsNames.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
		}

		[SecuritySafeCritical]
		public void LoadAttachmentProps(CFStorage attachmentStorage)
		{
			byte[] descBytes;
			try 
			{
				descBytes = attachmentStorage.GetStream("AttachDesc").GetData ();
			}
			catch(CFItemNotFound ex) 
			{
				throw new NotSupportedException ("Attachment description stream not found", ex);
			}

			if (descBytes.Length == 0)
				throw new ArgumentException ("no description bytes to read");

			int nextIndex = 2; //first two bytes are the version

			StringUtils.ReadStringA(descBytes, ref nextIndex);
			StringUtils.ReadStringA(descBytes, ref nextIndex);
			string displayName = StringUtils.ReadStringA(descBytes, ref nextIndex);
			StringUtils.ReadStringA(descBytes, ref nextIndex);
			StringUtils.ReadStringA(descBytes, ref nextIndex);
			string extension = StringUtils.ReadStringA(descBytes, ref nextIndex);

			nextIndex += 16;

			AttachmentMethod = (AttachType)BitConverter.ToUInt32(descBytes, nextIndex);
			nextIndex += 4;

			ContentID = StringUtils.ReadStringW(descBytes, ref nextIndex);
			StringUtils.ReadStringW(descBytes, ref nextIndex);
			StringUtils.ReadStringW(descBytes, ref nextIndex);
			StringUtils.ReadStringW(descBytes, ref nextIndex);
			string displayNameW = StringUtils.ReadStringW(descBytes, ref nextIndex);
			StringUtils.ReadStringW(descBytes, ref nextIndex);
			StringUtils.ReadStringW(descBytes, ref nextIndex);
			string extensionW = StringUtils.ReadStringW(descBytes, ref nextIndex);

			//there's other stuff here, but I'm not going to read it

			Name = displayName ?? displayNameW;
			Extension = extension ?? extensionW;

#if __ANDROID__
			MimeType = MimeTypeMap.Singleton.GetMimeTypeFromExtension(Extension == null ? null : Extension.Trim ('.'));
#endif

			switch (AttachmentMethod)
			{
				case AttachType.afByBalue:
					Content = attachmentStorage.GetStream ("AttachContents").GetData ();	
					return;
				case AttachType.afEmbeddedMessage:
					LoadRpmsgDataFromEmbeddedAttachment (attachmentStorage.GetStorage ("MAPIMessage"));
					return;
				default:
					LogUtils.Error ("unsupported attachment method: " + AttachmentMethod);
					Content = new byte[0];
					return;
			}
		}

		public void LoadRpmsgDataFromEmbeddedAttachment(CFStorage msgStorage)
		{
			LogUtils.Log ("");
			try
			{
				CFStorage firstAttachmentStorage = msgStorage.GetStorage(FIRST_MSG_ATTACHMENT_STORAGE_NAME);
				string fileName = Encoding.Unicode.GetString(firstAttachmentStorage.GetStream(PidTagAttachLongFilename).GetData());
				if (fileName.Equals(RPMSG_ATTACHMENT_NAME))
				{
					Content = firstAttachmentStorage.GetStream(PidTagAttachDataBinary).GetData();
					Extension = ".rpmsg";
					//embedded messages do not have an extension in the attachment props
					Name += ".msg"; 
					MimeType = Encoding.Unicode.GetString(firstAttachmentStorage.GetStream(MIME_TYPE_STREAM_NAME).GetData());
					return;
				}

				throw new CFItemNotFound("");
			}
			catch (CFItemNotFound)
			{
				LogUtils.Log("Embedded message is not protected");
				Content = new byte[0];
			}
		}
			
		public string GetAttachmentHTML(string base64Icon, string base64ShareIcon)
		{
			string attachmentTemplate = AppUtils.ReadResourceFile (Path.Combine (SIConstants.DEFAULT_RESOURCE_FOLDER, "MailHtml/Attachments/AttachmentButton.txt"));

			string siGuid = "10BF6356-9C77-4EE2-8A0F-B0F2BEFBC225"; //MUST match the guids in the attachmentTemplate
			string attachmentDiv = attachmentTemplate.Replace(HtmlReplaceString.ATTACH_NAME_ + siGuid, Name);
			attachmentDiv = attachmentDiv.Replace(HtmlReplaceString.ATTACH_ID_ + siGuid, UniqueID);
			attachmentDiv = attachmentDiv.Replace(HtmlReplaceString.ATTACH_SIZE_ + siGuid, GetReadableSize());
			attachmentDiv = attachmentDiv.Replace(HtmlReplaceString.ICON_PATH_ + siGuid, "data:image/png;base64," + base64Icon);
			attachmentDiv = attachmentDiv.Replace(HtmlReplaceString.SHARE_ICON_ + siGuid, "data:image/png;base64," + base64ShareIcon);

			return attachmentDiv;
		}

		private enum HtmlReplaceString
		{
			ATTACH_ID_,
			ICON_PATH_,
			ATTACH_NAME_,
			ATTACH_SIZE_,
			SHARE_ICON_
		}

		public string GetReadableSize()
		{
			int size = Content.Length;

			string[] suffixes = {"B", "KB", "MB", "GB", "TB", "PB"};

			string suffix = suffixes[0];
			for (int suffixIndex = 1; size >= 1024; size /= 1024, suffixIndex++ )
			{
				suffix = suffixes[suffixIndex];
			}
			return size + " " + suffix;
		}
	}
}

