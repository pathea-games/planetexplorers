using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SkillSystem;

namespace Pathea
{
	// For doodad additional save data
	public class SceneDoodadLodCmpt : LodCmpt, IPeMsg {
		public static event Action<SkEntity, SkEntity> commonDeathEvent;

		protected SceneObjAdditionalSaveData _additionalData = new SceneObjAdditionalSaveData();
		// Ver use byte to fit with boolean(1 byte), >=2 because boolean is 0 or 1;
		const byte c_VerToAddIsDamagable = 2;
		const byte c_VerToAddIsShown = 3;
		const byte c_CurVer = c_VerToAddIsShown;
		private bool _bShown = false;
		private bool _bDamagable = false;
		private IBoundInScene _bound = null;
		// properties
		public bool IsConstructed{	get{	return Entity.viewCmpt == null || Entity.viewCmpt.hasView;	}	}
		public int Index{ get; set; }	// In story mode, it is story asset id; In adventure mode, it is townid+xxx coded number
		public bool IsShown{
			get{
				return _bShown;
			}
			set{
				if(_bShown == value)	return;

				_bShown = value;
				if(_bShown){
					SceneMan.AddSceneObj(this);
				} else {
					SceneMan.RemoveSceneObj(this);
					if(IsConstructed){
						OnDestruct();
					}
				}
			}
		}
		public void SetShowVar(bool bShown)
		{
			_bShown = bShown;
		}
		public bool IsDamagable{
			get{
				return _bDamagable;
			}
			set{
				_bDamagable = value;
				SkAliveEntity skAlive = Entity.aliveEntity;
				if (skAlive != null) {
					if(_bDamagable){
						skAlive.SetAttribute(AttribType.CampID, SceneDoodadDesc.c_playerAttackableCamp);
						skAlive.SetAttribute(AttribType.DamageID, SceneDoodadDesc.c_playerAttackableDamage);
//						skAlive.deathEvent += DoodadEntityCreator.OnDoodadDeath;
					} else {
						skAlive.SetAttribute(AttribType.CampID, SceneDoodadDesc.c_neutralCamp);
						skAlive.SetAttribute(AttribType.DamageID, SceneDoodadDesc.c_neutralDamage);
//						skAlive.deathEvent -= DoodadEntityCreator.OnDoodadDeath;
					}
				}
			}
		}
		public void SetDamagable(int campId, int damageId,int playerId)
		{
			SkAliveEntity skAlive = Entity.aliveEntity;
			if (skAlive != null) {
				skAlive.SetAttribute(AttribType.CampID, campId);
				skAlive.SetAttribute(AttribType.DamageID, damageId);
				_bDamagable = campId != SceneDoodadDesc.c_neutralCamp || damageId != SceneDoodadDesc.c_neutralDamage;
//				if(_bDamagable){
//					skAlive.deathEvent += DoodadEntityCreator.OnDoodadDeath;
//				} else {
//					skAlive.deathEvent -= DoodadEntityCreator.OnDoodadDeath;
//				}
				if(playerId>=0)
					skAlive.SetAttribute(AttribType.DefaultPlayerID, playerId);
			}
		}
		// Override
		public override void Start()
		{
			BaseStart ();

			DoodadProtoDb.Item protoItem = DoodadProtoDb.Get(Entity.entityProto.protoId);
			_bound = new RadiusBound (protoItem.range, Entity.peTrans.trans);
			SkAliveEntity skAlive = Entity.aliveEntity;
			skAlive.deathEvent += DoodadEntityCreator.OnDoodadDeath;
			// Do not add to scene man here, set IsShown would do this thing
		}

		#region ISceneObjAgent
		public override GameObject Go		{	get{ 	return (Equals(null)||!Entity.hasView) ? null : Entity.gameObject;		}	}  // In Dep check, Go is treated as Model check, so add HasView
		public override IBoundInScene Bound{	get{ 	return _bound;				}	}
		public override bool NeedToActivate	{	get{	return false;         		}   }
		public override bool TstYOnActivate	{	get{	return false;         		}   }
		public override void OnConstruct()
		{
			if(Equals(null)){
				SceneMan.RemoveSceneObj(this);
				return;
			}

			BuildView ();
			if (onConstruct != null){
				onConstruct(Entity);
			}
		}
		public override void OnDestruct()
		{
			if(Equals(null)){
				SceneMan.RemoveSceneObj(this);
				return;
			}

			if (onDestruct != null){
				onDestruct(Entity);
			}
			DestroyView();
		}
		public override void OnActivate()
		{
			if(Equals(null)){
				SceneMan.RemoveSceneObj(this);
				return;
			}

			if (onActivate != null){
				onActivate(Entity);
			}
		}
		public override void OnDeactivate()
		{
			if(Equals(null)){
				SceneMan.RemoveSceneObj(this);
				return;
			}

			if (onDeactivate != null){
				onDeactivate(Entity);
			}
		}
		#endregion

		public void OnMsg(EMsg msg, params object[] args)
		{
			switch (msg) {
			case EMsg.View_Prefab_Build:
				DispatchViewData();
				break;
				
			case EMsg.View_Prefab_Destroy:
				CollectViewData ();
				break;
			}
		}

		public override void Deserialize(System.IO.BinaryReader r)
		{
			base.Deserialize (r);
			byte ver = r.ReadByte ();
			if (ver < c_VerToAddIsDamagable) {
				IsDamagable = ver != 0;
			} else if(ver == c_VerToAddIsDamagable){
				/*IsDamagable = */r.ReadBoolean ();
				Index = r.ReadInt32();
			} else if(ver == c_VerToAddIsShown){
				IsShown = r.ReadBoolean();
				/*IsDamagable = */r.ReadBoolean ();	// camp/damage has saved in skaliveentity
				Index = r.ReadInt32();
			} else{
				Debug.LogError("Unrecognized doodad save data:" + ver);
				return;
			}
			_additionalData.Deserialize (r);
			DispatchViewData ();
		}
		public override void Serialize(System.IO.BinaryWriter w)
		{
			base.Serialize(w);
			w.Write (c_CurVer);
			w.Write (IsShown);
			w.Write (IsDamagable);
			w.Write (Index);
			CollectViewData ();
			_additionalData.Serialize (w);
		}

		void CollectViewData()
		{
			if (Entity.viewCmpt != null && Entity.biologyViewCmpt.tView != null) {
				_additionalData.CollectData(Entity.biologyViewCmpt.tView.gameObject);
			}
		}
		void DispatchViewData()
		{
			if (Entity.viewCmpt != null && Entity.biologyViewCmpt.tView != null) {
				_additionalData.DispatchData(Entity.biologyViewCmpt.tView.gameObject);
			}
		}
	}
}
