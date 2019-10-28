using UnityEngine;
using System.Collections;

namespace Pathea
{
    public class PeMousePickCmpt : PeCmpt, IPeMsg
    {
        public MousePickablePeEntity mousePick
        {
            get
            {
                return mousePickable as MousePickablePeEntity;
            }
        }

        MousePickable mMousePick;
        MousePickable mousePickable
        {
            get
            {
                if (null == mMousePick)
                {
                    mMousePick = Entity.GetGameObject().GetComponent<MousePickable>();
                    if (mMousePick == null)
                    {
						CommonCmpt common = Entity.GetCmpt<CommonCmpt>();
						if(null == common || common.entityProto.proto == EEntityProto.Npc || common.entityProto.proto == EEntityProto.RandomNpc)
							mMousePick = Entity.GetGameObject().AddComponent<MousePickableNPC>();
						else
	                        mMousePick = Entity.GetGameObject().AddComponent<MousePickablePeEntity>();
                    }
                }

                return mMousePick;
            }
        }

        void IPeMsg.OnMsg(EMsg msg, params object[] args)
        {
            if (EMsg.View_Prefab_Build == msg)
            {
				mousePickable.CollectColliders();
            }
            else if (EMsg.View_Prefab_Destroy == msg)
            {
                mousePickable.ClearCollider();
            }
        }
    }
}