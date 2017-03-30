using com.microsoft.rightsmanagement.windows.viewer.ViewModels;
using System.Windows.Input;

namespace com.microsoft.rightsmanagement.windows.viewer.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			ApplicationCommands.Print.InputGestures.Add(new KeyGesture(Key.P, ModifierKeys.Control));
		}

		private void CanExecuteCopy(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ((MainVM)DataContext).AllowCopy;
		}

		private void ExecuteCopy(object sender, ExecutedRoutedEventArgs e)
		{
			Browser.CopyCommand.Execute(e.Parameter);
		}

		private void CanExecutePrint(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ((MainVM)DataContext).AllowPrint;
		}

		private void ExecutePrint(object sender, ExecutedRoutedEventArgs e)
		{
			Browser.PrintCommand.Execute(e.Parameter);
		}

		private void ExecuteSelectAll(object sender, ExecutedRoutedEventArgs e)
		{
			Browser.SelectAllCommand.Execute(e.Parameter);
		}
	}
}
