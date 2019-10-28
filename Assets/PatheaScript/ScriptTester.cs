using UnityEngine;

namespace PatheaScriptExt
{
    public class ScriptTester : MonoBehaviour
    {
        const float axis_x_testnpc1 = 720f;
        byte[] scriptData = null;
        PatheaScript.PsScriptMgr scriptMgr = null;

        void OnGUI()
        {
            TimerMgrGui();

            ScriptToolGui();
        }

        void Update()
        {
            PeTimerMgr.Instance.Update();

            if (null != scriptMgr)
            {
                scriptMgr.Tick();
            }
        }

        void TimerMgrGui()
        {
            PETimer timer = PeTimerMgr.Instance.Get("1");
            if (null != timer)
            {
                GUI.Label(new Rect(500, 50, 100, 20), timer.FormatString("hh:mm:ss"));
            }
        }

        void ScriptToolGui()
        {
            if (GUILayout.Button("ScriptLaunch"))
            {
                PatheaScriptExt.PeFactory f = new PatheaScriptExt.PeFactory();

                scriptMgr = PatheaScript.PsScriptMgr.Create(f);
            }

            if (GUILayout.Button("ScriptStore"))
            {
                if (null != scriptMgr)
                {
                    scriptData = PatheaScript.PsScriptMgr.Serialize(scriptMgr);
                }
            }

            if (GUILayout.Button("ScriptRestore"))
            {
                if (null != scriptData)
                {
                    scriptMgr = PatheaScript.PsScriptMgr.Deserialize(new PatheaScriptExt.PeFactory(), scriptData);
                }
            }
        }
    }
}