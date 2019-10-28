// #define DBG_ExpCompile
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

namespace SkillSystem
{

	public class SkExpCompiler : IExpCompiler
	{
		List<ICompilableExp> _reqs = new List<ICompilableExp>();
		string _srcCode = "";

		static Assembly CompileExpressionToMethod(string sourceCode)
		{
			// Note: Use "{{" to denote a single "{" in sourcecode
			CompilerParameters parms = new CompilerParameters();
			parms.GenerateExecutable = false;
			parms.GenerateInMemory = true;
			parms.IncludeDebugInformation = false;
			//parms.TempFiles.KeepFiles = true;
			/*
			var assemblies = typeof(ISkAttribs).Assembly.GetReferencedAssemblies().ToList();
			var assemblyLocations = assemblies.Select(a => 
				                  	Assembly.ReflectionOnlyLoad(a.FullName).Location).ToList();			
			assemblyLocations.Add(typeof(IAttribs).Assembly.Location);			
			parms.ReferencedAssemblies.AddRange(assemblyLocations.ToArray());
			*/
			parms.ReferencedAssemblies.Add(typeof(ISkAttribs).Assembly.Location);
			CodeDomProvider compiler = Microsoft.CSharp.CSharpCodeProvider.CreateProvider("CSharp");
			try
			{
				CompilerResults compilerResults = compiler.CompileAssemblyFromSource(parms, sourceCode);
				if (compilerResults.Errors.HasErrors)
				{
					throw new InvalidOperationException("Expression has a syntax error.");
				}
				return compilerResults.CompiledAssembly;
			}
			catch(Exception e)
			{
				Debug.LogError("[CompileFailed]"+sourceCode+":"+e);
				return null;
			}
		}
		public void Compile()
		{
			int t = Environment.TickCount;
#if DBG_ExpCompile
			_srcCode = "public static class Func0{ public static void func(ISkAttribs caster, ISkAttribs target, ISkAttribsModPara para){ " +
					"caster.raws[0] += target.sums[0];" +
					"caster.sums[0] += target.raws[0];" +
					"caster.raws[1] += target.sums[1];" +
					"caster.sums[1] += target.raws[1];" +
					//"caster.pack += 12;" +
					"caster.mask[0] = true;" +
				"}}";
			Assembly asm0 = CompileExpressionToMethod(_srcCode);
			_reqs[0].OnCompiled(asm0);
			bool[] masks = new bool[32];
			masks[0] = true;
			masks[1] = false;
			SkAttribsOpEXP op = (SkAttribsOpEXP)_reqs[0];
			SkAttribs casterBaseAttrib = new SkAttribs();
			SkAttribs casterAttrib = new SkAttribs(casterBaseAttrib, masks);
			SkAttribs targetBaseAttrib = new SkAttribs();
			SkAttribs targetAttrib = new SkAttribs(casterBaseAttrib, masks);
			op.Exec(targetAttrib, casterAttrib, targetAttrib, null);
#else
			Assembly asm = CompileExpressionToMethod(_srcCode);
			int n = _reqs.Count;
			for(int i = 0; i < n; i++)
			{
				MethodInfo method = asm.GetType("Func"+i).GetMethod("func");
				System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara> action = (System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>)Delegate.CreateDelegate(typeof(System.Action<ISkAttribs, ISkAttribs, ISkAttribsModPara>), null, method);
				_reqs[i].OnCompiled(action);
			}
#endif
			Debug.Log("[EXP]All Compiled:"+(Environment.TickCount-t));

			_reqs.Clear();
			_srcCode = "";
		}
		public void AddExpString(ICompilableExp op, string strExp)
		{
			int idx = _reqs.Count;
			// Note: Use "{{" to denote a single "{"
			string sourceCode = string.Format(
				"public static class Func"+idx+"{{ public static void func(ISkAttribs caster, ISkAttribs target, ISkAttribsModPara para){{ {0};}}}}",
				strExp);

			_reqs.Add(op);
			_srcCode += sourceCode;
		}
	}
}
