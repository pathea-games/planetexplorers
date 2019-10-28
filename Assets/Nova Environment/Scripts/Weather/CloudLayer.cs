using UnityEngine;
using System.Collections;

namespace NovaEnv
{
	public class CloudLayer : MonoBehaviour
	{
		int layerIndex = 0;
		public int LayerIndex
		{
			get { return layerIndex; }
			set
			{
				layerIndex = value;
				LayerMat.renderQueue = 2300 - layerIndex;
				transform.localPosition = (Executor.Settings.CloudHeight + layerIndex) * Vector3.up;
				transform.localScale = new Vector3(2,3,2) * Executor.Settings.CloudArea;
				CloudOffset = (layerIndex+5)*60 * new Vector3(0.1f, 0.1f, 0.1f);
			}
		}
		Executor executor;
		public Executor Executor
		{
			get { return executor; }
			set
			{
				executor = value;
				gameObject.AddComponent<MeshFilter>().sharedMesh = Executor.Settings.CloudLayerModel.GetComponent<MeshFilter>().sharedMesh;
				LayerMat = new Material(Executor.Settings.CloudShader);
				LayerMat.SetTexture("_NoiseTexture", Executor.NoiseTexture);
				gameObject.AddComponent<MeshRenderer>().material = LayerMat;
			} 
		}

		GameObject LayerModel;
		public Material LayerMat;

		Vector3 CloudOffset = Vector3.zero;

		public Color Color1;
		public Color Color2;
		public Color Color3;
		public Color Color4;

		void OnDestroy ()
		{
			Material.Destroy(LayerMat);
		}

		void Update ()
		{
			LayerMat.SetColor("_CloudColor1", Color1);
			LayerMat.SetColor("_CloudColor2", Color2);
			LayerMat.SetColor("_CloudColor3", Color3);
			LayerMat.SetColor("_CloudColor4", Color4);
			LayerMat.SetVector("_SunDirection", Executor.SunDirection);
			Vector3 cloudSpeed = Executor.Wind.WindDirection;
			cloudSpeed.y = cloudSpeed.z;
			cloudSpeed.z = 0;
			cloudSpeed *= 0.02f;
			cloudSpeed.z = cloudSpeed.magnitude * 1.2f;

			CloudOffset += Mathf.Sqrt((float)executor.Settings.TimeElapseSpeed) * 0.5f * Time.deltaTime * cloudSpeed;

			LayerMat.SetVector("_CloudOffset", CloudOffset);
		}
	}
}
