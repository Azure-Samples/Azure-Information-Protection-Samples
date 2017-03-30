using com.microsoft.rightsmanagement.mobile.viewer.lib;
using Microsoft.InformationProtection.RMS;
using Microsoft.InformationProtection.RMS.Interfaces;

namespace com.microsoft.rightsmanagement.windows.viewer.RMS
{
	public class PackagePfileWindows : PackagePfile
	{
		internal ISerializedLicense License { get; set; }

		public string OriginalExtension { get; private set; }

		public PackagePfileWindows(string filePath) : base(filePath)
		{
			License = SerializedLicenseProvider.Instance.FromFile(filePath);
		}

		protected override void OnBeforeDecrypt(RMSHandlerPfile handler)
		{
			var windowsHandler = handler as RMSHandlerPfileWindows;
			if (handler != null)
				windowsHandler.License = License;
		}

		protected override void OnAfterDecrypt(RMSHandlerPfile handler)
		{
			var windowsHandler = handler as RMSHandlerPfileWindows;
			OriginalExtension = windowsHandler.OriginalExtension;
		}

		public bool IsPFileExtension()
		{
			return m_filePath.EndsWith(".pfile");
		}
	}
}