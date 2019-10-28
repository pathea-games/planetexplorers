using UnityEngine;
using System.Collections;

public class AiMissionMonster : AiNetwork
{
	protected int _ownerId = -1;

	public int OwnerId { get { return _ownerId; } }

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		base.OnPEInstantiate(info);
		_ownerId = info.networkView.initialData.Read<int>();
	}
}
