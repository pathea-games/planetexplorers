using UnityEngine;
using System.Collections;
using Pathea;

public class MonsterCreater : MonoBehaviour 
{
    public enum ProtoType
    {
        Monster,
        Doodad,
        NpcRandom,
        NpcLine
    }

    public int protoID;
    public ProtoType protoType;
    public int colorId = -1;

    void Start()
	{
        //PeEntity entity = null;

        if (protoType == ProtoType.Doodad)
            DoodadEntityCreator.CreateDoodad(protoID, transform.position);
        else if (protoType == ProtoType.NpcRandom)
            NpcEntityCreator.CreateNpc(protoID, transform.position);
        else if (protoType == ProtoType.NpcLine)
            NpcEntityCreator.CreateStoryLineNpcFromID(protoID, transform.position);
        else
            MonsterEntityCreator.CreateAdMonster(protoID, transform.position, colorId, -1);

		Destroy(gameObject);
	}
}
