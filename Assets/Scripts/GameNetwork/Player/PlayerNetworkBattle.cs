using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using SkillAsset;

public partial class PlayerNetwork
{
	#region Variables
	private BattleInfo _battleInfo;
	#endregion

	#region Properties
	public BattleInfo Battle { get { return _battleInfo; } }
	#endregion

	#region Action Callback APIs
	void RPC_S2C_PlayerBattleInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_battleInfo = stream.Read<BattleInfo>();
	}
	#endregion
}
