using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Log = Microsoft.InformationProtection.RMS.Log;

namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	internal abstract class BaseVM<TModel> : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private TModel _model;

		public TModel Model
		{
			get { return _model; }
			set
			{
				_model = value;
				OnModelChanged();
			}
		}

		public virtual void OnModelChanged()
		{
			OnPropertyChanged("");
		}

		public void ShowError(Exception ex)
		{
			ShowError("Error", ex.Message);
			Log.Logger.Error(ex);
		}

		public virtual void ShowError(string title, string message, string details = null, Exception ex = null)
		{
			MessageBox.Show(message, title);
		}
	}
}