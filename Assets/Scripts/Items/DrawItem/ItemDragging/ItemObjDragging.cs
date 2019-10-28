using UnityEngine;


public class ItemObjDragging:DraggingMgr.IDragable
{
    protected ItemDraggingBase dragBase;
    ItemAsset.Drag mDrag;

	public ItemDraggingBase DragBase
	{
		get{return dragBase;}
	}

    public ItemObjDragging(ItemAsset.Drag drag)
    {
        mDrag = drag;
    }

    public ItemAsset.Drag GetItemDrag()
    {
        return mDrag;
    }

    public int GetItemProtoId()
    {
        if (mDrag.itemObj.protoId > 100000000)
		{
			if (StroyManager.Instance != null)
            	return StroyManager.Instance.ItemClassIdtoProtoId(mDrag.itemObj.protoData.itemClassId);
			else
				return mDrag.itemObj.protoId;
		}
        else
		{
            return mDrag.itemObj.protoId;
		}
    }

    public int GetItemInstanceID() 
    {
        return mDrag.itemObj.instanceId;
    }

    public Vector3 GetPos()
    {
        if (null == dragBase)
        {
            return Vector3.zero;
        }
        return dragBase.transform.position;
    }



    ItemDraggingBase CreateGameObj()
    {
        ItemAsset.Drag drag = GetItemDrag();

        if (drag == null)
        {
            return null;
        }

        GameObject go = drag.CreateDraggingGameObject(null);
        if (null == go)
        {
            return null;
        }

        ItemDraggingBase dragBase = go.GetComponent<ItemDraggingBase>();
        if (null == dragBase)
        {
            dragBase = go.AddComponent<ItemDraggingArticle>();
            //Debug.LogError("drag item has no ItemDraggingBase: " + go.name);
            //Object.Destroy(go);
            //return null;
        }

        dragBase.itemDragging = drag;

        go.SetActive(true);

        return dragBase;
    }

    void Destroy()
    {
        if (null != dragBase)
        {
            Object.Destroy(dragBase.gameObject);
        }
    }

    void DraggingMgr.IDragable.OnDragOut()
    {
        dragBase = CreateGameObj();
        if (null == dragBase)
        {
            return;
        }

        dragBase.OnDragOut();
    }

    bool DraggingMgr.IDragable.OnDragging(Ray cameraRay)
    {
        if (null == dragBase)
        {
            return false;
        }

        return dragBase.OnDragging(cameraRay);
    }

    bool DraggingMgr.IDragable.OnCheckPutDown()
    {
        if (null == dragBase)
        {
            return false;
        }

        return dragBase.OnCheckPutDown();
    }

    void DraggingMgr.IDragable.OnPutDown()
    {
        if (null == dragBase)
        {
            return;
        }

        if (dragBase.OnPutDown())
        {
            Destroy();
        }
    }

    void DraggingMgr.IDragable.OnCancel()
    {
        if (null == dragBase)
        {
            return;
        }

        dragBase.OnCancel();
        Destroy();
    }

    void DraggingMgr.IDragable.OnRotate()
    {
        if (null == dragBase)
        {
            return;
        }

        dragBase.OnRotate();
    }
}