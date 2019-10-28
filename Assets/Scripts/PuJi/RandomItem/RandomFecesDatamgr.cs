using Mono.Data.SqliteClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct ProbableItem{
    public int protoId;
    public int numMin;
    public int numMax;
    public float probability;

    public ProbableItem ParseString(string str){
        string[] strAry = str.Split(',');
        protoId = Convert.ToInt32(strAry[0]);
        string[] numStr = strAry[1].Split('-');
        numMin = Convert.ToInt32(numStr[0]);
        numMax = Convert.ToInt32(numStr[1]);
        probability = Convert.ToSingle(strAry[2]);
        return this;
    }
}

public class FecesData
{
    public int id;
    public string path;
    public List<ProbableItem> fixItem = new List<ProbableItem> ();
    public List<ProbableItem> probableItems= new List<ProbableItem> ();

    public static Dictionary<int,FecesData> fecesDataDict =  new Dictionary<int,FecesData>();
    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("feces");

        while (reader.Read())
        {
            FecesData fd = new FecesData();
            fd.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
            fd.path = reader.GetString(reader.GetOrdinal("path"));
            string fixItemStr = reader.GetString(reader.GetOrdinal("fixedItemId"));
            string[] fixItemStrAry = fixItemStr.Split(';');
            foreach (string fis in fixItemStrAry)
            {
                ProbableItem pi = new ProbableItem();
                fd.fixItem.Add(pi.ParseString(fis));
            }
            string randomItemStr = reader.GetString(reader.GetOrdinal("randomItem"));
            string[] randomItemStrAry = randomItemStr.Split(';');
            foreach (string ris in randomItemStrAry)
            {
                ProbableItem pi = new ProbableItem();
                fd.probableItems.Add(pi.ParseString(ris));
            }
            fecesDataDict.Add(fd.id, fd);
        }
    }

    public static FecesData GetFecesData(int id)
    {
        if (fecesDataDict.ContainsKey(id))
            return fecesDataDict[id];
        else
            return null;
    }
    public static List<int> GetAllId()
    {
       return fecesDataDict.Keys.ToList();
	}
	public static string GetModelPath(int seed){
		if(seed<0)
			seed=-seed;
		int count= fecesDataDict.Keys.Count;
		int pickIndex = seed%count;
		int pickId = fecesDataDict.Keys.ToList()[pickIndex];
		return fecesDataDict[pickId].path;
	}
}


public class RandomFecesDataMgr
{
	static string defaultPath = FecesModelPath.PATH_01;
	
    public static int[] GenFecesItemIdCount(out string modelPath)
    {
        List<int> itemIdCount = new List<int>();

        modelPath = defaultPath;
        List<int> allIds = FecesData.GetAllId();
        System.Random rand = new System.Random();
        int index = rand.Next(allIds.Count);
        int id = allIds[index];
        FecesData fd = FecesData.GetFecesData(id);
        modelPath = fd.path;
        foreach (ProbableItem pi in fd.fixItem)
        {
            if(rand.NextDouble()>pi.probability)
                continue;
            itemIdCount.Add(pi.protoId);
            itemIdCount.Add(rand.Next(pi.numMin, pi.numMax));
        }
        foreach (ProbableItem pi in fd.probableItems)
        {
            if (rand.NextDouble() > pi.probability)
                continue;
            itemIdCount.Add(pi.protoId);
            itemIdCount.Add(rand.Next(pi.numMin, pi.numMax));
        }
        return itemIdCount.ToArray();
    }
}