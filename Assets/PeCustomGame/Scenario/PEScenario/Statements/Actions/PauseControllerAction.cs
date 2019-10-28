using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("PAUSE CONTROLLER", true)]
    public class PauseControllerAction : ScenarioRTL.Action
    {
        // 在此列举参数
        
        // 在此初始化参数
        protected override void OnCreate()
        {
        
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
            PeInput.enable = false;
            return true;
        }
    }
}
