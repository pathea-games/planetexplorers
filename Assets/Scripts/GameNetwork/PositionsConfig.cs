using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.Generic;


//public class PositionsConfig
//{
//    public static PositionsConfig self;

//    [XmlArrayItemAttribute("Positon")]
//    public Vector3[] Positions;

//    public static Vector3 GetPosition(GameConfig.EGameMode mode)
//    {
//        GameConfig.EGameMode tmp = mode & GameConfig.EGameMode.GameModeMask;
//        int _mode = Mathf.Clamp((int)(tmp), (int)(GameConfig.EGameMode.None), (int)(GameConfig.EGameMode.MaxMode) - 1);

//        return self.Positions[_mode];
//    }

//    public static void InitTerrainConfig()
//    {
//        string filepath = GameConfig.PEDataPath + "ConfigFiles";
//        Directory.CreateDirectory(filepath);
//        filepath += "/Position.xml";
		
//        if (File.Exists(filepath))
//        {
//            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
//            {
//                XmlSerializer serialize = new XmlSerializer(typeof(PositionsConfig));
//                self = serialize.Deserialize(fs) as PositionsConfig;
//            }
//        }
//        else
//        {
//            using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write))
//            {
//                XmlSerializer serialize = new XmlSerializer(typeof(PositionsConfig));
//                self = new PositionsConfig();
//                self.Positions = new Vector3[(int)(GameConfig.EGameMode.MaxMode)];
//                serialize.Serialize(fs, self);
//            }
//        }
//    }
//}