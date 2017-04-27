using System;
using System.Collections.Generic;
using System.Text;

namespace SI.Mobile.RPMSGViewer.Lib
{
	public static class ConfigurationLink
	{
		public static Dictionary<string, string> ParseQuery(string query) 
		{
			Dictionary<string, string> dict = new Dictionary<string, string>();

			string[] items = query.Split('&');
			foreach (string item in items)
			{
				string key = item.Substring(0, item.IndexOf('='));
				string base64Value = item.Substring(item.IndexOf('=') + 1);
				string value = Encoding.UTF8.GetString(Convert.FromBase64String(base64Value));
				dict[key] = value;
			}

			return dict;
		}
	}
}

