using UnityEngine;
using System.Collections;

public class ShipExit : MonoBehaviour
{

    public int doorId;
    bool isShow = false;

    void OnTriggerEnter(Collider target)
    {
        if (null == target.GetComponentInParent<Pathea.MainPlayerCmpt>())
            return;
        if (isShow == true)
            return;

        isShow = true;
        MessageBox_N.ShowYNBox(PELocalization.GetString(82209002), SceneTranslate, SetFalse);
    }

    public void SceneTranslate()
    {
        MissionManager.Instance.yirdName = "main";
        if (Pathea.PeGameMgr.IsMultiStory)
        {
            if (PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.DienShip0)
                MissionManager.Instance.transPoint = new Vector3(14819.54f, 106.1666f, 8347.545f);
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.DienShip1)
				MissionManager.Instance.transPoint = new Vector3(16545f, 219f, 10748f);
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.DienShip2)
                MissionManager.Instance.transPoint = new Vector3(2890.597f, 385.7521f, 9852.657f);
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.DienShip3)
                MissionManager.Instance.transPoint = new Vector3(13863.91f, 173.9639f, 15278.54f);
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.DienShip4)
                MissionManager.Instance.transPoint = new Vector3(12562.67f, 643.8412f, 13587.89f);
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.DienShip5)
                MissionManager.Instance.transPoint = new Vector3(7844.74f, 455.2281f, 14668.19f);
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.L1Ship)
                MissionManager.Instance.transPoint = new Vector3(9684.722f, 368.9954f, 12795.33f);
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.PajaShip)
                MissionManager.Instance.transPoint = new Vector3(1570, 118, 8024);
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.LaunchCenter)
            {
                if (doorId == 1)
                    MissionManager.Instance.transPoint = new Vector3(1674, 237, 10365);
                else if (doorId == 2)
                    MissionManager.Instance.transPoint = new Vector3(1886, 267, 10392);
            }
        }
        else
        {
            if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip0)
                MissionManager.Instance.transPoint = new Vector3(14819.54f, 106.1666f, 8347.545f);
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip1)
				MissionManager.Instance.transPoint = new Vector3(16545f, 219f, 10748f);
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip2)
                MissionManager.Instance.transPoint = new Vector3(2890.597f, 385.7521f, 9852.657f);
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip3)
                MissionManager.Instance.transPoint = new Vector3(13863.91f, 173.9639f, 15278.54f);
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip4)
                MissionManager.Instance.transPoint = new Vector3(12562.67f, 643.8412f, 13587.89f);
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip5)
                MissionManager.Instance.transPoint = new Vector3(7844.74f, 455.2281f, 14668.19f);
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.L1Ship)
                MissionManager.Instance.transPoint = new Vector3(9679.52f, 371.66f, 12795.33f);
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.PajaShip)
                MissionManager.Instance.transPoint = new Vector3(1570, 118, 8024);
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.LaunchCenter)
            {
                if (doorId == 1)
                    MissionManager.Instance.transPoint = new Vector3(1674, 237, 10365);
                else if (doorId == 2)
                    MissionManager.Instance.transPoint = new Vector3(1886, 267, 10392);
            }
        }

        MissionManager.Instance.SceneTranslate();
        SetFalse();
    }

    public void SetFalse() { isShow = false; }
}
