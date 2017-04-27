using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SI.Mobile.RPMSGViewer.Lib;

namespace SI.Mobile.RPMSGViewer.Lib
{
    public static class StringUtils
    {
	    public static string GetFileName(this string path)
	    {
			return path.Substring(path.LastIndexOfAny(new char[] { '\\', '/' }) + 1);
	    }
			
		public static string ReadStringA(byte[] bytes, ref int startIndex)
		{
			return ReadString(bytes, ref startIndex, System.Text.Encoding.ASCII);
		}

		public static string ReadStringW(byte[] bytes, ref int startIndex)
		{
			return ReadString(bytes, ref startIndex, System.Text.Encoding.Unicode);
		}

		public static string ReadString(byte[] bytes, ref int startIndex, System.Text.Encoding enc)
		{
			int length = bytes[startIndex];
			length *= (byte)(enc.IsSingleByte ? 1 : 2);

			if (length == 0)
			{
				startIndex++;
				return null;
			}
			else
			{
				int prevIndex = startIndex + 1;
				startIndex = prevIndex + length;
				return enc.GetString(bytes, prevIndex, length);
			}
		}
    }
}
