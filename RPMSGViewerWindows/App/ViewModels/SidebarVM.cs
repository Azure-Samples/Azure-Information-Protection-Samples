using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using com.microsoft.rightsmanagement.windows.viewer.Models;

namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	class SidebarVM : BaseVM<SidebarModel>
	{
		private Visibility _visibility;
		public Visibility Visibility
		{
			get { return _visibility; }
			set
			{
				_visibility = value;
				switch (value)
				{
					case Visibility.Visible:
						Width = 200;
						break;
					case Visibility.Collapsed:
						Width = 50;
						break;
				}
				OnPropertyChanged("Width");
			}
		} 

		public int Width { get; set; }

		public class Item
		{
			public IconVM Icon { get; set; }
			public string Description { get; set; }
			public ICommand OnClick { get; set; }
			public bool Checked { get; set; }
		}

		public List<Item> TopItems => Model.TopItems?.Select(ItemModel2ItemVM).ToList();
		public List<Item> BottomItems => Model.BottomItems?.Select(ItemModel2ItemVM).ToList();

		private Item ItemModel2ItemVM(SidebarModel.Item itemModel)
		{
			return new Item
			{
				Icon = new IconVM {Model = itemModel.Icon},
				Description = itemModel.Description,
				OnClick = new DelegateCommand(itemModel.Click),
				Checked = Model.SelectedItem == itemModel
			};
		}

		public SidebarVM()
		{
			Visibility = Visibility.Visible;
		}
	}
}
