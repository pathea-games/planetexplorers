using UnityEngine;
using ScenarioRTL;
using Pathea;

namespace PeCustom
{
    [Statement("MODIFY STAT", true)]
    public class ModifyStatAction : ScenarioRTL.Action
    {
		// 在此列举参数
		OBJECT obj;   // ENTITY
		AttribType stat;
		EFunc func;
		float amt;

		// 在此初始化参数
		protected override void OnCreate()
		{
			obj = Utility.ToObject(parameters["object"]);
			stat = (AttribType)Utility.ToEnumInt(parameters["stat"]);
			func = Utility.ToFunc(parameters["func"]);
			amt = Utility.ToSingle(missionVars, parameters["amount"]);
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
                    w.Write(amt);
                    w.Write((byte)stat);
                    w.Write((byte)func);
                });

                PlayerNetwork.RequestServer(EPacketType.PT_Custom_ModifyStat, data);
            }
            else
            {
                PeEntity entity = PeScenarioUtility.GetEntity(obj);
                if (entity != null && entity.skEntity != null)
                {
                    float prev = entity.skEntity.GetAttribute((int)stat);
                    float value = Utility.Function(prev, amt, func);

                    float prev_hp = PeCreature.Instance.mainPlayer.HPPercent;
                    entity.SetAttribute(stat, value);
                    float curr_hp = PeCreature.Instance.mainPlayer.HPPercent;
                    float hp_drop = Mathf.Clamp01(prev_hp - curr_hp) * PeCreature.Instance.mainPlayer.GetAttribute(AttribType.HpMax);

                    // 如果是玩家掉血，则播放摄像机特效
                    if (hp_drop > 0)
                        PeCameraImageEffect.PlayHitEffect(hp_drop);
                }
            }
            return true;
        }
    }
}
