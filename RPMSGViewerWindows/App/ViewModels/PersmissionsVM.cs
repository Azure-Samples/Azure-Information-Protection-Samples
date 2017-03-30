using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.microsoft.rightsmanagement.mobile.viewer.lib;

namespace com.microsoft.rightsmanagement.windows.viewer.ViewModels
{
	internal class PersmissionsVM : BaseVM<EndUserLicense>
	{
		public string Protection => Model.TemplateName;
		public string Description => Model.Description;
		public string SignInAs => Model.IssuedTo;
		public string Owner => Model.Owner;
	}
}
