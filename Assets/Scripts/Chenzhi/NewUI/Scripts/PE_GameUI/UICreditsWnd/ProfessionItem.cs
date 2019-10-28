using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProfessionItem : MonoBehaviour {

    [SerializeField]
    private UILabel m_TitleLabel;
    [SerializeField]
    private UIGrid m_Grid;
    [SerializeField]
    private GameObject m_2NameItemPrefab;
    [SerializeField]
    private GameObject m_4NameItemPrefab;
    [SerializeField]
    public int CellHeight;
    public bool TitleIsNullOrEmpty { get; private set; }

    #region private methods

    private void UpdateTitleName(string titleName)
    {
        if (string.IsNullOrEmpty(titleName))
        {
            this.TitleIsNullOrEmpty = true;
            this.m_TitleLabel.gameObject.SetActive(false);
            Destroy(this.m_TitleLabel.gameObject);
            this.m_Grid.transform.localPosition = Vector3.zero;
            return;
        }
        this.TitleIsNullOrEmpty = false;
        this.m_TitleLabel.text = titleName;
    }

    private void UpdatePeoples(List<ManyPeopleName> manyPeopleList, bool isShow2Name)
    {
        if (null == manyPeopleList || manyPeopleList.Count <= 0)
            return;
        for (int i = 0; i < manyPeopleList.Count; i++)
        {
            ManyPeopleName names = manyPeopleList[i];
            ManyPeopleItem item = GetNewManyPeopleItem(names.NameList.Count, isShow2Name);
            item.name = "ManyPeopleName_" + i.ToString("D4");
            item.UpdateNames(names.NameList.ToArray());
        }
        this.m_Grid.cellHeight = this.CellHeight;
        this.m_Grid.Reposition();
    }

    ManyPeopleItem GetNewManyPeopleItem(int nameCount, bool isShow2Name)
    {
        GameObject go = GameObject.Instantiate(isShow2Name ?this.m_2NameItemPrefab:this.m_4NameItemPrefab) as GameObject;
        go.transform.parent = this.m_Grid.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        return go.GetComponent<ManyPeopleItem>();
    }

    #endregion

    #region public methods

    public void UpdateInfo(ProfessionInfo professionInfo,bool isShow2Name=true)
    {
        this.UpdateTitleName(professionInfo.ProfessionName);
        if (null != professionInfo.ManyPeopleList && professionInfo.ManyPeopleList.Count > 0)
        {
            this.UpdatePeoples(professionInfo.ManyPeopleList, isShow2Name);
        }
    }
    #endregion
}
