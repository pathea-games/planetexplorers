using System;
using UnityEngine;

public class OperatableItemAgent : ISceneSerializableObjAgent
{
    int mId;
    Vector3 mPos;
    public string mPrefabPath;

    int mAgentId;
    OperatableItem mOperatableItem;

    public OperatableItemAgent()
    {
    }

    public OperatableItemAgent(int id, Vector3 pos, string prefabPath)
    {
        mId = id;
        mPos = pos;
        mPrefabPath = prefabPath;
    }

    void Create()
    {
        GameObject prefabObj = Resources.Load(mPrefabPath) as GameObject;
        if (null == prefabObj)
        {
            Debug.LogError("Operatable item load failed.");
            return;
        }

        GameObject itemObj = GameObject.Instantiate(prefabObj, mPos, Quaternion.identity) as GameObject;

        if (null == itemObj)
        {
            return;
        }

        mOperatableItem = itemObj.GetComponent<OperatableItem>();
        if (null == mOperatableItem || false == mOperatableItem.Init(mId))
        {
            Debug.LogError("Operatable item load failed.");
            GameObject.Destroy(itemObj);
            mOperatableItem = null;
        }
    }

    void Destory()
    {
        if (mOperatableItem != null)
        {
            GameObject.Destroy(mOperatableItem.gameObject);
            mOperatableItem = null;
        }
    }
    
    int ISceneObjAgent.Id
    {
        get
        {
            return mAgentId;
        }
        set
        {
            mAgentId = value;
        }
    }
    public int ScenarioId { get; set; }

    GameObject ISceneObjAgent.Go
    {
        get
        {
            if (null == mOperatableItem)
            {
                return null;
            }
            else
            {
                return mOperatableItem.gameObject;
            }
        }
    }

    Vector3 ISceneObjAgent.Pos			{		get{	return mPos;        }	}	
	IBoundInScene ISceneObjAgent.Bound	{		get{ 	return null;		}	}
	bool ISceneObjAgent.NeedToActivate	{		get{	return false;		}	}
	bool ISceneObjAgent.TstYOnActivate	{		get{	return false;		}	}
    void ISceneObjAgent.OnConstruct()
    {
        Create();
    }

    void ISceneObjAgent.OnActivate()
    {
        throw new NotImplementedException();
    }

    void ISceneObjAgent.OnDeactivate()
    {
        throw new NotImplementedException();
    }

    void ISceneObjAgent.OnDestruct()
    {
        Destory();
    }

    void ISerializable.Serialize(System.IO.BinaryWriter bw)
    {
        bw.Write((int)mId);
        PETools.Serialize.WriteVector3(bw, mPos);
        PETools.Serialize.WriteNullableString(bw, mPrefabPath);
    }

    void ISerializable.Deserialize(System.IO.BinaryReader br)
    {
        mId = br.ReadInt32();
        mPos = PETools.Serialize.ReadVector3(br);
        mPrefabPath = PETools.Serialize.ReadNullableString(br);
    }
}