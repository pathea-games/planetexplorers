using UnityEngine;
using System;
using System.Collections;

namespace AiAsset
{
    public class AiMath
    {

        public static bool Raycast(Vector3 start, Vector3 end, LayerMask layer)
        {
            float distance = Vector3.Distance(start, end);
            Vector3 direction = start - end;
            return Physics.Raycast(start, direction, distance, layer);
        }

        public static bool Raycast(Vector3 start, Vector3 end, out RaycastHit hitInfo, LayerMask layer)
        {
            float distance = Vector3.Distance(start, end);
            Vector3 direction = start - end;
            return Physics.Raycast(start, direction, out hitInfo, distance, layer);
        }

        public static Vector3 ProjectOntoPlane(Vector3 v, Vector3 normal)
        {
            return v - Vector3.Project(v, normal);
        }

        public static float ProjectDistance(Vector3 v, Vector3 normal)
        {
            return (v - Vector3.Project(v, normal)).magnitude;
        }

        public static float ProjectDistance(Vector3 position1, Vector3 position2, Vector3 normal)
        {
            Vector3 v = position2 - position1;
            return (v - Vector3.Project(v, normal)).magnitude;
        }

        public static float InverseAngle(Transform local, Vector3 dir)
        {
            if (local == null || dir == Vector3.zero)
            {
                Debug.LogWarning("local || dir is error");
                return 0.0f;
            }

            Vector3 dirProject = AiMath.ProjectOntoPlane(dir, local.transform.up); 

            return Vector3.Angle(local.forward, dirProject);
        }

        public static float Dot(Transform self, Transform target)
        {
            Vector3 direction = target.position - self.position;
            Vector3 forward = Vector3.Project(direction, self.forward);
            Vector3 right = Vector3.Project(direction, self.right);

            Vector3 newDirection = forward + right;

            return Vector3.Dot(newDirection.normalized, self.forward);
        }

        public static bool IsNumberic(string message, out int result)
        {
            //判断是否为整数字符串 
            //是的话则将其转换为数字并将其设为out类型的输出值、返回true, 否则为false 
            result = -1;   //result 定义为out 用来输出值 
            try
            {
                //当数字字符串的为是少于4时，以下三种都可以转换，任选一种 
                //如果位数超过4的话，请选用Convert.ToInt32() 和int.Parse() 

                //result = int.Parse(message); 
                //result = Convert.ToInt16(message); 
                result = Convert.ToInt32(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
