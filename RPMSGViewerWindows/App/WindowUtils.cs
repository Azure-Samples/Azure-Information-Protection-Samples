using com.microsoft.rightsmanagement.mobile.viewer.lib;
using com.microsoft.rightsmanagement.windows.viewer.Models;
using com.microsoft.rightsmanagement.windows.viewer.ViewModels;
using com.microsoft.rightsmanagement.windows.viewer.Views;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace com.microsoft.rightsmanagement.windows.viewer
{
	internal static class WindowUtils
	{
		public static Window CreateWindow(SignInModel model)
		{
			return new SignInWindow
			{
				DataContext = new SignInVM
				{
					Model = model
				}
			};
		}

		public static Window CreateWindow(MainWindowModel model)
		{
			return new MainWindow
			{
				DataContext = new MainVM
				{
					Model = model
				}
			};
		}

		public static Window CreateWindow(EndUserLicense EUL)
		{
			return new PermissionsWindow
			{
				DataContext = new PersmissionsVM
				{
					Model = EUL
				}
			};
		}

		public static Window CreateWindow(PFileModel model)
		{
			return new PFileWindow {DataContext = new PFileVM {Model = model}};
		}

		public static void ExchangeMainWindow(Window window)
		{
			var temp = Application.Current.MainWindow;
			Application.Current.MainWindow = window;
			Application.Current.MainWindow.Show();
			temp.Close();
		}

		public static void ShowOntopMainWindow(Window window)
		{
			window.Owner = Application.Current.MainWindow;
			window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			window.ShowInTaskbar = false;
			window.ShowDialog();
		}

		public static IntPtr GetMainWindowHandle()
		{
			return new WindowInteropHelper(Application.Current.MainWindow).Handle;
		}

		public static void InvokeOnUIThread(Action action)
		{
			if (Application.Current != null)
				Application.Current.Dispatcher.Invoke(action);
			else
				action();
		}
	}
}
