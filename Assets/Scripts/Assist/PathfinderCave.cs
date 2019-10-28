using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using PETools;
using Pathfinding;
using Pathea;

public class PathfinderCave : MonoBehaviour 
{
    [System.Serializable]
    public class CaveData
    {
        public Vector2 center = Vector2.zero;
        public Vector2 size = Vector2.zero;

        public Vector2 min { get { return center - size * 0.5f; } }
        public Vector2 max { get { return center + size * 0.5f; } }

        static List<CaveData> s_CaveData = new List<CaveData>();

        public static CaveData Get(Vector2 v2)
        {
            float sqrDis = 0.0f;
            CaveData cave = null;

            if (s_CaveData.Count > 0)
            {
                cave = s_CaveData[0];
                sqrDis = Vector2.SqrMagnitude(v2 - s_CaveData[0].center);

                foreach (CaveData data in s_CaveData)
                {
                    float fd = Vector2.SqrMagnitude(v2 - data.center);
                    if(fd < sqrDis)
                    {
                        cave = data;
                        sqrDis = fd;
                    }
                }
            }
                
            return cave;
        }

        public static bool GetCenter(Vector2 v, out Vector2 v1, out Vector2 v2)
        {
            v1 = Vector2.zero;
            v2 = Vector2.zero;

            CaveData[] caves = s_CaveData.FindAll(ret => Vector2.SqrMagnitude(v - ret.center) <= 512.0f*512.0f).ToArray();

            if(caves != null && caves.Length > 0)
            {
                Vector2 min = caves[0].min;
                Vector2 max = caves[0].max;

                foreach (CaveData cave in caves)
                {
                    min = Vector2.Min(min, cave.min);
                    max = Vector2.Max(max, cave.max);
                }

                v1 = (max + min)*0.5f;
                v2 =  max - min;

                return true;
            }

            return false;
        }

        public static void Add(CaveData[] datas)
        {
            s_CaveData.AddRange(datas);
        }

        public static void Load(string path)
        {
            TextAsset text = Resources.Load(path) as TextAsset;
            if(text != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;
                XmlReader reader = XmlReader.Create(new StringReader(text.text), settings);
                xmlDoc.Load(reader);

                XmlElement element = xmlDoc.SelectSingleNode("CaveInfo") as XmlElement;
                foreach (XmlElement e in element.GetElementsByTagName("Cave"))
                {
                    float cx = XmlUtil.GetAttributeFloat(e, "centerX");
                    float cz = XmlUtil.GetAttributeFloat(e, "centerZ");

                    float sx = XmlUtil.GetAttributeFloat(e, "caveWidthX") + 32.0f;
                    float sz = XmlUtil.GetAttributeFloat(e, "caveWidthz") + 32.0f;

                    CaveData data = new CaveData();
                    data.center = new Vector2(cx, cz);
                    data.size = new Vector2(sx, sz);

                    s_CaveData.Add(data);
                }

                reader.Close();
            }
        }
    }

    public CaveData[] storyCaves;

    LayerGridGraph layerGridGraph;

    void Awake()
    {
        if(PeGameMgr.IsStory)
            CaveData.Add(storyCaves);

        CaveData.Load("Cave/CaveData");

        if (AstarPath.active == null) throw new System.Exception("There is no AstarPath object in the scene");

        layerGridGraph = AstarPath.active.astarData.layerGridGraph;

        if (layerGridGraph == null) throw new System.Exception("The AstarPath object has no GridGraph");

        UpdateCavePosition();
    }
	
	void Update () 
    {
        UpdateCavePosition();
	}

    void UpdateCavePosition()
    {
        if (layerGridGraph != null && PeCreature.Instance.mainPlayer != null)
        {
            PeEntity mainPlayer = PeCreature.Instance.mainPlayer;
            Vector2 v2 = new Vector2(mainPlayer.position.x, mainPlayer.position.z);

            //Vector2 center, size;
            //if(CaveData.GetCenter(v2, out center, out size))
            //{
            //    Vector3 v = new Vector3(center.x, 0.0f, center.y);
            //    Vector3 d = v - layerGridGraph.center;
            //    if(d != Vector3.zero)
            //    {
            //        layerGridGraph.center += d;
            //        layerGridGraph.GenerateMatrix();
            //    }

            //    int width = (int)size.x;
            //    int depth = (int)size.y;

            //    if(layerGridGraph.width != width || layerGridGraph.depth != depth)
            //    {
            //        layerGridGraph.width = width;
            //        layerGridGraph.depth = depth;

            //        layerGridGraph.UpdateSizeFromWidthDepth();
            //    }
            //}

            CaveData data = CaveData.Get(v2);
            if (data != null)
            {
                Vector3 v3 = new Vector3(data.center.x, 0.0f, data.center.y);
                Vector3 dir = v3 - layerGridGraph.center;
                if (dir != Vector3.zero)
                {
                    layerGridGraph.center += dir;
                    layerGridGraph.GenerateMatrix();
                }
            }
        }
    }
}
