using System;
using System.Windows.Input;
using Microsoft.InformationProtection.Lib;

namespace com.microsoft.rightsmanagement.windows.viewer
{
	internal class DelegateCommand : ICommand
	{
		private readonly Predicate<object> _canExecute;
		private readonly Action<object> _execute;

		public event EventHandler CanExecuteChanged;

		public DelegateCommand(Action execute) : this(o => execute(), null)
		{ }

		public DelegateCommand(Action<object> execute) : this(execute, null)
		{}

		public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			try
			{
				if (_canExecute == null)
					return true;
				return _canExecute(parameter);
			}
			catch (Exception ex)
			{
				Log.Logger.Error(ex);
				return false;
			}
			
		}

		public void Execute(object parameter)
		{
			try
			{
				_execute(parameter);
			}
			catch (Exception ex)
			{
				Log.Logger.Error(ex);
			}
		}

		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}