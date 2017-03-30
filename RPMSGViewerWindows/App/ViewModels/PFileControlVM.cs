using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using com.microsoft.rightsmanagement.mobile.viewer.lib;
using com.microsoft.rightsmanagement.windows.viewer.Models;
using com.microsoft.rightsmanagement.windows.viewer.RMS;
using com.microsoft.rightsmanagement.windows.viewer.Views;
using Microsoft.InformationProtection.Lib;
using Microsoft.InformationProtection.Lib.Extensions;
using Microsoft.InformationProtection.RMS.Exceptions;

namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	internal class PFileControlVM : BaseVM<PFileModel>
	{
		private readonly PFileVM _owner;
		private readonly PackagePfileWindows _package;
		private string _fileName;
		private bool _inProgress = true;
		private bool _isOpenButtonEnabled;
		private string _issuer;
		private Visibility _pfileControlVisibility;
		private TempFile _planFile;
		private string _templateName;

		public PFileControlVM(PFileModel model, PFileVM owner)
		{
			_owner = owner;
			Model = model;
			_package = Model.Package;

			Issuer = Model.Issuer;

			FileName = Path.GetFileName(Model.FilePath);

			IsOpenButtonEnabled = false;
		}

		public Visibility IsProgressAnimationVisible => InProgress ? Visibility.Visible : Visibility.Collapsed;
		public ICommand OpenCommand => new DelegateCommand(window => OpenFile(window as Window));

		public ICommand CancelCommand => new DelegateCommand(window =>
		{
			_planFile?.Dispose();
			((Window) window).Close();
		});

		public string TempalteDesciption
		{
			get { return _templateName; }
			set
			{
				_templateName = value;
				OnPropertyChanged();
			}
		}

		public string FileName
		{
			get { return _fileName; }
			private set
			{
				_fileName = value;
				OnPropertyChanged();
			}
		}

		public bool InProgress
		{
			get { return _inProgress; }
			set
			{
				_inProgress = value;
				OnPropertyChanged(nameof(IsProgressAnimationVisible));
				OnPropertyChanged(nameof(TempalteDesciption));
			}
		}

		public string Issuer
		{
			get { return _issuer; }
			set
			{
				_issuer = value;
				OnPropertyChanged();
			}
		}

		public bool IsOpenButtonEnabled
		{
			get { return _isOpenButtonEnabled; }
			set
			{
				_isOpenButtonEnabled = value;
				OnPropertyChanged();
			}
		}

		public Visibility PFileControlVisibility
		{
			get { return _pfileControlVisibility; }
			set
			{
				_pfileControlVisibility = value;
				OnPropertyChanged();
			}
		}

		private void OpenFile(Window window)
		{
			_planFile.Close();
			Process.Start(_planFile.FileName);

			window.Close();
		}

		private void DecryptFile()
		{
			try
			{
				var content = _package.Content;
				_planFile = new TempFile(_package.OriginalExtension);
				_planFile.Stream.Value.WriteAllBytes(content);

				WindowUtils.InvokeOnUIThread(() =>
				{
					InProgress = false;
					TempalteDesciption = _package.EUL?.TemplateName;
					IsOpenButtonEnabled = true;
				});
			}
			catch (Exception exception)
			{
				if (exception is NoPermissionsException || exception is RMSUnauthorizedException)
					_owner.ShowNoPermissions(FileName, Issuer);
				else
					MessageBox.Show(exception.Message);
			}
		}

		public void CreatePlainFileAndUpdateUi()
		{
			try
			{
				var thread = new Thread(DecryptFile);
				if ((Application.Current == null) || Application.Current.MainWindow is PFileWindow)
					thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
			}
			catch (Exception exception)
			{
				if (exception is NoPermissionsException || exception is RMSUnauthorizedException)
					_owner.ShowNoPermissions(FileName, Issuer);
				else
					MessageBox.Show(exception.Message);
			}
		}
	}
}