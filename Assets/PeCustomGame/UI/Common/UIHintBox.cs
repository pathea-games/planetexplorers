using UnityEngine;
using System.Collections;
using System;

public class UIHintBox : MonoBehaviour 
{
	[SerializeField] UILabel msgLb;
	[SerializeField] TweenScale tweenScale;
	
	public string Msg = "Check";
	
	public float multiplicator = 0.02f;
	public float errorVal = 0.01f;
	public bool isProcessing = true;

	public event Action onOpen;
	public event Action onClose;

	float mCurTime = 0;
	int mPointCount = 0;

	const int MaxPointCount = 6;

	public void Open ()
	{
		this.mCurTime = 0;
		this.mPointCount = 0;
		this.gameObject.SetActive(true);


		if (onOpen != null)
			onOpen();

		tweenScale.Play(true);

	}

	public void Close ()
	{
		tweenScale.Play(false);
	}

	void Update ()
	{
		if (isProcessing)
		{
			mCurTime = Mathf.Lerp(mCurTime, 1, multiplicator);//* Time.deltaTime;

			mPointCount = (int)Mathf.Clamp(MaxPointCount * (mCurTime / 1), 0, MaxPointCount);

			if (Mathf.Abs( mCurTime - 1) < errorVal)
			{
				mCurTime = 0;
				mPointCount = 0;
			}

			string point = "";
			for (int i = 0; i < mPointCount; i++)
			{
				point += ".";
			}

			msgLb.text =  Msg + point;
		}
		else
		{
			mCurTime = 0;
			msgLb.text = Msg;
		}

		CalcuMsgLabelPos();
	}

	void OnTweenFinished(UITweener tween)
	{
		if (tween.direction == AnimationOrTween.Direction.Reverse)
		{
			this.gameObject.SetActive(false);
			if (onClose != null)
				onClose();
		}
	}


	void CalcuMsgLabelPos ()
	{
		Vector3 msg_size = msgLb.font.CalculatePrintedSize(Msg, false, UIFont.SymbolStyle.None);
		msg_size.x *= msgLb.transform.localScale.x;
		Vector3 local_pos = msgLb.transform.localPosition;
		local_pos.x = -Mathf.FloorToInt( msg_size.x * 0.5f) - 5;
		msgLb.transform.localPosition = local_pos;
	}
}
