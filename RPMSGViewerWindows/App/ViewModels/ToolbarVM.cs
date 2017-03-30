using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using com.microsoft.rightsmanagement.mobile.viewer.lib;
using com.microsoft.rightsmanagement.windows.viewer.Models;
using com.microsoft.rightsmanagement.windows.viewer.Views;

namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	internal class ToolbarVM : BaseVM<ToolbarModel>
	{
		public string Title => Model.Title;
		public List<Item> Items => Model.Items?.Select(ModelItem2VMItem).ToList();
		public Item HamburgerVM => ModelItem2VMItem(Model.Hamburger);

		public class Item
		{
			public IconVM IconVM { get; set; }
			public ICommand OnClick { get; set; }
			public bool IsEnabled { get; set; }
			public double Opacity => IsEnabled ? 1 : 0.5;
		}

		private Item ModelItem2VMItem(ToolbarModel.Item modelItem)
		{
			return new Item
			{
				IconVM = new IconVM {Model = modelItem.IconType},
				IsEnabled = modelItem.IsEnabled,
				OnClick = new DelegateCommand(modelItem.OnClick)
			};
		}
	}
}
