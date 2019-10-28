using UnityEngine;
using System.Collections;

namespace Pathea
{
    public class PeTrans : PeCmpt, IPeMsg
    {
        const int VERSION_0 = 0;
        const int VERSION_1 = VERSION_0 + 1;
        const int CURRENT_VERSION = VERSION_1;

        Transform mCamAnchor;
        Transform mTrans;
        Transform mModel;
        Bounds mLocalBounds;
        Bounds mLocalBoundsExtend;

        //Vector3 mStayPos;
        bool mFastTravel = false;
        Vector3 mFastTravelPos;

        Vector3 mSpawnPosition;
        Vector3 mSpawnForward;

        float m_Radius;

		public override void Awake ()
		{
			base.Awake ();
			if (Application.isPlaying)
			{
				Create();
			}
		}

        void InitBound()
        {
            if (mModel != null)
            {
                mLocalBounds = PETools.PEUtil.GetLocalColliderBoundsInChildren(mModel.gameObject);
                mLocalBoundsExtend = new Bounds(mLocalBounds.center, mLocalBounds.size);
                mLocalBoundsExtend.Expand(3.0f);

                m_Radius = Mathf.Max(mLocalBounds.extents.x, mLocalBounds.extents.z);
            }
        }

        void Create()
        {
            mTrans = new GameObject("DummyTransform").transform;
            mTrans.parent = transform;
            PETools.PEUtil.ResetTransform(mTrans);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (null != mModel)
            {
                mTrans.position = mModel.position;
                mTrans.localScale = mModel.localScale;
                mTrans.rotation = mModel.rotation;

                if (mSpawnPosition == Vector3.zero)
                    mSpawnPosition = mTrans.position;

                if (mSpawnForward == Vector3.zero)
                    mSpawnForward = mTrans.forward;
            }
        }
        #region Transform
        public Vector3 position
        {
            get
            {
                if (null == mTrans)
                    return Vector3.zero;

                return mTrans.position;
            }
            set
            {
				if((value.x < -9999999 || value.x > 9999999) || (value.y < -9999999 || value.y > 9999999) || (value.z < -9999999 || value.z > 9999999))
				{
					Debug.LogError("[ERROR]Try to set wrong pos[" + value.x +"," + value.y + ","+ value.z+"] to entity "+name);
					return;
				}

                if (Entity.Id == 9008)
                {
                    if (PeGameMgr.IsMulti && PlayerNetwork._missionInited)
                    {
                        if (MissionManager.Instance.HadCompleteMission(18) && !MissionManager.Instance.HadCompleteMission(27))
                        {
                            if (value != new Vector3(12246.42f, 193.1f, 6528.76f))
                                return;
                        }
                    }    
                }
//				if(!PETools.PEUtil.CheckErrorPos(value))
//				{
//					Debug.LogError("[ERROR]Try to set error pos[" + value.x + "," + value.y + "," + value.z + "] to entity --------------------");
//				}

				mTrans.position = value;
				if (mModel != null)
				{
#if UNITY_EDITOR
					//if(VFVoxelTerrain.self != null && VFVoxelTerrain.self.Voxels != null && Entity.motionMgr != null && Entity.motionMgr.freezePhyState){
					//	bool inRange = LODOctreeMan.self._Lod0ViewBounds.Contains(value);
					//	if(inRange && VFVoxelTerrain.self.IsInTerrain(value.x, value.y+0.4f, value.z)){
					//		if(!inRange)	Debug.LogError("[ERROR]Try to set OutOfLod0 pos[" + value.x +"," + value.y + ","+ value.z+"] to entity "+name);
					//		else			Debug.LogError("[ERROR]Try to set InTerrain pos[" + value.x +"," + value.y + ","+ value.z+"] to entity "+name);
					//	}
					//}
#endif
					mModel.position = value;
				}

            }
        }

        public Vector3 forwardBottom
        {
            get 
            {
                Vector3 localPos = new Vector3(mLocalBounds.center.x, 0.0f, mLocalBounds.center.z);
                localPos += mLocalBounds.extents.z * Vector3.forward;
                return trans.TransformPoint(localPos);
            }
        }

        public Vector3 forwardUp
        {
            get
            {
                Vector3 localPos = new Vector3(mLocalBounds.center.x, 0.0f, mLocalBounds.center.z);
                localPos += mLocalBounds.extents.z * Vector3.forward;
                localPos += mLocalBounds.size.y * Vector3.up;
                return trans.TransformPoint(localPos);
            }
        }

        public Vector3 forwardCenter
        {
            get
            {
                Vector3 localPos = new Vector3(mLocalBounds.center.x, 0.0f, mLocalBounds.center.z);
                localPos += mLocalBounds.extents.z * Vector3.forward;
                localPos += mLocalBounds.extents.y * Vector3.up;
                return trans.TransformPoint(localPos);
            }
        }

        public Vector3 centerUp
        {
            get
            {
                Vector3 localPos = new Vector3(mLocalBounds.center.x, 0.0f, mLocalBounds.center.z);
                localPos += mLocalBounds.size.y * Vector3.up;
                return trans.TransformPoint(localPos);
            }
        }


        public Vector3 uiHeadTop
        {
            get
            {
                Vector3 localPos = mLocalBounds.center;
                localPos.y += mLocalBounds.size.y * 0.5f;
                return trans.TransformPoint(localPos);
            }
        }


        public Vector3 center
        {
            get
            {
                return mTrans.TransformPoint(mLocalBounds.center);
            }
        }

		public Vector3 scale
		{
			get
			{
				return existent.lossyScale;
			}
			set
			{
				existent.localScale = value;
			}
		}

        public Quaternion rotation
        {
            get
            {
                return existent.rotation;
            }
            set
            {
                existent.rotation = value;
            }
        }

        public Vector3 headTop
        {
            get
            {
                if (mModel == null)
                {
                    return existent.position + existent.up * 1.8f;
                }
                else
                {
                    return existent.position + existent.up * mLocalBounds.size.y + new Vector3(0,0.5f,0);
                }
            }
        }

        public Vector3 forward
        {
            get
            {
                return existent.forward;
            }
        }

        public Transform existent
        {
            get
            {
                if (mModel != null)
                    return mModel;
				else if(null != mTrans)
					return mTrans;
				return transform;
            }
        }

        public Transform realTrans
        {
            get
            {
                return mModel;
            }
        }

        public Transform trans
        {
            get
            {
                return mTrans;
            }
        }

        public Transform camAnchor
        {
            get
            {
                if (mCamAnchor == null)
                {
                    mCamAnchor = new GameObject("CamAnchor").transform;
                    mCamAnchor.parent = trans;
                    PETools.PEUtil.ResetTransform(mCamAnchor);

                    mCamAnchor.position =  headTop;
                }

                return mCamAnchor;
            }
        }

        public Bounds bound
        {
            get { return mLocalBounds; }
            set { mLocalBounds = value; }
        }

        public Bounds boundExtend
        {
            get
            {
                return mLocalBoundsExtend;
            }
        }

        public Vector3 spawnPosition { get { return mSpawnPosition; } }
        public Vector3 spawnForward { get { return mSpawnForward; } }
        #endregion

        public Vector3 fastTravelPos
        {
            get
            {
                return mFastTravelPos;
            }
            set
            {
                mFastTravel = true;
                mFastTravelPos = value;
            }
        }

        public bool fastTravel
        {
            get
            {
                return mFastTravel;
            }
        }

        public float radius
        {
            get
            {
                return m_Radius;
            }
        }

        public bool InsideBody(Vector3 position)
        {
            Bounds bounds = new Bounds();
            bounds.center = mLocalBounds.center;
            bounds.size = new Vector3(mLocalBounds.size.x * 0.8f, mLocalBounds.size.y, mLocalBounds.size.z * 0.8f);
            Vector3 localPos = trans.InverseTransformPoint(position);
            return bounds.Contains(localPos);
        }

        #region Serialize
        public override void Deserialize(System.IO.BinaryReader r)
        {
            int ver = r.ReadInt32();
            if (ver > CURRENT_VERSION)
            {
                Debug.LogError("version error");
                return;
            }
            
            position = PETools.Serialize.ReadVector3(r);
            rotation = PETools.Serialize.ReadQuaternion(r);
            if (ver == VERSION_0)
            {
                /*Vector3 mStayPos = */PETools.Serialize.ReadVector3(r);
            }
            else
            if (ver == VERSION_1)
            {
                mFastTravel = r.ReadBoolean();
            }

            mFastTravelPos = PETools.Serialize.ReadVector3(r);

            if (mFastTravel)
            {
                position = mFastTravelPos;
                mFastTravel = false;
            }
        }

        public override void Serialize(System.IO.BinaryWriter w)
        {
            w.Write(CURRENT_VERSION);

            PETools.Serialize.WriteVector3(w, position);
            PETools.Serialize.WriteQuaternion(w, rotation);
            w.Write(mFastTravel);
            PETools.Serialize.WriteVector3(w, mFastTravelPos);
        }

        #endregion

        public float GetSqrDistanceXZ(Collider collider)
        {
            Vector3 centerPos = mTrans.TransformPoint(mLocalBounds.center);
            Vector3 targetPos = PETools.PEUtil.GetCenter(collider);
            RaycastHit hitInfo;
            if (Physics.Raycast(centerPos,
                                targetPos - centerPos,
                                out hitInfo,
                                Vector3.Distance(centerPos, targetPos),
                                1 << collider.gameObject.layer))
            {
                Vector3 pos = mTrans.InverseTransformPoint(hitInfo.point);
                return bound.SqrDistance(pos);
            }

            return 0.0f;
        }

        public void ResetModel()
        {
            if (mModel != null)
            {
                mTrans.position = mModel.position;
                mTrans.rotation = mModel.rotation;

                mModel = null;

                Entity.SendMsg(EMsg.Trans_Simulator);
            }
        }

        public void SetModel(Transform model)
        {
            if (null != model)
            {
                mModel = model;
                InitBound();
                Entity.SendMsg(EMsg.Trans_Real);
            }
        }

        void IPeMsg.OnMsg(EMsg msg, params object[] args)
        {
            if (msg == EMsg.View_Model_Build)
            {
                GameObject modelObj = args[0] as GameObject;
                if (null != modelObj)
                {
                    SetModel(modelObj.transform);
                }
            }
            else if (msg == EMsg.View_Prefab_Destroy)
            {
                ResetModel();
            }
        }

        void OnDrawGizmosSelected()
        {
            PETools.PEUtil.DrawBounds(mModel, mLocalBounds, Color.red);
        }
    }

    namespace PeEntityExtTrans
    {
        public static class PeEntityExtTrans
        {
            #region Trans
            public static Transform ExtGetTrans(this PeEntity entity)
            {
                PeTrans tr = entity.peTrans;
                if (null == tr)
                {
                    return null;
                }
                return tr.trans;
            }

            public static void SetStayPos(this PeEntity entity, Vector3 pos) { }

            public static void SetBirthPos(this PeEntity entity, Vector3 pos) 
			{
				NpcCmpt npccmpt = entity.GetCmpt<NpcCmpt>();
				if(null != npccmpt)
				{
					npccmpt.FixedPointPos = pos;//NPCAI need
				}
			}

            public static Vector3 ExtGetPos(this PeEntity entity)
            {
                if (null == entity)
                {
                    return Vector3.zero;
                }

                PeTrans trans = entity.peTrans;

                if (null == trans)
                {
                    return Vector3.zero;
                }

                return trans.position;
            }

            public static void ExtSetPos(this PeEntity entity, Vector3 value)
            {
                if (null == entity)
                {
                    return;
                }

                PeTrans trans = entity.peTrans;

                if (null == trans)
                {
                    return;
                }

                trans.position = value;
            }

            public static Quaternion ExtGetRot(this PeEntity entity)
            {
                if (null == entity)
                {
                    return Quaternion.identity;
                }

                PeTrans trans = entity.peTrans;

                if (null == trans)
                {
                    return Quaternion.identity;
                }

                return trans.rotation;
            }

            public static void ExtSetRot(this PeEntity entity, Quaternion value)
            {
                if (null == entity)
                {
                    return;
                }

                PeTrans trans = entity.peTrans;

                if (null == trans)
                {
                    return;
                }

                trans.rotation = value;
            }

            public static Vector3 GetForward(this PeEntity entity)
            {
                PeTrans trans = entity.peTrans;
                if (null == trans)
                {
                    return Vector3.zero;
                }

                return trans.forward;
            }
            #endregion
        }
    }
}