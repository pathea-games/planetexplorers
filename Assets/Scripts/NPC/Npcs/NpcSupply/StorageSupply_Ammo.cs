using UnityEngine;
using Pathea;
using System.Collections;
using System.Collections.Generic;
using Pathea.PeEntityExtNpcPackage;
using Pathea.PeEntityExt;
using ItemAsset;

namespace Pathea
{
    public enum EAtkRangeExpend
    {
        Bullet,
        Arrow,
        Battery,
        Max
    }


    /************************************************
     * 远程弹药消耗
     * *********************************************/
    public  class StorageSupply_Ammo : StrorageSupply
    {
        AmmoExpend[] m_atkExpends;
        public StorageSupply_Ammo()
        {
            m_atkExpends = new AmmoExpend[(int)EAtkRangeExpend.Max];
			m_atkExpends[(int)EAtkRangeExpend.Bullet] = new AmmoExpend_Bullet();
            m_atkExpends[(int)EAtkRangeExpend.Arrow] = new AmmoExpend_Arrow();
            m_atkExpends[(int)EAtkRangeExpend.Battery] = new AmmoExpend_Battery();
        }

        public override ESupplyType Type
        {
            get { return ESupplyType.Ammo; }
        }

        public override bool DoSupply(PeEntity entity,CSAssembly csAssembly)
        {
            if (csAssembly == null || csAssembly.Storages == null)
                return false;

            if (entity.packageCmpt == null)
                return false;

            List<ItemObject> _atkObjs = entity.GetAtkEquipObjs(AttackType.Ranged);
            if (_atkObjs == null || _atkObjs.Count <= 0)
                return false;

            for (int i = 0; i < _atkObjs.Count; i++)
            {
                AmmoExpend expend = MatchExpend(_atkObjs[i]);
                if (expend == null)
                    continue;

                expend.SupplySth(entity, csAssembly);
            }

            return true;
        }

        public AmmoExpend MatchExpend(ItemObject obj)
        {
            if (obj.protoData.weaponInfo == null)
                return null;

            int costId = obj.protoData.weaponInfo.costItem;
            if(costId != 0)
            {
                for (int i = 0; i < m_atkExpends.Length; i++)
                {
                    if (m_atkExpends[i] != null && m_atkExpends[i].MatchExpend(costId))
                        return m_atkExpends[i];
                }
            }

            if (obj.protoData.weaponInfo.useEnergry)
            {
                for (int i = 0; i < m_atkExpends.Length; i++)
                {
                    if (m_atkExpends[i] != null && m_atkExpends[i].MatchExpend(SupplyNum.BATTERY))
                        return m_atkExpends[i];
                }
            }

            return null;
        }
    }
}


