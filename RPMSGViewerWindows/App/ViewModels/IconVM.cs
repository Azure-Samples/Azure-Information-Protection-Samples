using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using com.microsoft.rightsmanagement.mobile.viewer.lib;
using com.microsoft.rightsmanagement.windows.viewer.Models;

namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	class IconVM : BaseVM<Icon.IconType>
	{
		private Icon Icon => ServicesUtils.GetService<IconServiceWindows>().GetIcon(Model);

		public FontFamily Fontfamily => ServicesUtils.GetService<IconServiceWindows>().GetFontFamily(Icon);
		public string Character => Icon.Character;
	}
}
