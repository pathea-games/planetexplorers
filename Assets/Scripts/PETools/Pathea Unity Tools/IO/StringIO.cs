using UnityEngine;
using System;
using System.IO;
using System.Text;

namespace Pathea
{
	namespace IO
	{
		public static class StringIO
		{
			public static string LoadFromFile (string filename, Encoding encoding)
			{
				using (FileStream fs = new FileStream (filename, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					int size = (int)fs.Length;
					byte[] buf = new byte [size];
					fs.Read(buf, 0, size);
					string content = encoding.GetString(buf);
					fs.Close();
					return content;
				}
			}
			public static void SaveToFile (string filename, string content, Encoding encoding)
			{
				using (FileStream fs = new FileStream (filename, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					byte[] buf = encoding.GetBytes(content);
					fs.Write(buf, 0, buf.Length);
					fs.Close();
				}
			}
		}
	}
}
