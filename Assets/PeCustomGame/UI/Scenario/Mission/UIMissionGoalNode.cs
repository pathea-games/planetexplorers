using UnityEngine;
using System.Collections.Generic;

public class UIMissionGoalNode : MonoBehaviour
{
    [SerializeField] UILabel    titleLb;
    [SerializeField] UISprite   stateSprite;
    [SerializeField] UISprite   selectedSprite;
    [SerializeField] UIButton deleteBtn;

    [SerializeField] UICheckbox trackCB;

    [SerializeField] UITable    chlidNodeTb;
    [SerializeField] UITweener  tweener;

    [SerializeField] List<GameObject> m_ChildNodes = new List<GameObject>(5);

    [SerializeField] bool expanded = false;

    [HideInInspector] public UIMissionGoalNode parentNode;
    public int index = -1;

    public Color titleColor { get { return titleLb.color; } set { titleLb.color = value; } }

    // Help value
    public int value0 = -1;
    public int value1 = -1;
    public int value2 = -1;

    #region Event
    public delegate void DNotitfy (UIMissionGoalNode node);
    public event DNotitfy onTitleClick;
    public event DNotitfy onDeleteBtnClick;
    public System.Action<int, GameObject> onSetChildNodeContent;

    public delegate void DBoolParamNotity (bool active, UIMissionGoalNode node);
    public event DBoolParamNotity onTrackBoxActive;
    #endregion

    public string title { get { return titleLb.text; } set { titleLb.text = value; } }
    public bool isTracked { get { return trackCB.isChecked; } set { trackCB.isChecked = value; } }

    public bool IsSelected { get { return selectedSprite.enabled;} set { selectedSprite.enabled = value;} }

    public List<GameObject> childNode { get { return m_ChildNodes; } }

    public void PlayTween (bool foward)
    {
        tweener.Play(foward);
        expanded = foward;

        CheckState();

        DetermineStateSprite();
    }


    public void SetContent (string _title, bool _canAbort, bool _canTrack)
    {
        title = _title;

        if (trackCB != null)
            trackCB.gameObject.SetActive(_canTrack);
        if (deleteBtn != null)
            deleteBtn.gameObject.SetActive(_canAbort);
        //stateSprite.gameObject.SetActive(_canExpande);
    }

    public void UpdateChildNode(int count, GameObject prefab)
    {
        UIUtility.UpdateListGos(m_ChildNodes, prefab,
           chlidNodeTb.transform, count, OnSetChildNodeContent, null);

        CheckState();
        chlidNodeTb.repositionNow = true;
    }

    public void RemoveChildeNode (int index)
    {
        if (index < -1 && index > m_ChildNodes.Count)
            return;

        Destroy(m_ChildNodes[index].gameObject);
        m_ChildNodes[index].transform.parent = null;
        m_ChildNodes.RemoveAt(index);

        CheckState();
        chlidNodeTb.repositionNow = true;
    }

    public GameObject GetChildNode (int index)
    {
        return m_ChildNodes[index];
    }

    void OnSetChildNodeContent(int index, GameObject go)
    {

        if (onSetChildNodeContent != null)
        {
            onSetChildNodeContent(index, go);
        }
    }

    void CheckState ()
    {
        stateSprite.gameObject.SetActive(m_ChildNodes.Count != 0);
    }

    void DetermineStateSprite ()
    {
        if (expanded)
            stateSprite.spriteName = "mission_open";
        else
            stateSprite.spriteName = "mission_closed";
    }

    void Awake ()
    {
        CheckState();

        DetermineStateSprite();
    }

    #region UI_EVENT

    void OnTitileClick ()
    {
        expanded = !expanded;
        DetermineStateSprite();

        if (onTitleClick != null)
            onTitleClick(this);

    }

    void OnDeleteBtnClick ()
    {
        if (onDeleteBtnClick != null)
            onDeleteBtnClick(this);
    }

    void OnStateClick ()
    {
        expanded = !expanded;
        if (expanded)
            stateSprite.spriteName = "mission_open";
        else
            stateSprite.spriteName = "mission_closed";
        expanded = !expanded;

    }

    void OnTrackBoxActive (bool active)
    {
        if (onTrackBoxActive != null)
            onTrackBoxActive(active, this);
    }
    #endregion

}
