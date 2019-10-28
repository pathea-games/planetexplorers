using UnityEngine;

namespace Pathea
{
	public class PECapsuleHitResult
	{
		public Transform selfTrans;
		public Transform hitTrans;
		public Vector3 hitPos;
		public Vector3 hitDir;
		public float distance;
		public AttackForm selfAttackForm;
		public DefenceType hitDefenceType;
		public float damageScale;
	}

	[System.Serializable]
	public class PECapsuleTrigger
	{
		public enum AxisDir
		{
			X_Axis,
			Y_Axis,
			Z_Axis,
			Inverse_X_Axis,
			Inverse_Y_Axis,
			Inverse_Z_Axis,
		}

		public Transform trans;
		public float radius = 0.2f;
		public float heigh = 0.5f;
		public Vector3 offset = Vector3.zero;
		public AxisDir axis = AxisDir.Y_Axis;

		public bool enable { get{ return null != trans; } }

		float scaledRadius;

		Vector3 moveDir1;
		Vector3 moveDir2;

		Vector3 m_CenterPos1;
		Vector3 m_CenterPos2;

		Vector3 m_PosOffset1;
		Vector3 m_PosOffset2;

		Vector3 m_LastCenterPos1;
		Vector3 m_LastCenterPos2;
		Vector3 m_ParentCenterPos;

		public void ResetInfo()
		{
			if(null == trans)
				return;
			Vector3 dir = Vector3.up;
			switch(axis)
			{
			case AxisDir.X_Axis:
				dir = Vector3.right;
				break;
			case AxisDir.Z_Axis:
				dir = Vector3.forward;
				break;
			case AxisDir.Inverse_X_Axis:
				dir = Vector3.left;
				break;
			case AxisDir.Inverse_Y_Axis:
				dir = Vector3.down;
				break;
			case AxisDir.Inverse_Z_Axis:
				dir = Vector3.back;
				break;
			}

			m_PosOffset1 = radius * dir + offset;
			m_PosOffset2 = (heigh - radius) * dir + offset;
		}

		public void Update(Vector3 centerPos)
		{
			if(!enable)
				return;

			scaledRadius = Mathf.Abs(radius * trans.lossyScale.x);
			
			m_LastCenterPos1 = m_CenterPos1;
			m_LastCenterPos2 = m_CenterPos2;

			m_CenterPos1 = trans.TransformPoint(m_PosOffset1);
			m_CenterPos2 = trans.TransformPoint(m_PosOffset2);

			moveDir1 = m_CenterPos1 - m_LastCenterPos1;
			moveDir2 = m_CenterPos2 - m_LastCenterPos2;

			m_ParentCenterPos = centerPos;
		}

		bool CheckCollision(Vector3 pos1, Vector3 pos2, PECapsuleTrigger other, out PECapsuleHitResult result)
		{
			Vector3 closestPosSelf;
			Vector3 closestPosOther;
			WhiteCat.Utility.ClosestPoint(pos1, pos2, other.m_CenterPos1, other.m_CenterPos2, out closestPosSelf, out closestPosOther);
			Vector3 dir = closestPosOther - closestPosSelf;
			
			float radiusSum = scaledRadius + other.scaledRadius;
			
			if(dir.sqrMagnitude < radiusSum * radiusSum)
			{
				result = new PECapsuleHitResult();
				result.selfTrans = trans;
				result.hitTrans = other.trans;
				result.hitPos = closestPosSelf + dir * scaledRadius / radiusSum;
				result.hitDir = (moveDir1 + moveDir2 + dir).normalized;
				result.distance = 0;
				return true;
			}
			result = null;
			return false;
		}


		// 
		public bool CheckCollision(Vector3 pos1, Vector3 pos2, out PECapsuleHitResult result)
		{
			Vector3 closestPosSelf;
			Vector3 closestPosOther;
			WhiteCat.Utility.ClosestPoint(m_CenterPos1, m_CenterPos2, pos1, pos2, out closestPosSelf, out closestPosOther);
			Vector3 dir = closestPosOther - closestPosSelf;
			
			if(dir.sqrMagnitude < scaledRadius * scaledRadius)
			{
				result = new PECapsuleHitResult();
				result.selfTrans = trans;
				result.hitTrans = trans;
				result.hitDir = Vector3.Normalize(pos2 - pos1);
				result.hitPos = closestPosOther - result.hitDir * Mathf.Sqrt(scaledRadius * scaledRadius - dir.sqrMagnitude);
				result.distance = 0;
				return true;
			}
			result = null;
			return false;
		}
		
		public bool CheckCollision(PECapsuleTrigger other, out PECapsuleHitResult result)
		{
			int count1 = Mathf.FloorToInt(0.5f * moveDir1.magnitude/scaledRadius) + 1;
			
			int count2 = Mathf.FloorToInt(0.5f * moveDir2.magnitude/scaledRadius) + 1;

			//Vector3 normalizedMoveDir1 = moveDir1.normalized;
			//Vector3 normalizedMoveDir2 = moveDir2.normalized;
			float radius11 = Vector3.Distance(m_LastCenterPos1, m_ParentCenterPos);
			float radius12 = Vector3.Distance(m_CenterPos1, m_ParentCenterPos);
			float radius21 = Vector3.Distance(m_LastCenterPos2, m_ParentCenterPos);
			float radius22 = Vector3.Distance(m_CenterPos2, m_ParentCenterPos);

			int count = Mathf.Max(count1, count2);

			float pCount = count > 1 ? count - 1 : count;
			for(int i = 0; i < count; ++i)
			{
				float p =  i / pCount;
				Vector3 checkPoint1 = Vector3.Lerp(m_LastCenterPos1, m_CenterPos1, p);
				Vector3 checkPoint2 = Vector3.Lerp(m_LastCenterPos2, m_CenterPos2, p);
				if(m_ParentCenterPos != Vector3.zero)
				{
					if(checkPoint1 != m_ParentCenterPos)
						checkPoint1 = m_ParentCenterPos + (checkPoint1 - m_ParentCenterPos).normalized * Mathf.Lerp(radius11, radius12, p);
					
					if(checkPoint2 != m_ParentCenterPos)
						checkPoint2 = m_ParentCenterPos + (checkPoint2 - m_ParentCenterPos).normalized * Mathf.Lerp(radius21, radius22, p);
				}
				if(CheckCollision(checkPoint1, checkPoint2, other, out result))
					return true;
			}

			result = null;
			return false;
		}

		// return : checkpos inside the Capsule 
		public bool GetClosestPos(Vector3 pos, out PECapsuleHitResult result)
		{
			result = new PECapsuleHitResult();
			result.hitPos = Vector3.zero;
			result.hitDir = Vector3.zero;
			result.selfTrans = trans;
			result.hitTrans = trans;
			result.distance = Mathf.Infinity;
			if(null != trans)
			{
				Vector3 closestPos = WhiteCat.Utility.ClosestPoint(m_CenterPos1, m_CenterPos2, pos);
				Vector3 dir = closestPos - pos;
				float dis = dir.magnitude;
				result.hitDir = dir.normalized;
				if(dis < scaledRadius)
				{
					result.hitPos = pos;
					result.distance = 0;
					return true;
				}
				result.hitPos = closestPos - result.hitDir * scaledRadius;
				result.distance = dis - scaledRadius;
				return false;
			}
			return false;
		}
		
	#if UNITY_EDITOR
		Transform currentTrans;
		float currentRadius;
		float currentHeigh;
		Vector3 currentOffset = Vector3.zero;
		AxisDir currentAxis = AxisDir.Y_Axis;

		public bool TestHit;
		
		public void UpdateSettingValue()
		{
			bool changed = false;
			if(trans != currentTrans)
			{
				currentTrans = trans;
				changed = true;
			}
			
			if(scaledRadius != currentRadius)
			{
				currentRadius = scaledRadius; // = (radius < 0.5f * heigh) ? 0.5f * heigh : radius;
				heigh = currentHeigh = Mathf.Clamp(heigh, 2f * currentRadius, Mathf.Infinity);
				changed = true;
			}
			
			if(heigh != currentHeigh)
			{
				currentHeigh = heigh = (heigh < 2f * scaledRadius) ? 2f * scaledRadius : heigh;
				changed = true;
			}
			
			if(offset != currentOffset)
			{
				currentOffset = offset;
				changed = true;
			}
			
			if(axis != currentAxis)
			{
				currentAxis = axis;
				changed = true;
			}
			
			if(changed)
				ResetInfo();
		}

		public void DrawGizmos()
		{
			if(null == trans)
				return;
			if(!Application.isPlaying)
				Update(Vector3.zero);
			
			Vector3 normal0 = Vector3.zero;
			Vector3 normal1 = Vector3.zero;
			Vector3 normal2 = Vector3.zero;

			Vector3 pos1_1 = Vector3.zero, pos1_2 = Vector3.zero, pos1_3 = Vector3.zero, pos1_4 = Vector3.zero, pos2_1 = Vector3.zero, pos2_2 = Vector3.zero, pos2_3 = Vector3.zero, pos2_4 = Vector3.zero;
			Vector3 fromDir1= Vector3.zero, fromDir2 = Vector3.zero;

			switch(axis)
			{
			case AxisDir.X_Axis:
			case AxisDir.Inverse_X_Axis:
				normal0 = trans.right;
				normal1 = trans.forward;
				normal2 = trans.up;
				fromDir1 = trans.up;
				fromDir2 = trans.forward;

				pos1_1 = m_CenterPos1 + scaledRadius * trans.up;
				pos1_2 = m_CenterPos1 - scaledRadius * trans.up;
				pos1_3 = m_CenterPos1 + scaledRadius * trans.forward;
				pos1_4 = m_CenterPos1 - scaledRadius * trans.forward;
				
				pos2_1 = m_CenterPos2 + scaledRadius * trans.up;
				pos2_2 = m_CenterPos2 - scaledRadius * trans.up;
				pos2_3 = m_CenterPos2 + scaledRadius * trans.forward;
				pos2_4 = m_CenterPos2 - scaledRadius * trans.forward;
				break;
			case AxisDir.Y_Axis:
			case AxisDir.Inverse_Y_Axis:
				normal0 = trans.up;
				normal1 = trans.right;
				normal2 = trans.forward;
				fromDir1 = trans.forward;
				fromDir2 = trans.right;

				pos1_1 = m_CenterPos1 + scaledRadius * trans.right;
				pos1_2 = m_CenterPos1 - scaledRadius * trans.right;
				pos1_3 = m_CenterPos1 + scaledRadius * trans.forward;
				pos1_4 = m_CenterPos1 - scaledRadius * trans.forward;
				
				pos2_1 = m_CenterPos2 + scaledRadius * trans.right;
				pos2_2 = m_CenterPos2 - scaledRadius * trans.right;
				pos2_3 = m_CenterPos2 + scaledRadius * trans.forward;
				pos2_4 = m_CenterPos2 - scaledRadius * trans.forward;
				break;
			case AxisDir.Z_Axis:
			case AxisDir.Inverse_Z_Axis:
				normal0 = trans.forward;
				normal1 = trans.up;
				normal2 = trans.right;
				fromDir1 = trans.right;
				fromDir2 = trans.up;

				pos1_1 = m_CenterPos1 + scaledRadius * trans.right;
				pos1_2 = m_CenterPos1 - scaledRadius * trans.right;
				pos1_3 = m_CenterPos1 + scaledRadius * trans.up;
				pos1_4 = m_CenterPos1 - scaledRadius * trans.up;
				
				pos2_1 = m_CenterPos2 + scaledRadius * trans.right;
				pos2_2 = m_CenterPos2 - scaledRadius * trans.right;
				pos2_3 = m_CenterPos2 + scaledRadius * trans.up;
				pos2_4 = m_CenterPos2 - scaledRadius * trans.up;
				break;
			}

			if(axis >= AxisDir.Inverse_X_Axis)
			{
				fromDir1 *= -1f;
				fromDir2 *= -1f;
			}

			UnityEditor.Handles.color = TestHit ? Color.yellow : new Color(1f, 0f, 0.5f);

			UnityEditor.Handles.DrawWireArc(m_CenterPos1, normal1, fromDir1, 180f, scaledRadius);
			UnityEditor.Handles.DrawWireArc(m_CenterPos1, normal2, -fromDir2, 180f, scaledRadius);
			UnityEditor.Handles.DrawWireArc(m_CenterPos2, normal1, -fromDir1, 180f, scaledRadius);
			UnityEditor.Handles.DrawWireArc(m_CenterPos2, normal2, fromDir2, 180f, scaledRadius);
			//
			UnityEditor.Handles.DrawWireArc(m_CenterPos1, normal0, normal1, 360f, scaledRadius);
			UnityEditor.Handles.DrawWireArc(m_CenterPos2, normal0, normal1, 360f, scaledRadius);

			UnityEditor.Handles.DrawLine(pos1_1, pos2_1);
			UnityEditor.Handles.DrawLine(pos1_2, pos2_2);
			UnityEditor.Handles.DrawLine(pos1_3, pos2_3);
			UnityEditor.Handles.DrawLine(pos1_4, pos2_4);
		}
	#endif
	}
}