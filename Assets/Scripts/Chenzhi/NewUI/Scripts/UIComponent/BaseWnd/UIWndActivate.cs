using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIWndActivate : MonoBehaviour 
{
	public BoxCollider mCollider;
	[SerializeField] Vector3 mBgLocalPos;
	public Vector3 BgLocalPos
	{
		get
		{

			return mBgLocalPos;
		}
	}
	[SerializeField] UnityEvent OnActive;
	[SerializeField] UnityEvent OnDeactive;

	void Start()
	{
		if (mCollider == null)
			return;
        mCollider.center = Vector3.zero;
        mCollider.size = Vector3.one;
		mBgLocalPos = mCollider.transform.localPosition;
		Transform _parent = mCollider.transform.parent;
		do{
			mBgLocalPos += _parent.localPosition;
			_parent = _parent.parent;
		}while (_parent != this.transform && _parent != null);
	}


	public void Activate ()
	{
		OnActive.Invoke();
		if (mCollider != null) mCollider.center = new Vector3(0,0,0);
	}

	public void Deactivate ()
	{
		OnDeactive.Invoke();
		if (mCollider != null) mCollider.center = new Vector3(0,0,-13.5f);
	}

}
