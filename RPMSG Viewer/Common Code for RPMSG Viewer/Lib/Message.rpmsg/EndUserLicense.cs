using System;
using System.Text.RegularExpressions;
using SI.Mobile.RPMSGViewer.Lib;

#if __IOS__	
using MSRightsManagementBinding.ios;
#endif

#if __ANDROID__
using Com.Microsoft.Rightsmanagement;
#endif

namespace SI.Mobile.RPMSGViewer.Lib
{
	public class EndUserLicense
	{
		// RMS details
		public string IssuedTo 		{ get; set; }
		public string Owner 		{ get; set; }
		public string TemplateName	{ get; set; }
		public string Description	{ get; set; }
		public Rights _Rights		{ get; set; }
		public SIData _SIData		{ get; set; }

#if __IOS__	
		public EndUserLicense(MSUserPolicy userPolicy)
		{
			TemplateName	= userPolicy.PolicyName;
			Description		= userPolicy.PolicyDescription;
			Owner 			= userPolicy.Owner;
			IssuedTo 		= userPolicy.IssuedTo;

			_Rights = new Rights ();
			_Rights.Extract 	= userPolicy.AccessCheck(MSEmailRights.Extract);
			_Rights.Forward 	= userPolicy.AccessCheck(MSEmailRights.Forward);
			_Rights.Print 		= userPolicy.AccessCheck(MSEmailRights.Print);
			_Rights.Reply 		= userPolicy.AccessCheck(MSEmailRights.Reply);
			_Rights.ReplyAll	= userPolicy.AccessCheck(MSEmailRights.ReplyAll);

			try
			{
				string serializedPolicy = userPolicy.SerializedPolicy.ToString();
				Match match = Regex.Match(serializedPolicy, @"<AUTHENTICATEDDATA id=""APPSPECIFIC"" name=""SIData"">(?<data>.+?)</AUTHENTICATEDDATA>");
				_SIData = new SIData(match.Groups["data"].Value);
			}
			catch (Exception)
			{
				try
				{
					_SIData = new SIData(userPolicy.EncryptedAppData["SIData"].ToString()); // 5.10GA
				}
				catch (Exception)
				{
					LogUtils.Log("Could not parse mail SIData from EUL");
				}
			}
		}
#endif

#if __ANDROID__	
		public EndUserLicense(UserPolicy userPolicy)
		{
			TemplateName	= userPolicy.Name;
			Description		= userPolicy.Description;
			Owner 			= userPolicy.Owner;
			IssuedTo 		= userPolicy.IssuedTo;

			_Rights = new Rights();
			_Rights.Extract 	= userPolicy.AccessCheck(EmailRights.Extract);
			_Rights.Forward 	= userPolicy.AccessCheck(EmailRights.Forward);
			_Rights.Print 		= userPolicy.AccessCheck(EmailRights.Print);
			_Rights.Reply 		= userPolicy.AccessCheck(EmailRights.Reply);
			_Rights.ReplyAll	= userPolicy.AccessCheck(EmailRights.ReplyAll);

			try
			{
				_SIData = new SIData(userPolicy.SignedAppData["SIData"]);
			}
			catch (Exception)
			{
				try
				{
					_SIData = new SIData(userPolicy.EncryptedAppData["SIData"]); // 5.10GA
				}
				catch (Exception)
				{
					LogUtils.Log("Could not parse mail SIData from EUL");
				}
			}
		}
#endif
	}
}

