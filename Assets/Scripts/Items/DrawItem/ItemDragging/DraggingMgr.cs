using UnityEngine;

using Pathea.PeEntityExtNet;

public class DraggingMgr : Pathea.MonoLikeSingleton<DraggingMgr>
{
    public interface IDragable
    {
        void OnDragOut();
        bool OnDragging(Ray cameraRay);
        bool OnCheckPutDown();
        void OnPutDown();
        void OnCancel();
        void OnRotate();
    }

    public class EventArg : PeEvent.EventArg
    {
        public IDragable dragable;
    }

    PeEvent.Event<EventArg> mEventor = new PeEvent.Event<EventArg>();

    public PeEvent.Event<EventArg> eventor
    {
        get
        {
            return mEventor;
        }
    }

    IDragable mDragable;
    bool mPutDownEnable;
	Vector3 mLastMousePos;
	Vector3 mLastCameraPos;

	public IDragable Dragable{get {return mDragable;}}

    void Clear()
    {
        mDragable = null;
        mPutDownEnable = false;
    }

    public bool IsDragging()
    {
        return mDragable != null;
    }

    public bool Begin(IDragable dragable)
    {
        if (null == dragable)
        {
            return false;
        }

        mDragable = dragable;

        mDragable.OnDragOut();

        return true;
    }

    public bool End()
    {
        if (null != mDragable)
        {
            if (mDragable.OnCheckPutDown() && mPutDownEnable)
            {
                eventor.Dispatch(new EventArg() { dragable = mDragable }, this);

                mDragable.OnPutDown();

                Clear();
                return true;
            }
            else
            {
                Cancel();
                return false;
            }
        }

        Clear();

        return false;
    }

    public void Cancel()
    {
        if (null != mDragable)
        {
            mDragable.OnCancel();
        }

        Clear();
    }

    public void Rotate()
    {
        if (null == mDragable)
        {
            return;
        }

        mDragable.OnRotate();
    }

    public void UpdateRay()
    {
        if (null == mDragable || mDragable.Equals(null))
        {
            return;
        }
		if(mLastMousePos != Input.mousePosition || Vector3.SqrMagnitude(mLastCameraPos - Camera.main.transform.position) > 1f)
		{
			mLastMousePos = Input.mousePosition;
			mLastCameraPos = Camera.main.transform.position;
	        mPutDownEnable = mDragable.OnDragging(PeCamera.mouseRay);
		}
    }
} 