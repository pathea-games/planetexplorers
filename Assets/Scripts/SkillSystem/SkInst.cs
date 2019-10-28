//#define DBG_COL_ATK
using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SkillSystem
{
#if DBG_COL_ATK
	public class SkDebug
	{
		static List<int> _idList = new List<int>();
		internal static void LoadIds()
		{
			try
			{
				using(StreamReader sr = new StreamReader("skid.txt"))
				{
					while (sr.Peek() >= 0) 
					{
						int id = Convert.ToInt32(sr.ReadLine());
						_idList.Add(id);
					}
				}
			}
			catch
			{
				Debug.LogError("Failed to read SkId.txt");
			}
		}
		internal static bool IsDebuggingID(int id)
		{
			return _idList.Contains(id);
		}
	}
#endif

	public class SkData
	{
		internal int _id;
		internal int _desc;
		internal string _icon;
		internal string _name;

		internal bool _interruptable;
		internal float _coolingTime;
		internal int _coolingTimeType;
		internal float _coolingTimeShared;

		internal float _pretimeOfPrepare;
		internal float _postimeOfPrepare;
		internal SkEffect _effPrepare = null;

		internal SkCond _condToLoop = null;
		internal float[] _pretimeOfMain;
		internal float[] _timeOfMain;
		internal float[] _postimeOfMain;
		internal SkEffect _effMainOneTime = null;
		internal SkEffect[] _effMainEachTime = null;
		// For an example of melee attack, Each part of body can has corresponding trigerEvent, namely cond,effect and mods
		internal List<List<SkTriggerEvent>> _events = new List<List<SkTriggerEvent>>();

		internal float _pretimeOfEnding;
		internal float _postimeOfEnding;
		internal SkEffect _effEnding = null;

		internal float GetPretimeOfMain(int idx){			return idx < _pretimeOfMain.Length ? _pretimeOfMain [idx] : _pretimeOfMain [_pretimeOfMain.Length - 1];			}
		internal float GetTimeOfMain(int idx){				return idx < _timeOfMain.Length ? _timeOfMain [idx] : _timeOfMain [_timeOfMain.Length - 1];						}
		internal float GetPostimeOfMain(int idx){			return idx < _postimeOfMain.Length ? _postimeOfMain [idx] : _postimeOfMain [_postimeOfMain.Length - 1];			}
		internal List<SkTriggerEvent> GetEvents(int idx){	return idx < _events.Count ? _events[idx] : _events[_events.Count-1];		}
		internal void TryApplyEachEffOfMain(int idx, SkEntity tar, SkRuntimeInfo skrt){		
			SkEffect eff = idx < _effMainEachTime.Length ? _effMainEachTime [idx] : _effMainEachTime [_effMainEachTime.Length - 1];	
			if (eff != null) {
				eff.Apply(tar, skrt);
			}
		}

		internal static Dictionary<int, SkData> s_SkillTbl;
		internal static float[] ToSingleArray(string desc)
		{
			string[] strNums = desc.Split(new char[]{';'});
			int n = strNums.Length;
			float[] nums = new float[n];
			for(int i = 0; i < n; i++)
			{
				nums[i] = Convert.ToSingle(strNums[i]);
			}
			return nums;
		}
		internal static SkEffect[] ToSkEffectArray(string desc)
		{
			string[] strNums = desc.Split(new char[]{';'});
			int n = strNums.Length;
			SkEffect[] effs = new SkEffect[n];
			for(int i = 0; i < n; i++)
			{
				effs[i] = null;
				int id = Convert.ToInt32(strNums[i]);
				SkEffect.s_SkEffectTbl.TryGetValue(id, out effs[i]);
			}
			return effs;
		}
		public static void LoadData()
		{
			if (s_SkillTbl != null)			return;
#if DBG_COL_ATK
			SkDebug.LoadIds();
#endif
			SkEffect.LoadData();
			SkBuff.LoadData();
			SkTriggerEvent.LoadData();
			
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("skMain");
			//reader.Read(); // skip title if needed
			s_SkillTbl = new Dictionary<int, SkData>();
			while (reader.Read())
			{
				SkData skill = new SkData();
				skill._id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_id")));
				skill._desc = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_desc")));
				skill._icon = reader.GetString(reader.GetOrdinal("_icon"));
				skill._name = reader.GetString(reader.GetOrdinal("_name"));
				skill._interruptable = reader.GetString(reader.GetOrdinal("_interruptable")).Equals("1");
				skill._coolingTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_coolingTime")));
				skill._coolingTimeType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_coolingTimeType")));
				skill._coolingTimeShared = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_coolingTimeShared")));
				
				skill._pretimeOfPrepare = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_pretimeOfPrepare")));
				skill._postimeOfPrepare = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_postimeOfPrepare")));
				SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_effPrepare"))), out skill._effPrepare);

				skill._condToLoop = SkCond.Create(reader.GetString(reader.GetOrdinal("_cond")));
				skill._pretimeOfMain = ToSingleArray(reader.GetString(reader.GetOrdinal("_pretimeOfMain")));
				skill._timeOfMain = ToSingleArray(reader.GetString(reader.GetOrdinal("_timeOfMain")));
				skill._postimeOfMain = ToSingleArray(reader.GetString(reader.GetOrdinal("_postimeOfMain")));
				skill._effMainEachTime = ToSkEffectArray(reader.GetString(reader.GetOrdinal("_effMainEachTime")));
				SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_effMainOneTime"))), out skill._effMainOneTime);
				
				string[] strEventsList = reader.GetString(reader.GetOrdinal("_triggerEvents")).Split(';');
				foreach(string strEvents in strEventsList)
				{
					List<SkTriggerEvent> curEvents = new List<SkTriggerEvent>();
					string[] strCurEvents = strEvents.Split(',');
					foreach(string strEvent in strCurEvents)
					{
						int eventId = Convert.ToInt32(strEvent);
						SkTriggerEvent curEvent;
						SkTriggerEvent.s_SkTriggerEventTbl.TryGetValue(eventId, out curEvent);
						if(curEvent != null)
						{
							curEvents.Add(curEvent);
						}
					}
					skill._events.Add(curEvents);
				}

				skill._pretimeOfEnding = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_pretimeOfEnding")));
				skill._postimeOfEnding = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_postimeOfEnding")));
				SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_effEnding"))), out skill._effEnding);

				try{
					s_SkillTbl.Add(skill._id, skill);
				}catch(Exception e)	{
					Debug.LogError("Exception on skMain "+skill._id+" "+e);
				}
			}	

			new Thread(new ThreadStart(SkInst.s_ExpCompiler.Compile)).Start();
			//SkInst.s_ExpCompiler.Compile();
		}
	}
	public class SkCoolInfo
	{
		public bool  _bShare;
		public float _fMaxTime;
		public float _fLeftTime;
	}
	class SkInstPool : Pathea.MonoLikeSingleton<SkInstPool>
	{
		public List<SkInst> _skInsts = new List<SkInst>();
	}
	public class SkInst : SkRuntimeInfo
	{
		public const int 
			StepPrepare = 0,
			// >0:loop count;
			StepEnding  = -1,
			StepCooling = -2,
			StepEnded   = -99,
			StepNotStart= StepEnded;
		public const int 
			CoolNotStart= 0x00,
			CoolThis 	= 0x01,
			CoolShare	= 0x02;
		public static System.Random s_rand = new System.Random(0);
		public static IExpCompiler s_ExpCompiler = new SkExpEvaluator(); // SkExpCompiler not work in build

		internal ISkPara _para;
		internal SkEntity _caster; // At the same time, caster is the host of skInst
		internal SkEntity _target;
		internal SkEntity _tmpTar;
		internal SkData _skData;
		internal int _step = StepNotStart;
		internal int _coolStat = CoolNotStart;
		internal List<SkEntity> _hitInGuide = new List<SkEntity>();
		internal float _startTime = -1;
		internal CoroutineStoppable _coroutine;

		private bool _bSkipPreTime = false;
		private bool _bSkipMainTime = false;
		private bool _bSkipPostTime = false;
		private bool CheckSkipPreTime(){ 	return _bSkipPreTime; 	}
		private bool CheckSkipMainTime(){ 	return _bSkipMainTime; 	}
		private bool CheckSkipPostTime(){ 	return _bSkipPostTime; 	}

		internal bool _bExecExtEnable = false;
		internal List<SkTriggerEvent> _eventsActFromCol = new List<SkTriggerEvent>();
		internal List<SkTriggerEvent> _eventsActInGuide = new List<SkTriggerEvent>();
		internal Pathea.PECapsuleHitResult _colInfo = null;
		public float  _forceMagnitude = -1.0f;
		public Vector3  _forceDirection = Vector3.zero;
		#if DBG_COL_ATK
		enum EDbgSubStep
		{
			Non,
			PreTime,
			MainTime,
			PostTime,
		}
		private EDbgSubStep _dbgStep = EDbgSubStep.Non;
		#endif
		public bool LastHit{	get{ return _hitInGuide.Count > 0;		}	}
		public int GuideCnt{	get{ return _step;						}	}	// from 1 on
		public int SkillID{		get{ return _skData._id;				}	}
		public bool IsActive{	get{ return _step >= StepEnding;		}	}
		public bool SkipWaitAll{	set{ _bSkipPreTime = _bSkipMainTime = _bSkipPostTime = value;	}	}
		public bool SkipWaitPre{	get{ return _bSkipPreTime;	} set{	_bSkipPreTime = value;		}	}
		public bool SkipWaitMain{	get{ return _bSkipMainTime;	} set{	_bSkipMainTime = value;		}	}
		public bool SkipWaitPost{	get{ return _bSkipPostTime;	} set{	_bSkipPostTime = value;		}	}
		// SkRuntimeInfo
		public override ISkPara Para{ 		get{ return _para;		}	}
		public override SkEntity Caster{	get{ return _caster;		}	}
		public override SkEntity Target{	get{ return _target == null ? _tmpTar : _target;	}	}

		private SkInst(){}
		private void StopImm()
		{
			_coolStat = CoolNotStart;
			_step = StepEnded;
			SkInstPool.Instance._skInsts.Remove(this);
		}
		public void Start()
		{
			if (_coroutine == null) 
			{
				_coroutine = new CoroutineStoppable(_caster, Exec());
			}
		}
		public void Stop()
		{
			if(_step == StepCooling || _step == StepEnded)	return;

			_coroutine.stop = true;
			_bSkipPreTime = true;
			_bSkipMainTime = true;
			_bSkipPostTime = true;
			if (_caster != null) 									_caster.UnRegisterInstFromExt (this);	// event if this is not in the list, no harm
			if (_caster != null && _caster.isActiveAndEnabled) 		_caster.StartCoroutine (CoolDown ());
			else													StopImm();
		}
		private bool ClassifyTriggerEvents(int idx)		// Bool return true if ext events exist
		{
			List<SkTriggerEvent> curEvents = _skData.GetEvents(idx);
			_eventsActFromCol.Clear ();
			_eventsActInGuide.Clear ();
			foreach(SkTriggerEvent skEvent in curEvents)
			{
				if(skEvent._cond._type == SkCond.CondType.TypeRTCol)
				{
					_eventsActFromCol.Add(skEvent);
				}
				else
				if(skEvent._cond._type == SkCond.CondType.TypeNormal)
				{
					_eventsActInGuide.Add(skEvent);
				}
			}
			return _eventsActInGuide.Count < _skData.GetEvents(idx).Count;
		}
		private void ExecEventsInGuide()
		{
			foreach(SkTriggerEvent skEvent in _eventsActInGuide)				skEvent.Exec(this);
		}
		public void ExecEventsFromCol(Pathea.PECapsuleHitResult colInfo)	// exetern call
		{
			if(_bExecExtEnable)
			{
				SkEntity tar = (SkEntity)colInfo.hitTrans;
				if(tar != null && !_hitInGuide.Contains(tar) && (_target == null || _target == tar))
				{
					_colInfo = colInfo;
					foreach(SkTriggerEvent skEvent in _eventsActFromCol)
					{
						if(skEvent.Exec(this))
						{
							_hitInGuide.Add(tar);
							break; // filter left skevent if needed
						}
					}
                    //Debug.Log("[Skill] Hit:" + _hit);
				}
			}
#if DBG_COL_ATK
			else
			{
				Debug.Log("Event not executed because in " + _dbgStep);
			}
#endif
		}

		private IEnumerator CoolDown()
		{
			_step = StepCooling;
			_coolStat = CoolNotStart;
			float elapseTime = Time.time - _startTime;
			float coolingTime = _skData._coolingTime - elapseTime;
			float coolingTimeShared = _skData._coolingTimeShared - elapseTime;
			if(coolingTime > PETools.PEMath.Epsilon)			_coolStat |= CoolThis;
			if(coolingTimeShared > PETools.PEMath.Epsilon)	_coolStat |= CoolShare;

			bool bShareMin = coolingTimeShared <= coolingTime;
			float min = bShareMin ? coolingTimeShared : coolingTime;
			float max = bShareMin ? coolingTime : coolingTimeShared;

			if(min > PETools.PEMath.Epsilon)		yield return new WaitForSeconds(min);
			_coolStat &= ~(bShareMin ? CoolShare : CoolThis);

			if(max > PETools.PEMath.Epsilon)
			{
				float left = (min > PETools.PEMath.Epsilon) ? (max-min) : max;
				if(left > PETools.PEMath.Epsilon)	yield return new WaitForSeconds(left);
			}
			StopImm ();
		}
		// Main execution
		private IEnumerator Exec()
		{
			_startTime = Time.time;
            //Debug.Log("[SKILL] Start:+"+_skData._id);
            _step = StepPrepare;
			if(_skData._pretimeOfPrepare > PETools.PEMath.Epsilon)			yield return new WaitForSeconds(_skData._pretimeOfPrepare);
			if(_skData._effPrepare != null)							_skData._effPrepare.Apply(_caster, this);
			if(_skData._postimeOfPrepare > PETools.PEMath.Epsilon)			yield return new WaitForSeconds(_skData._postimeOfPrepare);
			
			// According to yin's opinion, move applyEffect front of events
			if(_skData._effMainOneTime != null)						_skData._effMainOneTime.Apply(_caster, this);
			do{
				bool bExtEvents = ClassifyTriggerEvents(_step);			
				_bExecExtEnable = false;            
				if(bExtEvents)										_caster.RegisterInstFromExt(this);//register this inst to recieve extern invocation

				_step++;
				_bSkipPreTime = false;
				_bSkipMainTime = false;
				_bSkipPostTime = false;
				_skData.TryApplyEachEffOfMain(_step-1, _caster, this);
				
				// Pre
#if DBG_COL_ATK
				_dbgStep = EDbgSubStep.PreTime;
#endif
				float fPretime = _skData.GetPretimeOfMain(_step-1);
				if(fPretime > PETools.PEMath.Epsilon)						yield return _caster.StartCoroutine(new WaitTimeSkippable(fPretime, CheckSkipPreTime));
                _hitInGuide.Clear();
				_bExecExtEnable = true;

				// Main
#if DBG_COL_ATK
				_dbgStep = EDbgSubStep.MainTime;
#endif
				ExecEventsInGuide();
				float fMaintime = _skData.GetTimeOfMain(_step-1);
				if(fMaintime > PETools.PEMath.Epsilon)						yield return _caster.StartCoroutine(new WaitTimeSkippable(fMaintime, CheckSkipMainTime));
				_bExecExtEnable = false;

				// Post
#if DBG_COL_ATK
				_dbgStep = EDbgSubStep.PostTime;
#endif
				float fPostime = _skData.GetPostimeOfMain(_step-1);
				if(fPostime > PETools.PEMath.Epsilon)						yield return _caster.StartCoroutine(new WaitTimeSkippable(fPostime, CheckSkipPostTime));

#if DBG_COL_ATK
				_dbgStep = EDbgSubStep.Non;
#endif
				if(bExtEvents)										_caster.UnRegisterInstFromExt(this);//Unregister this inst not to recieve extern invocation
			}while(_skData._condToLoop.Tst(this));
            //Debug.Log("[SKILL] Main Fin");
			_step = StepEnding;
			if(_skData._pretimeOfEnding > PETools.PEMath.Epsilon)			yield return new WaitForSeconds(_skData._pretimeOfEnding);
			if(_skData._effEnding != null)							_skData._effEnding.Apply(_caster, this);
			if(_skData._postimeOfEnding > PETools.PEMath.Epsilon)			yield return new WaitForSeconds(_skData._postimeOfEnding);
            //Debug.Log("[SKILL] Cooling");
			yield return _caster.StartCoroutine(CoolDown());
			//Debug.Log("[SKILL] End");
		}

		// Utils
		public int GetAtkDir()	// Caster-Target
		{
			SkEntity thisTar = _target != null ? _target : _tmpTar;
			if(thisTar == null || thisTar == null)
			{
				Debug.LogError("[SkInst]:Error in GetAtkDir");			
				return 0;
			}
			if (_colInfo == null)
				return 0;

			Transform transTarget = thisTar.GetTransform();
			Vector3 vecAtk = -_colInfo.hitDir;
			Vector3 vecTarFwd = transTarget.forward;
			float cosFwd = Vector3.Dot(vecAtk, vecTarFwd);
			if(cosFwd >  0.866f)						return 2;	//front
			if(cosFwd < -0.866f)						return 3;	//back
			Vector3 vecTarUwd = transTarget.up;
			float cosUwd = Vector3.Dot(vecAtk, vecTarUwd);
			if(cosUwd >  0.707f)						return 4;	//up
			if(cosUwd < -0.707f)						return 5;	//down

			Vector3 vecRgt = Vector3.Cross(vecTarUwd, vecTarFwd);
			float cosRgt = Vector3.Dot(vecAtk, vecRgt);
			return (cosRgt < 0) ? 0 : 1;	// left right
		}
		public Vector3 GetForceVec()	// Target-Caster
		{
			return _colInfo != null ? _colInfo.hitDir : Vector3.zero;
		}
		public Vector3 GetCollisionContactPoint()
		{
			return _colInfo != null ? _colInfo.hitPos : _caster.GetTransform ().position;
		}

		// skill instances management
		//============================
		public static SkInst StartSkill(SkEntity caster, SkEntity target, int skId, ISkPara para = null, bool bStartImm = true)
		{
			SkData skData = null;
			if(!SkData.s_SkillTbl.TryGetValue(skId, out skData))
			{
				Debug.LogError("[SkInst]:Invalid skill id:" + skId);
				return null;
			}

			#if DBG_COL_ATK
			//if(!SkDebug.IsDebuggingID(skId))	return null;
			#endif
			// cool check
			SkCoolInfo cinfo = GetSkillCoolingInfo (caster, skData);
			if (cinfo != null && cinfo._fLeftTime > 0) 
			{
				//Debug.LogError("[SkInst]:Skill id:" + skId +" in cooling..");
				return null;
			}

			SkInst inst = new SkInst();
			SkInstPool.Instance._skInsts.Add(inst);
			inst._skData = skData;
			inst._para = para;
			inst._caster = caster;
			inst._target = target;
			inst._coroutine = bStartImm ? new CoroutineStoppable(caster, inst.Exec()) : null;
			return inst;
		}
		public static int StopSkill(Func<SkInst, bool> match)
		{
			List<SkInst> insts = SkInstPool.Instance._skInsts;
			int n = insts.Count;
			for(int i = n-1; i >= 0; i--)
			{
				SkInst inst = insts[i];
				if(match(inst))
				{
					inst.Stop();
				}
			}
			return insts.Count - n;
		}
		// Get first matched skill
		public static SkInst GetSkill(Func<SkInst, bool> match)
		{
			List<SkInst> insts = SkInstPool.Instance._skInsts;
			int n = insts.Count;
			for(int i = n-1; i >= 0; i--)
			{
				SkInst inst = insts[i];
				if(match(inst))
				{
					return inst;
				}
			}
			return null;
		}
		public static bool IsSkillRunnable(SkEntity caster, int skId)
		{
			SkData skData = null;
			if(!SkData.s_SkillTbl.TryGetValue(skId, out skData))
			{
				return false;
			}
			// cool check
			SkCoolInfo cinfo = GetSkillCoolingInfo (caster, skData);
			if (cinfo != null && cinfo._fLeftTime > 0) 
			{
				return false;
			}
			return true;
		}
		public static float GetSkillCoolingPercent(SkEntity caster, int skId)
		{
			SkData skData = null;
			if(!SkData.s_SkillTbl.TryGetValue(skId, out skData))
			{
				//Debug.LogError("[SkInst]:Invalid skill id:" + skId);
				return -1;
			}
			SkCoolInfo cinfo = GetSkillCoolingInfo (caster, skData);
			if (cinfo != null && cinfo._fLeftTime > 0)
			{
				return cinfo._fLeftTime/cinfo._fMaxTime;
			}
			return 0;
		}
		public static SkCoolInfo GetSkillCoolingInfo(SkEntity caster, SkData skData)
		{
			List<SkInst> insts = SkInstPool.Instance._skInsts;
			foreach(SkInst it in insts)
			{
				if(it._caster == caster)
				{
					if(it._skData._id == skData._id && it._skData._coolingTime > PETools.PEMath.Epsilon)
					{
						SkCoolInfo cinfo = new SkCoolInfo ();
						cinfo._bShare = false;
						cinfo._fMaxTime = it._skData._coolingTime;
						cinfo._fLeftTime = it._skData._coolingTime - (Time.time - it._startTime);
						return cinfo;
					}
					else if(it._skData._coolingTimeType == skData._coolingTimeType && it._skData._coolingTimeShared > PETools.PEMath.Epsilon)
					{
						SkCoolInfo cinfo = new SkCoolInfo ();
						cinfo._bShare = true;
						cinfo._fMaxTime = it._skData._coolingTimeShared;
						cinfo._fLeftTime = it._skData._coolingTimeShared - (Time.time - it._startTime);
						return cinfo;
					}
				}
			}
			return null;
		}
	}
}


