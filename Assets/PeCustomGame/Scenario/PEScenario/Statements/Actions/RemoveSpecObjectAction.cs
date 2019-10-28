using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("REMOVE SPECIFIC OBJECT", true)]
    public class RemoveSpecObjectAction : ScenarioRTL.Action
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

                PlayerNetwork.RequestServer(EPacketType.PT_Custom_RemoveSpecObject, data);
            }
            else
            {
                if (PeScenarioUtility.RemoveObject(obj))
                {
                    Debug.LogWarning("Remove Spec Object Error!");
                }
            }

            return true;
        }
    }
}
