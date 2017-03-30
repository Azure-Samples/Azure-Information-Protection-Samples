using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace com.microsoft.rightsmanagement.windows.viewer.Views
{
	/// <summary>
	/// Interaction logic for PermissionsWindow.xaml
	/// </summary>
	public partial class PermissionsWindow : Window
	{
		public PermissionsWindow()
		{
			InitializeComponent();
		}

		private void CloseWindow(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void DragWindow(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}
	}
}
