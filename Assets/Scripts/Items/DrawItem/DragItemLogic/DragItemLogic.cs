using UnityEngine;
using System.Collections;

public class DragItemLogic : MonoBehaviour
{
    public int id;
	public ItemAsset.Drag itemDrag;
	public NetworkInterface mNetlayer;

    public void SetItemDrag(ItemAsset.Drag itemDrag)
    {
        this.itemDrag = itemDrag;
    }

    public void InitNetlayer(NetworkInterface netlayer)
    {
        mNetlayer = netlayer;
    }

    public virtual void OnActivate()
    {
    }

    public virtual void OnDeactivate()
    {
    }

    public virtual void OnConstruct()
    {
    }

    public virtual void OnDestruct()
    {
    }
}