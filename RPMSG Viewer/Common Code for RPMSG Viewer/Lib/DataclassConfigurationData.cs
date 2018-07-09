using System.Collections.Generic;
using System.Xml;

#if __ANDROID__
	using SI.Mobile.RPMSGViewer.android;
#endif

namespace SI.Mobile.RPMSGViewer.Lib
{
	public class DataclassConfigurationData
	{
		public string DataclassGroupName { get; private set; }
		public Dictionary<string, string> ClassificationList { get; private set; }

		private DataclassConfigurationData(string dataclassGroupName, Dictionary<string, string> classificationList)
		{
			DataclassGroupName = dataclassGroupName;
			ClassificationList = classificationList;
		}

		public static DataclassConfigurationData Create()
		{
#if __ANDROID__
			string xmlContent = Settings.GetString("Dataclasses", "");
#endif

#if __IOS__
			string xmlContent = AppUtils.GetSharedUserDefaults().StringForKey("Dataclasses");
#endif
			if (string.IsNullOrEmpty(xmlContent))
				xmlContent = AppUtils.ReadResourceFile("Classification/Configuration.xml");

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlContent);

			string dataclassGroupName = null;
			Dictionary<string, string> classificationList = new Dictionary<string, string>();

			XmlNode node = doc.DocumentElement;

			foreach (XmlNode child in node.ChildNodes) // Groups
			{
				dataclassGroupName = child.Attributes["name"].Value; //  handle multiple groups
				foreach (XmlNode grandchild in child.ChildNodes) // DataClasses
				{
					classificationList[grandchild.Attributes["name"].Value] = grandchild.Attributes["color"].Value;
				}
			}

			return new DataclassConfigurationData(dataclassGroupName, classificationList);
		}
	}
}

