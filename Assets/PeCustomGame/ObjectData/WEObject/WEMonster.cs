using UnityEngine;
using System.Collections;
using Pathea.Maths;
using System.Xml;
using System.Reflection;
using System;

[XMLObject("MONSTER")]
public class WEMonster : WEEntity
{
	private bool areaSpwan = false;

	[XMLIO(Attr = "area", DefaultValue = false, Order = -1)]
	public bool AreaSpwan
	{
		get { return areaSpwan; }
		set
		{
            areaSpwan = value;
		}
	}

	[XMLIO(Attr = "spawnAmount", DefaultValue = 1)]
	public int SpawnAmount = 1;

	[XMLIO(Attr = "isSocial", DefaultValue = false)]
	public bool IsSocial = false;

	[XMLIO(Attr = "amountPerSocial", DefaultValue = 1)]
	public int AmountPerSocial = 1;

	[XMLIO(Attr = "spawnScale", DefaultValue = 1f)]
	public float SpawnScale = 1f;

	[XMLIO(Attr = "spawnScaleError", DefaultValue = 0.1f)]
	public float SpawnScaleError = 0.1f;

	[XMLIO(Attr = "maxRespawn", DefaultValue = 0)]
	public int MaxRespawnCount = 0;
	
	[XMLIO(Attr = "respawnTime", DefaultValue = 180f)]
	public float RespawnTime = 180f;
}
