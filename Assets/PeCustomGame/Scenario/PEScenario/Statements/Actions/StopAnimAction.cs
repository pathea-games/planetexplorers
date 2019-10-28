using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("STOP ANIMATION", true)]
    public class StopAnimAction : ScenarioRTL.Action
    {
		// 在此列举参数
		OBJECT obj;	// ENTITY

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

                PlayerNetwork.RequestServer(EPacketType.PT_Custom_StopAnimation, data);
            }
            else
            {
                Pathea.PeEntity entity = PeScenarioUtility.GetEntity(obj);
                if (null == entity)
                    return true;
                //TODO:把ParametersBool设置为false(等待动作表)
                entity.animCmpt.SetTrigger("Custom_ResetAni");
            }

            return true;
        }
    }
}
