using UnityEngine;
using System.Collections;

public class SPPointBoss : SPPoint 
{
    void Start()
    {
//        if (GameGui_N.Instance != null)
//            GameUI.Instance.mLimitWorldMapGui.AddBoss(this);
    }

    new public void OnDestroy()
    {
        base.OnDestroy();

//        if(GameGui_N.Instance != null)
//			GameUI.Instance.mLimitWorldMapGui.RemoveBoss(this);
    }
}
