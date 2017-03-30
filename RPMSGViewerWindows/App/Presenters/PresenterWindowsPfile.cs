using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using com.microsoft.rightsmanagement.mobile.viewer.lib;
using com.microsoft.rightsmanagement.windows.viewer.Models;
using com.microsoft.rightsmanagement.windows.viewer.RMS;
using com.microsoft.rightsmanagement.windows.viewer.ViewModels;
using com.microsoft.rightsmanagement.windows.viewer.Views;
using Microsoft.InformationProtection.RMS;

namespace com.microsoft.rightsmanagement.windows.viewer.Presenters
{
	internal class PresenterWindowsPfile : Presenter
	{
		public PresenterWindowsPfile()
		{ }

		public PresenterWindowsPfile(Document model, IView view) : base(model,view)
		{ }

		public override void PopulateView()
		{
			PackagePfileWindows package = ModelProtected?.DocProtectedPackage as PackagePfileWindows;

			var model = new PFileModel
			{
				Package = package,
				Issuer = package.License.Issuer,
				FilePath = Model.Name
			};
			
			WindowUtils.InvokeOnUIThread(() =>
			{
				if (Application.Current?.MainWindow is MainWindow && Application.Current.MainWindow.IsActive)
				{
					var mainVM = View as MainVM;
					mainVM.InProgress = false;
					mainVM.HideWebBrowser();
					mainVM.DimDisplay = true;
					WindowUtils.ShowOntopMainWindow(WindowUtils.CreateWindow(model));
					mainVM.DimDisplay = false;
				}
				else
				{
					WindowUtils.CreateWindow(model).ShowDialog();
				}
			});
		}
	}
}
