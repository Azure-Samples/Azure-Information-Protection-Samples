using com.microsoft.rightsmanagement.mobile.viewer.lib;
using com.microsoft.rightsmanagement.mobile.viewer.pcl;
using com.microsoft.rightsmanagement.windows.viewer.lib;
using com.microsoft.rightsmanagement.windows.viewer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	internal class SettingsVM : BaseVM<SettingsModel>
	{
		public ICommand SignOutClick => new DelegateCommand(SignOut);
		public ICommand SendfeedbackClick => new DelegateCommand(Model.SendFeedback);
		public ICommand LearnMoreClick => new DelegateCommand(() => OpenURL(SIConstants.LEARN_MORE_URL));
		public ICommand LicenseClick => new DelegateCommand(() => OpenURL(SIConstants.LICENSE_URL));
		public ICommand PrivacyClick => new DelegateCommand(() => OpenURL(SIConstants.PRIVACYE_URL));
		public Visibility Visibility { get; set; } = Visibility.Collapsed;
		public bool UserIsSignedIn { get; set; } = true;

		private bool SignOutEnabled { get; set; }

		public string UserMail
		{
			get
			{
				string userMail = RmsUtils.GetSignedInUserMail();
				SignOutEnabled = !string.IsNullOrEmpty(userMail);
				OnPropertyChanged("SignOutEnabled");
				return userMail ?? AppResources.NO_SIGNED_IN_USER;
			}
		}

		public string Version
		{
			get
			{
				return "Version: " + AppUtils.GetAppVersion();
			}
		}

		private void SignOut()
		{
			try
			{
				MessageBoxResult result = MessageBox.Show(AppResources.SIGN_OUT_DIALOG_MSG, AppResources.TOOLBAR_BUTTON_SIGNOUT, MessageBoxButton.YesNo);
				if (result == MessageBoxResult.No)
					return;
				Model.SignOut();
				OnPropertyChanged("");
				UserIsSignedIn = false;
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		public void OpenURL(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		public  void SetVisibility(bool visibale)
		{
			Visibility = visibale ? Visibility.Visible : Visibility.Collapsed;
			OnPropertyChanged("Visibility");
		}
	}
}
