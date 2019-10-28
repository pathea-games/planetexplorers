using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillSystem;
using WhiteCat;

namespace Pathea.Projectile
{
    public class ProjectileBuilder : Singleton<ProjectileBuilder>
    {
        public class ProjectileRequest
        {
            public int m_ID;
            public int m_Index;
            public Transform m_Caster;
            public Transform m_Emitter;
            public Vector3 m_Position;
            public Quaternion m_Rotation;

            public Transform m_TargetTrans;
            public Vector3 m_TargetPos;

            public ProjectileRequest(   int _id, 
                                        Transform _caster, 
                                        Transform _emitter, 
                                        Vector3 _pos, 
                                        Quaternion _rot, 
                                        Transform _target, 
                                        Vector3 _targetPos, 
                                        int _index)
            {
                m_ID = _id;
                m_Caster = _caster;
                m_Emitter = _emitter;
                m_Position = _pos;
                m_Rotation = _rot;
                m_TargetTrans = _target;
                m_TargetPos = _targetPos;
                m_Index = _index;
            }

            Transform GetEmit()
            {
                return m_Emitter;
            }

            Vector3 GetPosition()
            {
                return m_Emitter != null ? m_Emitter.position : m_Position;
            }

            Quaternion GetRotation()
            {
                return m_Emitter != null ? m_Emitter.rotation : m_Rotation;
            }

            Transform GetTargetTrans()
            {
                return m_TargetTrans;
            }

            Vector3 GetTargetPosition()
            {
                return m_TargetPos;
            }

            public void Create(Transform parent)
            {
                ProjectileData data = ProjectileData.GetProjectileData(m_ID);
                if (data == null || m_Caster == null)
                    return;

                GameObject go = Resources.Load(data._path) as GameObject;
                if (go != null)
                {
                    GameObject obj = Instantiate(go) as GameObject;
                    obj.transform.parent = parent;
                    obj.transform.position = GetPosition();
                    obj.transform.rotation = GetRotation();

                    SkProjectile projectile = obj.GetComponent<SkProjectile>();
                    if (projectile != null)
                    {
                        projectile.SetData(data,m_Caster, m_Emitter, GetTargetTrans(), GetTargetPosition(), m_Index);
                    }

                    EffectLateupdateHelper helper = obj.GetComponent<EffectLateupdateHelper>();
                    if (null != helper)
                        helper.Init(m_Emitter);
                }
            }
        }

        List<ProjectileRequest> m_ReqList = new List<ProjectileRequest>();
        List<ProjectileRequest> m_Reqs = new List<ProjectileRequest>();

        void LateUpdate()
        {
			if(m_ReqList.Count > 0)
			{
				m_Reqs.Clear();
				m_Reqs.AddRange(m_ReqList);

	            for (int i = m_Reqs.Count - 1; i >= 0; i--)
	            {
	                ProjectileRequest req = m_ReqList[i];

	                req.Create(transform);

	                m_ReqList.Remove(req);
	            }
			}
        }

		public void Register(int id, Transform caster, Transform target, int index = 0, bool immediately = false)
		{
			ProjectileData projectileData = ProjectileData.GetProjectileData(id);
			if (projectileData == null || caster == null)
				return;
			
			Transform emitter = null;
			
			if (!string.IsNullOrEmpty(projectileData._bone) && "0" != projectileData._bone)
			{
				PeEntity entity = caster.GetComponent<PeEntity>();
				if (null != entity)
					emitter = entity.GetChild(projectileData._bone);
				else
					emitter = PETools.PEUtil.GetChild(caster, projectileData._bone);
			}
			ProjectileRequest req = new ProjectileRequest(id, caster, emitter, Vector3.zero, Quaternion.identity, target, Vector3.zero, index);
			if (immediately)
				req.Create(transform);
			else
				m_ReqList.Add(req);
		}

        public void Register(int id, Transform caster, Transform emitter, Transform target, int index = 0, bool immediately = false)
        {
            ProjectileRequest req = new ProjectileRequest(id, caster, emitter, Vector3.zero, Quaternion.identity, target, Vector3.zero, index);
            if (immediately)
                req.Create(transform);
            else
                m_ReqList.Add(req);
        }

        public void Register(int id, Transform caster, Transform emitter, Vector3 targetPosition, int index = 0, bool immediately = false)
        {
            ProjectileRequest req = new ProjectileRequest(id, caster, emitter, Vector3.zero, Quaternion.identity, null, targetPosition, index);
            if (immediately)
                req.Create(transform);
            else
                m_ReqList.Add(req);
        }

        public void Register(int id, Transform caster, Vector3 pos, Quaternion rot, Transform target, int index = 0, bool immediately = false)
        {
            ProjectileRequest req = new ProjectileRequest(id, caster, null, pos, rot, target, Vector3.zero, index);
            if (immediately)
                req.Create(transform);
            else
                m_ReqList.Add(req);
        }

        public void Register(int id, Transform caster, Vector3 pos, Quaternion rot, Vector3 targetPosition, int index = 0, bool immediately = false)
        {
            ProjectileRequest req = new ProjectileRequest(id, caster, null, pos, rot, null, targetPosition, index);
            if (immediately)
                req.Create(transform);
            else
                m_ReqList.Add(req);
        }

        public void Register(int id, Transform caster, SkRuntimeInfo info, int index = 0, bool immediately = false)
        {
            ProjectileData projectileData = ProjectileData.GetProjectileData(id);
            if (projectileData == null)
                return;

            Transform emitter = null;

			if (!string.IsNullOrEmpty(projectileData._bone) && "0" != projectileData._bone)
			{
				PeEntity entity = caster.GetComponent<PeEntity>();
				if (null != entity)
					emitter = entity.GetChild(projectileData._bone);
                else
                    emitter = PETools.PEUtil.GetChild(caster, projectileData._bone);
            }

            Transform trans = null;
			if (info.Target != null)
            {
				PeTrans tr = info.Target.GetComponent<PeTrans>();
                if (tr != null)
                    trans = tr.trans;
            }

			ShootTargetPara shoot = info.Para as ShootTargetPara;
			SkCarrierCanonPara canon = info.Para as SkCarrierCanonPara;
            if (shoot != null)
            {
                if(emitter != null)
                    Register(id, caster, emitter, shoot.m_TargetPos, index, immediately);
            }
            else if(canon != null)
            {
				WhiteCat.BehaviourController drive = info.Caster.GetComponent<WhiteCat.BehaviourController>();
                IProjectileData data = drive.GetProjectileData(canon);
                if (trans != null)
                    Register(id, caster, data.emissionTransform, trans, index, immediately);
                else
                    Register(id, caster, data.emissionTransform, data.targetPosition, index, immediately);
            }
            else
            {
                if(emitter != null)
                {
                    if (emitter != null)
                        Register(id, caster, emitter, trans, index, immediately);
                }
            }
        }
    }
}
