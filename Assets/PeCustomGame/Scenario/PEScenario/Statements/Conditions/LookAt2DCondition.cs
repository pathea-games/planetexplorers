using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("LOOK AT 2D")]
    public class LookAt2DCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
		EAxis axis;
		OBJECT obj1;
		OBJECT obj2;
		ECompare comp;
		float amt;

        // 在此初始化参数
        protected override void OnCreate()
        {
			axis = (EAxis)Utility.ToEnumInt(parameters["axis"]);
			obj1 = Utility.ToObject(parameters["object1"]);
			obj2 = Utility.ToObject(parameters["object2"]);
			comp = Utility.ToCompare(parameters["compare"]);
			amt = Utility.ToSingle(missionVars, parameters["amount"]);
        }
        
        // 判断条件
        public override bool? Check()
        {
			Transform trans1 = PeScenarioUtility.GetObjectTransform(obj1);
			Transform trans2 = PeScenarioUtility.GetObjectTransform(obj2);
			if (trans1 == null || trans2 == null)
				return false;

			Vector3 directrix = trans2.position - trans1.position;
			Vector3 forward = trans1.forward;
			Vector3 right = trans1.right;
			directrix.y = forward.y = right.y = 0;
			switch (axis)
			{
			case EAxis.Left: return Utility.Compare(Vector3.Angle(-right, directrix), amt, comp);
			case EAxis.Right: return Utility.Compare(Vector3.Angle(right, directrix), amt, comp);
			case EAxis.Down: return true;
			case EAxis.Up: return true;
			case EAxis.Backward: return Utility.Compare(Vector3.Angle(-forward, directrix), amt, comp);
			case EAxis.Forward: return Utility.Compare(Vector3.Angle(forward, directrix), amt, comp);
			}
			return false;
        }

        protected override void SendReq()
        {
            byte[] data = BufferHelper.Export(w =>
            {
                w.Write(reqId);
                BufferHelper.Serialize(w, obj1);
                BufferHelper.Serialize(w, obj2);
                w.Write(amt);
                w.Write((byte)axis);
                w.Write((byte)comp);
            });

            PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckLookAt2D, data);
        }
    }
}
