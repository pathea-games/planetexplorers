using UnityEngine;
using System.Collections;
using Pathea;

public class PEAbnormalTrigger
{
	public virtual bool Hit () { return false; }
	public virtual void Update() { }
	public virtual void Clear() { }
}

public class PEAT_Time : PEAbnormalTrigger
{
	public float interval{ get; set; }
	float m_ElapseTime;
	public override bool Hit ()
	{
		if (m_ElapseTime >= interval) 
		{
			m_ElapseTime = 0;
			return true;
		}
		return base.Hit ();
	}

	public override void Update ()
	{
		m_ElapseTime += Time.deltaTime;
	}

	public override void Clear ()
	{
		m_ElapseTime = 0;
		base.Clear ();
	}
}

public class PEAT_Event : PEAbnormalTrigger
{
	protected bool validEvent{ get; set; }

	public override bool Hit ()
	{
		if(validEvent)
		{
			validEvent = false;
			return true;
		}
		return base.Hit();
	}

	public void OnEvent()
	{
		validEvent = true;
	}

	public void OnEvent(int value)
	{
		OnEvent ();
	}

	public void OnEvent(float value)
	{
		OnEvent ();
	}
	public override void Update ()
	{
		validEvent = false;
	}
}

public class PEAT_Event_IntArray : PEAT_Event
{
	public int[] intValues{ get; set; }
	public void OnIntEvent(int value)
	{
		for (int i = 0; i < intValues.Length; ++i) 
		{
			if (intValues [i] == value)
			{
				validEvent = true;
				break;
			}
		}
	}
}

public class PEAT_AbnormalHit : PEAbnormalTrigger
{
	public int[] hitAbnormals{ get; set; }
	bool hitAbnormal;

	public override bool Hit ()
	{
		if(hitAbnormal)
		{
			hitAbnormal = false;
			return true;
		}
		return base.Hit ();
	}

	public void OnHitAbnormal(PEAbnormalType type)
	{
		for(int i = 0; i < hitAbnormals.Length; ++i)
		{
			if(hitAbnormals[i] == (int)type)
			{
				hitAbnormal = true;
				break;
			}
		}
	}

	public override void Update ()
	{
		hitAbnormal = false;
	}
}

public class PEAT_EffectEnd : PEAbnormalTrigger
{
	public PEAbnormal_N abnormal{ get; set; }

	public override bool Hit ()
	{
		return abnormal.effectEnd;
	}
}

public class PEAT_InWater : PEAbnormalTrigger
{
	public BiologyViewCmpt view{ get; set; }
	public PassengerCmpt passenger{ get; set; }

	public override bool Hit ()
	{
		if(null != passenger && passenger.IsOnCarrier())
			return false;
		return null != view.monoPhyCtrl && view.monoPhyCtrl.headInWater;
	}
}

