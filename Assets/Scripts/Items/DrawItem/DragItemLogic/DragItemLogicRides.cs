//using UnityEngine;
//using System.Collections;

//public class DragItemLogicRides : DragItemLogic
//{

//    ItemScript_Rides mRides = null;

//    public override void OnConstruct()
//    {
//        base.OnConstruct();

//        if (itemDrag != null)
//        {
//            GameObject obj = itemDrag.CreateViewGameObject(null);
//            if(obj != null)
//            {
//                obj.transform.parent = this.transform;
//                PETools.PEUtil.ResetTransform(obj.transform);

//                mRides = obj.GetComponent<ItemScript_Rides>();
//            }
//        }

//        if (mRides != null)
//        {
//            mRides.InitNetlayer(mNetlayer);
//            mRides.SetItemObject(itemDrag.itemObj);
//            mRides.id = id;

//            mRides.OnConstruct();
//        }
//    }

//    public override void OnDestruct()
//    {
//        base.OnDestruct();

//        if (mRides != null)
//        {
//            mRides.OnDestruct();
//        }
//    }

//    public override void OnActivate()
//    {
//        base.OnActivate();

//        if (mRides != null)
//        {
//            mRides.OnActivate();
//        }
//    }

//    public override void OnDeactivate()
//    {
//        base.OnDeactivate();

//        if (mRides != null)
//        {
//            mRides.OnDeactivate();
////            Object.Destroy(mBedView.gameObject);
//        }
//    }
//}
