using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIBtnSoundMgr : MonoBehaviour
{

    [HideInInspector]
    public List<UIButtonSound> mBtnSndList;

    void Awake()
    {
        mBtnSndList = new List<UIButtonSound>();
    }
    // Use this for initialization
    void Start()
    {
        mBtnSndList.AddRange(gameObject.GetComponentsInChildren<UIButtonSound>(true));
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBtnSound();
    }

    void UpdateBtnSound()
    {
        if (SystemSettingData.Instance == null)
            return;
        foreach (UIButtonSound item in mBtnSndList)
        {
            item.volume = SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.EffectVolume;
        }
    }
}
