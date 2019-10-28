using UnityEngine;
using System.Collections.Generic;
using Pathea;
using Pathea.Maths;
using ItemAsset.PackageHelper;

public class ItemDraggingBase : MonoBehaviour
{
    public float DraggingDistance = 10f;

    public ItemAsset.Drag itemDragging;

	public ItemDraggingBounds itemBounds;

	public float subTerrainClearRadius = 1;
	
	protected ItemDraggingBounds mAdsorbItemBounds;

	protected ItemDraggingBounds mOverlapedItemBounds;

	protected List<ItemDraggingBounds> mFindBounds;
	
	protected static readonly int layerMask = 1 << Layer.VFVoxelTerrain
			| 1 << Layer.GIEProductLayer
		   	| 1 << Layer.SceneStatic
		   	| 1 << Layer.Unwalkable;

	protected static readonly int treeAndEntityLayer = 1 << Layer.NearTreePhysics
			| 1 << Layer.Player
			| 1 << Layer.AIPlayer;

	protected static readonly float treeCheckHeight = 1f;

	protected virtual bool canPutUp { get { return false; }}

    public int itemObjectId
    {
        get
        {
            if (itemDragging == null)
            {
                return Pathea.IdGenerator.Invalid;
            }

            return itemDragging.itemObj.instanceId;
        }
    }

    public GameObject rootGameObject
    {
        get
        {
            return gameObject;
        }
    }

    protected void SetPos(Vector3 pos)
    {
        rootGameObject.transform.position = pos;
    }

    Pathea.PeTrans mTrans = null;
    protected Vector3 playerPos
    {
        get
        {
			if (null == mTrans && null != Pathea.PeCreature.Instance.mainPlayer)
            {
                mTrans = Pathea.PeCreature.Instance.mainPlayer.peTrans;
            }

            if (null == mTrans)
            {
                return Vector3.zero;
            }

            return mTrans.position;
        }
    }

	void OnDestroy()
	{
		CloseItemBounds ();
	}

    #region Dragging
    public virtual void OnDragOut()
    {
        transform.rotation = Quaternion.identity;
        Rigidbody rigb = rootGameObject.GetComponentInChildren<Rigidbody>();
        if (rigb != null)
            rigb.detectCollisions = false;

        Collider[] colls = GetComponentsInChildren<Collider>();
        foreach (Collider coll in colls)
        {
            coll.enabled = false;
            if (coll is MeshCollider)
            {
                coll.isTrigger = true;
            }
        }

		if(null == itemBounds)
			itemBounds = GetComponent<ItemDraggingBounds>();
		if (null != itemBounds)
			itemBounds.showBounds = true;
    }

	protected bool mOverlaped = false;
    protected bool mTooFar = false;
	protected bool mRayHitTerrain = false;	
	protected bool mHasTree = false;
    protected RaycastHit fhitInfo;
	protected Vector3 mHitPos;
	protected float mMinDis;
    public virtual bool OnDragging(Ray cameraRay)
    {
		UpdatePosByRay (cameraRay);

		FixPosByItemBounds (cameraRay);

		CheckTreeAndSkEntity();

		if (mRayHitTerrain && !rootGameObject.activeSelf)
			rootGameObject.SetActive(true);

		if(null != itemBounds) itemBounds.activeState = itemBounds.activeState && !mTooFar;

		return mRayHitTerrain && !mTooFar && !mOverlaped && !mHasTree;
    }

	protected virtual void UpdatePosByRay(Ray cameraRay)
	{
		mRayHitTerrain = Phy.Raycast (cameraRay, out fhitInfo, 100f, layerMask, transform);
		if (mRayHitTerrain)
		{
			mHitPos = fhitInfo.point;
			mMinDis = fhitInfo.distance;
			float distance = Vector3.Distance(playerPos, mHitPos);
			mTooFar = DraggingDistance < distance;
//			if(!mTooFar)
			SetPos (mHitPos);
		}
	}

	protected virtual void FixPosByItemBounds(Ray cameraRay)
	{
		if (null == itemBounds)
		{
			mOverlaped = false;
			return;
		}

		CloseItemBounds ();

		if (null == mFindBounds)
			mFindBounds = new List<ItemDraggingBounds> ();

		List<ISceneObjAgent> sceneObjs = SceneMan.GetActiveSceneObjs (typeof(DragItemAgent), true);

		float curDis;
		bool rayHitBounds = false;

		Bounds selfBounds = itemBounds.worldBounds;
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

					if(!rayHitBounds && mRayHitTerrain && otherBounds.Intersects(selfBounds))
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

			Vector3 finalPos = mHitPos;
			if (hitLeft)
				finalPos = mHitPos + (otherBounds.min.x - selfBounds.max.x) * Vector3.right;
			else if(hitRight)
				finalPos = mHitPos + (otherBounds.max.x - selfBounds.min.x) * Vector3.right;
			else if(hitFront)
				finalPos = mHitPos + (otherBounds.min.z - selfBounds.max.z) * Vector3.forward;
			else if(hitBack)
				finalPos = mHitPos + (otherBounds.max.z - selfBounds.min.z) * Vector3.forward;
			else if(hitTop)
			{
				finalPos = mHitPos + (otherBounds.max.y - selfBounds.min.y) * Vector3.up;
				if(!canPutUp)
				{
					rootGameObject.transform.position = finalPos;
					mOverlaped = true;
					itemBounds.activeState = false;
					mFindBounds.Clear();
					return;
				}
			}
			else if(hitBottom)
			{
				finalPos = mHitPos + (otherBounds.min.y - selfBounds.max.y) * Vector3.up;
				rootGameObject.transform.position = finalPos;
				mOverlaped = true;
				itemBounds.activeState = false;
				mFindBounds.Clear();
				return;
			}

			if(!hitTop)
			{
				if(Physics.Raycast (finalPos + Vector3.up, Vector3.down, out fhitInfo, 10f, layerMask) && mAdsorbItemBounds.worldBounds.min.y - fhitInfo.point.y < itemBounds.selfBounds.max.y)
				{
					finalPos.y = fhitInfo.point.y;
				}
				else
				{
					rootGameObject.transform.position = finalPos;
					mOverlaped = true;
					itemBounds.activeState = false;
					mFindBounds.Clear();
					return;
				}
			}

			rootGameObject.transform.position = finalPos;

			selfBounds = itemBounds.worldBounds;
			
			for(int i = 0; i < mFindBounds.Count; ++i)
			{
				if(selfBounds.Intersects(mFindBounds[i].worldBounds))
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
		
		mFindBounds.Clear();

		mOverlaped = false;
		itemBounds.activeState = rayHitBounds || mRayHitTerrain;
	}

    public virtual bool OnCheckPutDown()
    {
        return true;
    }

    public virtual bool OnPutDown()
    {
		CloseItemBounds();
		ClearSubterrain();
		PlaySound();
        return true;
    }

	protected void CloseItemBounds()
	{
		if (null != mAdsorbItemBounds) 
		{
			mAdsorbItemBounds.showBounds = false;
			mAdsorbItemBounds = null;
		}
		
		if (null != mOverlapedItemBounds) 
		{
			mOverlapedItemBounds.showBounds = false;
			mOverlapedItemBounds = null;
		}
	}

	protected void ClearSubterrain()
	{
		if (null == itemBounds)
			return;

		if (PeGameMgr.IsMulti)
		{
			List<Vector3> grassPos = new List<Vector3>();
			Bounds selfBounds = itemBounds.worldBounds;
			Vector3 minPos = selfBounds.min - subTerrainClearRadius * Vector3.one;
			Vector3 maxPos = selfBounds.max + subTerrainClearRadius * Vector3.one;
			for (float _x = minPos.x; _x <= maxPos.x; ++_x)
			{
				for (float _y = minPos.y; _y <= maxPos.y; ++_y)
				{
					for (float _z = minPos.z; _z <= maxPos.z; ++_z)
					{
						Vector3 pos = new Vector3(Mathf.RoundToInt(_x), Mathf.RoundToInt(_y), Mathf.RoundToInt(_z));
						Vector3 outPos;
						if (PeGrassSystem.DeleteAtPos(pos, out outPos))
							grassPos.Add(outPos);
					}
				}
			}

			if (grassPos.Count == 0)
				return;

			byte[] grassData = PETools.Serialize.Export((w) =>
			{
				w.Write(grassPos.Count);
				for (int i = 0; i < grassPos.Count; i++)
					BufferHelper.Serialize(w, grassPos[i]);
			});

			if (null != PlayerNetwork.mainPlayer)
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearGrass, grassData);
		}
		else
		{
			Bounds selfBounds = itemBounds.worldBounds;
			Vector3 minPos = selfBounds.min - subTerrainClearRadius * Vector3.one;
			Vector3 maxPos = selfBounds.max + subTerrainClearRadius * Vector3.one;
			for (float _x = minPos.x; _x <= maxPos.x; ++_x)
				for (float _y = minPos.y; _y <= maxPos.y; ++_y)
					for (float _z = minPos.z; _z <= maxPos.z; ++_z)
						PeGrassSystem.DeleteAtPos(new Vector3(Mathf.RoundToInt(_x), Mathf.RoundToInt(_y), Mathf.RoundToInt(_z)));
		}
	}

	protected void PlaySound()
	{
		if(0 != itemDragging.itemObj.protoData.placeSoundID)
			AudioManager.instance.Create(transform.position, itemDragging.itemObj.protoData.placeSoundID);
	}

	protected void CheckTreeAndSkEntity()
	{
		if (null == itemBounds)
		{
			mHasTree = false;
			return;
		}
		Bounds bounds = itemBounds.worldBounds;
//		Physics.CapsuleCast
		Vector3 pos1 = bounds.min;
		Vector3 pos2 = bounds.min;
		pos2.z = bounds.max.z;
		float height = bounds.max.y - bounds.min.y;
		int checkNum = Mathf.FloorToInt(height / treeCheckHeight) + 2;
		float checkLength = bounds.max.x - bounds.min.x;

		for(int i = 0; i < checkNum; ++i)
		{

			if(Physics.CheckCapsule(pos1, pos2, 0.01f, treeAndEntityLayer) 
			   || Physics.CapsuleCast(pos1, pos2, 0.01f, Vector3.right, checkLength, treeAndEntityLayer))
			{
				mHasTree = true;
				itemBounds.activeState = false;
				return;
			}
			pos1.y += treeCheckHeight;
			pos2.y += treeCheckHeight;
		}
		
		mHasTree = false;
	}

    public virtual void OnCancel(){}

    public virtual void OnRotate()
    {
        transform.Rotate(new Vector3(0, 90, 0));
    }
    #endregion

    protected void RemoveFromBag()
    {
        Pathea.PackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.packageCmpt;

        //some item stack count > 1, seed eg.
        if (itemDragging.itemObj.stackCount > 1)
        {
            pkg.DestroyItem(itemDragging.itemObj, 1);
        }
        else
        {
            pkg.Remove(itemDragging.itemObj);
        }

        if (Pathea.PlayerPackageCmpt.LockStackCount
            && !ItemAsset.ItemMgr.IsCreationItem(itemDragging.itemObj.protoId))
        {
            Pathea.PlayerPackageCmpt playerPkg = pkg as Pathea.PlayerPackageCmpt;
            if (playerPkg != null)
            {
                playerPkg.package.Add(itemDragging.itemObj.protoId, 1);
            }
        }
    }
}