using UnityEngine;
using System.Collections;
using SkillAsset;
using Pathea.Operate;
using Pathea;

public class MousePickRides : MousePickableChildCollider
{

    PERides rides;
    PERides _rides
    {
        get
        {
            if (rides == null)
            {
                rides = GetComponent<PERides>();
            }

            return rides;
        }
    }

    Pathea.PeTrans _playerTrans = null;
    Vector3 _playerPos
    {
        get
        {
            if (null == _playerTrans)
            {
                if (Pathea.PeCreature.Instance.mainPlayer != null)
                    _playerTrans = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PeTrans>();
            }

            if (null == _playerTrans)
            {
                return Vector3.zero;
            }

            return _playerTrans.position;
        }
    }

    OperateCmpt _playerOperate
    {
        get
        {
            return (null==MainPlayer.Instance.entity)?null:MainPlayer.Instance.entity.operateCmpt;
        }
    }

    PeEntity monsterEntity;
    PeEntity _monsterEntity
    {
        get
        {
            if (null == monsterEntity) monsterEntity = transform.GetComponentInParent<PeEntity>();
            return monsterEntity;
        }

    }

    public static readonly int RideItemID = 1740;

    public static readonly int RideOnTipsID = 8000982;
    public static readonly int RideOnDescribeID = 8000983;
    public static readonly int LackRideItemTipsID = 8000984;
    public static readonly int LowHpTipsID = 8000985;


    float _rideTime;
    const float OpIntervalTime = 0.5f;

    #region mono methods

    void Update()
    {
        if(PeInput.Get(PeInput.LogicFunction.InteractWithItem)
            && Time.realtimeSinceStartup - _rideTime > OpIntervalTime //lz-2017.02.23 上坐骑和下坐骑按键输入是同一个键,这里避免上坐骑完成一瞬间这里也满足条件又下来了
            && _rides && _rides.HasOperater(_playerOperate) 
            && _playerOperate && _playerOperate.IsActionRunning(PEActionType.Ride))
        {
            ExecUnRide(MainPlayer.Instance.entity);
        }
    }

    #endregion


    #region public methods
    public void Reset()
    {
        CollectColliders();
        AdjustOpDistance();
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    /// <summary>
    /// 执行骑这只怪物
    /// </summary>
    /// <param name="playerEntity">玩家Entity</param>
    public bool ExecRide(PeEntity playerEntity)
    {
        if (_monsterEntity && playerEntity && _rides)
        {
            Pathea.MotionMgrCmpt mmc = playerEntity.motionMgr;
            OperateCmpt operate = playerEntity.operateCmpt;
            if (null != mmc && !mmc.IsActionRunning(Pathea.PEActionType.Ride) && null != operate)
            {
                PERide ride = _rides.GetUseable();
                if (ride && ride.CanOperateMask(EOperationMask.Ride))
                {
                    return ride.StartOperate(operate, EOperationMask.Ride);
                }
                else Debug.Log("Try exec ride failed!！ ride is null!");
            }
            else Debug.LogFormat("Try exec ride failed!！ mmc is null:{0} ; operate is null:{1} ", null == mmc, null == operate);
        }
        else Debug.LogFormat("Try exec ride failed!！ _monsterEntity is null:{0} ; playerEntity is null:{1} ; _rides is null:{2} ", null == _monsterEntity, null == playerEntity, null==_rides);
        return false;
    }

    /// <summary>
    /// 坐骑模型被重刷，ride点被重建，乘骑恢复
    /// </summary>
    /// <param name="playerEntity"></param>
    /// <returns></returns>
    public bool RecoverExecRide(PeEntity playerEntity)
    {
        if (_monsterEntity && playerEntity && _rides)
        {
            Pathea.MotionMgrCmpt mmc = playerEntity.motionMgr;
            OperateCmpt operate = playerEntity.operateCmpt;
            if (null != mmc  && null != operate)
            {
                PERide ride = _rides.GetUseable();
                if (ride && ride.CanOperateMask(EOperationMask.Ride))
                {
                    if (mmc.IsActionRunning(Pathea.PEActionType.Ride))
                        mmc.EndImmediately(Pathea.PEActionType.Ride);

                    return ride.StartOperate(operate, EOperationMask.Ride);
                }
                else Debug.Log("Try recover ride failed!！ ride is null!");
            }
            else Debug.LogFormat("Try recover ride failed!！ mmc is null:{0} ; operate is null:{1} ", null == mmc, null == operate);
        }
        else Debug.LogFormat("Try recover ride failed!！ _monsterEntity is null:{0} ; playerEntity is null:{1} ; _rides is null:{2} ", null == _monsterEntity, null == playerEntity, null == _rides);
        return false;
    }
    /// <summary>
    /// 执行下这只怪物
    /// </summary>
    /// <param name="playerEntity">玩家Entity</param>
    public bool ExecUnRide(PeEntity playerEntity)
    {
        if (_monsterEntity && playerEntity && _rides)
        {
            Pathea.MotionMgrCmpt mmc = playerEntity.motionMgr;
            OperateCmpt operate = playerEntity.operateCmpt;
            if (null != mmc && mmc.IsActionRunning(Pathea.PEActionType.Ride) && null != operate)
            {
                PERide ride = _rides.GetRideByOperater(operate);
                if (ride)
                {
                    return ride.StopOperate(operate, EOperationMask.Ride);
                }
                else Debug.Log("Try exec unRide failed!！ ride is null!");
            }
            else Debug.LogFormat("Try exec unRide failed!！ mmc is null:{0} ; operate is null:{1} ", null == mmc, null == operate);
        }
        else Debug.LogFormat("Try exec ride failed!！ _monsterEntity is null:{0} ; playerEntity is null:{1} ; _rides is null:{2} ", null == _monsterEntity, null == playerEntity, null == _rides);
        return false;
    }

    #endregion


    #region override methods

    protected override void CheckOperate()
    {
        base.CheckOperate();

		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) && CanCmd())
        {
            TryExecAction();
        }
    }

    protected override bool CheckPick(Ray ray, out float dis)
    {
        return base.CheckPick(ray, out dis) && null != _playerOperate &&
            (!_playerOperate.IsActionRunning(PEActionType.Ride) && _rides.HasRide())                        //lz-2017.02.09 我没有执行骑的动作，并且坐骑上有空座位
            && (null != _monsterEntity && _monsterEntity.monster.CanRide);                                     //lz-2017.02.13  任务怪不能骑
    }

    protected override string tipsText
    {
        get
        {
            //if (_playerOperate)
            //{
            //    if (_rides.HasOperater(_playerOperate))
            //    {
            //        return null;
            //    }
            //    else if (_rides.HasRide())
            //    {
            //        return base.tipsText + "\n" + PELocalization.GetString(RideOnTipsID);
            //    }
            //}

            return null;
        }
    }

    #endregion

    #region private methods
    private bool CanCmd()
    {
        if (null != SelectItem_N.Instance && SelectItem_N.Instance.HaveOpItem())
        {
            return false;
        }

        if (DistanceInRange(_playerPos, base.operateDistance))
        {
            Pathea.SkAliveEntity alive = GetComponent<Pathea.SkAliveEntity>();
            if (alive != null && alive.isDead)
            {
                return false;
            }
            return true;
        }
        return false;
    }

    private void TryExecAction()
    {
        if (null == _playerOperate||null==_monsterEntity)
            return;

        _rideTime = Time.realtimeSinceStartup;

        //lw:怪物血量高于50%不能乘骑
        if(_monsterEntity.HPPercent > 0.5f)
        {
            PeTipMsg.Register(PELocalization.GetString(LowHpTipsID), PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Misc);
            return;
        }

        //lw:玩家有规定物品才能乘骑怪物
        PlayerPackageCmpt playerPackage = MainPlayer.Instance.entity.packageCmpt as PlayerPackageCmpt;
        ItemAsset.ItemObject item = playerPackage.package.FindItemByProtoId(RideItemID);
        if(item == null || MainPlayer.Instance.entity.UseItem.GetCdByItemProtoId(RideItemID) > PETools.PEMath.Epsilon)
        {
            PeTipMsg.Register(PELocalization.GetString(LackRideItemTipsID), PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Misc);
            return;
        }

        //lz-2017.02.23 多人的时候先发消息请求控制权,请求回来再执行骑，避免坐骑系统Ai等运行不正常
        if (PeGameMgr.IsMulti)
        {
            //PlayerNetwork.RequestuseItem(item.instanceId);
            PlayerNetwork.RequestReqMonsterCtrl(_monsterEntity.Id);
        }
        else
        {
            if (ExecRide(Pathea.MainPlayer.Instance.entity))
            {
                //MainPlayer.Instance.entity.UseItem.Use(item);
                playerPackage.DestroyItem(item.instanceId,1);
            }
        }

    }

    private void AdjustOpDistance()
    {
        Bounds bounds = SpeciesViewStudio.GetModelBounds(gameObject);
        if (bounds.size == Vector3.zero)
        {
            base.operateDistance = 0f;
        }
        else
        {
            base.operateDistance = Mathf.Max(bounds.extents.x, bounds.extents.z) + 1.5f;
        }
    }

    #endregion
}
