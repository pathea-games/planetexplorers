using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class CompountMiniMap : MonoBehaviour
{
	
	int oldTexSize = 512;
	int newTexSize = 2048;
	int totalPixe = 18432;
	int texOneSide;
	public int IndexStart = 0;
	public int IndexEnd = 35;
	bool mStart = false;	
	// Use this for initialization
	void Start ()
	{
		texOneSide = totalPixe / newTexSize;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
		if (Input.GetKeyDown (KeyCode.C)) {
			mStart = true;
			if (!Directory.Exists(Application.dataPath + "../../newMiniMap)"))
	            Directory.CreateDirectory(Application.dataPath + "../../newMiniMap");
		}
		if (Input.GetKeyDown (KeyCode.N)) {
			mStart = false;
		}
		if (mStart)
		{
			Texture2D mTexture = new Texture2D (newTexSize, newTexSize, TextureFormat.RGB24, false);
			Dictionary<int,Texture2D> mTexs = new Dictionary<int,Texture2D> ();
			
			bool end = false;
			for (int i=0; i<newTexSize; i++) {
				for (int j=0; j<newTexSize; j++) {
					if (Input.GetKeyDown (KeyCode.N)) {
						return;
					}
					int posX = IndexStart % (totalPixe / newTexSize) * newTexSize + i;
					int posY = IndexStart / (totalPixe / newTexSize) * newTexSize + j;
						
					int Index = posX / oldTexSize + posY / oldTexSize * (totalPixe / oldTexSize);
					if (!mTexs.ContainsKey (Index)) {
						Texture2D addTex = Resources.Load ("MiniMaps/MiniMap" + Index) as Texture2D;
						if (addTex != null) {
							mTexs.Add (Index, addTex);
						} else {
							end = true;
							mStart = false;
							break;
						}
					}
					if (!end) {
						IntVec2 offset = new IntVec2 ();
						offset.x = posX - Index % (totalPixe / oldTexSize) * oldTexSize;
						offset.y = posY - Index / (totalPixe / oldTexSize) * oldTexSize;
							
						mTexture.SetPixel (i, j, mTexs [Index].GetPixel (offset.x, offset.y));	
					}
				}
				if (end)
					break;
			}

			mTexture.Apply ();
			var bytes = mTexture.EncodeToPNG ();
			Destroy (mTexture);
			mTexs.Clear ();
			// For testing purposes, also write to a file in the project folder
			File.WriteAllBytes (Application.dataPath + "../../newMiniMap/MiniMap" + (IndexStart%texOneSide + (texOneSide - 1 - IndexStart/texOneSide) * texOneSide) + ".png", bytes);
			IndexStart++;
			if(IndexStart > IndexEnd)
				mStart = false;
			GC.Collect ();
		}
	}
}
