using System;
using System.Windows;
using System.Windows.Input;
using com.microsoft.rightsmanagement.windows.viewer.ViewModels;

namespace com.microsoft.rightsmanagement.windows.viewer.Views
{
	/// <summary>
	///     Interaction logic for PFileWindow.xaml
	/// </summary>
	public partial class PFileWindow
	{
		public PFileWindow()
		{
			InitializeComponent();
		}

		private void OnCloseClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void PFileWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}

		private void PFileWindow_OnContentRendered(object sender, EventArgs e)
		{
			var dataContext = DataContext as PFileVM;
			dataContext.PFileControlViewModel.CreatePlainFileAndUpdateUi();
		}
	}
}