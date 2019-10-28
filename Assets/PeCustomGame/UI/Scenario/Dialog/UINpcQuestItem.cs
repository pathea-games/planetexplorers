using UnityEngine;
using System;

public class UINpcQuestItem : MonoBehaviour
{
    [SerializeField] UILabel textLabel;
    [SerializeField] UISprite titleIcon;

    public int index;

    public Action<UINpcQuestItem> onClick;

    public string test
    {
        get
        {
            return textLabel.text;
        }

        set
        {
            textLabel.text = value;
        }
    }

    void OnBtnClick ()
    {
        if (onClick != null)
            onClick(this);
    }
}
