using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("REMOVE OBJECT", true)]
    public class RemoveObjectAction : ScenarioRTL.Action
    {
		// 在此列举参数
		OBJECT obj;	// PROTOTYPE
		RANGE range;

		// 在此初始化参数
		protected override void OnCreate()
		{
			obj = Utility.ToObject(parameters["object"]);
			range = Utility.ToRange(missionVars, parameters["range"]);
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
                    BufferHelper.Serialize(w, range);
                });

                PlayerNetwork.RequestServer(EPacketType.PT_Custom_RemoveObject, data);
            }
            else
            {
                if (PeScenarioUtility.RemoveObjects(obj, range))
                {
                    Debug.LogWarning("Remove objects faild!");
                }
            }

            return true;
        }
    }
}
