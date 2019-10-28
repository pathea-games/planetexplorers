using UnityEngine;
using System.Collections;

public class RadoRender : MonoBehaviour {

    void Awake() 
    {
        if (!MissionManager.Instance.m_PlayerMission.HasMission(550))
            GameObject.Destroy(this.gameObject);
    }
}
