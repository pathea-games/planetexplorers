using UnityEngine;
using System.Collections;
using ItemAsset;
using ItemAsset.PackageHelper;

public class MergeCostItem_N : MonoBehaviour
{
	public Grid_N 	mGrid;
	public UILabel	mNumCost;
	
	int	mCosPerItem;
	
	public void SetItem(ItemSample itemsp, int cosPerItem)
	{
		mGrid.SetItem(itemsp);
		mCosPerItem = cosPerItem;
	}
	
	public void UpdateNum(int num)
	{
		if(num < 1)
			num = 1;
		if(mGrid.Item != null)
		{
			mNumCost.text = (mCosPerItem*num).ToString();
            Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
			HaveEnoughItem(pkg.package.GetCount(mGrid.Item.protoId) >= mCosPerItem*num);
		}
		else
			mNumCost.text = "";
	}
	
	public void HaveEnoughItem(bool enough)
	{
		if(enough)
			mNumCost.color = Color.white;
		else
			mNumCost.color = Color.red;
	}
}
