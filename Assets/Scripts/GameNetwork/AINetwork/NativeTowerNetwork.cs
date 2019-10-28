using UnityEngine;
using System.Collections;

public class NativeTowerNetwork : AiNetwork 
{
    int mTownId;
    int mCampId;
    int mDamageId;
    Vector3 mScale;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
    {
        _id = info.networkView.initialData.Read<int>();
        _externId = info.networkView.initialData.Read<int>();
        _worldId = info.networkView.initialData.Read<int>();
        mTownId = info.networkView.initialData.Read<int>();
        mCampId = info.networkView.initialData.Read<int>();
        mDamageId = info.networkView.initialData.Read<int>();
        mScale = info.networkView.initialData.Read<Vector3>();
        
    }

    protected override void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        transform.position = stream.Read<Vector3>();
        transform.rotation = stream.Read<Quaternion>();
		authId = stream.Read<int>();

        DoodadEntityCreator.CreateNetRandTerDoodad(Id, ExternId, transform.position, mScale, transform.rotation, mTownId, mCampId, mDamageId);
    }
}
