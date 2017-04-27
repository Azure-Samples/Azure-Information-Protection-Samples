using System;

#if __IOS__
using CoreGraphics;
using UIKit;
#endif

namespace SI.Mobile.RPMSGViewer.Lib
{
	public static class UIUtils
	{
#if __IOS__
		public static nfloat GetScreenHeight()
		{
			CGRect size = UIScreen.MainScreen.Bounds;
			return size.Height;
		}
#endif
	}
}

