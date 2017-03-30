using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using com.microsoft.rightsmanagement.mobile.viewer.lib;

namespace com.microsoft.rightsmanagement.windows.viewer
{
	internal class IconServiceWindows : IconService
	{
		public FontFamily GetFontFamily(Icon icon)
		{
			return new FontFamily(new Uri("pack://application:,,,/Resources/Icons/"), $"./#{icon.FontName}");
		}
	}
}
