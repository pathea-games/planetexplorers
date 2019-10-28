using UnityEngine;

namespace Pathea
{
    public class LodCmpt : PeCmpt, ISceneObjAgent
    {
		protected void BaseStart()	// for
		{
			base.Start ();
		}

        public override void Start()
        {
			base.Start ();
			SceneMan.AddSceneObj(this);            
        }
        public override void OnDestroy()
        {
            SceneMan.RemoveSceneObj(this);           
			if (onDestroyEntity != null) 
				onDestroyEntity (Entity);
        }

		protected void EnablePhy()
        {
            if (Entity.motionMgr != null)
            {
				Entity.motionMgr.FreezePhySteateForSystem(false);
            }
        }
		protected void DisablePhy()
        {
			if (Entity.motionMgr != null)
            {
				Entity.motionMgr.FreezePhySteateForSystem(true);
            }
        }
		protected void BuildView()
        {
			Entity.biologyViewCmpt.Build();
        }

        void FadeIn()
        {
			Entity.biologyViewCmpt.Fadein();
            //Debug.LogError("FadeIn : " + Entity.Id);
        }

        void FadeOut()
        {
            Entity.biologyViewCmpt.Fadeout();
            //Debug.LogError("FadeOut : " + Entity.Id);
        }

        public  void DestroyView()
        {
            //servant
//            NpcCmpt c = Entity.NpcCmpt;
//			if (c != null && c.Alive != null)
//            {
//                PeEntity mainPlayer = PeCreature.Instance.mainPlayer;
//                if (mainPlayer != null)
//                {
//                    ServantLeaderCmpt servant = mainPlayer.GetCmpt<ServantLeaderCmpt>();
//					if (!c.Alive.isDead && c.Master == servant)
//                    {
//                        return;
//                    }
//                }
//            }

            //main player
			if (Entity.Id == PeCreature.Instance.mainPlayerId || Entity.viewCmpt == null)
            {
                return;
            }

			Entity.biologyViewCmpt.Destroy();
        }

		#region ISceneObjAgent
		int ISceneObjAgent.Id						{	get;set;}
		int ISceneObjAgent.ScenarioId 				{ 	get;set;}
		Vector3 ISceneObjAgent.Pos  				{   get{	return Equals(null) ? Vector3.zero : Entity.peTrans.position;	}	}		
		public virtual GameObject Go				{	get{ 	return Equals(null) ? null : Entity.gameObject;					}	} 
		public virtual IBoundInScene Bound			{	get{ 	return null;				}	}
		public virtual bool NeedToActivate			{   get{	return true;         		}   }
		public virtual bool TstYOnActivate			{	get{	return true;				}	}
        public virtual void OnConstruct()
        {
			if(Equals(null)){
				SceneMan.RemoveSceneObj(this);
				return;
			}

            if (!PeGameMgr.IsMulti && Entity.Field == MovementField.Sky && Entity.gravity > - PETools.PEMath.Epsilon && Entity.gravity < PETools.PEMath.Epsilon)
            {
                CancelInvoke("FadeOut");
                CancelInvoke("DelayDestroy");

                if (!Entity.hasView)
                    BuildView();
                else
                    FadeIn();

                EnablePhy();
            }

            if (onConstruct != null)
                onConstruct(Entity);
        }
		public virtual void OnDestruct()
        {
			if(Equals(null)){
				SceneMan.RemoveSceneObj(this);
				return;
			}

			if (Entity.hasView)
            {
                if(!IsInvoking("FadeOut"))
                    Invoke("FadeOut", 3f);

                if(!IsInvoking("DelayDestroy"))
                    Invoke("DelayDestroy", 5f);

                DisablePhy();
            }

            if (onDestruct != null)
                onDestruct(Entity);
        }
		public virtual void OnActivate()
        {
			if(Equals(null)){
				SceneMan.RemoveSceneObj(this);
				return;
			}

            //if (Entity.Equals(PeCreature.Instance.mainPlayer))
            //    Debug.LogError("OnActivate £º " + Time.time);

			Profiler.BeginSample ("LodActive:"+name);

            CancelInvoke("FadeOut");
            CancelInvoke("DelayDestroy");

            if (!Entity.hasView)
                BuildView();
            else
                FadeIn();

			EnablePhy();

            Entity.SendMsg(EMsg.Lod_Collider_Created);
            if (onActivate != null)
                onActivate(Entity);
			Profiler.EndSample ();
        }
		public virtual void OnDeactivate()
        {
			if(Equals(null)){
				SceneMan.RemoveSceneObj(this);
				return;
			}

            //if (Entity.Equals(PeCreature.Instance.mainPlayer))
            //    Debug.LogError("OnDeactivate £º " + Time.time);

            //			if(Entity.IsDeath())
            //				DestroyView();

            if (PeGameMgr.IsMulti || Entity.Field != MovementField.Sky)
            {
                if (!Entity.Equals(PeCreature.Instance.mainPlayer))
                {
                    if (!IsInvoking("FadeOut"))
                        Invoke("FadeOut", 3f);
                    if (!IsInvoking("DelayDestroy"))
                        Invoke("DelayDestroy", 5f);
                }

                DisablePhy();
            }

            Entity.SendMsg(EMsg.Lod_Collider_Destroying);
            if (onDeactivate != null)
                onDeactivate(Entity);
        }
		#endregion

		void DelayDestroy()
		{
			DestroyView();

			if (onDestroyView != null) 
				onDestroyView (Entity);
		}

        public System.Action<PeEntity> onConstruct;
        public System.Action<PeEntity> onDestruct;
        public System.Action<PeEntity> onActivate;
        public System.Action<PeEntity> onDeactivate;
		public System.Action<PeEntity> onDestroyView;
		public System.Action<PeEntity> onDestroyEntity;
    }
}