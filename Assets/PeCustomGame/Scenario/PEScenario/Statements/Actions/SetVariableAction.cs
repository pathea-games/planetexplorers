using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("SET VARIABLE", true)]
	public class SetVariableAction : ScenarioRTL.Action
    {
        // 在此列举参数
		string varname = "";
		EScope varscope;
		EFunc func;
		Var value;

        // 在此初始化参数
        protected override void OnCreate()
        {
			varname = Utility.ToVarname(parameters["var"]);
			varscope = (EScope)Utility.ToEnumInt(parameters["scope"]);
			func = Utility.ToFunc(parameters["func"]);
			value = Utility.ToVar(missionVars, parameters["value"]);
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
			VarScope vs = null;
			if (varscope == EScope.Global)
				vs = scenarioVars;
			else if (varscope == EScope.Mission)
				vs = missionVars;

			if (vs != null)
			{
				if (vs.VarDeclared(varname))
					vs[varname] = Utility.FunctionVar(vs[varname], value, func);
				else
					vs[varname] = Utility.FunctionVar(Var.zero, value, func);
			}
            return true;
        }
    }
}
