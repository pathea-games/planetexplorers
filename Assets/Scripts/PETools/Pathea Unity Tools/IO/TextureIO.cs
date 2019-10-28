using UnityEngine;
using System;
using System.IO;

namespace Pathea
{
	namespace IO
	{
		public static class FileUtil
		{
			// Load a texture from a png file
            public static bool LoadTextureFromFile(Texture2D tex, string filename)
			{
				if ( tex == null )
					return false;
				if ( !File.Exists(filename) )
					return false;
				
				byte[] texbuf = null;
				try
				{
					FileStream fs = new FileStream (filename, FileMode.Open);
					if ( fs.Length > 268435456 || fs.Length < 8 )
					{
						fs.Close();
						return false;
					}
					texbuf = new byte [(int)fs.Length];
					fs.Read(texbuf, 0, (int)fs.Length);
					fs.Close();
				}
				catch (Exception)
				{
					return false;
				}
				return tex.LoadImage(texbuf);
			}

			// Save a texture to a png file
            public static bool SaveTextureToFile(Texture2D tex, string filename)
			{
				if ( tex == null )
					return false;
				byte[] texbuf = tex.EncodeToPNG();
				try
				{
					FileStream fs = new FileStream (filename, FileMode.Create);
					fs.Write(texbuf, 0, texbuf.Length);
					fs.Close();
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}

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
				catch (Exception ex)
				{
					Debug.LogWarning(ex);
				}
				
				return new byte[0];
			}
			
			public static Shader LoadShader(string path)
			{
				using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					BinaryReader sr = new BinaryReader(fs);
					
					byte[] bs = sr.ReadBytes((int)fs.Length);

					sr.Close();
					fs.Close();

					string sh = System.Text.Encoding.UTF8.GetString(bs);
					Material mat = new Material (Shader.Find( sh));
					Shader shader = mat.shader;
					return shader;
				}
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
