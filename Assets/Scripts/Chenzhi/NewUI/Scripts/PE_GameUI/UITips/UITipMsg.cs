using UnityEngine;
using System.Collections;

public class UITipMsg : MonoBehaviour 
{
	public UILabel content;

	public UISprite icon;

    public UITexture tex;

    public int musicID;

    public void SetStyle(PeTipMsg.EStyle eStyle)
	{
        if (eStyle == PeTipMsg.EStyle.Text)
		{
			icon.gameObject.SetActive(false);
            tex.gameObject.SetActive(false);
			content.transform.localPosition = new Vector3(0, 0, 0);
		}
        else if (eStyle == PeTipMsg.EStyle.Icon)
		{
			icon.gameObject.SetActive(true);
            tex.gameObject.SetActive(false);
			content.transform.localPosition = new Vector3(40, 0, 0);
        }
        else if (eStyle == PeTipMsg.EStyle.Texture)
        {
            icon.gameObject.SetActive(false);
            tex.gameObject.SetActive(true);
            content.transform.localPosition = new Vector3(40, 0, 0);
        }
	}


	public Bounds GetBounds ()
	{
		Bounds finalBound = new Bounds();

		Bounds bound = NGUIMath.CalculateRelativeWidgetBounds(content.transform);
		Vector3 scale = content.transform.localScale;
		bound.min = Vector3.Scale(bound.min, scale);
		bound.max = Vector3.Scale(bound.max, scale);

		bound.center = content.transform.localPosition;

		finalBound.Encapsulate(bound);

		if (icon != null)
		{
			bound  = NGUIMath.CalculateRelativeWidgetBounds(icon.transform);
			scale = icon.transform.localScale;
			bound.min = Vector3.Scale(bound.min, scale);
			bound.max = Vector3.Scale(bound.max, scale);
			bound.center = icon.transform.localPosition;
			finalBound.Encapsulate(bound);
		}

		return finalBound;
	}

    public void Reset()
    {
        content.text = "";
        icon.spriteName = "";
        tex.mainTexture = null;
    }
}
