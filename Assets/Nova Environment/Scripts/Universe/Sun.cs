using UnityEngine;
using System.Collections;

namespace NovaEnv
{
	public class Sun : MonoBehaviour
	{
		Executor executor;
		public Executor Executor
		{
			get { return executor; }
			set
			{
				executor = value;
				Utils.DestroyChild(this.gameObject);
				SunParam param = executor.SunSettings;
				Obliquity = param.Obliquity;
				SunLight = Utils.CreateGameObject(null, "SunLight", transform).AddComponent<Light>();
				SunLight.type = LightType.Directional;
				SunLight.transform.localEulerAngles = new Vector3(0,180,0);
				SunLight.transform.localPosition = Vector3.forward * (executor.Settings.SkySize * 0.99f - 1);
				SunLight.shadows = LightShadows.Soft;
                SunLight.shadowBias = 0.2f;
				SunLight.cullingMask = executor.Settings.LightCullingMask;
				Tick();
			}
		}

		[HideInInspector] public Light SunLight;
		[SerializeField] float Obliquity;
		[SerializeField] float UCA;
		[SerializeField] float LCA;

		private bool _changeTrans = false;
		public Vector3 Direction;

		public void Tick ()
		{
			Vector3 forward = _getDir();
			Vector3 right = (_getDir(720.0f) - forward).normalized;
			Vector3 up = Vector3.Cross(forward, right).normalized;

			Debug.DrawLine(SunLight.transform.position, SunLight.transform.position + right*200, Color.red);
			Debug.DrawLine(SunLight.transform.position, SunLight.transform.position + up*200, Color.green);

			Direction = forward;

			Quaternion sun_quat = Quaternion.identity;
			sun_quat.SetLookRotation(forward, up);

			if ( Vector3.Angle(transform.forward, forward) > 0.3f )
				_changeTrans = true;
			if ( _changeTrans && Vector3.Angle(transform.forward, forward) < 0.01f )
				_changeTrans = false;
			Quaternion sunrot = transform.rotation;
			transform.rotation = sun_quat;
			Executor.SkySphereMat.SetVector("_StarAxisX", transform.right);
			Executor.SkySphereMat.SetVector("_StarAxisY", transform.up);
			Executor.SkySphereMat.SetVector("_StarAxisZ", transform.forward);
			Executor.SkySphereMat.SetVector("_SunPos", transform.forward);
			transform.rotation = sunrot;
			if (_changeTrans)
				transform.rotation = Quaternion.Slerp(transform.rotation, sun_quat, 0.3f);
		}

		public Vector3 _getDir (float timeofs = 0)
		{
			float sin_year_time = (float)System.Math.Sin(Executor.SunYear*6.28318530718);
			float sun_latitude = sin_year_time * Obliquity;
			float local_upper_latitude = Utils.NormalizeDegree(Executor.Settings.LocalLatitude);
			float local_lower_latitude = Utils.NormalizeDegree(180.0f - Executor.Settings.LocalLatitude);
			
			UCA = Utils.NormalizeDegree(90 - (sun_latitude - local_upper_latitude));
			LCA = Utils.NormalizeDegree((sun_latitude - local_lower_latitude) - 270);
			
			Vector3 sun_uc_point = new Vector3( 0, Mathf.Sin(UCA * Mathf.Deg2Rad), Mathf.Cos(UCA * Mathf.Deg2Rad) );
			Vector3 sun_lc_point = new Vector3( 0, Mathf.Sin(LCA * Mathf.Deg2Rad), Mathf.Cos(LCA * Mathf.Deg2Rad) );
			
			float sun_xmax = Mathf.Sqrt(Mathf.Clamp01(1 - ((sun_uc_point + sun_lc_point) * .5F).sqrMagnitude));
			
			float sun_rad = (float)(Executor.FracSunDay + timeofs/Executor.Settings.SecondsPerDay) * 2F * Mathf.PI;
			Vector3 sun_dir = Vector3.Lerp(sun_uc_point, sun_lc_point, Mathf.Cos(sun_rad) * .5F + .5F);
			sun_dir.x = Mathf.Lerp(-sun_xmax, sun_xmax, Mathf.Sin(sun_rad) * .5F + .5F);
			sun_dir.Normalize();

			return sun_dir;
		}
	}

	// Ps:  +Z = North
}
