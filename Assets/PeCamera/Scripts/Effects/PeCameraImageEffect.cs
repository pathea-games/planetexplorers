using UnityEngine;
using System.Collections;

public class PeCameraImageEffect : MonoBehaviour
{
	private static PeCameraImageEffect inst = null;

	[Header("Resource References")]
	public Material DamageMat;
	public Material FoodPoisonMat;
	public Material InjuredPoisonMat;
	public Material GRVInfestMat;
	public Material DizzyMat;
	public Material ScreenDirtMat;
	public Material ScreenMaskMat;

	public UnityStandardAssets.ImageEffects.Grayscale grayScale;
	public UnityStandardAssets.ImageEffects.MotionBlur motionBlur;
	public UnityStandardAssets.ImageEffects.BloomOptimized bloom;

	[Header("Damage")]
	public float Scale = 0;
	public float hp = 1;
	private float damageIntensity = 0;
	private float t = 0;
	private float v = 0;
	private float damageIntensityTarget = 0;
	public static void PlayHitEffect (float hpDrop)
	{
		if (hpDrop > 1)
		{
			inst.Hit(hpDrop);
			PeCamera.PlayShakeEffect(0, 0.2f, 0);
		}
	}

	public bool testHit = false;

	bool registed = false;

	Pathea.SkAliveEntity mainPlayerAliveEntity;

	[Header("Poison")]
	[Range(0,1)][SerializeField]
	float foodPoisonStrength = 0;
	float foodPoisonStrengthCurrent = 0;
	public static void SetFoodPoisonStrength (float s)
	{
		inst.foodPoisonStrength = Mathf.Clamp01(s);
	}

	[Range(0,1)][SerializeField]
	float injuredPoisonStrength = 0;
	float injuredPoisonStrengthCurrent = 0;
	public static void SetInjuredPoisonStrength (float s)
	{
		inst.injuredPoisonStrength = Mathf.Clamp01(s);
	}
	[SerializeField] float poisonMaxIntensityAtDaytime = 0.7f;
	[SerializeField] float poisonMaxIntensityAtNight = 0.35f;

	[Header("Dizzy")]
	[Range(0,1)][SerializeField]
	float dizzyStrength = 0;
	[SerializeField]
	float dizzyMaxDistortion = 0.01f;
	float dizzyStrengthCurrent = 0;
	public static void SetDizzyStrength (float s)
	{
		inst.dizzyStrength = Mathf.Clamp01(s);
	}

	[Header("GRV")]
	[Range(0,1)][SerializeField]
	float grvInfestStrength = 0;
	[SerializeField] float grvInfestEffectDuration = 5;
	[SerializeField] AnimationCurve grvInfestEffectIntensity;
	float grvInfestEffectTime = 0;
	float grvInfestEffectTimeSpeed = 0;
	[SerializeField] float grvMaxDistortion = 0.007f;
	[SerializeField] float GRVEffectCDDuration = 10f;
	float GRVCDTime;
	public static void SetGRVInfestStrength (float s)
	{
		inst.grvInfestStrength = Mathf.Clamp01(s);
	}
	
	[Header("Flashlight")]
	[SerializeField]
	bool triggerFlashlight = false;
	[SerializeField] float FlashStrength = 1;
	float flashlightTime = 0;
	float flashlightTimeSpeed = 0;
	float flashBloomStrength = 0;
	float flashBlurStrength = 0;
	[SerializeField] float flashlightDuration = 10;
	[SerializeField] AnimationCurve bloomStrengthAnimation;
	[SerializeField] AnimationCurve blurStrengthAnimation;
	public static void FlashlightExplode (float flashStrength = 1)
	{
		inst.FlashStrength = flashStrength;
		inst.flashlightTime = inst.flashlightDuration;
		inst.flashlightTimeSpeed = -1;
	}

	//[Header("Screen Dirt")]
	//[SerializeField]
	//Texture2D[] dirtTextures = new Texture2D[0] ;
	//[SerializeField]
	//int sprayDirtIndex = 0;
	//bool sprayDirt = false;
	//[SerializeField] float dirtMaxIntensityAtDaytime = 0.7f;
	//[SerializeField] float dirtMaxIntensityAtNight = 0.15f;
	public static void SprayDirtToScreen (int dirtIndex)
	{
		
	}
	
	[Header("Screen Mask")]
	[SerializeField]
	Texture2D[] maskTextures = new Texture2D[0] ;
	[SerializeField] AnimationCurve screenMaskAnimation;
	[SerializeField] float screenMaskDuration = 10f;
	//int maskIndex = 0;
	bool maskenabled = false;
	float screenMaskTime;

	public bool TestMask;

	public static void ScreenMask (int maskIndex, bool show = true, float duration = 10f)
	{
		if(inst.maskTextures.Length > maskIndex)
		{
			inst.ScreenMaskMat.mainTexture = inst.maskTextures[maskIndex];
			inst.screenMaskTime = 0;
			//inst.maskIndex = maskIndex;
			inst.maskenabled = show;
			inst.screenMaskDuration = duration;
		}
	}
	
	// Use this for initialization
	void Awake ()
	{
		inst = this;
	}

	void OnDestroy ()
	{
		inst = null;
	}

	void Start ()
	{
		DamageMat = Material.Instantiate(DamageMat) as Material;
		FoodPoisonMat = Material.Instantiate(FoodPoisonMat) as Material;
		InjuredPoisonMat = Material.Instantiate(InjuredPoisonMat) as Material;
		GRVInfestMat = Material.Instantiate(GRVInfestMat) as Material;
		DizzyMat = Material.Instantiate(DizzyMat) as Material;
		ScreenDirtMat = Material.Instantiate(ScreenDirtMat) as Material;
		ScreenMaskMat = Material.Instantiate(ScreenMaskMat) as Material;
	}

	void TryRegister ()
	{
		if (mainPlayerAliveEntity != null)
		{
			hp = mainPlayerAliveEntity.HPPercent;
			grayScale.saturate = Mathf.Lerp(grayScale.saturate, hp < 0.000001f ? 0f : 1f, 0.02f);
		}

		if (registed)
			return;

		if (Pathea.PeCreature.Instance == null)
			return;

		if (Pathea.PeCreature.Instance.mainPlayer == null)
			return;

		mainPlayerAliveEntity = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.SkAliveEntity>();

		if (mainPlayerAliveEntity == null)
			return;

		mainPlayerAliveEntity.onHpChange += HandleAliveEntityHpChange;

		registed = true;
	}

	void HandleAliveEntityHpChange (SkillSystem.SkEntity caster, float hpChange)
	{
		if (hpChange < 0)
		{
			Hit(-hpChange);
			PeCamera.PlayShakeEffect(0, 0.2f, 0);
		}
	}
		
	// Update is called once per frame
	void Update ()
	{
		float warning_bias = Mathf.Sin(t*10f) * 0.2f + 0.8f;
		float daynightlerpt = (float)GameTime.Timer.CycleInDay * 0.5f + 0.5f;

		// Normal Damage
		if (testHit)
		{
			testHit = false;
			Hit(50f);
		}
		TryRegister();
		damageIntensity = Mathf.SmoothDamp(damageIntensity, damageIntensityTarget, ref v, 0.16f);
		if (damageIntensity > damageIntensityTarget * 0.75f)
		{ 
			damageIntensityTarget = Mathf.Lerp(damageIntensityTarget, Mathf.Pow(Mathf.Clamp01(0.4f-hp), 2) * 2.5f, 0.15f);
		}
		t += Time.deltaTime;

		float final = Scale * damageIntensity * warning_bias * (daynightlerpt * 0.5f + 0.5f);
		DamageMat.SetFloat("_Intensity", final);

		// Poison
		if (foodPoisonStrength > injuredPoisonStrength - 0.001f)
		{
			foodPoisonStrengthCurrent = Mathf.Lerp(foodPoisonStrengthCurrent, foodPoisonStrength, 0.1f);
			injuredPoisonStrengthCurrent = Mathf.Lerp(injuredPoisonStrengthCurrent, 0f, 0.1f);
		}
		else
		{
			foodPoisonStrengthCurrent = Mathf.Lerp(foodPoisonStrengthCurrent, 0f, 0.1f);
			injuredPoisonStrengthCurrent = Mathf.Lerp(injuredPoisonStrengthCurrent, injuredPoisonStrength, 0.1f);
		}

		float poison_daynight = Mathf.Lerp(poisonMaxIntensityAtNight, poisonMaxIntensityAtDaytime, daynightlerpt);
		FoodPoisonMat.SetFloat("_Intensity", foodPoisonStrengthCurrent * warning_bias * poison_daynight);
		InjuredPoisonMat.SetFloat("_Intensity", injuredPoisonStrengthCurrent * warning_bias * poison_daynight);

		// Dizzy
		dizzyStrengthCurrent = Mathf.Lerp(dizzyStrengthCurrent, dizzyStrength, 0.02f);
		DizzyMat.SetFloat("_DistortionStrength", dizzyStrengthCurrent * warning_bias * dizzyMaxDistortion);
		DizzyMat.SetFloat("_Speed", dizzyStrengthCurrent * 0.03f);
		float dizzy_blurstrength = dizzyStrengthCurrent * 0.8f;

		// GRV
		float p = grvInfestStrength * 0.005f - 0.00001f;
		if (Random.value < p && GRVCDTime > GRVEffectCDDuration)
		{
			grvInfestEffectTime = 0;
			grvInfestEffectTimeSpeed = 1;
		}

		float grv_intens = Mathf.Clamp01(grvInfestEffectIntensity.Evaluate(grvInfestEffectTime / grvInfestEffectDuration));
		grv_intens *= grvInfestStrength;
		float grv_blurstrength = grv_intens * 0.75f;
		grv_intens *= grvMaxDistortion;
		grv_intens *= warning_bias;

		grvInfestEffectTime += Time.deltaTime * grvInfestEffectTimeSpeed;
		GRVCDTime += Time.deltaTime;
		if (grvInfestEffectTime > grvInfestEffectDuration)
		{
			grvInfestEffectTime = 0;
			grvInfestEffectTimeSpeed = 0;
			GRVCDTime = 0;
		}
		GRVInfestMat.SetFloat("_DistortionStrength", grv_intens);


		// Flashlight
		if (triggerFlashlight)
		{
			FlashlightExplode();
			triggerFlashlight = false;
		}

		float bloomtar = bloomStrengthAnimation.Evaluate(flashlightTime / flashlightDuration);
		float blurtar = blurStrengthAnimation.Evaluate(flashlightTime / flashlightDuration);
		flashBloomStrength = Mathf.Lerp(flashBloomStrength, bloomtar, 0.4f);
		flashBlurStrength = Mathf.Lerp(flashBlurStrength, blurtar, 0.4f);

		flashlightTime += flashlightTimeSpeed * Time.deltaTime;
		if (flashlightTime < 0)
		{
			flashlightTime = 0; 
			flashlightTimeSpeed = 0;
		}

		float bloom_intens = bloomtar * FlashStrength;
		float blur_amount = Mathf.Max (dizzy_blurstrength, grv_blurstrength, flashBlurStrength) * FlashStrength;

		bloom.enabled = bloom_intens > 0.01f;
		motionBlur.enabled = blur_amount > 0.05f;

		bloom.intensity = bloom_intens;
		motionBlur.blurAmount = blur_amount;

		if(maskenabled && screenMaskDuration > PETools.PEMath.Epsilon)
		{
			screenMaskTime += Time.deltaTime;
			if(screenMaskTime > screenMaskDuration)
			{
				maskenabled = false;
				screenMaskTime = screenMaskDuration;
			}
			ScreenMaskMat.SetFloat("_Intensity", screenMaskAnimation.Evaluate(screenMaskTime/screenMaskDuration));
		}

		if(TestMask)
		{
			TestMask = false;
			ScreenMask(0);
		}
	}

	public void Hit (float amount)
	{
		amount /= 150f;
		amount = Mathf.Clamp01(amount);

		damageIntensityTarget = damageIntensity + amount;
		if (damageIntensity < 0.01f)
			t = 0;
	}

	void OnPostRender ()
	{
		if (damageIntensity > 0.01f)
			DrawGLQuad(DamageMat);

		if (foodPoisonStrengthCurrent > 0.005f)
			DrawGLQuad(FoodPoisonMat);

		if (injuredPoisonStrengthCurrent > 0.005f)
			DrawGLQuad(InjuredPoisonMat);

		if (dizzyStrengthCurrent > 0.005f)
			DrawGLQuad(DizzyMat);

		if (grvInfestEffectTime > 0.005f)
			DrawGLQuad(GRVInfestMat);

		if (maskenabled)
			DrawGLQuad(ScreenMaskMat);
	}

	void DrawGLQuad (Material mat)
	{
		for (int i = 0; i < mat.passCount; ++i)
		{
			mat.SetPass(i);

			GL.PushMatrix();
			GL.LoadOrtho();
			GL.Color(Color.white);
			GL.Begin(GL.QUADS);
			GL.TexCoord2(0f, 0f);
			GL.Vertex3(0f, 0f, 0f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(-0f, 1f, 0f);
			GL.TexCoord2(1f, 1f);
			GL.Vertex3(1f, 1f, 0f);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(1f, 0f, 0f);
			GL.End();
			GL.PopMatrix();
		}
	}
}
