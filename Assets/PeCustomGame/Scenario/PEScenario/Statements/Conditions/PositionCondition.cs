using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("POSITION")]
    public class PositionCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
		OBJECT obj;
		RANGE range;

        // 在此初始化参数
        protected override void OnCreate()
        {
			obj = Utility.ToObject(parameters["object"]);
			range = Utility.ToRange(missionVars, parameters["range"]);
        }
        
        // 判断条件
        public override bool? Check()
        {
			Transform trans = PeScenarioUtility.GetObjectTransform(obj);
			if (trans == null)
				return range.type == RANGE.RANGETYPE.Anywhere;
			
			return range.Contains(trans.position);
        }

        protected override void SendReq()
        {
            byte[] data = BufferHelper.Export(w =>
            {
                w.Write(reqId);
                BufferHelper.Serialize(w, obj);
                BufferHelper.Serialize(w, range);
            });

            PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckPos, data);
        }
    }
}
