using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DienManager : MonoBehaviour {

    public static List<Transform> doors;
    public static List<List<Transform>> mulDoors = new List<List<Transform>>();
    public static bool doorsCanTrigger = true;

	public Light directionLight;


	void Start () {
        doors = new List<Transform>(this.GetComponentsInChildren<Transform>()).FindAll(delegate(Transform trans)
        {
            string name = trans.gameObject.name;
            if (name.Length < 24)
                return false;
            if (name.Substring(0, 22) == "scene_Dien_viyus_ship_") 
            {
                int num;
                if (IsNumberic(name.Substring(22, 2), out num))
                {
                    if (num >= 22 && num <= 26)
                        return true;
                }

            }
            return false;
        });
        List<Transform> temp = new List<Transform>(this.GetComponentsInChildren<Transform>()).FindAll(delegate (Transform trans)
        {
            string name = trans.gameObject.name;
            if (name.Length < 24)
                return false;
            if (name.Substring(0, 22) == "scene_Dien_viyus_ship_")
            {
                int num;
                if (IsNumberic(name.Substring(22, 2), out num))
                {
                    if (num >= 22 && num <= 26)
                        return true;
                }

            }
            return false;
        });
        mulDoors.Add(temp);
        doors.Reverse();
	}

    bool IsNumberic(string s,out int result)
    {
        result = -1;
        try
        {
            result = Convert.ToInt32(s);
            return true;
        }
        catch
        {
            return false;
        }
    }
	
	void Update () {
        CheckOpen();
	}

    void CheckOpen() 
    {
        if (MissionManager.Instance == null)
            return;
        if(Pathea.PeGameMgr.IsSingle)
        {
            if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.DienShip0)
            {
                if (!MissionManager.Instance.HadCompleteMission(640))
                    return;
            }
            else
            {
                if (!MissionManager.Instance.HasMission(906) && !MissionManager.Instance.HadCompleteMission(906))
                    return;
            }
        }
        else
        {
            if (PlayerNetwork.mainPlayer == null)
                return;
            if(PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.DienShip0)
            {
                if (!MissionManager.Instance.HadCompleteMission(640))
                    return;
            }
            else
            {
                if (!MissionManager.Instance.HasMission(906) && !MissionManager.Instance.HadCompleteMission(906))
                    return;
            }
        }

        if (Pathea.PeCreature.Instance.mainPlayer == null)
            return;

        if (doorsCanTrigger)
        {
            if(Pathea.PeGameMgr.IsSingle)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (Vector3.Distance(Pathea.PeCreature.Instance.mainPlayer.position, doors[i].position) <= 3)
                        DoorOpen(doors[i]);
                }
            }    
            else
            {
                foreach(var iter in mulDoors)
                {
                    foreach(var iter1 in iter)
                    {
                        if (iter1 != null && Vector3.Distance(Pathea.PeCreature.Instance.mainPlayer.position, iter1.position) <= 3)
                            DoorOpen(iter1);
                    }
                }
            }        
        }
        else
        {
            if(Pathea.PeGameMgr.IsSingle)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (Vector3.Distance(Pathea.PeCreature.Instance.mainPlayer.position, doors[i].position) <= 3
                        && Pathea.PeCreature.Instance.mainPlayer.position.z < doors[i].position.z)
                        DoorOpen(doors[i]);
                }
            }   
            else
            {
                foreach (var iter in mulDoors)
                {
                    foreach (var iter1 in iter)
                    {
                        if (iter1 != null && Vector3.Distance(Pathea.PeCreature.Instance.mainPlayer.position, iter1.position) <= 3
                        && Pathea.PeCreature.Instance.mainPlayer.position.z < iter1.position.z)
                            DoorOpen(iter1);
                    }
                }
            }         
        }
    }
    public static void DoorOpen(Transform door) 
    {
        if (door == null)
            return;
        door.GetComponent<Animator>().SetBool("IsOpen", true);
        door.GetComponent<BoxCollider>().enabled = false;
    }
    public static void DoorClose(Transform door)
    {
        if (door == null)
            return;
        door.GetComponent<Animator>().SetBool("IsOpen", false);
        door.GetComponent<BoxCollider>().enabled = true;
    }
}
