using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using SI.Mobile.RPMSGViewer.Lib;
using System.IO;

#if __IOS__	
using Foundation;
#endif

namespace SI.Mobile.RPMSGViewer.Lib
{
	public class RpmsgClassification
	{
		NameValueCollection m_Attributes;

		public string Name { get; private set; }
		public List<byte> Color { get; private set; }

		public RpmsgClassification (string classificationString)
		{
			m_Attributes = HttpUtility.ParseQueryString (classificationString);

			Name = m_Attributes ["N"];

			string colorHexValue = m_Attributes ["C"];
			if (colorHexValue == null) 
			{
				Color = null;
				return;
			}

			Color = new List<byte> ();
			for (int i = 1; i < 7; i += 2) 
			{
				string hexValue = colorHexValue.Substring (i, 2);
				Color.Add(Convert.ToByte(hexValue, 16));
			}
		}
			
		public static List<RpmsgClassification> GetClassifications(string allClassificationsString)
		{
			NameValueCollection classificationNameValues = HttpUtility.ParseQueryString (allClassificationsString);


			List<RpmsgClassification> classifiications = new List<RpmsgClassification>();
			foreach (string key in classificationNameValues.AllKeys) 
			{
				classifiications.Add(new RpmsgClassification(classificationNameValues[key]));
			}

			return classifiications;
		}
			
		public string GetClassificationHTML()
		{	
			string attachmentTemplate = AppUtils.ReadResourceFile (Path.Combine (SIConstants.DEFAULT_RESOURCE_FOLDER, "MailHtml/Header/Classification.txt"));

			string attachmentDiv = attachmentTemplate.Replace ("[ClassificationName]", Name);

			if (Color != null) {
				attachmentDiv = attachmentDiv.Replace ("[ClassificationColor]", "#" + Color [0].ToString ("X2") + Color [1].ToString ("X2") + Color [2].ToString ("X2"));
			} 
			else 
			{
				attachmentDiv = attachmentDiv.Replace ("background-color: [ClassificationColor];", "display: none;");
			}

			return attachmentDiv;
		}
	}
}

