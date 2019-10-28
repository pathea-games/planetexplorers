using UnityEngine;
using System.Collections;

public class NpcSpeakSentenseItem : MonoBehaviour
{
    [SerializeField]
    UILabel mLabel;
    [SerializeField]
    UISlicedSprite mSpr;


    //float mInterval = 0f;
    float mDuration = 0f;

    bool ableCalculate = false;
    bool mStartCountDown = false;

    public event System.Action<NpcSpeakSentenseItem> OnDestroySelfEvent;

    float GetDuration(int _length, float _interval)
    {
        float _dur = 0f;
        if (_length <= 50)
            _dur = 5f;
        else if (_length > 50 && _length <= 100)
            _dur = 7f;
        else if (_length > 100)
            _dur = 9f;

        if (_interval <= _dur && _interval > 0f)
            _dur = _interval;

        return _dur;
    }

    public void SayOneWord(string _content, float _interval)
    {
        mLabel.text = _content;
        //mInterval = _interval;
        mDuration = GetDuration(_content.Length, _interval);
        ableCalculate = true;
        mStartCountDown = true;
    }

    void CalculateSentenseLenth()
    {
        if (!ableCalculate)
            return;
        float scaleX = mLabel.relativeSize.x * mLabel.transform.localScale.x + 30f;
        Vector3 _scale = new Vector3(scaleX, mSpr.transform.localScale.y, mSpr.transform.localScale.z);
        mSpr.transform.localScale = _scale;

        ableCalculate = false;

    }

    void OnCountDown()
    {
        if (!mStartCountDown)
            return;
        if (mDuration <= 0f)
            return;
        mDuration -= Time.deltaTime;

        if (mDuration <= 1f)
            OnDisappear(mDuration);

        if (mDuration <= 0f)
        {
            if (OnDestroySelfEvent != null)
                OnDestroySelfEvent(this);
            Destroy(gameObject);
        }

    }

    public void AheadDisappear()
    {
        if (OnDestroySelfEvent != null)
            OnDestroySelfEvent(this);
        Destroy(gameObject);
    }

    void OnDisappear(float _alpha)
    {
        mLabel.color = new Color(1f, 1f, 1f, _alpha);
        mSpr.color = new Color(1f, 1f, 1f, _alpha);
    }

    void Update()
    {
        CalculateSentenseLenth();
        OnCountDown();
    }


}
