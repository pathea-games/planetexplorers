//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Xml;
//using System.Xml.Serialization;
//using System;
//using TownData;
//using RandomTownXML;

//namespace RandomTownXML
//{
//    [Serializable()]
//    [System.Xml.Serialization.XmlRoot("RandomTownDsc")]
//    public class RandomTownDsc
//    {
//        [XmlAttribute("TownSize")]
//        public int townSize { get; set; }
//        [XmlAttribute("DistanceX")]
//        public int distanceX { get; set; }
//        [XmlAttribute("DistanceZ")]
//        public int distanceZ { get; set; }
//        [XmlElement("StartTown")]
//        public Town startTown { get; set; }
//        [XmlArray("TownArray")]
//        [XmlArrayItem("Town", typeof(Town))]
//        public Town[] town { get; set; }
//    }



//    [Serializable()]
//    public class Town
//    {
//        [XmlAttribute("tid")]
//        public int tid { get; set; }
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

//        [XmlArray("NPCArray")]
//        [XmlArrayItem("NPC", typeof(NpcIdNum))]
//        public NpcIdNum[] npcIdNum { get; set; }

//        [XmlArray("BuildingNumArray")]
//        [XmlArrayItem("BuildingNum", typeof(BuildingNum))]
//        public BuildingNum[] buildingNum { get; set; }

//        [XmlArray("CellArray")]
//        [XmlArrayItem("Cell", typeof(Cell))]
//        public Cell[] cell { get; set; }

//        [XmlElement("LadderArray")]
//        public LadderArray ladderArray { get; set; }
//    }

//    [Serializable()]
//    public class BuildingNum
//    {
//        [XmlAttribute("bid")]
//        public int bid { get; set; }
//        [XmlAttribute("num")] //percent in procedural terrain
//        public int num { get; set; }
//    }
//    [Serializable()]
//    public class Cell
//    {
//        [XmlAttribute("x")]
//        public int x { get; set; }
//        [XmlAttribute("z")]
//        public int z { get; set; }
//        [XmlAttribute("rot")]
//        public int rot { get; set; }
//    }

//    [Serializable()]
//    public class LadderArray
//    {
//        [XmlAttribute("selectNum")]
//        public int selectNum { get; set; }
//        [XmlElement("Ladder")]
//        public Ladder[] ladder { get; set; }
//    }

//    [Serializable()]
//    public class Ladder
//    {
//        [XmlAttribute("x")]
//        public int x { get; set; }
//        [XmlAttribute("z")]
//        public int z { get; set; }
//        [XmlAttribute("rot")]
//        public int rot { get; set; }
//    }

//    [Serializable()]
//    public class NpcIdNum
//    {
//        [XmlAttribute("nid")]
//        public int nid { get; set; }
//        [XmlAttribute("num")]
//        public int num { get; set; }
//    }
//}

//public class RandomTownXMLParser
//{

//    private static RandomTownXMLParser instance;
//    public static RandomTownXMLParser Instance
//    {
//        get
//        {
//            if (null == instance)
//                instance = new RandomTownXMLParser();

//            return instance;
//        }
//    }

//    public RandomTownXMLParser()
//    {
//    }


//    public static void TestXxmlCreating()
//    {
//        string filePath = Application.dataPath + "/TestRandomTownXML";
//        Directory.CreateDirectory(filePath);
//        filePath += "/RandomTown.xml";
//        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
//        {
//            XmlSerializer serialize = new XmlSerializer(typeof(RandomTownDsc));
//            NpcIdNum[] npcIdNum = new NpcIdNum[2];
//            npcIdNum[0] = new NpcIdNum();
//            npcIdNum[1] = new NpcIdNum();
//            Cell[] cell = new Cell[2];
//            cell[0] = new Cell();
//            cell[1] = new Cell();
//            BuildingNum[] bdnum = new BuildingNum[2];
//            bdnum[0] = new BuildingNum();
//            bdnum[1] = new BuildingNum();
//            Town town = new Town();
//            town.npcIdNum = npcIdNum;
//            town.cell = cell;
//            town.buildingNum = bdnum;
//            RandomTownDsc rtd = new RandomTownDsc();
//            Town[] townArry = new Town[] { town, town };
//            rtd.town = townArry;
//            rtd.startTown = town;
//            serialize.Serialize(fs, rtd);
//        }
//    }


//}

