using System.Windows;
using com.microsoft.rightsmanagement.windows.viewer.Models;

namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	internal class NoPermissionVM : BaseVM<NoPermissionModel>
	{
		private string _fileName;
		private string _issuer;
		private Visibility _noPermissionVisibility;

		public NoPermissionVM(NoPermissionModel model)
		{
			FileName = model.FileName;
			Issuer = model.Issuer;
		}

		public string FileName
		{
			get { return _fileName; }
			private set
			{
				_fileName = value;
				OnPropertyChanged();
			}
		}

		public string Issuer
		{
			get { return _issuer; }
			set
			{
				_issuer = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(HyperLink));
			}
		}

		public string HyperLink => "mailto:" + Issuer;

		public Visibility NoPermissionVisibility
		{
			get { return _noPermissionVisibility; }
			set
			{
				_noPermissionVisibility = value;
				OnPropertyChanged();
			}
		}
	}
}