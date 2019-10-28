using UnityEngine;
using System.Collections;
using Pathea;
using System.Collections.Generic;

/// <summary>
/// 2017.01.05 驯服怪物UI控制
/// </summary>
public class UITameMonsterCtrl : UIStaticWnd
{
    static UITameMonsterCtrl mInstance;
    public static UITameMonsterCtrl Instance { get { return mInstance; } }

    [SerializeField]
    GameObject _tamingRoot;
    [SerializeField]
    GameObject _tameFailedRoot;
    [SerializeField]
    GameObject _tameSucceedRoot;
    [SerializeField]
    Transform _checkRangeRoot;
    [SerializeField]
    UISprite _centerImg;
    [SerializeField]
    UISprite _roundnessImg;
    [SerializeField]
    UISprite _borderImg;
    [SerializeField]
    UISlider _tameProgress;
    [SerializeField]
    UILabel _progressLbl;
    [SerializeField]
    TweenScale _tameSucceedTween;
    [SerializeField]
    TweenScale _tameFailedTween;
    [SerializeField]
    float _maxRadius = 75f;
    [SerializeField]
    float _effectShowTime = 0.2f;
    [SerializeField]
    TweenPosition _directionArrayTween;
    //[SerializeField]
    //int _arrayTweenOffset = 8;
    [SerializeField]
    float _arrayDuration = 0.2f;



    Vector3 _localPos;
    PeTrans _targetTrans;
    float _maxCheckRadius;
    List<GameObject> _rangeList;
    float _safeRadius;

    RangeAttribute _zRange;
    RangeAttribute _xRange;

    TameMonsterManager.TameState _curTameState;
    float _curWiatTime;
    TweenScale _curTween;
    GameObject _curRoot;

    #region mono methods

    void Update()
    {
        if (_targetTrans)
        {
            UpdateCenterImg();
            CheckShowArray(_localPos);
        }
    }

    #endregion

    #region overide methods

    public override void Show()
    {
        base.Show();
    }

    public override void OnCreate()
    {
        base.OnCreate();
        mInstance = this;
        Init();
    }

    protected override void OnHide()
    {
        base.OnHide();
        _targetTrans = null;
        _tamingRoot.SetActive(false);
        _tameFailedRoot.SetActive(false);
        _tameSucceedRoot.SetActive(false);
    }

    #endregion

    #region private methods

    [ContextMenu("ReCreateCheckRange")]
    void Init()
    {
        if (null == _rangeList) _rangeList = new List<GameObject>();

        if (_rangeList.Count > 0)
        {
            for (int i = 0; i < _rangeList.Count; i++)
            {
                Destroy(_rangeList[i]);
            }
        }

        if (TameMonsterConfig.instance.IfRangeList.Count > 0)
        {
            TameMonsterConfig.DropIfRangeInfo info = new global::TameMonsterConfig.DropIfRangeInfo();
            TameMonsterConfig.DropIfRangeInfo safeInfo = new global::TameMonsterConfig.DropIfRangeInfo();
            int depth = TameMonsterConfig.instance.IfRangeList.Count;
            float diameter;

            for (int i = 0; i < TameMonsterConfig.instance.IfRangeList.Count; i++)
            {
                info = TameMonsterConfig.instance.IfRangeList[i];
                diameter = AngleRadiusConvertToRadius(info.AngleRadius, TameMonsterConfig.instance.MaxRotate) * 2;
                InstantiateNewRangeImg(true, diameter, info.Color, depth);
                depth--;
                if (info.AllowStayTime == -1)
                {
                    safeInfo = info;
                }
            }

            _safeRadius = AngleRadiusConvertToRadius(safeInfo.AngleRadius, TameMonsterConfig.instance.MaxRotate);

            diameter = AngleRadiusConvertToRadius(info.AngleRadius, TameMonsterConfig.instance.MaxRotate) * 2;
            InstantiateNewRangeImg(false, diameter, info.Color, depth);
            _maxCheckRadius = diameter * 0.5f;
        }

        _directionArrayTween.duration = _arrayDuration;
        _directionArrayTween.gameObject.SetActive(false);
        _centerImg.transform.localPosition = Vector3.zero;
    }

    float GetRotateToPos(float rotate, RangeAttribute range, float maxRadius)
    {
        rotate = GetDirectionRotate(rotate);
        rotate = Mathf.Clamp(rotate, range.min, range.max);
        return (rotate - range.min) / (range.max - range.min) * (maxRadius * 2) - maxRadius;
    }

    void CheckShowArray(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > _safeRadius || Mathf.Abs(direction.y) > _safeRadius)
        {
            _directionArrayTween.gameObject.SetActive(true);
            _directionArrayTween.transform.parent.localPosition = -direction.normalized * (_maxCheckRadius + 7f);
            float angelZ = PETools.PEUtil.AngleZ(new Vector3(0, _safeRadius), -direction);
            if ((-direction.x) > 0) angelZ = 360 - angelZ;
            _directionArrayTween.transform.parent.localEulerAngles = new Vector3(0, 0, angelZ);
        }
        else _directionArrayTween.gameObject.SetActive(false);
    }

    RangeAttribute GetRange(float initRotate, float max)
    {
        initRotate = GetDirectionRotate(initRotate);
        return new RangeAttribute(initRotate - max, initRotate + max);
    }

    float GetDirectionRotate(float rotate)
    {
        return rotate < 180 ? rotate : rotate - 360;
    }

    void UpdateCenterImg()
    {
        if (_curTameState == TameMonsterManager.TameState.Taming)
        {
            if (_targetTrans && _centerImg)
            {
                _localPos.x = -GetRotateToPos(_targetTrans.rotation.eulerAngles.z, _zRange, _maxCheckRadius);
                _localPos.y = GetRotateToPos(_targetTrans.rotation.eulerAngles.x, _xRange, _maxCheckRadius);
                _localPos.z = _centerImg.transform.localPosition.z;
                _centerImg.transform.localPosition = _localPos;
            }
        }
    }

    void InstantiateNewRangeImg(bool isRoundness, float radius, Color col, int depth)
    {
        GameObject go = GameObject.Instantiate(isRoundness ? _roundnessImg.gameObject : _borderImg.gameObject);
        go.transform.parent = _checkRangeRoot;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(radius, radius);
        UISprite sprite = go.GetComponent<UISprite>();
        if (sprite)
        {
            sprite.depth = depth;
            sprite.color = isRoundness ? col : Color.white;
        }
        _rangeList.Add(go);
    }

    float AngleRadiusConvertToRadius(float angle, float maxAngle)
    {
        angle = Mathf.Clamp(angle, 0f, maxAngle);
        return angle / maxAngle * _maxRadius;
    }

    IEnumerator WaitEffectTimeIterator()
    {
        yield return new WaitForSeconds(_curWiatTime);
        if (null != _curTween)
        {
            ResetTween(_curTween);
            _curRoot.SetActive(false);
        }
    }

    private void ResetTween(TweenScale tween)
    {
        tween.transform.localScale = tween.from;
        tween.Reset();
    }

    private void PlayTween(TweenScale tween, GameObject root)
    {
        ResetTween(tween);
        _curTween = tween;
        _curRoot = root;
        _curWiatTime = _curTween.duration + _effectShowTime;
        _curTween.Play(true);
        StartCoroutine("WaitEffectTimeIterator");
    }

    #endregion

    #region public methods

    public void SetInfo(PeTrans trans)
    {
        if (trans)
        {
            _targetTrans = trans;
            _zRange = GetRange(_targetTrans.rotation.eulerAngles.z, TameMonsterConfig.instance.MaxRotate);
            _xRange = GetRange(_targetTrans.rotation.eulerAngles.x, TameMonsterConfig.instance.MaxRotate);
        }
    }

    public void UpdateTameProgress(int curProgress, int totalProgress)
    {
        if (_curTameState == TameMonsterManager.TameState.Taming)
        {
            _tameProgress.sliderValue = (totalProgress <= 0) ? 0f : ((curProgress * 1f) / totalProgress);
            _progressLbl.text = (totalProgress <= 0) ? "--" : Mathf.FloorToInt(_tameProgress.sliderValue * 100).ToString();
        }
    }

    public void SetTameState(TameMonsterManager.TameState tameState)
    {
        _curTameState = tameState;
        switch (tameState)
        {
            case TameMonsterManager.TameState.None:
                _tamingRoot.SetActive(false);
                _targetTrans = null;
                break;
            case TameMonsterManager.TameState.Taming:
                StopAllCoroutines();
                _tamingRoot.SetActive(true);
                _tameFailedRoot.SetActive(false);
                _tameSucceedRoot.SetActive(false);
                break;
            case TameMonsterManager.TameState.TameSucceed:
                _tamingRoot.SetActive(false);
                _tameFailedRoot.SetActive(false);
                _tameSucceedRoot.SetActive(true);
                PlayTween(_tameSucceedTween, _tameSucceedRoot);
                break;
            case TameMonsterManager.TameState.TameFailed:
                _tamingRoot.SetActive(false);
                _tameFailedRoot.SetActive(true);
                _tameSucceedRoot.SetActive(false);
                PlayTween(_tameFailedTween, _tameFailedRoot);
                _targetTrans = null;
                break;
        }
    }

    #endregion
}