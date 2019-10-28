using Mono.Data.SqliteClient;
using System;
using System.Collections.Generic;
using Pathea;

public enum AttackMaterial
{
	Flesh = 0,
	Bone,
	Carapace,
	Metal,
	Energy,
	Wood,
	Stone,
	HumanBody,
	Blast,
}

public enum DefenceMaterial
{
	Flesh = 0,
	Bone,
	Carapace,
	Metal,
	Energy,
	Wood,
	Stone,
	HumanBody,
	Blast,
}

public class CollisionEffectMgr
{
	static int[,] _particleEeffects;
	static int[,] _soundEeffects;
	static float[,] _damageScale;
	public static void LoadData()
	{
		_particleEeffects = new int[Enum.GetValues(typeof(AttackMaterial)).Length, Enum.GetValues(typeof(DefenceMaterial)).Length];
		_soundEeffects = new int[Enum.GetValues(typeof(AttackMaterial)).Length, Enum.GetValues(typeof(DefenceMaterial)).Length];
		_damageScale = new float[Enum.GetValues(typeof(AttackForm)).Length, Enum.GetValues(typeof(DefenceType)).Length];

		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("CollisionParticle");
		int readLineIndex = 0;
		int startIndex = 0;
		while (reader.Read())
		{
			for(int i = startIndex; i < reader.FieldCount; i++)
				_particleEeffects[readLineIndex, i - startIndex] = System.Convert.ToInt32(reader.GetString(i));
			readLineIndex++;
		}
		reader.Close();

		reader = LocalDatabase.Instance.ReadFullTable("CollisionSound");
		readLineIndex = 0;
		startIndex = 0;
		while (reader.Read())
		{
			for(int i = startIndex; i < reader.FieldCount; i++)
				_soundEeffects[readLineIndex, i - startIndex] = System.Convert.ToInt32(reader.GetString(i));
			readLineIndex++;
		}
		reader.Close();

		reader = LocalDatabase.Instance.ReadFullTable("DmgDefScale");
		readLineIndex = 0;
		startIndex = 2;
		while (reader.Read())
		{
			for(int i = startIndex; i < reader.FieldCount; i++)
				_damageScale[readLineIndex, i - startIndex] = System.Convert.ToSingle(reader.GetString(i));
			readLineIndex++;
		}
		reader.Close();
	}

	static int GetParticleEffectID(int casterMaterial, int hitMaterial)
	{
		return _particleEeffects[casterMaterial, hitMaterial];
	}

	public static int GetParticleEffectID(AttackMaterial casterMaterial, DefenceMaterial hitMaterial)
	{
		return GetParticleEffectID((int)casterMaterial, (int)hitMaterial);
	}

	static int GetSoundEffectID(int casterMaterial, int hitMaterial)
	{
		return _soundEeffects[casterMaterial, hitMaterial];
	}
	
	public static int GetSoundEffectID(AttackMaterial casterMaterial, DefenceMaterial hitMaterial)
	{
		return GetSoundEffectID((int)casterMaterial, (int)hitMaterial);
	}

	static float GetDamageScale(int attackForm, int defenceType)
	{
		return _damageScale[attackForm, defenceType];
	}

	public float GetDamageScale(AttackForm attackForm, DefenceType defenceType)
	{
		return GetDamageScale((int)attackForm, (int)defenceType);
	}
}
