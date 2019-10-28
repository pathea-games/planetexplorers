using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerBattleInfo
{
	public string Account;
	public string RoleName;
	public int Id;
	public BattleInfo Info;
	
	public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		var battleInfo = new PlayerBattleInfo();
		battleInfo.Account = stream.Read<string>();
		battleInfo.RoleName = stream.Read<string>();
		battleInfo.Id = stream.Read<int>();
		battleInfo.Info = stream.Read<BattleInfo>();
		
		return battleInfo;
	}
	
	public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		var battleInfo = (PlayerBattleInfo)value;
		
		stream.Write<string>(battleInfo.Account);
		stream.Write<string>(battleInfo.RoleName);
		stream.Write<int>(battleInfo.Id);
		stream.Write<BattleInfo>(battleInfo.Info);
	}
}

public class BattleInfo
{
	internal int _group;
    internal int _deathCount;
    internal int _killCount;
    internal float _point;
    internal int _meat;
    internal int _site;

    internal bool IsBattleOver()
    {
        return _killCount >= BattleConstData.Instance._win_kill ||
            _point >= BattleConstData.Instance._win_point ||
            _site >= BattleConstData.Instance._win_site;
    }

	internal void Update(BattleInfo info)
	{
		_deathCount = info._deathCount;
		_killCount = info._killCount;
		_meat = info._meat;
		_site = info._site;
		_point = info._point;
	}

	internal static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		var info = new BattleInfo();
		info._group = stream.Read<int>();
		info._deathCount = stream.Read<int>();
		info._killCount = stream.Read<int>();
		info._meat = stream.Read<int>();
		info._site = stream.Read<int>();
		info._point = stream.Read<float>();

		return info;
	}

	internal static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		var info = (BattleInfo)value;
		stream.Write<int>(info._group);
		stream.Write<int>(info._deathCount);
		stream.Write<int>(info._killCount);
		stream.Write<int>(info._meat);
		stream.Write<int>(info._site);
		stream.Write<float>(info._point);
	}
}
