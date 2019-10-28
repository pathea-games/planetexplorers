using UnityEngine;
using Pathea;
using System.Collections;
using System.Collections.Generic;
using Pathea.PeEntityExtNpcPackage;
using Pathea.PeEntityExt;
using ItemAsset;

namespace Pathea
{

	public class SupplyNum
	{
        //proto ID
        public const int ARROW = 49;
        public const int BULLET = 50;
        public const int BATTERY = 228;


		public const int ARROW_Supply_Max = 100;
        public const int ARROW_Supply_Min = 10;

		public const int BULLET_Supply_Max = 100;
        public const int BULLET_Supply_Min = 20;

		public const int BATTERY_Supply_Num = 1;
        public const float BATTERY_Supply_MinPercent =0;
        public const float BATTERY_Supply_MaxPercent = 0.5f;
	}

    public enum ESupplyType
    {
        Ammo,//弹药消耗：子弹、箭矢、电池
        Weapon,
        //Expend_Equip,
        //Expend_Food,
        MAX
    }

    public class NpcSupplyAgent
    {
        StrorageSupply[] mCsSupplies;
        public NpcSupplyAgent()
        {
            mCsSupplies = new StrorageSupply[(int)ESupplyType.MAX];
            mCsSupplies[(int)ESupplyType.Ammo] = new StorageSupply_Ammo();
            mCsSupplies[(int)ESupplyType.Weapon] = new StrorageSupply_Weapon();
        }

        public bool Supply(ESupplyType type, PeEntity entity, CSAssembly assembly)
        {
            return mCsSupplies[(int)type] != null ? mCsSupplies[(int)type].DoSupply(entity,assembly) : false;
        }

    }

	public class NpcSupply 
	{
        static NpcSupplyAgent m_Agent = new NpcSupplyAgent();

        public static bool AgentSupply(PeEntity npc,CSAssembly csAssembly ,ESupplyType type)
        {
            if (m_Agent == null)
                m_Agent = new NpcSupplyAgent();

            return m_Agent.Supply(type, npc, csAssembly);
        }

		/******************************************************
         * 基地仓库给特定npc补给(从仓库移除，添加到NPC背包)
         * ***************************************************/
        public static bool CsStorageSupply(PeEntity npc,CSAssembly assembly,int protoId,int count)
        {
            if (assembly == null || assembly.Storages == null || npc.packageCmpt == null)
                return false;

            int curCount = CSUtils.GetItemCounFromFactoryAndAllStorage(protoId, assembly);
            if (curCount > count)
            {
                if (CSUtils.CountDownItemFromAllStorage(protoId, count, assembly))
                {
                    return npc.packageCmpt.Add(protoId, count);
                }
            }
            else if (curCount > 0)
            {
                if (CSUtils.CountDownItemFromAllStorage(protoId, curCount, assembly))
                {
                    return npc.packageCmpt.Add(protoId, curCount);
                }
            }
            return false;
        }

         /************************************************
         * 与仓库交换电池
         * *********************************************/
        public static bool CsStrargeSupplyExchangeBattery(PeEntity npc, CSAssembly assembly,  List<ItemObject> objs, int protoId, int count = 1)
        {
            if (assembly == null || assembly.Storages == null || npc.packageCmpt == null)
                return false;

            List<ItemObject> curObjs = CSUtils.GetItemListInStorage(protoId, assembly);
            //当前背包里不可以用的item
            if(objs != null)
            {
                for (int i = 0; i < objs.Count;i++)
                {
                    //put back
                    if(CSUtils.AddItemObjToStorage(objs[i].instanceId,assembly))
                        npc.packageCmpt.Remove(objs[i]);
                }
            }

            if (curObjs.Count <= 0)
                return false;

            for (int i = 0; i < curObjs.Count;i++)
            {
                Energy energy = curObjs[i].GetCmpt<Energy>();
                if(energy != null && energy.floatValue.percent >0)
                {
                    if (CSUtils.RemoveItemObjFromStorage(curObjs[i].instanceId, assembly))
                        return  npc.packageCmpt.Add(curObjs[i]);

                }
            }
 
            return false;
        }

        public static void CsStorageSupplyNpcs(List<PeEntity> npcs, ESupplyType type)
        {
            if (npcs == null)
                return;

            for (int i = 0; i < npcs.Count;i++ )
            {
                if (npcs[i].NpcCmpt != null && npcs[i].NpcCmpt.Creater != null && npcs[i].NpcCmpt.Creater.Assembly != null)
                    AgentSupply(npcs[i], npcs[i].NpcCmpt.Creater.Assembly, type);
            }
        }

        public static void SupplyNpcsByCSAssembly(List<PeEntity> npcs, CSAssembly assembly, ESupplyType type)
        {
            if (npcs == null)
                return;

            for (int i = 0; i < npcs.Count; i++)
            {
                AgentSupply(npcs[i], assembly, type);
            }
          
        }

	}

}

