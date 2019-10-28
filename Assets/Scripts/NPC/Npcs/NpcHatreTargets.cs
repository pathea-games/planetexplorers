using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea.Operate;
using PETools;
using SkillSystem;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Mono.Data.SqliteClient;

namespace Pathea
{
	public class NpcHatreTargets : MonoBehaviour {
		
		
		static  NpcHatreTargets mInstance;
		public static NpcHatreTargets Instance { get {return mInstance;}}


		public List<PeEntity> mHatredTargets;
		//private List<PeEntity> tempList;

		void Awake(){
			mInstance = this;
			mHatredTargets = new List<PeEntity>();
			//tempList = new List<PeEntity>();
		}
			// Use this for initialization
		void Start () {
			StartCoroutine(ReflashTargets(10.0f));
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		IEnumerator ReflashTargets(float time)
		{
			while(true)
			{
				if(mHatredTargets.Count >0)
				{
                    //tempList.Clear();
                    //for(int i=0;i<mHatredTargets.Count;i++)
                    //{
                    //    if(mHatredTargets[i] != null)
                    //        tempList.Add(mHatredTargets[i]);
                    //}
                    //mHatredTargets.Clear();
                    //mHatredTargets.AddRange(tempList);
                    mHatredTargets.Clear();
				}
				yield return new WaitForSeconds(time);
			}
		}

        bool ContainTarget(PeEntity target)
        {
            return mHatredTargets.Contains(target);
        }

        void AddTarget(PeEntity target)
        {
			if(!ContainTarget(target))
               mHatredTargets.Add(target);
        }

        bool RemoveTarget(PeEntity target)
        {
            return mHatredTargets.Remove(target);
        }

        public  void TryAddInTarget(PeEntity SelfEntity, PeEntity TargetEntity, float damage,bool trans = false)
		{
			if(TargetEntity == null)
				return ;

			if (!trans && !ContainTarget(TargetEntity))
			{
				AddTarget(TargetEntity);
			}
               
            float tansDis = TargetEntity.IsBoss || SelfEntity.proto == EEntityProto.Doodad ? 128f : 64f;
            int playerID = (int)SelfEntity.GetAttribute(AttribType.DefaultPlayerID);   //(int)SeldEntity.GetAttribute((int)AttribType.DefaultPlayerID);

            bool canTrans = false;
            if (GameConfig.IsMultiClient)
            {
                //if (ForceSetting.Instance.GetForceType(playerID) == EPlayerType.Human)
                    canTrans = true;
            }
            else
            {
                //if (ForceSetting.Instance.GetForceID(playerID) == 1)
                    canTrans = true;
            }

            if (canTrans)
            {
                List<PeEntity> entities = null;
                if (playerID != 5 && playerID != 6)
                    entities = EntityMgr.Instance.GetEntities(SelfEntity.peTrans.position, tansDis, playerID, false, SelfEntity);
                else
                    entities = EntityMgr.Instance.GetEntitiesFriendly(SelfEntity.peTrans.position, tansDis, playerID, SelfEntity.ProtoID, false, SelfEntity);

                for (int i = 0; i < entities.Count; i++)
                {
                    if (!entities[i].Equals(SelfEntity) && entities[i].target != null)
                    {
                        entities[i].target.TransferHatred(TargetEntity, damage);
                    }
                }
            }
		}

        public void OnEnemyLost(PeEntity TargetEntity)
        {
            RemoveTarget(TargetEntity);
        }
	}
}

