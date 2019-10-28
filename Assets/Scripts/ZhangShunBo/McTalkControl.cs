using UnityEngine;
using System.Collections;

public class McTalkControl : MonoBehaviour {

    Transform[] trans;
	void Start () {
        trans = GetComponentsInChildren<Transform>(true);
	}
	
	// Update is called once per frame
	void Update () {
        if (MissionManager.Instance == null)
            return;
        if (MissionManager.Instance.HadCompleteMission(948))
        {
            trans[3].gameObject.SetActive(true);
            trans[4].gameObject.SetActive(true);
        }
        if (MissionManager.Instance.HadCompleteMission(959))
        {
            trans[5].gameObject.SetActive(true);
            trans[6].gameObject.SetActive(true);
        }
        if (MissionManager.Instance.HadCompleteMission(962))
        {
            trans[7].gameObject.SetActive(true);
            trans[8].gameObject.SetActive(true);
            trans[9].gameObject.SetActive(true);
            trans[10].gameObject.SetActive(true);
        }
        if (MissionManager.Instance.HadCompleteMission(969))
        {
            trans[11].gameObject.SetActive(true);
            trans[12].gameObject.SetActive(true);
        }
        GameObject.Destroy(this);
	}
}
