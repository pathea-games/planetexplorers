using UnityEngine;
using System.Collections;

public class LockShadowDistance : MonoBehaviour {
	[SerializeField] float LockShadowDist = 80f;
	float tempShadowDist;
	void Update()
	{
		if(QualitySettings.shadowDistance != LockShadowDist)
		{
			tempShadowDist = QualitySettings.shadowDistance;
			QualitySettings.shadowDistance = LockShadowDist;
		}
	}
	void OnDestroy()
	{
		QualitySettings.shadowDistance = tempShadowDist;
	}
}
