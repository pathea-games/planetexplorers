using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pathea;
using System.IO;

public class Scanner : PeCmpt
{
	MSScan _msScaner;
	int _radius = 80;
	int _radiusaddition;
	public int Radius
	{
		get
		{
			return _radius + _radiusaddition;
		}
		set
		{
			_radiusaddition = value;
		}
	}
	List<byte> _matList = new List<byte>();
	List<byte> _additionMatList = new List<byte>();
	public List<byte> GetMatList()
	{
		return _additionMatList;
	}
	public void Add(byte mat)
	{
		if (!_additionMatList.Contains (mat))
			_additionMatList.Add (mat);
	}

	public void ResetMat()
	{
		_additionMatList.Clear ();
		_additionMatList.AddRange (_matList);
	}


	const int VersionID = 1;
	public override void Start() 
	{
        base.Start();

		_msScaner = gameObject.AddComponent<MSScan> ();
	}

	#region IPEComponent implementation
	
//	public override void Serialize(BinaryWriter _out)
//	{
//		_out.Write(VersionID);
//		_out.Write(_radius);
//		PETools.Serialize.WriteBytes(_matList.ToArray(), _out);
//	}
//	
//	public override void Deserialize(BinaryReader _in)
//	{
//		int readVersion = _in.ReadInt32();
//		_radius = _in.ReadInt32();
//		byte[] buff = PETools.Serialize.ReadBytes(_in);
//		for(int i = 0; i < buff.Length; i++)
//		{
//			_matList.Add(buff[i]);
//		}
//	}
	
	#endregion

	public void Clear()
	{
		_matList.Clear ();
		_radius = 0;
	}

	public void AddMat( byte mat)
	{
		if(_matList != null && !_matList.Contains(mat))
		{
			_matList.Add(mat);
		}
	}

	public void RemoveMat(byte mat)
	{
		_matList.Remove (mat);
	}

	public void Scan()
	{
		_msScaner.MakeAScan(transform.position, _matList,Radius);
	}

}

