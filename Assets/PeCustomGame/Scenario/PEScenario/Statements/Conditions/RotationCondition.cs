using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("ROTATION")]
    public class RotationCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
		EAxis axis;
		OBJECT obj;
		DIRRANGE range;

        // 在此初始化参数
        protected override void OnCreate()
        {
			axis = (EAxis)Utility.ToEnumInt(parameters["axis"]);
			obj = Utility.ToObject(parameters["object"]);
			range = Utility.ToDirRange(missionVars, parameters["range"]);
        }
        
        // 判断条件
        public override bool? Check()
        {
			Transform trans = PeScenarioUtility.GetObjectTransform(obj);
			if (trans == null)
				return range.type == DIRRANGE.DIRRANGETYPE.Anydirection;

			switch (axis)
			{
			case EAxis.Left: return range.Contains(-trans.right);
			case EAxis.Right: return range.Contains(trans.right);
			case EAxis.Down: return range.Contains(-trans.up);
			case EAxis.Up: return range.Contains(trans.up);
			case EAxis.Backward: return range.Contains(-trans.forward);
			case EAxis.Forward: return range.Contains(trans.forward);
			}
			return false;
        }

        protected override void SendReq()
        {
            byte[] data = BufferHelper.Export(w =>
            {
                w.Write(reqId);
                BufferHelper.Serialize(w, obj);
                BufferHelper.Serialize(w, range);
                w.Write((byte)axis);
            });

            PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckRot, data);
        }
    }
}
