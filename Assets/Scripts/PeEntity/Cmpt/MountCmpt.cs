using Pathea.Operate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathea
{
    public class MountCmpt : PeCmpt, IPeMsg
    {
        public PeEntity Mount { get; private set; }

        #region msg
        public void OnMsg(EMsg msg, params object[] args)
        {
            switch (msg)
            {
                case EMsg.View_Prefab_Build:
                    if (PeGameMgr.IsSingle)
                        RelationshipDataMgr.RecoverRelationship(PeCreature.Instance.mainPlayerId);
                    else
                    {
                        if (Mount)
                        {
                            if (Mount.hasView)
                                RideMount();
                            else
                            {
                                Mount.biologyViewCmpt.Build();
                                StartCoroutine("RideMountIterator");
                            }
                        }
                    }
                    break;
            }
        }

        #endregion

        #region override methods

        public override void OnUpdate()
        {
            if (PeGameMgr.IsMulti && !Entity.IsMainPlayer)
            {
                //lz-2017.02.17 其他玩家离我太远了，模型删除，行为结束后，这里继续更新位置，保证位置正常
                if (Mount && Mount.peTrans && Entity && !Entity.hasView && Entity.peTrans)
                {
                    Entity.peTrans.position = Mount.peTrans.position;
                }
            }
        }

        #endregion

        #region pivate methods

        private IEnumerator RideMountIterator()
        {
            if (Mount && !Mount.hasView)
                yield return null;
            RideMount();
        }

        private void RideMount()
        {
            if (Mount && Mount.hasView && Mount.biologyViewCmpt && Mount.biologyViewCmpt.biologyViewRoot && Mount.biologyViewCmpt.biologyViewRoot.modelController
                && Entity && Entity.operateCmpt)
            {
                MousePickRides rides = Mount.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<MousePickRides>();
                if (rides)
                {
                    rides.ExecRide(Entity);
                }
            }
        }

        #endregion

        #region public methods

        public void SetMount(PeEntity mount)
        {
            if (null != mount)
            {
                Mount = mount;
                Mount.SetMount(true);
                if (!PeGameMgr.IsMulti)
                    RelationshipDataMgr.AddRelationship(Entity, Mount);
            }
        }

        public void DelMount()
        {
            if (null != Mount)
            {
                if (!PeGameMgr.IsMulti)
                    RelationshipDataMgr.RemoveRalationship(Entity.Id, Mount.ProtoID);
                Mount.SetMount(false);
                Mount = null;
            }
        }

        #endregion

    }
}
