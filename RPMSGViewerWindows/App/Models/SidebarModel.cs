using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.microsoft.rightsmanagement.mobile.viewer.lib;

namespace com.microsoft.rightsmanagement.windows.viewer.Models
{
	class SidebarModel
	{
		public class Item
		{
			public Icon.IconType Icon;
			public string Description;
			public Action OnClick;

			public void Click()
			{
				OnClick?.Invoke();
			}
		}

		public List<Item> TopItems;
		public List<Item> BottomItems;
		public Item SelectedItem;
	}
}
