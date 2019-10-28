//#define DBG_ExpEvaluate
#define CompileOnce4ActionArray
using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace SkillSystem
{
	public interface IExpCompiler
	{
		void Compile();
		void AddExpString(ICompilableExp op, string strExp);
	}

	public class SkExpEvaluator : IExpCompiler
	{
		List<ICompilableExp> _reqers = new List<ICompilableExp>();
		List<string> _progs = new List<string>();
		public SkExpEvaluator()
		{
			Mono.CSharp.Evaluator.Init(new string[] {} );
			Mono.CSharp.Evaluator.ReferenceAssembly(typeof(ISkAttribs).Assembly);
		}
		public void Compile()
		{
			int t = Environment.TickCount;
#if DBG_ExpEvaluate
			//var method = Mono.CSharp.Evaluator.Compile("System.Console.WriteLine(\"dynamic compiled\");");
			// or with return value.
			var method = Mono.CSharp.Evaluator.Compile("new System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>((caster, target, para) => {" +
			                                                                       "caster.raws[0] += target.sums[0];" +
			                                                                       // "caster.sums[0] += target.raws[0];" +
			                                                                       // "caster.raws[1] += target.sums[1];" +
			                                                                       // "caster.sums[1] += target.raws[1];" +
			                                                                       //"caster.pack += 12;" +
			                                                                       // "caster.mask[0] = true;" +
			                                                                       "});");
			object value = null;
			method(ref value);

			bool[] masks = new bool[32]; masks[0] = true; masks[1] = false;
			SkAttribs casterBaseAttrib = new SkAttribs();
			SkAttribs caster = new SkAttribs(casterBaseAttrib, masks);
			SkAttribs targetBaseAttrib = new SkAttribs();
			SkAttribs target = new SkAttribs(casterBaseAttrib, masks);
			System.Action<ISkAttribs, ISkAttribs, ISkAttribs> action = value as System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>;
			action(caster, target, null);
#elif CompileOnce4ActionArray
			string allprogs = "new System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>[]{";
			for(int i = 0; i < _progs.Count; i++){
				allprogs += _progs[i];
			}
			allprogs += "};";

			var method = Mono.CSharp.Evaluator.Compile(allprogs);
			object allActions = null;
			method(ref allActions);

			System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>[] actions = allActions as System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>[];
			for(int i = 0; i < _reqers.Count; i++) {
				_reqers[i].OnCompiled(actions[i]);
			}
#else
			for(int i = 0; i < _reqers.Count; i++) {
				var method = Mono.CSharp.Evaluator.Compile(_progs[i]);
				object action = null;
				method(ref action);
				_reqers[i].OnCompiled((System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>)action);
				//System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara> action = (System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>)Mono.CSharp.Evaluator.Evaluate(_progs[i]);
				//_reqer[i].OnCompiled(action);
			}
#endif
			_reqers.Clear();
			_progs.Clear ();
			Debug.Log("[EXP]All Compiled:"+(Environment.TickCount-t));
		}
		public void AddExpString(ICompilableExp op, string strExp)
		{
			string sourceCode = "new System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>((caster, target, para) => { "+strExp+"; }),";
			try{
				_reqers.Add(op);
				_progs.Add(sourceCode);
			}
			catch(Exception e)
			{
				Debug.LogError("Failed to add exp string:"+sourceCode+e);
			}
		}
	}
}
