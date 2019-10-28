using UnityEngine;
using System.Collections;

public class UIMissionGoalItem : MonoBehaviour
{
    [SerializeField] UITable goalItemRoot;
    [SerializeField] UILabel goalItemTextLb;
    [SerializeField] UISprite goalIconSprite;

    [SerializeField] GameObject goalBoolRoot;
    [SerializeField] UILabel goalBoolTextLb;
    [SerializeField] UICheckbox goalBoolCb;

    public Color textColor
    {
        get
        {
            return goalItemTextLb.color;
        }

        set
        {
            goalItemTextLb.color = value;
            goalBoolTextLb.color = value;
        }
    }

    public int index;

    public string itemText { get { return goalItemTextLb.text; } set { goalItemTextLb.text = value; } }

    // Help Value
    public int value0;
    public int value1;

    
    public void SetItemContent (string text, string sprite_name)
    {
        if (!goalItemRoot.gameObject.activeSelf)
            goalItemRoot.gameObject.SetActive(true);

        goalItemTextLb.text = text;
        if (!goalIconSprite.gameObject.activeSelf)
            goalIconSprite.gameObject.SetActive(true);
        goalIconSprite.spriteName = sprite_name;

        if (goalBoolRoot.activeSelf)
            goalBoolRoot.SetActive(false);

        goalItemRoot.Reposition();
    }

    public void SetItemContent (string text)
    {
        if (!goalItemRoot.gameObject.activeSelf)
            goalItemRoot.gameObject.SetActive(true);

        goalItemTextLb.text = text;

        if (goalIconSprite.gameObject.activeSelf)
             goalIconSprite.gameObject.SetActive(false);

        if (goalBoolRoot.activeSelf)
            goalBoolRoot.SetActive(false);

        goalItemRoot.Reposition();
    }

    public void SetBoolContent (string text, bool is_achived)
    {
        if (!goalBoolRoot.activeSelf)
            goalBoolRoot.SetActive(true);

        goalBoolTextLb.text = text;
        goalBoolCb.isChecked = is_achived;

        if (goalItemRoot.gameObject.activeSelf)
            goalItemRoot.gameObject.SetActive(false);
    }
}
