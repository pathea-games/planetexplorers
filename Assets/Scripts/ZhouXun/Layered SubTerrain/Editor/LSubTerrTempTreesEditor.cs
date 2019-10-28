using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(LSubTerrTempTrees))]
public class LSubTerrTempTreesEditor : Editor
{
	void OnSceneGUI() 
	{
		LSubTerrTempTrees tar = (LSubTerrTempTrees) target;

		foreach (KeyValuePair<GameObject, GlobalTreeInfo> kvp in tar.m_TempMap)
		{
			if (kvp.Key != null)
			{
				string content = "";
				content += "World Pos: " + kvp.Key.transform.position.ToString() + "\r\n";
				content += "Terrain Pos: (" + kvp.Value._treeInfo.m_pos.x.ToString("0.0000") + ", "
					+ kvp.Value._treeInfo.m_pos.y.ToString("0.0000") + ", "
					+ kvp.Value._treeInfo.m_pos.z.ToString("0.0000") + ")\r\n";
				content += "Terrain Key: " + kvp.Value._terrainIndex.ToString() + " (" 
					+ LSubTerrUtils.IndexToPos(kvp.Value._terrainIndex).x.ToString() + "," 
					+ LSubTerrUtils.IndexToPos(kvp.Value._terrainIndex).z.ToString() + ")\r\n";
				IntVector3 cell = new IntVector3(Mathf.FloorToInt(kvp.Key.transform.position.x) % LSubTerrConstant.Size, 
				                                 Mathf.FloorToInt(kvp.Key.transform.position.y),
				                                 Mathf.FloorToInt(kvp.Key.transform.position.z) % LSubTerrConstant.Size);
				int key = LSubTerrUtils.TreePosToKey(cell);
				content += "Tree Cell: " + cell.ToString() + "\r\n";
				content += "Cell Key: " + key.ToString() + "\r\n";
				content += "Tree Proto: " + kvp.Value._treeInfo.m_protoTypeIdx.ToString() + "\r\n";
				content += "Tree Scale: (" + kvp.Value._treeInfo.m_widthScale.ToString("0.00") + ", " 
					+ kvp.Value._treeInfo.m_heightScale.ToString("0.00") + ")\r\n";

				Handles.Label(kvp.Key.transform.position, content);
			}
		}
	}

}
