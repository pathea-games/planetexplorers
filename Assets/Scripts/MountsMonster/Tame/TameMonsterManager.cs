using UnityEngine;
using System.Collections;
using Pathea;
using System;
using System.Linq;
using RootMotion.FinalIK;

/// <summary>
/// lz-2017.01.05 管理整个怪 物驯服过程（怪物挣扎模拟，驯服UI显示，操作玩家重心）
/// </summary>
public class TameMonsterManager : MonoBehaviour
{
    public class DropIfRange
    {
        public float AngleRadius { get; private set; }
        public Color32 Color { get; private set; }
        public float AllowStayTime { get; private set; }
        public float DropOutRandomPercent { get; private set; }

        public float CurAddupTime { get; set; }
        public bool InRange
        {
            get { return _inRange; }
            set
            {
                if (_inRange != value)
                {
                    if (!value)
                    {
                        CurAddupTime = 0f;
                    }
                    _inRange = value;
                }
            }
        }

        private bool _inRange = false;

        public DropIfRange(TameMonsterConfig.DropIfRangeInfo info)
        {
            AngleRadius = info.AngleRadius;
            Color = info.Color;
            AllowStayTime = info.AllowStayTime;
            DropOutRandomPercent = Mathf.Clamp(info.DropOutRandomPercent, 0, 1);
        }
    }

    public enum TameState
    {
        None,
        Taming,
        TameSucceed,
        TameFailed,
    }

    PeEntity _monsterEntity;
    PeEntity _playerEntity;
    PeTrans _playerTrans;
    Transform _ridePosTrans;
    TameMonsterInputCtrl _inputCtrl;
    AnalogMonsterStruggle _analogStruggle;
    TameState _curTameState = TameState.None;
    Vector3 _frameOffsetRotate;
    Vector3 _offsetRotate;
    Vector3 _initRotate;
    DropIfRange[] _dropIfRanges;



    #region mono methods
    void Start()
    {

    }

    void Update()
    {
        if (_curTameState == TameState.Taming && _analogStruggle)
        {
            UITameMonsterCtrl.Instance.UpdateTameProgress(_analogStruggle.CurTameProgress, _analogStruggle.StruggleActionTotal);
            if (_analogStruggle.IsTameSucceed)
            {
                TameSucceed();
            }
        }
    }

    void LateUpdate()
    {

        if (_curTameState != TameState.TameFailed)
        {
            //lz-2017.01.05 同步玩家到坐骑的位置
            if (null != _ridePosTrans && _playerTrans)
            {
                _playerTrans.position = _ridePosTrans.position + TameMonsterConfig.instance.IkRideOffset;
            }
        }

        if (_curTameState == TameState.Taming)
        {
            if (_inputCtrl && _analogStruggle && _playerTrans && _ridePosTrans)
            {
                if (_offsetRotate.sqrMagnitude > 0.01f)
                {
                    var v1 = Vector3.Project(_analogStruggle.OffsetRotate, _offsetRotate);
                    var v2 = _analogStruggle.OffsetRotate - v1;
                    if (Vector3.Dot(v1, _offsetRotate) < 0f) v1 *= TameMonsterConfig.instance.PositiveFactor;
                    _offsetRotate += v1 + v2;
                }
                else _offsetRotate += _analogStruggle.OffsetRotate;

                _offsetRotate += _inputCtrl.OffsetRotate;
                _offsetRotate.y = 0f;
                _offsetRotate = Mathf.Clamp(_offsetRotate.magnitude, 0f, TameMonsterConfig.instance.MaxRotate) * _offsetRotate.normalized;

                _initRotate.y = _ridePosTrans.rotation.eulerAngles.y;
                _playerTrans.rotation = Quaternion.Euler(_initRotate + _offsetRotate);
                if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer)
                    PlayerNetwork.mainPlayer.RequestSyncRotation(_playerTrans.rotation.eulerAngles);
            }
            CheckInRange();
        }

        if (_curTameState == TameState.TameSucceed)
        {
            if (_playerTrans && _ridePosTrans)
            {
                _playerTrans.rotation = _ridePosTrans.rotation;
                if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer)
                    PlayerNetwork.mainPlayer.RequestSyncRotation(_playerTrans.rotation.eulerAngles);
            }
        }
    }

    #endregion

    #region private methods

    //Vector3 limitRotateInRadius(Vector3 curOffsetRotate, Vector3 frameOffsetRotate)
    //{
    //    //frameOffsetRotate = directionRotate > 0 ? max - directionRotate : Mathf.Abs(directionRotate) - max;
    //    frameOffsetRotate.z = LimitAngleInMax(curOffsetRotate.z, frameOffsetRotate.z, TameMonsterConfig.instance.HorizontalMaxRotate);
    //    frameOffsetRotate.x = LimitAngleInMax(curOffsetRotate.x, frameOffsetRotate.x, TameMonsterConfig.instance.VerticalMaxRotate);
    //    frameOffsetRotate.y = 0f;
    //    if (!PointInRadius(curOffsetRotate.x + frameOffsetRotate.x, curOffsetRotate.z + frameOffsetRotate.z, Math.Max(TameMonsterConfig.instance.VerticalMaxRotate, TameMonsterConfig.instance.HorizontalMaxRotate)))
    //    {
    //        frameOffsetRotate = Vector3.zero;
    //    }
    //    return curOffsetRotate + frameOffsetRotate;
    //}

    //float LimitAngleInMax(float curAngle, float frameOffsetAngle, float maxAngle)
    //{
    //    return Mathf.Clamp(curAngle + frameOffsetAngle, -maxAngle, maxAngle);
    //    if (Mathf.Abs(GetDirectionRotate(curAngle + frameOffsetAngle)) > maxAngle)
    //    {
    //        frameOffsetAngle = 0f;
    //    }
    //    return frameOffsetAngle;
    //}

    bool PointInRadius(float x, float z, float max)
    {
        x = Mathf.Abs(x);
        z = Mathf.Abs(z);
        max = Mathf.Abs(max);
        return (x * x + z * z) <= (max * max);
    }

    float GetDirectionRotate(float rotate)
    {
        return rotate < 180 ? rotate : rotate - 360;
    }

    private void Taming()
    {
        if (_curTameState != TameState.Taming)
        {
            _curTameState = TameState.Taming;

            if (_monsterEntity && _playerEntity)
            {
                SetMonsterAiState(false);
                _monsterEntity.monstermountCtrl.Taming();
                UITameMonsterCtrl.Instance.SetTameState(TameState.Taming);
            }
        }
    }

    private void TameSucceed()
    {
        if (_curTameState != TameState.TameSucceed)
        {
            _curTameState = TameState.TameSucceed;

            if (_monsterEntity && _playerEntity)
            {
                SetMonsterAiState(false);
                _monsterEntity.monstermountCtrl.TameSucceed();
            }
            UITameMonsterCtrl.Instance.SetTameState(TameState.TameSucceed);
            DestoryinputCtrl();
            DestoryAnalogStruggle();
            if (_playerEntity && _monsterEntity)
            {
                //lz-2017.02.17 单人驯服成功设为坐骑
                if (_playerEntity.mountCmpt) _playerEntity.mountCmpt.SetMount(_monsterEntity);
            }
            else
            {
                Debug.LogFormat("RelationshipDataMgr.AddRelationship failed! player is null:{0} , monster is null:{1}", (null == _playerEntity), (null == _monsterEntity));
            }
        }
    }

    public void TameFailed()
    {
        if (_curTameState != TameState.TameFailed)
        {
            _curTameState = TameState.TameFailed;

            if (_monsterEntity && _playerEntity)
            {
                SetMonsterAiState(true);
                _monsterEntity.monstermountCtrl.TameFailure();
            }

            UITameMonsterCtrl.Instance.SetTameState(TameState.TameFailed);
            DestoryinputCtrl();
            DestoryAnalogStruggle();
        }
    }

    void DestoryinputCtrl()
    {
        if (_inputCtrl)
        {
            _inputCtrl.SetTrans(null);
            GameObject.Destroy(_inputCtrl);
        }
    }

    void DestoryAnalogStruggle()
    {
        if (_analogStruggle)
        {
            _analogStruggle.SetInfo(null, null);
            GameObject.Destroy(_analogStruggle);
        }
    }

    private void SetMonsterAiState(bool isOpen)
    {
        if (_monsterEntity && _monsterEntity.BehaveCmpt)
        {
            if (isOpen)
                _monsterEntity.BehaveCmpt.Excute();
            else
                _monsterEntity.BehaveCmpt.Stop();
        }
    }

    private void InitDropIsRanges()
    {
        if (TameMonsterConfig.instance.IfRangeList.Count > 0)
        {
            TameMonsterConfig.instance.IfRangeList = TameMonsterConfig.instance.IfRangeList.OrderBy(a => (a.AngleRadius)).ToList();

            _dropIfRanges = new DropIfRange[TameMonsterConfig.instance.IfRangeList.Count];
            for (int i = 0; i < TameMonsterConfig.instance.IfRangeList.Count; i++)
            {
                _dropIfRanges[i] = new DropIfRange(TameMonsterConfig.instance.IfRangeList[i]);
            }
        }
    }

    float _xRotate, _zRotate;
    DropIfRange _preRangeTemp;
    DropIfRange _curRangeTemp;
    private void CheckInRange()
    {
        if (null != _dropIfRanges && _dropIfRanges.Length > 0)
        {
            _xRotate = Mathf.Abs(_offsetRotate.x);
            _zRotate = Mathf.Abs(_offsetRotate.z);
            for (int i = 0; i < _dropIfRanges.Length; i++)
            {
                if (i >= 1)
                {
                    _preRangeTemp = _dropIfRanges[i - 1];
                }
                _curRangeTemp = _dropIfRanges[i];

                if (_curRangeTemp.InRange && _curRangeTemp.AllowStayTime != -1)
                {
                    _curRangeTemp.CurAddupTime += Time.deltaTime;
                    if (_curRangeTemp.CurAddupTime >= _curRangeTemp.AllowStayTime)
                    {
                        _curRangeTemp.InRange = false;
                        float v = UnityEngine.Random.Range(0f, (1f - float.Epsilon));
                        if (v <= _curRangeTemp.DropOutRandomPercent)
                        {
                            PlayerDrop();
                            Debug.LogFormat("TameMonsterManager.PlayerDrop Succeed: RandomValue:{0} DropOutRandomPercent:{1}", v, _curRangeTemp.DropOutRandomPercent);
                        }
                        else
                        {
                            Debug.LogFormat("TameMonsterManager.PlayerDrop Failing: RandomValue:{0} DropOutRandomPercent:{1}", v, _curRangeTemp.DropOutRandomPercent);
                        }
                    }
                }

                if (_curRangeTemp.AllowStayTime != -1)
                {
                    //lz-2017.01.13 大于前一个范围，小于等于当前范围
                    if (
                        ((_preRangeTemp == null || _zRotate > _preRangeTemp.AngleRadius) && _zRotate <= _curRangeTemp.AngleRadius) ||
                        ((_preRangeTemp == null || _xRotate > _preRangeTemp.AngleRadius) && _xRotate <= _curRangeTemp.AngleRadius)
                      )
                    {
                        _curRangeTemp.InRange = true;
                    }
                    else
                    {
                        _curRangeTemp.InRange = false;
                    }
                }

                _preRangeTemp = null;
                _curRangeTemp = null;
            }
        }
    }

    private void PlayerDrop()
    {
        TameFailed();

        if (_playerEntity && _playerEntity.biologyViewCmpt) _playerEntity.biologyViewCmpt.ActivateCollider(true);

        if (_monsterEntity)
        {
            _monsterEntity.monstermountCtrl.HitFly(Vector3.up);
        }

    }
    #endregion

    #region public methods

    public void StartTame(PeEntity playerEntity, PeEntity monsterEntity, Transform ridePosTrans)
    {
        if (monsterEntity && playerEntity)
        {
            _monsterEntity = monsterEntity;
            _playerEntity = playerEntity;
            _playerTrans = _playerEntity.peTrans;
            _ridePosTrans = ridePosTrans;

            InitDropIsRanges();

            _monsterEntity.monstermountCtrl.InitTame(_playerEntity);
            //lz-2017.01.10 初始世界旋转度
            _initRotate = _playerTrans.rotation.eulerAngles;

            _inputCtrl = _ridePosTrans.gameObject.AddComponent<TameMonsterInputCtrl>();
            _inputCtrl.SetTrans(_playerEntity.peTrans);

            if (UITameMonsterCtrl.Instance)
            {
                UITameMonsterCtrl.Instance.Show();
                UITameMonsterCtrl.Instance.SetInfo(_playerEntity.peTrans);
            }

            _analogStruggle = _ridePosTrans.gameObject.AddComponent<AnalogMonsterStruggle>();
            _analogStruggle.SetInfo(_monsterEntity, ridePosTrans);

            Taming();
        }

    }

    public void ResetInfo()
    {
        _curTameState = TameState.None;

        if (_monsterEntity && _playerEntity)
        {
            SetMonsterAiState(true);
            _monsterEntity.monstermountCtrl.TameFailure();
        }

        UITameMonsterCtrl.Instance.SetTameState(_curTameState);
        DestoryinputCtrl();
        DestoryAnalogStruggle();
        _monsterEntity = null;
        _playerEntity = null;
    }

    public void LoadTameSucceed(PeEntity playerEntity, PeEntity monsterEntity, Transform ridePosTrans)
    {
        _curTameState = TameState.TameSucceed;
        _monsterEntity = monsterEntity;
        _playerEntity = playerEntity;
        _playerTrans = _playerEntity.peTrans;
        _ridePosTrans = ridePosTrans;
        SetMonsterAiState(false);
    }

    public void LoadTameSucceed(PeEntity playerEntity, PeEntity monsterEntity, Transform ridePosTrans,bool isNew)
    {
        _curTameState = TameState.TameSucceed;
        _monsterEntity = monsterEntity;
        _playerEntity = playerEntity;
        _playerTrans = _playerEntity.peTrans;
        _ridePosTrans = ridePosTrans;
        SetMonsterAiState(false);

        _monsterEntity.monstermountCtrl.InitTame(_playerEntity);
        _monsterEntity.monstermountCtrl.TameSucceed();
        if (isNew && _playerEntity.mountCmpt) _playerEntity.mountCmpt.SetMount(_monsterEntity);

    }

    #endregion
}