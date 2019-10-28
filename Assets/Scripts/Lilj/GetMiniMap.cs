using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class GetMiniMap : MonoBehaviour 
{
	public bool start = false;
	
	public float weightTime = 10f;

	public LayerMask mGroundLayer = LayerMask.GetMask("Water", "VFVoxelTerrain");
	
	float worldSize = 18432;
	int texSize = 512;
	int mNumOneside;
	Texture2D mMinimap;
	
	GameObject 	mCameraObj;
	Camera 		mCamera;
	
	GameObject	mFloowObj;
	
	VFVoxelTerrain mTerrain;
	//VoxelEditor mEditor;
	
	public int mIndex = 0;
	
	public int mIndexTo = 1297;
	
	float mtime = 0;
	
//	RenderTexture rendertex = null;
	
	// Use this for initialization
	void Start () {
		mNumOneside = (int)(worldSize/texSize);
		mMinimap = new Texture2D(texSize,texSize,TextureFormat.RGB24,false);
		mTerrain = VFVoxelTerrain.self;
		//mEditor = GameObject.Find("Voxel Terrain").GetComponent<VoxelEditor>();
		
//		rendertex = new RenderTexture(512, 512,24);
	}
	
	IEnumerator waitTime(float time)
	{
		yield return new WaitForSeconds(1);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.G) && false == start)
		{
			start = true;
			GameObject go = GameObject.Find("Player");
			if(go != null )
			{
				go.SetActive(false);
			}
			mTerrain.saveTerrain = true;
			//mCameraObj = GameObject.Find("GameMainCamera");
//			mCameraObj.GetComponent<FreeCamera>().enabled = true;
			//mCameraObj.GetComponent<CameraController>().enabled = false;
//			mCameraObj.GetComponent<GameMainGUI>().enabled = false;
			//mEditor.m_drawGizmo = true;
			
			mFloowObj = new GameObject("mapcenterObj");//GameObject.Find("mapcenterObj");
			SceneMan.SetObserverTransform(mFloowObj.transform);
			
			mCamera = Camera.main;
			mCamera.clearFlags = CameraClearFlags.Skybox;
			mCamera.farClipPlane = 2500;
			mCamera.nearClipPlane = 1;
			mCamera.orthographic = true;
			mCamera.orthographicSize = 256;
			mCamera.cullingMask = mGroundLayer.value;
			mCamera.transform.position = new Vector3(texSize/2 + mIndex%mNumOneside * texSize,1500,texSize/2 + mIndex/mNumOneside * texSize);
			mCamera.transform.rotation = new Quaternion(0.7f,0,0,0.7f);
			mFloowObj.transform.position = mCamera.transform.position + 600 * Vector3.down;
			mtime = Time.time;
			
//			mCamera.targetTexture = rendertex;
			
			if (!Directory.Exists(Application.dataPath + "../../MiniMaps"))
	            Directory.CreateDirectory(Application.dataPath + "../../MiniMaps");
//			mTerrain.lodMan.Refresh(new Vector3(mCamera.transform.position.x,mCamera.transform.position.y-500,mCamera.transform.position.z));//(mCamera.transform.position.x,mCamera.transform.position.y-500,mCamera.transform.position.z);
			
		}
		
//		if(Input.GetKeyDown(KeyCode.R))
//		{
//			GameObject go = GameObject.Find("Player");
//			if(go != null )
//			{
//				go.active = false;
//			}
//			mTerrain.saveTerrain = true;
//			mCameraObj = GameObject.Find("GameMainCamera");
//			mCameraObj.GetComponent<FreeCamera>().enabled = true;
//			mCameraObj.GetComponent<CameraController>().enabled = false;
//			mCameraObj.GetComponent<GameMainGUI>().enabled = false;
//			mEditor.m_drawGizmo = true;
//			mCamera = Camera.mainCamera;
//			mCamera.clearFlags = CameraClearFlags.Skybox;
//			mCamera.farClipPlane = 2500;
//			mCamera.nearClipPlane = 1;
//			mCamera.isOrthoGraphic = true;
//			mCamera.orthographicSize = 256;
//			mCamera.cullingMask = 1<< Pathea.Layer.VFVoxelTerrain;
//			mCamera.transform.position = new Vector3(texSize/2 + mIndex%mNumOneside * texSize,1500,texSize/2 + mIndex/mNumOneside * texSize);
//			mCamera.transform.rotation = new Quaternion(0.7f,0,0,0.7f);
////			mTerrain.lodMan.Refresh(new Vector3(mCamera.transform.position.x,mCamera.transform.position.y-500,mCamera.transform.position.z));//.initStartLocation(mCamera.transform.position.x,mCamera.transform.position.y-500,mCamera.transform.position.z);
//			
//		}
//		
//		
//		if(mTerrain.chunkRebuildList.Count == 0)
//		{
//			if(Input.GetMouseButtonDown(1))
//			{
//				mMinimap.ReadPixels(new Rect(Screen.width/2-256,0,texSize,texSize),0,0);
//				mMinimap.Apply();
//				var bytes = mMinimap.EncodeToPNG();
//				File.WriteAllBytes(Application.dataPath + "/../../MiniMaps/MiniMap"+mIndex+".png", bytes);
//				GC.Collect();
//			}
//		}
	}
	
	void OnPostRender ()
	{
		if(start)
		{
			if((Time.time - mtime>12f))
			{
				mtime = Time.time;
//				RenderTexture.active = rendertex;
				mMinimap.ReadPixels(new Rect((Screen.width - texSize)/2,0,texSize,texSize),0,0);
				mMinimap.Apply();
				var bytes = mMinimap.EncodeToPNG();
				File.WriteAllBytes(Application.dataPath + "../../MiniMaps/MiniMap"+mIndex+".png", bytes);
				if(mIndex <mIndexTo)
				{
					mIndex++;
//					while(mIndex < mIndexTo) //&& (mIndex%36 < 16 || mIndex%36 > 27 || mIndex/36 < 8 || mIndex/36 > 19))
//						mIndex++;
					
//					RaycastHit rayHit;
//					float heigt = 100f;
//					if(Physics.Raycast(mCamera.transform.position,Vector3.down,out rayHit, 2000f, 1<< Pathea.Layer.VFVoxelTerrain))
//						heigt = rayHit.point.y;
					
					mCamera.transform.position = new Vector3(texSize/2 + mIndex%mNumOneside * texSize,1000,texSize/2 + mIndex/mNumOneside * texSize);
					
					mFloowObj.transform.position = mCamera.transform.position + 500f * Vector3.down;
					
					if(mIndex > mIndexTo)
						start = false;
				}
				else
					start = false;
				GC.Collect();
			}
		}
	}
}
