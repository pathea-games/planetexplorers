#define PLANET_EXPLORERS
using UnityEngine;
using System.Collections;
using System.IO;
#if PLANET_EXPLORERS
using Pathea;
using ItemAsset;
using ItemAsset.PackageHelper;
#endif

public class VCEUICostItem : MonoBehaviour
{
	public int m_GameItemId;
	public int m_GameItemCost;
	public UISprite m_IconSprite;
	public UILabel m_NameLabel;
	public UILabel m_CountLabel;
	public bool m_IsEnough = false;
	
	// Use this for initialization
	void Start ()
	{
#if PLANET_EXPLORERS

		ItemSample item = new ItemSample(m_GameItemId);
		if (item.protoData == null)
			return;

		m_IconSprite.spriteName = item.iconString0;

		//ItemAsset.ItemData item = ItemAsset.ItemData.s_tblItemData.Find(iter => iter.m_ID == m_GameItemId);
		//m_IconSprite.spriteName = item.m_Icon.Split(',')[0];
		m_NameLabel.text = VCUtils.Capital(item.nameText, true);
		//if ( PlayerFactory.mMainPlayer != null )
		if (PeCreature.Instance.mainPlayer != null)
		{
			if ( VCEditor.Instance.m_CheatWhenMakeCreation )
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Cheat".ToLocalizationString() + "[-]";
				m_IsEnough = true;
			}
			else if ( Pathea.PeGameMgr.IsSingleBuild)
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Build".ToLocalizationString() + "[-]";
				m_IsEnough = true;
			}
            else if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
            {
                m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Tutorial".ToLocalizationString() + "[-]";
				m_IsEnough = true;
            }
			else
			{
                Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
				int have = pkg.package.GetCount(m_GameItemId);
				if ( have >= m_GameItemCost )
				{
					m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + have.ToString("#,##0").Trim() + "[-]";
					m_IsEnough = true;
				}
				else
				{
					m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [FF0000]" + have.ToString("#,##0").Trim() + "[-]";
					m_IsEnough = false;
				}
			}
		}
		else
		{
			m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim();
			m_IsEnough = false;
		}
#else
		m_IconSprite.spriteName = "";
		m_NameLabel.text = m_GameItemId.ToString();
		m_CountLabel.text = m_GameItemCost.ToString("# ##0");
		m_IsEnough = false;
#endif
	}
	
	// Update is called once per frame
	void Update ()
	{
#if PLANET_EXPLORERS
		if ( PeCreature.Instance.mainPlayer != null )
		{
			if ( VCEditor.Instance.m_CheatWhenMakeCreation )
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Cheat".ToLocalizationString() + "[-]";
				m_IsEnough = true;
			}
			else if ( Pathea.PeGameMgr.IsSingleBuild)
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Build".ToLocalizationString() + "[-]";
				m_IsEnough = true;
			}
            else if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
            {
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Tutorial".ToLocalizationString() + "[-]";
                m_IsEnough = true;
            }
            else
			{
                Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
				int have = pkg.package.GetCount(m_GameItemId);
				if ( have >= m_GameItemCost )
				{
					m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + have.ToString("#,##0").Trim() + "[-]";
					m_IsEnough = true;
				}
				else
				{
					m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [FF0000]" + have.ToString("#,##0").Trim() + "[-]";
					m_IsEnough = false;
				}
			}
		}
		else
		{
			m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim();
			m_IsEnough = false;
		}
#else
		m_CountLabel.text = m_GameItemCost.ToString("# ##0");
		m_IsEnough = false;
#endif
	}
}
