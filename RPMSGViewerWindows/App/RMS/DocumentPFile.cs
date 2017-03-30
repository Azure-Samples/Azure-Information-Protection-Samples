using System.Collections.Generic;
using com.microsoft.rightsmanagement.mobile.viewer.lib;

namespace com.microsoft.rightsmanagement.windows.viewer.RMS
{
	internal class DocumentPFile : DocumentPfile
	{
		public DocumentPFile()
		{
		}

		public DocumentPFile(CreationData creationData) : base(creationData)
		{
		}

		protected override Package CreatePackage()
		{
			return new PackagePfileWindows(FilePath);
		}

		public override List<string> Extensions => new List<string> {".pfile"};

		public override List<string> SupportedMimeTypes => new List<string>();
		public override string MimeTypeForDisplay => "";
	}
}