using UnityEngine;

namespace Pathea
{
    public class DragEntityLodCmpt : PeCmpt
    {
		BiologyViewCmpt mView;
        MotionMgrCmpt mMotionMgr;

        public override void Start()
        {
			base.Start ();
			mView = Entity.biologyViewCmpt;
			mMotionMgr = Entity.motionMgr;          
        }

        public void Activate()
        {
            mView.Build();

            if (mMotionMgr != null)
            {
                mMotionMgr.FreezePhySteateForSystem(false);
            }

            Entity.SendMsg(EMsg.Lod_Collider_Created);
        }

        public void Deactivate()
        {
            //UnityEngine.Debug.Log("collider under " + Entity + " will be destroy");
            if (mMotionMgr != null)
            {
                mMotionMgr.FreezePhySteateForSystem(true);
            }

            mView.Destroy();

            Entity.SendMsg(EMsg.Lod_Collider_Destroying);
        }
    }
}