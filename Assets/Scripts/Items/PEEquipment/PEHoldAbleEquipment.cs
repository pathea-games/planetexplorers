using UnityEngine;
using Pathea;
using System;

[Serializable]
public class CameraModeData
{
	public int      camModeIndex1st;
	public int 		camModeIndex3rd;
	public Vector3	offsetUp;
	public Vector3	offset;
	public Vector3 	offsetDown;

	public CameraModeData(int index1st, int index3rd, Vector3 up, Vector3 mid, Vector3 down)
	{
		camModeIndex1st = index1st;
		camModeIndex3rd = index3rd;
		offsetUp = up;
		offset = mid;
		offsetDown = down;
	}
	
	public static CameraModeData DefaultCameraData = new CameraModeData(0, 0, new Vector3(0,0,0), new Vector3(0,-0.1f,0), new Vector3(0,0,0));
}


[Serializable]
public class ActiveAttr
{
    public string[] m_Layers;

    public string   m_PutOnBone = "mountMain";
	public string	m_PutOffBone = "Long_Gun";
	
	public string	m_PutOnAnim = "SwordPutOn";
	public string	m_PutOffAnim = "SwordPutOff";

	public CameraModeData m_CamMode;
	
	public PEActionType	m_ActiveActionType = PEActionType.GunHold;
	public PEActionType m_UnActiveActionType = PEActionType.GunPutOff;
	public PEActionMask	m_HoldActionMask = PEActionMask.GunHold;
	public MoveStyle	m_BaseMoveStyle = MoveStyle.Normal;
	public MoveStyle 	m_MoveStyle = MoveStyle.Normal;
	public float		m_AimIKAngleRange = 40f;
}

public class PEHoldAbleEquipment : PECtrlAbleEquipment
{
	[Header("HoldAbleAttr")]
	public ActiveAttr m_HandChangeAttr;

    protected void InitLayer(PeEntity entity)
    {
        AnimatorCmpt animator = entity.GetComponent<AnimatorCmpt>();

        if (animator != null)
        {
            foreach (string layerName in m_HandChangeAttr.m_Layers)
            {
                if (string.IsNullOrEmpty(layerName))
                    continue;

                animator.SetLayerWeight(layerName, 1.0f);
            }
        }
    }

    protected void ResetLayer(PeEntity entity)
    {
        AnimatorCmpt animator = entity.GetComponent<AnimatorCmpt>();

        if (animator != null)
        {
            foreach (string layerName in m_HandChangeAttr.m_Layers)
            {
                if (string.IsNullOrEmpty(layerName))
                    continue;

                animator.SetLayerWeight(layerName, 0.0f);
            }
        }
    }

	public override void InitEquipment (PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
        InitLayer(entity);
		m_View.AttachObject(gameObject, m_HandChangeAttr.m_PutOffBone);
	}

    public override void RemoveEquipment()
    {
        base.RemoveEquipment();

        ResetLayer(m_Entity);
    }

	public virtual bool canHoldEquipment{ get{ return true; } }
}