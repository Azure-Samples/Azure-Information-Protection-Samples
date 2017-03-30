using System.IO;
using System.Windows;
using com.microsoft.rightsmanagement.windows.viewer.Models;

namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	internal class PFileVM : BaseVM<PFileModel>
	{
		private NoPermissionVM _noPermissionViewModel;
		private bool _noPermissionWindowVisible;
		private PFileControlVM _pFileControlViewModel;

		public override void OnModelChanged()
		{
			PFileControlViewModel = new PFileControlVM(Model, this);
			var noPermissionModel = new NoPermissionModel
			{
				FileName = Path.GetFileName(Model.FilePath),
				Issuer = Model.Issuer
			};

			NoPermissionViewModel = new NoPermissionVM(noPermissionModel) { NoPermissionVisibility = Visibility.Collapsed };

			NoPermissionWindowVisible = false;
			base.OnModelChanged();
		}

		public bool NoPermissionWindowVisible
		{
			get { return _noPermissionWindowVisible; }
			set
			{
				_noPermissionWindowVisible = value;

				_pFileControlViewModel.PFileControlVisibility = value ? Visibility.Collapsed : Visibility.Visible;
				_noPermissionViewModel.NoPermissionVisibility = value ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public PFileControlVM PFileControlViewModel
		{
			get { return _pFileControlViewModel; }
			set
			{
				_pFileControlViewModel = value;
				OnPropertyChanged();
			}
		}

		public NoPermissionVM NoPermissionViewModel
		{
			get { return _noPermissionViewModel; }
			set
			{
				_noPermissionViewModel = value;
				OnPropertyChanged();
			}
		}

		public void ShowNoPermissions(string fileName, string issuer)
		{
			NoPermissionWindowVisible = true;
		}
	}
}