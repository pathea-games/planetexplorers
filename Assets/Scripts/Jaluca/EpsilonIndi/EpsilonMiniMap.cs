using UnityEngine;
using System.Collections;

namespace TrainingScene
{
	public class EpsilonMiniMap : MonoBehaviour
	{
		public Texture trainingRoomMinimap;
		void Start()
		{
			UIMinMapCtrl.Instance.mPlayerPosText.gameObject.SetActive(false);
			UIMinMapCtrl.Instance.mTimeLabel.gameObject.SetActive(false);
			UIMinMapCtrl.Instance.mMiniMapTex.material.SetTexture("_MainTex", trainingRoomMinimap);
		}
		void Update()
		{
			//UIMinMapCtrl.Instance.mMinMapMaterial.SetTexture("_MainTex", trainingRoomMinimap);
		}
	}
}
