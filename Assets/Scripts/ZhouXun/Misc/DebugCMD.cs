#define GAME_DEBUG_CMD
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class DebugCMD : MonoBehaviour
{
	#if GAME_DEBUG_CMD
	public GUISkin GSkin;
	public string Tips = "[Ctrl]+[Alt]+[C][M][D] to show cmdline";
	private bool bShowCmdLine = false;
	private int HotKeyStep = 0;

	private GameObject selectedGameObject;
	private Material selectedMaterial;

	// Awake
	void Awake ()
	{
		DontDestroyOnLoad(this);
		InitCommands();
		m_DisabledGOs = new Dictionary<string, GameObject>();
	}

	// Update is called once per frame
	void Update ()
	{
		Tips = "[Ctrl]+[Alt]+[C][M][D] to show cmdline";
		if ( Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) )
		{
			if ( HotKeyStep == 0 && Input.GetKeyDown(KeyCode.C) )
			{
				HotKeyStep = 1;
			}
			else if ( HotKeyStep == 1 && Input.anyKeyDown )
			{
				if ( Input.GetKeyDown(KeyCode.M) )
					HotKeyStep = 2;
				else
					HotKeyStep = 0;
			}
			else if ( HotKeyStep == 2 && Input.anyKeyDown )
			{
				if ( Input.GetKeyDown(KeyCode.D) )
					bShowCmdLine = !bShowCmdLine;
				if ( Input.GetKeyDown(KeyCode.L) )
					bShowCmdLine = !bShowCmdLine;
				HotKeyStep = 0;
			}
		}
		else
		{
			HotKeyStep = 0;
		}

		if ( Application.isEditor )
		{
			if ( Input.GetKeyDown(KeyCode.BackQuote) )
			{
				bShowCmdLine = !bShowCmdLine;
			}
		}

		if ( bShowCmdLine )
		{
			if ( gui_focus_name == "CmdLine" )
			{
				if ( cmdstr.Contains("\n") )
				{
					cmdstr = cmdstr.Trim();
					cmdstr = cmdstr.Replace("\r\n", "");
					cmdstr = cmdstr.Replace("\n", "");
					if ( cmdstr.Length > 0 )
						ExecuteCommand(cmdstr);
					else
						console_text += ">>\r\n";
					cmdstr = "";
				}
				float dw = Input.GetAxis("Mouse ScrollWheel");
				if ( dw != 0 )
				{
					int idx = history_cmds.Count;
					for ( int i = 0; i < history_cmds.Count; ++i )
					{
						if ( history_cmds[i] == cmdstr )
						{
							idx = i;
							break;
						}
					}
					if ( idx - 1 >= 0 && dw > 0 )
						cmdstr = history_cmds[idx-1];
					if ( idx + 1 < history_cmds.Count && dw < 0 )
						cmdstr = history_cmds[idx+1];
				}
			}
			return_key = Input.GetKeyDown(KeyCode.Return);
		}
	}

	string cmdstr = "";
	string console_text = "";
	List<string> history_cmds = new List<string> ();
	string gui_focus_name = "";
	bool return_key = false;
#if UNITY_EDITOR
	System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
#endif
	void OnGUI ()
	{
#if UNITY_EDITOR
		if (GUI.Button(new Rect(10, 130 + 60, 70, 20), "GC")){
			sw.Reset();
			sw.Start();
			GC.Collect();
			sw.Stop();
			Debug.LogError("GC takes "+sw.ElapsedMilliseconds);
		}
#endif

		GUI.skin = GSkin;
		GUI.depth = -2000;
		if ( bShowCmdLine )
		{
			GUI.SetNextControlName("CmdLine");
			cmdstr = GUI.TextArea(new Rect(10, Screen.height - 30, Screen.width - 20, 20), cmdstr);
			GUI.SetNextControlName("Console");
			GUI.TextArea(new Rect(10, Screen.height - 234, Screen.width - 20, 199), console_text.Trim(), "label");
			gui_focus_name = GUI.GetNameOfFocusedControl();
			if ( return_key ) GUI.FocusControl("CmdLine");
		}
	}
	
	void ExecuteCommand (string cmd)
	{
		cmd = cmd.Trim();
		console_text += (cmd + "\r\n");
		if ( !history_cmds.Contains(cmd) )
			history_cmds.Add(cmd);

		foreach ( KeyValuePair<string, DCmdFunc> kvp in Commands )
		{
			string head = "-" + kvp.Key + " ";
			string emptyhead = "-" + kvp.Key;
			int lhead = head.Length;
			int idx = cmd.IndexOf(head);
			if ( idx == 0 )
			{
				string param = cmd.Substring(lhead, cmd.Length - lhead).Trim();
				try
				{
					string retval = kvp.Value(param);
					if ( retval.Trim().Length < 1 )
						retval = "Command has no return.";
					console_text += (">> " + retval + "\r\n");
				}
				catch (Exception e)
				{
					string error = "command error: " + e.ToString();
					console_text += (">> " + error + "\r\n");
				}
				return;
			}
			if ( cmd == emptyhead )
			{
				try
				{
					string retval = kvp.Value("");
					if ( retval.Trim().Length < 1 )
						retval = "Command has no return.";
					console_text += (">> " + retval + "\r\n");
				}
				catch (Exception e)
				{
					string error = "command error: " + e.ToString();
					console_text += (">> " + error + "\r\n");
				}
				return;
			}
		}
		console_text += ">> Bad command.\r\n";
	}

	#region COMMANDS_DEF
	private delegate string DCmdFunc (string param);
	private Dictionary<string, DCmdFunc> Commands;
	private void InitCommands ()
	{
		Commands = new Dictionary<string, DCmdFunc>();
		Commands.Add("help", CmdHelp);
		Commands.Add("activego", ActiveGO);
		Commands.Add("deactivego", DeactiveGO);
		Commands.Add("selectgo", SelectGO);
		Commands.Add("selectmat", SelectMaterial);
		Commands.Add("setshaderfile", SetShaderFromFile);
		Commands.Add("setshader", SetShader);
		Commands.Add("matprop", GetMaterialProperty);
		Commands.Add("timespeed", TimeSpeed);
		Commands.Add("passtime", PassTime);
		Commands.Add("repair", Repair);
		Commands.Add("recharge", Recharge);
        Commands.Add("entity", EntityCmd);
		Commands.Add("map", MapCmd);
		Commands.Add("fmodeditor", ToggleFMODEditor);
		Commands.Add("fmodbanklist", FMODBankList);
		Commands.Add("walkspeed", WalkingSpeed);
		Commands.Add("jumpheight", JumpingStrength);
		Commands.Add("nofall", NoFallingDamage);
		Commands.Add("whosyourdaddy", Invincible);
		Commands.Add("whosyourmommy", Uninvincible);
		Commands.Add("buildgod", BuildGod);
		Commands.Add("showbuildgui", ShowBuildGUI); 
		Commands.Add("ignoremapcheck", IgnoreMapCheck);
		Commands.Add("customevt", CustomEvent);
		Commands.Add("customcdt", CustomCondition);
		Commands.Add("customact", CustomAction);
		Commands.Add("scenariotool", ToogleScenarioTool);
		Commands.Add("lagtst", LagTest);
		Commands.Add("gopointer", GoPointer);
	}
	#endregion

	#region COMMAND_FUNCTIONS

	// -help
	string CmdHelp (string param)
	{
		string help = "";
		foreach ( KeyValuePair<string, DCmdFunc> kvp in Commands )
		{
			string head = "-" + kvp.Key + " ";
			help += head;
		}
		return help;
	}
	
	// -activego
	string ActiveGO (string param)
	{
		if ( param == "" )
			return "Parameter excepted";
		if ( m_DisabledGOs.ContainsKey(param) )
		{
			GameObject tar_go = m_DisabledGOs[param];
			tar_go.SetActive(true);
			return tar_go.name + " has been enabled.";
		}
		else
		{
			return param + " is active or not exist";
		}
	}
	
	// -deactivego
	private Dictionary<string, GameObject> m_DisabledGOs;
	string DeactiveGO (string param)
	{
		if ( param == "" )
			return "Parameter excepted";
		GameObject tar_go = GameObject.Find(param);
		if ( tar_go != null )
		{
			tar_go.SetActive(false);
			m_DisabledGOs.Add(param, tar_go);
			return tar_go.name + " has been disabled.";
		}
		else
		{
			return "Could not find GameObject " + param;
		}
	}

	string SelectGO (string param)
	{
		if ( param == "" )
			return "Parameter excepted";
		selectedGameObject = GameObject.Find(param);
		if ( selectedGameObject != null )
		{
			return selectedGameObject.name + " has been selected.";
		}
		else
		{
			return "Could not find GameObject " + param;
		}
	}

	string SelectMaterial (string param)
	{
		if (selectedGameObject == null)
			return "Select a GameObject first";
		int index = 0;
		if ( param != "" )
		{
			int.TryParse(param, out index);
		}

		Renderer r = selectedGameObject.GetComponent<Renderer>();
		if (r == null)
			return "No material selected";
		if (index >= r.materials.Length)
			return "No material selected";
		if (index < 0)
			return "No material selected";
		selectedMaterial = r.materials[index];
		if (selectedMaterial == null)
			return "No material selected";
		else
			return "Material: [" + selectedMaterial.name + "] selected!";
	}

	string SetShaderFromFile (string param)
	{
		if (selectedMaterial == null)
			return "select a material first";
		try
		{
			Shader shader = Pathea.IO.FileUtil.LoadShader(GameConfig.PEDataPath + param);
			if (shader == null)
				return "Create shader failed";
			selectedMaterial.shader = shader;
			return "Shader: [" + shader.name + "] has been set to material";
		}
		catch
		{
			return "Open shader file failed";
		}
	}
	
	string SetShader (string param)
	{
		if (selectedMaterial == null)
			return "select a material first";
		try
		{
			Shader shader = Resources.Load<Shader>(param);
			if (shader == null)
				return "Load shader failed";
			selectedMaterial.shader = shader;
			return "Shader: [" + shader.name + "] has been set to material";
		}
		catch
		{
			return "Open shader file failed";
		}
	}

	string GetMaterialProperty (string param)
	{
		if (selectedMaterial == null)
			return "select a material first";
		return "float: " + selectedMaterial.GetFloat(param).ToString() + 
			"\r\nColor: " + selectedMaterial.GetColor(param).ToString() + 
				"\r\nVector: " + selectedMaterial.GetVector(param).ToString();
	}
	
	// -timespeed
	string TimeSpeed (string param)
	{
		if ( param == "" )
			return "Parameter excepted";
		float speed = System.Convert.ToSingle(param);
		GameTime.Timer.ElapseSpeed = speed;
		return "GameTime.Timer.ElapseSpeed = " + param;
	}

	// -passtime
	string PassTime (string param)
	{
		if ( param == "" )
			return "Parameter excepted";
		string[] ps = param.Split(',');
		if ( ps.Length == 1 )
		{
			double g = Convert.ToDouble(ps[0]);
			GameTime.PassTime(g*3600);
		}
		else
		{
			double g = Convert.ToDouble(ps[0]);
			double t = Convert.ToDouble(ps[1]);
			GameTime.PassTime(g*3600, t);
		}
		return "ok";
	}
	string Repair (string param)
	{
		if ( param == "" )
			return "Parameter excepted";
		string[] ps = param.Split(',');

		int id = Convert.ToInt32(ps[0]) + 100000000;
		CreationData crd = CreationMgr.GetCreation(id);

		if ( crd != null )
		{
			float hp = crd.m_Attribute.m_Durability;
			if(ps.Length != 1) hp *= Mathf.Clamp01(Convert.ToSingle(ps[1]) * 0.01f);

			var itemObject = ItemAsset.ItemMgr.Instance.Get(id);
			var lifeCmpt = itemObject.GetCmpt<ItemAsset.LifeLimit>();
			lifeCmpt.floatValue.current = hp;

			//Pathea.SkAliveEntity skAliveEntity = (DragArticleAgent.GetById(id) as DragArticleAgent).itemScript.GetComponent<Pathea.SkAliveEntity>();
			//skAliveEntity.SetAttribute(Pathea.AttribType.Hp, hp, false);
		}
		return "ok";
	}
	string Recharge (string param)
	{
		if ( param == "" )
			return "Parameter excepted";
		string[] ps = param.Split(',');

		int id = Convert.ToInt32(ps[0]) + 100000000;
		CreationData crd = CreationMgr.GetCreation(id);
		
		if ( crd != null )
		{
			float fuel = crd.m_Attribute.m_MaxFuel;
			if(ps.Length != 1) fuel *= Mathf.Clamp01(Convert.ToSingle(ps[1]) * 0.01f);
			
			var itemObject = ItemAsset.ItemMgr.Instance.Get(id);
			var energyCmpt = itemObject.GetCmpt<ItemAsset.Energy>();
			energyCmpt.floatValue.current = fuel;

			//Pathea.SkAliveEntity skAliveEntity = (DragArticleAgent.GetById(id) as DragArticleAgent).itemScript.GetComponent<Pathea.SkAliveEntity>();
			//skAliveEntity.SetAttribute(Pathea.AttribType.Energy, fuel, false);
		}
		return "ok";
	}

    string MapCmd(string param)
    {
        PeMap.StaticPoint.Mgr.Instance.UnveilAll();
        return "ok";
    }

	string ToggleFMODEditor (string param)
	{
		PeFmodEditor.active = !PeFmodEditor.active;
		FMODAudioSource.rteActive = PeFmodEditor.active;
		Pathea.MainPlayerCmpt.gMainPlayer.m_ActionEnable = !PeFmodEditor.active;
		if (PeFmodEditor.active)
		{
			GameObject go = new GameObject ("FMOD Editor");
			go.AddComponent<PeFmodEditor>();
		}
		return "FMOD Editor is now " + (PeFmodEditor.active ? "opened" : "closed");
	}

	string FMODBankList (string param)
	{
		if (FMOD_StudioSystem.instance == null)
			return "No FMOD system found";
		if (FMOD_StudioSystem.instance.System == null)
			return "No FMOD system found";
		FMOD.Studio.Bank[] banks = new FMOD.Studio.Bank[0];
		FMOD_StudioSystem.instance.System.getBankList(out banks);
		string retval = "Bank Count: " + banks.Length.ToString() + "\r\n";
		foreach (FMOD.Studio.Bank bank in banks)
		{
			string bank_path = "";
			bank.getPath(out bank_path);
			retval += "    Bank [" + bank_path + "]\r\n";
			if (param.IndexOf("-event") >= 0 || param.IndexOf("-all") >= 0)
			{
				FMOD.Studio.EventDescription[] eventdescs = new FMOD.Studio.EventDescription[0] ;
				bank.getEventList(out eventdescs);
				//if (eventdescs.Length > 0)
				{
					retval += "        Events: (" + eventdescs.Length + ")\r\n";
					foreach (FMOD.Studio.EventDescription eventdesc in eventdescs)
					{
						string event_path = "";
						eventdesc.getPath(out event_path);
						retval += "            Event [" + event_path + "]\r\n";
					}
				}
			}
			if (param.IndexOf("-vca") >= 0 || param.IndexOf("-all") >= 0)
			{
				FMOD.Studio.VCA[] vcas = new FMOD.Studio.VCA[0] ;
				bank.getVCAList(out vcas);
				//if (vcas.Length > 0)
				{
					retval += "        VCAs: (" + vcas.Length + ")\r\n";
					foreach (FMOD.Studio.VCA vca in vcas)
					{
						string vca_path = "";
						vca.getPath(out vca_path);
						retval += "            VCA [" + vca_path + "]\r\n";
					}
				}
			}
			if (param.IndexOf("-bus") >= 0 || param.IndexOf("-all") >= 0)
			{
				FMOD.Studio.Bus[] buses = new FMOD.Studio.Bus[0] ;
				bank.getBusList(out buses);
				//if (buses.Length > 0)
				{
					retval += "        Buses: (" + buses.Length + ")\r\n";
					foreach (FMOD.Studio.Bus bus in buses)
					{
						string bus_path = "";
						bus.getPath(out bus_path);
						retval += "            Bus [" + bus_path + "]\r\n";
					}
				}
			}
			if (param.IndexOf("-string") >= 0 || param.IndexOf("-all") >= 0)
			{
				int strcnt = 0;
				bank.getStringCount(out strcnt);
				if (strcnt > 0)
				{
					retval += "        Strings: (" + strcnt + ")\r\n";
					for (int i = 0; i < strcnt; ++i)
					{
						Guid _guid;
						string _path;
						bank.getStringInfo(i, out _guid, out _path);
						retval += "            GUID {" + _guid.ToString() + "}    Path [" + _path + "]\r\n";
					}
				}
			}
		}
		Debug.Log(retval);
		return retval;
	}

	string WalkingSpeed (string param)
	{
		if (param == "")
			return "Parameter excepted";
		if (Pathea.PeCreature.Instance == null)
			return "Not in game";
		if (Pathea.PeCreature.Instance.mainPlayer == null)
			return "Not in game";

		HumanPhyCtrl hpc = Pathea.PeCreature.Instance.mainPlayer.GetComponentInChildren<HumanPhyCtrl>();
		if (hpc == null)
			return "HumanPhyCtrl not exist";
		float s = 1;
		if (!float.TryParse(param, out s))
			return "Invalid parameter";
		hpc.mSpeedTimes = s;
		//hpc.ResetSpeed(5*s);
		return "ok";
	}

	string JumpingStrength (string param)
	{
		if (param == "")
			return "Parameter excepted";
		if (Pathea.PeCreature.Instance == null)
			return "Not in game";
		if (Pathea.PeCreature.Instance.mainPlayer == null)
			return "Not in game";
		
		var mgr = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.MotionMgrCmpt>();
		if (mgr == null)
			return "MotionMgrCmpt not exist";
		var jump = mgr.GetAction<Pathea.Action_Jump>();
		if (jump == null)
			return "Action_Jump not exist";
		float s = 1;
		if (!float.TryParse(param, out s))
			return "Invalid parameter";
		jump.m_JumpHeight = 2*s;
		return "ok";
	}

	string NoFallingDamage (string param)
	{
		if (Pathea.PeCreature.Instance == null)
			return "Not in game";
		if (Pathea.PeCreature.Instance.mainPlayer == null)
			return "Not in game";
		
		Pathea.MainPlayerCmpt.gMainPlayer.FallDamageSpeedThreshold = 10000000f;

		return "ok";
	}

	string Invincible (string param)
	{
		if (Pathea.PeCreature.Instance == null)
			return "Not in game";
		if (Pathea.PeCreature.Instance.mainPlayer == null)
			return "Not in game";
		
		Pathea.PeCreature.Instance.mainPlayer.GetComponent<Pathea.BiologyViewCmpt>().ActivateInjured(false);
		
		return "ok";
	}

	string Uninvincible (string param)
	{
		if (Pathea.PeCreature.Instance == null)
			return "Not in game";
		if (Pathea.PeCreature.Instance.mainPlayer == null)
			return "Not in game";
		
		Pathea.PeCreature.Instance.mainPlayer.GetComponent<Pathea.BiologyViewCmpt>().ActivateInjured(true);
		
		return "ok";
	}

	string BuildGod (string param)
	{
		PEBuildingMan.Self.IsGod = !PEBuildingMan.Self.IsGod;
		
		return PEBuildingMan.Self.IsGod.ToString();
	}

	string IgnoreMapCheck(string param)
	{
//		UICustomSelectWndInterpreter csi = GameObject.FindObjectOfType<UICustomSelectWndInterpreter>();
//		if (csi != null)
//		{
//			csi.ignoreIntegrityCheck = !csi.ignoreIntegrityCheck ;
//			return csi.ignoreIntegrityCheck.ToString();
//		}
		UICustomSelectWndInterpreter.ignoreIntegrityCheck = !UICustomSelectWndInterpreter.ignoreIntegrityCheck;

		return UICustomSelectWndInterpreter.ignoreIntegrityCheck.ToString();
	}

	string ShowBuildGUI(string param)
	{
		PEBuildingMan.Self.GUITest = !PEBuildingMan.Self.GUITest;
		
		return PEBuildingMan.Self.GUITest.ToString();
	}

	string CustomEvent (string param)
	{
		param = param.Trim();
		if (string.IsNullOrEmpty(param))
			return "parameters needed";
		string[] ss = param.Split(new string[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
		string classname = ss[0];

		ScenarioRTL.EventListener evt = ScenarioRTL.Asm.CreateEventListenerInstance(classname);
		ScenarioRTL.IO.StatementRaw raw = new ScenarioRTL.IO.StatementRaw();

		if (evt == null)
			return "cannot create statement";

		raw.classname = classname;
		raw.order = 0;
		raw.parameters = new ScenarioRTL.IO.ParamRaw(ss.Length - 1);
		for (int i = 1; i < ss.Length; ++i)
		{
			string paramstr = ss[i];
			string[] nv = paramstr.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
			if (nv.Length == 2)
				raw.parameters.Set(i - 1, nv[0], nv[1]);
			else
				raw.parameters.Set(i - 1, "null", "0");
		}
		evt.Init(null, raw);
		evt.OnPost += OnCustomEventPost;
		evt.Listen();
		return "[" + classname + "] is listening";
	}

	void OnCustomEventPost (ScenarioRTL.EventListener evt)
	{
		Debug.LogWarning("[" + evt.classname + "] posted");
	}

	string CustomCondition (string param)
	{
		param = param.Trim();
		if (string.IsNullOrEmpty(param))
			return "parameters needed";
		string[] ss = param.Split(new string[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
		string classname = ss[0];

		ScenarioRTL.Condition cdt = ScenarioRTL.Asm.CreateConditionInstance(classname);
		ScenarioRTL.IO.StatementRaw raw = new ScenarioRTL.IO.StatementRaw();

		if (cdt == null)
			return "cannot create statement";
		
		raw.classname = classname;
		raw.order = 0;
		raw.parameters = new ScenarioRTL.IO.ParamRaw(ss.Length - 1);
		for (int i = 1; i < ss.Length; ++i)
		{
			string paramstr = ss[i];
			string[] nv = paramstr.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
			if (nv.Length == 2)
				raw.parameters.Set(i - 1, nv[0], nv[1]);
			else
				raw.parameters.Set(i - 1, "null", "0");
		}
		cdt.Init(null, raw);

		bool? check = cdt.Check();
		return "[" + classname + "] check result is " + check.ToString();
	}

	string CustomAction (string param)
	{
		param = param.Trim();
		if (string.IsNullOrEmpty(param))
			return "parameters needed";
		string[] ss = param.Split(new string[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
		string classname = ss[0];

		ScenarioRTL.Action act = ScenarioRTL.Asm.CreateActionInstance(classname);
		ScenarioRTL.IO.StatementRaw raw = new ScenarioRTL.IO.StatementRaw();

		if (act == null)
			return "cannot create statement";

		raw.classname = classname;
		raw.order = 0;
		raw.parameters = new ScenarioRTL.IO.ParamRaw(ss.Length - 1);
		for (int i = 1; i < ss.Length; ++i)
		{
			string paramstr = ss[i];
			string[] nv = paramstr.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
			if (nv.Length == 2)
				raw.parameters.Set(i - 1, nv[0], nv[1]);
			else
				raw.parameters.Set(i - 1, "null", "0");
		}
		act.Init(null, raw);
		StartCoroutine(CustomActionThread(act));
		return "[" + classname + "] is running";
	}

	IEnumerator CustomActionThread (ScenarioRTL.Action act)
	{
		while (!act.Logic()) yield return 0;
	}

	string ToogleScenarioTool (string param)
	{
		PeCustom.PeScenario.s_ShowTools = !PeCustom.PeScenario.s_ShowTools;
		return "ok";
	}
	
	string LagTest (string param)
	{
		param = param.Trim();
		if (string.IsNullOrEmpty(param))
			return "parameters needed";

		double threshold = 0;
		if (double.TryParse(param, out threshold))
		{
			if (threshold < 0.017)
				threshold = 0.017;
			LagTester.threshold = threshold;
			return "ok";
		}
		return "invalid parameter";
	}
	
	string GoPointer (string param)
	{
		ObjectDebugInfoShower shower = GetComponent<ObjectDebugInfoShower>();
		if (shower != null)
			shower.enabled = !shower.enabled;
		return "ok";
	}
	
	#region PeEntity
    const string entityTips = "param:<cmd> <id> [args]";
    string EntityCmd(string param)
    {
        if (string.IsNullOrEmpty(param))
        {
            return entityTips;
        }

        string[] tmp = param.Split(' ');
        if (tmp.Length < 2)
        {
            return entityTips;
        }

        string cmd = tmp[0];
        int id;
        if (!int.TryParse(tmp[1], out id))
        {
            return entityTips;
        }

        List<string> args = new List<string>(tmp);
        args.RemoveRange(0, 2);

        return DoEntityCmd(id, cmd, args.ToArray());
    }

    static bool stringToVector3(string text, out Vector3 v)
    {
        v = Vector3.zero;

        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        string[] arg = text.Split(',');
        if (arg.Length < 3)
        {
            return false;
        }

        if (!float.TryParse(arg[0], out v.x))
        {
            return false;
        }

        if (!float.TryParse(arg[1], out v.y))
        {
            return false;
        }

        if (!float.TryParse(arg[2], out v.z))
        {
            return false;
        }

        return true;
    }

    string DoEntityCmd(int id, string funcName, string[] args)
    {
        Pathea.PeEntity entity = Pathea.EntityMgr.Instance.Get(id);
        if (null == entity)
        {
            return "can't find entity by id:"+id;
        }

        switch (funcName)
        {
            case "servant":
                Pathea.NpcCmpt npcCmpt = entity.GetCmpt<Pathea.NpcCmpt>();
                if (npcCmpt == null)
                {
                    return "no NpcCmpt.";
                }

                Pathea.ServantLeaderCmpt leader = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.ServantLeaderCmpt>();
                if(leader == null)
                {
                    return "no ServantLeaderCmpt";
                }

                leader.AddServant(npcCmpt);
                npcCmpt.SetServantLeader(leader);                
                return "ok";
            case "start_skill":
                int targetId;
                if (!int.TryParse(args[0], out targetId))
                {
                    return "get target id failed";
                }
                Pathea.PeEntity targetEntity = Pathea.EntityMgr.Instance.Get(targetId);
				Pathea.SkAliveEntity skillCmpt = targetEntity.GetCmpt<Pathea.SkAliveEntity>();
                if (null == skillCmpt)
                {
                    return "target have no SkillCmpt";
                }
                int skillId;
                if (!int.TryParse(args[1], out skillId))
                {
                    return "get skill id failed";
                }
                skillCmpt.StartSkill(skillCmpt, skillId);
                return "ok";
            case "Kill":
                Pathea.PeEntity killEntity = Pathea.EntityMgr.Instance.Get(id);
                if (killEntity == null)
                    return "get entity failed with id : " + id;
                killEntity.SetAttribute(Pathea.AttribType.Hp, 0.0f, false);
                return "ok";

            //case "SetBool":
            //    string animationName = args[0];
            //    bool value;
            //    if (!bool.TryParse(args[1], out value))
            //    {
            //        return "get value failed";
            //    }

            //    entity.SetBool(animationName, value);
            //    return "ok";
            default:
                return "not implementd cmd";
        }
    }
    #endregion
	#endregion
	#endif
}
