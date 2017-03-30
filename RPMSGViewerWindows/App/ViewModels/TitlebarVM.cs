using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	class TitlebarVM : BaseVM<Window>
	{
		public ICommand OnMinimize => new DelegateCommand(() => Model.WindowState = WindowState.Minimized);
		public ICommand OnMaximize => new DelegateCommand(() => Model.WindowState = (Model.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal));
		public ICommand OnExit => new DelegateCommand(Model.Close);
	}
}
