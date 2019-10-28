using UnityEngine;
using System.Collections;
using Pathea;

namespace TrainingScene
{
	public class TrainingRoomLift : MonoBehaviour
	{
		void OnTriggerEnter(Collider other)
		{
			IKCmpt ik = other.transform.GetComponentInParent<IKCmpt>();
			if(null != ik)
				ik.ikEnable = false;
		}

		void OnTriggerExit(Collider other)
		{
			IKCmpt ik = other.transform.GetComponentInParent<IKCmpt>();
			if(null != ik)
				ik.ikEnable = true;
		}
	}
}