using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using VANativeCampXML;
using VArtifactTownXML;

namespace VANativeCampXML
{
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("VANativeCampDesc")]
    public class VANativeCampDesc
    {
        [XmlAttribute("NumMin")]
        public int numMin { get; set; }

        [XmlAttribute("NumMax")]
        public int numMax { get; set; }

        [XmlAttribute("DistanceMin")]
        public int distanceMin { get; set; }

        [XmlAttribute("DistanceMax")]
        public int distanceMax { get; set; }
        [XmlArray("NativeCampArray")]
        [XmlArrayItem("NativeCamp", typeof(NativeCamp))]
        public NativeCamp[] nativeCamps { get; set; }
    }

    [Serializable()]
    public class NativeCamp
    {
        [XmlAttribute("cid")]
        public int cid { get; set; }
        [XmlAttribute("weight")]
        public int weight { get; set; }

        [XmlAttribute("Level")]
        public int level { get; set; }
		
		[XmlAttribute("NativeType")]
		public int nativeType { get; set; }

        [XmlElement("NativeTower")]
        public NativeTower nativeTower { get; set; }

        [XmlArray("ArtifactUnitArray")]
        [XmlArrayItem("ArtifactUnit", typeof(ArtifactUnit))]
        public ArtifactUnit[] artifactUnitArray { get; set; }
    }


    public class NativeTower
    {
        [XmlAttribute("pathID")]
        public int pathID { get; set; }

		[XmlAttribute("campID")]
		public int campID { get; set; }

		[XmlAttribute("damageID")]
		public int damageID { get; set; }

        [XmlElement("DynamicNative")]
        public DynamicNative[] dynamicNatives { get; set; }
    }
    public class DynamicNative
    {
        [XmlAttribute("did")]
        public int did { get; set; }
        [XmlAttribute("type")]
        public int type { get; set; }
    }

}

public class VANativeCampXMLParser
{

    private static VANativeCampXMLParser instance;
    public static VANativeCampXMLParser Instance
    {
        get
        {
            if (null == instance)
                instance = new VANativeCampXMLParser();

            return instance;
        }
    }

    public VANativeCampXMLParser()
    {
    }


    public static void TestXxmlCreating()
    {
        string filePath = Application.dataPath + "/TestNativeCampXML";
        Directory.CreateDirectory(filePath);
        filePath += "/VANativeCamp.xml";
        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            XmlSerializer serialize = new XmlSerializer(typeof(VANativeCampDesc));

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

            NativeCamp camp = new NativeCamp();
            camp.artifactUnitArray = artifactUnitArray;
            DynamicNative dn = new DynamicNative();
            NativeTower nt = new NativeTower();
            nt.dynamicNatives = new DynamicNative[] { dn, dn };
            camp.nativeTower = nt;

            VANativeCampDesc ncd = new VANativeCampDesc();
            NativeCamp[] townArry = new NativeCamp[] { camp, camp };
            ncd.nativeCamps = townArry;
            serialize.Serialize(fs, ncd);
        }
    }


}
