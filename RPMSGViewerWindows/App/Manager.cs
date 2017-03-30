using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using com.microsoft.rightsmanagement.mobile.viewer.lib;
using com.microsoft.rightsmanagement.windows.viewer.lib;
using com.microsoft.rightsmanagement.windows.viewer.Models;
using com.microsoft.rightsmanagement.windows.viewer.Presenters;
using com.microsoft.rightsmanagement.windows.viewer.RMS;
using com.microsoft.rightsmanagement.windows.viewer.ViewModels;
using CefSharp;
using Microsoft.InformationProtection.Lib;
using Microsoft.InformationProtection.Lib.Extensions;
using NLog;
using NLog.Config;

namespace com.microsoft.rightsmanagement.windows.viewer
{
	internal class Manager
	{
		public static Manager Instance;

		private string _file;

		[STAThread]
		public static void Main(string[] args)
		{
			Instance = new Manager();
			Instance.Run(args);
		}

		private void Run(string[] args)
		{
			Initialize();
			ParseArgs(args);
			var mainWindowModel = CreateMainWindowModel();

			if (RmsUtils.SignIn(true))
			{
				Log.Logger.Info("User is signed in, navigate to main window");
				if (_file?.EndsWith(".pfile") ?? false)
					OpenFile(_file);
				else
					new Application().Run(WindowUtils.CreateWindow(mainWindowModel));
			}
			else
			{
				Log.Logger.Info("User isn't signed in, navigate to sign in window");
				var signInModel = CreateSignInModel(mainWindowModel);
				new Application().Run(WindowUtils.CreateWindow(signInModel));
			}
		}

		private MainWindowModel CreateMainWindowModel()
		{
			return new MainWindowModel
			{
				OpenDocument = OpenFile,
				OpenWithFile = _file,
				SignOut = SignOutImpl,
				SendFeedback = SendFeedbackImpl,
			};
		}

		private SignInModel CreateSignInModel(MainWindowModel mainWindowModel = null)
		{
			return new SignInModel
			{
				OnSignIn = () => RmsUtils.SignIn(false),
				OnSignUp = () => System.Diagnostics.Process.Start(SIConstants.RMS_SIGNUP_URI),
				OnFinish = () =>
				{
					if (_file?.EndsWith(".pfile") ?? false)
					{
						Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
						Application.Current.MainWindow.Close();
						OpenFile(_file);
						Application.Current.Shutdown();
					}
					else
						WindowUtils.ExchangeMainWindow(WindowUtils.CreateWindow(mainWindowModel ?? CreateMainWindowModel()));
				}
			};
		}

		private void SignOutImpl()
		{
			RmsUtils.SignOut();
			WindowUtils.ExchangeMainWindow(WindowUtils.CreateWindow(CreateSignInModel()));
		}

		private void SendFeedbackImpl()
		{
			MessageBox.Show("Coming soon...");
		}

		private void ParseArgs(string[] args)
		{
			Log.Logger.Info("Args: " + string.Join(" ", args));
			if (args.Length > 1)
				throw new ArgumentOutOfRangeException("too many arguments");

			if (args.Length > 0)
				_file = args[0];
		}

		private void OpenFile(string path)
		{
			_file = path;

			var creationData = new CreationData
			{
				FilePath = path,
				FileName = Path.GetFileName(path)
			};

			var presenter = PresenterFactory.Instance.Create(creationData);
			presenter.View = ServicesUtils.GetService<MainVM>();
			presenter.PopulateView();
		}

		#region Initialize

		private void Initialize()
		{
			InitializeLogging();
			RegisterServices();
			InitializeCefSharp();
			RegisterPresenters();
			RmsUtils.InitializeRms();
		}

		private void RegisterServices()
		{
			ServicesUtils.Register<RMSHandlerPfile>(new RMSHandlerPfileWindows());
			ServicesUtils.Register(new IconServiceWindows());
		}

		private void InitializeLogging()
		{
			var LogsFolder = Path.Combine(Assembly.GetExecutingAssembly().GetDirectory(), "Logs");
			var config = new LoggingConfiguration();
			config.AddTraceTarget(LogLevel.Trace);
			config.AddFileTarget(LogLevel.Trace, Path.Combine(LogsFolder, SIConstants.LOG_FILE_FULL_NAME));
			LogManager.Configuration = config;
		}

		private void InitializeCefSharp()
		{
			if (Cef.IsInitialized)
				return;

			var settings = new CefSettings();
			settings.CefCommandLineArgs.Add("disable-gpu", "disable-gpu");
			Cef.Initialize(settings);
		}

		private void RegisterPresenters()
		{
			PresenterFactory.Instance.Clear();

			PresenterFactory.Instance.Add
			(
				new PresenterWindows
				{
					ModelFactory = new DocumentFactory
					{
						new DocumentText(),
						new DocumentProtectedText(),
						new DocumentImage(),
						new DocumentProtectedImage(),
						new DocumentPPDF()
					}
				}
			);

			PresenterFactory.Instance.Add(
				new PresenterWindowsPfile
				{
					ModelFactory = new DocumentFactory
					{
						new DocumentPFile()
					}
				}
			);
		}

		#endregion
	}
}