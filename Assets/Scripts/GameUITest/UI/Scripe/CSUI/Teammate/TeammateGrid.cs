using UnityEngine;
using System.Collections;
using Pathea;

public class TeammateGrid : MonoBehaviour
{

    [SerializeField]
    UILabel mName;
    [SerializeField]
    UISlider mUISlider;

    [HideInInspector]
    public PlayerNetwork mPlayer;

    Vector3 mPos;

    public void SetInfo(PlayerNetwork _player)
    {
        mPlayer = _player;
		mName.text = _player.RoleName;
    }

    float maxHpTemp;
    void Update()
    {
        if (mPlayer != null && mPlayer.PlayerEntity != null)
        {
            //lz-2016.11.15 避免除数为0报错
            maxHpTemp = mPlayer.PlayerEntity.GetAttribute(AttribType.HpMax);
            mUISlider.sliderValue = maxHpTemp>0? mPlayer.PlayerEntity.GetAttribute(AttribType.Hp) / maxHpTemp:0;
            mPos = mPlayer.PlayerEntity.position;
        }
    }

    void OnTooltip(bool show)
    {
        if (!show)
        {
            ToolTipsMgr.ShowText(null);
            return;
        }

        string tipStr = "";
        tipStr = "Name:" + " " + mName.text + "\r\n" +"Position:" + " " + mPos.ToString();
        ToolTipsMgr.ShowText(tipStr);
    }
}
