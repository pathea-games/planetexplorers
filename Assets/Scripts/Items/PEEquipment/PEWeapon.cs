using UnityEngine;
using System.Collections;
using ItemAsset;
using SkillSystem;

public enum AttackType
{
    Melee,
    Ranged
}

[System.Serializable]
public class AttackMode
{
    public AttackType type;
	public float minRange;
	public float maxRange;
    public float minSwitchRange;
    public float maxSwitchRange;
	public float minAngle;
	public float maxAngle;
	public float frequency;
	public float damage;
    public bool ignoreTerrain;

    float m_LastUseTime = -99999f;

    public bool IsInCD() { return Time.time - m_LastUseTime <= frequency; }

	public void ResetCD()
	{
		m_LastUseTime = Time.time;
	}

	public bool IsInRange(float dis)
	{
		if(minRange < dis && maxRange > dis)  return true;
		else return false;
	}
}

public interface IWeapon
{
    string[] leisures { get; }
	ItemObject ItemObj{ get; }
	void HoldWeapon(bool hold);
	bool HoldReady { get; }
	bool UnHoldReady { get; }
	AttackMode[] GetAttackMode();
	bool CanAttack(int index = 0);
	void Attack(int index = 0, SkEntity targetEntity = null);
	bool AttackEnd(int index = 0);
	bool IsInCD(int index = 0);
}

public interface IAimWeapon
{
	void SetAimState(bool aimState);

	void SetTarget(Vector3 aimPos);

	void SetTarget(Transform trans);

	bool Aimed { get; }
}