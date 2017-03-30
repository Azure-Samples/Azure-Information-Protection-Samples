using com.microsoft.rightsmanagement.mobile.viewer.lib;
using com.microsoft.rightsmanagement.windows.viewer.ViewModels;
using System;
using System.IO;
using System.Windows.Controls;

namespace com.microsoft.rightsmanagement.windows.viewer
{
	public class PresenterWindows : Presenter
	{
		public PresenterWindows()
		{}

		public PresenterWindows(Document model, IView view) : base(model,view)
		{ }
		
		protected override void OnAfterUpdateView()
		{
			base.OnAfterUpdateView();
			PopulateMenuItems(ModelProtected?.DocProtectedPackage?.EUL);
		}

		protected override void OnBeforeUpdateView()
		{
			base.OnBeforeUpdateView();

			IntPtr mainWinHandle = IntPtr.Zero;
			WindowUtils.InvokeOnUIThread(() => mainWinHandle = WindowUtils.GetMainWindowHandle());
			bool allowPrintScreen = ModelProtected?.DocProtectedPackage?.EUL?._DocRights[UserRights.Extract] ?? true;
			if (allowPrintScreen)
				Microsoft.InformationProtection.RMS.MSIPC.Native.DllImports.IpcUnprotectWindow(mainWinHandle);
			else
				Microsoft.InformationProtection.RMS.MSIPC.Native.DllImports.IpcProtectWindow(mainWinHandle);
		}

		private void PopulateMenuItems(EndUserLicense EUL)
		{
			var mainVm = ServicesUtils.GetService<MainVM>();
			mainVm.Model.EUL = EUL;
			mainVm.Model.Print = (EUL?._DocRights[UserRights.Print] ?? true) ? Print : (Action)null;
			mainVm.Model.SaveAs = (EUL?._DocRights[UserRights.Export] ?? false) ? SaveAs : (Action<string>)null;
			mainVm.Model.OriginalExtension = ModelProtected?.GetOriginalExtension();
			mainVm.OnModelChanged();
		}

		private void Print()
		{
			PrintDialog dlg = new PrintDialog();
			dlg.ShowDialog();
		}

		private void SaveAs(string targetPath)
		{
			File.WriteAllBytes(targetPath, Model.DocPackage.Content);
		}
	}
}
