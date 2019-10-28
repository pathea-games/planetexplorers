using Mono.Data.SqliteClient;
using System.Collections.Generic;
using UnityEngine;

public class NpcStorageMgr : MonoBehaviour
{
    public const string NpcIconName = "storage_npc";
    const int SinglePlayerId = 1;
    static Dictionary<int, NpcStorage> mDicStorage = new Dictionary<int, NpcStorage>(10);

    private static void CreateNpcStorage(int id)
    {
        if (mDicStorage.ContainsKey(id))
        {
            Debug.LogError("storage:"+id+"' exist.");
            return;
        }

        NpcStorage npcStorage = new NpcStorage();

        npcStorage.Init(id);

        mDicStorage.Add(id, npcStorage);
    }

	static bool HasStorage(int id)
    {
        return mDicStorage.ContainsKey(id);
    }

    public static NpcStorage GetStorage(int id)
    {
        if (!HasStorage(id))
        {
            CreateNpcStorage(id);
        }

        return mDicStorage[id];
    }

    public static NpcStorage GetSinglePlayerStorage()
    {
        return GetStorage(SinglePlayerId);
    }

    public static bool IsStorageNpc(int npcId)
    {
        SqliteDataReader reader = LocalDatabase.Instance.SelectWhereSingle("NPCstorage", "*", "storagenpc_id", " = ", "'" + npcId + "'");
        if (!reader.Read())
            return false;

        return true;
    }
}