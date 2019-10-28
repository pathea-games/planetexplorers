using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[CustomEditor(typeof(LoadIKAnimData))]

public class EditIKAnimData : Editor
{
    private LoadIKAnimData m_loadIKAnimData;

    public void OnEnable()
    {
        m_loadIKAnimData = target as LoadIKAnimData;
    }


    public override void OnInspectorGUI()
    {
        if (!m_loadIKAnimData)
            return;

		EditorGUIUtility.labelWidth = 100;
		EditorGUIUtility.fieldWidth = 0;

        GUI.changed = false;
        m_loadIKAnimData.m_monsterID = EditorGUILayout.IntField("monster ID", m_loadIKAnimData.m_monsterID);

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("import and setup ik"))
        {
            m_loadIKAnimData.OnLoadIKAnimData();
        }

        if (GUILayout.Button("save prefab"))
        {
            //            UnityEngine.Object _obj = EditorUtility.CreateEmptyPrefab("Assets/Prefabs/test.prefab");
            //			EditorUtility.ReplacePrefab(m_loadIKAnimData.gameObject, _obj, ReplacePrefabOptions.Default);

            //ModelImporter _mdlImp = Selection.activeObject as ModelImporter;

        }
        EditorGUILayout.EndVertical();
    }
}
