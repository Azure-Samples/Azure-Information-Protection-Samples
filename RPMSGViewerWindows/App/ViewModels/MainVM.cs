using CefSharp;
using CefSharp.Wpf;
using com.microsoft.rightsmanagement.mobile.viewer.lib;
using com.microsoft.rightsmanagement.mobile.viewer.pcl;
using com.microsoft.rightsmanagement.windows.viewer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	internal class MainVM : BaseVM<MainWindowModel>, IView
	{
		public IWpfWebBrowser WebBrowser { get; set; }
		public ToolbarVM ToolbarVM { get; set; }
		public SidebarVM SidebarVM { get; set; }
		public SettingsVM SettingsVM { get; set; }
		public Visibility ProgressAnimationVisibility { get; set; } = Visibility.Collapsed;
		public Visibility WebBrowserVisibility { get; set; } = Visibility.Visible;

		private static byte[] BASE_URL = Encoding.UTF8.GetBytes(@"<base href='\Resources\HTML\'>");

		public bool AllowCopy
		{
			get
			{
				return Model.EUL?._DocRights[UserRights.Extract] ?? true;
			}
		}

		public bool AllowPrint
		{
			get
			{
				return Model.EUL?._DocRights[UserRights.Print] ?? true;
			}
		}

		public bool InProgress
		{
			set
			{
				ProgressAnimationVisibility = value ? Visibility.Visible : Visibility.Collapsed;
				WebBrowserVisibility = !value ? Visibility.Visible : Visibility.Hidden;
				OnPropertyChanged("ProgressAnimationVisibility");
				OnPropertyChanged("WebBrowserVisibility");
			}
		}

		public bool DimDisplay
		{
			set
			{
				Opacity = value ? 0.5 : 1;
				OnPropertyChanged("Opacity");
			}
		}

		public double Opacity { get; set; } = 1;

		private void UpdateContent(byte[] content, string mimeType)
		{
			const string FAKE_URL = @"file:///";
			DefaultResourceHandlerFactory resourceHandlerFactory = (DefaultResourceHandlerFactory) WebBrowser.ResourceHandlerFactory;
			resourceHandlerFactory.RegisterHandler(FAKE_URL, ResourceHandler.FromStream(new MemoryStream(content), mimeType));
			WebBrowser.LoadingStateChanged += BrowserLoadingStateChangedHnadler;
			WebBrowser.Load(FAKE_URL);
		}

		private void BrowserLoadingStateChangedHnadler(object sender, LoadingStateChangedEventArgs loadingStateChangedEventArgs)
		{
			if (loadingStateChangedEventArgs.IsLoading)
				return;

			InProgress = false;
			WebBrowser.LoadingStateChanged -= BrowserLoadingStateChangedHnadler;
		}

		public MainVM()
		{
			ServicesUtils.Register(this);

			SidebarVM = new SidebarVM
			{
				Model = new SidebarModel
				{
					TopItems = new List<SidebarModel.Item>
					{
						new SidebarModel.Item {Icon = Icon.IconType.View, Description = "View", OnClick = View},
						new SidebarModel.Item {Icon = Icon.IconType.Open, Description = "Open", OnClick = Open}
					},
					BottomItems = new List<SidebarModel.Item>
					{
						new SidebarModel.Item {Icon = Icon.IconType.Settings, Description = "Settings", OnClick = ShowSettings}
					}
				}
			};
			ToolbarVM = new ToolbarVM
			{
				Model = new ToolbarModel
				{
					Hamburger = new ToolbarModel.Item { IconType = Icon.IconType.Hamburger, OnClick = OpenCloseSidebar }
				}
			};
			SettingsVM = new SettingsVM();
		}

		public override void OnModelChanged()
		{
			SettingsVM.Model = new SettingsModel
			{
				SignOut = Model.SignOut,
				SendFeedback = Model.SendFeedback
			};
			SettingsVM.OnModelChanged();
			ToolbarVM.Model.Items = new List<ToolbarModel.Item>
			{
				new ToolbarModel.Item{ IconType = Icon.IconType.Permission, OnClick = ShowPermissions, IsEnabled = Model.EUL != null },
				new ToolbarModel.Item {IconType = Icon.IconType.Print, OnClick = Model.Print, IsEnabled = Model.Print != null},
				new ToolbarModel.Item {IconType = Icon.IconType.SaveAs, OnClick = SaveAs, IsEnabled = Model.SaveAs != null}
			};

			ToolbarVM.OnModelChanged();
			if (Model.OpenWithFile != null)
			{
				InProgress = true;
				var file = Model.OpenWithFile;
				Model.OpenWithFile = null;
				Model.OpenDocument(file);
			}
			base.OnModelChanged();
		}

		private void OpenCloseSidebar()
		{
			switch (SidebarVM.Visibility)
			{
				case Visibility.Collapsed:
					SidebarVM.Visibility = Visibility.Visible;
					break;
				case Visibility.Visible:
					SidebarVM.Visibility = Visibility.Collapsed;
					break;
			}
		}

		private void View()
		{
			WebBrowserVisibility = Visibility.Visible;
			SettingsVM.SetVisibility(false);
			OnPropertyChanged("WebBrowserVisibility");
		}

		private void Open()
		{
			try
			{
				OpenFileDialog dlg = new OpenFileDialog();
				if (dlg.ShowDialog() == true)
				{
					InProgress = true;
					var fileName = dlg.FileName;
					Task.Run(() => Model.OpenDocument(fileName));
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void ShowPermissions()
		{
			DimDisplay = true;
			WindowUtils.ShowOntopMainWindow(WindowUtils.CreateWindow(Model.EUL));
			DimDisplay = false;
		}

		public void SaveAs()
		{
			try
			{
				MessageBoxResult result = MessageBox.Show(AppResources.EXPORT_MSG, AppResources.EXPORT_MSG_TITLE, MessageBoxButton.YesNo);
				if (result != MessageBoxResult.Yes)
					return;

				SaveFileDialog saveDialog = new SaveFileDialog
				{
					FileName = Path.GetFileNameWithoutExtension(ToolbarVM.Title),
					Filter = GetFilterstring(Model.OriginalExtension)
				};
				saveDialog.FileOk += (sender, args) => Model.SaveAs(saveDialog.FileName);
				saveDialog.ShowDialog();
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}


		private string GetFilterstring(string originalExtension)
		{
			string dispalyName = originalExtension.Substring(1).ToUpper();
			return String.Format("{0} (*{1})|*{1}", dispalyName, originalExtension);
		}

		public void ShowSettings()
		{
			WebBrowserVisibility = Visibility.Hidden;
			SettingsVM.SetVisibility(true);
			OnPropertyChanged("WebBrowserVisibility");
		}

		#region IView
		public void ShowUserContent(byte[] content, string titleText, string mimeType, string baseUrl, bool allowZoom, bool allowCopy)
		{
			UpdateContent(content, mimeType);
			SidebarVM.Model.SelectedItem = SidebarVM.Model.TopItems[0];
			SidebarVM.OnModelChanged();
			ToolbarVM.Model.Title = titleText;
			ToolbarVM.OnModelChanged();
			SettingsVM.SetVisibility(false);
			OnPropertyChanged("");
		}

		public void ShowNoPermissions(byte[] content, string mimeType, string baseUrl)
		{
			var newContent = BASE_URL.Concat(content).ToArray();
			UpdateContent(newContent, mimeType);
		}

		public override void ShowError(string title, string message, string details = null, Exception ex = null)
		{
			InProgress = false;
			base.ShowError(title, message, details, ex);
		}

		public Action<Uri> OnClickedUrl { get; set; }

		public void HideWebBrowser()
		{
			WebBrowserVisibility = Visibility.Hidden;
			OnPropertyChanged("WebBrowserVisibility");
		}

		#endregion
	}
}
