/* Written for "Dawn of the Tyrant" by SixTimesNothing 
/* Please visit www.sixtimesnothing.com to learn more
/*
/* Note: This code is being released under the Artistic License 2.0
/* Refer to the readme.txt or visit http://www.perlfoundation.org/artistic_license_2_0
/* Basically, you can use this for anything you want but if you plan to change
/* it or redistribute it, you should read the license
*/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AttachedRiverScript))]

public class AttachedRiverEditor : Editor 
{
	public AttachedRiverScript riverScript;
	
	void Awake ()
	{
		riverScript = (AttachedRiverScript) target as AttachedRiverScript;
	}
	
	bool DrawRiverNodes(int i, int max)
	{
		bool bDirty = false;
		EditorGUILayout.BeginHorizontal();
			if(i < max)
			{
				RiverNodeObject curNode = riverScript.nodeObjects[i];
				Vector4 posw = new Vector4(curNode.position.x, curNode.position.y, curNode.position.z, curNode.width);
				posw = EditorGUILayout.Vector4Field(""+i+":", posw);
				curNode.position = new Vector3(posw.x, posw.y, posw.z);
				curNode.width = posw.w;		
				if(GUILayout.Button("-"))
				{
					riverScript.nodeObjects.Remove(curNode);
					bDirty = true;
				}			
			}
	
			if(GUILayout.Button("+"))
			{
				RiverNodeObject newNode = new RiverNodeObject();
				if(riverScript.nodeObjects == null){
					riverScript.nodeObjects = new List<RiverNodeObject>();
					riverScript.curRiverNodeToPosite = 0;
				}else if(i >= max){
					newNode.position = riverScript.nodeObjects[max-1].position;
					newNode.width = riverScript.nodeObjects[max-1].width;
				}else{
					newNode.position = riverScript.nodeObjects[i].position;
					newNode.width = riverScript.nodeObjects[i].width;
				}
				riverScript.nodeObjects.Insert(i, newNode);
				riverScript.curRiverNodeToPosite = i;
				bDirty = true;
			}
		EditorGUILayout.EndHorizontal();
		return bDirty;
	}

	void OnSceneGUI() 
	{
		Event currentEvent = Event.current;
		
		if (riverScript.nodeObjects != null && riverScript.nodeObjects.Count != 0 && !riverScript.finalized) 
		{
			int n = riverScript.nodeObjects.Count;
			for (int i = 0; i < n; i++) 
			{
				RiverNodeObject node = riverScript.nodeObjects[i];
				node.position = Handles.PositionHandle(node.position, Quaternion.identity);
				Handles.Label(node.position+Vector3.up, "["+i+"]");
			}
		}
	
		if(riverScript.curRiverNodeToPosite >= 0)
		{
			if(currentEvent.isKey && currentEvent.character == 'r')
			{
				/*
				if(riverScript.nodeObjects.Count > riverScript.curRiverNodeToPosite)
				{
					RiverNodeObject curRiverNode = riverScript.nodeObjects[riverScript.curRiverNodeToPosite];
					curRiverNode.position = GetTerrainCollisionInEditor(currentEvent, true);
					curRiverNode.position.y += riverScript.defRiverDepth;
					curRiverNode.width = riverScript.defRiverWidth;
				}
				*/
				riverScript.curRiverNodeToPosite = -1;					
				EditorUtility.SetDirty(riverScript);
				riverScript.CreateMesh(riverScript.riverSmooth);
			}
		}
		else if(GUI.changed)
		{
			EditorUtility.SetDirty(riverScript);
			riverScript.CreateMesh(riverScript.riverSmooth);
		}
	}
	
	public override void OnInspectorGUI() 
	{
		if(!riverScript.finalized)
		{
			EditorGUILayout.Separator();
			riverScript.terrainLayer = LayerMaskField("Terrain Layer", riverScript.terrainLayer, true);
			//EditorGUILayout.BeginHorizontal();
			//EditorGUILayout.PrefixLabel("Show handles");
			//riverScript.showHandles = EditorGUILayout.Toggle(riverScript.showHandles);
			//EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			riverScript.riverSmooth = (int) EditorGUILayout.IntSlider("Mesh Smooth", riverScript.riverSmooth, 5, 30);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			riverScript.defRiverDepth = EditorGUILayout.Slider("Def River Depth", riverScript.defRiverDepth, 0f, 10f);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			riverScript.defRiverWidth = EditorGUILayout.Slider("Def River Width", riverScript.defRiverWidth, 0f, 100f);
			EditorGUILayout.EndHorizontal();
			
			if(riverScript.nodeObjects != null)
			{
				int nNodes = riverScript.nodeObjects.Count;
				for(int i = 0; i <= nNodes; i++)
				{
					if(DrawRiverNodes(i, nNodes))	break;
				}
			}
			if (GUI.changed && riverScript.curRiverNodeToPosite < 0) 
			{
				EditorUtility.SetDirty(riverScript);
				riverScript.CreateMesh(riverScript.riverSmooth);
			}

			GUILayout.Label("Press r to refresh");
			if(GUILayout.Button("Gen Voxel Data"))
			{
				River2Voxel river2voxel = new River2Voxel();
				river2voxel.GenRiverVoxels(riverScript);
			}
		}
	}
	
	// This is a method that returns a point on the terrain that has been selected with the mouse when pressing a certain key
	public Vector3 GetTerrainCollisionInEditor(Event currentEvent, bool bInSceneGUI)
	{
		Vector3 returnCollision = new Vector3();
		
		AttachedRiverScript riverScript = (AttachedRiverScript) target as AttachedRiverScript;
		Ray terrainRay = new Ray();
		
		Camera SceneCameraReceptor = bInSceneGUI ? Camera.current : Camera.main;
		if(SceneCameraReceptor != null)
		{
			RaycastHit raycastHit = new RaycastHit();
			Vector2 newMousePosition = new Vector2(currentEvent.mousePosition.x, Screen.height - (currentEvent.mousePosition.y + 25));
			terrainRay = SceneCameraReceptor.ScreenPointToRay(newMousePosition);
			
			Debug.DrawRay(terrainRay.origin, terrainRay.direction, Color.green);
			if(Physics.Raycast(terrainRay, out raycastHit, Mathf.Infinity, riverScript.terrainLayer))
			{
				returnCollision = raycastHit.point;
				//returnCollision.x = Mathf.RoundToInt((returnCollision.x/terData.size.x) * terData.heightmapResolution);
				//returnCollision.y = returnCollision.y/terData.size.y;
				//returnCollision.z = Mathf.RoundToInt((returnCollision.z/terData.size.z) * terData.heightmapResolution);
			}
			else
				Debug.LogError("Error: No collision with terrain to create node");
		}
		
		return returnCollision;
	}
	
	public static List<string> layers;
	public static List<int> layerNumbers;
	public static string[] layerNames;
	public static long lastUpdateTick;
	
	/** Displays a LayerMask field.
	 * \param showSpecial Use the Nothing and Everything selections
	 * \param selected Current LayerMask
	 * \version Unity 3.5 and up will use the EditorGUILayout.MaskField instead of a custom written one.
	 */
	public static LayerMask LayerMaskField (string label, LayerMask selected, bool showSpecial) {
	
	    //Unity 3.5 and up
	
	    if (layers == null || (System.DateTime.Now.Ticks - lastUpdateTick > 10000000L && Event.current.type == EventType.Layout)) {
	        lastUpdateTick = System.DateTime.Now.Ticks;
	        if (layers == null) {
	            layers = new List<string>();
	            layerNumbers = new List<int>();
	            layerNames = new string[4];
	        } else {
	            layers.Clear ();
	            layerNumbers.Clear ();
	        }
	
	        int emptyLayers = 0;
	        for (int i=0;i<32;i++) {
	            string layerName = LayerMask.LayerToName (i);
	
	            if (layerName != "") {
	
	                for (;emptyLayers>0;emptyLayers--) layers.Add ("Layer "+(i-emptyLayers));
	                layerNumbers.Add (i);
	                layers.Add (layerName);
	            } else {
	                emptyLayers++;
	            }
	        }
	
	        if (layerNames.Length != layers.Count) {
	            layerNames = new string[layers.Count];
	        }
	        for (int i=0;i<layerNames.Length;i++) layerNames[i] = layers[i];
	    }
	
	    selected.value =  EditorGUILayout.MaskField (label,selected.value,layerNames);
	
	    return selected;
	}	
}