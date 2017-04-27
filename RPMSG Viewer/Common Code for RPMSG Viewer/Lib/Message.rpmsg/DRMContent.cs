using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using OpenMcdf;
using SI.Mobile.RPMSGViewer.Lib;
using System.Security;

namespace SI.Mobile.RPMSGViewer.Lib
{
	public class DRMContent
	{
		const string Stream_OutlookBodyStreamInfo = "OutlookBodyStreamInfo";
		const string Stream_BodyPTHTML = "BodyPT-HTML";
		const string Stream_BodyPTAsHTML = "BodyPTAsHTML";

		public string HTMLBody { get; set; }
		public byte[] HTMLBodyBytes { get; set; }
		public Encoding HTMLBodyEncoding { get; set; }
		public List<RpmsgAttachment> Attachments { get; set; } 

		[SecuritySafeCritical]
		public static DRMContent Parse(byte[] drmContentBytes)
		{
			DRMContent drmContent = new DRMContent();
			using (MemoryStream ms = new MemoryStream(drmContentBytes)) 
			{
				using (CompoundFile cf = new CompoundFile(ms)) 
				{
					byte[] OutlookBodyStreamInfoBytes = cf.RootStorage.GetStream(Stream_OutlookBodyStreamInfo).GetData();
					Int16 bodyType = BitConverter.ToInt16(OutlookBodyStreamInfoBytes, 0);
					Int32 codePage = BitConverter.ToInt32(OutlookBodyStreamInfoBytes, 2);

					try
					{
						drmContent.HTMLBodyBytes = cf.RootStorage.GetStream(Stream_BodyPTHTML).GetData();
					}
					catch (Exception ex)
					{
						throw new InvalidDataException("No message body stream found", ex);
					}

					drmContent.HTMLBodyEncoding = AppUtils.GetEncoding(codePage);
					string htmlBody = drmContent.HTMLBodyEncoding.GetString(drmContent.HTMLBodyBytes);

					// wrap plain text with HTML tags
					if (bodyType == 1)
						htmlBody = "<html><head></head><body>" + htmlBody + "</body></html>";
					
					drmContent.HTMLBody = htmlBody; 
					drmContent.Attachments = RpmsgAttachment.CreateAttachmentsFromStorage(cf);
					AppUtils.LoadImagesInHtml (drmContent);

					LogUtils.Log ("MsgRpmsgDecryptEnd");
				}

			}
			return drmContent;
		}
	}
}

