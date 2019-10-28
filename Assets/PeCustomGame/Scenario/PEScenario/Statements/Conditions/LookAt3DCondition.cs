using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("LOOK AT 3D")]
    public class LookAt3DCondition : ScenarioRTL.Condition
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
			switch (axis)
			{
			case EAxis.Left: return Utility.Compare(Vector3.Angle(-trans1.right, directrix), amt, comp);
			case EAxis.Right: return Utility.Compare(Vector3.Angle(trans1.right, directrix), amt, comp);
			case EAxis.Down: return Utility.Compare(Vector3.Angle(-trans1.up, directrix), amt, comp);
			case EAxis.Up: return Utility.Compare(Vector3.Angle(trans1.up, directrix), amt, comp);
			case EAxis.Backward: return Utility.Compare(Vector3.Angle(-trans1.forward, directrix), amt, comp);
			case EAxis.Forward: return Utility.Compare(Vector3.Angle(trans1.forward, directrix), amt, comp);
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

            PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckLookAt3D, data);
        }
    }
}
