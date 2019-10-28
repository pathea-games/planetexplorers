using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class BuildGui_N : UIStaticWnd 
{
	public static BuildGui_N mInstance;
	
	public GameObject		mOperationWnd;
	
	public UISlicedSprite	mBgSpr;
	
	
	
	//int						mCurrentBrushID = 1;
	//int						mCurrentItemId = 30200001;
	//int						mMatPage = 0;
	//int						mBrushPage = 0;
	const int 				NumPerPage = 22;
	
	void Awake()
	{
		mInstance = this;
	}
	
	void Update()
	{
		
	}
	
	void OnOpWndChange()
	{
		mOperationWnd.SetActive(!mOperationWnd.activeSelf);
	}
	
	void OnOpButtonClick()
	{
		
	}
}
