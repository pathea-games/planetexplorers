using UnityEngine;
using System.Collections;
using ItemAsset;
using Pathea;

public static class UIToolFuncs
{
    //public static bool CanEquip(ItemObject item, int sex)
    //{
    //    PeSex targetSex = (sex == 1) ? Pathea.PeSex.Female : Pathea.PeSex.Male;
    //    return CanEquip(item, targetSex);
    //}
	
	public static bool CanEquip(ItemObject item, PeSex targetSex)
	{
		if (null == item)
		{
			return false;
		}
		
		ItemAsset.Equip equip = item.GetCmpt<ItemAsset.Equip>();
		if (null == equip)
		{
			return false;
		}
		
		if(equip.equipPos == 0)
		{
			return false;
		}

        if (!Pathea.PeGender.IsMatch(equip.sex, targetSex))
		{
			return false;
		}
		
		return true;
	}

    //lz-2016.07.18 显示在屏幕中超出屏幕部分的处理模式
    public enum InScreenOffsetMode 
    { 
        OffsetBounds,    //偏移一个Bounds
        OffsetLimit    //偏移超出部分
    }

    //lz-2016.07.18 根据鼠标点的位置，将这个Transfrom显示在屏幕的中合适的位置，不超出屏幕
    public static void SetTransInScreenByMousePos(this Transform trans,InScreenOffsetMode offsetMode= InScreenOffsetMode.OffsetBounds)
    {
        Vector3 mousePos=Input.mousePosition;
        mousePos = new Vector3(mousePos.x - Screen.width / 2, mousePos.y - Screen.height / 2, trans.localPosition.z);
        Vector2 pos = GetPosInScreenByPos(trans, mousePos, offsetMode);
        trans.localPosition = new Vector3(pos.x, pos.y, trans.localPosition.z);
    }


    //lz-2016.07.18 根据Trans的Bounds和ToPos来计算出合适的位置
    public static Vector2 GetPosInScreenByPos(this Transform trans, Vector3 toPos,InScreenOffsetMode offsetMode)
    {
            Bounds wndBounds = NGUIMath.CalculateRelativeWidgetBounds(trans);
            //优先计算右下角方向
            float screenWidthHalf=Screen.width/2;
            float screenHeightHalf=Screen.height/2;
            float toPosLimitX = toPos.x + wndBounds.size.x;
            float toPosLimitY = toPos.y - wndBounds.size.y;
            switch (offsetMode)
            {
                case InScreenOffsetMode.OffsetBounds:
                    if (toPosLimitX < -screenWidthHalf)
                        toPos.x += wndBounds.size.x;
                    else if (toPosLimitX > screenWidthHalf)
                        toPos.x -= wndBounds.size.x;
                    if (toPosLimitY < -screenHeightHalf)
                        toPos.y += wndBounds.size.y;
                    else if (toPosLimitY > screenHeightHalf)
                        toPos.y -= wndBounds.size.y;
                    break;
                case InScreenOffsetMode.OffsetLimit:
                    toPos.x = Mathf.Clamp(toPosLimitX, -screenWidthHalf, screenWidthHalf);
                    toPos.y = Mathf.Clamp(toPosLimitY, -screenHeightHalf, screenHeightHalf);
                    break;
            }
            return toPos;
    }

}
