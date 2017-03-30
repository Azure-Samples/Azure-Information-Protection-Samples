using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using com.microsoft.rightsmanagement.mobile.viewer.lib;
using Microsoft.InformationProtection.Lib;
using Microsoft.InformationProtection.Lib.FileSystem;
using Microsoft.InformationProtection.RMS;
using Microsoft.InformationProtection.RMS.Exceptions;
using Microsoft.InformationProtection.RMS.Interfaces;
using Microsoft.InformationProtection.RMS.MSIPC.Native;
using Microsoft.InformationProtection.RMS.Structures;
using Log = Microsoft.InformationProtection.RMS.Log;
using UserRights = com.microsoft.rightsmanagement.mobile.viewer.lib.UserRights;

namespace com.microsoft.rightsmanagement.windows.viewer.lib
{
	internal static class RmsUtils
	{
		private static bool _ipcInitialized;

		static RmsUtils()
		{
			InitializeRms();
		}

		public static bool RemoveProtection(string fileName)
		{
			string _newPath;
			var _item = new FileItem(fileName, true, FileAccess.Read);
			var _outputFile = new TempFile(_item.Extension);
			_item.Stream.Value.Position = 0;
			using (var stream = _outputFile.Stream)
			{
				_newPath = IpcHelper.DecryptFileStream(_item.Stream, stream, _item.FileName,
					new PromptContext(PromptContextFlag.Silent));
			}

			_outputFile.Close();
			var fileExtension = Path.GetExtension(fileName);
			var newFileName = Path.GetDirectoryName(fileName) + @"\" +
			                  Path.GetFileNameWithoutExtension(_outputFile.FileName) + "." +
			                  fileName.Substring(fileName.Length - 3);


			File.Move(_outputFile.FileName, newFileName);

			ServicesUtils.GetService<TelemetryBase>()
				.LogEvent(TelemetryEvent.DecryptEnd, null, new Dictionary<string, string>
				{
					{"FileName", fileName}
				});

			return true;
		}

		public static void InitializeRms()
		{
			if (!_ipcInitialized)
			{
				Ipc.Initialize();
				IpcHelper.SetStore(SIConstants.MSIPC_PRIVATE_STORE_NAME);
				IpcHelper.SetApplicationId(SIConstants.APP_CLIENT_ID_WIN, SIConstants.REDIRECT_URI_WIN);
				_ipcInitialized = true;
				Log.Logger.Debug("Successfuly initialized Ipc");
			}
		}

		public static bool SignIn(bool offline)
		{
			try
			{
				Log.Logger.Debug("Signing in in offline mode: [{0}]", offline);
				using (var context = new PromptContext(offline ? PromptContextFlag.Offline : PromptContextFlag.None))
				{
					ConnectionInfo.GetAll(context);
				}

				return true;
			}
			catch (RMSCanceledException e)
			{
				Log.Logger.Info(e, "Sign in process was canceld by the user");
			}
			catch (RMSNeedOnlineException ex)
			{
				Log.Logger.Info(ex, "User is not signed in");
			}
			return false;
		}

		public static void SignUp()
		{
			Process.Start(SIConstants.RMS_SIGNUP_URI);
			Log.Logger.Info("The user was redirected to sign up page: [{0}]", SIConstants.RMS_SIGNUP_URI);
		}

		public static EndUserLicense GetEULFromLicense(ISerializedLicense license)
		{
			var key = license.Key;
			var customProtectionInfo = license.IsFromTemplate ? null : new CustomProtectionInfo(license);
			return new EndUserLicense
			{
				IssuedTo = key.UserDisplayName,
				TemplateName =
					customProtectionInfo == null
						? license.TemplateName
						: "Your permissions: " + PermissionAsString(Permissions.GetPermission(customProtectionInfo.Permission)),
				Description = license.TemplateDescription,
				Owner = license.Owner,
				ReferralInfo = license.ReferralInfo,
				_DocRights = new Dictionary<UserRights, bool>
				{
					{UserRights.View, key.AccessCheck(RightXrMLTag.VIEW)},
					{UserRights.Extract, key.AccessCheck(RightXrMLTag.EXTRACT)},
					{UserRights.Export, key.AccessCheck(RightXrMLTag.EXPORT)},
					{UserRights.Edit, key.AccessCheck(RightXrMLTag.EDIT)},
					{UserRights.Print, key.AccessCheck(RightXrMLTag.PRINT)}
				}
			};
		}

		//TODO localize strings
		private static string PermissionAsString(Permission permission)
		{
			if (permission == Permission.CoOwner)
				return "Co-owner";

			if (permission == Permission.CoAuthor)
				return "Co-author";

			if (permission == Permission.Reviewer)
				return "Reviewer";

			if (permission == Permission.Viewer)
				return "Viewer";

			return "Custom";
		}

		public static void SignOut()
		{
			try
			{
				var storePath = GetStoreDirPath();

				if (DirectoryUtils.Exists(storePath))
					DirectoryUtils.Delete(storePath, true, true, true);
			}
			catch (Exception ex)
			{
				LogUtils.Error("Failed to delete store folder", ex);
			}
		}

		public static string GetSignedInUserMail()
		{
			try
			{
				var c = DrmCertificate.GetRacs(GetStoreDirPath());
				var x = c.First();
				var email = x.IssuedPrincipal;
				return email;
			}
			catch (Exception ex)
			{
				LogUtils.Error("Failed to retrieve user's email", ex);
				return null;
			}
		}

		private static string GetStoreDirPath()
		{
			var storePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				@"Microsoft\MSIPC", SIConstants.MSIPC_PRIVATE_STORE_NAME);
			return storePath;
		}
	}
}