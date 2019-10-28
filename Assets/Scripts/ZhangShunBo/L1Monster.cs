using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

[RequireComponent(typeof(Collider))]
public class L1Monster : MonoBehaviour {

    public List<int> monsterProtoID;
    public List<int> dien1monsterID;
    public List<int> dien2monsterID;
    public List<int> dien3monsterID;
    public List<int> dien4monsterID;
    public List<int> dien5monsterID;

    void OnTriggerEnter(Collider target) 
    {
        if (null == target.GetComponentInParent<Pathea.MainPlayerCmpt>())
            return;
        if(PeGameMgr.IsSingle)
        {
            if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip1)
            {
                for (int i = 0; i < dien1monsterID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(dien1monsterID[i], true);
            }
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip2)
            {
                for (int i = 0; i < dien2monsterID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(dien2monsterID[i], true);
            }
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip3)
            {
                for (int i = 0; i < dien3monsterID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(dien3monsterID[i], true);
            }
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip4)
            {
                for (int i = 0; i < dien4monsterID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(dien4monsterID[i], true);
            }
            else if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip5)
            {
                for (int i = 0; i < dien5monsterID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(dien5monsterID[i], true);
            }
            else
            {
                for (int i = 0; i < monsterProtoID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(monsterProtoID[i], true);
            }
        }
        else
        {
            if (PlayerNetwork.mainPlayer._curSceneId == (int)SingleGameStory.StoryScene.DienShip1)
            {
                for (int i = 0; i < dien1monsterID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(dien1monsterID[i], true);
            }
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)SingleGameStory.StoryScene.DienShip2)
            {
                for (int i = 0; i < dien2monsterID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(dien2monsterID[i], true);
            }
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)SingleGameStory.StoryScene.DienShip3)
            {
                for (int i = 0; i < dien3monsterID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(dien3monsterID[i], true);
            }
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)SingleGameStory.StoryScene.DienShip4)
            {
                for (int i = 0; i < dien4monsterID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(dien4monsterID[i], true);
            }
            else if (PlayerNetwork.mainPlayer._curSceneId == (int)SingleGameStory.StoryScene.DienShip5)
            {
                for (int i = 0; i < dien5monsterID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(dien5monsterID[i], true);
            }
            else
            {
                for (int i = 0; i < monsterProtoID.Count; i++)
                    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(monsterProtoID[i], true);
            }
        }
        
        GetComponent<Collider>().enabled = false;
    }
}
