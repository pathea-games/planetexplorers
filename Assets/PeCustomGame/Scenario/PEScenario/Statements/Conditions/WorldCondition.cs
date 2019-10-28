using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("WORLD")]
    public class WorldCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
        OBJECT player;  // PLAYER
        int worldId;
        
        // 在此初始化参数
        protected override void OnCreate()
        {
            player = Utility.ToObject(parameters["player"]);
            worldId = Utility.ToInt(missionVars, parameters["id"]);
        }
        
        // 判断条件
        public override bool? Check()
        {
            Pathea.PeEntity entity = PeScenarioUtility.GetEntity(player);
            if (entity != null && Pathea.CustomGameData.Mgr.Instance.curGameData.WorldIndex == worldId)
                return true;
            return false;
        }

        protected override void SendReq()
        {
            byte[] data = BufferHelper.Export(w =>
            {
                w.Write(reqId);
                BufferHelper.Serialize(w, player);
                w.Write(worldId);
            });

            PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckWorld, data);
        }
    }
}
