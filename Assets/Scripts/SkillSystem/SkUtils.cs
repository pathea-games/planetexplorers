using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SkillSystem
{
	public class CoroutineStoppable : IEnumerator
	{
		public bool stop = false;
		IEnumerator enumerator;
        //MonoBehaviour behaviour;
        //Coroutine coroutine;

        public CoroutineStoppable(MonoBehaviour behaviour, IEnumerator enumerator)
		{
			this.stop = false;
			//this.behaviour = behaviour;
			this.enumerator = enumerator;
			
			if (behaviour != null && behaviour.gameObject.activeSelf)
			{
				behaviour.StartCoroutine(this);
			}
		}
		
		// Interface implementations
		public object Current { get { return enumerator.Current; } }
		public bool MoveNext() { return !stop && enumerator.MoveNext(); }
		public void Reset() { enumerator.Reset(); }
		#if false	// Tst Func set
		public static IEnumerator TstFunc(){
			while(true){
				Debug.Log("InTstFunc at :"+Time.time);
				yield return new WaitForSeconds(0.5f);
			}
		}
		public static IEnumerator StopTstFunc(CoroutineStoppable coroutine, float waitSecond){
			yield return new WaitForSeconds(waitSecond);
			coroutine.stop = true;
		}
		#endif
	}

	public class WaitTimeSkippable : IEnumerator
	{
		float _statTime;
		float _timeToWait;
		Func<bool> _deleCheckSkip;
		public WaitTimeSkippable(float timeToWait, Func<bool> checkToSkip)
		{
			_statTime = Time.time;
			_timeToWait = timeToWait;
			_deleCheckSkip = checkToSkip;
		}
		// Interface implementations
		public object Current { get { return null; } }
		public bool MoveNext() 
		{
			return (Time.time - _statTime) < _timeToWait && !_deleCheckSkip();
		}
		public void Reset() {	_statTime = Time.time;	}
	}

	public static class SystemExt
	{
		public static Vector3 Pos(this Collider col)
		{
			return col.bounds.center;
		}
	}
}
