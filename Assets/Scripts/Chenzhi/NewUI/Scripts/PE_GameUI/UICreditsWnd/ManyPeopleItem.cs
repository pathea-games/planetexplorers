using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ManyPeopleItem : MonoBehaviour {
    private enum SortType
    {
        None,
        LongToStart_End, //长的放两边
        longToCenter  //长的放中间
    }

    [SerializeField]
    private List<UILabel> m_NameLabels;
    [SerializeField]
    private SortType m_SortType= SortType.LongToStart_End;

    public int CurLabelCount { get { return null == m_NameLabels ? 0 : m_NameLabels.Count; } }
    

    #region public methods

    public void UpdateNames(params string[] nameArray)
    {
        UpdateNames(nameArray.ToList());
    }

    public void UpdateNames(List<string> nameArray)
    {
        if (null != nameArray && nameArray.Count > 0)
        {
            if (m_SortType != SortType.None)
            {
                nameArray = TrySortByNameLength(nameArray);
            }
            for (int i = 0; i < m_NameLabels.Count; i++)
            {
                if (i < nameArray.Count)
                {
                    this.m_NameLabels[i].text = nameArray[i];
                    this.m_NameLabels[i].MakePixelPerfect();
                }
                else
                {
                    Destroy(this.m_NameLabels[i].gameObject);
                }
            }
        }
    }
    #endregion

    #region pivate methods

    private List<string> TrySortByNameLength(List<string> nameArray)
    {
        switch (m_SortType)
        {
            case SortType.None:
                break;
            case SortType.LongToStart_End:
                if (nameArray.Count >= 3)
                {
                    //lz-2016.08.16 把最长的两个分别放在前面和后面
                    nameArray = nameArray.OrderByDescending(a => a.Length).ToList();
                    string tempName = nameArray[1];
                    nameArray[1] = nameArray[nameArray.Count - 1];
                    nameArray[nameArray.Count - 1] = tempName;
                }
                break;
            case SortType.longToCenter:
                if (nameArray.Count >= 3)
                {
                    //lz-2016.10.22 把最长的两个分别放在中间
                    int centerIndex = (int)(nameArray.Count * 0.5f);
                    int maxLength = 0;
                    int maxIndex = -1;
                    for (int i = 0; i < nameArray.Count; i++)
                    {
                        if (nameArray[i].Length > maxLength)
                        {
                            maxIndex = i;
                            maxLength = nameArray[i].Length;
                        }
                    }
                    if (maxIndex != -1)
                    {
                        string tempName = nameArray[maxIndex];
                        nameArray[maxIndex] = nameArray[centerIndex];
                        nameArray[centerIndex] = tempName;
                    }
                }
                break;
        }
        return nameArray;
    }
    #endregion
}
