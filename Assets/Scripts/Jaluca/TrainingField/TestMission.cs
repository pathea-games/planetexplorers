using UnityEngine;
using System.Collections;
using Pathea;
using TrainingScene;
public class TestMission : MonoBehaviour {
	GUIStyle archiveBtnStyle;
	bool gather;
	bool cut;
	bool replicate;
	bool dig;
	bool build;
	bool move;
    bool newmove;

	void InitStyle()
	{
		archiveBtnStyle = new GUIStyle();
		archiveBtnStyle.stretchHeight = true;
		archiveBtnStyle.stretchWidth = true;
	}
	
	void Awake()
	{
		InitStyle();
	}

	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(300, 0, 140, Screen.height));
		GetMission();
		GUILayout.EndArea();
		
		GUILayout.BeginArea(new Rect(450, 0, 170, Screen.height));
		CompleteMission();
		GUILayout.EndArea();

		GatherHerb();
	}
	
	void GetMission()
	{
		if (GUILayout.Button("GetGatherMission") && !gather)
		{
			HoloherbTask.Instance.InitScene();
			gather = true;
		}
		
		if (GUILayout.Button("GetCutMission") && !cut)
		{
			HolotreeTask.Instance.InitScene();
			cut = true;
		}
		
		if (GUILayout.Button("GetCopyMission") && !replicate)
		{
			replicate = true;
		}
		
		if (GUILayout.Button("GetDigMission") && !dig)
		{
			TerrainDigTask.Instance.InitScene();
			dig = true;
		}
		
		if (GUILayout.Button("GetBuildMission") && !build)
		{
			EmitlineTask.Instance.InitScene();
			build = true;
		}
		if (GUILayout.Button("GetMoveMission") && !move)
		{
			MoveTask.Instance.InitScene();
			move = true;
		}
        if (GUILayout.Button("GetNewMoveMission") && !newmove)
        {
            NewMoveTask.Instance.InitMoveScene();
            newmove = true;
        }
	}
	
	void CompleteMission()
	{
		if (GUILayout.Button("CompleteGatherMission") && gather)
		{
			HoloherbTask.Instance.DestroyScene();
			gather = false;
		}
		
		if (GUILayout.Button("CompleteCutMission") && cut)
		{
			HolotreeTask.Instance.DestroyScene();
			cut = false;
		}
		
		if (GUILayout.Button("CompleteCopyMission") && replicate)
		{
			replicate = false;
		}
		
		if (GUILayout.Button("CompleteDigMission") && dig)
		{
			//TerrainDigTask.Instance.OnItemAdd();
            TerrainDigTask.Instance.DestroyScene();

			dig = false;
		}
		
		if (GUILayout.Button("CompleteBuildMission") && build)
		{
			EmitlineTask.Instance.DestroyScene();
			build = false;
		}
		if (GUILayout.Button("CompleteMoveMission") && move)
		{
			MoveTask.Instance.DestroyScene();
			move = false;
		}
        if (GUILayout.Button("CompleteNewMoveMission") && newmove)
        {
            NewMoveTask.Instance.DestroyMoveScene();
            newmove = false;
        }
	}

	void GatherHerb()
	{
		GUILayout.BeginArea(new Rect(630, 0, 50, 40));
		if (GUILayout.Button("herb1") && gather)
		{
			HoloherbTask.Instance.SubHerb(HoloherbTask.Instance.transform.FindChild("holoherbs").FindChild("offset1").GetComponent<HoloherbAppearance>());
		}
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(685, 0, 50, 40));
		if (GUILayout.Button("herb2") && gather)
		{
			HoloherbTask.Instance.SubHerb(HoloherbTask.Instance.transform.FindChild("holoherbs").FindChild("offset2").GetComponent<HoloherbAppearance>());
		}
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(740, 0, 50, 40));
		if (GUILayout.Button("herb3") && gather)
		{
			HoloherbTask.Instance.SubHerb(HoloherbTask.Instance.transform.FindChild("holoherbs").FindChild("offset3").GetComponent<HoloherbAppearance>());
		}
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(795, 0, 50, 40));
		if (GUILayout.Button("herb4") && gather)
		{
			HoloherbTask.Instance.SubHerb(HoloherbTask.Instance.transform.FindChild("holoherbs").FindChild("offset4").GetComponent<HoloherbAppearance>());
		}
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(850, 0, 50, 40));
		if (GUILayout.Button("herb5") && gather)
		{
			HoloherbTask.Instance.SubHerb(HoloherbTask.Instance.transform.FindChild("holoherbs").FindChild("offset5").GetComponent<HoloherbAppearance>());
		}
		GUILayout.EndArea();
	}
}
