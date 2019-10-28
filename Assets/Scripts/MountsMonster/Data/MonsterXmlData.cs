using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using Behave.Runtime;
using System.Collections.Generic;


public interface ImxmlData { }
public class PounceData : ImxmlData
{
    public string _name;
    public string _pounce;
    public  float _startTime;
    public float _endTime;
    public float _stopTime;
    public int _skillID;

    public PounceData(XmlElement e)
    {
        //int id = XmlConvert.ToInt32(node.Attributes["id"].Value);
        // < Action Type = "Pounce" >< !--前扑-- >
        // < Data Name = "Pounce"  pounce = "attack5"  prob = "0.5" cdTime = "4" minRange = "1.0" maxRange = "4.0" minAngle = "-30" maxAngle = "30" startTime = "0.5" endTime = "1.2" stopTime = "2.167" skillID = "30100036" ></ Data >
        //</ Action >
        XmlNodeList nodes = e.GetElementsByTagName("Data");
        foreach (XmlNode node in nodes)
        {
            _name = node.Attributes["Name"].Value;
            _pounce = node.Attributes["pounce"].Value;
            _startTime = XmlConvert.ToSingle(node.Attributes["startTime"].Value);
            _endTime = XmlConvert.ToSingle(node.Attributes["endTime"].Value);
            _stopTime = XmlConvert.ToSingle(node.Attributes["stopTime"].Value);
            _skillID = XmlConvert.ToInt32(node.Attributes["skillID"].Value);
        }

    }
}
public class MonsterXmlData
{
    static Dictionary<int, ImxmlData> m_Data = new Dictionary<int, ImxmlData>();
   public static void InitializeData(int protoId,string btPath)
    {
        TextAsset asset = UnityEngine.Resources.Load(btPath) as TextAsset;
        if (asset != null && !string.IsNullOrEmpty(asset.text))
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(new StringReader(asset.text), settings);
            xmlDoc.Load(reader);

            XmlElement root = xmlDoc.SelectSingleNode("Tree") as XmlElement;
            XmlNodeList xmlNodeList = root.GetElementsByTagName("Action");
            foreach (XmlNode node in xmlNodeList)
            {
                XmlElement e = node as XmlElement;
                string typeName = XmlUtil.GetAttributeString(e, "Type");
                if (typeName.Equals("Pounce"))
                {
                    PounceData _dat = new PounceData(e);
                    m_Data.Add(protoId,_dat);
                    break;
                }
               
            }
        }


        }

    public static bool GetData<T>(int protoId, ref T t)
    {
        if(m_Data.ContainsKey(protoId))
        {
            t =(T)m_Data[protoId];
            return true;
        }
        return false;
    }

    }
