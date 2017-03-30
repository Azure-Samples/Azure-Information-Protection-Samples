using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.microsoft.rightsmanagement.mobile.viewer.lib;

namespace com.microsoft.rightsmanagement.windows.viewer.Models
{
	class MainWindowModel
	{
        public Action<string> SaveAs;
        public Action Print;
        public Action<string> OpenDocument;
        public EndUserLicense EUL;
        public string OriginalExtension;
		public string OpenWithFile;
		public Action SignOut;
		public Action SendFeedback;
	}
}
