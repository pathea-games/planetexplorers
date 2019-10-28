using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("CREATE OBJECT", true)]
    public class CreateObjectAction : ScenarioRTL.Action
    {
		// 在此列举参数
		int amt;
		OBJECT obj;	// PROTOTYPE
		RANGE range;

		// 在此初始化参数
		protected override void OnCreate()
		{
			amt = Utility.ToInt(missionVars, parameters["amount"]);
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
                    w.Write(amt);
                });

                PlayerNetwork.RequestServer(EPacketType.PT_Custom_CreateObject, data);
            }
            else
            {
                if (!PeScenarioUtility.CreateObjects(amt, obj, range))
                {
                    Debug.LogWarning("Create objects faild!");
                }
            }
            return true;
        }
    }
}
