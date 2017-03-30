using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.microsoft.rightsmanagement.mobile.viewer.lib;

namespace com.microsoft.rightsmanagement.windows.viewer.Models
{
	internal class ToolbarModel
	{
		public Item Hamburger;
		public string Title;
		public List<Item> Items;
		
		public class Item
		{
			public Icon.IconType IconType;
			public Action OnClick;
			public bool IsEnabled = true;

			public void Click()
			{
				OnClick?.Invoke();
			}
		}
	}
}
