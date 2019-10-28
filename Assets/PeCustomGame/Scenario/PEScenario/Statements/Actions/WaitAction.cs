using UnityEngine;
using ScenarioRTL;
using System.IO;

namespace PeCustom
{
    [Statement("WAIT")]
	public class WaitAction : ScenarioRTL.Action
    {
        // 在此列举参数
		float amt = 0;

        // 在此初始化参数
        protected override void OnCreate()
        {
			amt = Utility.ToSingle(missionVars, parameters["amount"]);
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
		float cur = 0;
        public override bool Logic()
        {
			cur += Time.deltaTime;
			if (cur >= amt)
			{
				cur = amt;
				return true;
			}
			return false;
        }
        
        // 恢复动作状态
        public override void RestoreState(BinaryReader r)
        {
			cur = r.ReadSingle();
        }
        // 存储动作状态
        public override void StoreState(BinaryWriter w)
        {
			w.Write(cur);
        }
    }
}
