using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MousePicker : Pathea.MonoLikeSingleton<MousePicker>
{
    const int MaxPickDis = 100;
    static int ObstacleLayer = (1 << Pathea.Layer.VFVoxelTerrain) | (1 << Pathea.Layer.SceneStatic);

    public enum EPriority
    {
        Level0 = 0,
        Level1,
        Level2,
        Level3,
        Max
    }

    public interface IPickable
    {
        float CheckPick(Ray ray);

        void SetPickState(bool isOver);

        void PickObj();

        string tips { get; }

        MousePicker.EPriority priority { get; }

        float delayTime { get; }
    }

	List<IPickable> mPickableList = new List<IPickable>();
	
	IPickable mPickedObj;
    float mHoveredTime = 0f;
    bool mPickedObjChanged = true;

    bool isOverHoveredTime
    {
        get
        {
            if(null == mPickedObj)
            {
                return false;
            }

            return mHoveredTime > mPickedObj.delayTime;
        }
    }
    public override void Update() 
	{
        if (!enable || Pathea.PeGameMgr.gamePause)
        {
            return;
        }

        mHoveredTime += Time.deltaTime;

		UpdateOpObject();

        if (isOverHoveredTime)
        {
            if (mPickedObjChanged)
            {
                HoverPicked(true);
                mPickedObjChanged = false;
            }

            ActiveFunction();
        }
	}

    void UpdateOpObject()
	{
        IPickable pickable = CheckPickable();

        if (pickable != mPickedObj)
        {
            SetCurPickable(pickable);
        }
	}

    void HoverPicked(bool value)
    {
        if (null != mPickedObj /*&& "null" != mPickedObj.ToString()*/)
        {
            mPickedObj.SetPickState(value);
        }

        UpdateTis(value);
    }

    public void UpdateTis(bool value = true)
    {
        if (value && null != mPickedObj)
        {
            MouseActionWnd_N.Instance.SetText(mPickedObj.tips);
        }
        else
        {
            MouseActionWnd_N.Instance.Clear();
        }
    }

    void SetCurPickable(IPickable pickable)
    {
        HoverPicked(false);

        mPickedObj = pickable;
        mHoveredTime = 0f;
        mPickedObjChanged = true;
    }

    IPickable CheckPickable()
    {
        if (null != UICamera.hoveredObject)
        {
            return null;
        }

        Ray ray = PeCamera.mouseRay;

        IPickable curPickable = null;
        EPriority curPriority = MousePicker.EPriority.Level0;
        float maxDistance = GetMaxDistance(ray);
        float curDistance = maxDistance;

        for (int i = 0; i < mPickableList.Count; i++) {
			IPickable pickable = mPickableList [i];
			if (null == pickable) {
				continue;
			}
			float dis = pickable.CheckPick (ray);
			if (dis > maxDistance) {
				continue;
			}
			if (pickable.priority > curPriority) {
				curPriority = pickable.priority;
				curDistance = dis;
				curPickable = pickable;
			}
			else if (pickable.priority == curPriority) {
				if (curDistance > dis) {
					curDistance = dis;
					curPickable = pickable;
				}
			}
		}
        return curPickable;
    }

    private static float GetMaxDistance(Ray ray)
    {
        float maxDistance = MaxPickDis;

        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, MaxPickDis, ObstacleLayer))
        {
            if (hitInfo.distance < maxDistance/* && null == hitInfo.transform.GetComponent<ItemScript>()*/)
            {
                maxDistance = hitInfo.distance;
            }
        }

        //some pickable item's layer is SceneStatic, like railway station
        return maxDistance + 10f;
    }

	void ActiveFunction()
	{
        if (null == mPickedObj)
        {
            return;
        }

        mPickedObj.PickObj();
	}
	
	public void Add(IPickable opObj)
	{
	    mPickableList.Add(opObj);
	}
	
	public bool Remove(IPickable p)
	{
        if (null == p)
        {
            return false;
        }

        if (p == mPickedObj)
        {
            if (null != MouseActionWnd_N.Instance)
            {
                MouseActionWnd_N.Instance.Clear();
            }
            mPickedObj = null;
        }

        return mPickableList.Remove(p);
	}

    public IPickable curPickObj
    {
        get
        {
            return mPickedObj;
        }
    }

    bool mEnable = true;
    public bool enable
    {
        get
        {
            return mEnable;
        }

        set
        {
            if (!value)
            {
                SetCurPickable(null);
                UpdateTis(false);
            }
            mEnable = value;
        }
    }
}
