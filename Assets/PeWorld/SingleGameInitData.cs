using ItemAsset.PackageHelper;
using Pathea.PeEntityExt;
using System.Collections.Generic;
using UnityEngine;
using ItemAsset;
using System.Linq;

namespace Pathea
{
    public static class SingleGameInitData
    {
        static void AddItemToPlayer(ItemAsset.MaterialItem[] items)
        {
            PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
            if (null == pkg)
            {
                return;
            }

            foreach (ItemAsset.MaterialItem item in items)
            {
                pkg.package.Add(item.protoId, item.count);
				//Use default itembox
				ItemObject obj = pkg.package._playerPak.GetItemByProtoID(item.protoId);
				if(null != obj)
					PeCreature.Instance.mainPlayer.UseItem.Use(obj);
            }
			pkg.package._playerPak.Sort (ItemPackage.ESlotType.Item);
        }

        static void AddMoneyToPlayer(int money)
        {
            PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
            if (null == pkg)
            {
                return;
            }

            pkg.money.current = money;
        }

        static void AddStoryPresent()
        {
            ItemAsset.MaterialItem[] items = { new ItemAsset.MaterialItem{protoId = 1358, count = 1}

                                                 //test
                                                 //,  new ItemAsset.MaterialItem{protoId = 1277, count = 1}
                                                 //,  new ItemAsset.MaterialItem{protoId = 229, count = 1998}

                                                 //,  new ItemAsset.MaterialItem{protoId = 1031, count = 1}
                                                 //,  new ItemAsset.MaterialItem{protoId = 1032, count = 10}
                                                 //,  new ItemAsset.MaterialItem{protoId = 1033, count = 2}
                                                 //,  new ItemAsset.MaterialItem{protoId = 1034, count = 2}
                                             };

            AddItemToPlayer(items);
        }

        static void AddAdventurePresent()
        {
            List<ItemAsset.MaterialItem> items = new List<ItemAsset.MaterialItem>();
            items.Add(new ItemAsset.MaterialItem { protoId = 1289, count = 1 });
            //lz-2017.03.14 加入全脚本功能
            if (RandomMapConfig.openAllScripts) items.Add(new ItemAsset.MaterialItem { protoId = 1743, count = 1 });
            AddItemToPlayer(items.ToArray());
            AddMoneyToPlayer(500);
        }

        static void AddBuildPresent()
        {
			ItemAsset.MaterialItem[] items = new MaterialItem[1];
#if DemoVersion
			items[0] = new ItemAsset.MaterialItem{protoId = 1725, count = 1};
#else
			items[0] = new ItemAsset.MaterialItem{protoId = 1291, count = 1};
#endif

            AddItemToPlayer(items);
            AddMoneyToPlayer(500);
        }

        static void AddDefaultClothToPlayer()
        {
            int[] initEquip = new int[]{113, 149, 210, 95, 131, 192};

            InitEquipment(PeCreature.Instance.mainPlayer, initEquip);
        }

        static void InitEquipment(PeEntity entity, IEnumerable<int> equipmentItemProtoIds)
        {
            if (equipmentItemProtoIds == null)
            {
                return;
            }

            Pathea.EquipmentCmpt equipmentCmpt = entity.GetCmpt<Pathea.EquipmentCmpt>();
            if (null == equipmentCmpt)
            {
                Debug.LogError("no equipment cmpt");
                return;
            }

            PeSex entitySex = entity.ExtGetSex();

            foreach (int equipmentItemProtoId in equipmentItemProtoIds)
            {
                ItemAsset.ItemProto itemProto = ItemAsset.ItemProto.Mgr.Instance.Get(equipmentItemProtoId);
                if (itemProto == null)
                {
                    continue;
                }

                if (!PeGender.IsMatch(itemProto.equipSex, entitySex))
                {
                    continue;
                }

                ItemAsset.ItemObject itemObj = ItemAsset.ItemMgr.Instance.CreateItem(equipmentItemProtoId);
                if (itemObj != null)
                {
                    equipmentCmpt.PutOnEquipment(itemObj);
                }
            }
        }

        //static void AddTestReplicatorFormula()
        //{
        //    PeEntity entity = PeCreature.Instance.mainPlayer;
        //    ReplicatorCmpt r = entity.GetCmpt<ReplicatorCmpt>();
        //    if (null == r)
        //    {
        //        r = entity.Add<ReplicatorCmpt>();
        //    }

        //    for (int i = 1; i < 10; i++)
        //    {
        //        r.replicator.AddFormula(i);
        //    }

        //    r.replicator.AddFormula(384);
        //    r.replicator.AddFormula(385);
        //    r.replicator.AddFormula(386);
        //}

        //static void CreateTestMonster()
        //{
        //    int testMonsterProtoId = 3;//13

        //    int id = WorldInfoMgr.Instance.FetchNonRecordAutoId();

        //    PeEntity entity = PeEntityCreator.Instance.CreateMonster(id, testMonsterProtoId);
        //    if (null == entity)
        //    {
        //        return;
        //    }

        //    SetPos(entity, GetRandomPosNearPlayer());
        //}

        //static void CreateSaveTestMonster()
        //{
        //    int testMonsterProtoId = 3;//13

        //    int id = WorldInfoMgr.Instance.FetchRecordAutoId();

        //    PeEntity entity = PeCreature.Instance.CreateMonster(id, testMonsterProtoId);
        //    if (null == entity)
        //    {
        //        return;
        //    }

        //    SetPos(entity, GetRandomPosNearPlayer());
        //}

        static Vector3 GetRandomPosNearPlayer()
        {
            PeTrans playerTrans = PeCreature.Instance.mainPlayer.peTrans;
            return playerTrans.position;
        }

        static Vector3 GetRandomPos(Vector3 pos)
        {
            return pos + Vector3.forward * Random.Range(0f, 18f) + Vector3.left * Random.Range(0f, 18f) + Vector3.up * Random.Range(0f, 3f);
        }

        static void SetPos(Pathea.PeEntity entity, Vector3 pos)
        {
            PeTrans trans = entity.peTrans;

            if (null == trans)
            {
                Debug.LogError("entity has no PeTrans");
                return;
            }

            trans.position = pos;
        }

        static void CreateTestRandomNpc()
        {
            int id = WorldInfoMgr.Instance.FetchRecordAutoId();

            PeEntity entity = PeCreature.Instance.CreateRandomNpc(1, id, Vector3.zero, Quaternion.identity, Vector3.one);
            if (null == entity)
            {
                Debug.LogError("create random npc failed");
                return;
            }

            SetPos(entity, GetRandomPos(GetRandomPosNearPlayer()));
        }

        static void CreateTestNpc()
        {
            int testNpcProtoId = 1;
            int id = WorldInfoMgr.Instance.FetchRecordAutoId();

			PeEntity entity = PeCreature.Instance.CreateNpc(id, testNpcProtoId, Vector3.zero, Quaternion.identity, Vector3.one);
            if (null == entity)
            {
                Debug.LogError("create random npc failed");
                return;
            }

            SetPos(entity, GetRandomPosNearPlayer());
        }

        public static void AddTestData()
        {
            AddStoryPresent();
            //AddTestReplicatorFormula();
            //CreateNpcFromDb();
            //CreateSaveTestMonster();
            //CreateTestMonster();

            //for (int i = 0; i < 10; i++)
            //{
            //    CreateTestRandomNpc();
            //}
        }

        public static void AddStoryInitData()
        {
            AddDefaultClothToPlayer();

            AddStoryPresent();
        }

        public static void AddBuildInitData()
        {
            AddDefaultClothToPlayer();

            AddBuildPresent();
        }

        public static void AddAdventureInitData()
        {
            AddDefaultClothToPlayer();

            AddAdventurePresent();
        }

        public static void AddCustomInitData()
        {
            AddDefaultClothToPlayer();
        }

		public static void AddTutorialInitData()
		{
			AddDefaultClothToPlayer();
		}
    }
}