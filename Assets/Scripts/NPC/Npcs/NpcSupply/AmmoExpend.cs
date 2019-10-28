using UnityEngine;
using Pathea;
using System.Collections;
using System.Collections.Generic;
using Pathea.PeEntityExtNpcPackage;
using Pathea.PeEntityExt;
using ItemAsset;

namespace Pathea
{
    /*****************************
     * 远程攻击型消耗
     * *****************************/
    public abstract class AmmoExpend
    {
        /*****************************
         * 是否耗能
         * *****************************/
        public abstract bool useEnergy { get; }

        /*****************************
         * 消耗品ID （protoID）
         * *****************************/
        public abstract int costId { get; }

        /*****************************
         * 攻击型消耗类型
         * *****************************/
        public abstract EAtkRangeExpend Type { get; }

        /*****************************
         * 补给物品
         * *****************************/
        public abstract bool SupplySth(PeEntity entity,CSAssembly asAssembly);

        /*****************************
         * 匹配消耗
         * *****************************/
        public virtual bool MatchExpend(int costProtoId) { return costId == costProtoId; }

    }

    /************************************************
     * 攻击型消耗——子弹
     * *********************************************/
    public class AmmoExpend_Bullet : AmmoExpend
    {
        const int BULLET_Supply_once = SupplyNum.BULLET_Supply_Max;
        const int BULLET_Supply_min = SupplyNum.BULLET_Supply_Min;

        public override EAtkRangeExpend Type
        {
            get { return EAtkRangeExpend.Bullet; }
        }

        public override int costId
        {
            get { return SupplyNum.BULLET; }
        }

        public override bool useEnergy
        {
            get { return false; }
        }

        public override bool SupplySth(PeEntity entity,CSAssembly asAssembly)
        {
            // find in package
            int _curOwn = entity.GetItemCount(costId);
            if (_curOwn > BULLET_Supply_min)
                return false;

            //find In csstorage
            return NpcSupply.CsStorageSupply(entity, asAssembly, costId, BULLET_Supply_once);

        }

    }


    /************************************************
     * 攻击型消耗——箭矢
     * *********************************************/
    public class AmmoExpend_Arrow : AmmoExpend
    {
        const int Arrow_Supply_min = SupplyNum.ARROW_Supply_Min;
        const int Arrow_Supply_Once = SupplyNum.ARROW_Supply_Max;

        public override EAtkRangeExpend Type
        {
            get { return EAtkRangeExpend.Arrow; }
        }

        public override int costId
        {
            get { return SupplyNum.ARROW; }
        }

        public override bool useEnergy
        {
            get { return false; }
        }

        public override bool SupplySth(PeEntity entity,CSAssembly asAssembly)
        {
            // find in package
            int _curOwn = entity.GetItemCount(costId);
            if (_curOwn > Arrow_Supply_min)
                return false;

            //find In csstorage
            return NpcSupply.CsStorageSupply(entity, asAssembly, costId, Arrow_Supply_Once);
        }
    }


    /************************************************
     * 攻击型消耗——电池
     * *********************************************/
    public class AmmoExpend_Battery : AmmoExpend
    {
        const float Energy_Percent_min = SupplyNum.BATTERY_Supply_MinPercent;
        const float Energy_Percent_Max = SupplyNum.BATTERY_Supply_MaxPercent;

        const int Energy_num = SupplyNum.BATTERY_Supply_Num;

        public override EAtkRangeExpend Type
        {
            get { return EAtkRangeExpend.Battery; }
        }

        public override int costId
        {
            get { return SupplyNum.BATTERY; }
        }

        public override bool useEnergy
        {
            get { return true; }
        }

        public override bool SupplySth(PeEntity entity,CSAssembly asAssembly)
        {

            // check self
            float _curOwn = entity.GetAttribute(AttribType.Energy);
            if (_curOwn > Energy_Percent_min)
                return false;

            //check package
            List<ItemObject> objs = entity.GetEquipObjs(EeqSelect.energy);
            if (objs != null && objs.Count > 0)
            {
                for (int i = 0; i < objs.Count; i++)
                {
                    Energy energy = objs[i].GetCmpt<Energy>();
                    if (energy != null && energy.floatValue.percent > Energy_Percent_min)
                        return false;
                }

            }

            //find In csstorage
            return NpcSupply.CsStrargeSupplyExchangeBattery(entity, asAssembly, objs, costId, Energy_num);
        }

    }
}

