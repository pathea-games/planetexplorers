using UnityEngine;
using ScenarioRTL;
using System.IO;

namespace PeCustom
{
    [Statement("PLAY SPEECH")]
    public class PlaySpeechAction : ScenarioRTL.Action
    {
		// 在此列举参数
		OBJECT obj;	// ENTITY
		string text;
		float time;

        float _curTime;
        bool _closeUIWnd = false;
        bool _started = false;


		// 在此初始化参数
		protected override void OnCreate()
		{
			obj = Utility.ToObject(parameters["object"]);
			text = Utility.ToText(missionVars, parameters["text"]);
			time = Utility.ToSingle(missionVars, parameters["time"]);
		}
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
            if (GameUI.Instance != null && GameUI.Instance.mNPCSpeech != null)
            {
                if (!_started)
                {
                    _started = true;
                    if (GameUI.Instance.mNPCSpeech.speechInterpreter.SetObject(obj))
                    {
                        GameUI.Instance.mNPCSpeech.speechInterpreter.SetSpeechContent(text);
                        GameUI.Instance.mNPCSpeech.speechInterpreter.onUIClick += OnSpeechUIClick;
                        GameUI.Instance.mNPCSpeech.Show();
                        if (GameUI.Instance.mNpcDialog.isShow)
                            GameUI.Instance.mNpcDialog.Hide();
                        GameUI.Instance.mNpcDialog.allowShow = false;
                        return false;
                    }
                    else
                    {
                        Debug.LogWarning("Get Object scenario id[" +obj.Id.ToString() + "] error");
                        GameUI.Instance.mNpcDialog.allowShow = true;
                        return true;
                    }
                    
                }
                else
                {
                    if (time > 0.0001f || time < -0.0001f)
                    {
                        _curTime += Time.deltaTime;
                        if (_closeUIWnd || _curTime > time)
                        {
                            GameUI.Instance.mNPCSpeech.Hide();
                            GameUI.Instance.mNPCSpeech.speechInterpreter.onUIClick -= OnSpeechUIClick;
                            GameUI.Instance.mNpcDialog.allowShow = true;
                            return true;
                        }
                        else
                            return false;
                    }
                    else
                    {
                        if (_closeUIWnd)
                        {
                            GameUI.Instance.mNPCSpeech.Hide();
                            GameUI.Instance.mNPCSpeech.speechInterpreter.onUIClick -= OnSpeechUIClick;
                            GameUI.Instance.mNpcDialog.allowShow = true;
                            return true;
                        }
                        else
                            return false;
                    }
                }
                
            }
            return true;
        }
        
        // 恢复动作状态
        public override void RestoreState(BinaryReader r)
        {
            //_closeUIWnd = r.ReadBoolean();
            _curTime = r.ReadSingle();

        }
        // 存储动作状态
        public override void StoreState(BinaryWriter w) 
        {
            //w.Write(_closeUIWnd);
            w.Write(_curTime);
        }

        void OnSpeechUIClick ()
        {
            _closeUIWnd = true;
            GameUI.Instance.mNPCSpeech.speechInterpreter.onUIClick -= OnSpeechUIClick;
        }
    }
}
