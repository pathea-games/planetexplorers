using UnityEngine;
using System.Collections.Generic;
using PeCustom;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;

public class UINpcDialogWndInterpreter : MonoBehaviour
{
    public UINpcDialogWnd npcDialogWnd;

    public int npoId { get; private set; }
    public PeEntity npoEntity { get; private set; }

    public bool refreshQuestsNow = false;

    // Event
    public delegate void DNotify(int world_index, int npo_id, int quest_id);
    public event DNotify onQuestClick;  

    public bool SetNpoEntity(PeEntity npo_entity)
    {
        PeScenarioEntity pse = npo_entity.gameObject.GetComponent<PeScenarioEntity>();
        if (pse != null)
        {
            npoId = pse.spawnPoint.ID;

            npoEntity = npo_entity;

            npcDialogWnd.SetNPCInfo(npoEntity.ExtGetName(), npoEntity.ExtGetFaceIcon());

            refreshQuestsNow = true;
            return true;
        }
        return false;
    }

    public bool SetNpoId(int npo_id)
    {
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

            npcDialogWnd.SetNPCInfo(npoEntity.ExtGetName(), npoEntity.ExtGetFaceIcon());

            refreshQuestsNow = true;

            return true;
        }
        else
        {
            npoId = -1;
            npcDialogWnd.SetNPCInfo("", "Null");
        }

       return false;
    } 

    bool _initialized = false;
    public void Init ()
    {
        if (!_initialized)
        {
            npcDialogWnd.onQuestItemClick += OnQuestItemClick;
            npcDialogWnd.onSetItemContent += OnSetQuestItemContent;
            npcDialogWnd.onBeforeShow += OnBeforeNpcWndShow;
            npcDialogWnd.e_OnHide += OnNpoWndHide;
            PeCustomScene.Self.scenario.dialogMgr.onNpoQuestsChanged += OnNpoQuestsChanged;

            _initialized = true;
        }
    }


    public void Close()
    {
        if (_initialized)
        {
            npcDialogWnd.onQuestItemClick -= OnQuestItemClick;
            npcDialogWnd.onSetItemContent -= OnSetQuestItemContent;
            npcDialogWnd.onBeforeShow -= OnBeforeNpcWndShow;
            npcDialogWnd.e_OnHide -= OnNpoWndHide;
            PeCustomScene.Self.scenario.dialogMgr.onNpoQuestsChanged -= OnNpoQuestsChanged;

            _initialized = false;
        }
    }

    void Update ()
    {
        if (CreatureMgr.Instance != null && npcDialogWnd.isShow && npoEntity != null)
        {
            float sqr_dis = Vector3.SqrMagnitude(CreatureMgr.Instance.mainPlayer.peTrans.position - npoEntity.ExtGetPos());
            if (sqr_dis >= 64)
            {
                npcDialogWnd.Hide();
            }
        }
        
    }

    void LateUpdate()
    {
        if (!_initialized)
            return;

        if (refreshQuestsNow)
        {
            RefreshQuests();
            refreshQuestsNow = false;
        }
    }

    IList<string> _dialogs = null;
    public void RefreshQuests()
    {
        if (!_initialized)
            return;

       _dialogs = PeCustomScene.Self.scenario.dialogMgr.GetQuests(CustomGameData.Mgr.Instance.curGameData.WorldIndex, npoId);
        if (_dialogs != null)
        {
            npcDialogWnd.UpdateTable(_dialogs.Count);
        }
        else
        {
            npcDialogWnd.UpdateTable(0);
        }
    }

    #region CALL_BACK

    void OnBeforeNpcWndShow ()
    {
        if (refreshQuestsNow)
        {
            RefreshQuests();
            refreshQuestsNow = false;
        }

        PeScenarioUtility.SetNpoReqDialogue(npoEntity);
    }

	void OnNpoWndHide (UIBaseWidget widget = null)
    {
        PeScenarioUtility.RemoveNpoReq(npoEntity, EReqType.Dialogue);
    }

    void OnNpoQuestsChanged(int world_index, int npo_id)
    {
        if (world_index != CustomGameData.Mgr.Instance.curGameData.WorldIndex
            || npoId != npo_id)
            return;

        refreshQuestsNow = true;
    }

    void OnSetQuestItemContent (UINpcQuestItem item)
    {
        if (_dialogs == null)
        {
            Debug.LogWarning("The giving dialog is null");
            return;
        }

        if (item.index < 0 || item.index >= _dialogs.Count)
        {
            Debug.LogWarning("The index is out of range");
            return;
        }

        item.test = _dialogs[item.index];
    }

    void OnQuestItemClick (UINpcQuestItem item)
    {
        if (item.index < 0 || item.index >= _dialogs.Count)
        {
            Debug.LogWarning("The index is out of range");
            return;
        }

        int world_index = CustomGameData.Mgr.Instance.curGameData.WorldIndex;
        int quest_id = PeCustomScene.Self.scenario.dialogMgr.GetQuestId(world_index, npoId, item.index);

        if (quest_id != -1)
        {
            if (onQuestClick != null)
            {
                onQuestClick(world_index, npoId, quest_id);
            }

            Debug.Log("Click the Quest id [" + quest_id.ToString() + "]");
        }
        else
        {
            Debug.LogWarning("cant find the quest id");
        }
    }

    #endregion
}
