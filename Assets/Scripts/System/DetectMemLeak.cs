using UnityEngine;
using System.Collections;

public class DetectMemLeak : MonoBehaviour {
	int nObjs;
	int nTexs;
	int nAudios;
	int nMats;
	int nCompos;
	int nGObjs;
	int nMeshs;
	int nGrassMeshs;
	int nTreeMeshs;
	int nOclMeshs;
	int nChnkGos;
	
	float lastUpdateTime = 0;
	
	void UpdateStatistics(){
		nObjs = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object)).Length;
		nTexs = Resources.FindObjectsOfTypeAll(typeof(Texture)).Length;
		nAudios = Resources.FindObjectsOfTypeAll(typeof(AudioClip)).Length;
		nMats = Resources.FindObjectsOfTypeAll(typeof(Material)).Length;
		nCompos = Resources.FindObjectsOfTypeAll(typeof(Component)).Length;
		nGObjs = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Length;
		Object[] meshs = Resources.FindObjectsOfTypeAll(typeof(Mesh));
		nMeshs = meshs.Length;
		nGrassMeshs = 0;
		nTreeMeshs = 0;
		nOclMeshs = 0;
		foreach(Object obj in meshs)
		{
			if(obj.name.Equals("ocl_mesh"))				nOclMeshs++;
			if(obj.name.Contains("tree"))				nTreeMeshs++;
			else
			if(obj.name.Contains("grass"))				nGrassMeshs++;
		}
        nChnkGos = Resources.FindObjectsOfTypeAll(typeof(VFVoxelChunkGo)).Length;
	}
    void OnGUI () {
		if(Time.time > lastUpdateTime+1)
		{
			UpdateStatistics();
			lastUpdateTime = Time.time;
		}
		GUI.color = new Color(1,0,0);
		GUILayout.BeginArea(new Rect(Screen.width - 256, 0, Screen.width, 256));
        GUILayout.Label("All-----------" + nObjs);
        GUILayout.Label("Textures------" + nTexs);
        GUILayout.Label("AudioClips----" + nAudios);
		GUILayout.Label("Materials-----" + nMats);
		GUILayout.Label("Components----" + nCompos);
		GUILayout.Label("GameObjects---" + nGObjs);
        GUILayout.Label("Meshes--------" + nMeshs + "("+nOclMeshs+"/"+nGrassMeshs+"/"+nTreeMeshs+"/"+(nMeshs-nGrassMeshs-nTreeMeshs-nOclMeshs)+")");
        GUILayout.Label("chks GObj-----" + nChnkGos);
		GUILayout.EndArea();
    }
}
