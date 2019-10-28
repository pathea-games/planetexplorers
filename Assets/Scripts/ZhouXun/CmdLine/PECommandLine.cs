#define NEED_NO_ACTIVATION
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;

public class PECommandLine
{
    class CmdLine
    {
        Dictionary<string, string> mDicCmd = new Dictionary<string,string>(5);
        public bool Parse(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return false;
            }

            string[] args = line.Split(' ');

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    if (!args[i + 1].StartsWith("-"))
                    {
                        if (!mDicCmd.ContainsKey(args[i]))
                        {
                            mDicCmd.Add(args[i], args[i + 1]);
                        }
                        i++;
                    }
                    else
                    {
                        mDicCmd.Add(args[i], null);
                    }
                }
            }

            return true;
        }

        public string GetCmd(string cmd)
        {
            if(mDicCmd.ContainsKey(cmd))
            {
                return mDicCmd[cmd];
            }

            return null;
        }

        public bool HasCmd(string cmd)
        {
            return mDicCmd.ContainsKey(cmd);
        }

        public void Print()
        {
            foreach (KeyValuePair<string, string> item in mDicCmd)
            {
                Debug.Log("["+ item.Key +"] = "+item.Value);
            }
        }
    }

	static int _rw = 1280;
	static int _rh = 720;
	static bool _fs = false;
	static string _oclPara = null;
	static string _invitePara = null;
	public static int W{ get { return _rw; } }
	public static int H{ get { return _rh; } }
	public static bool FullScreen{ get { return _fs; } }
	public static string OclPara{ get { return _oclPara; } }
	public static string InvitePara{ get { return _invitePara; } }

	public static void ParseArgs ()
	{

		string verLog = string.Empty;
#if SteamVersion
		verLog = DateTime.Now.ToString("G") + " Steam Version:" + GameConfig.GameVersion;
#else
		verLog = DateTime.Now.ToString("G") + " Pathea Version:" + GameConfig.GameVersion;
#endif
#if DemoVersion
		verLog += " [Demo]";
#endif
#if UNITY_STANDALONE_LINUX
		verLog += " Linux";
#elif UNITY_STANDALONE_OSX
		verLog += " Osx";
#elif Win32Ver
		verLog += " Win32";
#else
		verLog += " Win64";
#endif
		Debug.Log(verLog);

		//for test
		//SteamFriendPrcMgr.StartUpServerID = 635512081887878468;
		//SteamFriendPrcMgr.InviteState = INVITESTATE.E_INVITE_MAINUI;
		if (!Application.isEditor )
		{
            try
            {
#if !UNITY_STANDALONE_LINUX 
#if !NEED_NO_ACTIVATION
                Activation.Instance.CheckActivatation();
                if (!Activation.Instance.Activated)
                {
                    Debug.Log("Not Activated, game quit.");
                    Application.Quit();
                }
#endif
#endif
				string cmdl = Environment.CommandLine;
                CmdLine cmdLine = new CmdLine();
                if (!cmdLine.Parse(cmdl))
                {
                    Application.Quit();
                    return;
                }
                //cmdLine.Print();

                // DO NOT DELETE !!!
                if (!cmdLine.HasCmd("-from-launcher"))
                {
                    Debug.LogError("The game must open with launcher");
                    Application.Quit();
                    return;
                }

				if (cmdLine.HasCmd("-rw") && cmdLine.HasCmd("-rh"))
                {
                    _rw = Convert.ToInt32(cmdLine.GetCmd("-rw"));
                    _rh = Convert.ToInt32(cmdLine.GetCmd("-rh"));
                    if (_rw > 16384)					_rw = 16384;
                    else if (_rw < 1280)				_rw = 1280;
                    if (_rh > 4096)						_rh = 4096;
                    else if (_rh < 720)					_rh = 720;

                    _fs = cmdLine.HasCmd("-fs");                    
					Screen.fullScreen = _fs;
					if(_fs && SystemInfo.graphicsDeviceVersion.Contains("OpenGL")){	
						// Special for full screen OpenGL
						// Use native resolution otherwise game black screen
						_rw = Screen.currentResolution.width;
						_rh = Screen.currentResolution.height;
					}
					Debug.Log("Request Game resolution from launcher["+_rw+"X"+_rh+"] fs[" + _fs + "]");
					Screen.SetResolution(_rw,_rh,_fs);
                } else {
					_rw = Screen.currentResolution.width;
					_rh = Screen.currentResolution.height;
					_fs = true;
					Debug.Log("Request Game resolution Screen current resolution["+_rw+"X"+_rh+"] fs[" + _fs + "]");
					Screen.SetResolution(_rw,_rh,_fs);
				}
				if(cmdLine.HasCmd("-ql"))
				{
					int ql = Convert.ToInt32(cmdLine.GetCmd("-ql"));
					QualitySettings.SetQualityLevel(ql, true);
				}
                if (cmdLine.HasCmd("-language"))
                {
                    string value = cmdLine.GetCmd("-language");
                    SystemSettingData.Instance.mLanguage = value.ToLower();
					Debug.Log("Request Language: "+SystemSettingData.Instance.mLanguage);
                }
                if (cmdLine.HasCmd("-ocl"))
                {
                    _oclPara = cmdLine.GetCmd("-ocl");
                }
				if (cmdLine.HasCmd("-inviteto"))
				{
					_invitePara = cmdLine.GetCmd("-inviteto");
				} 
				if (cmdLine.HasCmd("-userpath"))
				{
					string strUserPath = cmdLine.GetCmd("-userpath");
					GameConfig.SetUserDataPath(strUserPath);
					Debug.Log("Request UserDataPath: "+strUserPath);
				} 
            }
            catch (Exception ex)
            {
                Debug.Log("Process CommandLine excepution:  " + ex.ToString());
                Application.Quit();
            }
		}
	}
}
