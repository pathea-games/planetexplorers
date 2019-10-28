using UnityEngine;
using ScenarioRTL;
using Pathea;

namespace PeCustom
{
    [Statement("ENABLE SPAWN", true)]
    public class EnableSpawnAction : ScenarioRTL.Action
    {
		// 在此列举参数
		OBJECT spawn;	// SPAWN

		// 在此初始化参数
		protected override void OnCreate()
		{
			spawn = Utility.ToObject(parameters["spawnpoint"]);
		}
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
            if (Pathea.PeGameMgr.IsMulti)
            {
                byte[] data = PETools.Serialize.Export(w => { BufferHelper.Serialize(w, spawn); });
                PlayerNetwork.RequestServer(EPacketType.PT_Custom_EnableSpawn, data);
            }
            else
            {
                if (!PeScenarioUtility.EnableSpawnPoint(spawn, true))
                {
                    Debug.LogWarning("Enable spawn point error");
                }
            }

            return true;
        }
    }
}
