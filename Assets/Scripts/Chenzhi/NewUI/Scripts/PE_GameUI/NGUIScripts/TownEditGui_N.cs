using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TownEditGui_N : UIStaticWnd
{
	static TownEditGui_N mInstance;
	public static TownEditGui_N Instance { get { return mInstance; } }
	public DragableBuildingFile_N mFilePerfab;
	public UIGrid mFileGrid;
	
	List<DragableBuildingFile_N> mBuildFileList = new List<DragableBuildingFile_N>();
	
	DragableBuildingFile_N mCurrentFile;
	
	EditBuilding mOpBuilding;
	
	//Vector3 mPressMousePos = Vector3.zero;
	
	void Awake()
	{
		mInstance = this;
	}
		
	public void ResetFileList()
	{
		List<string> fileNameList = new List<string>();
		foreach(int id in BlockBuilding.s_tblBlockBuildingMap.Keys)
			fileNameList.Add(System.IO.Path.GetFileNameWithoutExtension(BlockBuilding.s_tblBlockBuildingMap[id].mPath));
		
		foreach(DragableBuildingFile_N file in mBuildFileList)
			if(file)
				Destroy(file.gameObject);
		mBuildFileList.Clear();
		
		foreach(String fileName in fileNameList)
		{
			DragableBuildingFile_N addFile = Instantiate(mFilePerfab) as DragableBuildingFile_N;
			addFile.SetFile(fileName, gameObject);
			addFile.transform.parent = mFileGrid.transform;
			addFile.transform.localScale = Vector3.one;
			addFile.transform.localRotation = Quaternion.identity;
			mBuildFileList.Add(addFile);
		}
		mFileGrid.Reposition();
	}
	
	void OnFileDrag(DragableBuildingFile_N dragFile)
	{
		if(null == mOpBuilding && mCurrentFile != dragFile)
		{
			mCurrentFile = dragFile;
			TownEditor.Instance.OnCreateBuilding(mCurrentFile.FileName);
		}
	}
	
	public void SetOpBuild(EditBuilding editBuilding)
	{
		mOpBuilding = editBuilding;
		mCurrentFile = null;
	}
	
	void Update()
	{
		if(null != mOpBuilding)
		{
			if(Input.GetMouseButtonUp(0))
			{
				TownEditor.Instance.PutBuildingDown();
			}
			else if(Input.GetKeyUp(KeyCode.T))
			{
				TownEditor.Instance.TurnBuilding();
			}
			else if(Input.GetKeyUp(KeyCode.Escape))
			{
				TownEditor.Instance.CancelSelect();
			}
			else if(Input.GetKeyUp(KeyCode.Delete))
			{
				TownEditor.Instance.DeletBuilding();
			}
			else
			{
				if(null == UICamera.hoveredObject)
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					
					float nearestDis = 10000f;
					
					RaycastHit[] hitInfos = Physics.RaycastAll(ray,500f, 1 << Pathea.Layer.VFVoxelTerrain);
					if(hitInfos.Length > 0)
					{
						foreach(RaycastHit hit in hitInfos)
						{
							if(hit.distance < nearestDis && !hit.transform.name.Contains("B45Chnk"))
							{
								nearestDis = hit.distance;
								TownEditor.Instance.SetOpBuildingPosition(BuildBlockManager.BestMatchPosition(hit.point));
							}
						}
					}
				}
			}
		}
	}
}
