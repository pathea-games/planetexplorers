using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;
using TownData;

namespace VoxelPaintXML
{
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("AutoPaintDesc")]
    public class rootCLS
    {
        [XmlAttribute("regionsMap")]
        public string regionsMap { get; set; }
        [XmlArray("RegionArray")]
        [XmlArrayItem("Region", typeof(RegionDescArrayCLS))]
        public RegionDescArrayCLS[] RegionDescArrayValues { get; set; }
    }
    [Serializable()]
    public class RegionDescArrayCLS
    {
        [XmlAttribute("color")]
        public string color { get; set; }
        [XmlAttribute("name")]
        public string name { get; set; }
        //[XmlAttribute("terChance")] //percent in procedural terrain
        //public float terChance { get; set; }
        [XmlAttribute("mineChance")] //percent in procedural terrain
        public float mineChance { get; set; }
        [XmlElement("Trees")]
        public PlantDescArrayCLS trees { get; set; }
        [XmlElement("Grasses")]
        public PlantDescArrayCLS grasses { get; set; }
        [XmlElement("NewGrasses")]
        public PlantDescArrayCLS newgrasses { get; set; }
        [XmlArray("HeightDescArray")]
        [XmlArrayItem("HeightDesc", typeof(HeightDescArrayCLS))]
        public HeightDescArrayCLS[] HeightDescArrayValues { get; set; }
        [XmlArray("TerrainDescArray")]
        [XmlArrayItem("TerrainDesc", typeof(TerrainDesc))]
        public TerrainDesc[] TerrainDescArrayValues { get; set; }
        [XmlArray("MineChanceArrayCLS")]
        [XmlArrayItem("MineChance", typeof(MineChanceArrayCLS))]
        public MineChanceArrayCLS[] MineChanceArrayValues { get; set; }
    }



    [Serializable()]
    public class PlantDescArrayCLS
    {
        [XmlAttribute("startFadeIn")]	// now its range is 0~255
        public float startFadeIn { get; set; }
        [XmlAttribute("start")]
        public float start { get; set; }
        //[XmlAttribute("sizeVariate")]
        //public float sizeVariate { get; set; }
        [XmlAttribute("cellSize")]
        public float cellSize { get; set; }
        //[XmlArray("PlantHeightDescArray")]
        //[XmlArrayItem("PlanteHeightDesc", typeof(PlantHeightDesc))]
        [XmlElement("PlantHeightDesc")]
        public PlantHeightDesc[] PlantHeightDescValues { get; set; }
    }

    [Serializable()]
    public class PlantHeightDesc
    {
        [XmlAttribute("start")]
        public int start { get; set; }
        [XmlAttribute("end")]
        public int end { get; set; }
        //[XmlArray("PlantGradientDescArray")]
        //[XmlArrayItem("PlantGradientDesc", typeof(PlantGradientDesc))]
        [XmlElement("PlantGradientDesc")]
        public PlantGradientDesc[] PlantGradientDescValues { get; set; }
    }

    [Serializable()]
    public class PlantGradientDesc
    {
        [XmlAttribute("start")]
        public int start { get; set; }
        [XmlAttribute("end")]
        public int end { get; set; }
        [XmlElement("Plant")]
        public PlantDescCLS[] PlantDescArrayValues { get; set; }
        // not in serialization
        public float startTan;
        public float endTan;
    }

    [Serializable()]
    public class PlantDescCLS
    {
        [XmlAttribute("index")]
        public int idx { get; set; }
        [XmlAttribute("pct")]			// now its range is 0~99
        public float pct { get; set; }
        [XmlAttribute("wScaleMin")]
        public float wScaleMin { get; set; }
        [XmlAttribute("wScaleMax")]
        public float wScaleMax { get; set; }
        [XmlAttribute("hScaleMin")]
        public float hScaleMin { get; set; }
        [XmlAttribute("hScaleMax")]
        public float hScaleMax { get; set; }
    }

    [Serializable()]
    public class TerrainDesc
    {
        [XmlAttribute("terChance")]
        public float terChance { get; set; }
    }

    [Serializable()]
    public class MineChanceArrayCLS
    {
        [XmlAttribute("type")]
        public int type { get; set; }
        [XmlAttribute("perc")]
        public int perc { get; set; }
    }
    [Serializable()]
    public class HeightDescArrayCLS
    {
        [XmlAttribute("start")]
        public int start { get; set; }
        [XmlAttribute("end")]
        public int end { get; set; }
        [XmlArray("GradientDescArray")]
        [XmlArrayItem("GradientDesc", typeof(GradientDescArrayCLS))]
        public GradientDescArrayCLS[] GradientDescArrayValues { get; set; }
    }
    [Serializable()]
    public class GradientDescArrayCLS
    {
        [XmlAttribute("start")]
        public int start { get; set; }
        [XmlAttribute("end")]
        public int end { get; set; }
        [XmlArray("VoxelRateArray")]
        [XmlArrayItem("VoxelRate", typeof(VoxelRateArrayCLS))]
        public VoxelRateArrayCLS[] VoxelRateArrayValues { get; set; }
        // not in serialization
        public float startTan;
        public float endTan;
    }

    [Serializable()]
    public class VoxelRateArrayCLS
    {
        [XmlAttribute("type")]
        public int type { get; set; }
        [XmlAttribute("perc")]
        public int perc { get; set; }
    }
}

public class VoxelPaintXMLParser
{
    const int vegeCell0 = 6;
    const int vegeCell1 = 2;
    const int vegeCell2 = 1;
    const double noiseScale = 8.0 / 256;
    const float noVegeRad0 = (float)(Math.PI) * 0.25f;//45 tree	     //(float)(Math.PI)*0.1f;			//18 degree
    const float noVegeRad1 = (float)(Math.PI) * 0.333333f;//60  all  //(float)(Math.PI)*0.111111f;	//20 degree
    const float noVegeRad2 = (float)(Math.PI) * 0.1f; //18  //newGrass
    const float noVegeRad3 = (float)(Math.PI) * 0.194444f;//35  grass
    static readonly float noVegeTan0 = (float)Math.Tan(noVegeRad0);
    static readonly float noVegeTan1 = (float)Math.Tan(noVegeRad1);
    static readonly float noVegeTan2 = (float)Math.Tan(noVegeRad2);
    static readonly float noVegeTan3 = (float)Math.Tan(noVegeRad3);
    const double _1000ToHalfPi = Math.PI / 2000.0;
    //const float fadeVegeRad0 = noVegeRad0*0.5f;
    //const float fadeRangeRad0 = noVegeRad0 - fadeVegeRad0;
    //tree distribution
    const float fadeInScale = 8;
    const float fadeInParm = 0.3f;
    const float startScale = 4;
    const float startParm = 0.7f;
	public static int MapTypeToRegionId(RandomMapType type){
		return (int)type - 1;
	}

    public VoxelPaintXML.rootCLS prms;
    private VoxelPaintXML.RegionDescArrayCLS curRegion;
    private VoxelPaintXML.HeightDescArrayCLS curHeight;
    private VoxelPaintXML.GradientDescArrayCLS curGradient;
    private float reqGradTan, reqHeight;
    private double reqNoise;

	private int terrainSectionsMapW;
    //private int terrainSectionsMapH;
    private Color32[] terrainSectionsCols32;
    private byte defType;
    private SimplexNoise myNoise;
    private System.Random myRand;
    public int RandSeed { set { myRand = new System.Random(value); } }

    // Load XML
#if UNITY_EDITOR
    public void LoadXMLAtPath(string xmlPath = "Assets/Editor/EditorAssets/paintVxMat.xml", string regionMapFolder = "Assets/Editor/EditorAssets/")
    {
        TextAsset xmlResource =  UnityEditor.AssetDatabase.LoadAssetAtPath(xmlPath, typeof(TextAsset)) as TextAsset;
        StringReader reader = new StringReader(xmlResource.text);
        if (null == reader)
            return;

        XmlSerializer serializer = new XmlSerializer(typeof(VoxelPaintXML.rootCLS));
        prms = (VoxelPaintXML.rootCLS)serializer.Deserialize(reader);
        reader.Close();
        Grad1000ToTan();

        Texture2D terrainSectionsMap = UnityEditor.AssetDatabase.LoadAssetAtPath(regionMapFolder + prms.regionsMap, typeof(Texture2D)) as Texture2D;
        if (terrainSectionsMap != null)
        {
            terrainSectionsMapW = terrainSectionsMap.width;
            //terrainSectionsMapH = terrainSectionsMap.height;
            terrainSectionsCols32 = terrainSectionsMap.GetPixels32();
        }
        else
        {
            terrainSectionsCols32 = null;
            Debug.LogWarning("VoxelPaintXMLParser: No sectionmap found.");
        }
        curRegion = prms.RegionDescArrayValues[0];
        curHeight = curRegion.HeightDescArrayValues[0];
        curGradient = curHeight.GradientDescArrayValues[0];
        defType = 3;
        myNoise = new SimplexNoise();
        myRand = new System.Random(0);
    }
#endif
    public void LoadXMLInResources(string xmlPath, string regionMapFolder, SimplexNoise noise, System.Random rand)
    {
        TextAsset xmlResource = Resources.Load(xmlPath) as TextAsset;

        XmlSerializer serializer = new XmlSerializer(typeof(VoxelPaintXML.rootCLS));
        StringReader reader = new StringReader(xmlResource.text);
        if (null == reader)
            return;

        prms = (VoxelPaintXML.rootCLS)serializer.Deserialize(reader);
        reader.Close();
        Grad1000ToTanAndSetHeight();

        //SetPlantHeight();
        //SetTerrainTextureHeight();

        Texture2D terrainSectionsMap = Resources.Load(regionMapFolder + prms.regionsMap) as Texture2D;
        if (terrainSectionsMap != null)
        {
            terrainSectionsMapW = terrainSectionsMap.width;
            //terrainSectionsMapH = terrainSectionsMap.height;
            terrainSectionsCols32 = terrainSectionsMap.GetPixels32();
        }
        else
        {
            terrainSectionsCols32 = null;
            Debug.LogWarning("VoxelPaintXMLParser: No sectionmap found.");
        }
        curRegion = prms.RegionDescArrayValues[0];
        curHeight = curRegion.HeightDescArrayValues[0];
        curGradient = curHeight.GradientDescArrayValues[0];
        defType = 3;
        myNoise = noise;
        myRand = rand;
    }
    public void LoadXMLInResources(string xmlPath, string regionMapFolder, int randSeed)
    {
        LoadXMLInResources(xmlPath, regionMapFolder, new SimplexNoise(randSeed), new System.Random(randSeed));
    }
    private void Grad1000ToTan()
    {
        foreach (VoxelPaintXML.RegionDescArrayCLS regionDesc in prms.RegionDescArrayValues)
        {

            foreach (VoxelPaintXML.HeightDescArrayCLS heightDesc in regionDesc.HeightDescArrayValues)
            {
                foreach (VoxelPaintXML.GradientDescArrayCLS gradientDesc in heightDesc.GradientDescArrayValues)
                {
                    gradientDesc.startTan = (float)Math.Tan(gradientDesc.start * _1000ToHalfPi);
                    gradientDesc.endTan = (float)Math.Tan(gradientDesc.end * _1000ToHalfPi);
                }
            }

            if (regionDesc.trees != null)
            {
                foreach (VoxelPaintXML.PlantHeightDesc plantHeight in regionDesc.trees.PlantHeightDescValues)
                {
                    foreach (VoxelPaintXML.PlantGradientDesc plantGradientDesc in plantHeight.PlantGradientDescValues)
                    {
                        plantGradientDesc.startTan = (float)Math.Tan(plantGradientDesc.start * _1000ToHalfPi);
                        plantGradientDesc.endTan = (float)Math.Tan(plantGradientDesc.end * _1000ToHalfPi);
                    }
                }
            }

            if (regionDesc.grasses != null)
            {
                foreach (VoxelPaintXML.PlantHeightDesc plantHeight in regionDesc.grasses.PlantHeightDescValues)
                {
                    foreach (VoxelPaintXML.PlantGradientDesc plantGradientDesc in plantHeight.PlantGradientDescValues)
                    {
                        plantGradientDesc.startTan = (float)Math.Tan(plantGradientDesc.start * _1000ToHalfPi);
                        plantGradientDesc.endTan = (float)Math.Tan(plantGradientDesc.end * _1000ToHalfPi);
                    }
                }
            }

            if (regionDesc.newgrasses != null)
            {
                foreach (VoxelPaintXML.PlantHeightDesc plantHeight in regionDesc.newgrasses.PlantHeightDescValues)
                {
                    foreach (VoxelPaintXML.PlantGradientDesc plantGradientDesc in plantHeight.PlantGradientDescValues)
                    {
                        plantGradientDesc.startTan = (float)Math.Tan(plantGradientDesc.start * _1000ToHalfPi);
                        plantGradientDesc.endTan = (float)Math.Tan(plantGradientDesc.end * _1000ToHalfPi);
                    }
                }
            }
        }
    }
    private void Grad1000ToTanAndSetHeight()
    {
        foreach (VoxelPaintXML.RegionDescArrayCLS regionDesc in prms.RegionDescArrayValues)
        {

            foreach (VoxelPaintXML.HeightDescArrayCLS heightDesc in regionDesc.HeightDescArrayValues)
            {
                if (heightDesc.start == 0)
                {
                    heightDesc.end += (int)(VFDataRTGen.waterHeight - 3.9f);
                }
                else
                {
                    if (heightDesc.start == 2 && VFDataRTGen.waterHeight > 7)
                    {
                        heightDesc.start += (int)(VFDataRTGen.waterHeight - 3.9f);
                        heightDesc.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                    }
                    else if (heightDesc.start != 2 && VFDataRTGen.waterHeight > 7)
                    {
                        heightDesc.start += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                        heightDesc.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                    }
                    else
                    {
                        heightDesc.start += (int)(VFDataRTGen.waterHeight - 3.9f);
                        heightDesc.end += (int)(VFDataRTGen.waterHeight - 3.9f);
                    }
                }
                foreach (VoxelPaintXML.GradientDescArrayCLS gradientDesc in heightDesc.GradientDescArrayValues)
                {
                    gradientDesc.startTan = (float)Math.Tan(gradientDesc.start * _1000ToHalfPi);
                    gradientDesc.endTan = (float)Math.Tan(gradientDesc.end * _1000ToHalfPi);
                }
            }

            if (regionDesc.trees != null)
            {
                foreach (VoxelPaintXML.PlantHeightDesc plantHeight in regionDesc.trees.PlantHeightDescValues)
                {
                    if (VFDataRTGen.waterHeight > 7)
                    {
                        if (plantHeight.start == 3)
                        {
                            plantHeight.start += (int)(VFDataRTGen.waterHeight - 3.9f);
                            plantHeight.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                        }
                        else
                        {
                            plantHeight.start += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                            plantHeight.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                        }
                    }
                    else
                    {
                        plantHeight.start += (int)(VFDataRTGen.waterHeight - 3.9f);
                        plantHeight.end += (int)(VFDataRTGen.waterHeight - 3.9f);
                    }
                    foreach (VoxelPaintXML.PlantGradientDesc plantGradientDesc in plantHeight.PlantGradientDescValues)
                    {
                        plantGradientDesc.startTan = (float)Math.Tan(plantGradientDesc.start * _1000ToHalfPi);
                        plantGradientDesc.endTan = (float)Math.Tan(plantGradientDesc.end * _1000ToHalfPi);
                    }
                }
            }

            if (regionDesc.grasses != null)
            {
                foreach (VoxelPaintXML.PlantHeightDesc plantHeight in regionDesc.grasses.PlantHeightDescValues)
                {
                    if (VFDataRTGen.waterHeight > 7)
                    {
                        if (plantHeight.start == 3)
                        {
                            plantHeight.start += (int)(VFDataRTGen.waterHeight - 3.9f);
                            plantHeight.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                        }
                        else
                        {
                            plantHeight.start += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                            plantHeight.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                        }
                    }
                    else
                    {
                        plantHeight.start += (int)(VFDataRTGen.waterHeight - 3.9f);
                        plantHeight.end += (int)(VFDataRTGen.waterHeight - 3.9f);
                    }
                    foreach (VoxelPaintXML.PlantGradientDesc plantGradientDesc in plantHeight.PlantGradientDescValues)
                    {
                        plantGradientDesc.startTan = (float)Math.Tan(plantGradientDesc.start * _1000ToHalfPi);
                        plantGradientDesc.endTan = (float)Math.Tan(plantGradientDesc.end * _1000ToHalfPi);
                    }
                }
            }

            if (regionDesc.newgrasses != null)
            {
                foreach (VoxelPaintXML.PlantHeightDesc plantHeight in regionDesc.newgrasses.PlantHeightDescValues)
                {
                    if (VFDataRTGen.waterHeight > 7)
                    {
                        if (plantHeight.start == 3)
                        {
                            plantHeight.start += (int)(VFDataRTGen.waterHeight - 3.9f);
                            plantHeight.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                        }
                        else
                        {
                            plantHeight.start += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                            plantHeight.end += (int)(VFDataRTGen.waterHeight - 3.9f) - 5;
                        }
                    }
                    else
                    {
                        plantHeight.start += (int)(VFDataRTGen.waterHeight - 3.9f);
                        plantHeight.end += (int)(VFDataRTGen.waterHeight - 3.9f);
                    }
                    foreach (VoxelPaintXML.PlantGradientDesc plantGradientDesc in plantHeight.PlantGradientDescValues)
                    {
                        plantGradientDesc.startTan = (float)Math.Tan(plantGradientDesc.start * _1000ToHalfPi);
                        plantGradientDesc.endTan = (float)Math.Tan(plantGradientDesc.end * _1000ToHalfPi);
                    }
                }
            }
        }
    }

    //GenNoise, return value in [-1,1]
    public double GetNoise(int noisePosX, int noisePosZ)
    {
        return myNoise.Noise(noisePosX * noiseScale, noisePosZ * noiseScale);
    }
    //Paint voxel type
    private byte GetVoxelTypeFromCurGradient()
    {
        int rate = (int)((reqNoise + 1) * 50);
		VoxelPaintXML.VoxelRateArrayCLS voxelRateDesc;
        for (int i = 0; i < curGradient.VoxelRateArrayValues.Length; i++) {
			voxelRateDesc = curGradient.VoxelRateArrayValues [i];
			if (rate <= voxelRateDesc.perc) {
				return (byte)voxelRateDesc.type;
			}
		}
        return defType;
    }
    private byte GetVoxelTypeFromCurHeight()
    {
		VoxelPaintXML.GradientDescArrayCLS gradientDesc;
        for (int i = 0; i < curHeight.GradientDescArrayValues.Length; i++) {
			gradientDesc = curHeight.GradientDescArrayValues [i];
			if (gradientDesc.startTan <= reqGradTan && gradientDesc.endTan > reqGradTan) {
				curGradient = gradientDesc;
				return GetVoxelTypeFromCurGradient ();
			}
		}
        return defType;
    }
    private byte GetVoxelTypeFromCurRegion()
    {
		VoxelPaintXML.HeightDescArrayCLS heightDesc;
        for (int i = 0; i < curRegion.HeightDescArrayValues.Length; i++) {
			heightDesc = curRegion.HeightDescArrayValues [i];
			if (heightDesc.start <= reqHeight && heightDesc.end > reqHeight) {
				curHeight = heightDesc;
				return GetVoxelTypeFromCurHeight ();
			}
		}
        return defType;
    }
    public byte GetVoxelType(float gradTan, float height, double noise, int idxRegion)
    {
		if(gradTan>999){
			Debug.LogError("gradTan:"+gradTan);
			gradTan = 999;
		}
		if(gradTan<0){
			Debug.LogError("gradTan:"+gradTan);
			gradTan = 0;
		}
		reqGradTan = gradTan;
        reqHeight = height;
        reqNoise = noise;
        //if(curRegion == prms.RegionDescArrayValues[idxRegion])
        //{
        //    if(curGradient.startTan <= reqGradTan && curGradient.endTan > reqGradTan)
        //    {
        //        return GetVoxelTypeFromCurGradient();
        //    }
        //    else
        //    {
        //        return GetVoxelTypeFromCurHeight();
        //    }				
        //}
        //else
        //{
        curRegion = prms.RegionDescArrayValues[idxRegion];
        return GetVoxelTypeFromCurRegion();
        //}
    }
    public byte GetVoxelType(float gradTan, float height, int posX, int posZ)
    {
        int iCol = 0;
        bool colMatch = true;	//default
        reqGradTan = gradTan;
        reqHeight = height;
        reqNoise = GetNoise(posX, posZ);

        if (curRegion.color != null)
        {
            int u = (int)(posX * terrainSectionsMapW / (float)VoxelTerrainConstants._worldSideLenX);
            int v = (int)(posZ * terrainSectionsMapW / (float)VoxelTerrainConstants._worldSideLenZ);
            Color32 col = terrainSectionsCols32[(v * terrainSectionsMapW + u)];
            iCol = (col.a << 24) | (col.r << 16) | (col.g << 8) | (col.b);
            colMatch = Convert.ToInt32(curRegion.color, 16) == iCol;
        }
        if (colMatch)
        {
            if (curHeight.start <= reqHeight && curHeight.end > reqHeight)
            {
                if (curGradient.startTan <= reqGradTan && curGradient.endTan > reqGradTan)
                {
                    return GetVoxelTypeFromCurGradient();
                }
                else
                {
                    return GetVoxelTypeFromCurHeight();
                }
            }
            else
            {
                return GetVoxelTypeFromCurRegion();
            }
        }
        foreach (VoxelPaintXML.RegionDescArrayCLS regionDesc in prms.RegionDescArrayValues)
        {
            if (Convert.ToInt32(regionDesc.color, 16) == iCol)
            {
                curRegion = regionDesc;
                return GetVoxelTypeFromCurRegion();
            }
        }
        Debug.Log("Unrecognized region color------");
        return defType;
    }
    public byte GetVxMatByGradient(float h, float hXL, float hXR, float hZL, float hZR, int posX, int posZ)
    {
        float fxGrad = hXR - hXL;
        float fzGrad = hZR - hZL;
        double dh = Math.Sqrt((fxGrad * fxGrad + fzGrad * fzGrad)) * 0.5;
        return GetVoxelType((float)dh, h, posX, posZ);
    }

    private void PlantAVegetation(int startX, int startZ, int iX, int iZ, int iScl, VoxelPaintXML.PlantDescCLS[] pVeges, float[][] tileHeightBuf, float posVar,
                                  List<TreeInfo> outlstTreeInfo)
    {
        float fx = iX + ((float)myRand.NextDouble() - 0.5f) * posVar; if (fx < 0) return;
        float fz = iZ + ((float)myRand.NextDouble() - 0.5f) * posVar; if (fz < 0) return;
        int ix = (int)(fx + 0.5f) + VoxelTerrainConstants._numVoxelsPrefix;
        int iz = (int)(fz + 0.5f) + VoxelTerrainConstants._numVoxelsPrefix;
        float fy = tileHeightBuf[iz][ix]; if (fy < 4.0f) return;//minHeight

        int nVegesTypes = pVeges.Length;
        int idxVegeType = 0;
        int rand100 = myRand.Next(100);
        for (; idxVegeType < nVegesTypes && pVeges[idxVegeType].pct < rand100; idxVegeType++) ;
        if (idxVegeType >= nVegesTypes)
            return;
        /* Now there are very few legacy grasses , so we ignore the following code.
        float rad = tileGradBuf[iz][ix];
        if (rad > noVegeRad0)			
            return;
        if ((rad - fadeVegeRad0) > (float)myRand.NextDouble() * fadeRangeRad0)
            return;
        */

        TreeInfo _treeinfo = TreeInfo.GetTI();
        _treeinfo.m_clr = Color.white;
        _treeinfo.m_lightMapClr = Color.white;
        _treeinfo.m_protoTypeIdx = pVeges[idxVegeType].idx;
		_treeinfo.m_pos = new Vector3(fx*iScl + startX, fy, fz*iScl + startZ);
        _treeinfo.m_heightScale = pVeges[idxVegeType].hScaleMin + (float)(myRand.NextDouble()) * (pVeges[idxVegeType].hScaleMax - pVeges[idxVegeType].hScaleMin);
        _treeinfo.m_widthScale = pVeges[idxVegeType].wScaleMin + (float)(myRand.NextDouble()) * (pVeges[idxVegeType].wScaleMax - pVeges[idxVegeType].wScaleMin);
        //_treeinfo.m_heightScale = 1;
        //_treeinfo.m_widthScale = 1;
        outlstTreeInfo.Add(_treeinfo);
    }
    private void PlantANewGrass(int startX, int startZ, int iX, int iZ, float fDensity, VoxelPaintXML.PlantDescCLS[] pVeges, float[][] tileHeightBuf,
                                List<VoxelGrassInstance> outlstGrassInst)
    {
        int ix = iX + VoxelTerrainConstants._numVoxelsPrefix;
        int iz = iZ + VoxelTerrainConstants._numVoxelsPrefix;
        float fy = tileHeightBuf[iz][ix]; if (fy < 4.0f) return;//minHeight

        float fHeightXL = tileHeightBuf[iz - 1][ix];
        float fHeightXR = tileHeightBuf[iz + 1][ix];
        float fHeightZL = tileHeightBuf[iz][ix - 1];
        float fHeightZR = tileHeightBuf[iz][ix + 1];
        float sx = fHeightZR - fHeightZL;	// _iC is x
        float sz = fHeightXR - fHeightXL;	// _iR is z

        int nVegesTypes = pVeges.Length;
        int idxVegeType = 0;
        int rand100 = myRand.Next(100);
        for (; idxVegeType < nVegesTypes && pVeges[idxVegeType].pct < rand100; idxVegeType++) ;
        if (idxVegeType >= nVegesTypes)
            return;

        VoxelGrassInstance _grassInst = new VoxelGrassInstance();
        _grassInst.Position = new Vector3(startX + iX, fy, startZ + iZ);
        _grassInst.Density = fDensity;
        _grassInst.Normal = new Vector3(-sx, 2.0f, sz); // The attribute will normalize this vector
        _grassInst.ColorDw = new Color32(0xff, 0xff, 0xff, 0xff);
        _grassInst.Prototype = pVeges[idxVegeType].idx;
        outlstGrassInst.Add(_grassInst);
    }

	public void PlantTrees(VFTile terTile, double[][] tileNoiseBuf, float[][] tileHeightBuf, float[][] tileGradTanBuf,
	                            List<TreeInfo> outlstTreeInfo, RandomMapType[][] tileMapType, int szCell)
	{
		int startX = terTile.tileX << VoxelTerrainConstants._shift;
		int startZ = terTile.tileZ << VoxelTerrainConstants._shift;
		int iScl = 1 << terTile.tileL;
		for (int iz = 0; iz < VoxelTerrainConstants._numVoxelsPerAxis; iz += szCell)
		{
			int idxZ = iz + VoxelTerrainConstants._numVoxelsPrefix;
			double[] xNoiseBuf = tileNoiseBuf[idxZ];
			float[] xHeightBuf = tileHeightBuf[idxZ];
			float[] xGradTanBuf = tileGradTanBuf[idxZ];
			byte[][] xyVoxels = terTile.terraVoxels[idxZ];
			for (int ix = 0; ix < VoxelTerrainConstants._numVoxelsPerAxis; ix += szCell)
			{
				int idxX = ix + VoxelTerrainConstants._numVoxelsPrefix;
				float fy = xHeightBuf[idxX];
				byte[] yVoxels = xyVoxels[idxX];
				byte vType = yVoxels[((int)fy)*VFVoxel.c_VTSize + 1];
				if (VFDataRTGen.IsNoPlantType(vType)) {
					continue;
				}
				
				float gradTan = xGradTanBuf[idxX];
				//坡度大于60度草树皆不刷
				if (gradTan >= noVegeTan1) continue;

				double noise256 = (xNoiseBuf[idxX] + 1) * 128;
				curRegion = prms.RegionDescArrayValues[MapTypeToRegionId(tileMapType[iz][ix])];
				if (gradTan <  noVegeTan0)
				{
					VoxelPaintXML.PlantDescArrayCLS veges0 = curRegion.trees;
					if (veges0 != null)
					{
						//树的空间单位
						if (0 == (ix*iScl) % veges0.cellSize && 0 == (iz*iScl) % veges0.cellSize)
						{
							if (noise256 >= veges0.start || (noise256 > veges0.startFadeIn && myRand.NextDouble() <= (noise256 - veges0.startFadeIn) / (veges0.start - veges0.startFadeIn)))
							{
								VoxelPaintXML.PlantHeightDesc treeHeight = null;
								for (int i = 0; i < veges0.PlantHeightDescValues.Length; i++) {
									VoxelPaintXML.PlantHeightDesc heightDesc = veges0.PlantHeightDescValues [i];
									if (heightDesc.start <= fy && heightDesc.end > fy) {
										treeHeight = heightDesc;
									}
								}
								if (treeHeight != null)
								{
									VoxelPaintXML.PlantGradientDesc treeGradient = null;
									for (int i = 0; i < treeHeight.PlantGradientDescValues.Length; i++) {
										VoxelPaintXML.PlantGradientDesc GradientDesc = treeHeight.PlantGradientDescValues [i];
										if (GradientDesc.start <= gradTan && GradientDesc.end > gradTan) {
											treeGradient = GradientDesc;
										}
									}
									PlantAVegetation(startX, startZ, ix, iz, iScl, treeGradient.PlantDescArrayValues, tileHeightBuf, 5.0f, outlstTreeInfo);
								}
							}
						}
					}
				}				
				
				//坡度大于45度不刷草药
				//if (gradTan >= noVegeTan0) continue;
				if (gradTan < noVegeTan3)
				{
					VoxelPaintXML.PlantDescArrayCLS veges1 = curRegion.grasses;
					if (veges1 != null)
					{
						//旧草的空间单位
						if (0 == (ix*iScl) % veges1.cellSize && 0 == (iz*iScl) % veges1.cellSize)
						{
							if (noise256 >= veges1.start || (noise256 > veges1.startFadeIn && myRand.NextDouble() <= (noise256 - veges1.startFadeIn) / (veges1.start - veges1.startFadeIn)))
							{
								VoxelPaintXML.PlantHeightDesc grassHeight = null;
								foreach (VoxelPaintXML.PlantHeightDesc heightDesc in veges1.PlantHeightDescValues)
								{
									if (heightDesc.start <= fy && heightDesc.end > fy)
									{
										grassHeight = heightDesc;
									}
								}
								if (grassHeight != null)
								{
									VoxelPaintXML.PlantGradientDesc grassGradient = null;
									foreach (VoxelPaintXML.PlantGradientDesc GradientDesc in grassHeight.PlantGradientDescValues)
									{
										if (GradientDesc.start <= gradTan && GradientDesc.end > gradTan)
										{
											grassGradient = GradientDesc;
										}
									}
									if (grassGradient != null)
									{
										PlantAVegetation(startX, startZ, ix, iz, iScl, grassGradient.PlantDescArrayValues, tileHeightBuf, 1.7f, outlstTreeInfo);
									}
								}
							}
						}
					}
				}
			}
		}
	}
	public void PlantGrass(VFTile terTile, double[][] tileNoiseBuf, float[][] tileHeightBuf, float[][] tileGradTanBuf,
	                       List<VoxelGrassInstance> outlstGrassInst, RandomMapType[][] tileMapType, int szCell)
	{
		int startX = terTile.tileX << VoxelTerrainConstants._shift;
		int startZ = terTile.tileZ << VoxelTerrainConstants._shift;
		//int iScl = 1 << terTile.tileL;
		for (int iz = 0; iz < VoxelTerrainConstants._numVoxelsPerAxis; iz += szCell)
		{
			int idxZ = iz + VoxelTerrainConstants._numVoxelsPrefix;
			double[] xNoiseBuf = tileNoiseBuf[idxZ];
			float[] xHeightBuf = tileHeightBuf[idxZ];
			float[] xGradTanBuf = tileGradTanBuf[idxZ];
			byte[][] xyVoxels = terTile.terraVoxels[idxZ];
			for (int ix = 0; ix < VoxelTerrainConstants._numVoxelsPerAxis; ix += szCell)
			{
				int idxX = ix + VoxelTerrainConstants._numVoxelsPrefix;
				float fy = xHeightBuf[idxX];
				byte[] yVoxels = xyVoxels[idxX];
				byte vType = yVoxels[((int)fy)*VFVoxel.c_VTSize + 1];
				if (VFDataRTGen.IsNoPlantType(vType)) {
					continue;
				}				
				
				float gradTan = xGradTanBuf[idxX];				
				//坡度大于60度草树皆不刷
				if (gradTan >= noVegeTan1) continue;
				// greed>30 no new grass
				if (gradTan >= noVegeTan2) continue;

				curRegion = prms.RegionDescArrayValues[MapTypeToRegionId(tileMapType[iz][ix])];
				VoxelPaintXML.PlantDescArrayCLS veges2 = curRegion.newgrasses;
				if (veges2 != null)
				{
					//新草的空间单位
					if (0 == ix % veges2.cellSize && 0 == iz % veges2.cellSize)
					{
						double noise256 = (xNoiseBuf[idxX] + 1) * 128;
						if (noise256 >= veges2.start || (noise256 > veges2.startFadeIn && myRand.NextDouble() <= (noise256 - veges2.startFadeIn) / (veges2.start - veges2.startFadeIn)))
						{
							VoxelPaintXML.PlantHeightDesc newGrassHeight = null;
							VoxelPaintXML.PlantGradientDesc newGrassGradient = null;
							foreach (VoxelPaintXML.PlantHeightDesc heightDesc in veges2.PlantHeightDescValues)
							{
								if (heightDesc.start <= fy && heightDesc.end > fy)
								{
									newGrassHeight = heightDesc;
								}
							}
							if (newGrassHeight != null)
							{
								foreach (VoxelPaintXML.PlantGradientDesc GradientDesc in newGrassHeight.PlantGradientDescValues)
								{
									if (GradientDesc.start <= gradTan && GradientDesc.end > gradTan)
									{
										newGrassGradient = GradientDesc;
									}
								}
								if (newGrassGradient != null)
								{
									float fDensity = ((float)noise256 - veges2.startFadeIn) / (veges2.start - veges2.startFadeIn); /// old : veges0
									if (fDensity > 1.0f) fDensity = 1.0f;
									PlantANewGrass(startX, startZ, ix, iz, fDensity, newGrassGradient.PlantDescArrayValues, tileHeightBuf, outlstGrassInst);
								}
							}
						}
					}
				}
			}
		}
	}

	public void PlantVegetation(VFTile terTile, double[][] tileNoiseBuf, float[][] tileHeightBuf, float[][] tileGradTanBuf,
                                List<TreeInfo> outlstTreeInfo, List<VoxelGrassInstance> outlstGrassInst, RandomMapType[][] tileMapType, int szCell)
    {
		int startX = terTile.tileX << VoxelTerrainConstants._shift;
		int startZ = terTile.tileZ << VoxelTerrainConstants._shift;
		int iScl = 1 << terTile.tileL;
		for (int iz = 0; iz < VoxelTerrainConstants._numVoxelsPerAxis; iz += szCell)
        {
            int idxZ = iz + VoxelTerrainConstants._numVoxelsPrefix;
            double[] xNoiseBuf = tileNoiseBuf[idxZ];
            float[] xHeightBuf = tileHeightBuf[idxZ];
            float[] xGradTanBuf = tileGradTanBuf[idxZ];
			byte[][] xyVoxels = terTile.terraVoxels[idxZ];
			for (int ix = 0; ix < VoxelTerrainConstants._numVoxelsPerAxis; ix += szCell)
            {
				int idxX = ix + VoxelTerrainConstants._numVoxelsPrefix;
				float fy = xHeightBuf[idxX];
				byte[] yVoxels = xyVoxels[idxX];
				byte vType = yVoxels[((int)fy)*VFVoxel.c_VTSize + 1];
				if (VFDataRTGen.IsNoPlantType(vType)) {
					continue;
                }

				curRegion = prms.RegionDescArrayValues[MapTypeToRegionId(tileMapType[iz][ix])];

                VoxelPaintXML.PlantDescArrayCLS veges0 = curRegion.trees;
                VoxelPaintXML.PlantDescArrayCLS veges1 = curRegion.grasses;
                VoxelPaintXML.PlantDescArrayCLS veges2 = curRegion.newgrasses;

                VoxelPaintXML.PlantHeightDesc treeHeight = null;
                VoxelPaintXML.PlantHeightDesc grassHeight = null;
                VoxelPaintXML.PlantHeightDesc newGrassHeight = null;

                VoxelPaintXML.PlantGradientDesc treeGradient = null;
                VoxelPaintXML.PlantGradientDesc grassGradient = null;
                VoxelPaintXML.PlantGradientDesc newGrassGradient = null;

                double noise256 = (xNoiseBuf[idxX] + 1) * 128;                
                float gradTan = xGradTanBuf[idxX];

                //坡度大于60度草树皆不刷
                if (gradTan >= noVegeTan1) continue;
                if (gradTan <  noVegeTan0)
                {
                    if (veges0 != null)
                    {
                        //树的空间单位
                        if (0 == ix % veges0.cellSize && 0 == iz % veges0.cellSize)
                        {
                            bool bTree = false;
                            if (noise256 >= veges0.start || (noise256 > veges0.startFadeIn && myRand.NextDouble() <= (noise256 - veges0.startFadeIn) / (veges0.start - veges0.startFadeIn)))
                            {
                                bTree = true;
                            }

                            if (bTree)
                            {
                                for (int i = 0; i < veges0.PlantHeightDescValues.Length; i++) {
									VoxelPaintXML.PlantHeightDesc heightDesc = veges0.PlantHeightDescValues [i];
									if (heightDesc.start <= fy && heightDesc.end > fy) {
										treeHeight = heightDesc;
									}
								}
                                if (treeHeight != null)
                                {
                                    for (int i = 0; i < treeHeight.PlantGradientDescValues.Length; i++) {
										VoxelPaintXML.PlantGradientDesc GradientDesc = treeHeight.PlantGradientDescValues [i];
										if (GradientDesc.start <= gradTan && GradientDesc.end > gradTan) {
											treeGradient = GradientDesc;
										}
									}
									PlantAVegetation(startX, startZ, ix, iz, iScl, treeGradient.PlantDescArrayValues, tileHeightBuf, 5.0f, outlstTreeInfo);
                                }
                            }
                        }
                    }
                }


                //坡度大于45度不刷草药
                //if (gradTan >= noVegeTan0) continue;
                if (gradTan < noVegeTan3)
                {
                    if (veges1 != null)
                    {
                        //旧草的空间单位
                        if (0 == ix % veges1.cellSize && 0 == iz % veges1.cellSize)
                        {
                            if (noise256 >= veges1.start || (noise256 > veges1.startFadeIn && myRand.NextDouble() <= (noise256 - veges1.startFadeIn) / (veges1.start - veges1.startFadeIn)))
                            {
                                foreach (VoxelPaintXML.PlantHeightDesc heightDesc in veges1.PlantHeightDescValues)
                                {
                                    if (heightDesc.start <= fy && heightDesc.end > fy)
                                    {
                                        grassHeight = heightDesc;
                                    }
                                }
                                if (grassHeight != null)
                                {
                                    foreach (VoxelPaintXML.PlantGradientDesc GradientDesc in grassHeight.PlantGradientDescValues)
                                    {
                                        if (GradientDesc.start <= gradTan && GradientDesc.end > gradTan)
                                        {
                                            grassGradient = GradientDesc;
                                        }
                                    }
                                    if (grassGradient != null)
                                    {
										PlantAVegetation(startX, startZ, ix, iz, iScl, grassGradient.PlantDescArrayValues, tileHeightBuf, 1.7f, outlstTreeInfo);
                                    }
                                }
                            }
                        }
                    }
                }

                // legacy grass
                // new grass

                // greed>30 no new grass
                if (gradTan >= noVegeTan2) continue;
                if (veges2 != null)
                {
                    //新草的空间单位
                    if (0 == ix % veges2.cellSize && 0 == iz % veges2.cellSize)
                    {
                        if (noise256 >= veges2.start || (noise256 > veges2.startFadeIn && myRand.NextDouble() <= (noise256 - veges2.startFadeIn) / (veges2.start - veges2.startFadeIn)))
                        {
                            foreach (VoxelPaintXML.PlantHeightDesc heightDesc in veges2.PlantHeightDescValues)
                            {
                                if (heightDesc.start <= fy && heightDesc.end > fy)
                                {
                                    newGrassHeight = heightDesc;
                                }
                            }
                            if (newGrassHeight != null)
                            {
                                foreach (VoxelPaintXML.PlantGradientDesc GradientDesc in newGrassHeight.PlantGradientDescValues)
                                {
                                    if (GradientDesc.start <= gradTan && GradientDesc.end > gradTan)
                                    {
                                        newGrassGradient = GradientDesc;
                                    }
                                }
                                if (newGrassGradient != null)
                                {
                                    float fDensity = ((float)noise256 - veges2.startFadeIn) / (veges0.start - veges0.startFadeIn);
                                    if (fDensity > 1.0f) fDensity = 1.0f;
                                    PlantANewGrass(startX, startZ, ix, iz, fDensity, newGrassGradient.PlantDescArrayValues, tileHeightBuf, outlstGrassInst);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
