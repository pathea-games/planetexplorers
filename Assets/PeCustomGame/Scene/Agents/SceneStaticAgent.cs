// Custom game scene mode static agent : Such as Doodad, Effect
// (c) by Wu Yiqiu


using UnityEngine;
using System.Collections;
using Pathea;

namespace PeCustom
{
	public class SceneStaticAgent : PeCustomScene.SceneElement, ISceneObjAgent
	{
		#region ISceneObjAgent

		public int Id { get; set;}
        public int ScenarioId { get; set; }
		public GameObject Go{		get { return (entity == null) ? null : entity.gameObject;	}}
		public Vector3 Pos { 		get { return mPoint.spawnPos; }  }
		public IBoundInScene Bound{	get	{ return null;}}
        public bool NeedToActivate{	get { return false;}}
		public bool TstYOnActivate{ get { return false;}}

		public void OnActivate() {}

		public void OnConstruct()
		{
			if (!spawnPoint.isDead && entity == null
                && (spawnPoint.Enable || spawnPoint.EntityID != -1))
				CreateEntityStatic();
		}
		
		public void OnDeactivate() {}
		public void OnDestruct() {}

		#endregion

		public PeEntity entity{ get; set; }
		public int protoId {get { return mPoint.Prototype;}}
		public  Quaternion Rot { get { return mPoint.Rotation; }}
		public  Vector3 Scale { get { return mPoint.Scale; } }

		public bool IsTarget { get { return mPoint.IsTarget; } }
		public bool Visible { get { return mPoint.Visible; }}

		private SpawnPoint mPoint;
		public SpawnPoint spawnPoint { get { return mPoint;} }

		public bool IsSave { get; private set;}

		public SceneStaticAgent(DoodadSpawnPoint sp, bool is_saved)
		{
			mPoint = sp;
			IsSave = is_saved;
		}

		void CreateEntityStatic()
		{
			scene.Notify(ESceneNoification.CreateDoodad, this, IsSave);

			if (entity != null)
			{
                entity.scenarioId = ScenarioId;

                SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
                if (skAlive != null)
                {
                    skAlive.deathEvent += OnEntityDeath;
                }


                LodCmpt entityLodCmpt = entity.lodCmpt;
				if(entityLodCmpt != null)
				{
						entityLodCmpt.onDestruct += (e)=>{DestroyEntity();};
				}
			}
			
		}

        public bool ForceCreateEntity ()
        {
            if (spawnPoint.isDead)
                return false;

            CreateEntityStatic();
            return true;
        }


        public void DestroyEntity()
		{
			if (entity != null) 
			{
				scene.Notify(ESceneNoification.EntityDestroy, mPoint, entity);
				entity = null;
			}
		}

        void OnEntityDeath(SkillSystem.SkEntity cur, SkillSystem.SkEntity caster)
        {
            SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
            if (cur == skAlive)
            {
                scene.Notify(ESceneNoification.DoodadDead, this);
                SceneMan.RemoveSceneObj(this);
                Debug.Log("Doodad Entity id [" + skAlive.GetId() + "] is dead");
            }
        }


    }
}

