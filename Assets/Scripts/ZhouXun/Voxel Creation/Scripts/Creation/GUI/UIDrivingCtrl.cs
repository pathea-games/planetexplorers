using UnityEngine;
using System.Collections;
using Pathea;
using WhiteCat;
using System;
using WhiteCat.BitwiseOperationExtension;

public class UIDrivingCtrl : MonoBehaviour 
{
	[SerializeField] UILabel durabilityLabel;
	[SerializeField] UISlider durabilitySlider;

	[SerializeField] UILabel energyLabel;
	[SerializeField] UISlider energySlider;

	[SerializeField] UILabel speedLabel;
	[SerializeField] UISlider speedSlider;

	[SerializeField] UILabel jetExhaustLabel;
	[SerializeField] UISlider jetExhaustSlider;

	[SerializeField] GameObject[] weaponTogglesOn;
	[SerializeField] GameObject[] weaponTogglesOff;

	[SerializeField] TweenInterpolator interpolator;
	[SerializeField] TweenInterpolator attackModeAnim;
	[SerializeField] float updateInterval = 0.1f;

    [SerializeField] TweenInterpolator driveHelpTween;
    [SerializeField] Transform driveHelpContentTrans;
    [SerializeField] UILabel driveHelpLbl;
    [SerializeField] UIButton driveHelpBtn;


    private static UIDrivingCtrl mInstance;
	public static UIDrivingCtrl Instance
	{
		get { return mInstance; }
	}


	void Awake()
	{
		mInstance = this;
        UIEventListener.Get(driveHelpBtn.gameObject).onClick += OnTipsBtnClick;
        InitDriveHelpState();
    }

    void OnDestroy()
    {
		if(null != driveHelpBtn)
	        UIEventListener.Get(driveHelpBtn.gameObject).onClick -= OnTipsBtnClick;
    }


	Func<float> maxDurability;
	Func<float> durability;
	Func<float> maxEnergy;
	Func<float> energy;
	Func<float> speed;
	Func<float> jetExhaust;

	int lastDurability = 0;
	int lastDurabilityPermillage = 0;
	int lastEnergy = 0;
	int lastEnergyPermillage = 0;
	int lastSpeedx10 = 0;
	int lastJetExhaustPermillage = 0;
//	int lastHeight = 0;

	float nextUpdateTime = -1f;


	public void Show(
		Func<float> maxDurability,
		Func<float> durability,
		Func<float> maxEnergy,
		Func<float> energy,
		Func<float> speed,
		Func<float> jetExhaust)
	{
		this.maxDurability = maxDurability;
		this.durability = durability;
		this.maxEnergy = maxEnergy;
		this.energy = energy;
		this.speed = speed;
		this.jetExhaust = jetExhaust;

		Update();
		gameObject.SetActive(true);

		interpolator.speed = 1f;
		interpolator.isPlaying = true;

        UpdateDriveHelpContent();
        driveHelpTween.speed = 1f;
        driveHelpTween.isPlaying = true;
    }

    public bool IsShow { get { return gameObject.activeInHierarchy; } }

	public void Hide()
	{
		nextUpdateTime = -1f;
		interpolator.speed = -1;
		interpolator.isPlaying = true;

        driveHelpTween.speed = -1f;
        driveHelpTween.isPlaying = true;
    }


	public void SetWweaponGroupTogglesVisible(bool visible, BehaviourController controller)
	{
		attackModeAnim.speed = visible ? 1f : -1f;

		for(int i=0; i< weaponTogglesOn.Length;i++)
		{
			SetWweaponGroupToggles(i, controller.IsWeaponGroupEnabled(i));
        }
	}


	public void SetWweaponGroupToggles(int index, bool on)
	{
		weaponTogglesOn[index].SetActive(on);
		weaponTogglesOff[index].SetActive(!on);
	}


	void Update()
	{
		try
		{
			if(Time.timeSinceLevelLoad >= nextUpdateTime)
			{
				nextUpdateTime = Time.timeSinceLevelLoad + updateInterval;

				int tempInt;
				float tempFloat;

				tempFloat = durability();
				tempInt = Mathf.CeilToInt(tempFloat);
				if(tempInt != lastDurability)
				{
					lastDurability = tempInt;
					durabilityLabel.text = tempInt.ToString();
				}

				tempFloat /= maxDurability();
				tempInt = Mathf.CeilToInt(tempFloat * 1000f);
				if(tempInt != lastDurabilityPermillage)
				{
					lastDurabilityPermillage = tempInt;
					durabilitySlider.sliderValue = tempInt * 0.001f;
				}

				tempFloat = energy();
				tempInt = Mathf.CeilToInt(tempFloat);
				if(tempInt != lastEnergy)
				{
					lastEnergy = tempInt;
					energyLabel.text = tempInt.ToString();
				}

				tempFloat /= maxEnergy();
				tempInt = Mathf.CeilToInt(tempFloat * 1000f);
				if(tempInt != lastEnergyPermillage)
				{
					lastEnergyPermillage = tempInt;
					energySlider.sliderValue = tempInt * 0.001f;
				}

				tempFloat = speed();
				tempInt = Mathf.RoundToInt(tempFloat * 10f);
				if(tempInt != lastSpeedx10)
				{
					lastSpeedx10 = tempInt;
					speedLabel.text = (tempInt * 0.1f).ToString("0.0");
					speedSlider.sliderValue = tempInt / PEVCConfig.instance.maxRigidbodySpeed / 36f;
				}

				tempFloat = jetExhaust==null ? 0 : jetExhaust();
				tempInt = Mathf.CeilToInt(tempFloat * 1000f);
				if(tempInt != lastJetExhaustPermillage)
				{
					lastJetExhaustPermillage = tempInt;
					jetExhaustSlider.sliderValue = tempInt * 0.001f;
					jetExhaustLabel.text = Mathf.CeilToInt(tempFloat * 100f).ToString();
				}
			}
        }
		catch{}
	}

    #region drivehelp methods

    bool _helpIsShow = true;
    const string SAVESHOWTIPKEY = "UIDriveTips";

    void InitDriveHelpState()
    {
        if (null != UIRecentDataMgr.Instance)
        {
            driveHelpIsShow = (UIRecentDataMgr.Instance.GetIntValue(SAVESHOWTIPKEY, _helpIsShow ? 1 : 0) > 0 ? true : false);
        }
    }
    
    bool driveHelpIsShow
    {
        get { return _helpIsShow; }
        set
        {
            if (_helpIsShow != value)
            {
                _helpIsShow = value;
                if (null != UIRecentDataMgr.Instance)
                {
                    UIRecentDataMgr.Instance.SetIntValue(SAVESHOWTIPKEY, _helpIsShow ? 1 : 0);
                }
            }
            driveHelpBtn.transform.localRotation = _helpIsShow ? Quaternion.Euler(Vector3.one) : Quaternion.Euler(new Vector3(0, 0, 180));
            driveHelpContentTrans.gameObject.SetActive(_helpIsShow);
        }
    }
    private void OnTipsBtnClick(GameObject go)
    {
        driveHelpIsShow = !driveHelpIsShow;
    }

    private void UpdateDriveHelpContent()
    {
        string content = PELocalization.GetString(82201084);
        if (string.IsNullOrEmpty(content))
            return;

        content =content.Replace("$W$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MoveForward).ToString());
        content = content.Replace("$S$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MoveBackward).ToString());
        content = content.Replace("$A$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MoveLeft).ToString());
        content = content.Replace("$D$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MoveRight).ToString());

        content = content.Replace("$E$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.InteractWithItem).ToString());
        content = content.Replace("$L$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.SwitchLight).ToString());
        content = content.Replace("$F$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.Vehicle_AttackModeOnOff).ToString());

        content = content.Replace("$Space$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.Vehicle_LiftUp).ToString());
        content = content.Replace("$Alt$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.Vehicle_LiftDown).ToString());

        content = content.Replace("$F1$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.VehicleWeaponGrp1).ToString());
        content = content.Replace("$F2$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.VehicleWeaponGrp2).ToString());
        content = content.Replace("$F3$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.VehicleWeaponGrp3).ToString());
        content = content.Replace("$F4$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.VehicleWeaponGrp4).ToString());

        content = content.Replace("$Shift$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.Vehicle_Sprint).ToString());

        content = content.Replace("$Z$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MissleTarget).ToString());
        content = content.Replace("$X$", PeInput.GetKeyCodeByLogicFunKey(PeInput.LogicFunction.MissleLaunch).ToString());

        driveHelpLbl.text = content;
        driveHelpLbl.MakePixelPerfect();
    }
    #endregion
}
