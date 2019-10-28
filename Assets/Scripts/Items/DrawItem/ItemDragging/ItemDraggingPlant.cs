using UnityEngine;
using System.Collections.Generic;

public class ItemDraggingPlant : ItemDraggingArticle
{
//    public IntVector3 mSafePos;

    public override void OnDragOut()
    {
        if (null != itemDragging)
        {
            PlantInfo addInfo = PlantInfo.GetPlantInfoByItemId(itemDragging.itemObj.protoId);
            ItemScript_Plant isp = gameObject.GetComponent<ItemScript_Plant>(); 
            isp.ResetModel(3, addInfo);
        }

        base.OnDragOut();
    }

	bool checkTerrain(Vector3 pos)
	{
		VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(Mathf.RoundToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.RoundToInt(pos.z));
		Collider[] cols = Physics.OverlapSphere(transform.position, 0.2f, -1 ^ (1 << Pathea.Layer.VFVoxelTerrain));
		foreach (Collider col in cols)
		{
			if (!col.isTrigger)
				return false;
		}
		
		if (voxel.Type == 19 || voxel.Type == 63)
			return true;
		return false;
	}

	protected override void FixPosByItemBounds (Ray cameraRay)
	{
		if (null == itemBounds)
		{
			mOverlaped = false;
			return;
		}

		if (!mRayHitTerrain)
		{
			itemBounds.activeState = false;
			return;
		}
		
		mHitPos.x = Mathf.RoundToInt(mHitPos.x);
		mHitPos.y -= 0.1f;
		mHitPos.z = Mathf.RoundToInt(mHitPos.z);			
		SetPos(mHitPos);
		
		CloseItemBounds ();
		
		if (null == mFindBounds)
			mFindBounds = new List<ItemDraggingBounds> ();
		
		List<ISceneObjAgent> sceneObjs = SceneMan.GetActiveSceneObjs (typeof(DragItemAgent), true);
		
		float curDis;
		bool rayHitBounds = false;

		Bounds selfBounds = itemBounds.worldBounds;
		selfBounds.size -= 0.01f * Vector3.one;
		Bounds otherBounds;
		
		for (int i = 0; i < sceneObjs.Count; ++i)
		{
			DragItemAgent dragItem = sceneObjs[i] as DragItemAgent;
			if(null != dragItem.gameObject)
			{
				ItemDraggingBounds checkBounds = dragItem.gameObject.GetComponentInChildren<ItemDraggingBounds>();
				if(null != checkBounds)
				{
					otherBounds = checkBounds.worldBounds;
					mFindBounds.Add(checkBounds);			
					if(otherBounds.IntersectRay(cameraRay, out curDis) && curDis <= mMinDis + 0.05f 
					   && Vector3.Distance(playerPos, cameraRay.origin + curDis * cameraRay.direction.normalized) < DraggingDistance)
					{
						mTooFar = false;
						mMinDis = curDis;
						mAdsorbItemBounds = checkBounds;
						mHitPos = cameraRay.origin + curDis * cameraRay.direction.normalized;
						rayHitBounds = true;

					}		
					if(mRayHitTerrain && otherBounds.Intersects(selfBounds))
						mAdsorbItemBounds = checkBounds;
				}
			}
		}
		
		if (null != mAdsorbItemBounds)
		{
			SetPos(mHitPos);
			
			mFindBounds.Remove(mAdsorbItemBounds);
			
			mAdsorbItemBounds.showBounds = true;
			mAdsorbItemBounds.activeState = true;
			
			otherBounds = mAdsorbItemBounds.worldBounds;
			
			bool hitLeft = false, hitRight = false, hitFront = false, hitBack = false, hitTop = false, hitBottom = false;
			if (rayHitBounds)
			{
				rootGameObject.transform.position = mHitPos;
				selfBounds = itemBounds.worldBounds;
				
				if(Mathf.Abs(mHitPos.x - otherBounds.min.x) < 0.01f)
					hitLeft = true;
				else if(Mathf.Abs(mHitPos.x - otherBounds.max.x) < 0.01f)
					hitRight = true;
				else if(Mathf.Abs(mHitPos.z - otherBounds.max.z) < 0.01f)
					hitBack = true;
				else if(Mathf.Abs(mHitPos.z - otherBounds.min.z) < 0.01f)
					hitFront = true;
				else if(Mathf.Abs(mHitPos.y - otherBounds.min.y) < 0.01f)
					hitBottom = true;
				else if(Mathf.Abs(mHitPos.y - otherBounds.max.y) < 0.01f)
					hitTop = true;
			}
			else
			{
				float maxDS = 100f;
				if(Mathf.Abs(selfBounds.max.x - otherBounds.min.x) < maxDS)
				{
					hitLeft = true;
					maxDS = Mathf.Abs(selfBounds.max.x - otherBounds.min.x);
				}
				if(Mathf.Abs(selfBounds.min.x - otherBounds.max.x) < maxDS)
				{
					hitLeft = false;
					hitRight = true;
					maxDS = Mathf.Abs(selfBounds.min.x - otherBounds.max.x);
				}
				if(Mathf.Abs(selfBounds.min.z - otherBounds.max.z) < maxDS)
				{
					hitLeft = false;
					hitRight = false;
					hitBack = true;
					maxDS = Mathf.Abs(selfBounds.min.z - otherBounds.max.z);
				}
				if(Mathf.Abs(selfBounds.max.z - otherBounds.min.z) < maxDS)
				{
					hitLeft = false;
					hitRight = false;
					hitBack = false;
					hitFront = true;
				}
			}
			if (hitLeft)
			{
				mHitPos += (otherBounds.min.x - selfBounds.max.x) * Vector3.right;
				mHitPos.x = Mathf.FloorToInt(mHitPos.x);
			}
			else if(hitRight)
			{
				mHitPos += (otherBounds.max.x - selfBounds.min.x) * Vector3.right;
				mHitPos.x = Mathf.CeilToInt(mHitPos.x);
			}
			else if(hitFront)
			{
				mHitPos += (otherBounds.min.z - selfBounds.max.z) * Vector3.forward;
				mHitPos.z = Mathf.FloorToInt(mHitPos.z);
			}
			else if(hitBack)
			{
				mHitPos += (otherBounds.max.z - selfBounds.min.z) * Vector3.forward;
				mHitPos.z = Mathf.CeilToInt(mHitPos.z);
			}
			else if(hitTop)
			{
				mHitPos += (otherBounds.max.y - selfBounds.min.y) * Vector3.up;
			}
			else if(hitBottom)
			{
				mHitPos += (otherBounds.min.y - selfBounds.max.y) * Vector3.up;
			}

			mHitPos.x = Mathf.RoundToInt(mHitPos.x);
			mHitPos.y -= 0.1f;
			mHitPos.z = Mathf.RoundToInt(mHitPos.z);			
			if(Physics.Raycast (mHitPos + Vector3.up, Vector3.down, out fhitInfo, 10f, layerMask))
				mHitPos.y = fhitInfo.point.y;
			SetPos(mHitPos);

			if(hitTop || hitBottom)
			{
				mOverlaped = true;
				itemBounds.activeState = false;
				return;
			}

			selfBounds = itemBounds.worldBounds;
			selfBounds.size -= 0.01f * Vector3.one;
			
			for(int i = 0; i < mFindBounds.Count; ++i)
			{
				if(null != mFindBounds[i] && selfBounds.Intersects(mFindBounds[i].worldBounds))
				{
					mOverlaped = true;
					itemBounds.activeState = false;
					mOverlapedItemBounds = mFindBounds[i];
					mOverlapedItemBounds.showBounds = true;
					mOverlapedItemBounds.activeState = false;
					mFindBounds.Clear();
					return;
				}
			}
		}

		
		if(!checkTerrain(mHitPos))
		{
			mOverlaped = true;
			itemBounds.activeState = false;
			return;
		}
		
		mFindBounds.Clear();
		mOverlaped = false;
		itemBounds.activeState = true;
	}
}
