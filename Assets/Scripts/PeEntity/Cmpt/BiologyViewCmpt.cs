using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using SkillSystem;
using PETools;
using WhiteCat.UnityExtension;

namespace Pathea
{
	public class BiologyViewCmpt : ViewCmpt, IRagdollHandler, IPeMsg
	{
        [SerializeField]
        string m_PrefabPath = "";

        int mColorID = -1;

        AssetReq mPrefabReq;

        Transform mViewPrefab;

		// Components
		public IKCmpt cmptIK;
		public CommonCmpt cmptCommon;
		public EquipmentCmpt cmptEquipment;
		public HumanPhyCtrl monoPhyCtrl;
		public IKAimCtrl monoIKAimCtrl;
		public PEIK.IKAnimEffectCtrl monoIKAnimCtrl;
		public PEModelController monoModelCtrlr;
		public PERagdollController monoRagdollCtrlr;
		public PEDefenceTrigger monoDefenceTrigger;
		public WhiteCat.BoneCollector monoBoneCollector;

		protected bool mTargetInjuredState = true;
		public bool canInjured{ get { return mTargetInjuredState; } }

		public PEDefenceTrigger defenceTrigger{ get { return monoDefenceTrigger; } }

		public BiologyViewRoot biologyViewRoot{ get; set; }

		public override bool hasView
		{
			get { return null != mViewPrefab; }
		}


		public override Transform centerTransform
		{
			get
			{
				Transform bone = null;

				if (defenceTrigger != null)
					bone = defenceTrigger.centerBone;

				if (bone == null)
					bone = GetCenterBone();

				return bone;
			}
		}


		public string prefabPath{ get { return m_PrefabPath; } }
		public Transform tView 	{ get { return mViewPrefab; } }
        public bool IsRagdoll 	{ get { return monoRagdollCtrlr != null ? monoRagdollCtrlr.active : false; } }

		List<Type> m_HideMask = new List<Type>();

		static List<Collider> g_NPCModelColliders = new List<Collider>();
		static List<Collider> g_NPCOutColliders = new List<Collider>();

		Collider[] m_SelfCols;
		Collider[] m_OutCols;

        bool m_IsNPCOutCol = false;

		bool m_PhyActive = true;

		protected bool m_MeshBuildEnd;

        public void SetViewPath(string path)
        {
            m_PrefabPath = path;
        }

        public void SetColorID(int id)
        {
            mColorID = id;
        }

        string GetModelName()
        {
            MonsterProtoDb.Item item = MonsterProtoDb.Get(Entity.ProtoID);
            return item != null ? item.modelName : null;
        }

        void SetupMaterials(int colorID, string modelName)
        {
            if(!string.IsNullOrEmpty(modelName) && colorID >= 0)
            {
                if(!MonsterRandomDb.ContainsMaterials(colorID, modelName))
                    MonsterRandomDb.RegisterMaterials(colorID, modelName, monoModelCtrlr.GetMaterials());

                Material[] tmpMaterials = MonsterRandomDb.GetMaterials(colorID, modelName);
                if (tmpMaterials != null && tmpMaterials.Length > 0)
                {
                    if(monoModelCtrlr != null)
                        monoModelCtrlr.SetMaterials(tmpMaterials);

                    if (monoRagdollCtrlr != null)
                        monoRagdollCtrlr.SetMaterials(tmpMaterials);
                }
            }
        }

        public void ActivatePhysics(bool value)
        {
            if (monoModelCtrlr != null)
                monoModelCtrlr.ActivatePhysics(value);

			if(Entity.Id == 9008)
			   m_PhyActive = value;

//            if (monoRagdollCtrlr != null)
//                monoRagdollCtrlr.ActivatePhysics(value);
        }

        public void ActivateRagdollPhysics(bool value)
        {
            if (monoRagdollCtrlr != null)
                monoRagdollCtrlr.ActivatePhysics(value);
        }

        public void ActivateCollider(bool value)
        {
            if (monoModelCtrlr != null)
			{
                monoModelCtrlr.ActivateColliders(value);
				ResetOutCollider(value);
			}
        }

        public void ActivateInjured(bool value, float delayTime = 0f)
        {
			mTargetInjuredState = value;
			CancelInvoke("DoActivateInjured");
			if(PETools.PEMath.Epsilon > delayTime)
				DoActivateInjured();
			else
				Invoke("DoActivateInjured", delayTime);
        }

		void DoActivateInjured()
		{
			if(monoDefenceTrigger != null)
				monoDefenceTrigger.active = mTargetInjuredState;
		}

		public void ActivateRenderer(Type type, bool value)
		{
			if(value)
				m_HideMask.Remove(type);
			else if(!m_HideMask.Contains(type))
				m_HideMask.Add(type);
			UpdateRender();
		}

		void UpdateRender()
		{
			if (monoModelCtrlr != null)
				monoModelCtrlr.ActivateRenderer(m_HideMask.Count == 0 && !IsRagdoll);
			if(monoRagdollCtrlr != null)
				monoRagdollCtrlr.ActivateRenderer(m_HideMask.Count == 0 && IsRagdoll);
		}

        public void Fadein(float time = 2.0f)
        {
            if (monoModelCtrlr != null && !IsRagdoll)
                monoModelCtrlr.FadeIn(time);

            if (monoRagdollCtrlr != null && IsRagdoll)
                monoRagdollCtrlr.FadeIn(time);
        }

        public void Fadeout(float time = 2.0f)
        {
            if (monoModelCtrlr != null && !IsRagdoll)
                monoModelCtrlr.FadeOut(time);

            if (monoRagdollCtrlr != null && IsRagdoll)
                monoRagdollCtrlr.FadeOut(time);
        }

        public void HideView(float time)
        {
            if (monoModelCtrlr != null && !IsRagdoll)
                monoModelCtrlr.HideView(time);

            if (monoRagdollCtrlr != null && IsRagdoll)
                monoRagdollCtrlr.HideView(time);
        }

        IEnumerator FadeInDelay(float delayTime, float time)
        {
            yield return new WaitForSeconds(delayTime);

            Fadein(time);
        }

        public Collider GetModelCollider(string name)
        {
            Transform child = monoModelCtrlr != null ? PETools.PEUtil.GetChild(monoModelCtrlr.transform, name) : null;
            return child != null ? child.GetComponent<Collider>() : null;
        }

        public Collider GetRagdollCollider(string name)
        {
            Transform child = monoRagdollCtrlr != null ? PETools.PEUtil.GetChild(monoRagdollCtrlr.transform, name) : null;
            return child != null ? child.GetComponent<Collider>() : null;
        }

        public Transform GetModelTransform(string name)
        {
			return monoModelCtrlr != null ? PETools.PEUtil.GetChild(monoModelCtrlr.transform, name) : null;
        }

        public Rigidbody GetModelRigidbody()
        {
			return monoModelCtrlr != null ? monoModelCtrlr.Rigid : null;
        }

        public bool IsReadyGetUp()
        {
            return monoRagdollCtrlr != null ? monoRagdollCtrlr.IsReadyGetUp() : true;
        }

        public Transform GetRagdollTransform(string name)
        {
            return monoRagdollCtrlr != null ? PETools.PEUtil.GetChild(monoRagdollCtrlr.transform, name) : null;
        }

        public void IgnoreCollision(Collider collider, bool isIgnore = true)
        {
			if(null == collider || !collider.enabled)
				return;
//            if(mViewPrefab != null && collider != null)
//            {
//                PEUtil.IgnoreCollision(collider.gameObject, mViewPrefab.gameObject, isIgnore);
//            }
			if(null != monoModelCtrlr)
				PEUtil.IgnoreCollision(monoModelCtrlr.colliders, collider);
			if(null != monoRagdollCtrlr)
				PEUtil.IgnoreCollision(monoRagdollCtrlr.colliders, collider);
        }


        #region after build
		protected Func<IEnumerator> _coroutineMeshProc = null;
		protected IEnumerator ProcPostBoneLoad()
		{
			//biologyViewRoot.gameObject.SetActive (false);
			if (monoPhyCtrl == null)					monoPhyCtrl = biologyViewRoot.humanPhyCtrl;
			if (monoIKAimCtrl == null)					monoIKAimCtrl = biologyViewRoot.ikAimCtrl;
			if (monoIKAnimCtrl == null)					monoIKAnimCtrl = biologyViewRoot.ikAnimEffectCtrl;
			if (monoModelCtrlr == null)					monoModelCtrlr = biologyViewRoot.modelController;
			if (monoRagdollCtrlr == null)				monoRagdollCtrlr = biologyViewRoot.ragdollController;
			if (monoDefenceTrigger == null)				monoDefenceTrigger = biologyViewRoot.defenceTrigger;

			m_MeshBuildEnd = false;

            if (monoRagdollCtrlr != null)
                monoRagdollCtrlr.SetHandler(this); //EMsg.View_Ragdoll_Build

            // Collider ignore and other
            if (monoModelCtrlr != null){
                SetupMaterials(mColorID, GetModelName());

				if(Entity.proto == EEntityProto.Monster && Entity.Race == ERace.Mankind)
					monoModelCtrlr.gameObject.layer = Layer.AIPlayer;

				PEUtil.IgnoreCollision(monoModelCtrlr.colliders, monoModelCtrlr.colliders);
				if(monoRagdollCtrlr != null)
					PEUtil.IgnoreCollision(monoModelCtrlr.colliders, monoRagdollCtrlr.colliders);
			}
			BuildOutCollider(biologyViewRoot.gameObject);

            if (Entity.IsDeath())
                OnDeath(null, null);
            else if (monoModelCtrlr != null)
                monoModelCtrlr.ActivateDeathMode(false);

            HideView(0.01f);

            if (Entity.Id == 9008)
			   ActivatePhysics(m_PhyActive);

			DoActivateInjured();

			yield return 0;

			if(null == biologyViewRoot)
				yield break;

			// Proc Mesh
			if (_coroutineMeshProc != null) {
				yield return biologyViewRoot.StartCoroutine (_coroutineMeshProc());
			}
			m_MeshBuildEnd = true;
			
			if(null == biologyViewRoot)
				yield break;
			
			if(null == Entity)
				yield break;

            // Send Msg
			if (monoModelCtrlr != null)	{
				Profiler.BeginSample("EMsg.View_Model_Build");
				Entity.SendMsg(EMsg.View_Model_Build, monoModelCtrlr.gameObject, biologyViewRoot);
				Profiler.EndSample();
				yield return 0;
			}			
			
			if(null == biologyViewRoot)
				yield break;
			
			if(null == Entity)
				yield break;
			Profiler.BeginSample("EMsg.View_Prefab_Build");
			Entity.SendMsg(EMsg.View_Prefab_Build, this, biologyViewRoot);
			Profiler.EndSample ();
			yield return 0;
			
			if(null == biologyViewRoot)
				yield break;

			//biologyViewRoot.gameObject.SetActive (true);
			biologyViewRoot.StartCoroutine(FadeInDelay(0.01f, 3.0f));	// Note: prev version use this Monobehavior(NOT ROOT) to start Coroutine
		}
		protected virtual void OnBoneLoad(GameObject obj)
		{
			mPrefabReq = null;

            mViewPrefab = obj.transform;
            mViewPrefab.parent = Entity.GetGameObject().transform;

            mViewPrefab.position = Entity.peTrans.position;
            mViewPrefab.rotation = Entity.peTrans.rotation;
			mViewPrefab.localScale = Entity.peTrans.scale;

			biologyViewRoot = mViewPrefab.GetComponent<BiologyViewRoot>();
			monoBoneCollector = mViewPrefab.GetComponent<WhiteCat.BoneCollector>();
			if (null == biologyViewRoot) {
				// Some doodads haven't root, we add it to start coroutine and make coroutine stopable while destroying mViewPrefab 
				//Debug.LogError("BiologyViewRoot not found in " + mViewPrefab.name);	
				biologyViewRoot = mViewPrefab.gameObject.AddComponent<BiologyViewRoot>();	
			}

			biologyViewRoot.StartCoroutine (ProcPostBoneLoad());
        }

		//build outcol for can't be pushed entites
		void BuildOutCollider(GameObject obj)
		{
			if(null == monoModelCtrlr) return;

			List<Collider> selfCols = new List<Collider>();
			List<Collider> outCols = new List<Collider>();

			m_IsNPCOutCol = false;
			if(Entity.commonCmpt.entityProto.proto == EEntityProto.Monster)
			{
				MonsterProtoDb.Item monsterProto = MonsterProtoDb.Get(Entity.commonCmpt.entityProto.protoId);
				if(null == monsterProto || monsterProto.canBePush)
					return;
			}
			else if(Entity.commonCmpt.entityProto.proto != EEntityProto.Npc
			        && Entity.commonCmpt.entityProto.proto != EEntityProto.Player
			        && Entity.commonCmpt.entityProto.proto != EEntityProto.RandomNpc)
				return;
			else
				m_IsNPCOutCol = true;

			Collider[] colliders;
			if(null != monoModelCtrlr.colliders && monoModelCtrlr.colliders.Length > 0)
				colliders = monoModelCtrlr.colliders;
			else
				colliders = monoModelCtrlr.GetComponentsInChildren<Collider>(true);

			if(null == colliders)
			{
				Debug.LogError(name + " don't has cols.");
				return;
			}
			for(int i = 0; i < colliders.Length; ++i)
			{
				if(colliders[i] == null){
					Debug.LogError("[BView]Colliders have empty element:"+name);
					continue;
				}
				if(!colliders[i].isTrigger)
				{
					Collider addCol = null;
					GameObject newObj = new GameObject("OutCollider" + i);
					newObj.layer = Layer.ShowModel;
					newObj.transform.parent = obj.transform;
					newObj.transform.localScale = Vector3.one;
					PEFollowSimple follow = newObj.AddComponent<PEFollowSimple>();
					follow.master = colliders[i].transform;
					if(colliders[i] is BoxCollider)
					{
						BoxCollider modelCol = colliders[i] as BoxCollider;
						BoxCollider newCol = newObj.AddComponent<BoxCollider>();
						addCol = newCol;
						newCol.center = modelCol.center;
						newCol.size = modelCol.size + 0.2f * Vector3.one;
					}
					else if(colliders[i] is SphereCollider)
					{
						SphereCollider modelCol = colliders[i] as SphereCollider;
						SphereCollider newCol = newObj.AddComponent<SphereCollider>();
						addCol = newCol;
						newCol.center = modelCol.center;
						newCol.radius = modelCol.radius + 0.1f;
					}
					else if(colliders[i] is CapsuleCollider)
					{
						CapsuleCollider modelCol = colliders[i] as CapsuleCollider;
						CapsuleCollider newCol = newObj.AddComponent<CapsuleCollider>();
						addCol = newCol;
						newCol.center = modelCol.center;
						newCol.radius = modelCol.radius + 0.1f;
						newCol.height = modelCol.height + 0.2f;
						newCol.direction = modelCol.direction;
					}

					if(m_IsNPCOutCol)
					{
						g_NPCOutColliders.Add(addCol);
						g_NPCModelColliders.Add(colliders[i]);
					}
					selfCols.Add(colliders[i]);
					outCols.Add(addCol);
				}
			}
			m_SelfCols = selfCols.ToArray();
			m_OutCols = outCols.ToArray();
			selfCols.Clear();
			outCols.Clear();
			selfCols = null;
			outCols = null;
			ResetOutCollider();
		}

		void ResetOutCollider(bool active = true)
		{
			if(null == m_SelfCols || null == m_OutCols)
				return;
			if(m_IsNPCOutCol)
			{
				for(int i = g_NPCModelColliders.Count - 1; i >= 0; --i)
				{
					if(null == g_NPCModelColliders[i])
					{
						g_NPCModelColliders.RemoveAt(i);
						continue;
					}
					PEUtil.IgnoreCollision(m_OutCols, g_NPCModelColliders[i]);
				}
				for(int i = g_NPCOutColliders.Count - 1; i >= 0; --i)
				{
					if(null == g_NPCOutColliders[i])
					{
						g_NPCOutColliders.RemoveAt(i);
						continue;
					}
					PEUtil.IgnoreCollision(m_SelfCols, g_NPCOutColliders[i]);
				}
			}			
			PEUtil.IgnoreCollision(monoModelCtrlr.colliders, m_OutCols);
			PEUtil.IgnoreCollision(monoRagdollCtrlr.colliders, m_OutCols);

			for(int i = 0; i < m_OutCols.Length; ++i)
				if(null != m_OutCols[i])
					m_OutCols[i].enabled = active;
		}

		protected virtual void OnBoneLoadSync(GameObject modelObject)
		{
			if (cmptEquipment) cmptEquipment.ResetModels();
		}

        #endregion


        #region override PeCmpt

        public override void Start()
        {
            base.Start();

			cmptIK = Entity.GetCmpt<IKCmpt>();
            cmptCommon = GetComponent<CommonCmpt>();
			cmptEquipment = GetComponent<EquipmentCmpt>();

            Entity.aliveEntity.deathEvent += OnDeath;
			Entity.aliveEntity.reviveEvent += OnRevive;

            //there is no scene event
            if(SceneMan.self == null)
            {
				this.Invoke(0.1f, Build);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

			Entity.aliveEntity.deathEvent -= OnDeath;
			Entity.aliveEntity.reviveEvent -= OnRevive;

			if(mPrefabReq != null)
			{
				mPrefabReq.Deactivate();
			}
		}
		
		public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            m_PrefabPath = r.ReadString();
        }

        public override void Serialize(BinaryWriter w)
        {
            base.Serialize(w);
            w.Write(m_PrefabPath);
        }

        #endregion


        void OnDeath(SkEntity self, SkEntity injurer)
        {
            ActivateRagdoll(null, false);
            ActivateInjured(false);

            if(monoModelCtrlr != null)
            {
                monoModelCtrlr.ActivateDeathEffect();
                monoModelCtrlr.ActivateDeathMode(true);
            }
        }

        void OnRevive(SkEntity self)
        {
//            ActivateInjured(true);
        }

        bool HasColliderUnder(Vector3 pos)
        {
            return Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, 100f);
        }

        //void SetPhy(bool flag)
        //{
        //    MotionMgrCmpt m = Entity.GetCmpt<MotionMgrCmpt>();
        //    if (null == m)
        //    {
        //        return;
        //    }

        //    m.FreezePhy = flag;
        //}

        public Transform modelTrans { get { return monoModelCtrlr != null ? monoModelCtrlr.transform : null; } }

        Transform GetCenterBone()
        {
            string boneName = "";

            if (monoRagdollCtrlr != null && monoRagdollCtrlr.ragdollRootBone != null)
            {
                boneName = monoRagdollCtrlr.ragdollRootBone.name;
            }

            Transform bone = monoModelCtrlr != null ? PETools.PEUtil.GetChild(monoModelCtrlr.transform, boneName) : null;
            return bone != null ? bone : (monoModelCtrlr != null ? monoModelCtrlr.transform : null);
        }

        public void ActivateRagdoll(RagdollHitInfo hitInfo = null, bool isGetupReady = true)
        {
			if (monoRagdollCtrlr != null)
			{
				monoRagdollCtrlr.Activate(hitInfo, isGetupReady);
			}

			if(null != cmptIK) cmptIK.ikEnable = false;
        }

        public void DeactivateRagdoll(bool immediately = false)
        {
			if(monoRagdollCtrlr != null)
			{
				monoRagdollCtrlr.Deactivate(immediately);
			}

			if(null != cmptIK) cmptIK.ikEnable = true;
		}

        //add cloth
		public virtual void AddPart(int partMask, string path)
		{

		}
		
        //remove cloth
		public virtual void RemovePart(int partMask)
		{

		}

        //remove weapon obj
        public bool DetachObject(GameObject obj)
        {
            if (monoModelCtrlr != null)
            {
                if(obj != null)
                {
					if (monoBoneCollector)
					{
						monoBoneCollector.RemoveEquipment(obj.transform);
                        monoModelCtrlr.Remodel();
						Entity.SendMsg(EMsg.View_Model_DeatchJoint, obj);
					}
                }
            }
            return true;
        }

        public void Reattach(GameObject obj, string boneName)
        {
            if (monoModelCtrlr != null)
            {
				if (monoBoneCollector)
				{
					monoBoneCollector.SwitchBone(obj.transform, boneName);
                    monoModelCtrlr.Remodel();
					Entity.SendMsg(EMsg.View_Model_ReattachJoint, obj);
				}
			}
        }


		public bool IsAttached(GameObject obj)
		{
			if (monoModelCtrlr != null)
			{
				if (monoBoneCollector)
				{
					return monoBoneCollector.FindEquipGroup(obj.transform) >= 0;
                }
			}

			return false;
		}

		                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
        //add weapon obj
        public void AttachObject(GameObject obj, string boneName)
        {
			Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            if (monoModelCtrlr != null)
            {
				if (monoBoneCollector)
				{
					monoBoneCollector.AddEquipment(obj.transform, boneName);
					PEUtil.IgnoreCollision(monoModelCtrlr.colliders, colliders);

					int layer = monoModelCtrlr.gameObject.layer;
					obj.transform.TraverseHierarchy((t, i) => { t.gameObject.layer = layer; });
                    monoModelCtrlr.Remodel();
					Entity.SendMsg(EMsg.View_Model_AttachJoint, obj);
				}
			}
			
			if (monoRagdollCtrlr != null)
				PEUtil.IgnoreCollision(monoRagdollCtrlr.colliders, colliders);
        }

        public virtual void Build()
        {
            if (string.IsNullOrEmpty(prefabPath) ||null != mViewPrefab || null != mPrefabReq)
                return;

			mPrefabReq = AssetsLoader.Instance.AddReq(m_PrefabPath, Entity.peTrans.position, Entity.peTrans.rotation, Vector3.one);
			mPrefabReq.ReqFinishHandler += OnBoneLoad;
        }

		GameObject BuildModelSync()
		{
			if (string.IsNullOrEmpty(prefabPath) /*|| null != mViewPrefab || null != mPrefabReq*/)
				return null;

			GameObject tmpGo = AssetsLoader.Instance.InstantiateAssetImm(m_PrefabPath, Entity.peTrans.position, Entity.peTrans.rotation, Vector3.one);
			if (tmpGo == null)
				return null;

			BiologyViewRoot oldBiologyViewRoot = biologyViewRoot;
			PEModelController oldMonoModelCtrlr = monoModelCtrlr;
			WhiteCat.BoneCollector oldMonoBoneCollector = monoBoneCollector;
			biologyViewRoot = tmpGo.GetComponent<BiologyViewRoot>();
			monoModelCtrlr = biologyViewRoot.modelController;
			monoBoneCollector = tmpGo.GetComponent<WhiteCat.BoneCollector>();

			GameObject tmpModel = monoModelCtrlr.gameObject;
			OnBoneLoadSync(tmpModel);

			// Recover
			biologyViewRoot = oldBiologyViewRoot;
			monoModelCtrlr = oldMonoModelCtrlr;
			monoBoneCollector = oldMonoBoneCollector;

			tmpModel.transform.SetParent(null, true);
			Destroy(tmpGo);
			return tmpModel;
		}

		public virtual void Destroy()
        {
            if (mViewPrefab != null)
            {
                Entity.SendMsg(EMsg.View_Prefab_Destroy, mViewPrefab.gameObject);
				monoBoneCollector = null;

                if (monoModelCtrlr != null)
                    Entity.SendMsg(EMsg.View_Model_Destroy, monoModelCtrlr.gameObject);

                if (monoRagdollCtrlr != null)
                    Entity.SendMsg(EMsg.View_Ragdoll_Destroy, monoRagdollCtrlr.gameObject);

				if(mPrefabReq != null)
				{
					mPrefabReq.Deactivate();
				}

                GameObject.Destroy(mViewPrefab.gameObject);
            }
        }

		public virtual GameObject CloneModel()
		{
			GameObject model = null;

			if (null != monoModelCtrlr && monoBoneCollector != null && m_MeshBuildEnd)
			{
				bool isRagdoll = monoBoneCollector.isRagdoll;
				monoBoneCollector.isRagdoll = false;

				model = Instantiate(monoModelCtrlr.gameObject) as GameObject;

				if (isRagdoll)
				{
					var renderers = model.GetComponentsInChildren<Renderer>(true);
					for (int i = 0; i < renderers.Length; i++)
					{
						renderers[i].enabled = true;
					}
				}

				monoBoneCollector.isRagdoll = isRagdoll;
			}
			else
			{
				try{
					model = BuildModelSync();
				} catch {
					model = null;
				}
			}

            if (model != null)
			    model.transform.rotation = Quaternion.identity;
			return model;
		}

		#region IRagdollHandler
		void IRagdollHandler.OnRagdollBuild(GameObject obj)
        {
            Entity.SendMsg(EMsg.View_Ragdoll_Build, obj);
        }

        void IRagdollHandler.OnDetachJoint(GameObject boneRagdoll)
        {
            Entity.SendMsg(EMsg.View_Ragdoll_DeatchJoint, boneRagdoll);
        }

        void IRagdollHandler.OnReattachJoint(GameObject boneRagdoll)
        {
            Entity.SendMsg(EMsg.View_Ragdoll_ReattachJoint, boneRagdoll);
        }

        void IRagdollHandler.OnAttachJoint(GameObject boneRagdoll)
        {
            Entity.SendMsg(EMsg.View_Ragdoll_AttachJoint, boneRagdoll);
        }

        void IRagdollHandler.OnFallBegin(GameObject ragdoll)
        {
            if (cmptCommon != null && cmptCommon.entityProto.proto == EEntityProto.Monster)
                ActivateCollider(false);
            Entity.SendMsg(EMsg.View_Ragdoll_Fall_Begin, ragdoll);
        }

        void IRagdollHandler.OnFallFinished(GameObject ragdoll)
        {
            Entity.SendMsg(EMsg.View_Ragdoll_Fall_Finished, ragdoll);
        }

        void IRagdollHandler.OnGetupBegin(GameObject ragdoll)
        {
            Entity.SendMsg(EMsg.View_Ragdoll_Getup_Begin, ragdoll);
        }

        void IRagdollHandler.OnGetupFinished(GameObject ragdoll)
        {
            if(cmptCommon != null && cmptCommon.entityProto.proto == EEntityProto.Monster)
                ActivateCollider(true);
            Entity.SendMsg(EMsg.View_Ragdoll_Getup_Finished, ragdoll);
        }

        public void ActivateRagdollRenderer(bool isActive)
		{
			UpdateRender();
        }
        #endregion

		#region IPeMsg implementation
		public void OnMsg (EMsg msg, params object[] args)
		{
			switch(msg)
			{
			case EMsg.View_FirstPerson:
				bool firstPerson = (bool)args[0];
				ActivateRenderer((Type)args[1], !firstPerson);
				if(firstPerson)
					Fadeout(0.2f);
				else
					Fadein(0.2f);
				break;
			}
		}
		#endregion
    }
}
