using System;
using Com.Microsoft.Rightsmanagement;
using Com.Microsoft.Rightsmanagement.Exceptions;
using SI.Mobile.RPMSGViewer.android.RMS;
using SI.Mobile.RPMSGViewer.Lib;

namespace SI.Mobile.RPMSGViewer.android
{
	public class RpmsgHandlerAndroid: RpmsgHandler
	{
		private MainActivity m_MainActivity { get; set; }

		EndUserLicense m_UserLicense = null;

		public RpmsgHandlerAndroid(MessageRpmsg messageRpmsg, DecryptSuccessDelegate onDecryptSuccess, DecryptErrorDelegate onDecryptError, MainActivity mainActivity) :
			base(messageRpmsg, onDecryptSuccess, onDecryptError)
		{
			LogUtils.Log("RpmsgHandlerAndroid with fileStream");
			m_MainActivity = mainActivity;		
		}
			
		public override void DecryptImpl()
		{
			LogUtils.Log("Decrypt");

			try
			{				
				AuthenticationRequestCallback authenticationRequestCallback = new AuthenticationRequestCallback(
					m_MainActivity,
					Settings.GetPreferenceValueById(Resource.String.Settings_KeepRMSCredentials_Key, true));

				authenticationRequestCallback.CancelEvent += AuthenticationCompleteCallback_OnCancel;
				authenticationRequestCallback.SuccessEvent += AuthenticationCompleteCallback_OnSuccess;
				authenticationRequestCallback.FailureEvent += AuthenticationCompleteCallback_OnError;
		
				RMSPolicyCreationCallback policyCreationCallback = new RMSPolicyCreationCallback(OnUserPolicyAcquisitionComplete);
				policyCreationCallback.CancelEvent += RMSPolicyCreationCallback_OnCancel;
				policyCreationCallback.FailureEvent += RMSPolicyCreationCallback_OnFailure;

				UserPolicy.Acquire(
					m_MessageRpmsg._PublishingLicense.PublishingLicenseBytes,
					null,
					authenticationRequestCallback, 
					new ConsentCallback(m_MainActivity), 
					PolicyAcquisitionFlags.None,
					policyCreationCallback);
			}
			catch (Exception ex)
			{
				LogUtils.Error("Error while aquiring user policy", ex);
				OnDecryptError (ex);
			}
		}

		private void OnUserPolicyAcquisitionComplete(UserPolicy userPolicy)
		{
			try
			{
				LogUtils.Log("UserPolicyAcquisitionComplete");

				m_UserLicense = new EndUserLicense(userPolicy);

				RMSProtectedStreamCreationCallback protectedStreamCreationCallback = new RMSProtectedStreamCreationCallback(m_MessageRpmsg._EncryptedDRMContent);
				protectedStreamCreationCallback.CancelEvent += RMSProtectedStreamCreationCallback_OnCancel;
				protectedStreamCreationCallback.FailureEvent += RMSProtectedStreamCreationCallback_OnFailure;
				protectedStreamCreationCallback.SuccessEvent += RMSProtectedStreamCreationCallback_OnSuccess;

				CustomProtectedInputStream.Create(
					userPolicy,
					protectedStreamCreationCallback.EncryptedMemoryStream, 
					protectedStreamCreationCallback.PaddedContentLength, 
					protectedStreamCreationCallback);
			}
			catch (Exception ex)
			{
				LogUtils.Error("Could not create decryption stream", ex);
				OnDecryptError (ex);
			}
		}

		#region AuthenticationCompleteCallback events
		protected void AuthenticationCompleteCallback_OnCancel()
		{
			LogUtils.Log("AuthenticationCompleteCallback_OnCancel");
			OnDecryptError (new System.OperationCanceledException());
		}

		protected void AuthenticationCompleteCallback_OnError(Java.Lang.Exception p0)
		{
			LogUtils.Error("AuthenticationCompleteCallback_OnError", p0);
			OnDecryptError (new Exception (p0.Message));
		}

		protected void AuthenticationCompleteCallback_OnSuccess()
		{
			LogUtils.Log("AuthenticationCompleteCallback_OnSuccess");
		}
		#endregion

		#region RMSProtectedStreamCreationCallback events
		void RMSProtectedStreamCreationCallback_OnCancel()
		{
			LogUtils.Log("RMSProtectedStreamCreationCallback_OnRMSCancel");
			OnDecryptError (new System.OperationCanceledException());
		}
		
		void RMSProtectedStreamCreationCallback_OnFailure(Exception ex)
		{
			LogUtils.Error("RMSProtectedStreamCreationCallback_OnRMSFailure", ex);
			OnDecryptError (ex);
		}

		void RMSProtectedStreamCreationCallback_OnSuccess(byte[] drmContentBytes)
		{
			LogUtils.Log("RMSProtectedStreamCreationCallback_OnRMSSuccess");

			DRMContent drmContent = DRMContent.Parse(drmContentBytes);
			OnDecryptSuccess(drmContent, m_UserLicense);
		}
		#endregion

		#region RMSPolicyCreationCallback events
		void RMSPolicyCreationCallback_OnCancel()
		{
			LogUtils.Log("RMSPolicyCreationCallback_OnCancel");
			OnDecryptError (new System.OperationCanceledException());
		}

		void RMSPolicyCreationCallback_OnFailure(Java.Lang.Exception p0)
		{
			ProtectionException protEx = p0 as ProtectionException;
			if (protEx != null) {
				if (protEx.Type == ProtectionExceptionType.NoConsumptionRightsException) {
					OnDecryptError (new NoPermissionsException (p0.Message));
					return;
				}
			}

			LogUtils.Error("RMSPolicyCreationCallback_OnFailure", p0);
			OnDecryptError (new Exception (p0.Message));
		}
			
		#endregion
	}
}

