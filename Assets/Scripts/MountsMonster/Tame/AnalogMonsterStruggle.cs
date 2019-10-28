using UnityEngine;
using Pathea;
using PETools;
using System.Collections.Generic;
using System;
using System.Linq;
using RootMotion.FinalIK;

/// <summary>
/// 2017.01.05 模拟怪物挣扎
/// </summary>
public class AnalogMonsterStruggle : MonoBehaviour
{
    public enum MonsterAction
    {
        None,
        Forward,
        Back,
        Left,
        Right,
        Jump,
        MoveDirtion
    }

    public struct SingleAction
    {
        public MonsterAction ActionType { get; private set; }
        public float ExecTime { get; private set; }
        public SpeedState Speed { get; private set; }
        public Vector3 Dirtion { get; private set; }

        public SingleAction(MonsterAction actionType, float execTime, SpeedState speed, Vector3 dirtion)
        {
            ActionType = actionType;
            ExecTime = execTime;
            Speed = speed;
            Dirtion = dirtion;
        }
    }

    public class SingleActionSeed
    {
        public MonsterAction Action { get { return _info.Action; } }
        public float RandomTime
        {
            get
            {
                return (_info.TimeRandomRange.Min == _info.TimeRandomRange.Max) ? _info.TimeRandomRange.Min : UnityEngine.Random.Range(_info.TimeRandomRange.Min, _info.TimeRandomRange.Max);
            }
        }
        public float ActionPercent { get; private set; }
        public SpeedState RandomSpeed
        {
            get
            {
                switch (Action)
                {
                    case MonsterAction.Forward:
                    case MonsterAction.Back:
                    case MonsterAction.Left:
                    case MonsterAction.Right:
                    case MonsterAction.MoveDirtion:
                        float v = UnityEngine.Random.Range(0f, (_info.WalkRandomPercent + _info.RunRandomPercent + _info.SprintRandomPercent - float.Epsilon));
                        if (v <= _info.WalkRandomPercent) return SpeedState.Walk;
                        v += _info.WalkRandomPercent;
                        if (v <= _info.RunRandomPercent) return SpeedState.Run;
                        return SpeedState.Sprint;
                    case MonsterAction.None:
                    case MonsterAction.Jump:
                        return SpeedState.None;
                }
                return SpeedState.None;
            }
        }

        TameMonsterConfig.SingleActionSeedInfo _info;
        public SingleActionSeed(TameMonsterConfig.SingleActionSeedInfo info, float actionPercent)
        {
            _info = info;
            ActionPercent = actionPercent;
        }
    }


    Transform _ridePosTrans;

    PeEntity _monsterEntity;
    MonstermountCtrl _mMCtrl;
    Vector3 _offsetRotate;

    Vector3 _lastPosition;
    Vector3 _lastVelocity;

    Vector3 _lastUp;
    //Quaternion _lastAngularVelocity;

    Vector3 _backupOffsetRotate;

    Dictionary<MonsterAction, SingleActionSeed> _actionSeedDic;
    float _percentTotal;
    Queue<SingleAction> _struggleActions;
    SingleAction _curActionInfo;

    public Vector3 OffsetRotate { get { return _offsetRotate; } }
    public bool IsTameSucceed { get; private set; }

    public int CurTameProgress { get { return (null == _struggleActions) ? 0 : (StruggleActionTotal - _struggleActions.Count); } }

    public int StruggleActionTotal { get; private set; }

    #region mono methods
    void Update()
    {
        if (_ridePosTrans)
        {
            var newPosition = _ridePosTrans.position;
            var newVelocity = (newPosition - _lastPosition) / Time.deltaTime;
            _lastPosition = newPosition;

            var deltaVelocity = newVelocity - _lastVelocity;
            _lastVelocity = newVelocity;

            _offsetRotate = _ridePosTrans.InverseTransformDirection(deltaVelocity).normalized
                * deltaVelocity.magnitude * TameMonsterConfig.instance.MonsterMoveFactor;

            var newUp = _ridePosTrans.up;
            float newAngularSpeed;
            Vector3 newAxis;
            Quaternion.FromToRotation(_lastUp, newUp).ToAngleAxis(out newAngularSpeed, out newAxis);
            newAngularSpeed /= Time.deltaTime;
            var newAngularVelocity = Quaternion.AngleAxis(newAngularSpeed, newAxis);
            _lastUp = newUp;

            //var deltaAngularVelocity = Quaternion.Inverse(_lastAngularVelocity) * newAngularVelocity;
           // _lastAngularVelocity = newAngularVelocity;
            
            _offsetRotate += _ridePosTrans.InverseTransformDirection(newAngularVelocity * _ridePosTrans.up)
                * TameMonsterConfig.instance.MonsterRotateFactor;

            _offsetRotate.y = _offsetRotate.z;
            _offsetRotate.z = -_offsetRotate.x;
            _offsetRotate.x = _offsetRotate.y;
            _offsetRotate.y = 0f;
        }
    }

    #endregion

    #region private methods

    float VectorToAngle(Vector3 v1, Vector3 v2)
    {
        return Mathf.Asin(Vector3.Distance(Vector3.zero, Vector3.Cross(v1.normalized, v2.normalized))) * Mathf.Rad2Deg;
    }

    float VectorToAngleZ(Vector3 v1, Vector3 v2)
    {
        v1.z = 0.0f;
        v2.z = 0.0f;

        return Vector3.Cross(v1, v2).z > 0 ? -VectorToAngle(v1, v2) : VectorToAngle(v1, v2);
    }

    float VectorToAngleX(Vector3 v1, Vector3 v2)
    {
        v1.x = 0.0f;
        v2.x = 0.0f;

        return Vector3.Cross(v1, v2).x > 0 ? VectorToAngle(v1, v2) : -VectorToAngle(v1, v2);
    }

    /// <summary> 绑定行为 </summary>
    void BindAction()
    {
        if (null == _mMCtrl || TameMonsterConfig.instance.ActionList.Count <= 0) return;

        _actionSeedDic = new Dictionary<MonsterAction, SingleActionSeed>();
        _percentTotal = 0f;
        for (int i = 0; i < TameMonsterConfig.instance.ActionList.Count; i++)
        {
            TameMonsterConfig.SingleActionSeedInfo info = TameMonsterConfig.instance.ActionList[i];
            if (info.Action != MonsterAction.Jump || (info.Action == MonsterAction.Jump && _mMCtrl.HasJump()))
            {
                _percentTotal += info.RandomPercent;
                _actionSeedDic[info.Action] = new SingleActionSeed(info, _percentTotal);
            }
        }
    }

    /// <summary> 生成模拟怪物挣扎行为时间轴 </summary>
    void GenerateStruggleActions()
    {
        if (null == _actionSeedDic) return;

        //怪物的Bounds
        Bounds bounds = GetMonsterBounds();
        //怪物体积
        float monsterBulk = bounds.size.x * bounds.size.y * bounds.size.z;
        //怪物体积Clamp
        monsterBulk = Mathf.Clamp(monsterBulk, TameMonsterConfig.instance.MonsterBulkRange.Min, TameMonsterConfig.instance.MonsterBulkRange.Max);
        //怪物体积占比
        float bulkProportion = (monsterBulk - TameMonsterConfig.instance.MonsterBulkRange.Min) / (TameMonsterConfig.instance.MonsterBulkRange.Max - TameMonsterConfig.instance.MonsterBulkRange.Min);
        //体积占比转换为行为数占比
        float actionSize = bulkProportion * TameMonsterConfig.instance.MonsterStruggleActionSizeRange.Max;
        //行为数Clamp
        StruggleActionTotal = (int)Mathf.Clamp(actionSize, TameMonsterConfig.instance.MonsterStruggleActionSizeRange.Min, TameMonsterConfig.instance.MonsterStruggleActionSizeRange.Max);

        _struggleActions = new Queue<SingleAction>();
        MonsterAction actionType;
        Vector3 dirtion = Vector3.one;
        for (int i = 0; i < StruggleActionTotal; i++)
        {
            actionType = GetRandomAction();
            if (_actionSeedDic.ContainsKey(actionType))
            {
                if (actionType == MonsterAction.MoveDirtion)
                {
                    dirtion = GetRandomDirtion();
                }
                SingleAction tempInfo = new SingleAction(actionType, _actionSeedDic[actionType].RandomTime, _actionSeedDic[actionType].RandomSpeed, dirtion);
                //随机挣扎行为时间轴加到队列
                _struggleActions.Enqueue(tempInfo);
            }
        }
    }

    MonsterAction GetRandomAction()
    {
        if (null == _actionSeedDic) return MonsterAction.None;
        float value = UnityEngine.Random.Range(0f, _percentTotal - float.Epsilon);
        foreach (var item in _actionSeedDic)
        {
            if (value <= item.Value.ActionPercent)
            {
                return item.Key;
            }
        }
        return MonsterAction.None;
    }

    Bounds GetMonsterBounds()
    {
        Bounds modelBounds = new Bounds();

        if (_monsterEntity && _monsterEntity.peTrans)
        {
            SkinnedMeshRenderer mesh = _monsterEntity.peTrans.realTrans.GetComponentInChildren<SkinnedMeshRenderer>();
            if (mesh == null) return modelBounds;
            mesh.updateWhenOffscreen = true;
            modelBounds = mesh.bounds;
        }
        return modelBounds;
    }

    Vector3 GetRandomDirtion()
    {
        Vector3 dirtion = Vector3.one;
        if (_monsterEntity && _monsterEntity.peTrans)
        {
            Vector3 toPos = Vector3.one;
            //lz-2016.12.30 先尝试使用第一种方法找一安全的下坐骑的位置
            if (!PETools.PEUtil.GetNearbySafetyPos(_monsterEntity.peTrans, PETools.PEUtil.GetOffRideMask, PETools.PEUtil.Standlayer, ref toPos))
            {
                //lz-2016.12.30 如果第一种没找到就使用第二种
                float minRadus = Mathf.Max(_monsterEntity.peTrans.bound.size.x, _monsterEntity.peTrans.bound.size.z);
                toPos = PETools.PEUtil.GetEmptyPositionOnGround(_monsterEntity.peTrans.position + _monsterEntity.peTrans.bound.center, minRadus, minRadus + 3);
            }
            dirtion = _monsterEntity.peTrans.position - toPos;
        }
        return dirtion;
    }


    void ExecAction(SingleAction actionInfo)
    {
        if (_mMCtrl && actionInfo.ActionType != MonsterAction.None)
        {
            switch (actionInfo.ActionType)
            {
                case MonsterAction.Forward:
                    _mMCtrl.Forward(ActionCallBack, actionInfo.ExecTime, actionInfo.Speed);
                    break;
                case MonsterAction.Back:
                    _mMCtrl.Back(ActionCallBack, actionInfo.ExecTime, actionInfo.Speed);
                    break;
                case MonsterAction.Left:
                    _mMCtrl.Left(ActionCallBack, actionInfo.ExecTime, actionInfo.Speed);
                    break;
                case MonsterAction.Right:
                    _mMCtrl.Right(ActionCallBack, actionInfo.ExecTime, actionInfo.Speed);
                    break;
                case MonsterAction.Jump:
                    _mMCtrl.Jump(ActionCallBack);
                    break;
                case MonsterAction.MoveDirtion:
                    _mMCtrl.MoveDirtion(actionInfo.Dirtion, ActionCallBack, actionInfo.ExecTime, actionInfo.Speed);
                    break;
            }
        }
    }


    void ActionCallBack()
    {
        StartMonsterStruggle();
    }

    void StartMonsterStruggle()
    {
        if (null == _struggleActions)
        {
            Debug.Log("AnalogMonsterStruggle: _struggleActions is null!");
            return;
        }
        if (_struggleActions.Count > 0)
        {
            _curActionInfo = _struggleActions.Dequeue();
            ExecAction(_curActionInfo);
            IsTameSucceed = false;
        }
        else
        {
            IsTameSucceed = true;
        }
    }

    #endregion

    #region public methods

    public void SetInfo(PeEntity monsterEntity, Transform ridePosTrans)
    {
        if (monsterEntity && ridePosTrans)
        {
            _monsterEntity = monsterEntity;
            _mMCtrl = _monsterEntity.monstermountCtrl;

            _ridePosTrans = ridePosTrans;

            _lastPosition = _ridePosTrans.position;
            _lastUp = _ridePosTrans.up;
            //_lastAngularVelocity = Quaternion.identity;

            IsTameSucceed = false;
            BindAction();
            GenerateStruggleActions();
            StartMonsterStruggle();
        }
    }

    #endregion
}
