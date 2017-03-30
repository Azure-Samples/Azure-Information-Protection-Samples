using System;
using System.IO;
using com.microsoft.rightsmanagement.mobile.viewer.lib;
using com.microsoft.rightsmanagement.windows.viewer.lib;
using Microsoft.InformationProtection.RMS;
using Microsoft.InformationProtection.RMS.Exceptions;
using Microsoft.InformationProtection.RMS.Interfaces;
using Microsoft.InformationProtection.RMS.MSIPC.Native;
using Microsoft.InformationProtection.RMS.Structures;

namespace com.microsoft.rightsmanagement.windows.viewer
{
	internal class RMSHandlerPfileWindows : RMSHandlerPfile
	{
		public ISerializedLicense License { get; set; }

		public string OriginalExtension { get; protected set; }

		protected override void DecryptImpl()
		{
			try
			{
				if (License == null)
					License = SerializedLicenseProvider.Instance.FromFile(FilePath);

				using (var output = new MemoryStream())
				{
					var newPath = IpcHelper.DecryptFileStream(new MemoryStream(EncryptedBytes, false), output, FilePath,
						new PromptContext(PromptContextFlag.None), DecryptFlags.OpenAsRMSAware);

					OriginalExtension = Path.GetExtension(newPath);

					OnDecryptSuccess(output.ToArray());
				}

				EUL = RmsUtils.GetEULFromLicense(License);
			}
			catch (RMSUnauthorizedException ex)
			{
				FinishWaitingWithError(new NoPermissionsException(ex.Message, "", License?.Issuer));
			}
			catch (NoPermissionsException ex)
			{
				FinishWaitingWithError(ex);
			}
			catch (Exception exception)
			{
				OnDecryptError(exception);
			}
		}

		//TODO: Remove this after Telemetry task is done
		public override void DecryptEnded()
		{
			//disables Telemetry
		}
	}
}