using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;


public class GMTools : EditorWindow
{
    public List<Vector3> m_PathList = new List<Vector3>();
    public Dictionary<int, GameObject> m_PrefabMap = new Dictionary<int, GameObject>();
    string strID = "";
    Vector3 newPos = Vector3.zero;
    int idx = 0;
    bool bClickPos = false;
    //public static GMTools Instance = this;

    [UnityEditor.MenuItem("Window/PathConfig")]
    
    static void Init()
    {
        GMTools window = (GMTools)EditorWindow.GetWindow(typeof(GMTools));
        window.Show();
    }

    void OnGUI()
    {
		if ( GameConfig.IsInVCE )
			return;
        //EditorGUILayout.LabelField("Time since start: ",
        //EditorApplication.timeSinceStartup.ToString());
        //this.Repaint();
        
        
        GUILayout.Label("                              PathConfig");

        EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

        strID = EditorGUILayout.TextField("MissionID", strID);
        if (GUILayout.Button("AddMiss", GUILayout.Width(60), GUILayout.Height(20)))
        {
            Debug.Log(strID);
        }

        if (GUILayout.Button("LoadMiss", GUILayout.Width(60), GUILayout.Height(20)))
        {
            LoadXML();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
        if (GUILayout.Button("AddStep", GUILayout.Width(60), GUILayout.Height(20)))
        {
            //if (strID == "")
            //    return;

            idx++;
            m_PathList.Add(newPos);
        }

        if (GUILayout.Button("DelStep", GUILayout.Width(60), GUILayout.Height(20)))
        {
            if (m_PathList.Count > 0)
            {
                m_PathList.RemoveAt(m_PathList.Count - 1);
                if (m_PrefabMap.ContainsKey(m_PathList.Count))
                    Destroy(m_PrefabMap[m_PathList.Count]);
                m_PrefabMap.Remove(m_PathList.Count);

            }

            idx--;
            if (idx < 0)
                idx = 0;
        }

        if (GUILayout.Button("ClickPos"))
        {
            bClickPos = true;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

        for (int i = 0; i < idx; i++)
        {
            m_PathList[i] = EditorGUILayout.Vector3Field("Step" + i, m_PathList[i]);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
        if (GUILayout.Button("Save"))
        {
            Debug.Log(strID);
        }

        if (GUILayout.Button("Clean"))
        {
            Clean();
        }

        if (GUILayout.Button("ExportTxt"))
        {
            ExportTxt();
        }
        EditorGUILayout.EndHorizontal();

        if (Input.GetMouseButton(0))
        {
            if (bClickPos)
            {
                ClickPos();
                bClickPos = false;
            }
        }

        UpdatePrefabPos();

    }

    void ClickPos()
    {
        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo, 200, AiUtil.groundedLayer))
        {
            if (m_PathList.Count > 0)
            {
                m_PathList[m_PathList.Count - 1] = hitInfo.point;
                string prefab = "Prefab/Item/Other/Malti_td_tower_C";

                if (!m_PrefabMap.ContainsKey(idx - 1))
                {
                    UnityEngine.Object obj = Resources.Load(prefab);
                    Quaternion quaternion = Quaternion.identity;
                    GameObject gameObject = Instantiate(obj, hitInfo.point, quaternion) as GameObject;
                    m_PrefabMap.Add(idx - 1, gameObject);
                }
            }
        }
    }

    public void Clean()
    {
        idx = 0;
        strID = "";
        foreach (KeyValuePair<int, GameObject> ite in m_PrefabMap)
        {
            Destroy(ite.Value);
        }

        m_PathList.Clear();
        bClickPos = false;
    }

    void UpdatePrefabPos()
    {
        foreach (KeyValuePair<int, GameObject> ite in m_PrefabMap)
        {
            if (ite.Value == null)
                continue;

            if(m_PathList.Count != 0 && m_PathList.Count > ite.Key)
                ite.Value.transform.position = m_PathList[ite.Key];
        }
    }

    void ExportTxt()
    {
        string savePath = "";
        StringBuilder sb = new StringBuilder();
        SaveFileDialog opf = new SaveFileDialog();
        opf.Filter = "ÎÄ±ŸÎÄŒþ|*.txt";
        if (opf.ShowDialog() == DialogResult.OK)
        {
            savePath = opf.FileName;
        }
        else
            return;

        if (savePath == "")
        {
            MessageBox.Show("ÇëÑ¡Ôñ±£ŽæÂ·Ÿ¶£¡");
            return;
        }

        string content = "";
        for (int i = 0; i < m_PathList.Count; i++)
        {
            content = "m_PathList.Add(new Vector3" + m_PathList[i].ToString() + ");" + "\r\n";
            sb.Append(content);
        }

        System.IO.File.WriteAllText(savePath, sb.ToString(), System.Text.Encoding.UTF8);
    }

    void GoToSave()
    {
        WriteXML();
    }

    void WriteXML()
    {

    }

    void LoadXML()
    {

    }
}