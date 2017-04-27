using System;
using System.IO;
using System.Text.RegularExpressions;
using Foundation;
using MSRightsManagementBinding.ios;
using SI.Mobile.RPMSGViewer.Lib;
using SI.Mobile.RPMSGViewer.Lib;

namespace SI.Mobile.RPMSGViewer.ios
{
	public class RpmsgHandlerIOS : RpmsgHandler
	{
		private const int DRM_USER_POLICY_HEADER_LENGTH = 8;

		public RpmsgHandlerIOS(MessageRpmsg messageRpmsg, DecryptSuccessDelegate onDecryptSuccess, DecryptErrorDelegate onDecryptError) :
			base(messageRpmsg, onDecryptSuccess, onDecryptError)
		{
		}

		public override void DecryptImpl()
		{
			try
			{
				LogUtils.Log("Decrypt");

				MSUserPolicy.UserPolicyWithSerializedPolicy (
					NSData.FromArray (m_MessageRpmsg._PublishingLicense.PublishingLicenseBytes),
					null,
					new AuthenticationCallback (OnDecryptError),
					new RMSConsentCallback (OnDecryptError),
					MSPolicyAcquisitionOptions.Default,
					OnPolicyAquireComplete);
			}
			catch(Exception ex) 
			{
				LogUtils.Error("Error while aquiring user policy", ex);
				OnDecryptError (ex);
			}
		}
			
		public const int NSERROR_CODE_RMS_NOPERMISSIONS = -4;
		public const string NSERROR_RMS_DOMAIN = "RMSDomain";

		private void OnPolicyAquireComplete(MSUserPolicy policy, NSError error)
		{
			LogUtils.Log ("OnPolicyAquireComplete");

			if (error != null) 
			{
				if (error.Code == NSERROR_CODE_RMS_NOPERMISSIONS && error.Domain == NSERROR_RMS_DOMAIN) 
				{
					OnDecryptError(new NoPermissionsException(error.ToString()));
					return;
				}

				LogUtils.Error("Creating policy failed with error");
				OnDecryptError(new Exception("Creating policy failed with error: " + error.ToString()));
				return;
			}

			if (policy == null) 
			{
				LogUtils.Error("Policy data is null");
				OnDecryptError(new Exception("Policy data is null"));
				return;
			}

			try
			{
				NSData protectedData = NSData.FromArray (m_MessageRpmsg._EncryptedDRMContent.EncryptedDRMContentBytes);
				nuint dataSize = protectedData.Length - DRM_USER_POLICY_HEADER_LENGTH;
				MSCustomProtectedData.CustomProtectedDataWithPolicy (policy, protectedData, DRM_USER_POLICY_HEADER_LENGTH, dataSize, OnDecryptComplete);
			}
			catch(Exception ex)
			{
				LogUtils.Error("Could not create Protected Data", ex);
				OnDecryptError(ex);
			}
		}

		private void OnDecryptComplete(MSCustomProtectedData data, NSError error)
		{
			try
			{
				LogUtils.Log ("OnDecryptComplete");

				if (error != null)
					throw new Exception(error.ToString());

				NSData nsdata = data.RetrieveData;
				byte[] dataBytes = new byte[nsdata.Length];
				System.Runtime.InteropServices.Marshal.Copy(nsdata.Bytes, dataBytes, 0, Convert.ToInt32(nsdata.Length));

				DRMContent drmContent = DRMContent.Parse(dataBytes);
				OnDecryptSuccess(drmContent, new EndUserLicense(data.UserPolicy));
			}
			catch (Exception ex)
			{
				LogUtils.Error("Error while decrypting data ", ex);
				OnDecryptError(ex);
			}
		}
	}
}