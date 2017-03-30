using System;
using System.Linq;
using System.Windows.Input;
using com.microsoft.rightsmanagement.windows.viewer.Models;
using System.Windows;
using com.microsoft.rightsmanagement.windows.viewer.Views;
using System.Threading.Tasks;
using com.microsoft.rightsmanagement.mobile.viewer.lib;
using com.microsoft.rightsmanagement.windows.viewer.lib;
using Log = Microsoft.InformationProtection.RMS.Log;

namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	internal class SignInVM : BaseVM<SignInModel>
	{
		private bool _inProgress;

		public ICommand SignInClick => new DelegateCommand(SignIn);

		public ICommand SignUpClick => new DelegateCommand(SignUp);
		
		public bool InProgress
		{
			get { return _inProgress; }
			private set
			{
				_inProgress = value;
				OnPropertyChanged("");
			}
		}

		public Visibility IsProgressAnimationVisible => InProgress ? Visibility.Visible : Visibility.Hidden;

		public bool IsButtonEnabled => !InProgress;

		private async void SignIn()
		{
			try
			{
				InProgress = true;
				var succeeded = await Task.Run(Model.OnSignIn);
				if (succeeded)
				{
					Log.Logger.Info("User signed in successfuly");
					Model.OnFinish();
				}
			}
			catch (Exception ex)
			{
				LogUtils.Error(ex);
				MessageBox.Show(ex.Message);
			}
			finally
			{
				InProgress = false;
			}
		}

		private void SignUp()
		{
			try
			{
				Model.OnSignUp();
			}
			catch (Exception ex)
			{
				LogUtils.Error(ex);
				MessageBox.Show(ex.Message);
			}
		}
	}
}