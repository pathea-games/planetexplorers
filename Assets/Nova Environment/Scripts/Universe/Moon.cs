using UnityEngine;
using System.Collections;

namespace NovaEnv
{
	public class Moon : MonoBehaviour
	{
		Executor executor;
		public Executor Executor
		{
			get { return executor; }
			set
			{
				executor = value;
				Utils.DestroyChild(this.gameObject);
				MoonParam param = executor.MoonsSettings[Index];
				gameObject.name = param.Name;
				Size = param.Size;
				LightIntensity = param.LightIntensity;
				LightColor = param.LightColor;
				Period = param.Period;
				Phi = param.Phi;
				Obliquity = param.Obliquity;

				MoonMat = new Material (executor.Settings.MoonBodyShader);
				MoonMat.SetTexture("_MainTexture", param.MainTex);
				MoonMat.SetTexture("_NormalTexture", param.BumpTex);
				MoonMat.SetVector("_MoonRect", new Vector4 (param.MoonTexRect.x, param.MoonTexRect.y, param.MoonTexRect.xMax, param.MoonTexRect.yMax));
				MoonMat.SetColor("_TintColor", param.TintColor);
				MoonMat.renderQueue = 1000 + executor.MoonsSettings.Length - Index + 1;

				GameObject body = Utils.CreateGameObject(PrimitiveType.Quad, "MoonBody", transform);
				body.transform.localPosition = Vector3.forward * executor.Settings.SkySize * 
					(0.94f + 0.05f*(float)(Index)/(float)(executor.MoonsSettings.Length));
				body.transform.localScale = Vector3.one * Size * 200;
				body.GetComponent<Renderer>().material = MoonMat;
				MoonBody = body;

				MoonLight = Utils.CreateGameObject(null, "MoonLight", transform).AddComponent<Light>();
				MoonLight.type = LightType.Directional;
				MoonLight.transform.localEulerAngles = new Vector3(0,180,0);
				MoonLight.transform.localPosition = new Vector3 (body.transform.localPosition.x, 
				                                                 body.transform.localPosition.y, 
				                                                 body.transform.localPosition.z - 1);
				MoonLight.color = LightColor;
				MoonLight.intensity = LightIntensity;
				MoonLight.shadows = LightShadows.None;
				MoonLight.cullingMask = executor.Settings.LightCullingMask;
				Tick();
			}
		}

		public int Index;
		[SerializeField] float Size;
		[HideInInspector] public Light MoonLight;
		[SerializeField] float LightIntensity;
		[SerializeField] Color LightColor;
		[SerializeField] double Period;
		[SerializeField] double Phi;
		[SerializeField] float Obliquity;
		[SerializeField] float UCA;
		[SerializeField] float LCA;
		[SerializeField] float DayPhase;

		Material MoonMat;
		GameObject MoonBody;

		public void OnDestroy ()
		{
			Material.Destroy(MoonMat);
		}
		
		public void Tick ()
		{
			MoonLight.enabled = Executor.Settings.MoonLightEnable;

			Vector3 forward = _getDir();
			Vector3 right = (_getDir(720.0f) - forward).normalized;
			Vector3 up = Vector3.Cross(forward, right).normalized;

			Debug.DrawLine(MoonBody.transform.position, MoonBody.transform.position + right*Size*300, Color.red);
			Debug.DrawLine(MoonBody.transform.position, MoonBody.transform.position + up*Size*300, Color.green);

			Quaternion moon_quat = Quaternion.identity;
			moon_quat.SetLookRotation(forward, up);

			if (Executor.Settings.MainCamera != null)
			{
				Quaternion body_quat = Quaternion.identity;
				body_quat.SetLookRotation(Executor.Settings.MainCamera.transform.forward, up);
				MoonBody.transform.rotation = body_quat;
			}

			transform.localRotation = moon_quat;
		}

		Vector3 _getDir (float timeofs = 0)
		{
			double sun_period = Executor.SunSettings.Period;
			float local_upper_latitude = Utils.NormalizeDegree(Executor.Settings.LocalLatitude);
			float local_lower_latitude = Utils.NormalizeDegree(180.0f - Executor.Settings.LocalLatitude);
			double moon_phase_period = sun_period / (sun_period / Period - 1.0);
			float moon_phase = (float)((Executor.UTC + timeofs - Phi) / moon_phase_period);
			moon_phase = moon_phase - Mathf.Floor(moon_phase);
			
			float new_moon_latitude = Obliquity - Executor.SunSettings.Obliquity;
			float full_moon_latitude = -new_moon_latitude;
			
			float t = (1 - Mathf.Cos((float)(moon_phase + Executor.SunYear/18.6 + 0.25) * 2f * Mathf.PI)) * 0.5F;
			float moon_latitude = Mathf.Lerp(new_moon_latitude, full_moon_latitude, t);
			
			UCA = Utils.NormalizeDegree(90 - (moon_latitude - local_upper_latitude));
			LCA = Utils.NormalizeDegree((moon_latitude - local_lower_latitude) - 270);
			
			Vector3 moon_uc_point = new Vector3( 0, Mathf.Sin(UCA * Mathf.Deg2Rad), Mathf.Cos(UCA * Mathf.Deg2Rad) );
			Vector3 moon_lc_point = new Vector3( 0, Mathf.Sin(LCA * Mathf.Deg2Rad), Mathf.Cos(LCA * Mathf.Deg2Rad) );
			
			float moon_xmax = Mathf.Sqrt(Mathf.Clamp01(1 - ((moon_uc_point + moon_lc_point) * .5F).sqrMagnitude));
			
			float moon_rad = (float)(Executor.FracSunDay + timeofs/Executor.Settings.SecondsPerDay - moon_phase) * 2.0F * Mathf.PI;
			Vector3 moon_dir = Vector3.Lerp(moon_uc_point, moon_lc_point, Mathf.Cos(moon_rad) * .5F + .5F);
			moon_dir.x = Mathf.Lerp(-moon_xmax, moon_xmax, Mathf.Sin(moon_rad) * .5F + .5F);
			moon_dir.Normalize();

			if (Mathf.Abs(timeofs) < 0.01f)
			{
				DayPhase = (float)(moon_phase * moon_phase_period / executor.Settings.SecondsPerDay);
				MoonMat.SetVector("_SunDir", transform.InverseTransformDirection(Executor.SunDirection));
				MoonMat.SetColor("_CurrSkyColor", Executor.Sky.SkyColorAtPoint(moon_dir, Executor.SunDirection));
				MoonMat.SetFloat("_Overcast", Executor.Sky.Overcast);

				moon_phase = DayPhase; // avoid warning
			}

			return moon_dir;
		}
	}
	
	// Ps:  +Z = North
}
