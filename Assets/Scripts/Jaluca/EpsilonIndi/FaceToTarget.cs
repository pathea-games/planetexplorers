using UnityEngine;
using System.Collections;

namespace EpsilonIndi
{
	public class FaceToTarget : MonoBehaviour 
	{
		[SerializeField] Transform target;	
		void Update()
		{
			transform.forward = (target.position - transform.position).normalized;
		}
	}

}
