using UnityEngine;
using System.Collections;
using Pathea.Maths;
using System.IO;
using System;

public class VProjectSettings
{
    static Vector3 scale = new Vector3(256f, 128f, 256f);
    public Vector3 size
    {
        get
        {
            return new Vector3(scale.x * WorldPileCount.x
                , scale.y * WorldPileCount.y
                , scale.z * WorldPileCount.z);

        }
    }

	public static string Header = "VProjectSettings";
	public static int CurrentVersion = 0x1;

	public INTVECTOR3 WorldPileCount = INTVECTOR3.zero;
	public INTVECTOR3 WorldSize
	{
		get
		{
            return WorldPileCount;
		}
	}

	public bool SaveToFile (string filename)
	{
		try
		{
			using (FileStream fs = new FileStream (filename, FileMode.Create))
			{
				BinaryWriter w = new BinaryWriter (fs);
				w.Write(Header);
				w.Write(CurrentVersion);
				w.Write(WorldPileCount.x);
				w.Write(WorldPileCount.y);
				w.Write(WorldPileCount.z);
				w.Close();
				fs.Close();
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
	
	public bool LoadFromFile (string filename)
	{
		try
		{
			bool retval = true;
			using (FileStream fs = new FileStream (filename, FileMode.Open))
			{
				BinaryReader r = new BinaryReader (fs);
				string h = r.ReadString();
				if (string.Compare(h, Header) == 0)
				{
					int version = r.ReadInt32();
					switch (version)
					{
					case 0x1:
						WorldPileCount.x = r.ReadInt32();
						WorldPileCount.y = r.ReadInt32();
						WorldPileCount.z = r.ReadInt32();
						break;
					default:
						retval = false;
						break;
					}
				}
				else
				{
					retval = false;
				}
				r.Close();
				fs.Close();
			}
			return retval;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
