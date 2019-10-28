using ItemAsset;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript_Plant : ItemScript 
{
	FarmPlantLogic mPlant;
	
	int			mGrowTimeIndex;
	bool		mIsDead;
	GameObject	mModel;
	
	byte        mTerrainType;

    public delegate void PlantUpdated();

    public PlantUpdated plantUpdated;
	public GameObject mLowerWaterTex;
	public GameObject mLowerCleanTex;
	
	const double VarPerOp = 30.0;

	bool mInitBounds;

    public override void OnConstruct()
    {
        base.OnConstruct();
        mPlant = GetComponentInParent<FarmPlantLogic>();
		//mPlant = FarmManager.Instance.GetPlantByItemObjID(mItemObj.instanceId);
		if(null == mPlant)
		{
            //mPlant = FarmManager.Instance.CreatePlant(mItemObj.instanceId, PlantInfo.GetPlantInfoByItemId(protoId).mTypeID, transform.position);
            //mPlant.mTerrianType = mTerrainType;
            //mPlant.UpdateGrowRate(0);
            Debug.LogError(transform.position+"No logic layer!");
            return;
		}
        if (GetComponent<DragItemMousePickPlant>() == null)
            gameObject.AddComponent<DragItemMousePickPlant>();

        //mPlant.mPos = transform.position;
		mGrowTimeIndex = mPlant.mGrowTimeIndex;
		mIsDead = mPlant.mDead;
        ResetModel(mPlant.mGrowTimeIndex, mPlant.mPlantInfo); 
        UpdatModel();
	}
	
	public void ResetModel(int index, PlantInfo plantInfo, bool isdead = false)
	{
		if (!mInitBounds)
			InitBounds (plantInfo);
        if (index > plantInfo.mGrowTime.Length - 1)
        {
            index = plantInfo.mGrowTime.Length - 1;
        }
		mGrowTimeIndex = index;
		mIsDead = isdead;
		if(null != mModel)
			Destroy(mModel);
		if(plantInfo.mDeadModePath[index] != "0")
		{
			Object obj;
			if(mIsDead)
				obj = Resources.Load(plantInfo.mDeadModePath[index]);
			else
				obj = Resources.Load(plantInfo.mModelPath[index]);
			mModel = Instantiate(obj) as GameObject;
			mModel.transform.parent = rootGameObject.transform;
			mModel.transform.localPosition = Vector3.zero;
			mModel.transform.localScale = (mGrowTimeIndex == 1) ? (0.5f * Vector3.one) : Vector3.one;
			mModel.transform.rotation = Quaternion.identity;
//			mModel.AddComponent<BoxCollider>();
			ColliderClickTrigger collderTrigger = mModel.AddComponent<ColliderClickTrigger>();
			collderTrigger.mTrigerTarget = gameObject;
			collderTrigger.mLeftBtn = false;
			collderTrigger.mRightBtn = true;
		}

        if (plantUpdated != null)
        {
            plantUpdated();
        }
	}

	public void UpdatModel()
	{
		if(null != mPlant)
		{
			if(mGrowTimeIndex != mPlant.mGrowTimeIndex || mIsDead != mPlant.mDead)
				ResetModel(mPlant.mGrowTimeIndex, mPlant.mPlantInfo, mPlant.mDead);
			
			if(!mPlant.mDead && !mPlant.IsRipe)
			{
				if(mLowerWaterTex.activeSelf != mPlant.NeedWater)
					mLowerWaterTex.SetActive(mPlant.NeedWater);
				
				if(mLowerCleanTex.activeSelf != mPlant.NeedClean)
					mLowerCleanTex.SetActive(mPlant.NeedClean);
			}
			else
			{
				if(mLowerWaterTex.activeSelf)
					mLowerWaterTex.SetActive(false);
				
				if(mLowerCleanTex.activeSelf)
					mLowerCleanTex.SetActive(false);
			}
			if(null != mModel && null != mModel.GetComponent<Collider>())
			{
				float height = mModel.GetComponent<Collider>().bounds.max.y + 0.2f;
				mLowerWaterTex.transform.position = new Vector3(mLowerWaterTex.transform.position.x, height, mLowerWaterTex.transform.position.z);
				mLowerCleanTex.transform.position = new Vector3(mLowerCleanTex.transform.position.x, height, mLowerCleanTex.transform.position.z);
            }
            if (plantUpdated != null)
            {
                plantUpdated();
            }
		}
	}

	void InitBounds(PlantInfo plantInfo)
	{
		mInitBounds = true;
		ItemDraggingBounds draggingBounds = gameObject.AddComponent<ItemDraggingBounds>();
		Vector3 size = plantInfo.mSize * Vector3.one;
		size.y = plantInfo.mHeight;
		draggingBounds.ResetBounds (0.5f * plantInfo.mHeight * Vector3.up, size);
	}
}
