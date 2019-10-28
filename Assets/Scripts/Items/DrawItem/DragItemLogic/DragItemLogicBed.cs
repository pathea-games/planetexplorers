using UnityEngine;
using System.Collections;

public class DragItemLogicBed : DragItemLogic//, ISaveDataInScene
{
    //void Start ()
    //{
    //    Debug.Log("<color=aqua>DragItemLogicTest started</color>");
    //}


    //void ISaveDataInScene.ImportData(byte[] data)
    //{
    //    Debug.Log("<color=aqua>DragItemLogicTest import data</color>");
    //}

    //byte[] ISaveDataInScene.ExportData()
    //{
    //    Debug.Log("<color=aqua>DragItemLogicTest export data</color>");
    //    return null;
    //}

    ItemScript_Bed mBedView = null;

    public override void OnConstruct()
    {
        base.OnConstruct();

        if (itemDrag != null)
        {
            GameObject obj = itemDrag.CreateViewGameObject(null);
            if(obj != null)
            {
                obj.transform.parent = this.transform;
                PETools.PEUtil.ResetTransform(obj.transform);

                mBedView = obj.GetComponent<ItemScript_Bed>();
            }
        }

        if (mBedView != null)
        {
            mBedView.InitNetlayer(mNetlayer);
            mBedView.SetItemObject(itemDrag.itemObj);
            mBedView.id = id;

            mBedView.OnConstruct();
        }
    }

    public override void OnDestruct()
    {
        base.OnDestruct();

        if (mBedView != null)
        {
            mBedView.OnDestruct();
        }
    }

    public override void OnActivate()
    {
        base.OnActivate();

        if (mBedView != null)
        {
            mBedView.OnActivate();
        }
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        if (mBedView != null)
        {
            mBedView.OnDeactivate();
//            Object.Destroy(mBedView.gameObject);
        }
    }
}
