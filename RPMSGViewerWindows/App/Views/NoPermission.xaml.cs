using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace com.microsoft.rightsmanagement.windows.viewer.Views
{
	/// <summary>
	///     Interaction logic for NoPermission.xaml
	/// </summary>
	public partial class NoPermission : UserControl
	{
		public NoPermission()
		{
			InitializeComponent();
		}

		private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		private void Close_OnClick(object sender, RoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);
			parentWindow?.Close();
		}

		private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
		{
			throw new System.NotImplementedException();
		}
	}
}