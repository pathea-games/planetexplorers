using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class TeammateCtrl : MonoBehaviour
{

    [SerializeField]
    TeammateItemCtrl mColumnPrefab;
    [SerializeField]
    UIGrid mGrid;

    List<PlayerNetwork> mTeammates = new List<PlayerNetwork>();//所有队友

    List<TeammateItemCtrl> mTeammateItemList = new List<TeammateItemCtrl>();

    PeEntity mSelf;

    int mSelfTeamID;

    const int PerColumnCount = 8;

    bool mDoDestroy = false;

    void Awake()
    {
        if (PeGameMgr.IsMulti)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }

    void OnEnable()
    {
        PlayerNetwork.OnTeamChangedEventHandler += RefreshTeammate;
    }

    void OnDisable()
    {
        PlayerNetwork.OnTeamChangedEventHandler -= RefreshTeammate;
    }

	public void RefreshTeammate()
    {
		if (null == PlayerNetwork.mainPlayer)
			return;

		mTeammates.Clear();
		PlayerNetwork.PlayerAction((p) =>
		{
			if (p.Id != PlayerNetwork.mainPlayerId && p.TeamId == PlayerNetwork.mainPlayer.TeamId)
				mTeammates.Add(p);
		});

		DestroyColumn();

		if (mTeammates.Count != 0)
			CreateColumn(mTeammates);
	}

    void CreateColumn(List<PlayerNetwork> playerList)
    {
        if (null == playerList || playerList.Count <= 0)
            return;

        int intColnumn = (int)(playerList.Count / PerColumnCount);
        int remainder = playerList.Count % PerColumnCount;
        
        if (intColnumn > 0)
        {
            List<PlayerNetwork> rangeList = null;
            for (int i = 0; i < intColnumn; i++)
            {
                rangeList = playerList.GetRange(i * PerColumnCount, PerColumnCount);
                InstantiateColumn(rangeList);
            }
        }
        if (remainder > 0)
        {
            List<PlayerNetwork> rangeList = playerList.GetRange(intColnumn * PerColumnCount, remainder);
            InstantiateColumn(rangeList);
        }
    }

    void DestroyColumn()
    {
        for (int i = 0; i < mTeammateItemList.Count; i++)
        {
            Destroy(mTeammateItemList[i].gameObject);
        }
        mTeammateItemList.Clear();
        mDoDestroy = true;
    }

    void InstantiateColumn(List<PlayerNetwork> lis)
    {
        if (null == lis || lis.Count <= 0) return;
        TeammateItemCtrl tic = Instantiate(mColumnPrefab) as TeammateItemCtrl;
        tic.transform.parent = mGrid.transform;
        tic.transform.localPosition = Vector3.zero;
        tic.transform.localScale = Vector3.one;

        tic.SetGrid(lis);
        mTeammateItemList.Add(tic);
    }

    void LateUpdate()
    {
        if (mDoDestroy)
        {
            mDoDestroy = false;
            mGrid.repositionNow = true;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            RefreshTeammate();
    }
}
