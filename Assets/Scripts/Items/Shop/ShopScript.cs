using UnityEngine;
using System.Collections;

public class ShopScript
{
    public void InitShop()
    {

    }

    public bool BuyItem(int id)
    {
        ShopData data = ShopRespository.GetShopData(id);
        if (data == null)
            return false;

        return true;
    }
}
