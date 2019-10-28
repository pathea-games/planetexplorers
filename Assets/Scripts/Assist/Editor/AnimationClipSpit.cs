using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using PETools;

public struct ClipData
{
    public string name;
    public int startframe;
    public int endframe;
    public bool loop;
}

public class AnimationClipData
{
    const string DataPath = "Assets/Models/AnimationClipData/Clip_Monster.xml";

    static AnimationClipData _Instance;
    public static AnimationClipData Instance
    {
        get
        {
            if (null == _Instance)
                _Instance = new AnimationClipData();

            return _Instance;
        }
    }

    Dictionary<string, List<ClipData>> m_Clips;

    public AnimationClipData()
    {
        m_Clips = new Dictionary<string, List<ClipData>>();

        LoadData("Assets/Models/AnimationClipData/Clip_Monster.xml");
        LoadData("Assets/Models/AnimationClipData/Clip_Native.xml");
        LoadData("Assets/Models/AnimationClipData/Clip_Robot.xml");
    }

    public List<ClipData> GetClipData(string fileName)
    {
        if(m_Clips.ContainsKey(fileName))
            return m_Clips[fileName];

        return null;
    }

    void LoadData(string path)
    {
        if (File.Exists(path))
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            TextAsset asset = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;

            if (asset == null)
                return;

            XmlReader reader = XmlReader.Create(new StringReader(asset.text), settings);
            xmlDoc.Load(reader);

            XmlElement element = xmlDoc.SelectSingleNode("Clips") as XmlElement;
            foreach (XmlElement e in element.GetElementsByTagName("Clip"))
            {
                string fileName = XmlUtil.GetAttributeString(e, "filename");
                if (fileName == "") continue;

                if (!m_Clips.ContainsKey(fileName))
                    m_Clips.Add(fileName, new List<ClipData>());

                ClipData clip;
                clip.name = XmlUtil.GetAttributeString(e, "name");
                clip.startframe = XmlUtil.GetAttributeInt32(e, "startframe");
                clip.endframe = XmlUtil.GetAttributeInt32(e, "endframe");
                clip.loop = XmlUtil.GetAttributeBool(e, "loop");

                m_Clips[fileName].Add(clip);
            }

            reader.Close();
        }
    }
}

public class AnimationClipSpit : AssetPostprocessor 
{
    void OnPreprocessModel()
    {
        ModelImporter model = assetImporter as ModelImporter;
        if (model == null)
            return;

        List<ClipData> clipDatas = AnimationClipData.Instance.GetClipData(Path.GetFileName(assetPath));
        if (clipDatas != null && clipDatas.Count > 0)
        {
            bool reset = (clipDatas.Count != model.clipAnimations.Length);

            List<ModelImporterClipAnimation> newClips = new List<ModelImporterClipAnimation>();
            List<ModelImporterClipAnimation> oldClips = new List<ModelImporterClipAnimation>(model.clipAnimations);
            foreach (ClipData clipData in clipDatas)
            {
                ModelImporterClipAnimation clip = oldClips.Find(ret => ret.name == clipData.name);

                if (clip == null)
                    clip = new ModelImporterClipAnimation();

                if(clip.name != clipData.name)
                {
                    clip.name = clipData.name;
                    reset = true;
                }

                if(Mathf.Abs(clip.firstFrame - clipData.startframe) > 0.001f)
                {
                    clip.firstFrame = clipData.startframe;
                    reset = true;
                }

                if(Mathf.Abs(clip.lastFrame - clipData.endframe) > 0.001f)
                {
                    clip.lastFrame = clipData.endframe;
                    reset = true;
                }

                if(clip.loopTime != clipData.loop)
                {
                    clip.loopTime = clipData.loop;
                    reset = true;
                }

                newClips.Add(clip);
            }

            if(reset)
                model.clipAnimations = newClips.ToArray();
        }
    }
}
