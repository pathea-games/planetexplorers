using UnityEngine;
using System.Collections.Generic;
using PeCustom;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;


public class UINpcSpeechBoxInterpreter : MonoBehaviour
{
    // TODO: Npc Speech Box 解释器
    public UINpcSpeechBox npcSpeechBox;

    public int npoId { get; private set;}
    public PeEntity npoEntity { get; private set;}

    // Event
    public delegate void DNotifyParmInt(int choice);
    public event DNotifyParmInt onChoiceClickForward;
    public event DNotifyParmInt onChoiceClick; 

    public delegate void DNotityParmVoid();
    public event DNotityParmVoid onUIClick;

    public void SetSpeechContent(string text)
    {
        if (!_initialized)
            return;

        npcSpeechBox.SetContent(text);
    }


    public void SetChoiceCount()
    {
        if (!_initialized)
            return;

        _choices = PeCustomScene.Self.scenario.dialogMgr.GetChoices();
        npcSpeechBox.SetContent(_choices.Count);
    }

    public bool SetNpoEntity(PeEntity entity)
    {
        if (!_initialized)
            return false;

        PeScenarioEntity pse = entity.gameObject.GetComponent<PeScenarioEntity>();
        if (pse != null)
        {
            npoId = pse.spawnPoint.ID;
            npoEntity = entity;

            npcSpeechBox.SetNpcInfo(npoEntity.ExtGetName(), npoEntity.ExtGetFaceIconBig());
            return true;
        }
        else
        {
            if (entity == CreatureMgr.Instance.mainPlayer)
            {
                npoId = -1;
                npoEntity = entity;
                BiologyViewCmpt viewCmpt = npoEntity.biologyViewCmpt;
                Texture2D big_head = PeViewStudio.TakePhoto(viewCmpt, 150, 150, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);
                npcSpeechBox.SetNpcInfo(npoEntity.ExtGetName(), big_head);
            }
        }

        return false;
    }

    public bool SetNpoId(int npo_id)
    {
        if (!_initialized)
            return false;

        SpawnDataSource sds = PeCustomScene.Self.spawnData;
        if (sds.ContainMonster(npo_id))
        {
            MonsterSpawnPoint msp = sds.GetMonster(npo_id);
            if (msp.agent != null && msp.agent.entity != null)
                npoEntity = msp.agent.entity;
        }
        else if (sds.ContainNpc(npo_id))
        {
            NPCSpawnPoint nsp = sds.GetNpc(npo_id);
            if (nsp.agent != null && nsp.agent.entity != null)
                npoEntity = nsp.agent.entity;
        }

        if (npoEntity != null)
        {
            npoId = npo_id;

            npcSpeechBox.SetNpcInfo(npoEntity.ExtGetName(), npoEntity.ExtGetFaceIconBig());

            return true;
        }
        else
        {
            npoId = -1;
            npcSpeechBox.SetNpcInfo("", "Null");
        }

        return false;
    }

    public bool SetObject(OBJECT obj)
    {
        if (!_initialized)
            return false;

        npoEntity = PeScenarioUtility.GetEntity(obj);

        if (npoEntity != null)
        {
            npoId = obj.Id;

            if (npoEntity == CreatureMgr.Instance.mainPlayer)
            {
                BiologyViewCmpt viewCmpt = npoEntity.biologyViewCmpt;
                Texture2D big_head = PeViewStudio.TakePhoto(viewCmpt, 150, 150, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);
                npcSpeechBox.SetNpcInfo(npoEntity.ExtGetName(), big_head);
            }
            else
                npcSpeechBox.SetNpcInfo(npoEntity.ExtGetName(), npoEntity.ExtGetFaceIconBig());

            return true;
        }
        else
        {
            npoId = -1;
            npcSpeechBox.SetNpcInfo("", "Null");
        }

        return false;
    }



    bool _initialized = false;
    public void Init ()
    {
        if (!_initialized)
        {
            npcSpeechBox.onQuestItemClick += OnQuestItemClick;
            npcSpeechBox.onSetItemContent += OnSetQuestItemContent;
            npcSpeechBox.onUIClick += OnUIClick;
            PeCustomScene.Self.scenario.dialogMgr.onChoiceChanged += OnChoiceChanged;
            _initialized = true;
        }
    }

    public void Close()
    {
        if (_initialized)
        {
            npcSpeechBox.onQuestItemClick -= OnQuestItemClick;
            npcSpeechBox.onSetItemContent -= OnSetQuestItemContent;
            npcSpeechBox.onUIClick -= OnUIClick;
            PeCustomScene.Self.scenario.dialogMgr.onChoiceChanged -= OnChoiceChanged;
            _initialized = false;
        }
    }



    #region CALL_BACK

    void OnChoiceChanged()
    {
        if (npcSpeechBox.IsChoice)
            SetChoiceCount();
    }

    IList<string> _choices = null;
    void OnSetQuestItemContent(UINpcQuestItem item)
    {
        if (_choices == null)
        {
            Debug.LogWarning("The giving dialog is null");
            return;
        }

        if (item.index < 0 || item.index >= _choices.Count)
        {
            Debug.LogWarning("The index is out of range");
            return;
        }

        item.test = _choices[item.index];
    }

    void OnQuestItemClick(UINpcQuestItem item)
    {
        if (item.index < 0 || item.index >= _choices.Count)
        {
            Debug.LogWarning("The index is out of range");
            return;
        }

        int choice_id = PeCustomScene.Self.scenario.dialogMgr.GetChoiceId(item.index);

        if (choice_id != -1)
        {
            if (onChoiceClickForward != null)
                onChoiceClickForward(choice_id);

            if (onChoiceClick != null)
                onChoiceClick(choice_id);

            Debug.Log("Click the choice id [" + choice_id.ToString() + "]");
        }
        else
        {
            Debug.LogWarning("cant find the quest id");
        }
    }

    void OnUIClick()
    {
        if (onUIClick != null)
            onUIClick();
    }
    #endregion
}
