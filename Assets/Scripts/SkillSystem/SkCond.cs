//#define DBG_COL_ATK

using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkillSystem
{
	public class SkCondColDesc
	{
		public enum ColType
		{
			ColName,
			ColLayer,
		}
		public string[][] colStrings = new string[2][];
		public ColType[] colTypes = new ColType[2]{ ColType.ColName, ColType.ColName };
		public SkCondColDesc(string colDesc)
		{
			string[] colNamesDesc = colDesc.Split(new char[]{','}, 2);
			colStrings[0] = colNamesDesc[0].Split(new char[]{':'});
			int idx = 0;
			if(colStrings[0].Length > 1)
			{
				colTypes[0] = colStrings[0][0].Contains("name") ? ColType.ColName : ColType.ColLayer;
				idx = 1;
			}
			colStrings[0] = colStrings[0][idx].Split(new char[]{'|'});

			idx = 0;
			colStrings[1] = colNamesDesc[1].Split(new char[]{':'});
			if(colStrings[1].Length > 1)
			{
				colTypes[1] = colStrings[1][0].Contains("name") ? ColType.ColName : ColType.ColLayer;
				idx = 1;
			}
			colStrings[1] = colStrings[1][idx].Split(new char[]{'|'});
		}
		public bool Contain(Pathea.PECapsuleHitResult colInfo)
		{
			string strCol0 = (colTypes[0] == ColType.ColName) ? colInfo.selfTrans.name : LayerMask.LayerToName(colInfo.selfTrans.gameObject.layer);
			string strCol1 = (colTypes[1] == ColType.ColName) ? colInfo.hitTrans.name : LayerMask.LayerToName(colInfo.hitTrans.gameObject.layer);
			if(colStrings[0][0].Length == 0 || colStrings[0].Contains(strCol0))
			{
				if(colStrings[1][0].Length == 0 || colStrings[1].Contains(strCol1))
					return true;
#if DBG_COL_ATK
				string log = "FAILED Target Col:" + strCol1 + "|";
				foreach (string str in colStrings[1]) log += str + ",";
				Debug.Log(log);
#endif
				return false;
			}
#if DBG_COL_ATK
			string log = "FAILED Target Col:" + strCol0 + "|";
			foreach (string str in colStrings[0]) log += str + ",";
			Debug.Log(log);
#endif
			return false;
		}
	}
	public class SkFuncInOutPara
	{
		public SkInst _inst;
		public System.Object _para;	// in/out
		public bool _ret;
		public SkFuncInOutPara(SkInst inst, System.Object para, bool ret = false)
		{
			_inst = inst;
			_para = para;
			_ret = ret;
		}
		// Helper---input parse
		public SkCondColDesc GetColDesc()
		{
			string desc = _para as string;
			return new SkCondColDesc(desc);
		}
		// Helper---output parse
		public List<SkEntity> GetTargets()
		{
			return _para as List<SkEntity>;
		}
	}
	public class SkCond
	{
		public enum CondType
		{
			TypeNormal,
			TypeRTCol,
		}

		internal CondType _type = CondType.TypeNormal;
		internal System.Object _para;
		internal List<SkEntity> _retTars;
		internal System.Func<SkInst, System.Object, SkFuncInOutPara> _cond;
		private SkCond(){}
		private static SkCond CreateSub(string desc)
		{
			SkCond cond = new SkCond();
			try{
				string[] strCond = desc.Split(new char[]{','}, 2);
				switch(strCond[0].ToLower())
				{
				case "loopcnt":
					cond._cond = (SkInst inst, System.Object y)=>new SkFuncInOutPara(inst, y, 
					                                                                 inst.GuideCnt > 0 && inst.GuideCnt < Convert.ToInt32(strCond[1]));
					break;
				case "key":
					cond._cond = (SkInst inst, System.Object y)=>new SkFuncInOutPara(inst, y, 
					                                                                 //Input.GetKey(strCond[1]));
					                                                                 Input.GetKey(KeyCode.Q));
					break;
				case "lasthit":
					cond._cond = (SkInst inst, System.Object y)=>new SkFuncInOutPara(inst, y, 
					                                                                 inst.LastHit);
					break;

				case "col":
					cond._para = strCond[1];
					cond._cond = TstCol;
					cond._type = CondType.TypeRTCol;
					break;
				case "colname":
					cond._para = strCond[1];
					cond._cond = TstColName;
					cond._type = CondType.TypeRTCol;
					break;
				case "collayer":
					cond._para = strCond[1];
					cond._cond = TstColLayer;
					cond._type = CondType.TypeRTCol;
					break;
				case "colinfo":
					cond._cond = TstColInfo;
					cond._type = CondType.TypeRTCol;
					break;

				case "range":
					cond._para = strCond[1];
					cond._cond = TstInRange;
					break;
				case "true":
					cond._cond = (SkInst inst, System.Object y)=>new SkFuncInOutPara(inst, y, true);
					break;
				case "false":
					cond._cond = (SkInst inst, System.Object y)=>new SkFuncInOutPara(inst, y, false);
					break;
				default:
					cond._para = strCond;
					cond._cond = TstExternalFunc;
					break;
				}
			}
			catch
			{
				Debug.LogError("[SKERR]Unrecognized sk cond:"+desc);
			}
			return cond;
		}
		public static SkCond Create(string desc)
		{
			string[] strCond = desc.Split(new char[]{';'});
			List<SkCond> lstCond = new List<SkCond>();
			foreach(string condDesc in strCond)
			{
				try
				{
					int cnt = Convert.ToInt32(condDesc);
					if(lstCond.Count == 0)
					{
						lstCond.Add(CreateSub("true"));
					}
					for(int i = 1; i < cnt; i++)
					{
						lstCond.Add(lstCond[lstCond.Count-1]);
					}
				}
				catch
				{
					lstCond.Add(CreateSub(condDesc));
				}
			}
			SkCond cond;
			if(lstCond.Count == 1)
			{
				cond = lstCond[0];
			}
			else
			{
				cond = new SkCond();
				cond._para = lstCond;
				cond._cond = (SkInst inst, System.Object y)=>{
					List<SkCond> paraLstCond = y as List<SkCond>;
					if(inst.GuideCnt > 0 && inst.GuideCnt <= paraLstCond.Count && paraLstCond[inst.GuideCnt-1].Tst(inst))
					{
						return new SkFuncInOutPara(inst, paraLstCond[inst.GuideCnt-1]._retTars, true);
					}
					return new SkFuncInOutPara(inst, y, false);
				};
			}

			return cond;
		}
		public bool Tst(SkInst inst)
		{
			SkFuncInOutPara funcOut = _cond(inst, _para);
			_retTars = funcOut._ret ? (funcOut._para as List<SkEntity>) : null;
			return funcOut._ret;
		}
		internal static SkFuncInOutPara TstCol(SkInst inst, System.Object strColDesc)
		{
			SkFuncInOutPara funcOut = new SkFuncInOutPara(inst, strColDesc);
			if(inst._colInfo == null)	return funcOut;

			funcOut._ret = false;
			SkCondColDesc cols = funcOut.GetColDesc();
			if(cols.Contain(inst._colInfo))
			{
				SkEntity curTar = (SkEntity)inst._colInfo.hitTrans;
				if(curTar != null)
				{
					funcOut._para = new List<SkEntity>(){curTar};
				}
				funcOut._ret = true;
			}
			return funcOut;
		}
		internal static SkFuncInOutPara TstColName(SkInst inst, System.Object strColDesc)
		{
			SkFuncInOutPara funcOut = new SkFuncInOutPara(inst, strColDesc);
			if(inst._colInfo == null)	return funcOut;

			SkCondColDesc cols = funcOut.GetColDesc();
			cols.colTypes[0] = cols.colTypes[1] = SkCondColDesc.ColType.ColName;
			cols.colStrings[1] = new string[1]{""};	// Ignore target col name
			if(cols.Contain(inst._colInfo))
			{
				SkEntity curTar = (SkEntity)inst._colInfo.hitTrans;
				if(curTar != null)
				{
					funcOut._para = new List<SkEntity>(){curTar};
				}
				funcOut._ret = true;
			}
			return funcOut;
		}
		internal static SkFuncInOutPara TstColLayer(SkInst inst, System.Object strColDesc)
		{
			SkFuncInOutPara funcOut = new SkFuncInOutPara(inst, strColDesc);
			if(inst._colInfo == null)	return funcOut;

			SkCondColDesc cols = funcOut.GetColDesc();
			cols.colTypes[0] = cols.colTypes[1] = SkCondColDesc.ColType.ColLayer;
			if(cols.Contain(inst._colInfo))
			{
				SkEntity curTar = (SkEntity)inst._colInfo.hitTrans;
				if(curTar != null)
				{
					funcOut._para = new List<SkEntity>(){curTar};
				}
				funcOut._ret = true;
			}
			return funcOut;
		}
		internal static SkFuncInOutPara TstColInfo(SkInst inst, System.Object para)
		{
			SkFuncInOutPara funcOut = new SkFuncInOutPara(inst, para);
			if(inst._colInfo == null)	return funcOut;

			SkEntity curTar = (SkEntity)inst._colInfo.hitTrans;
			if(curTar != null)
			{
				funcOut._para = new List<SkEntity>(){curTar};
				funcOut._ret = true;
			}
			return funcOut;
		}
		internal static SkFuncInOutPara TstInRange(SkInst inst, System.Object para)
		{
			SkFuncInOutPara funcOut = new SkFuncInOutPara(inst, para);
			// TODO code inst._caster.TstInRange(funcPara);
			return funcOut;
		}
		// TBD : add external func for target???
		internal static SkFuncInOutPara TstExternalFunc(SkInst inst, System.Object para)
		{
			string[] condDesc = para as string[];
			SkFuncInOutPara funcInOut = new SkFuncInOutPara(inst, condDesc.Length < 2 ? null : condDesc[1]);
			inst._caster.SendMessage(condDesc[0], funcInOut, SendMessageOptions.DontRequireReceiver);
			return funcInOut;
		}
	}
}