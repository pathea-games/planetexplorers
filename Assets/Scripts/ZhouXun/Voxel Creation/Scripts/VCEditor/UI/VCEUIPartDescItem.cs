using UnityEngine;
using System.Collections;
using WhiteCat;

public class VCEUIPartDescItem : MonoBehaviour
{
	public VCPart m_PartProp;
	public UILabel m_NameLabel;
	public UILabel m_DescLabel;

	public void Set ( VCPart part_prop )
	{
		m_PartProp = part_prop;
		m_NameLabel.text = part_prop.gameObject.name;
		m_DescLabel.text = part_prop.description;
	}
}
