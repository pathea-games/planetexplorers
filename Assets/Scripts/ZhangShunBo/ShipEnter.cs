using UnityEngine;
using System.Collections;

public class ShipEnter : MonoBehaviour {

    void OnTriggerEnter(Collider o) 
    {
		if (Pathea.PeGameMgr.IsCustom)
			return;
        if (o.gameObject.layer != Pathea.Layer.Player)
            return;
        Pathea.PeEntity enti = o.gameObject.GetComponentInParent<Pathea.PeEntity>();
        if (enti == null || enti != Pathea.PeCreature.Instance.mainPlayer)
            return;
        
        if (this.transform.parent.gameObject.name.Substring(0, 24) == "scene_Dien_viyus_ship_on")
        {
            Vector3 v = Pathea.PeCreature.Instance.mainPlayer.position;
			if (Vector3.Distance(v, new Vector3(16545.25f, 213.93f, 10645.7f)) < 150)
            {
				MissionManager.Instance.transPoint = new Vector3(16545.25f, 23f, 10548f);
                MissionManager.Instance.yirdName = "DienShip1";
            }
            else if (Vector3.Distance(v, new Vector3(2876f, 365.6f, 9750.3f)) < 150)
            {
                MissionManager.Instance.transPoint = new Vector3(2876f, 283.6f, 9652.3f);
                MissionManager.Instance.yirdName = "DienShip2";
            }
            else if (Vector3.Distance(v, new Vector3(13765.5f, 175.7f, 15242.7f)) < 150)
            {
                MissionManager.Instance.transPoint = new Vector3(13765.5f, 93.7f, 15144.7f);
                MissionManager.Instance.yirdName = "DienShip3";
            }
            else if (Vector3.Distance(v, new Vector3(12547.7f, 623.7f, 13485.5f)) < 150)
            {
                MissionManager.Instance.transPoint = new Vector3(12547.7f, 541.7f, 13387.5f);
                MissionManager.Instance.yirdName = "DienShip4";
            }
            else if (Vector3.Distance(v, new Vector3(7750.4f, 449.7f, 14712.8f)) < 150)
            {
                MissionManager.Instance.transPoint = new Vector3(7750.4f, 367.7f, 14614.8f);
                MissionManager.Instance.yirdName = "DienShip5";
            }
            else
            {
                MissionManager.Instance.transPoint = new Vector3(14798.09f, 20.98818f, 8246.396f);
                MissionManager.Instance.yirdName = "DienShip0";
            }
            MissionManager.Instance.SceneTranslate();
        }
        else if (this.transform.parent.gameObject.name.Substring(0,24) == "scene_Epiphany_L1Outside")
        {
            if (!MissionManager.Instance.HadCompleteMission(607))
                return;
            if (MissionManager.Instance.HadCompleteMission(617) && !MissionManager.Instance.HadCompleteMission(618))
                return;
            MissionManager.Instance.transPoint = new Vector3(9649.354f, 90.488f, 12744.77f);
            MissionManager.Instance.yirdName = "L1Ship";
            MissionManager.Instance.SceneTranslate();
        }
        else if (this.transform.parent.gameObject.name.Substring(0,27) == "scene_paja_port_shipoutside")
        {
            if (!MissionManager.Instance.HadCompleteMission(800))
                return;
            if(Pathea.PeGameMgr.IsMulti)
                MissionManager.Instance.transPoint = new Vector3(1593.278f, 149.051f - 500f, 8021.335f);
            else
                MissionManager.Instance.transPoint = new Vector3(1593.278f, 149.051f, 8021.335f);
            MissionManager.Instance.yirdName = "PajaShip";
            MissionManager.Instance.SceneTranslate();
        }
        else if (this.transform.parent.gameObject.name.Substring(0,26) == "scene_paja_launch_center03")
        {
            if (Pathea.PeGameMgr.IsMulti)
                MissionManager.Instance.transPoint = new Vector3(1607, 158-500, 10411);
            else
                MissionManager.Instance.transPoint = new Vector3(1607, 158, 10411);
            MissionManager.Instance.yirdName = "LaunchCenter";
            MissionManager.Instance.SceneTranslate();
        }
        else if (this.transform.parent.gameObject.name.Substring(0,26) == "scene_paja_launch_center01")
        {
            if (Pathea.PeGameMgr.IsMulti)
                MissionManager.Instance.transPoint = new Vector3(2019,146-500,10402);
            else
                MissionManager.Instance.transPoint = new Vector3(2019, 146, 10402);
            MissionManager.Instance.yirdName = "LaunchCenter";
            MissionManager.Instance.SceneTranslate();
        }
    }
}
