using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SI.Mobile.RPMSGViewer.Lib;
using IOPath = System.IO.Path;

#if __ANDROID__
	using Android.Content;
	using Android.Graphics;
	using Android.Graphics.Drawables;
	using SI.Mobile.RPMSGViewer.android;
#endif

#if __IOS__	
	using Foundation;
	using UIKit;
#endif

namespace SI.Mobile.RPMSGViewer.Lib
{
	public static class AppUtils
	{
		private const string HTML_BODY_START_TAG = "<(?i)body.*?>";
		private const string HTML_BODY_END_TAG   = "</(?i)body>";
		private const string HTML_HEAD_START_TAG = "<(?i)head.*?>";

#if __ANDROID__
		static public Context ApplicationContext { get; set; }
#endif

#if __IOS__
		public static NSUserDefaults GetSharedUserDefaults()
		{
			return new NSUserDefaults("group.com.secureislands.mobile.keyboard", NSUserDefaultsType.SuiteName);
		}
#endif

		static public bool IsTablet 
		{
			get 
			{ 
#if __IOS__
				string deviceModel = UIDevice.CurrentDevice.Model;
				return deviceModel.StartsWith("iPad", StringComparison.InvariantCultureIgnoreCase);
#endif

#if __ANDROID__
				return ApplicationContext.Resources.GetBoolean(Resource.Boolean.IsTablet);
#endif
			}
		}
			
		public static string GetErrorPage(string title, string details)
		{
			return ReadResourceFile(SIConstants.ERROR_PAGE_FILE_PATH).Replace("#ERROR_TITLE#", title).
				Replace("#ERROR_DETAILS#", details);
		}

		public static string GetDefaultPage(bool landscapeMode = false)
		{
			string welcomeImg;
			if (IsTablet)
				welcomeImg = landscapeMode ? "welcome_landscape" : "welcome_portrait";
			else
				welcomeImg = "welcome_phone";
			
			return ReadResourceFile(SIConstants.DEFAULT_PAGE_FILE_PATH).Replace("[WELCOME_SCREEN]", welcomeImg);
		}

#if __IOS__
		public static string GetDefaultPage(UIInterfaceOrientation orientation)
		{
			bool landscapeMode = orientation == UIInterfaceOrientation.LandscapeLeft || orientation == UIInterfaceOrientation.LandscapeRight;
			return GetDefaultPage(landscapeMode);
		}
#endif

		public static string GetLoadingPage()
		{
			return ReadResourceFile(SIConstants.LOADING_PAGE_FILE_PATH).Replace("#TITLE#", "Processing File");
		}
			
		public static string GetAboutPage()
		{
			return ReadResourceFile(SIConstants.ABOUT_PAGE_FILE_PATH);
		}

		public static string ReadResourceFile(string path)
		{
#if __IOS__	
			return File.ReadAllText(path);
#endif

#if __ANDROID__
			return ApplicationContext.Assets.Open(path).ToString(Encoding.UTF8);
#endif
		}

		public static string GeneratePermissionsHtml(string htmlContent, EndUserLicense eul)
		{
			List<string> permissions = new List<string>();
			if (eul._Rights.Extract)
				permissions.Add("Extract");
			if (eul._Rights.Forward)
				permissions.Add("Forward");
			if (eul._Rights.Reply)
				permissions.Add("Reply");
			if (eul._Rights.ReplyAll)
				permissions.Add("Reply All");
			if (eul._Rights.Print)
				permissions.Add("Print");
			
			string permissionList = "";
			foreach (string permission in permissions)
			{
				permissionList += "<li>" + permission + "</li>";
			}

			return htmlContent
				.Replace("[TEMPLATE_NAME]", eul.TemplateName)
				.Replace("[DESCRIPTION]", eul.Description)
				.Replace("[ISSUED_TO]", eul.IssuedTo)
				.Replace("[OWNER]", eul.Owner)
				.Replace("[RMS_RIGHTS]", permissionList);
		}

		public static void LoadImagesInHtml(DRMContent data)
		{
			string result = data.HTMLBody;
			List<RpmsgAttachment> notEmbeddedAttachments = new List<RpmsgAttachment>();

			try
			{
				foreach (RpmsgAttachment attachment in data.Attachments)
				{
					if (attachment.ContentID == null || !result.Contains(attachment.ContentID))
					{
						notEmbeddedAttachments.Add(attachment);
						continue;
					}
					string imgPath = IOPath.Combine(IOPath.GetTempPath(), Guid.NewGuid() + attachment.Name);
					File.WriteAllBytes(imgPath, attachment.Content);

					result = result.Replace("cid:" + attachment.ContentID, imgPath);
				}

				data.HTMLBody = result;
				data.Attachments = notEmbeddedAttachments;
			}
			catch (Exception)
			{
				LogUtils.Log("Failed loading inline images for display");
			}
		}
				
		public static string AddHtmlHeaderAndAttachments(DRMContent drmContent, EndUserLicense userLicense)
		{
			string html = EmbedHeadScriptsAndStyles(drmContent.HTMLBody, userLicense);
			html = EmbedAttachments(html, drmContent, userLicense);
			html = AddMailDetailsHeader(html, drmContent, userLicense);
			return html;
		}

		private static string EmbedHeadScriptsAndStyles(string html, EndUserLicense userLicense)
		{
			string fixPositionScript = ReadResourceFile(IOPath.Combine(SIConstants.DEFAULT_RESOURCE_FOLDER, "MailHtml/FixPositionScript.txt"));
			string restrictImagesWidthStyle = ReadResourceFile(IOPath.Combine(SIConstants.DEFAULT_RESOURCE_FOLDER, "MailHtml/RestrictImagesWidth.txt"));
			string preventTextResize = ReadResourceFile(IOPath.Combine(SIConstants.DEFAULT_RESOURCE_FOLDER, "MailHtml/PreventTextResize.txt"));
			string preventTextCopy = ReadResourceFile(IOPath.Combine(SIConstants.DEFAULT_RESOURCE_FOLDER, "MailHtml/PreventTextCopy.txt"));

			string headStartTag = Regex.Match(html, HTML_HEAD_START_TAG).Value;

			string styles = preventTextResize + restrictImagesWidthStyle;
			if (userLicense._Rights.Extract == false)
				styles += preventTextCopy;
			
			string modifiedHtml = html.Replace(headStartTag, headStartTag + styles + fixPositionScript);

			return modifiedHtml;
		}
			
		private static string EmbedAttachments(string html, DRMContent drmContent, EndUserLicense userLicense)
		{
			if (drmContent.Attachments == null)
				return html;

			string bodyEndTag = Regex.Match(html, HTML_BODY_END_TAG).Value;
			string fixedDiv = "<div id=\"si_bottomFixed_10BF6356-9C77-4EE2-8A0F-B0F2BEFBC225\" style=\"position:relative; bottom:0px; left:0px\" >"; //the id of the div MUST be the same as the ID in the attachmentFixPositionScript
			html = html.Replace(bodyEndTag, fixedDiv + bodyEndTag);

			foreach (RpmsgAttachment att in drmContent.Attachments) 
			{
				string base64AppIcon = null;
				string base64ShareIcon = null;

#if __IOS__	
				UIImage appIcon = GetIconImageFromExtension(att.Extension);
				base64AppIcon = appIcon.AsPNG().GetBase64EncodedString(NSDataBase64EncodingOptions.None);

				UIImage shareIcon = UIImage.FromFile("MailHtml/Attachments/share.png");
				base64ShareIcon = shareIcon.AsPNG().GetBase64EncodedString(NSDataBase64EncodingOptions.None);
#endif

#if __ANDROID__
				Drawable appIcon = ApplicationContext.Resources.GetDrawable(Utils.GetIconIdFromExtension(att.Extension));
				base64AppIcon = GetBase64EncodedFromDrawable(appIcon);

				Drawable shareIcon = ApplicationContext.Resources.GetDrawable(Resource.Drawable.share);
				base64ShareIcon = GetBase64EncodedFromDrawable(shareIcon);
#endif

				html = html.Replace 
					(
						bodyEndTag, 
						att.GetAttachmentHTML(base64AppIcon,base64ShareIcon) + bodyEndTag
					);
			}

			return html.Replace(bodyEndTag, "</div>" + bodyEndTag);
		}

#if __IOS__	
		static public UIImage GetIconImageFromExtension(string extension)
		{
			switch (extension) 
			{
				case ".docx":
					return UIImage.FromFile("MailHtml/Attachments/wordicon.png");
				case ".pptx":
					return UIImage.FromFile("MailHtml/Attachments/ppicon.png");
				case ".xlsx":
					return UIImage.FromFile("MailHtml/Attachments/excelicon.png");
				case ".jpg":
					return UIImage.FromFile("MailHtml/Attachments/jpgicon.png");
				case ".png":
					return UIImage.FromFile("MailHtml/Attachments/pngicon.png");
				case ".pdf":
					return UIImage.FromFile("MailHtml/Attachments/pdficon.png");
				case ".txt":
					return UIImage.FromFile("MailHtml/Attachments/txticon.png");
				case ".zip":
					return UIImage.FromFile("MailHtml/Attachments/zipicon.png");
				case ".rpmsg":
					return UIImage.FromFile("MailHtml/Attachments/rpmsgicon.png");
				default:
					return UIImage.FromFile("MailHtml/Attachments/defaulticon.png");
			}
		}
#endif

		private static string AddMailDetailsHeader(string html, DRMContent drmContent, EndUserLicense userLicense)
		{
			if (userLicense._SIData == null)
				return html;
			
			string headerHtml = ReadResourceFile(IOPath.Combine(SIConstants.DEFAULT_RESOURCE_FOLDER, "MailHtml/Header/MailDetailsHeader.txt"));
			string recipients = string.Join(", ", userLicense._SIData.ToNames);
			if (userLicense._SIData.CCNames.Length > 0)
				recipients += ", " + string.Join(", ", userLicense._SIData.CCNames);

			bool noAttachments = drmContent.Attachments.Count == 0;

			string base64AttachmentsIcon = null;

#if __IOS__	
			UIImage attachmentsIcon = UIImage.FromFile("MailHtml/Header/attachmentsIcon.png");
			base64AttachmentsIcon = attachmentsIcon.AsPNG().GetBase64EncodedString(NSDataBase64EncodingOptions.None);
#endif

#if __ANDROID__
			Drawable shareIcon = ApplicationContext.Resources.GetDrawable(Resource.Drawable.attachmentsIcon);
			base64AttachmentsIcon = GetBase64EncodedFromDrawable(shareIcon);
#endif

			string attachmentsHtml = noAttachments ? "" : drmContent.Attachments.Count +
				"<img src=\"data:image/png;base64," + base64AttachmentsIcon + "\" alt=\"attachmentIcon\" style=\" height:0.9em; margin-top: 0.1em\" />";
			
			headerHtml = headerHtml
				.Replace("[MailSubject]", userLicense._SIData.Subject)
				.Replace("[SenderName]", userLicense._SIData.SenderName)
				.Replace("[SendTime]", userLicense._SIData.SendTime)
				.Replace("[RecipientNames]", recipients)
				.Replace("[AttachmentCount]", attachmentsHtml)
				.Replace("[SenderInitial]", userLicense._SIData.SenderName.Trim().Substring(0, 1).ToUpper());

			RpmsgClassification primaryClassification = userLicense._SIData.Classifications.Find(cls => cls.Color != null);

			//there is no classification with color
			if (primaryClassification == null && userLicense._SIData.Classifications.Count > 0) 
			{
				//take the first dataclass
				primaryClassification = userLicense._SIData.Classifications[0];
			}
				
			if (primaryClassification != null) 
			{
				headerHtml = headerHtml.Replace ("<!--[ClassificationIndicator]-->", primaryClassification.GetClassificationHTML());
			} 

			string bodyStartTag = Regex.Match(html, HTML_BODY_START_TAG).Value;
			return html.Replace(bodyStartTag, bodyStartTag + headerHtml);
		}

#if __ANDROID__
		private static string GetBase64EncodedFromDrawable(Drawable drawable)
		{
			using (MemoryStream stream = new MemoryStream ()) 
			{
				((BitmapDrawable)drawable).Bitmap.Compress (Bitmap.CompressFormat.Png, 100, stream);
				return Android.Util.Base64.EncodeToString (stream.ToArray (), Android.Util.Base64Flags.Default);
			}
		}
#endif

		// workaround Xamarin bug https://bugzilla.xamarin.com/show_bug.cgi?id=30709
		static Dictionary<Int32, Encoding> m_EncodingCache = new Dictionary<Int32, Encoding>();

		public static Encoding GetEncoding(Int32 codepage)
		{
			if (m_EncodingCache.ContainsKey (codepage)) {
				return m_EncodingCache [codepage];
			}

			Encoding encoding = Encoding.GetEncoding (codepage);

			if (encoding == null) 
				encoding = Encoding.UTF8; // fallback to UTF8

			m_EncodingCache [codepage] = encoding;
			return encoding;
		}

		public static string GenerateClassificationLine(string dataclass)
		{
			return "<<Classification: \"" + dataclass + "\">>";
		}

#if __IOS__
		public static UIColor CreateColor(string hexColor)
		{
			try
			{
				hexColor = hexColor.Replace("#", "");
				int red = Int32.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				int green = Int32.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				int blue = Int32.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
				return UIColor.FromRGB(red, green, blue);
			}
			catch
			{
				return UIColor.Gray;
			}
		}
#endif
	}
}
