using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkillSystem
{
	public class SkColliderTracker
	{
		public class Positions
		{
			const int _N = 2;
			const float _SqrMinDist = 0.0625f;
			Vector3[] _lstPos = null;
			public int _cur = 0;
			public Vector3 this[int idx]
			{
				get{
					return _lstPos[idx];
				}
			}
			public Positions(Vector3 pos)
			{
				_cur = 0;
				_lstPos = new Vector3[_N];
				_lstPos[_cur] = pos;
			}
			public void Add(Vector3 curPos)
			{
				if(Vector3.SqrMagnitude(curPos - _lstPos[_cur]) >= _SqrMinDist)
				{
					_cur ++;
					if(_cur >= _N)	_cur = 0;
					_lstPos[_cur] = curPos;
				}
			}
			public Vector3 GetMoveVec(Vector3 curPos)
			{
				Vector3 vecMove = curPos - _lstPos[_cur];
				if(Vector3.SqrMagnitude(vecMove) >= _SqrMinDist)
				{
					return vecMove;
				}
				return curPos - _lstPos[_cur == 0 ? _N-1 : _cur];
			}
		}
		Dictionary<Collider, Positions> _colPositions = new Dictionary<Collider, Positions>();
		SkInst _inst;
		public SkColliderTracker(SkInst inst)
		{
			_inst = inst;
			_inst._caster.StartCoroutine(Exec());
		}
		public void Add(SkCond colCond)
		{
			SkFuncInOutPara funcOut = new SkFuncInOutPara(_inst, colCond._para);
			SkCondColDesc cols = funcOut.GetColDesc();
			if(cols.colTypes[0] == SkCondColDesc.ColType.ColName)
			{
				foreach(string str in cols.colStrings[0])
				{
					Collider col = _inst._caster.GetCollider(str);
					if(col != null && !_colPositions.ContainsKey(col))
					{
						_colPositions[col] = new Positions(col.Pos());
					}
				}
			}
			else
			{
				//TODO : Getcollider by layer
				Debug.LogError("Unimplemented code for getting collider from collayer cond");
				/*
				foreach(string str in cols.colStrings[0])
				{
					Collider col = _inst._caster.GetCollider(str);
					if(col != null && !_colPositions.ContainsKey(col))
					{
						_colPositions[col] = new Positions(col.Pos());
					}
				}
				*/
			}
		}
		public bool GetMoveVec(Collider col, out Vector3 moveVec)
		{
			Positions poses;
			if(_colPositions.TryGetValue(col, out poses))
			{
				moveVec = poses.GetMoveVec(col.Pos());
				return true;
			}
			moveVec = Vector3.zero;
			return false;
		}
		private IEnumerator Exec()
		{
			while(_inst.IsActive)
			{
				yield return new WaitForFixedUpdate();		//pos is bigger error in normal coroutine, so we wait for FixedUpdate
				if(_colPositions.Count > 0)
				{
					List<Collider> cols = _colPositions.Keys.Cast<Collider>().ToList();
					foreach(Collider col in cols)
					{
						if(col != null)	// for safety in fast-traveling
						{
							_colPositions[col].Add(col.Pos());
						}
					}
				}

			}
		}
	}
}
