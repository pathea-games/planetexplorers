using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;

public class GenCaveHM {
	CustomRandom myRand = null;
	SimplexNoise myNoise = null;
	public int NumSmoothOps = 0;
	public void init(CustomRandom _myrand, SimplexNoise _myNoise, int _caveWidthX, int _caveWidthZ, int _shrinkFactor, int _padding)
	{
		myRand = _myrand;
		myNoise = _myNoise;
		caveHMWidthX = _caveWidthX;
		caveHMWidthZ = _caveWidthZ;
		caveShrinkFactor = _shrinkFactor;
		cavePadding = _padding;
		if(_shrinkFactor == 0)
		{
			_shrinkFactor = 4;
		}
	}

	float[,] caveHeightmapSrc;
	public float[,] caveHeightmapFloor;
	public float[,] caveHeightmapCeiling;
	
	int[] fourDirections = new int[]{
		1,0,
		-1,0,
		0,1,
		0,-1
	};
	
	int caveShrinkFactor = 4;
	int caveHMWidthX;
	int caveHMWidthZ;
	int cavePadding;
	public int getCaveWidthX()
	{
		return caveHMWidthX;
	}
	public int getCaveWidthZ()
	{
		return caveHMWidthZ;
	}
	
	public void GenCave(int stopAtMiners, float MinerSpawnRate)
	{
		int dimX = caveHMWidthX / caveShrinkFactor;
		int dimZ = caveHMWidthZ / caveShrinkFactor;
		List<IntVector2> Miners = new List<IntVector2>();
		Miners.Add(new IntVector2(dimX / 2, dimZ / 2));
			
		byte[,] caveMapShrunk = new byte[dimX, dimZ];	//Already initialized with 0
		int[] DirectIndices = new int[4];
		int mx = 0;
		int my = 0;
		int minerCount = 0;
		for(int iter = 0; Miners.Count != 0 && minerCount < stopAtMiners; iter++)
		{
			for(int i = Miners.Count - 1; i >= 0; i-- )
			{
				// count the number of viable directions
				int cvincr = 0;
				for(int j = 0;j < 4; j++)
				{
					mx = Miners[i].x + fourDirections[j << 1];
					my = Miners[i].y + fourDirections[(j << 1) + 1];
					if(mx >= cavePadding && my >= cavePadding && mx < dimX - cavePadding && my < dimZ - cavePadding)
					{
						if(caveMapShrunk[mx, my] == 0)
						{
							DirectIndices[cvincr] = j;
							cvincr++;
						}
					}
				}
				if(cvincr == 0)
				{
					// no neighbour block available. this miner taps out
					Miners.RemoveAt(i);
				}
				else
				{
					int dirIdx = DirectIndices[Mathf.FloorToInt(myRand.Value * 0.999f * cvincr)];
					mx = Miners[i].x + fourDirections[dirIdx << 1];
					my = Miners[i].y + fourDirections[(dirIdx << 1) + 1];
					
					//dig
					caveMapShrunk[mx, my] = 1;
					
					// chance to spawn a new miner.
					if (myRand.Value < MinerSpawnRate)
					{
						Miners.Add(new IntVector2(Miners[i].x, Miners[i].y));
						minerCount++;
					}
					
					Miners[i].x = mx;
					Miners[i].y = my;
				}
			}
		}
	
		// usually 3 iterations would clean things up pretty well.
		for(int i = 0; i < NumSmoothOps; i++ )
		{
			smooth(caveMapShrunk, dimX, dimZ);
		}
		
		
		Texture2D caveTex = new Texture2D(dimX, dimZ, TextureFormat.ARGB32, false, true);
		caveTex.filterMode = FilterMode.Bilinear;
		caveTex.wrapMode = TextureWrapMode.Clamp;
		Color[] caveTexColors = new Color[dimX * dimZ];	//Already initialized with color(0,0,0,0)
		// copy it to the texture
		int idxCaveTexColors = 0;
		for(int y = 0; y < dimZ; y++)
		{
			for(int x = 0; x < dimX; x++)
			{
				float clrVal = (caveMapShrunk[x,y] == 1) ? 1.0f : 0.0f;
				caveTexColors[idxCaveTexColors++] = new Color(clrVal,clrVal,clrVal,1);
			}
		}
		caveTex.SetPixels(caveTexColors);
		// sample the texture to get the enlarged, cave array.
		caveHeightmapSrc = new float[caveHMWidthX, caveHMWidthZ];
		for(int z = 0; z < caveHMWidthZ; z++)
		{
			for(int x = 0; x < caveHMWidthX; x++)
			{
				caveHeightmapSrc[x, z] = caveTex.GetPixelBilinear((float)x / caveHMWidthX, (float)z / caveHMWidthZ).r;
				//caveHeightmapSrc[x, z] = Mathf.Sqrt(caveTex.GetPixelBilinear((float)x / caveHMWidthX, (float)z / caveHMWidthZ).r);
				//caveHeightmapSrc[x, z] = Mathf.Pow(caveTex.GetPixelBilinear((float)x / caveHMWidthX, (float)z / caveHMWidthZ).r, UnityEngine.Random.Range(0.5f,2.0f));
			}
		}
#if false		// debug texture
//		DebugTextures.Instance.textures = new Texture2D[5];
//		DebugTextures.Instance.textures[0] = caveTex;
		Texture2D caveHeightmapSrcTex = new Texture2D(caveHMWidthX, caveHMWidthZ, TextureFormat.ARGB32, false, true);
		caveHeightmapSrcTex.filterMode = FilterMode.Bilinear;
		caveHeightmapSrcTex.wrapMode = TextureWrapMode.Clamp;
		Color[] tmpColors = new Color[caveHMWidthX * caveHMWidthZ];
		int idxTmpColors = 0;
		for(int z = 0; z < caveHMWidthZ; z++)
		{
			for(int x = 0; x < caveHMWidthX; x++)
			{
				float r = caveHeightmapSrc[x,z];
				tmpColors[idxTmpColors++] = new Color(r,r,r,1);
			}
		}
		caveHeightmapSrcTex.SetPixels(tmpColors);
//		DebugTextures.Instance.textures[1] = caveHeightmapSrcTex;
#endif
	}
	
	void smooth(byte[,] map, int w, int h)
	{
		int mx, my;
		for(int y = 0; y < h; y++)
		{
			for(int x = 0; x < w; x++)
			{
				if( map[x,y] == 1)		continue;
				
				int cvincr = 0;
				for(int j = 0;j < 4; j++)
				{
					mx = x + fourDirections[j << 1];
					my = y + fourDirections[(j << 1) + 1];
					if(mx >= 0 && my >= 0 && mx < w && my < h)
					{
						if(map[mx, my] == 1)
						{
							cvincr++;
						}
					}
				}
				if(cvincr > 2)
				{
					map[x,y] = 1;
				}
			}
		}
	}
	public void ApplyNoise(	int bl_max_iterations, float bl_length, float bl_power,
							int bv_max_iterations, float bv_length, float bv_power)
	{
		caveHeightmapCeiling = new float[caveHMWidthX, caveHMWidthZ];
		caveHeightmapFloor = new float[caveHMWidthX, caveHMWidthZ];
		Array.Copy(caveHeightmapSrc, caveHeightmapCeiling, caveHMWidthX * caveHMWidthZ);
		Array.Copy(caveHeightmapSrc, caveHeightmapFloor, caveHMWidthX * caveHMWidthZ);
		
		ApplyNoiseToHeightmap(bl_max_iterations, bl_length, bl_power, caveHeightmapFloor, 34.56f);
		ApplyNoiseToHeightmap(bv_max_iterations, bv_length, bv_power, caveHeightmapCeiling, 45.321f);
		// rescale the heightmaps
		NormalizeHM(caveHeightmapFloor, caveHMWidthX, caveHMWidthZ);
		NormalizeHM(caveHeightmapCeiling, caveHMWidthX, caveHMWidthZ);
#if false		// debug textures
		Texture2D floorTex = new Texture2D(caveHMWidthX, caveHMWidthZ, TextureFormat.ARGB32, false, true);
		floorTex.filterMode = FilterMode.Bilinear;
		floorTex.wrapMode = TextureWrapMode.Clamp;
		Color[] tmpColors = new Color[caveHMWidthX * caveHMWidthZ];
		int idxTmpColors = 0;
		for(int z = 0; z < caveHMWidthZ; z++)
		{
			for(int x = 0; x < caveHMWidthX; x++)
			{
				float r = caveHeightmapFloor[x,z];
				tmpColors[idxTmpColors++] = new Color(r,r,r,1);
			}
		}
		floorTex.SetPixels(tmpColors);
		DebugTextures.Instance.textures[2] = floorTex;
		
		
		Texture2D ceilTex = new Texture2D(caveHMWidthX, caveHMWidthZ, TextureFormat.ARGB32, false, true);
		ceilTex.filterMode = FilterMode.Bilinear;
		ceilTex.wrapMode = TextureWrapMode.Clamp;
		idxTmpColors = 0;
		for(int z = 0; z < caveHMWidthZ; z++)
		{
			for(int x = 0; x < caveHMWidthX; x++)
			{
				float r = caveHeightmapCeiling[x,z];
				tmpColors[idxTmpColors++] = new Color(r,r,r,1);
			}
		}
		ceilTex.SetPixels(tmpColors);
		DebugTextures.Instance.textures[3] = ceilTex;
#endif	
	}
	void ApplyNoiseToHeightmap(int max_iterations, float _length, float _power, float[,] hmHandle, float offset = 0.0f)
	{
		float power = _power;
		float length = _length;
		
		for(int i = 0; i < max_iterations; i++){
			
			for(int z = 0; z < caveHMWidthZ; z++){
				for(int x = 0; x < caveHMWidthX; x++){
					float u = (float)x / (float)caveHMWidthX;
					float v = (float)z / (float)caveHMWidthZ;
					float changeFromNoise = (float)myNoise.Noise((u + offset)*length,(v + offset)*length);
					changeFromNoise *= caveHeightmapSrc[x,z];
					hmHandle[x, z] += changeFromNoise*power;
				}
			}
			power /= 2.0f;
			length *= 2.0f;
		}
	}
	const float trimLevel = 0.4f;
	const float reverseTrimLevel = 1.0f/(1.0f-trimLevel);
	void NormalizeHM(float[,] hmHandle, int w, int h)
	{
		float low = 65535.0f;
		float high = -65535.0f;
		for(int y = 0; y < h; y++){
			for(int x = 0; x < w; x++){
				if(hmHandle[x,y] < low)
					low = hmHandle[x,y];
				else if(hmHandle[x,y] > high)
					high = hmHandle[x,y];
			}
		}
		float range = high - low;
		
		for(int y = 0; y < h; y++){
			for(int x = 0; x < w; x++){
				hmHandle[x,y] = (hmHandle[x,y] - low) / range;
				
				// trim out the low values, and rescale the larger ones.
//				if(hmHandle[x,y] < trimLevel)
//					hmHandle[x,y] = 0.0f;
				//hmHandle[x,y] = (hmHandle[x,y] - trimLevel) * reverseTrimLevel;
//				hmHandle[x,y] = Mathf.Sqrt(hmHandle[x,y]);
			}
		}
		
	}
}