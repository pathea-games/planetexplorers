using UnityEngine;
using System.Collections;
using Pathea;

/// <summary>
/// lz-2017.01.05 在驯服怪物的时候控制人物的重心
/// </summary>
public class TameMonsterInputCtrl : MonoBehaviour
{
    Quaternion _curRotate;
    PeTrans _targetTrans;
    float _tempX, _tempZ;
    Vector3 _offsetRotate;

    public Quaternion InitRotate { get; private set; }
    public Vector3 OffsetRotate { get { return _offsetRotate; } }

    #region mono methods
    
    void Update()
    {
        if (_targetTrans)
        {
            _tempZ = PeInput.GetAxisH();
            _tempX = PeInput.GetAxisV();

            if (Mathf.Abs(_tempX) > float.Epsilon)
                _offsetRotate.x = (_tempX * Time.deltaTime) * TameMonsterConfig.instance.CtrlRotateSpeed;
            else
                _offsetRotate.x = 0f;

            if (Mathf.Abs(_tempZ) > float.Epsilon)
                _offsetRotate.z = -(_tempZ * Time.deltaTime) * TameMonsterConfig.instance.CtrlRotateSpeed;
            else
                _offsetRotate.z = 0f;

            _offsetRotate.y = 0f;
        }
    }


    #endregion

    #region public methods

    public void SetTrans(PeTrans trans)
    {
        if (trans)
        {
            _targetTrans = trans;
            InitRotate = _targetTrans.transform.rotation;
        }
    }

    #endregion
}
