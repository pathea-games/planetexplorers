using UnityEngine;
using System.Collections;

public class MoveByPath : MonoBehaviour {

    public static bool IsPlaying(GameObject target) 
    {
        WhiteCat.TweenInterpolator tip = target.GetComponent<WhiteCat.TweenInterpolator>();
        if (tip == null)
            return false;
        return tip.isPlaying; 
    }

    GameObject pathObj;
    WhiteCat.BezierPath mPath;
    WhiteCat.PathDriver mDrive;
    WhiteCat.TweenPathDriver mTween;
    WhiteCat.TweenInterpolator mPolator;

    void SetData(GameObject path) 
    {
        pathObj = path;
        mPath = path.GetComponent<WhiteCat.BezierPath>();

        mDrive = gameObject.AddComponent<WhiteCat.PathDriver>();
        mDrive.path = mPath;

        mTween = gameObject.AddComponent<WhiteCat.TweenPathDriver>();
        mTween.from = 0f;
        mTween.to = mDrive.path.pathTotalLength;

        if (mPolator == null)
        {
            mPolator = gameObject.AddComponent<WhiteCat.TweenInterpolator>();
            mPolator.enabled = false;
            mPolator.wrapMode = WhiteCat.WrapMode.Once;
        }
    }

    public void StartMove(GameObject pathObj, WhiteCat.RotationMode rotateType = WhiteCat.RotationMode.Ignore,
        WhiteCat.TweenMethod moveType = WhiteCat.TweenMethod.Linear)
    {
        SetData(pathObj);

        mDrive.rotationMode = rotateType;
        mPolator.method = moveType;
        mPolator.enabled = true;

        mPolator.onArriveAtEnding.AddListener(Destroy);
    }

    public void SetDurationDelay(float totalTime,float delay)
    {
        if(null == mPolator)
        {
            mPolator = gameObject.AddComponent<WhiteCat.TweenInterpolator>();
            mPolator.enabled = false;
            mPolator.wrapMode = WhiteCat.WrapMode.Once;
        }
        mPolator.duration = totalTime;
        mPolator.delay = delay;
    }

    public void AddStartListener(UnityEngine.Events.UnityAction call) 
    {
        if (null == mPolator)
        {
            mPolator = gameObject.AddComponent<WhiteCat.TweenInterpolator>();
            mPolator.enabled = false;
            mPolator.wrapMode = WhiteCat.WrapMode.Once;
        }
        mPolator.onArriveAtBeginning.AddListener(call);
    }

    public void AddEndListener(UnityEngine.Events.UnityAction call)
    {
        if (null == mPolator)
        {
            mPolator = gameObject.AddComponent<WhiteCat.TweenInterpolator>();
            mPolator.enabled = false;
            mPolator.wrapMode = WhiteCat.WrapMode.Once;
        }
        mPolator.onArriveAtEnding.AddListener(call);
    }

    void Destroy() 
    {
        GameObject.Destroy(mTween);
        GameObject.Destroy(mPolator);
        GameObject.Destroy(mDrive);
        GameObject.Destroy(pathObj);
        GameObject.Destroy(this);
    }

//#if UNITY_EDITOR
//    WhiteCat.BezierPath testPath;

//    void SetData(WhiteCat.BezierPath path,GameObject target)
//    {
//        mPath = path;

//        mDrive = target.AddComponent<WhiteCat.PathDriver>();
//        mDrive.path = mPath;

//        mTween = target.AddComponent<WhiteCat.TweenPathDriver>();
//        mTween.from = 0f;
//        mTween.to = mDrive.path.pathTotalLength;

//        if (mPolator == null)
//        {
//            mPolator = target.AddComponent<WhiteCat.TweenInterpolator>();
//            mPolator.enabled = false;
//            mPolator.wrapMode = WhiteCat.WrapMode.Once;
//        }
//    }

//    void OnGUI()
//    {
//        if (GUI.Button(new Rect(10, 130 + 100, 70, 20), "DebugPath"))
//        {
//            pathObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
//            pathObj.name = "test";

//            SetData(gameObject.GetComponent<WhiteCat.BezierPath>(), pathObj);
//            mPolator.enabled = true;

//            AddEndListener(DebugDestroy);
//        }
//    }

//    void DebugDestroy() 
//    {
//        GameObject.Destroy(pathObj);
//    }
//#endif
}
