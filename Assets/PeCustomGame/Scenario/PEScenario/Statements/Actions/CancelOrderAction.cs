using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("CANCEL ORDER", true)]
    public class CancelOrderAction : ScenarioRTL.Action
    {
        // 在此列举参数
        OBJECT obj;	// NPOBJECT

        // 在此初始化参数
        protected override void OnCreate()
        {
            obj = Utility.ToObject(parameters["object"]);
        }

        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
            if (Pathea.PeGameMgr.IsMulti)
            {
                byte[] data = PETools.Serialize.Export(w =>
                {
                    BufferHelper.Serialize(w, obj);
                });

                PlayerNetwork.RequestServer(EPacketType.PT_Custom_CancelOrder, data);
            }
            else
            {
                Pathea.PeEntity entity = PeScenarioUtility.GetEntity(obj);
                if (entity != null && entity.requestCmpt != null)
                {
                    if (entity.requestCmpt.Contains(Pathea.EReqType.Dialogue))
                        entity.requestCmpt.RemoveRequest(Pathea.EReqType.Dialogue);
                    if (entity.requestCmpt.Contains(Pathea.EReqType.MoveToPoint))
                        entity.requestCmpt.RemoveRequest(Pathea.EReqType.MoveToPoint);
                    if (entity.requestCmpt.Contains(Pathea.EReqType.FollowTarget))
                        entity.requestCmpt.RemoveRequest(Pathea.EReqType.FollowTarget);
                }
            }

            return true;
        }
    }
}
