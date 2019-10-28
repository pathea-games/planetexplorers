using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RedGrass
{
	public abstract class DataIO<T> : MonoBehaviour 
		where T : class
	{
		public virtual void Init (EvniAsset evni)
		{
			mEvni = evni;
		}

		public abstract bool AddReqs (T req);

		public abstract bool AddReqs (List<T> reqs);

		public abstract void ClearReqs ();

		public abstract void StopProcess();

		public abstract void ProcessReqsImmediatly();

		public delegate void OnReqsFinished ();
		public abstract bool Active (OnReqsFinished call_back);

		public abstract void Deactive ();

		public abstract bool isActive ();

		protected EvniAsset mEvni;
	}
}

