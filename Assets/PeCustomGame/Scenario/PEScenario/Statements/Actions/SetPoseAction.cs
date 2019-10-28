using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("SET POSE", true)]
    public class SetPoseAction : ScenarioRTL.Action
    {
		// 在此列举参数
		OBJECT obj;	// ENTITY
		EFunc func_pos;
		Vector3 pos;
		EFunc func_rot;
		Vector3 rot;

		// 在此初始化参数
		protected override void OnCreate()
		{
			obj = Utility.ToObject(parameters["object"]);
			func_pos = Utility.ToFunc(parameters["funcp"]);
			pos = Utility.ToVector(missionVars, parameters["point"]);
			func_rot = Utility.ToFunc(parameters["funcr"]);
			rot = Utility.ToVector(missionVars, parameters["euler"]);
		}
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
            //TODO: 获取到世界边界参数，才能做clamp操作
            Pathea.PeEntity entity = PeScenarioUtility.GetEntity(obj);
            if (entity != null && entity.peTrans != null)
            {
                if (pos != Vector3.zero)
                {
                    if (func_pos == EFunc.Plus || func_pos == EFunc.Minus)
                        entity.peTrans.position = LocalToWorld(entity.peTrans, pos, func_pos);
                    else
                        entity.peTrans.position = LocalToWorld(entity.peTrans, pos, EFunc.SetTo);
                }

                if (rot != Vector3.zero)
                {
                    Quaternion q = Quaternion.identity;
                    q.eulerAngles = Calculate(entity.peTrans.rotation.eulerAngles, rot, func_rot);
                    entity.peTrans.rotation = q;
                }
            }
            return true;
        }

        const int radius = 2;   //若点不是合法的站立点，将在一定区域内搜索合法站立点，这是此区域的半径
        private Vector3 LocalToWorld(Pathea.PeTrans trans, Vector3 local,EFunc func) 
        {
            Vector3 world = Vector3.Cross(Vector3.up, trans.forward) * pos.x + Vector3.up * pos.y + trans.forward * pos.z;
            if (func == EFunc.Plus)
                world = PETools.PEUtil.GetRandomPositionOnGround(trans.position + world, 0, radius);
            else if (func == EFunc.Minus)
                world = PETools.PEUtil.GetRandomPositionOnGround(trans.position - world, 0, radius);
            else if (func == EFunc.SetTo)
                world = local;
            return world;
        }

        private Vector3 Calculate(Vector3 lhs, Vector3 rhs, EFunc func) 
        {
            if (func == EFunc.Plus)
                return lhs + rhs;
            else if (func == EFunc.Minus)
                return lhs - rhs;
            else
                return rhs;
        }
    }
}
