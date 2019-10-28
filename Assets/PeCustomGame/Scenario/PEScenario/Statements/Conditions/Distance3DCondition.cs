using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("DISTANCE 3D")]
    public class Distance3DCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
		OBJECT obj1;
		OBJECT obj2;
		ECompare comp;
		float dist;

        // 在此初始化参数
        protected override void OnCreate()
        {
			obj1 = Utility.ToObject(parameters["object1"]);
			obj2 = Utility.ToObject(parameters["object2"]);
			comp = Utility.ToCompare(parameters["compare"]);
			dist = Utility.ToSingle(missionVars, parameters["distance"]);
        }
        
        // 判断条件
        public override bool? Check()
        {
            if (Pathea.PeGameMgr.IsSingle)
            {
                Transform trans1 = PeScenarioUtility.GetObjectTransform(obj1);
                Transform trans2 = PeScenarioUtility.GetObjectTransform(obj2);
                if (trans1 == null || trans2 == null)
                    return false;
                float d = Vector3.Distance(trans1.position, trans2.position);
                return Utility.Compare(d, dist, comp);
            }
            else if (Pathea.PeGameMgr.IsMulti)
            {
                bool? result = ReqCheck();
                if (result != null)
                    Debug.Log("Distance 3D result = " + result);
                return result;
            }

            return false;
        }

        protected override void SendReq()
        {
            // 向服务器端发请求
            byte[] data = BufferHelper.Export(w =>
            {
                w.Write(reqId);

                BufferHelper.Serialize(w, obj1);
                BufferHelper.Serialize(w, obj2);

                w.Write((byte)comp);
                w.Write(dist);
            });

            PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckDist3D, data);
        }
    }
}
