using System;

namespace SI.Mobile.RPMSGViewer.Lib
{
	public class PublishingLicense
	{
		public PublishingLicense(byte[] publishingLicenseBytes)
		{
			PublishingLicenseBytes = publishingLicenseBytes;
		}

		// Issuance license
		public byte[] PublishingLicenseBytes { get; private set; }
	}
}

