using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using WhiteCat.UnityExtension;
using ItemAsset;
using SkillSystem;
using Pathea.PeEntityExtTrans;

namespace PETools
{
    public class PEUtil
    {
		static Transform _transMainCamera = null;
		public static Transform MainCamTransform{
			get{
				if(_transMainCamera == null && Camera.main != null){
					_transMainCamera = Camera.main.transform;
				}
				return _transMainCamera;
			}
		}

		internal static int TreeLayer = 1 << Pathea.Layer.TreeStatic
				| 1 << Pathea.Layer.NearTreePhysics;
		internal static int WanderLayer = 1 << Pathea.Layer.VFVoxelTerrain
			| 1 << Pathea.Layer.SceneStatic;
		internal static int IgnoreWanderLayer = 1 << Pathea.Layer.Unwalkable 
				| 1 << Pathea.Layer.Building;
		internal static int Standlayer = 1 << Pathea.Layer.VFVoxelTerrain
				| 1 << Pathea.Layer.Building
				| 1 << Pathea.Layer.SceneStatic
				| 1 << Pathea.Layer.Unwalkable
				| 1 << Pathea.Layer.NearTreePhysics;

        //lz-2016.12.30 下坐骑的时候检测碰撞的Layer
        internal static int GetOffRideMask = 1 << Pathea.Layer.Default
            | 1 << Pathea.Layer.AIPlayer
            | 1 << Pathea.Layer.Player
            | 1 << Pathea.Layer.ProxyPlayer
            | 1 << Pathea.Layer.SceneStatic
            | 1 << Pathea.Layer.VFVoxelTerrain
            | 1 << Pathea.Layer.TreeStatic
            | 1 << Pathea.Layer.Ragdoll
            | 1 << Pathea.Layer.Unwalkable
            | 1 << Pathea.Layer.GIEProductLayer
            | 1 << Pathea.Layer.NearTreePhysics
            | 1 << Pathea.Layer.ProxyPlayer;

        public static T GetComponent<T>(GameObject obj) where T : MonoBehaviour
        {
            T t = null;

            Transform tr = obj.transform;
            while (tr != null && ((t = tr.GetComponent<T>()) == null))
            {
                tr = tr.parent;
            }

            return t;
        }

        public static string ToPrefabName(string name)
        {
            if (name.Contains("(Clone)"))
                return name.Substring(0, name.LastIndexOf("(Clone)"));
            else
                return name;
        }

		public static bool GetStandPosWithoutOverlap(Vector3 centerPos, float radius, ref Vector3 retPos, int overlapLayermask)
		{
			if (Physics.CheckSphere (centerPos, radius, overlapLayermask)) {
				Collider[] cols = Physics.OverlapSphere (centerPos, radius, overlapLayermask);
				if(cols != null && cols.Length > 0){
					Bounds bounds = cols[0].bounds;
					for(int i = 1; i < cols.Length; i++){
						bounds.Encapsulate(cols[i].bounds);
					}
					float dx = centerPos.x - bounds.center.x;
					float dz = centerPos.z - bounds.center.z;
					float mx = bounds.extents.x + radius - Mathf.Abs(dx);
					float mz = bounds.extents.z + radius - Mathf.Abs(dz);
					retPos = centerPos;
					if(mx < mz){
						if(dx < 0.0f){
							retPos.x = bounds.center.x - (bounds.extents.x + radius);
						} else {
							retPos.x = bounds.center.x + (bounds.extents.x + radius);
						}
					} else {
						if(dz < 0.0f){
							retPos.z = bounds.center.z - (bounds.extents.z + radius);
						} else {
							retPos.z = bounds.center.z + (bounds.extents.z + radius);
						}
					}
					// Raycast to get y

					return true;
				}
			}
			return false;
		}

        public static Transform GetChild(Transform root, string boneName, bool lowerCompare = false)
        {
			if (root == null || boneName == "" || "0" == boneName)
                return null;

			if(lowerCompare)
			{
				if(root.name.ToLower().Equals(boneName.ToLower()))
					return root;
			}
			else
			{
				if (root.name.Equals(boneName))
	                return root;
			}

            //Transform[] trs = root.GetComponentsInChildren<Transform>(true);
            //for (int i = 0; i < trs.Length; i++)
            //{
            //    if(trs[i] != null && trs[i].name.Equals(boneName))
            //    {
            //        return trs[i];
            //    }
            //}

            if (root.childCount > 0)
            {
                for (int i = 0; i < root.childCount; i++)
                {
					Transform result = GetChild(root.GetChild(i), boneName, lowerCompare);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        public static Transform GetChild(Transform root, Transform child)
        {
            if (root == null || child == null)
                return null;

			List<Transform> trList = new List<Transform>(root.GetComponentsInChildren<Transform>(true));
            return trList.Find(ret => ret == child);
        }

        public static string ToSystemPath(string path)
        {
            return Path.Combine(Application.dataPath, path.Substring(("Assets/").Length));
        }

        public static float Magnitude(Vector3 v, bool is3D = true)
        {
            if(!is3D)
                return Mathf.Sqrt(v.x*v.x + v.z*v.z);
            else
                return Mathf.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z);
        }

        public static float Magnitude(Vector3 v1, Vector3 v2, bool is3D = true)
        {
            return Magnitude(v1 - v2, is3D);
        }

        public static float MagnitudeH(Vector3 v1, Vector3 v2)
        {
            Vector3 v = v1 - v2;
            return Mathf.Sqrt(v.x*v.x + v.z*v.z);
        }

        public static float SqrMagnitude(Vector3 v1, Vector3 v2, bool is3D = true)
        {
            Vector3 v = v1 - v2;
            if (!is3D) v.y = 0.0f;
            return v.sqrMagnitude;
        }

        public static float SqrMagnitudeH(Vector3 v1, Vector3 v2)
        {
            Vector3 v = v1 - v2;
            return v.x * v.x + v.z * v.z;
        }

        public static float Height(Vector3 v1, Vector3 v2)
        {
            return v2.y - v1.y;
        }

        public static float SqrMagnitude(Vector3 v)
        {
            return v.sqrMagnitude;
        }

        public static float SqrMagnitudeH(Vector3 v)
        {
            return (Vector3.ProjectOnPlane(v, Vector3.up)).sqrMagnitude;
        }

        public static float SqrMagnitude(Transform t1, Bounds b1, Transform t2, Bounds b2, bool is3D = true)
        {
            if (t1 == null || t2 == null)
                return 0.0f;

            Vector3 v1 = t1.TransformPoint(b1.ClosestPoint(t1.InverseTransformPoint(t2.position)));
            Vector3 v2 = t2.TransformPoint(b2.ClosestPoint(t2.InverseTransformPoint(t1.position)));

            Vector3 v = v1 - v2;

            if (!is3D) v.y = 0.0f;

            return v.sqrMagnitude;
        }

        public static float Angle(Vector3 v1, Vector3 v2)
        {
            return Vector3.Angle(v1, v2);
        }

        public static float AngleH(Vector3 v1, Vector3 v2)
        {
            v1.y = 0.0f;
            v2.y = 0.0f;

            return Vector3.Angle(v1, v2);
        }

        public static float AngleZ(Vector3 v1, Vector3 v2)
        {
            v1.z = 0.0f;
            v2.z = 0.0f;

            return Vector3.Angle(v1, v2);
        }

		public static float AngleX(Vector3 v1, Vector3 v2)
		{
			v1.x = 0.0f;
			v2.x = 0.0f;
			
			return Vector3.Angle(v1, v2);
		}

		public static float SqrDistHToCam(Vector3 v1)
		{
			Vector3 v2 = Camera.main.transform.position;
			Vector3 v = v1 - v2;
			return v.x * v.x + v.z * v.z;
		}

        public static IntVector4 ToIntVector4(Vector3 position, int lod)
        {
            IntVector4 intVector4 = new IntVector4(new IntVector3(position), lod);

            int mask = lod + VoxelTerrainConstants._shift;
            intVector4.x = (intVector4.x >> mask) << mask;
            intVector4.y = (intVector4.y >> mask) << mask;
            intVector4.z = (intVector4.z >> mask) << mask;

            return intVector4;
        }

        public static IntVector3 ToIntVector3(Vector3 position, int lod)
        {
            IntVector4 intVector4 = new IntVector4(new IntVector3(position), lod);

            int mask = lod + VoxelTerrainConstants._shift;
            intVector4.x = (intVector4.x >> mask) << mask;
            intVector4.y = (intVector4.y >> mask) << mask;
            intVector4.z = (intVector4.z >> mask) << mask;

            return new IntVector3(intVector4.x, intVector4.y, intVector4.z);
        }

        public static IntVector2 ToIntVector2(Vector3 position)
        {
            return new IntVector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        }

        public static IntVector2 ToIntVector2(Vector3 position, int lod)
        {
            IntVector4 intVector4 = new IntVector4(new IntVector3(position), lod);

            int mask = lod + VoxelTerrainConstants._shift;
            intVector4.x = (intVector4.x >> mask) << mask;
            intVector4.y = (intVector4.y >> mask) << mask;
            intVector4.z = (intVector4.z >> mask) << mask;

            return new IntVector2(intVector4.x, intVector4.z);
        }

        public static IntVector2 ToIntVector2(IntVector4 intVector4)
        {
            return new IntVector2(intVector4.x, intVector4.z);
        }

        public static RaycastHit[] RaycastAll(Vector3 pos, Vector3 dir, float distance, int layerMask = 0, bool trigger = false)
        {
            List<RaycastHit> hitInfos;
            if (layerMask == 0)
                hitInfos = new List<RaycastHit>(Physics.RaycastAll(pos, dir, distance));
            else
                hitInfos = new List<RaycastHit>(Physics.RaycastAll(pos, dir, distance, layerMask));
            return hitInfos.FindAll(ret => ret.collider.isTrigger).ToArray();
        }

        public static bool Raycast(Vector3 pos, Vector3 dir, float distance, out RaycastHit hitInfo, int layerMask = 0, bool trigger = false)
        {
            RaycastHit[] hitInfos = RaycastAll(pos, dir, distance, layerMask, trigger);

            if(hitInfos != null && hitInfos.Length > 0)
            {
                hitInfo = hitInfos[0];
                float dValue = (pos - hitInfos[0].point).sqrMagnitude;
                foreach (RaycastHit hit in hitInfos)
                {
                    if((pos - hit.point).sqrMagnitude < dValue)
                    {
                        hitInfo = hit;
                        dValue = (pos - hit.point).sqrMagnitude;
                    }
                }
                return true;
            }

            hitInfo = new RaycastHit();
            return false;
        }

        public static bool Raycast(Vector3 pos, Vector3 dir, float distance, out RaycastHit hitInfo, Vector3 srcPos, int layerMask = 0, bool trigger = false)
        {
            RaycastHit[] hitInfos = RaycastAll(pos, dir, distance, layerMask, trigger);

            if (hitInfos != null && hitInfos.Length > 0)
            {
                hitInfo = hitInfos[0];
                float dValue = (srcPos - hitInfos[0].point).sqrMagnitude;
                foreach (RaycastHit hit in hitInfos)
                {
                    if ((srcPos - hit.point).sqrMagnitude < dValue)
                    {
                        hitInfo = hit;
                        dValue = (pos - hit.point).sqrMagnitude;
                    }
                }
                return true;
            }

            hitInfo = new RaycastHit();
            return false;
        }

        public static Vector3 GetTreeSurfacePos(GlobalTreeInfo tree)
        {
            RaycastHit hit;
            Vector3 pos;
            if (Pathea.PeGameMgr.IsStory)
                pos = tree.WorldPos;
            else
                pos = tree._treeInfo.m_pos;
            Physics.Raycast(pos + (Vector3.up) * 10, Vector3.down, out hit, 30f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));

            return hit.collider == null ? Vector3.zero : hit.point;
        }

        public static Vector3 CalculateAimPos(Vector3 dirPos, Vector3 standPos)
        {
            Vector3 newPos = dirPos;
            bool isUp = dirPos.y - standPos.y > Mathf.Epsilon;
            float heighit = isUp ? NPCConstNum.IK_Aim_Heiget_UpY : NPCConstNum.IK_Aim_Heiget_DownY;
            float DY = Mathf.Abs(dirPos.y - standPos.y) > heighit ? (dirPos.y - standPos.y) / Mathf.Abs(dirPos.y - standPos.y) * heighit : dirPos.y - standPos.y;
            newPos.y = standPos.y + DY;
            return newPos + Vector3.up * NPCConstNum.IK_Aim_Height;
        }

		public static  bool InAimAngle(Vector3 targetPos,Vector3 standPos,Vector3 rootDir,float angle = 80.0f)
		{
			Vector3 dir = targetPos - standPos;
			float angleH = PETools.PEUtil.AngleH(rootDir, dir);
			bool inAimAngle = angleH <= angle  ? true : false;
			return inAimAngle;
		}

		public static bool InAimDistance(Vector3 targetPos,Vector3 standPos,float minDistance,float maxdistance)
		{
			return MagnitudeH(targetPos,standPos) >= minDistance && MagnitudeH(targetPos,standPos) <= maxdistance;
		}


		public static bool InAimAngle(Transform trans,Transform mode,float angle = 80.0f)
		{
			Vector3 dir = trans.position - mode.position;
			float angleH = PETools.PEUtil.AngleH(mode.forward, dir);
			bool inAimAngle = angleH <= angle  ? true : false;
			return inAimAngle;
		}

        public static bool GetFixedPosition(IntVector4[] points, Vector3 pos, out Vector3 fixedPos, int layerMask = 0)
        {
            int yMin = points[0].y;
            int yMax = points[0].y + VoxelTerrainConstants._numVoxelsPerAxis << points[0].w;

//            float dValue = Mathf.Abs(pos.y - points[0].y);

            foreach (IntVector4 point in points)
            {
                yMin = Mathf.Min(yMin, point.y);
                yMax = Mathf.Max(yMax, point.y + VoxelTerrainConstants._numVoxelsPerAxis << point.w);
            }

            Vector3 start = new Vector3(pos.x, yMax, pos.z);
            float distance = Mathf.Abs(yMax - yMin);

            RaycastHit hitInfo;
            if (PETools.PEUtil.Raycast(start, Vector3.down, distance, out hitInfo, layerMask))
            {
                fixedPos = hitInfo.point;
                return true;
            }

            fixedPos = Vector3.zero;
            return false;
        }

        public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, bool is3D = false)
        {
            Vector3 local = Vector3.zero;
            if (is3D)
                local = Random.insideUnitSphere.normalized * Random.Range(minRadius, maxRadius);
            else
            {
                Vector2 rv2 = Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
                local = new Vector3(rv2.x, 0.0f, rv2.y);
            }
            
            return center + local;
        }

        public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, int layer)
        {
            Vector2 radVector2 = Random.insideUnitCircle * Random.Range(minRadius, maxRadius);
            Vector3 newPos = center + new Vector3(radVector2.x, 0.0f, radVector2.y);

            RaycastHit hitInfo;
            if (Physics.Raycast(newPos + Vector3.up * 128.0f, Vector3.down, out hitInfo, 256.0f, layer))
                return hitInfo.point;

            return Vector3.zero;
        }

        public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, float minHeight, float maxHeight)
        {
            return GetRandomPosition(center, minRadius, maxRadius) + Vector3.up * Random.Range(minHeight, maxHeight);
        }

        public static Vector3 GetRandomPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minDir, float maxDir)
        {
            Vector3 dir = new Vector3(direction.x, 0.0f, direction.z);
            dir = Quaternion.AngleAxis(Random.Range(minDir, maxDir), Vector3.up) * dir;
            return center + dir.normalized * Random.Range(minRadius, maxRadius); 
        }

        public static Vector3 GetRandomPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minDir, float maxDir, int layer,float upD = 128.0f,float downD = 256.0f)
        {
            Vector3 dir = new Vector3(direction.x, 0.0f, direction.z);
            dir = Quaternion.AngleAxis(Random.Range(minDir, maxDir), Vector3.up) * dir;
            Vector3 newPos = center + dir.normalized * Random.Range(minRadius, maxRadius);

            RaycastHit hitInfo;
            if (Physics.Raycast(newPos + Vector3.up * upD, Vector3.down, out hitInfo, downD, layer))
                return hitInfo.point;

            return Vector3.zero;
        }

        public static Vector3 GetRandomPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minDir, float maxDir, float minHeight, float maxHeight)
        {
            Vector3 radPos = GetRandomPosition(center, direction, minRadius, maxRadius, minDir, maxDir);
            return radPos + Vector3.up * Random.Range(minHeight, maxHeight);
        }

        public static Vector3 GetRandomPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minDir, float maxDir, float minHeight, float maxHeight, int layer)
        {
            Vector3 radPos = GetRandomPosition(center, direction, minRadius, maxRadius, minDir, maxDir);
            Vector3 newPos = radPos + Vector3.up * Random.Range(minHeight, maxHeight);

            RaycastHit hitInfo;
            if (Physics.Raycast(newPos + Vector3.up * 128.0f, Vector3.down, out hitInfo, 256.0f, layer))
            {
                return hitInfo.point;
            }

            return Vector3.zero;
        }

        public static bool GetNearbySafetyPos(PeTrans boundsTrans, int layermask, int standLayer, ref Vector3 position)
        {
            position = Vector3.zero;

            var trans = boundsTrans.realTrans;
            var max = boundsTrans.bound.max;
            var extents = boundsTrans.bound.extents;
            var center = boundsTrans.bound.center;

            Vector3 loc;
            for (loc.y = max.y; loc.y < max.y + 2.5f; loc.y += 0.5f)
            {
                for (float offsetZ = 0.25f; offsetZ < extents.z + 2.5f; offsetZ += 0.5f)
                {
                    for (float offsetX = 0.25f; offsetX < extents.x + 2.5f; offsetX += 0.5f)
                    {
                        loc.z = center.z + offsetZ;

                        loc.x = center.x - offsetX;
                        if (CheckPosIsSafety(trans, layermask, standLayer,position = trans.TransformPoint(loc))) return true;

                        loc.x = center.x  + offsetX;
                        if (CheckPosIsSafety(trans, layermask, standLayer,position = trans.TransformPoint(loc))) return true;

                        loc.z = center.z - offsetZ;
                        if (CheckPosIsSafety(trans, layermask, standLayer,position = trans.TransformPoint(loc))) return true;

                        loc.x = center.x - offsetX;
                        if (CheckPosIsSafety(trans, layermask, standLayer,position = trans.TransformPoint(loc))) return true;
                    }
                }
            }

            return false;
        }


        public static bool CheckPosIsSafety(Transform trans,int layermask,int standLayer, Vector3 pos)
        {
            Vector3 end = pos;
            end.y += 1.5f;
            if (!Physics.CheckCapsule(pos, end, 0.55f, layermask, QueryTriggerInteraction.Ignore))
            {
                // 是否有墙壁
                var direction = pos - trans.position;

                var hits = Physics.RaycastAll(
                    trans.position, direction, direction.magnitude,
                    layermask, QueryTriggerInteraction.Ignore);

                bool ok = true;

                for (int i = 0; i < hits.Length; i++)
                {
                    if (!hits[i].transform.IsChildOf(trans))
                    {
                        ok = false;
                        break;
                    }
                }

                // 落脚点检测
                if (ok)
                {
                    return !(Physics.Raycast(pos, Vector3.down, 128f, standLayer, QueryTriggerInteraction.Ignore));
                }
            }
            return false;
        }

        public static Vector3 GetVoxelPosition(Vector3 pos)
        {
            Ray rayStart = new Ray(pos + Vector3.up*128, Vector3.down);
            Vector3 point;
            if (PE.RaycastVoxel(rayStart, out point, 256, 10, 1))
                return point;
            
            return Vector3.zero;
        }

        public static Vector3 GetVoxelPositionOnGround(Vector3 pos, float minHeight, float maxHeight)
        {
            Vector3 voxelPos = GetVoxelPosition(pos);

            if (voxelPos != Vector3.zero && !CheckPositionUnderWater(pos))
                return voxelPos + Vector3.up * Random.Range(minHeight, maxHeight);

            return Vector3.zero;
        }

        public static Vector3 GetVoxelPositionInWater(Vector3 pos, float minHeight, float maxHeight)
        {
            Vector3 voxelPos = GetVoxelPosition(pos);

            float height;
            if (voxelPos != Vector3.zero && CheckPositionUnderWater(voxelPos, out height))
            {
                float mnh = Mathf.Max(0.0f, Mathf.Min(minHeight, height));
                float mxh = Mathf.Max(0.0f, Mathf.Min(maxHeight, height));
                if(mxh - mnh > 1.0f)
                    return voxelPos + Vector3.up * Random.Range(minHeight, maxHeight);
            }

            return Vector3.zero;
        }

        public static Vector3 GetVoxelPositionOnGroundInSky(Vector3 pos, float minHeight, float maxHeight)
        {
            Vector3 voxelPos = GetVoxelPosition(pos);

            if (voxelPos != Vector3.zero)
            {
                float height;
                if (!CheckPositionUnderWater(voxelPos, out height) || height < PETools.PEMath.Epsilon)
                    return voxelPos + Vector3.up * Random.Range(minHeight, maxHeight);
                else
                    return voxelPos + Vector3.up * Random.Range(minHeight, maxHeight) + Vector3.up * height;
            }

            return Vector3.zero;
        }

		public static Vector3 GetEqualPositionToStand(Vector3 _Camerapos,Vector3 _Cameradir,Vector3 _Playerpos,Vector3 _Palyerdir,float radiu)
		{
			Vector3 _pos = _Camerapos;
			Vector3 newPosition = PEUtil.GetRandomPosition(_pos, _Cameradir, 2, radiu, -90.0f, 90.0f);
			if(PEUtil.CheckPositionNearCliff(newPosition))
			{
				_pos = _Playerpos;
				newPosition = PEUtil.GetRandomPosition(_pos, _Palyerdir, 2, radiu, -90.0f, 90.0f);
			}
			
			RaycastHit hitInfo;
			Ray ray = new Ray(_pos,Vector3.up);
			//Target in the hole
			if(Physics.Raycast(ray, out hitInfo, 128.0f, PEConfig.GroundedLayer))
			{
				ray = new Ray(newPosition, Vector3.up);
				if(Physics.Raycast(ray, out hitInfo, 128.0f, PEConfig.GroundedLayer))
				{
					//hole in water
					if(PEUtil.CheckPositionUnderWater(hitInfo.point - Vector3.up))
						return newPosition;
					else
					{
						ray = new Ray(newPosition, Vector3.down);
						if(Physics.Raycast(ray, out hitInfo, 128.0f, PEConfig.GroundedLayer))
							return hitInfo.point + Vector3.up;
					}
				}
				else
					return _Playerpos;
			}
			else
			{
				//Target not in the hole
				Ray rayStart = new Ray(newPosition + 128.0f * Vector3.up, -Vector3.up);
				if(Physics.Raycast(rayStart, out hitInfo, 256.0f, PEConfig.GroundedLayer))
				{
					if(PEUtil.CheckPositionUnderWater(hitInfo.point))
						return newPosition;
					else
						return hitInfo.point + Vector3.up;
				}
			}

			return _Playerpos;
		}

        static bool GetCorrectHeight(Vector3 pos, float minHeight, float maxHeight, ref Vector3 newPos)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(pos, Vector3.up, out hitInfo, 128.0f, WanderLayer))
            {
                RaycastHit hitInfo1;
                if (Physics.Raycast(hitInfo.point - Vector3.up*0.1f, Vector3.down, out hitInfo1, 256.0f, IgnoreWanderLayer))
                    return false;

                if (!Physics.Raycast(hitInfo.point - Vector3.up * 0.1f, Vector3.down, out hitInfo1, 256.0f, WanderLayer) || CheckPositionUnderWater(hitInfo1.point))
                    return false;
                else
                    newPos = hitInfo1.point + Vector3.up * Random.Range(minHeight, maxHeight);
            }
            else
            {
                RaycastHit hitInfo1;
                if (Physics.Raycast(pos + Vector3.up * 128.0f, Vector3.down, out hitInfo1, 256.0f, IgnoreWanderLayer))
                    return false;

                if (!Physics.Raycast(pos + Vector3.up * 128.0f, Vector3.down, out hitInfo1, 256.0f, WanderLayer) || CheckPositionUnderWater(hitInfo1.point))
                    return false;
                else
                    newPos = hitInfo1.point + Vector3.up * Random.Range(minHeight, maxHeight);
            }

            return true;
        }

        public static Vector3 GetRandomPositionOnGround(Vector3 center, float minRadius, float maxRadius, bool isResult = true)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = GetRandomPosition(center, minRadius, maxRadius);

                if (!GetCorrectHeight(pos, 0.0f, 0.0f, ref pos))
                    continue;
                else
                    return pos;
            }

            return isResult ? GetVoxelPositionOnGround(GetRandomPosition(center, minRadius, maxRadius), 0.0f, 0.0f) :Vector3.zero;
        }

		public static Vector3 GetRandomPositionOnGroundForWander(Vector3 center, float minRadius, float maxRadius)
		{
			//Vector3 direction = Vector3.forward;
			for (int i = 0; i < 5; i++)
			{
				Vector3 pos = GetRandomPosition(center, minRadius, maxRadius);
				//Vector3 pos = GetRandomPosition(center, direction,minRadius,maxRadius,  minDir,  maxDir);
				RaycastHit hitInfo;

				if(Physics.SphereCast(pos + Vector3.up * 16.0f, 0.5f, Vector3.down, out hitInfo, 20.0f, IgnoreWanderLayer))
					continue;

				if (Physics.Raycast(pos + Vector3.up * 16.0f, Vector3.down, out hitInfo, 20.0f, WanderLayer))
                {
					//log_1959: prevent colony npcs from walking off cliffs or in the water if the colony is in a weird place like that
                    if (CheckPositionUnderWater(hitInfo.point) || CheckPositionNearCliff(hitInfo.point))
                        continue;

                    return hitInfo.point;
                }
					
			}

			return center;
		}

		public static Vector3 GetRandomPositionInCircle(Vector3 center, float minRadius, float maxRadius,Vector3 direction,float minDir,float maxDir)
		{
			for (int i = 0; i < 5; i++)
			{
				//Vector3 pos = GetRandomPosition(center, minRadius, maxRadius);
				Vector3 pos = GetRandomPosition(center, direction,minRadius,maxRadius,  minDir,  maxDir);
				RaycastHit hitInfo;
				
				if(Physics.SphereCast(pos + Vector3.up * 16.0f, 0.5f, Vector3.down, out hitInfo, 20.0f, IgnoreWanderLayer))
					continue;
				
				if (Physics.Raycast(pos + Vector3.up * 16.0f, Vector3.down, out hitInfo, 20.0f, WanderLayer))
				{
					//log_1959: prevent colony npcs from walking off cliffs or in the water if the colony is in a weird place like that
					if (CheckPositionUnderWater(hitInfo.point) || CheckPositionNearCliff(hitInfo.point))
						continue;
					
					return hitInfo.point;
				}
				
			}
			
			return center;
		}
		
        public static Vector3 GetRandomPositionOnGround(Vector3 center, float minRadius, float maxRadius, float minHeight, float maxHeight, bool isResult = true)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = GetRandomPosition(center, minRadius, maxRadius);

                if (!GetCorrectHeight(pos, minHeight, maxHeight, ref pos))
                    continue;
                else
                    return pos;
            }

            return isResult ? GetVoxelPositionOnGround(GetRandomPosition(center, minRadius, maxRadius), minHeight, maxHeight) : Vector3.zero;
        }

        public static Vector3 GetEmptyPositionOnGround(Vector3 center, float minRadius, float maxRadius)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = GetRandomPosition(center, minRadius, maxRadius);
                RaycastHit hitInfo;

                if (!Physics.SphereCast(pos + Vector3.up * 16.0f, 0.5f, Vector3.down, out hitInfo, 20.0f, IgnoreWanderLayer))
                    return pos;
            }
            return Vector3.zero;
        }

        public static Vector3 GetRandomPositionOnGround(Vector3 center, Vector3 dir, float minRadius, float maxRadius, float minAngle, float maxAngle, bool isResult = true)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);

                if (!GetCorrectHeight(pos, 0.0f, 0.0f, ref pos) || PEUtil.CheckPositionUnderWater(pos+Vector3.up*0.5f))
                    continue;
                else
                    return pos;
            }

            return isResult ? GetVoxelPositionOnGround(GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle), 0.0f, 0.0f) : Vector3.zero;
        }

        public static Vector3 GetRandomPositionOnGround(Vector3 center, Vector3 dir, float minRadius, float maxRadius, float minHeight, float maxHeight, float minAngle, float maxAngle, bool isResult = true)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);

                if (!GetCorrectHeight(pos, minHeight, maxHeight, ref pos) || PEUtil.CheckPositionUnderWater(pos+Vector3.up*0.5f))
                    continue;
                else
                    return pos;
            }

            return isResult ? GetVoxelPositionOnGround(GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle), minHeight, maxHeight) : Vector3.zero;
        }

        public static Vector3 GetRandomPositionInWater(Vector3 center, Vector3 dir, float minRadius, float maxRadius, float minHeight, float maxHeight, float minAngle, float maxAngle, bool isResult = true)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);

                RaycastHit hitInfo;
                if (Physics.Raycast(pos + Vector3.up * 128.0f, Vector3.down, out hitInfo, 256.0f, IgnoreWanderLayer))
                    continue;

                if (Physics.Raycast(pos + Vector3.up * 128.0f, Vector3.down, out hitInfo, 256.0f, WanderLayer))
                {
                    float height;
                    if (GetWaterSurfaceHeight(hitInfo.point, out height))
                    {
                        float rMin = height - maxHeight;
                        float rMax = height - minHeight;

                        rMin = Mathf.Max(rMin, hitInfo.point.y);
                        rMax = Mathf.Max(rMax, hitInfo.point.y);

                        if (rMax <= rMin)
                            continue;

                        return new Vector3(hitInfo.point.x, Random.Range(rMin, rMax), hitInfo.point.z);
                    }
                }
            }

            return isResult ? GetVoxelPositionInWater(GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle), minHeight, maxHeight) : Vector3.zero;
        }

        public static Vector3 GetRandomPositionInSky(Vector3 center, Vector3 dir, float minRadius, float maxRadius, float minHeight, float maxHeight, float minAngle, float maxAngle, bool isReslut = true)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector3 pos = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);

                RaycastHit hitInfo;
                if (Physics.Raycast(pos + Vector3.up * 128.0f, Vector3.down, out hitInfo, 256.0f, IgnoreWanderLayer))
                    continue;

                if (Physics.Raycast(pos + Vector3.up * 128.0f, Vector3.down, out hitInfo, 256.0f, WanderLayer))
                {
                    float height;
                    Vector3 newPos = Vector3.zero;
                    if (!CheckPositionUnderWater(hitInfo.point, out height) || height < 0.0f)
                        newPos = hitInfo.point + Vector3.up * Random.Range(minHeight, maxHeight);
                    else
                        newPos = hitInfo.point + Vector3.up * Random.Range(minHeight, maxHeight) + Vector3.up * height;

                    //Vector3 rayDir = newPos - center;
                    float rayDis = Vector3.Distance(newPos, center);
                    if (Physics.Raycast(center, newPos - center, rayDis, PEConfig.ObstacleLayer))
                        continue;

                    return newPos;
                }
            }

            return isReslut ? GetVoxelPositionOnGroundInSky(GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle), minHeight, maxHeight) : Vector3.zero;
        }

		public static Vector3 GetRandomFollowPosInSky(Vector3 center, Vector3 dir, float minRadius, float maxRadius, float minHeight, float maxHeight, float minAngle, float maxAngle, bool isReslut = true)
		{
			for (int i = 0; i < 10; i++)
			{
				Vector3 pos = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);

				RaycastHit hitInfo;
				if (Physics.Raycast(pos + Vector3.up * maxHeight, Vector3.down, out hitInfo, 2.0f*maxHeight, IgnoreWanderLayer))
					continue;
				
				if (Physics.Raycast(pos + Vector3.up * maxHeight, Vector3.down, out hitInfo, 2.0f*maxHeight, WanderLayer))
				{
					float height;
					if (!CheckPositionUnderWater(hitInfo.point, out height) || height < 0.0f)
						return hitInfo.point + Vector3.up * Random.Range(minHeight, maxHeight);
					else
						return hitInfo.point + Vector3.up * Random.Range(minHeight, maxHeight) + Vector3.up * height;
				}
			}
			
			return isReslut ? GetVoxelPositionOnGroundInSky(GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle), minHeight, maxHeight) : Vector3.zero;

		}
		public static Vector3 CorrectionPostionToStand(Vector3 pos,float upHeigtht = 1.0f,float downHeight = 16.0f)
		{
			RaycastHit hitInfo;
            if (Physics.Raycast(pos + Vector3.up * upHeigtht, Vector3.down, out hitInfo, downHeight, Standlayer))
			{
				return hitInfo.point;
			}
			return pos;
		}

        public static Vector3 GetDirtionPostion(Vector3 center,Vector3 dir,float minRadius,float maxRadius,float minAngle,float maxAngle,float dis3D)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector3 pos = GetRandomPosition(center, dir, minRadius, maxRadius, minAngle, maxAngle);

                RaycastHit hitInfo;
                if (Physics.Raycast(pos + Vector3.up * 32.0f, Vector3.down, out hitInfo, 2.0f * 32.0f, Standlayer))
                {
                    float d = Magnitude(hitInfo.point,center);
                    if ( d <= dis3D) //hitInfo.point.y >= center.y &&
                        return hitInfo.point;
                    
                }
            }
            return Vector3.zero;
        }

		//检测目标点能否站立
		public static Vector3 CheckPosForNpcStand(Vector3 pos)
		{
			RaycastHit hitInfo;			
			//检测目标点是否会导致NPC站在模型中
			bool _Inmode = Physics.Raycast(pos, Vector3.up, out hitInfo, 2.0f, Standlayer);
			Vector3 _modePos = hitInfo.point;
			if(_Inmode &&  Physics.Raycast(_modePos + Vector3.up * 8.0f, Vector3.down, out hitInfo, 8.0f, Standlayer))
			{
				return hitInfo.point;
			}

			return pos;
		}

		public static Vector3 GetCenter(Collider c)
        {
            if (c is BoxCollider)
                return (c as BoxCollider).center;
            else if (c is SphereCollider)
                return (c as SphereCollider).center;
            else if (c is CapsuleCollider)
                return (c as CapsuleCollider).center;

            Debug.LogError("type is error : " + c.GetType().ToString());
            return Vector3.zero;
        }

        public static Vector3 GetCenterOfWorld(Collider c)
        {
            return c.transform.TransformPoint(GetCenter(c));
        }

        public static bool IsScopeAngle(Vector3 v, Vector3 n, Vector3 axis, float minAngle, float maxAngle)
        {
            Vector3 v1 = Vector3.ProjectOnPlane(v, axis);
            Vector3 n1 = Vector3.ProjectOnPlane(n, axis);

            Vector3 mv1 = Quaternion.AngleAxis(minAngle, axis) * n1;
            Vector3 mv2 = Quaternion.AngleAxis(maxAngle, axis) * n1;

            float value1 = Vector3.Cross(mv1, v1.normalized).y;
            float value2 = Vector3.Cross(mv2, v1.normalized).y;

            if (Vector3.Cross(mv1, mv2).y > 0.0f)
            {
                if (Mathf.Abs(value1 - value2) < 0.0001f)
                    return true;
                else
                    return value1 >= 0.0f && value2 <= 0.0f;
            }
            else
            {
                if (Mathf.Abs(value1 - value2) < 0.0001f)
                    return true;
                else
                    return value1 >= 0.0f || value2 <= 0.0f;
            }
        }

        public static float GetAngle(Vector3 v, Vector3 n, Vector3 axis)
        {
            Vector3 v1 = Vector3.ProjectOnPlane(v, axis);
            Vector3 n1 = Vector3.ProjectOnPlane(n, axis);

            float angle = Vector3.Angle(v1.normalized, n1.normalized);
            float value = Vector3.Cross(v1, n1).y;

            return value > 0.0f ? -angle : angle;
        }

        public static float Angle(Vector3 v, Vector3 n, Vector3 axis)
        {
            Vector3 v1 = Vector3.ProjectOnPlane(v, axis);
            Vector3 n1 = Vector3.ProjectOnPlane(n, axis);

            return Vector3.Angle(v1.normalized, n1.normalized);
        }

        public static void ResetTransform(Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }

        public static string[] ToArrayString(string str, char c)
        {
            return str.Split(new char[]{ c });
        }

        public static int[] ToArrayInt32(string str, char c)
        {
            string[] strs = ToArrayString(str, c);
            List<int> tmpList = new List<int>();
            foreach (string s in strs)
            {
                tmpList.Add(System.Convert.ToInt32(s));
            }

            return tmpList.ToArray();
        }

        public static byte[] ToArrayByte(string str, char c)
        {
            string[] strs = ToArrayString(str, c);
            List<byte> tmpList = new List<byte>();
            foreach (string s in strs)
            {
                tmpList.Add(System.Convert.ToByte(s));
            }

            return tmpList.ToArray();
        }

        public static float[] ToArraySingle(string str, char c)
        {
            string[] strs = ToArrayString(str, c);
            List<float> tmpList = new List<float>();
            foreach (string s in strs)
            {
                tmpList.Add(System.Convert.ToSingle(s));
            }

            return tmpList.ToArray();
        }

        public static Vector3 ToVector3(string str, char c)
        {
            string[] strs = ToArrayString(str, c);

            if (strs.Length != 3)
                return Vector3.zero;

            List<float> tmpList = new List<float>();
            foreach (string s in strs)
            {
                tmpList.Add(System.Convert.ToSingle(s));
            }

            return new Vector3(tmpList[0], tmpList[1], tmpList[2]);
        }

        public static Color32 ToColor32(string data, char c)
        {
            byte[] datas = ToArrayByte(data, c);
            if (datas.Length != 4)
                return new Color32(0, 0, 0, 0);
            else
                return new Color32(datas[0], datas[1], datas[2], datas[3]);
        }

        public static void DrawBounds(Bounds bound, Color color)
        {
            if (Application.isEditor)
            {
                Vector3[] vert = new Vector3[8];
                for (int i = 0; i < 8; i++)
                {
                    vert[i] = bound.center;
                    if ((i & 1) == 0)
                        vert[i] -= bound.extents.x * new Vector3(1, 0, 0);
                    else
                        vert[i] += bound.extents.x * new Vector3(1, 0, 0);
                    if ((i & 2) == 0)
                        vert[i] -= bound.extents.y * new Vector3(0, 1, 0);
                    else
                        vert[i] += bound.extents.y * new Vector3(0, 1, 0);
                    if ((i & 4) == 0)
                        vert[i] -= bound.extents.z * new Vector3(0, 0, 1);
                    else
                        vert[i] += bound.extents.z * new Vector3(0, 0, 1);
                }
                Debug.DrawLine(vert[0], vert[1], color);
                Debug.DrawLine(vert[2], vert[3], color);
                Debug.DrawLine(vert[4], vert[5], color);
                Debug.DrawLine(vert[6], vert[7], color);
                Debug.DrawLine(vert[0], vert[4], color);
                Debug.DrawLine(vert[1], vert[5], color);
                Debug.DrawLine(vert[2], vert[6], color);
                Debug.DrawLine(vert[3], vert[7], color);
                Debug.DrawLine(vert[0], vert[2], color);
                Debug.DrawLine(vert[1], vert[3], color);
                Debug.DrawLine(vert[4], vert[6], color);
                Debug.DrawLine(vert[5], vert[7], color);
            }
        }

        public static void DrawBounds(Transform tr, Bounds bound, Color color)
        {
            if (Application.isEditor && tr != null)
            {
                Vector3[] vert = new Vector3[8];
                for (int i = 0; i < 8; i++)
                {
                    vert[i] = bound.center;
                    if ((i & 1) == 0)
                        vert[i] -= bound.extents.x * new Vector3(1, 0, 0);
                    else
                        vert[i] += bound.extents.x * new Vector3(1, 0, 0);
                    if ((i & 2) == 0)
                        vert[i] -= bound.extents.y * new Vector3(0, 1, 0);
                    else
                        vert[i] += bound.extents.y * new Vector3(0, 1, 0);
                    if ((i & 4) == 0)
                        vert[i] -= bound.extents.z * new Vector3(0, 0, 1);
                    else
                        vert[i] += bound.extents.z * new Vector3(0, 0, 1);

                    vert[i] = tr.TransformPoint(vert[i]);
                }
                Debug.DrawLine(vert[0], vert[1], color);
                Debug.DrawLine(vert[2], vert[3], color);
                Debug.DrawLine(vert[4], vert[5], color);
                Debug.DrawLine(vert[6], vert[7], color);
                Debug.DrawLine(vert[0], vert[4], color);
                Debug.DrawLine(vert[1], vert[5], color);
                Debug.DrawLine(vert[2], vert[6], color);
                Debug.DrawLine(vert[3], vert[7], color);
                Debug.DrawLine(vert[0], vert[2], color);
                Debug.DrawLine(vert[1], vert[3], color);
                Debug.DrawLine(vert[4], vert[6], color);
                Debug.DrawLine(vert[5], vert[7], color);
            }
        }

		public static void DrawGLBounds(Bounds bound, Color color, Material mat)
		{
			// Create material if not has
			if (mat == null)
			{
				return;
			}
			Vector3[] vert = new Vector3[8];
			
			for (int i = 0; i < 8; i++)
			{
				vert[i] = bound.center;
				
				if ((i & 1) == 0)
					vert[i] -= bound.extents.x * Vector3.right;
				else
					vert[i] += bound.extents.x * Vector3.right;
				
				if ((i & 2) == 0)
					vert[i] -= bound.extents.y * Vector3.up;
				else
					vert[i] += bound.extents.y * Vector3.up;
				
				if ((i & 4) == 0)
					vert[i] -= bound.extents.z * Vector3.forward;
				else
					vert[i] += bound.extents.z * Vector3.forward;
			}
			
			// Save camera's matrix.
			GL.PushMatrix();
			
			// Set the current material
			mat.SetPass(0);
			
			// Draw Lines -- twelve edges
			GL.Begin(GL.LINES);
			GL.Color(color);
			GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
			GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
			GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
			GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
			GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
			GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
			GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
			GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
			GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
			GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
			GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
			GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
			GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
			GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
			GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
			GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
			GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
			GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
			GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
			GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
			GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
			GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
			GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
			GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
			GL.End();
			
			// Draw Quads -- six faces
			GL.Begin(GL.QUADS);
			GL.Color(color * 0.15f);
			GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
			GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
			GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
			GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
			GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
			GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
			GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
			GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
			GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
			GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
			GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
			GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
			GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
			GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
			GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
			GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
			GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
			GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
			GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
			GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
			GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
			GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
			GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
			GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
			GL.End();
			
			// Restore camera's matrix.
			GL.PopMatrix();
		}

        public static Bounds GetLocalBounds(GameObject obj, Bounds bounds)
        {
            Bounds newBounds = bounds;
            newBounds.center = obj.transform.InverseTransformPoint(newBounds.center);
            return newBounds;
        }

        public static Bounds GetWorldBounds(GameObject obj, Bounds bounds)
        {
            Bounds newBounds = bounds;
            newBounds.center = obj.transform.TransformPoint(newBounds.center);
            return newBounds;
        }

        public static Bounds GetWordColliderBoundsInChildren(GameObject obj)
        {
            Bounds bound = new Bounds();

            if (obj != null)
            {
                Collider[] colliders = obj.GetComponentsInChildren<Collider>();
                if (colliders != null)
                {
                    foreach (Collider collider in colliders)
                    {
                        if (collider != null && !collider.isTrigger)
                        {
                            if (bound.size == Vector3.zero)
                                bound = collider.bounds;
                            else
                                bound.Encapsulate(collider.bounds);
                        }
                    }
                }
            }

            return bound;
        }

        public static Bounds GetLocalColliderBoundsInChildren(GameObject obj)
        {
            Bounds bound = new Bounds();

            if (obj != null)
            {
                Quaternion original = obj.transform.rotation;
                obj.transform.rotation = Quaternion.identity;

                Collider[] colliders = obj.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    if (!collider.isTrigger)
                    {
                        Bounds bounds = collider.bounds;
                        bounds.center = obj.transform.InverseTransformPoint(bounds.center);
                        if (bound.size == Vector3.zero)
                            bound = bounds;
                        else
                            bound.Encapsulate(bounds);
                    }
                }

                obj.transform.rotation = original;
            }

            return bound;
        }

        public static Bounds GetLocalViewBoundsInChildren(GameObject obj)
        {
            Bounds bound = new Bounds();

            if (obj != null)
            {
                Quaternion original = obj.transform.rotation;
                obj.transform.rotation = Quaternion.identity;

                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    Bounds bounds = renderer.bounds;
                    bounds.center = obj.transform.InverseTransformPoint(bounds.center);
                    if (bound.size == Vector3.zero)
                        bound = bounds;
                    else
                        bound.Encapsulate(bounds);
                }

                obj.transform.rotation = original;
            }

            return bound;
        }

        public static bool CheckPositionOnGround(Vector3 position, out Vector3 groundPosition, float lowHeight, float upHeight, LayerMask groundLayer)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(position + Vector3.up * upHeight, -Vector3.up, out hitInfo, lowHeight + upHeight, groundLayer))
            {
                groundPosition = hitInfo.point;
                return true;
            }

            groundPosition = Vector3.zero;
            return false;
        }

        public static bool CheckPositionUnderWater(Vector3 v)
        {
            if (VFVoxelWater.self != null)
                return VFVoxelWater.self.IsInWater(v.x, v.y, v.z);
            else
                return false;
        }

        public static bool CheckPositionUnderWater(Vector3 v, out float height)
        {
            if (CheckPositionUnderWater(v))
            {
                height = VFVoxelWater.self.UpToWaterSurface(v.x, v.y, v.z);
                return true;
            }
            else
            {
                height = 0.0f;
                return false;
            }
        }

		public static bool CheckPositionInSky(Vector3 position,float upHeight = 5.0f)
		{
			RaycastHit hitInfo;
			if (!Physics.Raycast(position + Vector3.up * upHeight, Vector3.down, out hitInfo, upHeight + 2.0f, GameConfig.GroundLayer))
			{
				return true;
			}
			return false;
		}

		public static bool CheckPositionNearCliff(Vector3 position,float radiu = 16.0f)
		{
			RaycastHit hitInfo;
			if (!Physics.Raycast(position + Vector3.up * radiu, Vector3.down, out hitInfo, 128.0f, GameConfig.GroundLayer))
			{
				return hitInfo.distance >= radiu*2;
			}
			return false;
		}

		public static bool CheckErrorPos(Vector3 fixedPos )
		{
			if((fixedPos.x > -10 && fixedPos.x < 10 ) && (fixedPos.y > -10 && fixedPos.y < 10) && (fixedPos.z > -10 && fixedPos.z < 10))
				return false;
			if((fixedPos.x < -9999999 || fixedPos.x > 9999999) || (fixedPos.y < -9999999 || fixedPos.y > 9999999) || (fixedPos.z < -9999999 || fixedPos.z > 9999999))
				return false;
			return true;
		}

        public static bool GetWaterSurfaceHeight(Vector3 v, out float waterHeight)
        {
            if (CheckPositionUnderWater(v))
            {
                float height = VFVoxelWater.self.UpToWaterSurface(v.x, v.y, v.z);
                waterHeight = height + v.y;
                return true;
            }

            waterHeight = 0.0f;
            return false;
        }

		/// <summary>
		/// Gets the terrain normal with circle shape.
		/// </summary>
		public static Vector3 GetTerrainNormal(Vector3 center, float radius, int radiusAccuracy, int angleAccuracy,
		                                       int groundlayer, Vector3 checkRayUpDir, float checkDis = 5f)
		{
			if(checkRayUpDir == Vector3.zero)
				checkRayUpDir = Vector3.up;
			checkRayUpDir.Normalize();
			if(radiusAccuracy < 1)
				radiusAccuracy = 1;
			if(angleAccuracy < 3)
				angleAccuracy = 3;

			Vector3 updir = checkRayUpDir;
			Vector3 rightDir = Vector3.right;
			if(checkRayUpDir != Vector3.up)
				Vector3.OrthoNormalize(ref updir, ref rightDir);

			RaycastHit hitInfo;
			
			Vector3 retNormal = Vector3.zero;

			if(Physics.Raycast(center + checkRayUpDir, -checkRayUpDir, out hitInfo, checkDis + 1f, groundlayer))
				retNormal = hitInfo.normal;

			for (int i = 0; i < radiusAccuracy; i++) 
			{
				for(int j = 0; j < angleAccuracy; j++)
				{
					Vector3 rayStart = center + checkRayUpDir
							+ Quaternion.AngleAxis(360*j/angleAccuracy, checkRayUpDir) * (rightDir * radius * (i + 1)/radiusAccuracy);
					if ( Physics.Raycast(rayStart, checkRayUpDir*-2, out hitInfo, checkDis + 1f, groundlayer) )
						retNormal += hitInfo.normal * (1f - (i + 1f) / (radiusAccuracy));
				}
			}
			return retNormal;
		}

        public static bool IsDamageCollider(Collider collider)
        {
            return collider.gameObject.tag.Equals("Damage");
        }

        public static void IgnoreCollision(GameObject obj1, GameObject obj2, bool isIgnore = true)
        {
            if (obj1 != null && obj2 != null)
            {
                Collider[] arr1 = obj1.GetComponentsInChildren<Collider>();
                Collider[] arr2 = obj2.GetComponentsInChildren<Collider>();

                for (int i = 0; i < arr1.Length; i++)
                {
                    if (!arr1[i].gameObject.activeSelf)
                        continue;

                    bool e1 = arr1[i].enabled;
                    arr1[i].enabled = true;
                    for (int j = 0; j < arr2.Length; j++)
                    {
                        if (!arr2[j].gameObject.activeSelf)
                            continue;

                        bool e2 = arr2[j].enabled;
                        arr2[j].enabled = true;
                        if (arr1[i] != arr2[j])
                        {
                            Physics.IgnoreCollision(arr1[i], arr2[j], isIgnore);
                        }
                        arr2[j].enabled = e2;
                    }
                    arr1[i].enabled = e1;
                }
            }
		}
		
		public static void IgnoreCollision(Collider col1, Collider col2, bool isIgnore = true)
		{
			if(null == col1 || null == col2)
				return;
			bool col1Enable = col1.enabled;
			bool col2Enable = col2.enabled;
			col1.enabled = true;
			col2.enabled = true;
			if(col1 != col2 && col1.enabled && col2.enabled)
				Physics.IgnoreCollision(col1, col2, isIgnore);
			col1.enabled = col1Enable;
			col2.enabled = col2Enable;
		}
		
		public static void IgnoreCollision(Collider[] cols1, Collider col2, bool isIgnore = true)
		{
			if(null == cols1 || null == col2)
				return;
			Collider col1;
			for(int i = 0; i < cols1.Length; ++i)
			{
				col1 = cols1[i];
				IgnoreCollision(col1, col2);
			}
		}

		public static void IgnoreCollision(Collider[] cols1, Collider[] cols2, bool isIgnore = true)
		{
			if(null == cols1 || null == cols2)
				return;
			Collider col1, col2;
			for(int i = 0; i < cols1.Length; ++i)
			{
				for(int j = 0; j < cols2.Length; ++j)
				{
					col1 = cols1[i];
					col2 = cols2[j];
					IgnoreCollision(col1, col2);
				}
			}
		}

        public static RaycastHit[] SortHitInfo(RaycastHit[] hits, bool ignoreTrigger = true)
        {
            List<RaycastHit> hitInfos = new List<RaycastHit>(hits);

            if (ignoreTrigger)
            {
                hitInfos = hitInfos.FindAll(ret => !ret.collider.isTrigger);
            }

            //hitInfos.Sort(SortHitInfo);
            hitInfos.Sort(delegate(RaycastHit hit1, RaycastHit hit2) { return hit1.distance.CompareTo(hit2.distance); });

            return hitInfos.ToArray();
        }

        public static bool GetPositionLayer(Vector3 position, out Vector3 point, int layer, int obstructLayer)
        {
            point = Vector3.zero;

            RaycastHit hitInfo;

			if(Physics.SphereCast(position + Vector3.up * 128.0f, 1.0f, Vector3.down, out hitInfo, 256.0f, IgnoreWanderLayer))
            //if (Physics.Raycast(position + Vector3.up * 128.0f, Vector3.down, out hitInfo, 256.0f, obstructLayer))
                return false;

            if (Physics.Raycast(position + Vector3.up * 128.0f, Vector3.down, out hitInfo, 256.0f, layer))
            {
                point = hitInfo.point;
                return true;
            }

            return false;
        }

        public static Vector3 ConstantSlerp(Vector3 from, Vector3 to, float angle)
        {
            float value = Mathf.Min(1, angle / Vector3.Angle(from, to));
            return Vector3.Slerp(from, to, value);
        }
			
		/// <summary>
		/// Gets the terrain normal at a direction.
		/// </summary>
		public static Vector3 GetTerrainNormal(Vector3 center, Vector3 direction,
		                                       int groundLayer, Vector3 checkRayUpDir, int accuracy = 3, float checkDis = 5f)
		{
			if(direction == Vector3.zero)
				return Vector3.up;
			direction.Normalize();
			float checkDisInterval = checkDis / accuracy;
			Ray checkRay = new Ray(center + checkRayUpDir, direction);
			float dis = checkDis;
			RaycastHit hitInfo;
			if(Physics.Raycast(checkRay, out hitInfo, checkDis, groundLayer))
				dis = hitInfo.distance;
			int checkTime = Mathf.FloorToInt(dis / checkDisInterval);
			if(checkTime == 0)
				checkTime = 1;
			Vector3 normal = Vector3.zero;
			for(int i = 0; i < checkTime; i++)
			{
				checkRay.origin = center + checkRayUpDir + i * checkDisInterval * direction;
				checkRay.direction = Vector3.down;
				if(Physics.Raycast(checkRay, out hitInfo, checkDis, groundLayer))
					normal += hitInfo.normal * (checkTime - i) / checkTime;
				else
					break;
			}
			return normal;
		}

        public static bool ContainsParameter(Animator animator, string parameter)
        {
            for (int i = 0; i < animator.parameters.Length; i++)
            {
                if (animator.parameters[i].name.Equals(parameter))
                    return true;
            }

            return false;
        }

		public static Vector3 CardinalSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension = 0.5f)
		{
			return p1
				+ (p2 - p0) * tension * t
					+ ((p2 - p1) * 3 - (p3 - p1) * tension - (p2 - p0) * tension * 2) * t * t
					+ ((p3 - p1) * tension - (p2 - p1) * 2 + (p2 - p0) * tension) * t * t * t;
		}

		public static bool CheckForWard(Pathea.PeEntity entity, Vector3 pos,Vector3 dir,float wide,out Vector3 v,out float ditance,float radiu)
		{
			v = Vector3.zero;
			ditance = 0.0f;
			if(entity == null)
				return false;

			if(entity.IsNpcInDinnerTime || entity.IsNpcInSleepTime || entity.NpcHasAnyRequest || entity.attackEnemy != null)
				return false;


			RaycastHit hitInfo;
			Vector3 pos1 = pos + Vector3.up;
			int layer = 1 << Pathea.Layer.Player;
			Debug.DrawLine(pos1,pos1 +dir * radiu,Color.red);

//			if(Physics.SphereCast(pos + Vector3.up * 128.0f, 2.0f, Vector3.down, out hitInfo, 256.0f, layer))
//			{
//				v = (hitInfo.point - pos).normalized;
//				ditance = hitInfo.distance;
//				return true;
//			}
		
//			Collider[] hitInfos = Physics.OverlapSphere(pos,radiu,layer);
//			if(hitInfos != null && hitInfos.Length != 0)
//			{
//				for(int i=0;i<hitInfos.Length;i++)
//				{
//					if((pos -hitInfos[i].transform.position).normalized == Vector3.zero)
//						continue;
//
//					v = v + (pos -hitInfos[i].transform.position).normalized;
//				//	v = v + (pos - hitInfos[i].ClosestPointOnBounds(pos)).normalized *hitInfos[i].contactOffset;
//				}
//				return true;
//			}
			if(Physics.SphereCast(pos1, wide, dir, out hitInfo, radiu,layer))
			{
				v = Vector3.ProjectOnPlane(pos - hitInfo.transform.position, Vector3.up);
				ditance = hitInfo.distance;
				return true;
			}
			return false;

		}

        public static bool CanAttackReputation(PeEntity e1, PeEntity e2)
        {
            if (e1 == null || e2 == null)
                return false;

            int pid1 = (int)e1.GetAttribute(AttribType.DefaultPlayerID);
            int pid2 = (int)e2.GetAttribute(AttribType.DefaultPlayerID);

            return CanAttackReputation(pid1, pid2);
        }

        public static bool CanAttackReputation(int pid1, int pid2)
        {
            //lw:声望为Neutral时：目标攻击该阵营的，便还击
            //声望小于Neutral时：一见到目标便攻击
            //声望大于Neutral时：两者之间不能互相攻击
            //if (ReputationSystem.Instance.HasReputation(pid1, pid2)
            //   && ReputationSystem.Instance.GetReputationLevel(pid1, pid2) == ReputationSystem.ReputationLevel.Neutral)
            //    return true;

            if (ReputationSystem.Instance.HasReputation(pid1, pid2) 
                && ReputationSystem.Instance.GetReputationLevel(pid1, pid2) >= ReputationSystem.ReputationLevel.Neutral)
                return false;

            //lw:声望为Neutral时：目标攻击该阵营的，便还击
            //声望小于Neutral时：一见到目标便攻击
            //声望大于Neutral时：两者之间不能互相攻击
            //if (ReputationSystem.Instance.HasReputation(pid2, pid1)
            //   && ReputationSystem.Instance.GetReputationLevel(pid2, pid1) == ReputationSystem.ReputationLevel.Neutral)
            //    return true;

            if (ReputationSystem.Instance.HasReputation(pid2, pid1) 
                && ReputationSystem.Instance.GetReputationLevel(pid2, pid1) >= ReputationSystem.ReputationLevel.Neutral)
                return false;

            return true;
        }


		public static bool CanCordialReputation(int pid1, int pid2)
		{
			if (ReputationSystem.Instance.HasReputation(pid1, pid2) 
			    && ReputationSystem.Instance.GetReputationLevel(pid1, pid2) >= ReputationSystem.ReputationLevel.Cordial)
				return true;
			
			if (ReputationSystem.Instance.HasReputation(pid2, pid1) 
			    && ReputationSystem.Instance.GetReputationLevel(pid2, pid1) >= ReputationSystem.ReputationLevel.Cordial)
				return true;
			
			return false;
		}

		public static bool CanDamage(SkEntity e1, SkEntity e2)
		{
			if(null == e1 || null == e2)
				return true;

			Pathea.Projectile.SkProjectile p = e1 as Pathea.Projectile.SkProjectile;
			if(p != null)
			{
				SkEntity caster = p.GetSkEntityCaster();
				if (caster == null)
					return false;
				else
					e1 = p.GetSkEntityCaster();
			}
			
			int p1 = System.Convert.ToInt32(e1.GetAttribute((int)Pathea.AttribType.DefaultPlayerID));
			int p2 = System.Convert.ToInt32(e2.GetAttribute((int)Pathea.AttribType.DefaultPlayerID));
			
			int d1 = System.Convert.ToInt32(e1.GetAttribute((int)Pathea.AttribType.DamageID));
			int d2 = System.Convert.ToInt32(e2.GetAttribute((int)Pathea.AttribType.DamageID));			
			
			return PETools.PEUtil.CanDamageReputation(p1, p2) && ForceSetting.Instance.Conflict(p1, p2) && Pathea.DamageData.GetValue(d1, d2) != 0;
		}

        public static bool CanDamageReputation(PeEntity e1, PeEntity e2)
        {
            if (e1 == null || e2 == null)
                return false;

            int pid1 = (int)e1.GetAttribute(AttribType.DefaultPlayerID);
            int pid2 = (int)e2.GetAttribute(AttribType.DefaultPlayerID);

            return CanDamageReputation(pid1, pid2);
        }

        public static bool CanDamageReputation(int pid1, int pid2)
        {
            //lz-2017.05.08 历险模式声望值高于Neutral也可以攻击

            if (ReputationSystem.Instance.HasReputation(pid1, pid2) 
                &&(!PeGameMgr.IsAdventure && ReputationSystem.Instance.GetReputationLevel(pid1, pid2) > ReputationSystem.ReputationLevel.Neutral))
                return false;

            if (ReputationSystem.Instance.HasReputation(pid2, pid1) 
                && (!PeGameMgr.IsAdventure && ReputationSystem.Instance.GetReputationLevel(pid2, pid1) > ReputationSystem.ReputationLevel.Neutral))
                return false;

            return true;
        }

        public static bool CanAttack(PeEntity e1, PeEntity e2)
        {
            if (e1 == null || e2 == null)
                return false;

            if (!CanAttackReputation(e1, e2))
                return false;

            int pid1 = (int)e1.GetAttribute(AttribType.DefaultPlayerID);
            int pid2 = (int)e2.GetAttribute(AttribType.DefaultPlayerID);

            int cid1 = (int)e1.GetAttribute (AttribType.CampID);
            int cid2 = (int)e2.GetAttribute (AttribType.CampID);

            return ForceSetting.Instance.Conflict(pid1, pid2) && Mathf.Abs(ThreatData.GetInitData(cid1, cid2)) > PETools.PEMath.Epsilon;
        }

        public static bool IsBlocked(PeEntity e1, PeEntity e2)
        {
            Vector3 v1 = e1.centerTop;
			Vector3 v2 = e2.centerPos; //e2.centerBone != null ? e2.centerBone.position :

			int selfLayer = e1.biologyViewCmpt != null && e1.biologyViewCmpt.monoModelCtrlr != null ? e1.biologyViewCmpt.monoModelCtrlr.gameObject.layer : -1;

            RaycastHit[] hitInfos = Physics.RaycastAll(v1, v2 - v1, Vector3.Distance(v1, v2));
            for (int i = 0; i < hitInfos.Length; i++)
            {
                if (hitInfos[i].collider.isTrigger)
                    continue;

				if(selfLayer == Layer.Player  && selfLayer == hitInfos[i].transform.gameObject.layer)
					continue;

                if (hitInfos[i].collider.gameObject.layer == Layer.Water)
                    continue;

                if (hitInfos[i].transform.IsChildOf(e1.transform))
                    continue;

                if (hitInfos[i].transform.IsChildOf(e2.transform))
                    continue;

                if(e2.carrier != null)
                {
                    bool isPassenger = false;
                    e2.carrier.ForeachPassenger((PESkEntity passenger, bool isDriver) =>
                    {
                        if (hitInfos[i].transform.IsChildOf(passenger.transform))
                            isPassenger = true;
                    });

                    if (isPassenger)
                        continue;
                }

                return true;
            }

            return false;
        }

        public static bool IsBlocked(PeEntity e, Vector3 position)
        {
			int selfLayer = e.biologyViewCmpt != null && e.biologyViewCmpt.monoModelCtrlr != null ? e.biologyViewCmpt.monoModelCtrlr.gameObject.layer : -1;
           
            RaycastHit[] hitInfos = Physics.RaycastAll(e.centerTop, position - e.centerTop, Vector3.Distance(e.centerTop, position));
            for (int i = 0; i < hitInfos.Length; i++)
            {
                if (hitInfos[i].collider.isTrigger)
                    continue;

                if (selfLayer == Layer.Player && selfLayer == hitInfos[i].transform.gameObject.layer)
					continue;

                if (hitInfos[i].collider.gameObject.layer == Layer.Water)
                    continue;

                if (hitInfos[i].transform.IsChildOf(e.transform))
                    continue;

                return true;
            }

            return false;
        }

        public static bool IsNpcsuperposition(PeEntity npc,Enemy enemy)
        {
            if (npc.NpcCmpt == null || enemy.entityTarget.target == null)
                return false;

            Bounds npcBounds = new Bounds(npc.position, npc.peTrans.bound.size); 
            List<PeEntity> melees = enemy.entityTarget.target.GetMelees();
            for (int i = 0; i < melees.Count;i++ )
            {
                if (melees[i].Equals(npc))
                    continue;

                Bounds other = new Bounds(melees[i].position, melees[i].peTrans.bound.size);
                if (npcBounds.Intersects(other))
                    return true;
            }
            return false;
        }

        public static bool IsNpcsuperposition(Vector3 dirPos, Enemy enemy)
        {
            if (enemy.entityTarget.target == null)
                return false;

            List<PeEntity> melees = enemy.entityTarget.target.GetMelees();
            for (int i = 0; i < melees.Count; i++)
            {
                Bounds other = new Bounds(melees[i].position, melees[i].peTrans.bound.size);
                if (other.Contains(dirPos))
                    return true;
            }
            return false;
        }

        public static bool IsInAstarGrid(Vector3 position)
        {
            if (AstarPath.active == null)
                return false;
            else
            {
                int n = AstarPath.active.graphs.Length;
                for (int i = 0; i < n; i++)
                {
                    Pathfinding.GridGraph grid = AstarPath.active.graphs[i] as Pathfinding.GridGraph;
                    if(grid != null)
                    {
                        float dx = Mathf.Abs(position.x - grid.center.x);
                        float dz = Mathf.Abs(position.z - grid.center.z);

                        if (dx > grid.nodeSize * grid.width ||
                            dz > grid.nodeSize * grid.depth)
                            return false;
                    }
                }

                return true;
            }
        }

        public static bool IsDamageBlock(PeEntity peEntity)
        {
            if (peEntity.maxRadius > 2f || peEntity.Stucking(1.0f))
            {
                Vector3 point1 = peEntity.position;
                Vector3 point2 = peEntity.position + Vector3.up*peEntity.maxHeight;
                float radius = peEntity.bounds.extents.x;
                float maxDistance = Mathf.Max(0.0f, peEntity.bounds.extents.z - peEntity.bounds.extents.x) + 0.5f;
                RaycastHit[] hitInfos = Physics.CapsuleCastAll(point1, point2, radius, peEntity.tr.forward, maxDistance, PEConfig.TerrainLayer);
                for (int i = 0; i < hitInfos.Length; i++)
                {
                    if (hitInfos[i].collider.name.StartsWith("b45Chnk"))
                        return true;
                }
            }

            return false;
        }

        public static bool IsUnderBlock(PeEntity peEntity)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(peEntity.position, Vector3.up, out hitInfo, 128.0f, Standlayer))            
                return true;

            return false;
        }

        public static bool IsForwardBlock(PeEntity peEntity, Vector3 dir, float distance,float minAngle = 15.0f)
        {
            RaycastHit hitInfo;
           
            for (int i = -2; i < 3; i++)
            {
                //Vector3 n = Vector3.Cross(dir,Vector3.up);
                Vector3 dir0 = Quaternion.AngleAxis(minAngle *i, Vector3.up) * dir;

                Debug.DrawRay(peEntity.position + Vector3.up, dir0 * distance, Color.white);
                if (Physics.Raycast(peEntity.position + Vector3.up, dir0, out hitInfo, distance, Standlayer))
                    return true;

                //Debug.DrawRay(peEntity.position + Vector3.up, dir0 * distance, Color.white);
                //if (Physics.Raycast(peEntity.position + 0.25f * Vector3.up, dir0, out hitInfo, 0.5f, Standlayer))
                //    return true;

                //Debug.DrawRay(peEntity.position + Vector3.up, dir0 * distance, Color.white);
                //if (Physics.Raycast(peEntity.position + 0.5f * Vector3.up, dir0, out hitInfo, 0.5f, Standlayer))
                //    return true; 
            }
            return false;
        }


        public static SkillSystem.SkEntity GetCaster(SkillSystem.SkEntity caster)
        {
            SkillSystem.SkEntity skCaster = caster;

            Pathea.Projectile.SkProjectile pro = skCaster as Pathea.Projectile.SkProjectile;
            if (pro != null)
                skCaster = pro.GetSkEntityCaster();

            WhiteCat.CreationSkEntity creation = skCaster as WhiteCat.CreationSkEntity;
            if (creation != null && creation.driver != null)
                skCaster = creation.driver.skEntity;

            return skCaster;
        }

		public static T GetCmpt<T>(Transform root) where T : Component
		{
			return (T)root.TraverseHierarchySerial ((t, ddddd) => {
				return t.GetComponent<T> ();});
		}

		public static T[] GetCmpts<T>(Transform root) where T : Component
		{
			System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T> (4);
			
			root.TraverseHierarchySerial ((t, ddddd) =>
			                              {
				var c = t.GetComponent<T> ();
				if (c)list.Add(c);
			});
			
			return list.ToArray ();
		}
		
		public static T[] GetAllCmpts<T>(Transform root) where T : Component
		{
			System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>(4);
			
			root.TraverseHierarchy((t, ddddd) =>
			                       {
				T[] c = t.GetComponents<T>();
				if (c != null && c.Length > 0) list.AddRange(c);
			});
			
			return list.ToArray();
		}

		public static bool IsChildItemType(int seldTypeID, int parentTypeID)
		{
			if(0 == seldTypeID || 0 == parentTypeID || seldTypeID == parentTypeID)
				return true;
			ItemProto.Mgr.ItemEditorType editorType = ItemProto.Mgr.Instance.GetEditorType(seldTypeID);
			if(null == editorType || 0 == editorType.parentID)
				return false;
			else if(parentTypeID == editorType.parentID)
				return true;
			return IsChildItemType(editorType.parentID, parentTypeID);
		}

        public static bool InRange(Vector3 centor,Vector3 targetPos,float radius,bool Is3D = true)
        {
            float sqrDistanceH = PEUtil.Magnitude(centor, targetPos, Is3D);
            return sqrDistanceH < radius;
        }

		public static GlobalTreeInfo RayCastTree(Ray ray, float distance)
		{
			GlobalTreeInfo gTreeinfo = null;
			if(null != LSubTerrainMgr.Instance)
			{
				gTreeinfo = LSubTerrainMgr.RayCast(ray, distance);
			}
			else if(null != RSubTerrainMgr.Instance)
			{
				TreeInfo findTree = RSubTerrainMgr.RayCast(ray, distance);
				if(null != findTree)
					gTreeinfo = new GlobalTreeInfo(-1, findTree);
			}
			return gTreeinfo;
		}

		public static GlobalTreeInfo RayCastTree(Vector3 origin, Vector3 dir, float distance)
		{
			return RayCastTree(new Ray(origin, dir), distance);
		}

		public static GlobalTreeInfo GetTreeinfo(Collider col)
		{
			if(null == col)
				return null;
			
			GlobalTreeInfo gTreeinfo = null;
			if(null != LSubTerrainMgr.Instance)
			{
				gTreeinfo = LSubTerrainMgr.GetTreeinfo(col);
			}
			else if(null != RSubTerrainMgr.Instance)
			{
				TreeInfo findTree = RSubTerrainMgr.GetTreeinfo(col);
				if(null != findTree)
					gTreeinfo = new GlobalTreeInfo(-1, findTree);
			}
			return gTreeinfo;
		}

		
		public static bool IsVoxelOrBlock45(SkEntity entity)
		{
			return entity is VFVoxelTerrain || entity is Block45Man;
		}

        public static bool RagdollTranlate(PeEntity entity, Vector3 pos)
        {
            if(entity != null && entity.lodCmpt != null && pos != Vector3.zero)
            {
                entity.lodCmpt.DestroyView();
                entity.ExtSetPos(pos);
                SceneMan.SetDirty(entity.lodCmpt);
                return true;
            }
            return false;
        }

	}
}


