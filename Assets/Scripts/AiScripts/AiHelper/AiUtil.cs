using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class AiUtil
{
	public static bool GetNearNodePosWalkable(Vector3 pos, out Vector3 outPos)
	{
        outPos = pos;
        return true;
        //if (AstarPath.active != null) {
        //    Pathfinding.GraphNode node = AstarPath.active.GetNearest (pos, Pathfinding.PathNNConstraint.Default, null).node;
        //    if(node != null){
        //        outPos = (Vector3)node.position;
        //        return true;
        //    }
        //}
		
		//return false;
	}

	public static IntVector2 ConvertToIntVector2FormLodLevel(IntVector4 node, int lodLevel)
	{
		int mask = lodLevel + VoxelTerrainConstants._shift;
		int x = (node.x >> mask) << mask;
		int z = (node.z >> mask) << mask;
		return new IntVector2(x, z);
	}

    public static IntVector4 ConvertToIntVector4(Vector3 position, int lod)
    {
        IntVector4 intVec4 = new IntVector4(new IntVector3(position), lod);
        
        int mask = lod + VoxelTerrainConstants._shift;
        intVec4.x = (intVec4.x >> mask) << mask;
        intVec4.y = (intVec4.y >> mask) << mask;
        intVec4.z = (intVec4.z >> mask) << mask;

        return intVec4;
    }

    public static int GetHarm(GameObject go)
    {
        //if (go == null)
        //    return -1;

        //Projectile projectile = go.GetComponent<Projectile>();
        //if (projectile != null && projectile.emitRunner != null)
        //    go = projectile.emitRunner.gameObject;

        //Equipment equipment = go.GetComponent<Equipment>();
        //if (equipment != null && equipment.mSkillRunner != null)
        //    go = equipment.mSkillRunner.gameObject;

        //Player player = go.GetComponent<Player>();
        //CreationSkillRunner creation = VCUtils.GetComponentOrOnParent<CreationSkillRunner>(go);
        ////BuildingRunnerB br = go.GetComponent<BuildingRunnerB>();
        //ColonyRunner cr = go.GetComponent<ColonyRunner>();

        //if (player != null || creation != null || cr != null)
        //{
        //    if (!GameConfig.IsMultiMode)
        //        return AiAsset.AiHarmData.PlayerHarm;
        //    else
        //    {
        //        if (player != null)
        //            return player.TeamId;
        //        else if (creation != null)
        //            return creation.TeamId;
        //        else
        //            return AiAsset.AiHarmData.PlayerHarm;
        //    }
        //}

        //AiObject ai = VCUtils.GetComponentOrOnParent<AiObject>(go);
        //if (ai != null) 
        //    return ai.harm;

        ////Debug.LogError("Can't find harm : " + go.name);
        return -1;
    }

    public static int GetCamp(GameObject go)
    {
        //Projectile projectile = go.GetComponent<Projectile>();
        //if (projectile != null && projectile.emitRunner != null)
        //    go = projectile.emitRunner.gameObject;

        //Equipment equipment = go.GetComponent<Equipment>();
        //if (equipment != null && equipment.mSkillRunner != null)
        //{
        //    go = equipment.mSkillRunner.gameObject;
        //}

        //Player player = go.GetComponent<Player>();
        //CreationSkillRunner creation = VCUtils.GetComponentOrOnParent<CreationSkillRunner>(go);
        //BuildingRunnerB br = go.GetComponent<BuildingRunnerB>();
        //ColonyRunner cr = go.GetComponent<ColonyRunner>();

        //if (player != null || creation != null || br != null || cr != null)
        //{
        //    if (!GameConfig.IsMultiMode)
        //        return AiAsset.AiHatredData.PlayerCamp;
        //    else
        //    {
        //        if (player != null)
        //            return player.TeamId;
        //        else if (creation != null)
        //            return creation.TeamId;
        //        else
        //            return AiAsset.AiHatredData.PlayerCamp;
        //    }
        //}

        //AiObject ai = VCUtils.GetComponentOrOnParent<AiObject>(go);
        //if (ai != null)
        //    return ai.camp;

        //Debug.LogError("Can't find camp : " + go.name);
        return -1;
    }

	public static Bounds GetLocalBounds(Collider collider)
	{
		if(collider == null)
			return new Bounds();

		Bounds localBound = collider.bounds;
		localBound.center = collider.transform.InverseTransformPoint(collider.bounds.center);
		return localBound;
	}

    public static Vector3 GetColliderCenter(Collider collider)
    {
        if (collider == null)
            return Vector3.zero;

        Vector3 center = Vector3.zero;
        if (collider is CapsuleCollider)
            center = (collider as CapsuleCollider).center;
        else if (collider is SphereCollider)
            center = (collider as SphereCollider).center;
        else if (collider is BoxCollider)
            center = (collider as BoxCollider).center;
        else if (collider is CharacterController)
            center = (collider as CharacterController).center;
		else if(collider is MeshCollider)
			return (collider as MeshCollider).bounds.center;
        else
            Debug.LogError("collider is error!!");

        return collider.transform.TransformPoint(center);
    }
	
	public static float GetColliderRadius(Collider collider)
    {
        if (collider == null)
            return 0.0f;

        float radius = 0.0f;
        if (collider is CapsuleCollider)
		{
			CapsuleCollider cap = collider as CapsuleCollider;
			if(cap.direction == 0)
				radius = cap.height * 0.5f;
			else if(cap.direction == 1)
				radius = cap.radius;
			else
				radius = cap.height * 0.5f;
		}
        else if (collider is SphereCollider)
            radius = (collider as SphereCollider).radius;
        else if (collider is BoxCollider)
            radius = (collider as BoxCollider).size.z * 0.5f;
        else if (collider is CharacterController)
            radius = (collider as CharacterController).radius;
		else if(collider is MeshCollider)
			radius = (collider as MeshCollider).bounds.extents.z;
        else
            Debug.LogError("collider is error!!");

        return radius;
    }
	
	public static float GetColliderHeight(Collider collider)
    {
        if (collider == null)
            return 0.0f;

        float height = 0.0f;
        if (collider is CapsuleCollider)
		{
			CapsuleCollider cap = collider as CapsuleCollider;
			if(cap.direction == 0)
				height = cap.radius * 2f;
			else if(cap.direction == 1)
				height = cap.height;
			else
				height = cap.radius * 2f;
		}
        else if (collider is SphereCollider)
            height = (collider as SphereCollider).radius;
        else if (collider is BoxCollider)
            height = (collider as BoxCollider).size.y;
        else if (collider is CharacterController)
            height = (collider as CharacterController).height;
		else if(collider is MeshCollider)
			height = (collider as MeshCollider).bounds.extents.y;
        else
            Debug.LogError("collider is error!!");

        return height;
    }

    public static float GetColliderSide(Collider collider)
    {
        if (collider == null)
            return 0.0f;

        float side = 0.0f;
        if (collider is CapsuleCollider)
        {
            CapsuleCollider cap = collider as CapsuleCollider;
            if (cap.direction == 0)
                side = cap.height * 0.5f;
            else if (cap.direction == 1)
                side = cap.radius;
            else
                side = cap.height * 0.5f;
        }
        else if (collider is SphereCollider)
            side = (collider as SphereCollider).radius;
        else if (collider is BoxCollider)
            side = (collider as BoxCollider).size.x * 0.5f;
        else if (collider is CharacterController)
            side = (collider as CharacterController).radius;
        else
            Debug.LogError("collider is error!!");

        return side;
    }

	public static float Angle3D(Vector3 from, Vector3 to)
	{
		return Vector3.Angle(from, to);
	}

	public static float Angle2D(Vector3 from, Vector3 to)
	{
		Vector3 vf = new Vector3(from.x, 0.0f, from.z);
		Vector3 vt = new Vector3(to.x, 0.0f, to.z);

		return Vector3.Angle(vf, vt);
	}

    public static float MagnitudeH(Vector3 distance)
    {
        distance.y = 0;
        return distance.magnitude;
    }

    public static float SqrMagnitudeH(Vector3 distance)
    {
        distance.y = 0;
        return distance.sqrMagnitude;
    }

    public static float SqrMagnitude(Vector3 distance)
    {
        return distance.sqrMagnitude;
    }

    public static float DistanceXZ(Vector3 v1, Vector3 v2)
    {
        v1.y = v2.y;
        return Vector3.Distance(v1, v2);
    }

    public static float DistanceYABS(Vector3 v1, Vector3 v2)
    {
        return Mathf.Abs(v1.y - v2.y);
    }

    public static float DotH(Vector3 vector1, Vector3 vector2)
    {
        Vector3 v1 = new Vector3(vector1.x, 0.0f, vector1.z);
        Vector3 v2 = new Vector3(vector2.x, 0.0f, vector2.z);

        return Vector3.Dot(v1.normalized, v2.normalized);
    }

    public static float DotV(Vector3 vector)
    {
        Vector3 v1 = new Vector3(vector.x, 0.0f, vector.z);

        return Vector3.Dot(v1.normalized, vector.normalized);
    }

    public static float Angle(Vector3 v1, Vector3 v2)
    {
        if (v1 == Vector3.zero || v2 == Vector3.zero)
            return 0.0f;

        return Vector3.Angle(v1, v2);
    }

    public static float AngleXZ(Vector3 v1, Vector3 v2)
    {
        v1.y = 0.0f;
        v2.y = 0.0f;

        if (v1 == Vector3.zero || v2 == Vector3.zero)
            return 0.0f;

        return Vector3.Angle(v1, v2);
    }

    public static Vector3 GetNextPosition(Vector3 v, Vector3 velocity, float height)
    {
        LayerMask layer = 1 << Pathea.Layer.VFVoxelTerrain |
                          1 << Pathea.Layer.SceneStatic |
							1 << Pathea.Layer.Building |
                          1 << Pathea.Layer.Unwalkable;

        Ray rayStart = new Ray(v + velocity + Vector3.up * height, -Vector3.up);

        RaycastHit hitInfo;
        if (Physics.Raycast(rayStart, out hitInfo, height * 2, layer))
        {
            return hitInfo.point;
        }

        return Vector3.zero;
    }

    public static Vector3 GetNextPosition(Vector3 v, Vector3 velocity, float upHeight, float lowHeight)
    {
        LayerMask layer = 1 << Pathea.Layer.VFVoxelTerrain |
                          1 << Pathea.Layer.SceneStatic |
                          1 << Pathea.Layer.Unwalkable;

        Ray rayStart = new Ray(v + velocity + Vector3.up * upHeight, -Vector3.up);

        RaycastHit hitInfo;
        if (Physics.Raycast(rayStart, out hitInfo, upHeight + lowHeight, layer))
        {
            return hitInfo.point;
        }

        return Vector3.zero;
    }


    public static LayerMask obstructLayer = LayerMask.NameToLayer("");
	public static LayerMask groundedLayer = SceneMan.DependenceLayer;
	public static LayerMask voxelLayer = 1<<Pathea.Layer.VFVoxelTerrain;
    public static Vector3 GetRandomPositionInCave(Vector3 center, float minRadius, float maxRadius, LayerMask layer, int attempt)
    {
        int att = Mathf.Clamp(attempt, 1, 100);

        for (int i = 0; i < att; i++)
        {
            Vector2 v2 = Random.insideUnitCircle;
            v2 = v2.normalized * Random.Range(minRadius, maxRadius);

            Vector3 v3 = center + new Vector3(v2.x, 0.0f, v2.y);

            if (CheckPositionInCave(ref v3, 128.0f, voxelLayer))
            {
                return v3;
            }
        }

        return Vector3.zero;
    }

	public static Vector3 GetRandomPositionInCave(IntVector4 node, int attempt)
	{
		int att = Mathf.Clamp(attempt, 1, 100);
		
		for (int i = 0; i < att; i++)
		{
			Vector3 position = node.ToVector3();
			position += new Vector3(Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w),
			                        0.0f,
			                        Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w));

            RaycastHit hitInfo;
			if(AiUtil.CheckPositionOnGround(position, out hitInfo, 0.0f, 
			                                VoxelTerrainConstants._numVoxelsPerAxis << node.w, 
			                                1 << Pathea.Layer.VFVoxelTerrain) 
                && CheckSlopeValid(hitInfo.normal, 45))
			{
                if (AiUtil.CheckPositionInCave(hitInfo.point, 128.0f, voxelLayer))
				{
					return hitInfo.point;
				}
			}
		}

		return Vector3.zero;
	}

    public static Vector3 GetRandomPosition(IntVector4 node)
    {
        Vector3 position = node.ToVector3();
        position += new Vector3(Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w),
                    0.0f,
                    Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w));
        return position;
    }

    public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius)
    {
        Vector2 v2 = Random.insideUnitCircle;
        v2 = v2.normalized * Random.Range(minRadius, maxRadius);

        return center + new Vector3(v2.x, 0.0f, v2.y);
    }

    public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, Vector3 direction, float minAngle, float maxAngle)
    {
        direction.y = 0.0f;

        Vector3 _dir = Quaternion.AngleAxis(Random.Range(minAngle, maxAngle), Vector3.up) * direction;

        _dir = _dir.normalized * Random.Range(minRadius, maxRadius);

        return center + _dir;
    }

    public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius, float height, LayerMask layer, int attempt)
    {
        int att = Mathf.Clamp(attempt, 1, 100);

        for (int i = 0; i < att; i++)
        {
            Vector2 v2 = Random.insideUnitCircle;
            v2 = v2.normalized * Random.Range(minRadius, maxRadius);

            Vector3 v3 = center + new Vector3(v2.x, 0.0f, v2.y);

            RaycastHit hitInfo;
            if (Physics.Raycast(v3 + Vector3.up * height, -Vector3.up, out hitInfo, height * 2, layer))
            {
                if (CheckSlopeValid(hitInfo.normal, 45) && GetVoxelType(hitInfo) >= 0)
                {
                    return hitInfo.point;
                }
            }
        }

        return Vector3.zero;
    }

    public static Vector3 GetRandomPosition(Vector3 center, float minRadius, float maxRadius,
        Vector3 direction, float minAngle, float maxAngle, float height, LayerMask layer, int attempt)
    {
        int att = Mathf.Clamp(attempt, 1, 100);

        for (int i = 0; i < att; i++)
        {
            direction.y = 0.0f;

            Vector3 _dir = Quaternion.AngleAxis(Random.Range(minAngle, maxAngle), Vector3.up) * direction;

            _dir = _dir.normalized * Random.Range(minRadius, maxRadius);

            Vector3 v3 = center + _dir;

            RaycastHit hitInfo;
            if (Physics.Raycast(v3 + Vector3.up * height, -Vector3.up, out hitInfo, height * 2, layer))
            {
                if (CheckSlopeValid(hitInfo.normal, 45))
                {
                    return hitInfo.point;
                }
            }
        }

        return Vector3.zero;
    }

    public static Vector3 GetRandomPositionInLand(Vector3 center, float minRadius, float maxRadius, float height, LayerMask layer, int attempt)
    {
        int att = Mathf.Clamp(attempt, 1, 100);

        for (int i = 0; i < att; i++)
        {
            Vector2 v2 = Random.insideUnitCircle;
            v2 = v2.normalized * Random.Range(minRadius, maxRadius);

            Vector3 v3 = center + new Vector3(v2.x, 0.0f, v2.y);

            RaycastHit hitInfo;
            if (Physics.Raycast(v3 + Vector3.up * height, -Vector3.up, out hitInfo, height * 2, layer))
            {
                if (!CheckPositionUnderWater(hitInfo.point))
                {
                    return hitInfo.point;
                }
            }
        }

        return Vector3.zero;
    }

    public static Vector3 GetRandomPositionInLand(Vector3 center, float minRadius, float maxRadius,
    Vector3 direction, float minAngle, float maxAngle, float height, LayerMask layer, int attempt)
    {
        int att = Mathf.Clamp(attempt, 1, 100);

        for (int i = 0; i < att; i++)
        {
            direction.y = 0.0f;

            Vector3 _dir = Quaternion.AngleAxis(Random.Range(minAngle, maxAngle), Vector3.up) * direction;

            _dir = _dir.normalized * Random.Range(minRadius, maxRadius);

            Vector3 v3 = center + _dir;

            RaycastHit hitInfo;
            if (Physics.Raycast(v3 + Vector3.up * height, -Vector3.up, out hitInfo, height * 2, layer))
            {
                if (!CheckPositionUnderWater(hitInfo.point))
                {
                    return hitInfo.point;
                }
            }
        }

        return Vector3.zero;
    }

    public static Vector3 GetRandomPositionInWater( Vector3 center, float minHeight, float maxHeight, 
                                                    float minRadius, float maxRadius, int attempt)
    {
        int att = Mathf.Clamp(attempt, 1, 100);

        float riverHeight;
        if (CheckPositionUnderWater(center, out riverHeight))
        {
            for (int i = 0; i < att; i++)
            {
                float bottom = riverHeight;

                Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
                Vector3 pos = center + new Vector3(offset.x, 0.0f, offset.y);

                RaycastHit hitInfo;
                if (Physics.Raycast(pos + Vector3.up * (riverHeight - pos.y), Vector3.down, out hitInfo, 512.0f, voxelLayer))
                {
                    bottom = hitInfo.point.y;
                }

                float up = riverHeight - minHeight;
                float down = Mathf.Max(bottom, riverHeight - maxHeight);

                if (up <= PETools.PEMath.Epsilon || up <= down)
                    continue;

                float heightOffset = Random.Range(down, up);

                Vector3 patrolPosition = new Vector3(pos.x, heightOffset, pos.z);

                if (CheckPositionUnderWater(patrolPosition))
                    return patrolPosition;
            }
        }

        return Vector3.zero;
    }

    public static Vector3 GetRandomPositionInWater( Vector3 center, float minHeight, float maxHeight,
                                                    Vector3 direction, float minAngle, float maxAngle,
                                                    float minRadius, float maxRadius, int attempt)
    {
        int att = Mathf.Clamp(attempt, 1, 100);

        float riverHeight;
        if (CheckPositionUnderWater(center, out riverHeight))
        {
            for (int i = 0; i < att; i++)
            {
                float bottom = riverHeight;

                direction.y = 0.0f;
                Vector3 _dir = Quaternion.AngleAxis(Random.Range(minAngle, maxAngle), Vector3.up) * direction;

                Vector2 offset = _dir.normalized * Random.Range(minRadius, maxRadius);
                Vector3 pos = center + new Vector3(offset.x, 0.0f, offset.y);

                RaycastHit hitInfo;
                if (Physics.Raycast(pos + Vector3.up * (riverHeight - pos.y), Vector3.down, out hitInfo, 512.0f, voxelLayer))
                {
                    bottom = hitInfo.point.y;
                }

                float up = riverHeight - minHeight;
                float down = Mathf.Max(bottom, riverHeight - maxHeight);

                if (up <= PETools.PEMath.Epsilon || up <= down)
                    continue;

                float heightOffset = Random.Range(down, up);

                Vector3 patrolPosition = new Vector3(pos.x, heightOffset, pos.z);

                if (CheckPositionUnderWater(patrolPosition))
                    return patrolPosition;
            }
        }

        return Vector3.zero;
    }

    public static Vector3 GetRandomPositionInSky(Vector3 center, float minHeight, float maxHeight,
        float minRadius, float maxRadius, int attempt)
    {
        int att = Mathf.Clamp(attempt, 1, 100);

        for (int i = 0; i < att; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
            Vector3 pos = center + new Vector3(offset.x, Random.Range(minHeight, maxHeight), offset.y);

            if (CheckPositionUnderWater(pos))
                continue;

            if (CheckPositionInCave(pos, 128, voxelLayer))
                continue;

            return pos;
        }

        return Vector3.zero;
    }

    public static Vector3 GetRandomPositionInSky(Vector3 center, float minHeight, float maxHeight,
                                                 Vector3 direction, float minAngle, float maxAngle,
                                                 float minRadius, float maxRadius, int attempt)
    {
        int att = Mathf.Clamp(attempt, 1, 100);

        for (int i = 0; i < att; i++)
        {
            direction.y = 0.0f;
            Vector3 _dir = Quaternion.AngleAxis(Random.Range(minAngle, maxAngle), Vector3.up) * direction;
            Vector2 offset = _dir * Random.Range(minRadius, maxRadius);
            Vector3 pos = center + new Vector3(offset.x, Random.Range(minHeight, maxHeight), offset.y);

            if (CheckPositionUnderWater(pos))
                continue;

            if (CheckPositionInCave(pos, 128, voxelLayer))
                continue;

            return pos;
        }

        return Vector3.zero;
    }

    public static Vector3 ToVector3(string vecString)
    {
		if (!string.IsNullOrEmpty(vecString))
        {
            string[] vec = vecString.Split(new char[] { ',' });
            if (vec.Length == 3)
            {
                float _x = Convert.ToSingle(vec[0]);
                float _y = Convert.ToSingle(vec[1]);
                float _z = Convert.ToSingle(vec[2]);

                return new Vector3(_x, _y, _z);
            }
        }
        return Vector3.zero;
    }

    public static string[] Split(string value, char parameter)
    {
        return Split(value, new char[] { parameter });
    }

    public static string[] Split(string value, char[] parameter)
    {
        return value.Split(parameter);
    }

	public static bool CheckPositionOnGround(Vector3 position, out RaycastHit hitInfo, float height, LayerMask groundLayer)
	{
		if (Physics.Raycast(position + Vector3.up * height, -Vector3.up, out hitInfo, height * 2, groundLayer))
		{
			position = hitInfo.point;
			return true;
		}

		hitInfo = new RaycastHit();
		return false;
	}

	public static bool CheckPositionOnGround(Vector3 position, out RaycastHit hitInfo, float lowHeight, float upHeight, LayerMask groundLayer)
	{
		if (Physics.Raycast(position + Vector3.up * upHeight, -Vector3.up, out hitInfo, lowHeight + upHeight, groundLayer))
		{
			position = hitInfo.point;
			return true;
		}

		hitInfo = new RaycastHit();
		return false;
	}

	public static bool CheckPositionOnGround(Vector3 position, float height, LayerMask groundLayer)
	{
		return Physics.Raycast(position + Vector3.up * height, -Vector3.up, height * 2, groundLayer);
	}
	
	public static bool CheckPositionOnGround(ref Vector3 position, float height, LayerMask groundLayer)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(position + Vector3.up * height, -Vector3.up, out hitInfo, height * 2, groundLayer))
        {
            position = hitInfo.point;
            return true;
        }

        return false;
    }

    public static bool CheckPositionOnGround(ref Vector3 position, float lowHeight, float upHeight, LayerMask groundLayer)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(position + Vector3.up * upHeight, -Vector3.up, out hitInfo, lowHeight + upHeight, groundLayer))
        {
            position = hitInfo.point;
            return true;
        }

        return false;
    }

    public static bool CheckSlopeValid(Vector3 normal, float slopeAngle)
    {
        float angle = Vector3.Dot(normal.normalized, Vector3.up);

        float cosAngle = Mathf.Cos(slopeAngle * Mathf.Deg2Rad);

        return angle > cosAngle;
    }

    public static bool CheckPositionOnGround(ref Vector3 position, out Vector3 normal, float lowHeight, float upHeight, LayerMask groundLayer)
    {
        normal = Vector3.zero;

        RaycastHit hitInfo;
        if (Physics.Raycast(position + Vector3.up * upHeight, -Vector3.up, out hitInfo, lowHeight + upHeight, groundLayer))
        {
            position = hitInfo.point;
            normal = hitInfo.normal;
            return true;
        }

        return false;
    }

    public static bool CheckTransformInCave(Transform tr)
    {
        if (tr == null)
            return false;
		else
			return CheckPositionInCave(GetColliderCenter(tr.GetComponent<Collider>()), 256, voxelLayer);
    }

	public static bool CheckPositionInCave(Vector3 position, float distance, LayerMask layer)
	{
		int count = 0;
		int verticalNumber = 4;
		int horizontalNumber = 8;
		float coverage = 0.7f;
		float upDeviationAngle = 60f;
		
		RaycastHit hitInfo;
		for(int i = 0; i < verticalNumber; i++)
		{
			for (int j = 0; j < horizontalNumber; j++) 
			{
				Vector3 rayDirection = Quaternion.AngleAxis(360 * j / (float)horizontalNumber, Vector3.up) * Vector3.forward;
//				Vector3 axis = Vector3.Cross(rayDirection, Vector3.up);
//				rayDirection = Quaternion.AngleAxis(88 * i / (float)verticalNumber, axis) * rayDirection;
				rayDirection = Vector3.Slerp(Vector3.up, rayDirection, i * upDeviationAngle / verticalNumber / 90f);
				
				//Debug.DrawRay(position, rayDirection.normalized * 5, Color.red);
				if (Physics.Raycast(position, rayDirection, out hitInfo, distance, layer))
				{
					count++;
				}
			}
		}

		if(count >= verticalNumber * horizontalNumber * coverage)
			return true;
		else
			return false;
	}

    public static bool CheckPositionInCave(ref Vector3 position, float distance, LayerMask layer)
    {
		if(CheckPositionInCave(position, distance, layer))
		{
			RaycastHit hitInfo;
			if(Physics.Raycast(position, Vector3.up, out hitInfo, distance, layer))
			{
				if(Physics.Raycast(hitInfo.point, -Vector3.up, out hitInfo, distance, layer))
				{
					position = hitInfo.point;
				}
			}
			else
			{
				if(Physics.Raycast(position + Vector3.up * distance, -Vector3.up, out hitInfo, 2 * distance, layer))
				{
					position = hitInfo.point;
				}
			}

			return true;
		}

		return false;
    }

    public static bool CheckPositionInCave(Vector3 position, out Vector3 point)
    {
        point = Vector3.zero;

        RaycastHit hitInfo;
        if (Physics.Raycast(position, Vector3.up, out hitInfo, 256.0f, 1 << Pathea.Layer.VFVoxelTerrain))
        {
            point = hitInfo.point;
            return true;
        }

        return false;
    }

    public static bool CheckPositionUnderWater(Vector3 v)
    {
        if (VFVoxelWater.self != null)
        {
            return VFVoxelWater.self.IsInWater(v.x, v.y + 0.5f, v.z);
        }
        return false;
    }

    public static bool CheckPositionUnderWater(Vector3 v, out float waterHeight)
    {
        if (VFVoxelWater.self != null)
        {
            float height = VFVoxelWater.self.UpToWaterSurface(v.x, v.y + 0.5f, v.z);
            if (height > PETools.PEMath.Epsilon)
            {
                waterHeight = height + v.y + 0.5f;
                return true;
            }
        }

        waterHeight = 0.0f;
        return false;
    }

    public static LightUnit CheckPositionInLightRange(Vector3 position)
    {
        if (LightMgr.Instance != null)
        {
            foreach (LightUnit light in LightMgr.Instance.lights)
            {
                if (light == null || light.lamp == null)
                    continue;

				if(light.lamp.type == LightType.Point)
				{
					float sqrDistance = AiUtil.SqrMagnitude(light.transform.position - position);
					if (sqrDistance <= light.lamp.range * light.lamp.range)
						return light;
				}
				else if (light.lamp.type == LightType.Spot)
				{
					float sqrDistance = AiUtil.SqrMagnitude(light.transform.position - position);
					if (sqrDistance <= light.lamp.range * light.lamp.range)
	                {
						float angle = Vector3.Angle(light.lamp.transform.forward, position - light.lamp.transform.position);
						if(angle <= light.lamp.spotAngle)
							return light;
	                }
				}
            }
        }

        return null;
    }

    public static bool CheckHitObstacle(Collider collider, Vector3 pos, LayerMask layer)
    {
        if (pos == Vector3.zero) return false;
        if (collider == null) return true;

        if (collider is CharacterController)
        {
            CharacterController ctrl = collider as CharacterController;
            return !Physics.CheckCapsule(pos, pos + Vector3.up * ctrl.height, ctrl.radius + 0.5f, layer);
        }
        else if (collider is CapsuleCollider)
        {
            CapsuleCollider cc = collider as CapsuleCollider;
            switch (cc.direction)
            {
                case 0: return !Physics.CheckCapsule(pos, pos + Vector3.up * cc.height, cc.radius + 0.5f, layer);
                case 1: return !Physics.CheckCapsule(pos, pos + Vector3.up * cc.height, cc.radius + 0.5f, layer);
                case 2: return !Physics.CheckCapsule(pos, pos + Vector3.up * cc.height, cc.radius + 0.5f, layer);
                default: return true;
            }
        }

        return false;
    }

    public static bool CheckObstacle(Collider collider, LayerMask layer)
    {
        if (collider == null) return true;

        Transform tr = collider.transform;

        if (collider is CharacterController)
        {
            CharacterController ctrl = collider as CharacterController;
            return !Physics.CheckCapsule(tr.position, tr.position + Vector3.up * ctrl.height, ctrl.radius + 0.5f, layer);
        }
        else if (collider is CapsuleCollider)
        {
            CapsuleCollider cc = collider as CapsuleCollider;
            Vector3 center = tr.position + cc.center;
            switch (cc.direction)
            {
                case 0: return !Physics.CheckCapsule(center - tr.right * cc.height * 0.5f, center + tr.right * cc.height * 0.5f, cc.radius, layer);
                case 1: return !Physics.CheckCapsule(center - tr.up * cc.height * 0.5f, center + tr.up * cc.height * 0.5f, cc.radius, layer);
                case 2: return !Physics.CheckCapsule(center - tr.forward * cc.height * 0.5f, center + tr.forward * cc.height * 0.5f, cc.radius, layer);
                default: return true;
            }
        }

        return false;
    }

    public static VFVoxelChunkGo GetChunk(Vector3 position)
    {
        if (VFVoxelTerrain.self == null) return null;

        int _x = Mathf.FloorToInt(position.x);
        int _y = Mathf.FloorToInt(position.y);
        int _z = Mathf.FloorToInt(position.z);

        VFVoxelChunkData _chunkData = VFVoxelTerrain.self.Voxels.readChunk(
            _x >> VoxelTerrainConstants._shift, 
            _y >> VoxelTerrainConstants._shift, 
            _z >> VoxelTerrainConstants._shift);

        if (_chunkData == null) return null;

        return _chunkData.ChunkGo;
    }

	public static VFVoxelChunkGo GetChunk(IntVector4 intPos)
	{
		if (VFVoxelTerrain.self == null) return null;
		
		VFVoxelChunkData _chunkData = VFVoxelTerrain.self.Voxels.readChunk(
			intPos.x >> VoxelTerrainConstants._shift, 
			intPos.y >> VoxelTerrainConstants._shift, 
			intPos.z >> VoxelTerrainConstants._shift,
			intPos.w);
		
		if (_chunkData == null) return null;
		
		return _chunkData.ChunkGo;
	}

    public static Vector3 GetChunkPosMin(Vector3 position)
    {
        int _x = Mathf.FloorToInt(position.x / VoxelTerrainConstants._numVoxelsPerAxis);
        int _y = Mathf.FloorToInt(position.y / VoxelTerrainConstants._numVoxelsPerAxis);
        int _z = Mathf.FloorToInt(position.z / VoxelTerrainConstants._numVoxelsPerAxis);

        return new Vector3(_x, _y, _z);
    }

	public static IntVector3 GetChunkPosMinInt(Vector3 position)
	{
		int _x = Mathf.FloorToInt(position.x / VoxelTerrainConstants._numVoxelsPerAxis);
		int _y = Mathf.FloorToInt(position.y / VoxelTerrainConstants._numVoxelsPerAxis);
		int _z = Mathf.FloorToInt(position.z / VoxelTerrainConstants._numVoxelsPerAxis);

		return new IntVector3(_x, _y, _z);
	}

    public static Bounds GetBounds(Vector3 position)
    {
        Vector3 boundMin = GetChunkPosMin(position);
        Vector3 boundCenter = boundMin + Vector3.one * VoxelTerrainConstants._numVoxelsPerAxis * 0.5f;

        return new Bounds(boundCenter, Vector3.one * VoxelTerrainConstants._numVoxelsPerAxis);
    }

    public static Vector3 GetBoundsCenter(Vector3 position)
    {
        Vector3 boundMin = GetChunkPosMin(position);
        return boundMin + Vector3.one * VoxelTerrainConstants._numVoxelsPerAxis * 0.5f;
    }

    public static Bounds GetCustomBounds(Vector3 position, uint size)
    {
        return new Bounds(GetBoundsCenter(position), Vector3.one * VoxelTerrainConstants._numVoxelsPerAxis * size);
    }

    public static Bounds GetCustomBoundsIdx(Vector3 position, uint size)
    {
        return new Bounds(GetChunkPosMin(position), Vector3.one * size);
    }

	public static int GetDependType(Vector3 pos)
	{
		Ray ray = new Ray(pos + 0.5f * Vector3.up, Vector3.down);
		RaycastHit hitInfo;
		if(Physics.Raycast(ray, out hitInfo, 1f
		                   , (1<<Pathea.Layer.VFVoxelTerrain) | (1<<Pathea.Layer.GIEProductLayer) | (1<<Pathea.Layer.SceneStatic) | (1<<Pathea.Layer.Unwalkable)))
		{
			if(hitInfo.distance < 1f)
				return AiUtil.GetDependType(hitInfo);
		}

		return 0;
	}

	static int GetDependType(RaycastHit hitInfo)
	{
        if (hitInfo.transform == null)
            return 0;

		if(hitInfo.transform.gameObject.layer == Pathea.Layer.VFVoxelTerrain)
		{
            if (hitInfo.transform.GetComponent<B45ChunkGo>() != null)
				return 2;
			else
				return 1;
		}
		else if(hitInfo.transform.gameObject.layer == Pathea.Layer.SceneStatic)
		{
			return 3;
		}
		else if(hitInfo.transform.gameObject.layer == Pathea.Layer.Unwalkable)
		{
			return 4;
		}
        else if (hitInfo.transform.gameObject.layer == Pathea.Layer.GIEProductLayer)
        {
            return 5;
        }

		return 0;
	}

    //public static Player GetClosedPlayer(Vector3 pos)
    //{
    //    float dis = Mathf.Infinity;
    //    Player _player = null;
    //    //foreach (GameObject go in PlayerFactory.playerList)
    //    //{
    //    //    if (go == null) continue;

    //    //    Player p = go.GetComponent<Player>();

    //    //    if (p == null) continue;

    //    //    Vector3 distance = p.transform.position - pos;
    //    //    distance.y = 0.0f;

    //    //    if (distance.sqrMagnitude < dis)
    //    //    {
    //    //        _player = p;
    //    //        dis = distance.sqrMagnitude;
    //    //    }
    //    //}

    //    return _player;
    //}

    public static Transform GetChild(Transform parent, string childName)
    {
        if (childName == "")
            return null;

        foreach (Transform it in parent)
        {
            if (it.name.Equals(childName))
                return it;
            else
            {
                Transform child = GetChild(it, childName);
                if (child != null)
                {
                    return child;
                }
            }
        }

        return null;
    }

    public static bool CheckCorrectPosition(Vector3 position, LayerMask mask)
    {
//        if (!AiUtil.CheckPositionOnGround(ref position, 5.0f, mask))
//        {
//            Vector3 newPos = AiUtil.GetRandomPosition(position, 0.0f, 5.0f, 5.0f, mask, 5);
//            if (newPos != Vector3.zero)
//            {
//                position = newPos;
//                return true;
//            }
//        }
//        else
//        {
//            return true;
//        }
//
//        return false;

		return AiUtil.CheckPositionOnGround(position, 5.0f, mask);
    }

    //�ų�player
    //public static Player GetClosedPlayer(Vector3 pos, Player player)
    //{
    //    float dis = Mathf.Infinity;
    //    Player _player = null;
    //    //foreach (GameObject go in PlayerFactory.playerList)
    //    //{
    //    //    if (go == null || (player != null && go == player.gameObject)) 
    //    //        continue;

    //    //    Player p = go.GetComponent<Player>();

    //    //    if (p == null) continue;

    //    //    Vector3 distance = p.transform.position - pos;
    //    //    distance.y = 0.0f;

    //    //    if (distance.sqrMagnitude < dis)
    //    //    {
    //    //        _player = p;
    //    //        dis = distance.sqrMagnitude;
    //    //    }
    //    //}

    //    return _player;
    //}

    public static void DrawBounds(Transform tr, Bounds bound, Color color)
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
                //vert[i] = tr.TransformPoint(vert[i]);
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

    public static Bounds TransfromOBB2AABB(Transform tr, Bounds bound)
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

        Bounds tmpBound = new Bounds();
        tmpBound.center = tr.TransformPoint(bound.center);

        foreach (Vector3 v in vert)
            tmpBound.Encapsulate(v);

        return tmpBound;
    }

    public static Vector3 GetRunawayPosition(AiObject aiObj, Vector3 direction, float minRadius, float maxRadius, float angle)
    {
        float minRandomRadius = aiObj.radius + minRadius;
        float maxRandomRadius = aiObj.radius + maxRadius;

        if (aiObj.motor.habit == LifeArea.LA_Land)
            return GetRandomPositionInLand(aiObj.position, minRandomRadius, maxRandomRadius, 
                                     direction, -angle, angle, 10.0f, groundedLayer, 10);
        else if (aiObj.motor.habit == LifeArea.LA_Water)
            return GetRandomPositionInWater(aiObj.position, aiObj.height, 128.0f,
                                            direction, -angle, angle, minRandomRadius, maxRandomRadius, 10);
        else if (aiObj.motor.habit == LifeArea.LA_Sky)
            return GetRandomPositionInSky(aiObj.position, 64.0f, 128.0f, direction, -angle, angle, minRandomRadius, maxRandomRadius, 10);

        return Vector3.zero;
    }

    public static Vector3 RunawayDirectionCorrect(AiObject ai, Vector3 direction)
    {
        if (ai == null)
            return direction;

        if (!ai.motor.CheckMovementValid(direction))
            return Vector3.zero;

        Vector3 _direction = direction;
        //RaycastHit hitInfo;
        //if (AiUtil.CalculateCollsion(ai.collider, _direction, 2f, out hitInfo, AiManager.Manager.avoidLayer))
        //{
        //    _direction = Quaternion.AngleAxis(45.0f, ai.transform.up) * direction;

        //    if (AiUtil.CalculateCollsion(ai.collider, _direction, 2f, out hitInfo, AiManager.Manager.avoidLayer))
        //    {
        //        _direction = Quaternion.AngleAxis(-45.0f, ai.transform.up) * direction;
        //    }
        //}

        //if (ai is AiSkyMonster)
        //{
        //    AiSkyMonster skyMonster = ai as AiSkyMonster;
        //    skyMonster.isLand = false;
        //    direction.y = 0.0f;
        //    direction += Vector3.up * 0.5f;
        //}

        //if (ai is AiWaterMonster)
        //{
        //    direction.y = 0.0f;
        //}

        return _direction;
    }

    public static bool CalculateCollsion(Collider collider, Vector3 moveDir, float inspectRange, out RaycastHit hitInfo, LayerMask mask)
    {
        if (collider == null)
        {
            hitInfo = new RaycastHit();
            return false;
        }

        Vector3 point1 = Vector3.zero;
        Vector3 point2 = Vector3.zero;
        float capRadius = 0.0f;
        if(collider is CapsuleCollider)
        {
            CapsuleCollider capsule = collider as CapsuleCollider;
            if (capsule.direction == 2)
            {
                Transform transform = collider.transform;
                capRadius = capsule.height * 0.5f;
                point1 = transform.position + transform.up * capsule.radius - transform.forward * capsule.height * 0.5f;
                point2 = transform.position + transform.up * capsule.radius + transform.forward * capsule.height * 0.5f;
            }
            else if (capsule.direction == 1)
            {
                Transform transform = collider.transform;
                capRadius = capsule.radius;
                point1 = transform.position;
                point2 = transform.position + transform.up * capsule.height;
            }
        }
        else if (collider is CharacterController)
        {
            CharacterController controller = collider as CharacterController;
            Transform transform = collider.transform;
            capRadius = controller.radius;
            point1 = transform.position;
            point2 = transform.position + transform.up * controller.height;
        }

        return Physics.CapsuleCast(point1, point2, capRadius, moveDir, out hitInfo, inspectRange, mask);
    }

	public static bool CheckPositionOnTerrainCollider(Vector3 position, float height = 96.0f)
	{
        return true;
        //RaycastHit info;
        //if(Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, 
        //                   out info, height, AiManager.Manager.groundedLayer))
        //    return true;
        //else
        //    return false;
	}

	public static Color GetPixelFromWorldPosition(Texture2D tex, Vector3 position)
	{
		if(tex == null)
			return new Color();

		int x = (int)(position.x / VoxelTerrainConstants._worldSideLenX * tex.width);
		int z = (int)(position.z / VoxelTerrainConstants._worldSideLenZ * tex.height);
		
		return tex.GetPixel(x, z);
	}

    public static Bounds IntVector4ToBounds(IntVector4 intVec4)
    {
        Bounds bounds = new Bounds();
        Vector3 min = intVec4.ToVector3();
        Vector3 max = intVec4.ToVector3() + Vector3.one * (VoxelTerrainConstants._numVoxelsPerAxis << intVec4.w);
        bounds.SetMinMax(min, max);
        return bounds;
    }

    static int SortHitInfo(RaycastHit hit1, RaycastHit hit2)
    {
        return hit1.distance.CompareTo(hit2.distance);
    }

    public static RaycastHit[] SortHitInfoFromDistance(RaycastHit[] hits, bool ignoreTrigger = true)
    {
        List<RaycastHit> hitInfos = new List<RaycastHit>(hits);

        if(ignoreTrigger)
        {
            hitInfos = hitInfos.FindAll(ret => !ret.collider.isTrigger);
        }

        hitInfos.Sort(SortHitInfo);

        return hitInfos.ToArray();
    }

    public static bool GetCloestRaycastHit(out RaycastHit hitInfo, RaycastHit[] hits, bool ignoreTrigger = true)
    {
        RaycastHit[] hitinfos = SortHitInfoFromDistance(hits, ignoreTrigger);
        if (hitinfos.Length > 0)
        {
            hitInfo = hitinfos[0];
            return true;

        }

        hitInfo = new RaycastHit();
        return false;
    }

    public static PointType GetPointType(Vector3 position)
    {
        PointType type = PointType.PT_NULL;

        RaycastHit hitInfo;
        if (AiUtil.CheckPositionUnderWater(position))
            type = PointType.PT_Water;
        else if (AiUtil.CheckPositionInCave(position, 128.0f, voxelLayer))
            type = PointType.PT_Cave;
        else if (AiUtil.CheckPositionOnGround(position, out hitInfo, 5.0f, groundedLayer))
        {
            if (CheckSlopeValid(hitInfo.normal, 45.0f))
                type = PointType.PT_Ground;
            else
                type = PointType.PT_Slope;
        }

        return type;
    }

    public static int GetMapID(Vector3 position)
    {
       return (int)VFDataRTGen.GetXZMapType((int)position.x, (int)position.z);
    }

    public static int GetAreaID(Vector3 position)
    {
//        Vector2 pos = new Vector2(position.x, position.z);
//        Rect rect = new Rect();
//        //0 - 5 kilometer
//        rect.width = 10 * 1000.0f;
//        rect.height = 10 * 1000.0f; ;
//        rect.center = Vector2.zero;
//
//        if (rect.Contains(pos))
//            return 1;
//
//        //5 - 10 kilometer
//        rect.width = 20 * 1000.0f;
//        rect.height = 20 * 1000.0f; ;
//        rect.center = Vector2.zero;
//
//        if (rect.Contains(pos))
//            return 2;
//
//        //10 - 15 kilometer
//        rect.width = 30 * 1000.0f;
//        rect.height = 30 * 1000.0f; ;
//        rect.center = Vector2.zero;
//
//        if (rect.Contains(pos))
//            return 3;
//
//        //15 - 20 kilometer
//        rect.width = 40 * 1000.0f;
//        rect.height = 40 * 1000.0f; ;
//        rect.center = Vector2.zero;
//
//        if (rect.Contains(pos))
//            return 4;
//
//        return -1;
		
		int level = VATownGenerator.Instance.GetLevelByRealPos(new IntVector2((int)position.x,(int)position.z));
		switch(level){
			case 0:return 1;
			case 1:return 2;
			case 2:return 3;
			case 3:return 4;
			case 4:return 4;
			default: return -1;
		}
    }

    public static int GetVoxelType(RaycastHit hitInfo)
    {
        int layer = hitInfo.transform.gameObject.layer;

        if (layer == Pathea.Layer.VFVoxelTerrain)
        {
            if (hitInfo.transform.GetComponent<B45ChunkGo>() != null)
            {
                return -1;
            }
            else if (hitInfo.transform.GetComponent<VFVoxelChunkGo>() != null)
            {
                return 1;
            }
        }

        return 0;
    }

	public static bool CheckBlockBrush(Pathea.PeEntity entity)
	{
		if(PEBuildingMan.Self != null /*&& PEBuildingMan.Self.brushBound != null*/ && !PEBuildingMan.Self.brushBound.Equals(null))
		{
			Bounds bounds = PEBuildingMan.Self.brushBound;
		    Bounds npcBounds = new Bounds(entity.position, entity.peTrans.bound.size);
				
			return npcBounds.Intersects(bounds);
		}
		return false;
	}


    public static bool InBlocking()
    {
        return PEBuildingMan.Self != null /*&& PEBuildingMan.Self.brushBound != null*/ && !PEBuildingMan.Self.brushBound.Equals(null);
    }

    public static bool CheckBlockBrush(Pathea.PeEntity entity,out Vector3 avoidPos)
    {
        avoidPos = Vector3.zero;
        if (InBlocking())
        {
            Bounds bounds = PEBuildingMan.Self.brushBound;
            Bounds npcBounds = new Bounds(entity.position, entity.peTrans.bound.size);

            avoidPos = bounds.center;
            return npcBounds.Intersects(bounds);
        }
        return false;
    }

    public static bool InDigging(Pathea.PeEntity target)
    {
        return target.motionEquipment.digTool != null && target.motionEquipment.digTool.m_Indicator != null && target.motionEquipment.digTool.m_Indicator.show;
    }

	public static  bool CheckDig(Pathea.PeEntity entity, Pathea.PeEntity target)
	{ 
		if(entity == null || target == null)
			return false;

        if (InDigging(target))
		{
			Bounds npcBouds = new Bounds(entity.position, entity.peTrans.bound.size);
			Bounds digBounds = target.motionEquipment.digTool.m_Indicator.bounds;
			
			return npcBouds.Intersects(digBounds);
		}
		return false;
	}

    public static bool CheckDig(Pathea.PeEntity entity, Pathea.PeEntity target,out Vector3 avoidPos)
    {
        avoidPos = Vector3.zero;
        if (entity == null || target == null)
            return false;

        if (target.motionEquipment.digTool != null && target.motionEquipment.digTool.m_Indicator != null && target.motionEquipment.digTool.m_Indicator.show)
        {
            Bounds npcBouds = new Bounds(entity.position, entity.peTrans.bound.size);
            Bounds digBounds = target.motionEquipment.digTool.m_Indicator.bounds;

            avoidPos = digBounds.center;
            return npcBouds.Intersects(digBounds);
        }
        return false;
    }

    public static bool InDragging()
    {
        return DraggingMgr.Instance != null && DraggingMgr.Instance.IsDragging();
    }

	public static  bool CheckDraging(Pathea.PeEntity entity)
	{
		if(DraggingMgr.Instance == null || entity == null)
			return false;

		if(DraggingMgr.Instance.IsDragging())
		{
			ItemObjDragging item = DraggingMgr.Instance.Dragable as ItemObjDragging;
			if(item ==null)
				return false;
			
			if(item == null || item.DragBase == null || item.DragBase.rootGameObject == null || null == item.DragBase.itemBounds)
				return false;
			
			Bounds npcBouds = new Bounds(entity.position, entity.peTrans.bound.size);
			Bounds iteamBounds = new Bounds(item.DragBase.rootGameObject.transform.position, item.DragBase.itemBounds.worldBounds.size);
			
			return npcBouds.Intersects(iteamBounds);
		}
		return false;
	}

    public static bool CheckDraging(Pathea.PeEntity entity,out Vector3 avoidPos)
    {
        avoidPos = Vector3.zero;
        if (DraggingMgr.Instance == null || entity == null)
            return false;

        if (DraggingMgr.Instance.IsDragging())
        {
            ItemObjDragging item = DraggingMgr.Instance.Dragable as ItemObjDragging;
            if (item == null)
                return false;

            if (item == null || item.DragBase == null || item.DragBase.rootGameObject == null || null == item.DragBase.itemBounds)
                return false;

            Bounds npcBouds = new Bounds(entity.position, entity.peTrans.bound.size);
            Bounds iteamBounds = new Bounds(item.DragBase.rootGameObject.transform.position, item.DragBase.itemBounds.worldBounds.size);

            avoidPos = item.DragBase.rootGameObject.transform.position;
            return npcBouds.Intersects(iteamBounds);
        }
        return false;
    }

	static int _nFrmOfDcAgents = -1;
	static List<ISceneObjAgent> _dcAgents = null;
    public static bool CheckCreation(Pathea.PeEntity entity)
    {
		if (Time.frameCount != _nFrmOfDcAgents) {
			_dcAgents = SceneMan.GetSceneObjs<DragCreationAgent>();
			_nFrmOfDcAgents = Time.frameCount;
		}
		if (_dcAgents != null && _dcAgents.Count > 0)
        {
            Bounds npcBouds = new Bounds(entity.position, entity.peTrans.bound.size);
			for (int i = 0; i < _dcAgents.Count; i++)
            {
				DragCreationAgent creation = _dcAgents[i] as DragCreationAgent;
                if (creation != null && creation.peTrans != null)
                {
                    Bounds creationBd = new Bounds(creation.position, creation.peTrans.bound.size);
                    creationBd.Expand(5.0f);
                    if (creationBd.size != Vector3.zero && creationBd.Intersects(npcBouds))
                        return true;
                }
            }
        }

        return false;
    }
    public static bool CheckCreation(Pathea.PeEntity entity,out Vector3 avoidPos)
    {
        avoidPos = Vector3.zero;
		if (Time.frameCount != _nFrmOfDcAgents) {
			_dcAgents = SceneMan.GetSceneObjs<DragCreationAgent>();
			_nFrmOfDcAgents = Time.frameCount;
		}
		if (_dcAgents != null && _dcAgents.Count >0)
        {
            Bounds npcBouds = new Bounds(entity.position, entity.peTrans.bound.size);
			for(int i=0;i<_dcAgents.Count;i++)
            {
				DragCreationAgent creation = _dcAgents[i] as DragCreationAgent;
                if(creation != null && creation.peTrans != null)
                {
                    Bounds creationBd = new Bounds(creation.position, creation.peTrans.bound.size);
                    creationBd.Expand(5.0f);
                    avoidPos = creation.position;
                    if (creationBd.size != Vector3.zero && creationBd.Intersects(npcBouds))
                        return true;
                }
            }
        }
        return false;
    }
}
