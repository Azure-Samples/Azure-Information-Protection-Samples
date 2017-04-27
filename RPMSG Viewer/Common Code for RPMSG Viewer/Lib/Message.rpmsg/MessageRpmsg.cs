using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using SI.Mobile.RPMSGViewer.Lib;
using OpenMcdf;
using System.IO;
using Ionic.Zlib;
using System.Security;

namespace SI.Mobile.RPMSGViewer.Lib
{
	public class MessageRpmsg
	{
		public PublishingLicense _PublishingLicense { get; set; }
		public EncryptedDRMContent _EncryptedDRMContent { get; set; }

		private const string RPMSG_PREFIX = "\x76\xE8\x04\x60\xC4\x11\xE3\x86";
		private const string STORAGE_DATASPACES = "\x0006DataSpaces";
		private const string STORAGE_TRANSFOMINFO = "TransformInfo";
		private const string STORAGE_DRMTRANSFOM = "\tDRMTransform";
		private const string STREAM_ISSUANCELICENSE = "\x0006Primary";
		private const string DRMCONTENT_STREAM = "\tDRMContent";
		private const string BODYPT_HTML_STREAM = "BodyPT-HTML";

		public static MessageRpmsg Parse(string fileUrl)
		{
			return Parse(File.ReadAllBytes(fileUrl));
		}

		[SecuritySafeCritical]
		public static MessageRpmsg Parse(byte[] compressedRpmsgBytes)
		{
			LogUtils.Log("");

			MessageRpmsg messageRpmsg = new MessageRpmsg ();

			byte[] decompressedRpmsgBytes = DecompressRpmsg(compressedRpmsgBytes);
			using (MemoryStream ms = new MemoryStream(decompressedRpmsgBytes))
			{
				using (CompoundFile cf = new CompoundFile (ms)) 
				{
					messageRpmsg._PublishingLicense = new PublishingLicense (ExtractPublishingLicenseBytes (cf));
					messageRpmsg._EncryptedDRMContent = new EncryptedDRMContent (ExtractDRMContent (cf));
				}
			}

			return messageRpmsg;
		}

		private static byte[] DecompressRpmsg(byte[] compressedBytes)
		{
			LogUtils.Log("");

			using (MemoryStream fsOut = new MemoryStream())
			{
				using (MemoryStream fsIn = new MemoryStream(compressedBytes))
				{
					byte[] header = new byte[12];
					byte[] compressedData = new byte[1];

					fsIn.Seek(RPMSG_PREFIX.Length, SeekOrigin.Begin);
					while (true)
					{
						if (fsIn.Read(header, 0, 12) != 12)
							break;

						//						int marker = BitConverter.ToInt32(header, 0);
						//						int sizeUncompressed = BitConverter.ToInt32(header, 4);
						int sizeCompressed = BitConverter.ToInt32(header, 8);

						if (sizeCompressed > compressedData.Length)
							compressedData = new byte[sizeCompressed];

						fsIn.Read(compressedData, 0, sizeCompressed);
						fsOut.Write(compressedData, 0, sizeCompressed);
					}
				}

				return ZlibStream.UncompressBuffer(fsOut.ToArray());
			}
		}

		[SecuritySafeCritical]
		private static byte[] ExtractPublishingLicenseBytes(CompoundFile cf)
		{
			CFStream PLStream = (CFStream)cf.RootStorage.GetStorage(STORAGE_DATASPACES)
				.GetStorage(STORAGE_TRANSFOMINFO)
				.GetStorage(STORAGE_DRMTRANSFOM)
				.GetStream(STREAM_ISSUANCELICENSE);

			int PLSize = (int)PLStream.Size;
			byte[] publishingLicenseBytes = PLStream.GetData(173, ref PLSize);

			new byte[] { 0xEF, 0xBB, 0xBF }.CopyTo(publishingLicenseBytes, 0);
			Array.Resize(ref publishingLicenseBytes, publishingLicenseBytes.Length + 2);

			return publishingLicenseBytes;
		}

		[SecuritySafeCritical]
		private static byte[] ExtractDRMContent(CompoundFile cf)
		{
			CFStream DRMContentStream = (CFStream)cf.RootStorage.GetStream(DRMCONTENT_STREAM);

			int DRMContentSize = (int)DRMContentStream.Size;
			return DRMContentStream.GetData(0, ref DRMContentSize);
		}
	}
}
