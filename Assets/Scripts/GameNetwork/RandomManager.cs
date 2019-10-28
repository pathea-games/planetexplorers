using System.Collections;
using System.Collections.Generic;
using System;

public class CustomRandom
{
	private const int _MaxRandomValue = 0x400;
	private float[] _Value = new float[_MaxRandomValue];
	private int _Index = 0;

	private CustomRandom()
	{
	}

	public CustomRandom(int seed)
	{
		Random random = new Random(seed);
		for (int i = 0; i < _MaxRandomValue; i++)
		{
			_Value[i] = (float)random.NextDouble();
		}
	}

	public void Reset()
	{
		_Index = 0;
	}

	public float Value{		get{	return _Value[_Index++&0x3ff];		}	}
	public float Range(float min, float max)
	{
		return (max - min) * Value + min;
	}

	public int Range(int min, int max)
	{
		return (int)((max - min) * Value + min);
	}	
}

//public static class RandomManager
//{
//	private static Dictionary<int, CustomRandom> _Randoms = new Dictionary<int, CustomRandom>();

//	private static CustomRandom _CurrentRandom;

//	private const int _DefaultSeed = 123654789;
//	private static int _CurrentSeed = _DefaultSeed;

//	public static int Seed
//	{
//		set
//		{
//			_CurrentSeed = value;
//			if (!_Randoms.ContainsKey(_CurrentSeed))
//			{
//				CustomRandom _random = new CustomRandom(_CurrentSeed);
//				_Randoms.Add(_CurrentSeed, _random);
//			}

//			_CurrentRandom = _Randoms[_CurrentSeed];
//		}
//	}

//	public static int CurrentSeed { get { return _CurrentSeed; } }

//	public static float Value
//	{
//		get
//		{
//			if (null == _CurrentRandom)
//				Seed = _DefaultSeed;

//			return _CurrentRandom.Value;
//		}
//	}

//	public static void Reset(int seed)
//	{
//		if (_Randoms.ContainsKey(seed))
//			_Randoms[seed].Reset();
//	}

//	public static float Range(float min, float max)
//	{
//		return (max - min) * Value + min;
//	}

//	public static int Range(int min, int max)
//	{
//		return Mathf.RoundToInt((max - min) * Value + min);
//	}
//}
