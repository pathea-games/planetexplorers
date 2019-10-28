using UnityEngine;
using System.Collections;

public class CounterScript : MonoBehaviour 
{
	public int runCount = 1;
	public string m_Description;
	
	private float m_FinalCounter;
	public float FinalCounter 	{ get {return m_FinalCounter; } }

	private float m_CurCounter;
	public float CurCounter  { get {return m_CurCounter; } }
	
	// When Time is up, this function will be called
	public delegate void NoParamDel ();
	public NoParamDel OnTimeUp;
	// 
	public delegate void TimeTickDel (float deltaTime);
	public TimeTickDel OnTimeTick;
	
//	private bool m_TimeUp = true;
//	
//	private double m_StartTime = 0;
	
	private bool m_First = true;

	private UTimer m_Timer = new UTimer();

	public void Init(float curCounter, float finalCounter)
	{
		m_FinalCounter = finalCounter;
		m_CurCounter = curCounter;
		m_Timer.Second = (double)(m_FinalCounter - m_CurCounter);

		m_FinalCounter = finalCounter;
		m_First = true;
	}

	public void SetFinalCounter(float finalCounter)
	{
		Init(m_CurCounter, finalCounter);
	}

	public void SetCurCounter (float curCounter)
	{
		Init(curCounter, m_FinalCounter);
	}

	public void SetRunCount(int count){
		runCount = count;
	}

	void Awake ()
	{
	}
	
	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!m_First)
			return;

//		if (GameConfig.GameMode == GameConfig.EGameMode.Singleplayer_Build)
//			m_Timer.ElapseSpeed = -1;
//		else
        //m_Timer.ElapseSpeed = Mathf.Min(-1, -GameTime.Timer.ElapseSpeed / GameTime.NormalTimeSpeed);

        m_Timer.ElapseSpeed = -1;
		m_Timer.Update(Time.deltaTime);

		m_CurCounter = (m_FinalCounter - (float)m_Timer.Second);

		if (m_Timer.Tick <= 0)
		{
			if (OnTimeUp != null)
				OnTimeUp();
			if(runCount<=1)
				DestroyImmediate(this);
			else
				runCount--;
		}
		else
		{
			if (OnTimeTick != null)
				OnTimeTick(Mathf.Abs(m_Timer.ElapseSpeed * Time.deltaTime));
		}
//		if (m_First )
//		{
//			m_StartTime = GameTime.Timer.Second;
//			m_First = false;
//			return;
//		}
//
//		
//		if (CurCounter < FinalCounter)
//		{
//			float delta =  (float)(GameTime.Timer.Second - m_StartTime) * 0.083f;
//
//			CurCounter += delta;
//			if (CurCounter > FinalCounter)
//				delta -= (FinalCounter - CurCounter);
//			if (OnTimeTick != null)
//				OnTimeTick(delta);
//		}
//		else
//		{
//			if (OnTimeUp != null && m_TimeUp)
//			{
//				m_TimeUp = false;
//				OnTimeUp();
//			}
//			DestroyImmediate(this);
//		}
//		
//		m_StartTime = GameTime.Timer.Second;
	}
}
