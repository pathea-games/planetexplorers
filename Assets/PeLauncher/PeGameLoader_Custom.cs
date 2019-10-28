using System.Collections.Generic;
using UnityEngine;

using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using PeCustom;

namespace Pathea
{
	namespace GameLoader
	{
		class LoadCustomEntityCreator : ModuleLoader
		{

			YirdData mInfo;
			public LoadCustomEntityCreator(bool bNew,  YirdData info)
				: base(bNew)
			{
				mInfo = info;
			}
			
			protected override void New()
			{
//				LoadPrefab();
				
//				EntityCreateMgr.Instance.New();
				
				CreateCreature();
			}
			
			protected override void Restore()
			{
				PeCustomScene.Self.SceneRestore(mInfo);
			}
			
			void CreateCreature()
			{
				PeCustomScene.Self.SceneNew(mInfo);

			}
		}

		class LoadCustomPlayerSpawnPos : LoadPlayerSpawnPos
		{
			Vector3 mPos;
			
			public LoadCustomPlayerSpawnPos(bool bNew, Vector3 pos) : base(bNew) { mPos = pos; }
			
			protected override void New()
			{
				SetPos(mPos);
			}
		}

        class LoadCustomStory : LoadScenario
		{
			CustomGameData mData;
			
			public LoadCustomStory(bool bNew, CustomGameData data)
				: base(bNew)
			{
                mData = data;
			}
			

			protected override void Init()
			{
                // [INIT SCENARIO]
                PeCustomScene.Self.ScenarioInit(mData);
            }
			
			protected override void New()
			{
				base.New();

                Init();
            }

            protected override void Restore()
            {
                base.Restore();

                Init();
                // [INIT SCENARIO]
                PeCustomScene.Self.ScenarioRestore();
            }
        }

        class LoadMultiCustom : LoadScenario
        {
            CustomGameData mData;

            public LoadMultiCustom(bool bNew, CustomGameData data)
                : base(bNew)
            {
                mData = data;
            }


            protected override void Init()
            {
                // [INIT SCENARIO]
                PeCustomScene.Self.ScenarioInit(mData);
            }

            protected override void New()
            {
                base.New();

                Init();
            }

            protected override void Restore()
            {
                base.Restore();

                Init();
                // [INIT SCENARIO]
                PeCustomScene.Self.ScenarioRestore();
            }
        }

        class LoadCustomDragItem : PeLauncher.ILaunchable
		{
			IEnumerable<WEItem> mItems;
			public LoadCustomDragItem(IEnumerable<WEItem> items)
			{
				mItems = items;
			}
			
			void PeLauncher.ILaunchable.Launch()
			{
				if (mItems == null)
				{
					return;
				}
				
//				PeCustomMan.Self.staticObjMgr.SetDragItems(mItems);

			}

		}

		class LoadCustomDoodad : ModuleLoader
		{
			IEnumerable<WEDoodad> mItems;
			
			public LoadCustomDoodad(bool bNew,  IEnumerable<WEDoodad> items)
				: base(bNew)
			{
				mItems = items;
			}


			protected override void New()
			{
				if (null == mItems)
				{
					return;
				}

//				PeCustomMan.Self.staticObjMgr.SetDoodads(mItems);
			}
			
			protected override void Restore()
			{
//				PeCustomMan.Self.staticObjMgr
			}
		}

		class LoadCustomSceneEffect : PeLauncher.ILaunchable
		{
			IEnumerable<WEEffect> mEffects;

            public LoadCustomSceneEffect(IEnumerable<WEEffect> effects)
			{
				mEffects = effects;
			}
			
			void PeLauncher.ILaunchable.Launch()
			{
				if (null == mEffects)
				{
					return;
				}
				
//				foreach (WEEffect item in mEffects)
//				{
//					SceneStaticEffectAgent a = SceneStaticEffectAgent.Create(item.Prototype, item.Position, item.Rotation, item.Scale, item.ID);
//                    
//                    SceneMan.AddSceneObj(a);
//                }
//				PeCustomMan.Self.staticObjMgr.SetEffects(mEffects);
            }
        }


		class LoadSingleCustomInitData : ModuleLoader
		{
			public LoadSingleCustomInitData(bool bNew) : base(bNew) { }
			
			protected override void New()
			{
				SingleGameInitData.AddCustomInitData();
			}
			
			protected override void Restore()
			{
				
			}
		}

		class LoadCustomCreature : ModuleLoader
		{
			public LoadCustomCreature(bool bNew) : base(bNew) { }
			
			protected override void New()
			{
				PeCreature.Instance.New();
				PeCustom.CreatureMgr.Instance.New();
				MainPlayer.Instance.New();

				PeCreature.Instance.destoryEntityEvent += PeCustom.CreatureMgr.Instance.OnPeCreatureDestroyEntity;

				Vector3 pos = GetPlayerSpawnPos();
				Debug.Log("player init pos:" + pos);
				PeEntity player = MainPlayer.Instance.CreatePlayer(
					WorldInfoMgr.Instance.FetchRecordAutoId(),
					pos, Quaternion.identity, Vector3.one,
					CustomCharactor.CustomDataMgr.Instance.Current
					);
				

				PeTrans v = player.peTrans;
				v.position = pos;
			}
			
			protected override void Restore()
			{
				PeCreature.Instance.Restore();
				PeCustom.CreatureMgr.Instance.Restore();
				MainPlayer.Instance.Restore();
			}
			
			Vector3 GetPlayerSpawnPos()
			{
				return Pathea.PlayerSpawnPosProvider.Instance.GetPos();
			}
		}
		
	}
}