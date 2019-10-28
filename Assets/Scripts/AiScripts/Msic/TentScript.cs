using UnityEngine;
using System.Collections;

public class TentScript : MonoBehaviour 
{
    public Vector3 m_Pos;
    public string m_NpcName = "";

    void Start()
    {
        m_Pos = transform.position + Vector3.up * 0.5f;
        if (StroyManager.Instance == null)
            return;

        if (StroyManager.Instance.m_TentList.ContainsKey(m_Pos))
            return;

        StroyManager.Instance.m_TentList.Add(m_Pos, this);
    }

    public void CmdToSleep(/*AiNpcObject npc*/)
    {
        //npc.SetCmdMoveTo(m_Pos, 7, npc.walkSpeed);
        //npc.arriveDestination = OnSleepToDest;
    }

    private void OnSleepToDest(/*AiNpcObject npc*/)
	{
        //Debug.Log("The NPC [" + npc.NpcName + "] is starting to Sleep.");
        //npc.SetPos(m_Pos);
        //npc.Sleep(true);
        //npc.arriveDestination = null;
	}

    
}
