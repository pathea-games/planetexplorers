using UnityEngine;
using ScenarioRTL;
using System.IO;

namespace PeCustom
{
    [Statement("END CHOICE GROUP")]
    public class EndChoiceGroupAction : ScenarioRTL.Action
    {
        // 在此列举参数


        bool _started = true;
        bool _closeUIWnd = false;
        // 在此初始化参数
        protected override void OnCreate()
        {
        
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
            if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null)
            {
                if (_started)
                {
                    if (PeCustomScene.Self.scenario.dialogMgr.EndChooseGroup())
                    {
                        GameUI.Instance.mNPCSpeech.speechInterpreter.SetNpoEntity(CreatureMgr.Instance.mainPlayer);
                        GameUI.Instance.mNPCSpeech.speechInterpreter.SetChoiceCount();
                        GameUI.Instance.mNPCSpeech.speechInterpreter.onChoiceClickForward += OnChioceClick;
                        GameUI.Instance.mNPCSpeech.Show();
                        GameUI.Instance.mNpcDialog.allowShow = false;
                    }

                    _started = false;
                }
                else
                {
                    if (_closeUIWnd)
                    {
                        //GameUI.Instance.mNPCSpeech.speechInterpreter.onChoiceClickForward -= OnChioceClick;
                        //GameUI.Instance.mNpcDialog.allowShow = true;
                        return true;
                    }
                }

                return false;
            }
            return true;
        }

        // 恢复动作状态
        public override void RestoreState(BinaryReader r)
        {
            //_closeUIWnd = r.ReadBoolean();

        }
        // 存储动作状态
        public override void StoreState(BinaryWriter w)
        {
            //w.Write(_closeUIWnd);
        }

        void OnChioceClick(int choice_id)
        {
            _closeUIWnd = true;
            GameUI.Instance.mNPCSpeech.Hide();
            GameUI.Instance.mNPCSpeech.speechInterpreter.onChoiceClickForward -= OnChioceClick;
            GameUI.Instance.mNpcDialog.allowShow = true;
        }
    }
}
