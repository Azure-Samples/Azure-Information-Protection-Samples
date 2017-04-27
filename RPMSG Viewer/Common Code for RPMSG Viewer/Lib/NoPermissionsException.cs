using System;

namespace SI.Mobile.RPMSGViewer.Lib
{
	public class NoPermissionsException : Exception
	{
		public NoPermissionsException (string msg) : base(msg)
		{
		}
	}
}

