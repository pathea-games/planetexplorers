using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("DISTANCE 2D")]
    public class Distance2DCondition : ScenarioRTL.Condition
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
				Vector3 p1 = trans1.position;
				Vector3 p2 = trans2.position;
				p1.y = p2.y = 0;
				float d = Vector3.Distance(p1, p2);
				return Utility.Compare(d, dist, comp);
			}
			else if (Pathea.PeGameMgr.IsMulti)
			{
				bool? result = ReqCheck();
                if (result != null)
                    Debug.Log("Distance 2D result = " + result);
                return result;
			}
			return false;
        }

		protected override void SendReq ()
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

            PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckDist2D, data);
        }
    }
}
