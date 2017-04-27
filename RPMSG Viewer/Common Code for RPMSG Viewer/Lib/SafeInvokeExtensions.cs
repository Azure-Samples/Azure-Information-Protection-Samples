using System;
using SI.Mobile.RPMSGViewer.Lib;

namespace SI.Mobile.RPMSGViewer.Lib
{
	public static class SafeInvokeExtensions
	{
		#if __IOS__
		public static void SafeInvokeOnMainThread(this Foundation.NSObject nsObject, Action action, Action<Exception> failureAction = null)
			{
		nsObject.InvokeOnMainThread(() => SafeInvoke(((Exception error) => LogUtils.Error(error)), failureAction, action));
			}
		#endif

		#if __ANDROID__
		public static void SafeInvokeOnMainThread(this Android.App.Activity activity, Action action, Action<Exception> failureAction = null)
			{
			activity.RunOnUiThread(() => SafeInvoke(((Exception error) => LogUtils.Error(error)), failureAction, action));
			}
		#endif


		private static void SafeInvoke(Action<Exception> failureLogAction, Action<Exception> failureDisplayErrorAction, Action action)
		{
			try
			{
				action();
			}
			catch(Exception ex)
			{
				if (failureLogAction != null)
					failureLogAction(ex);

				if (failureDisplayErrorAction != null)
					failureDisplayErrorAction(ex);
			}
		}

	}
}
