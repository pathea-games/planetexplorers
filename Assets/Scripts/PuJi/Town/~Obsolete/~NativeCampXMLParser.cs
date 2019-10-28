//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Collections;
//using System.IO;
//using System.Xml;
//using System.Xml.Serialization;
//using System;
//using RandomTownXML;
//using NativeCampXML;
//using UnityEngine;

//namespace NativeCampXML
//{
//    [Serializable()]
//    [System.Xml.Serialization.XmlRoot("NativeCampDesc")]
//    public class NativeCampDesc
//    {
//        [XmlAttribute("NumMin")]
//        public int numMin { get; set; }

//        [XmlAttribute("NumMax")]
//        public int numMax { get; set; }

//        [XmlAttribute("DistanceMin")]
//        public int distanceMin { get; set; }

//        [XmlAttribute("DistanceMax")]
//        public int distanceMax { get; set; }
//        [XmlArray("NativeCampArray")]
//        [XmlArrayItem("NativeCamp", typeof(NativeCamp))]
//        public NativeCamp[] nativeCamps { get; set; }
//    }

//    [Serializable()]
//    public class NativeCamp
//    {
//        [XmlAttribute("cid")]
//        public int cid { get; set; }

//        [XmlAttribute("CellNumX")]
//        public int cellNumX { get; set; }

//        [XmlAttribute("CellNumZ")]
//        public int cellNumZ { get; set; }

//        [XmlAttribute("CellSizeX")]
//        public int cellSizeX { get; set; }

//        [XmlAttribute("CellSizeZ")]
//        public int cellSizeZ { get; set; }

//        [XmlAttribute("weight")]
//        public int weight { get; set; }

//        [XmlAttribute("Level")]
//        public int level { get; set; }

//        [XmlElement("NativeTower")]
//        public NativeTower nativeTower { get; set; }

//        [XmlArray("NativeArray")]
//        [XmlArrayItem("Native", typeof(NativeIdNum))]
//        public NativeIdNum[] nativeIdNum { get; set; }
//        [XmlArray("BuildingNumArray")]
//        [XmlArrayItem("BuildingNum", typeof(BuildingNum))]
//        public BuildingNum[] buildingNum { get; set; }
//        [XmlArray("CellArray")]
//        [XmlArrayItem("Cell", typeof(Cell))]
//        public Cell[] cell { get; set; }

//        //[XmlArray("LadderArray")]
//        //[XmlArrayItem("Ladder", typeof(Ladder))]
//        //public Ladder[] ladder { get; set; }
//        [XmlElement("LadderArray")]
//        public LadderArray ladderArray { get; set; }
//    }

//    public class NativeTower
//    {
//        [XmlAttribute("x")]
//        public int x { get; set; }

//        [XmlAttribute("z")]
//        public int z { get; set; }

//        [XmlAttribute("rot")]
//        public int rot { get; set; }


//        [XmlAttribute("pathID")]
//        public int pathID { get; set; }

//        [XmlAttribute("bid")]
//        public int bid { get; set; }

//        [XmlElement("DynamicNative")]
//        public DynamicNative[] dynamicNatives { get; set; }
//    }

//    public class DynamicNative
//    {
//        [XmlAttribute("did")]
//        public int did { get; set; }
//        [XmlAttribute("type")]
//        public int type { get; set; }
//    }

//    public class NativeIdNum
//    {
//        [XmlAttribute("aid")]
//        public int aid { get; set; }
//        [XmlAttribute("num")]
//        public int num { get; set; }
//    }
//}

//public class NativeCampXMLParser
//{

//    private static NativeCampXMLParser instance;
//    public static NativeCampXMLParser Instance
//    {
//        get
//        {
//            if (null == instance)
//                instance = new NativeCampXMLParser();

//            return instance;
//        }
//    }

//    public NativeCampXMLParser()
//    {
//    }


//    public static void TestXxmlCreating()
//    {
//        string filePath = Application.dataPath + "/TestNativeCampXML";
//        Directory.CreateDirectory(filePath);
//        filePath += "/NativeCamp.xml";
//        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
//        {
//            XmlSerializer serialize = new XmlSerializer(typeof(NativeCampDesc));
//            NativeIdNum[] nativeIdNum = new NativeIdNum[2];
//            nativeIdNum[0] = new NativeIdNum();
//            nativeIdNum[1] = new NativeIdNum();
//            Cell[] cell = new Cell[2];
//            cell[0] = new Cell();
//            cell[1] = new Cell();
//            BuildingNum[] bdnum = new BuildingNum[2];
//            bdnum[0] = new BuildingNum();
//            bdnum[1] = new BuildingNum();
//            NativeCamp camp = new NativeCamp();
//            NativeTower nt = new NativeTower();
//            camp.nativeTower = nt;
//            camp.nativeIdNum = nativeIdNum;
//            camp.cell = cell;
//            camp.buildingNum = bdnum;
//            NativeCampDesc ncd = new NativeCampDesc();
//            NativeCamp[] townArry = new NativeCamp[] { camp, camp };
//            ncd.nativeCamps = townArry;
//            serialize.Serialize(fs, ncd);
//        }
//    }


//}
