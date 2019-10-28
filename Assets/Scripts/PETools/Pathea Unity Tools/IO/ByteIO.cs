using UnityEngine;
using System;
using System.IO;

namespace Pathea
{
	namespace IO
	{
		public static class ByteIO
		{
			public static byte[] LoadBytes(string path)
			{
				try
				{
					using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
					{
						BinaryReader sr = new BinaryReader(fs);
						
						byte[] bs = sr.ReadBytes((int)fs.Length);
						
						sr.Close();
						fs.Close();
						
						return bs;
					}
				}
				catch
				{
					//Debug.LogWarning(ex);
				}
				
				return new byte[0];
			}
			
			public static void SaveBytes(string path, byte[] bytes)
			{
				try
				{
					using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
					{
						BinaryWriter sw = new BinaryWriter(fs);
						
						sw.Write(bytes);
						
						sw.Close();
						fs.Close();
					}
				}
				catch (Exception ex)
				{
					Debug.LogWarning(ex);
				}
			}
		}
	}
}
