using UnityEngine;
using System.Collections;

namespace Steer3D
{
	public enum BehaviourType
	{
		Seek = 1,
		Flee,
		Flees,
		Pursue,
		Evade,
		PathFollow
	}

	[RequireComponent(typeof(SteerAgent))]
	public abstract class SteeringBehaviour : MonoBehaviour
	{
		public BehaviourType type
		{
			get { return (BehaviourType)System.Enum.Parse(typeof(BehaviourType), this.GetType().Name); }
		}

		public bool active = true;
		protected SteerAgent agent;
		protected Vector3 position { get { return agent.position; } }
		protected virtual void Awake ()
		{
			agent = GetComponent<SteerAgent>();
			agent.RegisterBehaviour(this);
		}

		protected virtual void OnDestroy ()
		{
			agent.DeregisterBehaviour(this);
		}

		public abstract void Behave ();
		public abstract bool idle { get; }
	}
}