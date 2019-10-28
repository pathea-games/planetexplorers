using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("PLAY ANIMATION", true)]
    public class PlayAnimAction : ScenarioRTL.Action
    {
        // 在此列举参数
		string name;
		OBJECT obj;	// ENTITY

        public static bool playerAniming = false;
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			name = Utility.ToVarname(parameters["name"]);
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
                    w.Write(name);
                });

                PlayerNetwork.RequestServer(EPacketType.PT_Custom_StartAnimation, data);
            }
            else
            {
                Pathea.PeEntity entity = PeScenarioUtility.GetEntity(obj);
                if (null == entity)
                    return true;

                if (obj.isCurrentPlayer)
                {
                    playerAniming = true;
                    entity.animCmpt.AnimEvtString += OnAnimString;
                }

                string type = name.Split('_')[name.Split('_').Length - 1];
                if (type == "Once")
                    entity.animCmpt.SetTrigger(name);
                else if (type == "Muti")
                    entity.animCmpt.SetBool(name, true);
            }

            return true;
        }

        void OnAnimString(string param)
        {
            if (param == "OnCustomAniEnd")
            {
                if (obj.isCurrentPlayer)
                    playerAniming = false;
            }
        }
    }
}
