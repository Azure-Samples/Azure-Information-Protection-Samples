using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.microsoft.rightsmanagement.mobile.viewer.lib;

namespace com.microsoft.rightsmanagement.windows.viewer.Menu
{
    internal class ProtectionMenuItem : IMenuItem
    {
        public bool IsEnabled { get; set; }
        public Icon Icon { get; } = ServicesUtils.GetService<IconServiceWindows>().GetIcon(Icon.IconType.Protection);
        public string Description { get; }
        public void Click()
        {
            throw new NotImplementedException();
        }
    }
}
