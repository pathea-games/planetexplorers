using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom.Compiler;

namespace SkillSystem
{
	public interface ICompilableExp
	{
		void OnCompiled(Action<ISkAttribs, ISkAttribs, ISkAttribsModPara> action);
	}
	public interface ISkAttribsOp{	
		void Exec(ISkAttribs dstAttribs, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para);
	}
	//=====================Ops========================
	public class SkAttribsOpMAT : ISkAttribsOp	// tmp op such as buffs: A*(x+B)+C
	{
		internal int _idx;	
		internal int _idxParaMul = -1;
		internal int _idxParaPreAdd = -1;
		internal int _idxParaPostAdd = -1;
		internal int _idxMul = -1;
		internal int _idxPreAdd = -1;
		internal int _idxPostAdd = -1;
		internal float _mul;
		internal float _preAdd;
		internal float _postAdd;

		public SkAttribsOpMAT(string strArgs)
		{
			string[] args = strArgs.Split(',');
			_idx = Convert.ToInt32(args[0]);
			try
			{
				_mul = Convert.ToSingle(args[1]);
			}
			catch
			{
				string[] para = args[1].Split(':');
				_idxParaMul = Convert.ToInt32(para[0]);
				_idxMul = Convert.ToInt32(para[1]);
			}

			try
			{
				_preAdd = Convert.ToSingle(args[2]);
			}
			catch
			{
				string[] para = args[1].Split(':');
				_idxParaPreAdd = Convert.ToInt32(para[0]);
				_idxPreAdd = Convert.ToInt32(para[1]);
			}

			try
			{
				_postAdd = Convert.ToSingle(args[3]);
			}
			catch
			{
				string[] para = args[1].Split(':');
				_idxParaPostAdd = Convert.ToInt32(para[0]);
				_idxPostAdd = Convert.ToInt32(para[1]);
			}
		}
		public void Exec(ISkAttribs dstAttribs, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
		{
			if(_idxParaMul < 0 && _idxParaPreAdd < 0 && _idxParaPostAdd < 0)	return;
			// Do nothing but set paras
			if(_idxParaMul == 0)		_mul = paraCaster.sums[_idxMul];
			if(_idxParaMul == 1)		_mul = paraTarget.sums[_idxMul];
			_idxParaMul = -1;
			if(_idxParaPreAdd == 0)		_preAdd = paraCaster.sums[_idxPreAdd];
			if(_idxParaPreAdd == 1)		_preAdd = paraTarget.sums[_idxPreAdd];
			_idxParaPreAdd = -1;
			if(_idxParaPostAdd == 0)	_postAdd = paraCaster.sums[_idxPostAdd];
			if(_idxParaPostAdd == 1)	_postAdd = paraTarget.sums[_idxPostAdd];
			_idxParaPostAdd = -1;
		}
	}
	public class SkAttribsOpMAD : ISkAttribsOp
	{
		internal int _idx;	
		internal float _mul;
		internal float _add;
		public SkAttribsOpMAD(string strArgs)
		{
			string[] args = strArgs.Split(',');
			_idx = Convert.ToInt32(args[0]);
			_mul = Convert.ToSingle(args[1]);
			_add = Convert.ToSingle(args[2]);
		}
		public void Exec(ISkAttribs dstAttribs, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
		{
			float var = dstAttribs.raws[_idx];
			var = var*_mul + _add;
			dstAttribs.raws[_idx] = var;
		}
	}
	public class SkAttribsOpEXP : ISkAttribsOp, ICompilableExp
	{
		public bool _bTmpOp = false;
		public int _idx = 0;
		public Action<ISkAttribs, ISkAttribs, ISkAttribsModPara> _deleOpExp;
		public SkAttribsOpEXP(string strArgs)
		{
			string[] args = strArgs.Split(new char[]{','}, 2);
			_idx = Convert.ToInt32(args[0]);
			_bTmpOp = args[1].Contains("buff");
			SkInst.s_ExpCompiler.AddExpString(this, args[1]);
		}
		public void OnCompiled(Action<ISkAttribs, ISkAttribs, ISkAttribsModPara> action)
		{
			_deleOpExp = action;
		}
		public void Exec(ISkAttribs dstAttribs, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
		{
			_deleOpExp(paraCaster, paraTarget, para);
		}
	}
	public class SkAttribsOpGET : ISkAttribsOp
	{
		internal int[] _ids; 
		public SkAttribsOpGET(string strArgs)
		{
			string[] args = strArgs.Split(',');
			int n = args.Length;
			_ids = new int[n];
			for(int i = 0; i < n; i++)
			{
				_ids[i] = Convert.ToInt32(args[i]);
			}
		}
		public void Exec(ISkAttribs dstAttribs, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
		{
			dstAttribs.pack += _ids;
		}
	}
	public class SkAttribsOpRND : ISkAttribsOp
	{
		internal int _cntMin;	// inclusive
		internal int _cntMax;	// inclusive, while the corresponging para in Range func is exclusive, so we'll give it a plus with 1. 
		internal int[] _ids;		// According id to know which kind of things it is, items or skills
		internal float[] _odds;
		public SkAttribsOpRND(string strArgs)
		{
			string[] args = strArgs.Split(',');
			_cntMin = Convert.ToInt32(args[0]);
			_cntMax = Convert.ToInt32(args[1]);
			int nIds = (args.Length-2)/2;
			_ids = new int[nIds];
			_odds = new float[nIds];
			for(int i = 0; i < nIds; i++)
			{
				_ids[i] = Convert.ToInt32(args[2+i*2]);
				_odds[i] = Convert.ToSingle(args[3+i*2]);
			}
		}
		public void Exec(ISkAttribs dstAttribs, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
		{
			int cnt = SkInst.s_rand.Next(_cntMin, _cntMax+1);
			int nIds = _ids.Length;
			int[] cnts = new int[nIds];
			for (int n = 0; n < cnt; n++)
			{
				float rate = (float)SkInst.s_rand.NextDouble();
				for(int i = 0; i < nIds; i++)
				{
					if (rate <= _odds[i])
					{
						cnts[i]++;
						break;
					}
				}
			}
			List<int> items = new List<int>(nIds*2);
			for(int i = 0; i < nIds; i++)
			{
				if(cnts[i] > 0)
				{
					items.Add(_ids[i]);
					items.Add(cnts[i]);
				}
			}
			dstAttribs.pack += items.ToArray();
		}
	}
	//=====================Ops========================

	// furtherTODO : code to support operator cascade, and refactoring code on OPs
	public class SkAttribsModifier
	{
		internal List<ISkAttribsOp> _ops = new List<ISkAttribsOp>();
		internal List<ISkAttribsOp> _tmpOps = new List<ISkAttribsOp>();
		internal List<int> _tmpOpsIdxs = new List<int>();
		private SkAttribsModifier(){}
		public static SkAttribsModifier Create(string desc)
		{
			if(desc.Equals("0") || desc.Equals(""))	return null;

			SkAttribsModifier modifier = new SkAttribsModifier();
			string[] strMods = desc.Split('#');	// use this as splitter because it is not used in expression
			foreach(string strMod in strMods)
			{
				string[] descOp = strMod.Split(new Char[]{','},2);
				switch(descOp[0].ToLower())
				{
				case "mat":	{
					SkAttribsOpMAT op = new SkAttribsOpMAT(descOp[1]);
					modifier._tmpOps.Add(op);
					modifier._tmpOpsIdxs.Add(op._idx);
					}
					break;
				case "mad":	
					modifier._ops.Add(new SkAttribsOpMAD(descOp[1]));
					break;
				case "exp":
				{
					SkAttribsOpEXP op = new SkAttribsOpEXP(descOp[1]);
					if(op._bTmpOp)
					{
						modifier._tmpOps.Add(op);
						modifier._tmpOpsIdxs.Add(op._idx);
					}
					else
					{
						modifier._ops.Add(op);
					}
				}
					break;
				case "get":
					modifier._ops.Add(new SkAttribsOpGET(descOp[1]));
					break;
				case "rnd":
					modifier._ops.Add(new SkAttribsOpRND(descOp[1]));
					break;
				default:
					Debug.Log("[Error]:Unrecognized atttribModifier."+strMod);
					continue;
				}
			}
			return modifier;
		}
		public void ReqExecTmp(ISkAttribs dst)
		{
			int n = _tmpOpsIdxs.Count;
			for(int i = 0; i < n; i++)
			{
				dst.modflags[_tmpOpsIdxs[i]] = true;
			}
		}
		public void Exec(ISkAttribs dst, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para)
		{
			int n = _ops.Count;
			for(int i = 0; i < n; i ++)
			{
				_ops[i].Exec(dst, paraCaster, paraTarget, para);
			}
			ReqExecTmp(dst);
		}
		public void TryExecTmp(ISkAttribs dst, ISkAttribs paraCaster, ISkAttribs paraTarget, ISkAttribsModPara para, int idxToMod, int times = 1)
		{
			int n = _tmpOpsIdxs.Count;
			for(int i = 0; i < n; i++)
			{
				if(_tmpOpsIdxs[i] == idxToMod)
				{
					for(int j = 0; j < times; j++)
					{
						_tmpOps[i].Exec(dst, paraCaster, paraTarget, para);

						SkAttribsOpMAT op = _tmpOps[i] as SkAttribsOpMAT;
						if(op != null)
						{
							dst.buffMul += op._mul;
							dst.buffPreAdd += op._preAdd;
							dst.buffPostAdd += op._postAdd;
						}
					}
				}
			}
		}
	}
}