using UnityEngine;
using System.Collections;
#if UNITY_5
using MotionBlur = UnityStandardAssets.ImageEffects.MotionBlur;
#endif

public class PlayerShakeEffect : MonoBehaviour 
{
	public static bool bShaking = false;
//	float fOriginalFOV = 45;
//	float fShakeTime = 0;
	public float ShakeEffectLife = 0.25f;
//	float ShakeRange = 1.0f;
//	float ShakePos = 0;
//	float LastHP = -500;
	
//	PlayerDeathEffect GrayEffect = null;
	
	// Use this for initialization
	void Start () 
	{
		bShaking = false;
//		fOriginalFOV = 0;
//		fShakeTime = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
        //if ( PlayerFactory.mMainPlayer == null )
        //    return;
        //GrayEffect = GetComponent<PlayerDeathEffect>();
        //if ( GrayEffect != null )
        //{
        //    if ( !PlayerFactory.mMainPlayer.mDeath )
        //    {
        //        //if ( bShaking )
        //        //	GrayEffect.DisplayEffect(0.8f, 0.0f);
        //        //else
        //            GrayEffect.DisplayEffect(0.0f, 0.5f);
        //    }
        //    else
        //    {
        //        GrayEffect.DisplayEffect(1.0f, 0.3f);
        //    }
        //}
		
        //float NowHP = PlayerFactory.mMainPlayer.GetAttribute(Pathea.AttribType.Hp);
        //if ( NowHP < LastHP && NowHP/PlayerFactory.mMainPlayer.GetAttribute(Pathea.AttribType.HpMax) < 0.99f && !PlayerFactory.mMainPlayer.mDeath )
        //{
        //    ShakeRange = Mathf.Lerp( 0.3f, 4.0f, (LastHP - NowHP)/PlayerFactory.mMainPlayer.GetAttribute(Pathea.AttribType.HpMax) );
        //    ShakeRange = Mathf.Clamp( ShakeRange, 0.3f, 4.0f );
        //    Shake();
        //}
	
        //LastHP = PlayerFactory.mMainPlayer.GetAttribute(Pathea.AttribType.Hp);
		
        //MotionBlur mb = gameObject.GetComponent<MotionBlur>();
        //if ( mb == null )
        //    return;
        //mb.blurAmount = 0.4f;
		
        //if ( bShaking )
        //{
        //    ShakePos = ShakeRange * ( Mathf.Cos(2.0f*Mathf.PI*(fShakeTime/ShakeEffectLife)) );
        //    camera.fov = fOriginalFOV + ShakePos;
        //    fShakeTime += Time.deltaTime;
        //    if ( fShakeTime > ShakeEffectLife )
        //    {
        //        bShaking = false;
        //        mb.enabled = false;
        //        fShakeTime = 0;
        //        camera.fov = fOriginalFOV;
        //        fOriginalFOV = 0;
        //    }
        //}
	}
	
	void Shake()
	{
//		if ( !bShaking )
//			fOriginalFOV = GetComponent<Camera>().fieldOfView;
		bShaking = true;
//		fShakeTime = 0;
		MotionBlur mb = gameObject.GetComponent<MotionBlur>();
		if ( mb == null )
			return;
		mb.enabled = true;
	}
}
