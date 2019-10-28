using UnityEngine;

// Matters config
// Each material will use one game item ( n -> 1 )
// Read "localdatabase" to fetch the matter configs for game.
public class VCMatterInfo
{
	public int ItemIndex;
	public int Order;
	public int ItemId;
	public string Name{ get { return ItemAsset.ItemProto.GetName(ItemId); } }
	public float Attack;
	public float Defence;
	public float Durability;
	public float Hp;
	public float SellPrice;
	public float Density;
	public float Elasticity;
    public float DefaultBumpStrength;
	public Color32 DefaultSpecularColor;
	public float DefaultSpecularStrength;
	public float DefaultSpecularPower;
	public Color32 DefaultEmissiveColor;
	public float DefaultTile;
	public string DefaultDiffuseRes;
	public string DefaultBumpRes;
}
