using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using OpenMcdf;

namespace SI.Mobile.RPMSGViewer.Lib
{
    public static class StreamUtils
    {
		/// <summary>
		/// read entire stream buffer from stream
		/// </summary>
		/// <param name="source">Stream to read from</param>
		/// <returns>The stream content as byte array</returns>
		public static byte[] ReadAllBytes(this Stream source)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				source.CopyTo(ms);
				return ms.ToArray();
			}
		}

		public static byte[] ReadBytes(Stream stream, int count)
		{
			byte[] buffer = new byte[count];

			int total = 0;
			do
			{
				int read = stream.Read(buffer, total, count - total);
				if (read < 1)
					break;

				total += read;

			} while (total < count);

			Array.Resize(ref buffer, total);
			return buffer;
		}

		public static string ToString(this Stream source, Encoding encoding)
		{
			return encoding.GetString(ReadAllBytes(source));
		}

		[SecuritySafeCritical]
		public static string GetIQPDetailFromStream(CFStorage storage, string streamName)
		{
			CFStream IQPDetailStream = storage.GetStream(streamName);
			int IQPDetailSize = (int)IQPDetailStream.Size;
			byte[] IQPDetailBytes = IQPDetailStream.GetData(3, ref IQPDetailSize);
			return Encoding.UTF8.GetString(IQPDetailBytes);
		}
    }
}
