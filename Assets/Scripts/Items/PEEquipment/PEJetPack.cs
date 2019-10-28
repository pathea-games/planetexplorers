using UnityEngine;

public class PEJetPack : PECtrlAbleEquipment
{
	public int			m_StartSoundID = 794;
	public int			m_SoundID = 794;
	public GameObject	m_EffectObj;
	const string		AttachBone = "Bow_box";

	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_View.AttachObject(gameObject, PEJetPack.AttachBone);
	} 
}
