using UnityEngine;

namespace Pathea
{
	public enum DefenceType
	{
		Flesh = 0,
		FleshWeekness,
		Humanbody,
		Shell,
		Metal,
		MetalWeekness,
		Carrier,
		Building,
		Energy
	}

	public class PEDefenceTrigger : MonoBehaviour 
	{
		public Transform modelRoot;
		public Transform centerBone;
		[System.Serializable]
		public class PEDefencePart
		{
			// NameJustForEdit
			public string name;
			public DefenceType defenceType;
			public DefenceMaterial defenceMaterial;
			public float damageScale;
			public PECapsuleTrigger capsule;

			public void Init()
			{
				capsule.ResetInfo();
				if(0 == damageScale)
					damageScale = 1f;
			}

			public void Update()
			{
				capsule.Update(Vector3.zero);
			}
		}

		public PEDefencePart[] defenceParts;
		
		bool m_Active = true;

		bool m_PartInfoUpdated;
		
		public bool active
		{
			get { return m_Active; }
			set
			{
				if(m_Active == value) return;
				m_Active = value;
				Collider col = GetComponent<Collider>();
				if(null != col)	col.enabled = m_Active;
			} 
		}

		public bool GetClosest(Vector3 pos, float maxDistance, out PECapsuleHitResult result)
		{
			UpdateInfo();
			bool getpos = false;
			float minDis = maxDistance;
			result = null;

			PECapsuleHitResult findResult;
			
			for(int i = 0; i < defenceParts.Length; i++)
			{
				if(!defenceParts[i].capsule.enable)
					continue;

				if(defenceParts[i].capsule.GetClosestPos(pos, out findResult))
				{
					findResult.hitDir = Vector3.Normalize(centerBone.position - pos);
					findResult.hitDefenceType = defenceParts[i].defenceType;
					findResult.damageScale = defenceParts[i].damageScale;
					result = findResult;
					return true;
				}
				else
				{
					bool isCenterBone = (defenceParts[i].capsule.trans == centerBone);
					if(isCenterBone ? (findResult.distance > maxDistance) : (findResult.distance > minDis))
					   continue;

					findResult.hitDefenceType = defenceParts[i].defenceType;
					findResult.damageScale = defenceParts[i].damageScale;
					result = findResult;
					if(isCenterBone)
						return true;
					minDis = findResult.distance;
					getpos = true;
				}
			}

			return getpos;
		}

		public bool RayCast(Ray castRay, float maxDistance, out PECapsuleHitResult result)
		{
			UpdateInfo();
			bool getpos = false;
			result = null;
			float minDis = maxDistance;

			Vector3 rayPos1 = castRay.origin, rayPos2 = castRay.origin + castRay.direction.normalized * maxDistance;

			PECapsuleHitResult findResult;

			for(int i = 0; i < defenceParts.Length; i++)
			{
				if(!defenceParts[i].capsule.enable)
					continue;
				if(defenceParts[i].capsule.CheckCollision(rayPos1, rayPos2, out findResult))
				{
					getpos = true;
					float dis = Vector3.Magnitude(rayPos1 - findResult.hitPos);
					if(dis < minDis)
					{
						minDis = dis;
						result = findResult;
						result.hitDefenceType = defenceParts[i].defenceType;
						result.damageScale = defenceParts[i].damageScale;
						result.distance = Vector3.Distance(findResult.hitPos, castRay.origin);
					}
				}
			}

			return getpos;
		}

		void Reset()
		{
			Transform checkTrans = (transform.parent != null) ? transform.parent : transform;
			PEModelController mc = checkTrans.GetComponentInChildren<PEModelController>();
			PEInjuredController ic = checkTrans.GetComponentInChildren<PEInjuredController>();
			modelRoot = centerBone = (null != mc) ? mc.transform : transform;
			if(null != ic && null != mc)
				CheckOldCols(ic.transform, mc.transform);
		}

		void CheckOldCols(Transform defenceRoot, Transform modelRoot)
		{
			Collider[] findCols = defenceRoot.GetComponentsInChildren<Collider>(true);
			defenceParts = new PEDefencePart[findCols.Length];
			for(int i = 0; i < findCols.Length; i++)
			{
				defenceParts[i] = new PEDefencePart();
				defenceParts[i].name = findCols[i].name;
				defenceParts[i].defenceType = DefenceType.Flesh;
				defenceParts[i].damageScale = 1f;
				defenceParts[i].capsule = new PECapsuleTrigger();
				defenceParts[i].capsule.axis = PECapsuleTrigger.AxisDir.Inverse_X_Axis;
				defenceParts[i].capsule.trans = PETools.PEUtil.GetChild(modelRoot, findCols[i].name);
			}
		}

		void Start()
		{
			for(int i = 0; i < defenceParts.Length; i++)
				defenceParts[i].Init();
			if (null == centerBone || null == modelRoot)
			{
				PEModelController mc = transform.GetComponentInChildren<PEModelController>();
				if(null != mc) centerBone = modelRoot = mc.transform;
			}
//			if(null == modelRoot)
//				Debug.LogError("modelRoot is null." + ((null != transform.parent) ? transform.parent.name : ""));
//			if(null == centerBone)
//				Debug.LogError("centerBone is null" + ((null != transform.parent) ? transform.parent.name : ""));
		}
		
		// Update is called once per frame
		void LateUpdate ()
		{
            ///lw.2017.1.19ø’±®¥Ì–ﬁ∏¥£∫Version:V1.0.6 from SteamId 76561197984028121
            ///The referenced script on this Behaviour is missing!
            if (this != null && null != modelRoot)
			{
				transform.position = modelRoot.position;
				transform.rotation = modelRoot.rotation;
				transform.localScale = modelRoot.localScale;
			}
			m_PartInfoUpdated = false;
		}

		public void UpdateInfo()
		{
			if(!m_PartInfoUpdated)
			{
				m_PartInfoUpdated = true;				
				UpdatePartsInfo();
			}
		}
		
		void UpdatePartsInfo()
		{
			for(int i = 0; i < defenceParts.Length; i++)
				defenceParts[i].Update();
		}

		#if UNITY_EDITOR

		public bool showDebugInfo = false;

		void OnValidate()
		{
			for(int i = 0; i < defenceParts.Length; i++)
				defenceParts[i].capsule.UpdateSettingValue();
		}

		void OnDrawGizmosSelected()
		{
			if(null == defenceParts)
				return;
			if(showDebugInfo || !Application.isPlaying)
				for(int i = 0; i < defenceParts.Length; i++)
					defenceParts[i].capsule.DrawGizmos();
		}
		#endif

		public void SyncBone(PEDefenceTrigger other)
		{
			if(null == other)
				return;
			modelRoot = other.modelRoot;
			centerBone = other.centerBone;

			for(int i = 0; i < defenceParts.Length; i++)
			{
				for(int j = 0; j < other.defenceParts.Length; j++)
				{
					if(defenceParts[i].name == other.defenceParts[j].name)
					{
						defenceParts[i].capsule.trans = other.defenceParts[j].capsule.trans;
					}
				}
			}			
			UpdateInfo();
		}
	}
}