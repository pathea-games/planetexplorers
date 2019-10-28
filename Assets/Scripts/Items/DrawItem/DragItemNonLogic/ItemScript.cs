using ItemAsset;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    protected ItemObject mItemObj;

    protected NetworkInterface mNetlayer;

	public NetworkInterface netLayer{ get { return mNetlayer;} }

    public int id;

    public int itemObjectId
    {
        get
        {
            if (null != mItemObj)
            {
                return mItemObj.instanceId;
            }
            else
            {
                return Pathea.IdGenerator.Invalid;
            }
        }
    }

    public ItemObject MItemObj 
    {
        get 
        {
            return mItemObj;
        }
    }

    public GameObject rootGameObject
    {
        get
        {
            return gameObject;
        }
    }
	    
    public virtual void SetItemObject(ItemObject itemObj)
	{
        mItemObj = itemObj;
	}

    public void InitNetlayer(NetworkInterface netlayer)
    {
        mNetlayer = netlayer;
    }

    void EnableCollider()
    {
		Collider[] colls = rootGameObject.GetComponentsInChildren<Collider>();
        foreach (Collider coll in colls)
        {
            coll.enabled = true;

            if (coll is MeshCollider)
            {
                coll.isTrigger = false;
            }
        }
	}

    void DisableCollider()
    {
        Collider[] colls = rootGameObject.GetComponentsInChildren<Collider>();
        foreach (Collider coll in colls)
        {
            coll.enabled = false;
        }
    }

    public virtual void OnDeactivate()
    {
		//if (null != rootGameObject)
		//{
		//	Rigidbody rigb = rootGameObject.GetComponentInChildren<Rigidbody>();
		//	if (null != rigb)
		//	{
		//		rigb.detectCollisions = false;
		//		rigb.useGravity = false;
		//		rigb.constraints |= RigidbodyConstraints.FreezePositionY;
		//	}
		//}

      //  EnableCollider();
    }

    public virtual void OnActivate()
    {
		//if (null != rootGameObject)
		//{
		//	Rigidbody rigb = rootGameObject.GetComponentInChildren<Rigidbody>();
		//	if (null != rigb)
		//	{
		//		rigb.detectCollisions = true;
		//		rigb.useGravity = true;
		//		rigb.constraints &= ~RigidbodyConstraints.FreezePositionY;
		//	}
		//}

       // EnableCollider();
    }

    public virtual void OnDestruct()
    {

    }

    public virtual void OnConstruct()
    {

    }
}