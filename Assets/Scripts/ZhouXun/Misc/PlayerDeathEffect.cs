using UnityEngine;
using System.Collections;
#if UNITY_5
using GrayscaleEffect = UnityStandardAssets.ImageEffects.Grayscale;
using BloomAndLensFlares = UnityStandardAssets.ImageEffects.BloomAndFlares;
#endif

public class PlayerDeathEffect : MonoBehaviour 
{
	float GrayScale = 0.0f;
	float GrayScaleWanted = 0.0f;
	float BlurThreshold = 0.5f;
	float BlurThresholdWanted = 0.5f;
	public float _Dumper = 0.025f;
	
	
	// Use this for initialization
	void Start () 
	{
		GrayScale = 0;
		GrayScaleWanted = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
        //if ( PlayerFactory.mMainPlayer == null )
        //    return;
				
		if ( Mathf.Abs( GrayScale - GrayScaleWanted ) < 0.001f )
			GrayScale = GrayScaleWanted;
		else
			GrayScale += (GrayScaleWanted - GrayScale)*_Dumper;
		
		if ( Mathf.Abs( BlurThreshold - BlurThresholdWanted ) < 0.001f )
			BlurThreshold = BlurThresholdWanted;
		else
			BlurThreshold += (BlurThresholdWanted - BlurThreshold)*_Dumper;

		GrayscaleEffect gs = gameObject.GetComponent<GrayscaleEffect>();
		BloomAndLensFlares bf = gameObject.GetComponent<BloomAndLensFlares>();
		if ( gs == null )
			return;
		if ( GrayScale < 0.001f )
		{
#if UNITY_5
			gs.rampOffset = 0;
#else
			gs.grayAmount = 0;
#endif
			gs.enabled = false;
		}
		else
		{
			gs.enabled = true;
#if UNITY_5
			gs.rampOffset = GrayScale;
#else
			gs.grayAmount = GrayScale;
#endif
		}
		if ( bf == null )
			return;
		if ( bf.enabled )
		{
#if UNITY_5
			bf.bloomThreshold = BlurThreshold;
#else
			bf.bloomThreshhold = BlurThreshold;
#endif
		}
	}
	
	public void DisplayEffect( float grayscalewanted, float blurwanted )
	{
		GrayScaleWanted = grayscalewanted;
		BlurThresholdWanted = blurwanted;
	}
}
