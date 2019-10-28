using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using VArtifactTownXML;

namespace VArtifactTownXML
{
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("VArtifactTownDesc")]
    public class VArtifactTownDesc
    {
        [XmlAttribute("townsize")]
        public int townsize { get; set; }

        [XmlAttribute("DistanceX")]
        public int distanceX { get; set; }
        [XmlAttribute("DistanceZ")]
        public int distanceZ { get; set; }
        [XmlElement("VAStartTown")]
        public VATown vaStartTown { get; set; }
        [XmlArray("VATownArray")]
        [XmlArrayItem("VATown", typeof(VATown))]
        public VATown[] vaTown { get; set; }
    }



    [Serializable()]
    public class VATown
    {
        [XmlAttribute("tid")]
        public int tid { get; set; }
        [XmlAttribute("Level")]
        public int level { get; set; }
        [XmlAttribute("weight")]
        public int weight { get; set; }

        [XmlArray("ArtifactUnitArray")]
        [XmlArrayItem("ArtifactUnit", typeof(ArtifactUnit))]
        public ArtifactUnit[] artifactUnitArray { get; set; }
    }


    public class ArtifactUnit
    {
        [XmlAttribute("id")]
        public string id { get; set; }
        [XmlAttribute("pos")]
        public string pos { get; set; }
        [XmlAttribute("rot")]
        public string rot { get; set; }
        [XmlAttribute("type")]
        public int type { get; set; }
        [XmlArray("NPCArray")]
        [XmlArrayItem("NPC", typeof(NpcIdNum))]
        public NpcIdNum[] npcIdNum { get; set; }

        [XmlArray("BuildingNumArray")]
        [XmlArrayItem("BuildingNum", typeof(BuildingIdNum))]
        public BuildingIdNum[] buildingIdNum { get; set; }
    }

    [Serializable()]
    public class BuildingIdNum
    {
        [XmlAttribute("bid")]
        public int bid { get; set; }
        [XmlAttribute("num")] //percent in procedural terrain
        public int num { get; set; }

        [XmlAttribute("posIndex")]
        public int posIndex = -1;
    }

    [Serializable()]
    public class NpcIdNum
    {
        [XmlAttribute("nid")]
        public int nid { get; set; }
        [XmlAttribute("num")]
        public int num { get; set; }
    }


}
public class VArtifactTownXMLParser
{

    private static VArtifactTownXMLParser instance;
    public static VArtifactTownXMLParser Instance
    {
        get
        {
            if (null == instance)
                instance = new VArtifactTownXMLParser();

            return instance;
        }
    }

    public VArtifactTownXMLParser()
    {
    }


    public static void TestXxmlCreating()
    {
        string filePath = Application.dataPath + "/TestVATownXML";
        Directory.CreateDirectory(filePath);
        filePath += "/VATown.xml";
        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            XmlSerializer serialize = new XmlSerializer(typeof(VArtifactTownDesc));
            NpcIdNum[] npcIdNum = new NpcIdNum[2];
            npcIdNum[0] = new NpcIdNum();
            npcIdNum[1] = new NpcIdNum();
            BuildingIdNum[] bdnum = new BuildingIdNum[2];
            bdnum[0] = new BuildingIdNum();
            bdnum[1] = new BuildingIdNum();
            ArtifactUnit artifactUnit = new ArtifactUnit();
            artifactUnit.id = "-1";
            artifactUnit.pos = "100,200";
            artifactUnit.rot = "-1";
            artifactUnit.npcIdNum = npcIdNum;
            artifactUnit.buildingIdNum = bdnum;
            ArtifactUnit[] artifactUnitArray = new ArtifactUnit[] { artifactUnit, artifactUnit };
            VATown town = new VATown();
            town.artifactUnitArray = artifactUnitArray;

            VArtifactTownDesc vatd = new VArtifactTownDesc();
            VATown[] townArry = new VATown[] { town, town };
            vatd.vaStartTown = town;
            vatd.vaTown = townArry;
            serialize.Serialize(fs, vatd);
        }
    }


}