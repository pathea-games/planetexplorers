using UnityEngine;
using System.Collections;

public class OpenL1 : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (MissionManager.Instance == null)
            return;
        if (MissionManager.Instance.m_PlayerMission.HadCompleteMission(606))
            gameObject.SetActive(false);
	}
}
