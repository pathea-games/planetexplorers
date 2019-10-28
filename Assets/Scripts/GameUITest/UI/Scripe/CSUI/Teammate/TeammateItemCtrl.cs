using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeammateItemCtrl : MonoBehaviour
{

    //List<PlayerNetwork> mPlayerList;

    [SerializeField]
    UIGrid mGrid;
    [SerializeField]
    TeammateGrid mPrefab;

    public void SetGrid(List<PlayerNetwork> _lis)
    {
        //mPlayerList = _lis;
        CreatGridList(_lis);
    }

    void CreatGridList(List<PlayerNetwork> _lis)
    {
        for (int i = 0; i < _lis.Count; i++)
        {
            InstantiateGrid(_lis[i]);
        }
        mGrid.repositionNow = true;
    }

    void InstantiateGrid(PlayerNetwork _pn)
    {
        TeammateGrid item = Instantiate(mPrefab) as TeammateGrid;
        item.transform.parent = mGrid.transform;
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;

        item.SetInfo(_pn);
    }

}
