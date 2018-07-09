using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SI.Mobile.RPMSGViewer.Lib
{
	public class LogUtils
	{
		public static void Log(string message, 
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{
#if __IOS__
			Console.Out.WriteLine("{0} {1}({2}): {3}", sourceFilePath.GetFileName(), memberName, sourceLineNumber, message);
#endif

#if __ANDROID__
				Android.Util.Log.Info(sourceFilePath.GetFileName(), "{0}({1}): {2}", memberName, sourceLineNumber, message);
#endif
			}
			catch (Exception)
			{
				// logging is not enabled
			}
		}

#if __ANDROID__
		public static void Error(Java.Lang.Exception javaEx,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			Error("", javaEx, memberName, sourceFilePath, sourceLineNumber);
		}

		public static void Error(string errorMsg, Java.Lang.Exception javaEx,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			Error(errorMsg, new Exception(javaEx.ToString()), memberName, sourceFilePath, sourceLineNumber);
		}
#endif


		public static void Error(Exception ex = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			Error("", ex, memberName, sourceFilePath, sourceLineNumber);
		}

		public static void Error(string errorMsg, Exception ex = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{
#if __IOS__
				Console.Error.WriteLine("{0} {1}({2}): {3}{4}{5}", sourceFilePath.GetFileName(), memberName, sourceLineNumber, errorMsg, Environment.NewLine,  ex != null ? ex.GetType().ToString() : "");
#endif

#if __ANDROID__
				Android.Util.Log.Error(sourceFilePath.GetFileName(), "{0}({1}): {2}{3}{4}", memberName, sourceLineNumber, errorMsg, Environment.NewLine, ex != null ? ex.GetType().ToString() : "");
#endif
			}
			catch (Exception)
			{
				// logging is not enabled
			}
		}
	}
}
