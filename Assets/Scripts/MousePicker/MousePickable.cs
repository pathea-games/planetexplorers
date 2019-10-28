using System.Collections.Generic;
using UnityEngine;
using Pathea;

public class MousePickable : MonoBehaviour, MousePicker.IPickable
{
	public float operateDistance = 19f;
    public class RMouseClickEvent : PeEvent.EventArg
    {
        public MousePickable mousePickable;
    }

    PeEvent.Event<RMouseClickEvent> mEventor = new PeEvent.Event<RMouseClickEvent>();

    public PeEvent.Event<RMouseClickEvent> eventor
    {
        get
        {
            return mEventor;
        }
    }

    MousePicker.EPriority mPriority = MousePicker.EPriority.Level0;

    [SerializeField]
    protected List<Collider> mCollider = new List<Collider>(1);
	PeTrans _peTrans = null;
    public void ClearCollider()
    {
        mCollider.Clear();
    }	
	public void CollectColliders()
	{
		_peTrans = GetComponent<PeTrans> ();
		ClearCollider ();
		Collider[] cols = GetComponentsInChildren<Collider>(true);
		foreach (Collider col in cols)
		{
			//if (col.enabled)
			//{
				mCollider.Add(col);
			//}
		}
	}

    public MousePicker.EPriority priority
    {
        get
        {
            return mPriority;
        }
        set
        {
            mPriority = value;
        }
    }

	public bool DistanceInRange(Vector3 pos, float dist)
    {
		float fScope = dist * 1.25f;
		Vector3 vDist = _peTrans == null ? (pos - transform.position) : (pos - _peTrans.position);
		if (vDist.x > fScope || vDist.x < -fScope || vDist.y > fScope || vDist.y < -fScope || vDist.z > fScope || vDist.z < -fScope)	// rough filter
			return false;

		float sqrDist = dist * dist;
		if (_peTrans != null) {
			Bounds bound = _peTrans.bound;
			bound.center = _peTrans.trans.TransformPoint(bound.center);
			return sqrDist >= bound.SqrDistance (pos);
		} else {
			for (int i = 0; i < mCollider.Count; i++) {
				Collider col = mCollider [i];
				if (col != null) {
					if (sqrDist >= col.bounds.SqrDistance (pos)) {
						return true;
					}
				}
			}
		}
	    return false;
    }

    #region subclass override
    void Start()
    {
        MousePicker.Instance.Add(this);
		OnStart();
    }

	protected virtual void OnStart() { }

    protected virtual void OnDestroy()
    {
		ClearCollider ();
        MousePicker.Instance.Remove(this);
    }

    protected virtual bool CheckPick(Ray ray, out float dis)
    {
		if (null != MainPlayer.Instance.entity && DistanceInRange (MainPlayer.Instance.entity.position, operateDistance)) 
		{
			RaycastHit hitInfo;
			for (int i = 0; i < mCollider.Count; i++) 
			{
				Collider col = mCollider [i];
				if (null != col && col.Raycast (ray, out hitInfo, 100f)) 
				{
					dis = hitInfo.distance;
					return true;
				}
			}
		}
        dis = 0;
        return false;
    }

    protected virtual string tipsText
    {
        get
        {
            return null;
        }
    }

    protected virtual void CheckOperate()
    {
        if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu))
        {
            RMouseClickEvent e = new RMouseClickEvent();
            e.mousePickable = this;
            eventor.Dispatch(e);
        }
    }

    protected virtual void SetPickState(bool isOver)
    {
        OutlineObject outLine = GetComponent<OutlineObject>();
        if (null == outLine)
        {
            return;
        }

        if (isOver)
        {    
            outLine.color = new Color(0, 0.5f, 1f, 1f);
        }
        else
        {
            Destroy(outLine);
        }
    }

    #endregion

    #region MousePicker.IPickable
    float MousePicker.IPickable.CheckPick(Ray ray)
    {
        float dis = 0;
        if (CheckPick(ray, out dis))
        {
            return dis;
        }

        return float.MaxValue;
    }

    void MousePicker.IPickable.SetPickState(bool isOver)
    {
        SetPickState(isOver);
    }

    string MousePicker.IPickable.tips
    {
        get
        {
            return tipsText;
        }
    }

    MousePicker.EPriority MousePicker.IPickable.priority
    {
        get
        {
            return priority;
        }
    }

    void MousePicker.IPickable.PickObj()
    {
        CheckOperate();
    }

    float MousePicker.IPickable.delayTime
    {
        get { return 0f; }
    }

    #endregion
}