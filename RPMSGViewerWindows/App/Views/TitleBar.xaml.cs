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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace com.microsoft.rightsmanagement.windows.viewer.Views
{
	/// <summary>
	/// Interaction logic for Titlebar.xaml
	/// </summary>
	public partial class Titlebar : UserControl
	{
		public Titlebar()
		{
			InitializeComponent();
		}
		
		private void Minimize_OnClick(object sender, RoutedEventArgs e)
		{
			Window.GetWindow(this).WindowState = WindowState.Minimized;
		}

		private void Mximize_OnClick(object sender, RoutedEventArgs e)
		{
			GetWindow().WindowState = GetWindow().WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
		}

		private void Exit_OnClick(object sender, RoutedEventArgs e)
		{
			GetWindow().Close();
		}

		private Window GetWindow()
		{
			return Window.GetWindow(this);
		}
	}
}
