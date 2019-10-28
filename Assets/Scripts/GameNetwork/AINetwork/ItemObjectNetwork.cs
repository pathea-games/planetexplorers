using UnityEngine;
using System.Collections;
using SkillAsset;

public class CommonNetworkObject : CommonInterface
{
	public override ESkillTargetType GetTargetType() { return ESkillTargetType.TYPE_SkillRunner; }
	public override Vector3 GetPosition()
	{
		return transform.position;
	}
}
