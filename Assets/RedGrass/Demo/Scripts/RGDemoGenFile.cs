using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RedGrass;

public class RGDemoGenFile : MonoBehaviour 
{
	const int tile = 32;
	const int xcount = 192;
	const int zcount = 192;
	const int xzcount = xcount*zcount;
	const int xstart = -3072;
	const int zstart = -3072;
	const int xend = xstart + xcount * tile;
	const int zend = zstart + zcount * tile;
	const string filename = "D:/grassinst_test.dat";
	
	List<RedGrassInstance>[] allGrasses;
	int[] offsets;
	int[] lens;
	FileStream fs;
	BinaryWriter w;
	
	int x = xstart;
	int z = zstart;
	int idx = 0;

	// Use this for initialization
	void Start ()
	{
		allGrasses = new List<RedGrassInstance>[xzcount];
		offsets = new int [xzcount];
		lens = new int [xzcount];
		for ( int i = 0; i < xzcount; ++i )
		{
			allGrasses[i] = new List<RedGrassInstance> ();
		}
		fs = new FileStream (filename, FileMode.OpenOrCreate, FileAccess.Write);
		w = new BinaryWriter (fs);
		w.Seek(xzcount*8, SeekOrigin.Begin);
	}

	// Update is called once per frame
	void Update ()
	{
		GenOneTile();
		GenOneTile();
		GenOneTile();
		GenOneTile();	
	}

	void GenOneTile ()
	{
		if ( idx == xzcount )
		{
			w.Seek(0, SeekOrigin.Begin);
			for ( int i = 0; i < xzcount; ++i )
			{
				w.Write(offsets[i]);
				w.Write(lens[i]);
			}
			w.Close();
			fs.Close();
			MonoBehaviour.Destroy(this);
			return;
		}
		
		offsets[idx] = (int)(fs.Position);
		RaycastHit rch;
		SimplexNoise noise = new SimplexNoise ();
		
		for ( int _x = 0; _x < tile; ++_x )
		{
			for ( int _z = 0; _z < tile; ++_z )
			{
				Vector3 pos = new Vector3 (x + _x + 0.5f, 1043, z + _z + 0.5f);
				float ns = (float)(noise.Noise(pos.x/64.0f, pos.y/64.0f, pos.z/64.0f));
				float nst = (float)(noise.Noise(pos.y/100.0f, pos.z/100.0f, pos.x/100.0f));
				if ( ns < -0.48f )
				{
					continue;
				}
				float p = Mathf.Clamp01((float)(ns) + 0.5f);
				if ( Physics.Raycast(pos, Vector3.down, out rch, 1000, 1 << Pathea.Layer.VFVoxelTerrain) ) 
				{
					RedGrassInstance vgi = new RedGrassInstance();
					vgi.Position = rch.point;
					vgi.Density = p;
					vgi.Normal = rch.normal;
					vgi.ColorF = Color.white;
					if ( Random.value > Mathf.Sqrt(rch.normal.y) )
					{
						vgi.Prototype = nst > 0 ? 64 : 65;
						allGrasses[idx].Add(vgi);
					}
					vgi.Prototype = nst > 0 ? 0 : 1;
					allGrasses[idx].Add(vgi);
					Debug.DrawRay(rch.point, rch.normal, new Color(rch.normal.x*0.5f+0.5f, rch.normal.y*0.5f+0.5f, rch.normal.z*0.5f+0.5f), 1);
				}
			}
		}
		lens[idx] = allGrasses[idx].Count;
		
		foreach ( RedGrassInstance vgi in allGrasses[idx] )
		{
			vgi.WriteToStream(w);
		}
		allGrasses[idx].Clear();
		
		x += tile;
		if ( x > xend-1 )
		{
			z += tile;
			x = xstart;
		}
		idx++;
		Debug.Log(idx.ToString());
	}

}
