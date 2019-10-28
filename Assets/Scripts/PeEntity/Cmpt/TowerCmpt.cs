using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SkillSystem;
using Pathea.Effect;

namespace Pathea
{
    public class TowerEffect
    {
        float m_hpPercent;
        int m_effectID;
        int m_AudioID;
        GameObject m_effect;
        AudioController m_auCtrl;
        EffectBuilder.EffectRequest m_Request;

        public float hpPercent { get { return m_hpPercent; } }
        public int effectID { get { return m_effectID; } }
        public GameObject effect { get { return m_effect; } }

        public TowerEffect(float argHpPercent, int argEffectID, int argAudioID)
        {
            m_hpPercent = argHpPercent;
            m_effectID = argEffectID;
            m_AudioID = argAudioID;
            m_effect = null;
            m_Request = null;
            m_auCtrl = null;
        }

        public void ActivateEffect(PeEntity peEntity)
        {
            if(peEntity.HPPercent <= m_hpPercent && !peEntity.IsDeath())
            {
                if(m_effect == null && m_Request == null)
                {
                    m_Request = EffectBuilder.Instance.Register(m_effectID, null, peEntity.tr);
                    m_Request.SpawnEvent += OnSpawned;
                    m_auCtrl = AudioManager.instance.Create(peEntity.position, m_AudioID, null, true, false);
                }

                if (m_effect != null && !m_effect.activeSelf)
                {
                    m_effect.SetActive(true);

                    if (m_auCtrl != null)
                        m_auCtrl.PlayAudio(2.0f);
                }
            }
            else
            {
                if (m_effect != null && m_effect.activeSelf)
                {
                    m_effect.SetActive(false);

                    if (m_auCtrl != null)
                        m_auCtrl.StopAudio(2.0f);
                }
            }
        }

        public void Destroy()
        {
            if(m_effect != null)
                GameObject.Destroy(m_effect);

            if (m_auCtrl != null)
                m_auCtrl.Delete();

            m_auCtrl = null;
            m_Request = null;
        }

        public void OnSpawned(GameObject obj)
        {
            m_effect = obj;
        }
    }

    public enum ECostType
    {
        None,
        Item,
        Energy
    }

    public interface ITowerOpration
    {
        ECostType ConsumeType { get; }
        int ItemID { get; }
        int ItemCount { get; set; }
        int ItemCountMax { get; }
        int EnergyCount { get; set; }
        int EnergyCountMax { get; }
    }

    public class TowerCmpt : PeCmpt, IPeMsg, ITowerOpration
    {
        PEBarrelController m_Barrel;
        PeTrans m_Trans;
        SkEntity m_SkEntity;

		public event Action<float> onConsumeChange;

        ECostType m_CostType;
        public ECostType CostType
        {
            get { return m_CostType; }
            set { m_CostType = value; }
        }

        int m_ConsumeItem;

        public int ConsumeItem
        {
            get { return m_ConsumeItem; }
            set { m_ConsumeItem = value; }
        }
        int m_ConsumeCost;

        public int ConsumeCost
        {
            get { return m_ConsumeCost; }
            set { m_ConsumeCost = value; }
        }

        int m_ConsumeCount;

        int m_ConsumeCountMax;

        public int ConsumeCountMax
        {
            get { return m_ConsumeCountMax; }
            set { m_ConsumeCountMax = value; }
        }

        int m_ConsumeEnergy;

        int m_ConsumeEnergyCost;

        public int ConsumeEnergyCost
        {
            get { return m_ConsumeEnergyCost; }
            set { m_ConsumeEnergyCost = value; }
        }
        int m_ConsumeEnergyMax;

        public int ConsumeEnergyMax
        {
            get { return m_ConsumeEnergyMax; }
            set { m_ConsumeEnergyMax = value; }
        }

        int m_SkillID;

        public int SkillID
        {
            get { return m_SkillID; }
            set { m_SkillID = value; }
        }

        bool m_NeedVoxel = true;
        public bool NeedVoxel
        {
            get { return m_NeedVoxel; }
            set { m_NeedVoxel = value; }
        }

        bool m_OnBlock;

        Transform m_Target;
        BillBoard m_BillBoard;
        List<TowerEffect> m_TowerEffect;

        public int ConsumeCount
        {
            get { return m_ConsumeCount; }
            set 
            { 
                m_ConsumeCount = value;
                m_ConsumeCount = Mathf.Clamp(m_ConsumeCount, 0, m_ConsumeCountMax);
            }
        }

        public int ConsumeEnergy
        {
            get { return m_ConsumeEnergy; }
            set 
            { 
                m_ConsumeEnergy = value;
				m_ConsumeEnergy = Mathf.Clamp(m_ConsumeEnergy, 0, m_ConsumeEnergyMax);
            }
        }

        public Transform Target
        {
            set 
            { 
                if(m_Target != value)
                {
                    m_Target = value;

                    if(m_Barrel != null)
                        m_Barrel.AimTarget = m_Target;
                }
            }
            get { return m_Target; }
        }

		public float ChassisY { get { return null == m_Barrel ? 0f : m_Barrel.ChassisY; } }
		public Vector3 PitchEuler { get { return null == m_Barrel ? Vector3.zero : m_Barrel.PitchEuler; } }

		public void ApplyChassis(float rotY)
		{
			if (null != m_Barrel)
				m_Barrel.ApplyChassis(rotY);
		}

		public void ApplyPitchEuler(Vector3 angleEuler)
		{
			if (null != m_Barrel)
				m_Barrel.ApplyPitchEuler(angleEuler);
		}

        public bool IsEnable
        {
            get
            {
                if (m_NeedVoxel && !m_OnBlock)
                    return false;

                return true;
            }
        }

        public bool HaveCost()
        {
            switch (m_CostType)
            {
                case ECostType.None:
                    return true;
                case ECostType.Item:
                    if (m_ConsumeCount - m_ConsumeCost >= 0)
                        return true;
                    else
                        return false;
                case ECostType.Energy:
                    if (m_ConsumeEnergy - m_ConsumeEnergyCost >= 0)
                        return true;
                    else
                        return false;
            }

            return false;
        }

        public void Fire(SkEntity target)
        {
            if (!HaveCost())
                return;

            if(m_SkEntity != null && m_SkillID > 0)
            {
                Cost();
                m_SkEntity.StartSkill(target, m_SkillID);
            }
        }

        public bool IsSkillRunning()
        {
            if (m_SkEntity != null)
                return m_SkEntity.IsSkillRunning(m_SkillID);

            return false;
        }

        public bool Angle(Vector3 position, float angle)
        {
            if (m_Barrel != null)
                return m_Barrel.Angle(position, angle);

            return false;
        }

        public bool PitchAngle(Vector3 position, float angle)
        {
            if (m_Barrel != null)
                return m_Barrel.PitchAngle(position, angle);

            return false;
        }

        public bool CanAttack(Vector3 position, Transform target = null)
        {
            if (m_Barrel != null)
                return m_Barrel.CanAttack(position, target);

            return false;
        }

        public bool EstimatedAttack(Vector3 position, Transform target = null)
        {
            if (m_Barrel != null)
                return m_Barrel.EstimatedAttack(position, target);

            return false;
        }

        public bool Evaluate(Vector3 position)
        {
            if (m_Barrel != null)
                return m_Barrel.Evaluate(position);

            return false;
        }

        void Cost()
        {
            if (m_CostType == ECostType.Item)
                ConsumeCount -= m_ConsumeCost;

            if (m_CostType == ECostType.Energy)
                ConsumeEnergy -= m_ConsumeEnergyCost;

			
			if(null != onConsumeChange)
				onConsumeChange((m_CostType == ECostType.Item)?m_ConsumeCost:m_ConsumeEnergyCost);
        }

        bool CheckOnBuildTerrain()
        {
            if (m_Trans == null)
                return false;

            //for (int x = -1; x <= 1; x++)
            //{
            //    for (int z = -1; z <= 1; z++)
            //    {
            //        Vector3 worldPos = m_Trans.trans.TransformPoint(x * m_Trans.bound.extents.x, -0.5f, z * m_Trans.bound.extents.z);
            //        IntVector3 ipos = new IntVector3(Mathf.FloorToInt(worldPos.x * BSBlock45Data.s_ScaleInverted),
            //                                         Mathf.FloorToInt(worldPos.y * BSBlock45Data.s_ScaleInverted),
            //                                         Mathf.FloorToInt(worldPos.z * BSBlock45Data.s_ScaleInverted));
            //        B45Block block = Block45Man.self.DataSource.SafeRead(ipos.x, ipos.y, ipos.z);

            //        if (block.blockType >> 2 == 0)
            //            return false;
            //    }
            //}

            //float blockLength = BuildBlockManager.MinBrushSize;
            float blockLength = BSBlock45Data.s_Scale;
            for (int x = -1; x <= 0; x++)
            {
                for (int z = -1; z <= 0; z++)
                {
                    Vector3 worldPos = m_Trans.trans.TransformPoint(x * blockLength, -blockLength, z * blockLength);
					int iposx = Mathf.FloorToInt(worldPos.x * BSBlock45Data.s_ScaleInverted);
                    int iposy = Mathf.FloorToInt(worldPos.y * BSBlock45Data.s_ScaleInverted);
                    int iposz = Mathf.FloorToInt(worldPos.z * BSBlock45Data.s_ScaleInverted);
                    B45Block block = Block45Man.self.DataSource.SafeRead(iposx, iposy, iposz);

                    if (block.blockType >> 2 == 0)
                        return false;
                }
            }

            return true;
        }

        public override void Start()
        {
            base.Start();

            m_Trans = GetComponent<PeTrans>();
            m_SkEntity = GetComponent<SkEntity>();

			Entity.peSkEntity.onHpChange += OnHpChange;
			Entity.peSkEntity.onHpReduce += OnDamage;
            Entity.peSkEntity.attackEvent += OnAttack;
			Entity.peSkEntity.onSkillEvent += OnSkillTarget;

            m_TowerEffect = new List<TowerEffect>();
            TowerProtoDb.Item item = TowerProtoDb.Get(Entity.ProtoID);
            if(item != null && item.effects != null && item.effects.Count > 0)
            {
                for (int i = 0; i < item.effects.Count; i++)
                {
                    m_TowerEffect.Add(new TowerEffect(item.effects[i].hpPercent, item.effects[i].effectID, item.effects[i].audioID));
                }
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if ((Time.frameCount % 30) != 0)
                return;

            m_OnBlock = CheckOnBuildTerrain();

            if(m_BillBoard != null)
            {
                if (m_NeedVoxel && !m_OnBlock)
                    m_BillBoard.gameObject.SetActive(true);
                else
                    m_BillBoard.gameObject.SetActive(false);
            }

            if (Entity.hasView)
            {
                for (int i = 0; i < m_TowerEffect.Count; i++)
                {
                    m_TowerEffect[i].ActivateEffect(Entity);
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (m_TowerEffect != null && m_TowerEffect.Count > 0)
            {
                for (int i = 0; i < m_TowerEffect.Count; i++)
                {
                    m_TowerEffect[i].Destroy();
                }
            }
        }

        public override void Serialize(System.IO.BinaryWriter w)
        {
            base.Serialize(w);

            w.Write(m_NeedVoxel);
            w.Write(m_SkillID);
            w.Write((int)m_CostType);
            w.Write(m_ConsumeItem);
            w.Write(m_ConsumeCost);
            w.Write(m_ConsumeCount);
            w.Write(m_ConsumeCountMax);
            w.Write(m_ConsumeEnergy);
            w.Write(m_ConsumeEnergyCost);
            w.Write(m_ConsumeEnergyMax);
        }

        public override void Deserialize(System.IO.BinaryReader r)
        {
            base.Deserialize(r);

            m_NeedVoxel = r.ReadBoolean();
            m_SkillID = r.ReadInt32();
            m_CostType = (ECostType)r.ReadInt32();
            m_ConsumeItem = r.ReadInt32();
            m_ConsumeCost = r.ReadInt32();
            m_ConsumeCount = r.ReadInt32();
            m_ConsumeCountMax = r.ReadInt32();
            m_ConsumeEnergy = r.ReadInt32();
            m_ConsumeEnergyCost = r.ReadInt32();
            m_ConsumeEnergyMax = r.ReadInt32();
        }

        public void OnMsg(EMsg msg, params object[] args)
        {
            switch (msg)
            {
                case EMsg.View_Prefab_Build:
					BiologyViewRoot viewRoot = (BiologyViewRoot)args[1];
                    m_Barrel = viewRoot.barrelController;
                    m_BillBoard = viewRoot.billBoard;

                    if(m_Target != null)
                        m_Barrel.AimTarget = m_Target;

				    break;
                case EMsg.View_Model_Destroy:
                    if(m_TowerEffect != null && m_TowerEffect.Count > 0)
                    {
                        for (int i = 0; i < m_TowerEffect.Count; i++)
                        {
                            m_TowerEffect[i].Destroy();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

		void OnAttack(SkEntity skEntity, float damage)
		{
			PeEntity tarEntity = skEntity.GetComponent<PeEntity>();
			if (tarEntity != null && tarEntity != Entity)
			{
				float tansDis = tarEntity.IsBoss ? 128f : 64f;
				int playerID = (int)Entity.GetAttribute(AttribType.DefaultPlayerID);
				bool canTrans = false;
				if (GameConfig.IsMultiClient)
				{
					if (ForceSetting.Instance.GetForceType(playerID) == EPlayerType.Human)
						canTrans = true;

					int tarPlayerId = (int)tarEntity.GetAttribute(AttribType.DefaultPlayerID);
					if (ForceSetting.Instance.GetForceType(tarPlayerId) == EPlayerType.Human)
					{
						List<PeEntity> entities = EntityMgr.Instance.GetEntities(tarEntity.position, tansDis, tarPlayerId, false, tarEntity);
						for (int i = 0; i < entities.Count; i++)
						{
							if (!entities[i].Equals(Entity) && entities[i].target != null)
								entities[i].target.TransferHatred(Entity, damage);
						}
					}
				}
				else
				{
					if (ForceSetting.Instance.GetForceID(playerID) == 1)
						canTrans = true;
				}
				
				if(canTrans)
				{
					List<PeEntity> entities = EntityMgr.Instance.GetEntities(Entity.position, tansDis, playerID, false, Entity);
					for (int i = 0; i < entities.Count; i++)
					{
						if (!entities[i].Equals(tarEntity) && entities[i].target != null)
							entities[i].target.TransferHatred(tarEntity, damage);
					}
				}
				
			}
		}

        void OnHpChange(SkEntity entity, float damage)
        {
            //for (int i = 0; i < m_TowerEffect.Count; i++)
            //{
            //    m_TowerEffect[i].ActivateEffect(Entity);
            //}
        }
		
		void OnDamage (SkEntity entity, float damage)
		{
			if (null == Entity.peSkEntity || null == entity)
				return;
			
			PeEntity peEntity = entity.GetComponent<PeEntity>();
			if(peEntity == Entity)
				return ;
			
			float tansDis = peEntity.IsBoss ? 128f : 64f;
			int playerID = (int)Entity.peSkEntity.GetAttribute((int)AttribType.DefaultPlayerID);
			
			bool canTrans = false;
			if (GameConfig.IsMultiClient)
			{
				if (ForceSetting.Instance.GetForceType(playerID) == EPlayerType.Human)
					canTrans = true;

				int tarPlayerId = (int)peEntity.GetAttribute(AttribType.DefaultPlayerID);
				if (ForceSetting.Instance.GetForceType(tarPlayerId) == EPlayerType.Human)
				{
					List<PeEntity> entities = EntityMgr.Instance.GetEntities(peEntity.position, tansDis, tarPlayerId, false, peEntity);
					for (int i = 0; i < entities.Count; i++)
					{
						if (!entities[i].Equals(Entity) && entities[i].target != null)
							entities[i].target.TransferHatred(Entity, damage);
					}
				}
			}
			else
			{
				if (ForceSetting.Instance.GetForceID(playerID) == 1)
					canTrans = true;
			}
			
			if(canTrans)
			{
				List<PeEntity> entities = EntityMgr.Instance.GetEntities (Entity.peTrans.position, tansDis, playerID, false, Entity);
				for(int i = 0; i < entities.Count; i++)
				{
					if (!entities[i].Equals (Entity) && entities[i].target != null) 
					{
						entities[i].target.TransferHatred (peEntity, damage);
					}
				}
			}
			
		}
		
		SkEntity GetCaster (SkEntity entity)
		{
			SkEntity skCaster = entity;
			
			Pathea.Projectile.SkProjectile pro = skCaster as Pathea.Projectile.SkProjectile;
			if (pro != null)
				skCaster = pro.GetSkEntityCaster ();
			
			WhiteCat.CreationSkEntity creation = skCaster as WhiteCat.CreationSkEntity;
			if (creation != null && creation.driver != null)
				skCaster = creation.driver.skEntity;
			
			return skCaster;
		}
		
		void OnSkillTarget(SkEntity caster)
		{
			if (null == Entity.aliveEntity || null == caster)
				return;
			
			int playerID = (int)Entity.aliveEntity.GetAttribute ((int)AttribType.DefaultPlayerID);
			PeEntity peEntity = caster.GetComponent<PeEntity>();
			if(peEntity == Entity)
				return ;
			
			float tansDis = peEntity.IsBoss ? 128f : 64f;
			bool canTrans = false;
			if (GameConfig.IsMultiClient)
			{
				if (ForceSetting.Instance.GetForceType(playerID) == EPlayerType.Human)
					canTrans = true;
			}
			else
			{
				if (ForceSetting.Instance.GetForceID(playerID) == 1)
					canTrans = true;
			}
			
			if (canTrans)
			{
				List<PeEntity> entities = EntityMgr.Instance.GetEntities (Entity.peTrans.position, tansDis, playerID, false, Entity);
				for(int i = 0; i < entities.Count; i++)
				{
					if (entities[i] == null)
						continue;
					
					if (!entities[i].Equals (Entity) && entities[i].target != null) 
					{
						entities[i].target.OnTargetSkill(peEntity.skEntity);
					}
				}
			}
		}


        public int ItemID
        {
            get { return m_ConsumeItem; }
        }

        public int ItemCount
        {
            get
            {
                return m_ConsumeCount;
            }
            set
            {
                ConsumeCount = value;
            }
        }

        public int ItemCountMax
        {
            get { return m_ConsumeCountMax; }
        }

        public int EnergyCount
        {
            get
            {
                return m_ConsumeEnergy;
            }
            set
            {
                ConsumeEnergy = value;
            }
        }

        public int EnergyCountMax
        {
            get { return m_ConsumeEnergyMax; }
        }

        public ECostType ConsumeType
        {
            get { return m_CostType; }
        }
    }
}
