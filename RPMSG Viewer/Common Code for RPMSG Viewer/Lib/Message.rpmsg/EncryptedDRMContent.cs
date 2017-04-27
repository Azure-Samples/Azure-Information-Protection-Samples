using System;

namespace SI.Mobile.RPMSGViewer.Lib
{
	public class EncryptedDRMContent
	{
		public EncryptedDRMContent (byte[] encryptedDRMContentBytes)
		{
			EncryptedDRMContentBytes = encryptedDRMContentBytes;
		}

		// Issuance license
		public byte[] EncryptedDRMContentBytes { get; private set; }
	}
}

