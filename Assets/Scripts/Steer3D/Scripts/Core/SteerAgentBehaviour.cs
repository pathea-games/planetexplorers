using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Steer3D
{
	public partial class SteerAgent : MonoBehaviour
	{
		public Seek AlterSeekBehaviour (Vector3 target, float slowingRadius, float arriveRadius, float weight = 1f)
		{
			Seek seek = InvokeBehaviour<Seek>();
			seek.target = target;
            seek.targetTrans = null;
			seek.weight = weight;
			seek.slowingRadius = slowingRadius;
			seek.arriveRadius = arriveRadius;
			return seek;
		}
		
		public Seek AlterSeekBehaviour (Transform target, float slowingRadius, float arriveRadius, float weight = 1f)
		{
			Seek seek = InvokeBehaviour<Seek>();
			seek.targetTrans = target;
			seek.weight = weight;
			seek.slowingRadius = slowingRadius;
			seek.arriveRadius = arriveRadius;
			return seek;
		}

		public Flee AlterFleeBehaviour (Vector3 target, float affectRadius, float fleeRadius, float forbiddenRadius, float weight = 1f)
		{
			Flee flee = InvokeBehaviour<Flee>();
			flee.target = target;
			flee.weight = weight;
			flee.affectRadius = affectRadius;
			flee.fleeRadius = fleeRadius;
			flee.forbiddenRadius = forbiddenRadius;
			return flee;
		}

		public Flee AlterFleeBehaviour (Transform target, float affectRadius, float fleeRadius, float forbiddenRadius, float weight = 1f)
		{
			Flee flee = InvokeBehaviour<Flee>();
			flee.targetTrans = target;
			flee.weight = weight;
			flee.affectRadius = affectRadius;
			flee.fleeRadius = fleeRadius;
			flee.forbiddenRadius = forbiddenRadius;
			return flee;
		}

		public Flees AlterFleesBehaviour ()
		{
			Flees flees = InvokeBehaviour<Flees>();
			return flees;
		}

		public Pursue AlterPursueBehaviour (Vector3 target, float slowingRadius, float arriveRadius, float weight = 1f)
		{
			Pursue pursue = InvokeBehaviour<Pursue>();
			pursue.target = target;
			pursue.weight = weight;
			pursue.slowingRadius = slowingRadius;
			pursue.arriveRadius = arriveRadius;
			return pursue;
		}
		
		public Pursue AlterPursueBehaviour (Transform target, float slowingRadius, float arriveRadius, float weight = 1f)
		{
			Pursue pursue = InvokeBehaviour<Pursue>();
			pursue.targetTrans = target;
			pursue.weight = weight;
			pursue.slowingRadius = slowingRadius;
			pursue.arriveRadius = arriveRadius;
			return pursue;
		}

		public Evade AlterEvadeBehaviour (Vector3 target, float affectRadius, float fleeRadius, float forbiddenRadius, float weight = 1f)
		{
			Evade evade = InvokeBehaviour<Evade>();
			evade.target = target;
			evade.weight = weight;
			evade.affectRadius = affectRadius;
			evade.fleeRadius = fleeRadius;
			evade.forbiddenRadius = forbiddenRadius;
			return evade;
		}
		
		public Evade AlterEvadeBehaviour (Transform target, float affectRadius, float fleeRadius, float forbiddenRadius, float weight = 1f)
		{
			Evade evade = InvokeBehaviour<Evade>();
			evade.targetTrans = target;
			evade.weight = weight;
			evade.affectRadius = affectRadius;
			evade.fleeRadius = fleeRadius;
			evade.forbiddenRadius = forbiddenRadius;
			return evade;
		}

		public PathFollow AlterPathFollowBehaviour (Vector3[] path, float slowingRadius, float arriveRadius, float weight = 1f)
		{
			PathFollow pathfollow = InvokeBehaviour<PathFollow>();
			pathfollow.Reset(path);
			pathfollow.weight = weight;
			pathfollow.slowingRadius = slowingRadius;
			pathfollow.arriveRadius = arriveRadius;
			return pathfollow;
		}

		public T InvokeBehaviour<T> () where T : SteeringBehaviour
		{
			BehaviourType type = (BehaviourType)(System.Enum.Parse(typeof(BehaviourType), typeof(T).Name));
			if (behaviours.ContainsKey(type))
				return (T)(behaviours[type]);
			return gameObject.AddComponent<T>();
		}

		public T FindBehaviour<T> () where T : SteeringBehaviour
		{
			BehaviourType type = (BehaviourType)(System.Enum.Parse(typeof(BehaviourType), typeof(T).Name));
			if (behaviours.ContainsKey(type))
				return (T)(behaviours[type]);
			return null;
		}

		public T SetBehaviourActive<T> (bool active) where T : SteeringBehaviour
		{
			T t = FindBehaviour<T>();
			if (t != null)
				t.active = active;
			return t;
		}
		
		public bool RemoveBehaviour<T> () where T : SteeringBehaviour
		{
			T t = FindBehaviour<T>();
			if (t != null)
			{
				SteeringBehaviour.Destroy(t);
				DeregisterBehaviour(t);
				return true;
			}
			return false;
		}
		
		// internal

		Dictionary<BehaviourType, SteeringBehaviour> behaviours = new Dictionary<BehaviourType, SteeringBehaviour>();
		
		internal void RegisterBehaviour (SteeringBehaviour behaviour)
		{
			if (behaviours == null)
				behaviours = new Dictionary<BehaviourType, SteeringBehaviour>();

			BehaviourType bt = behaviour.type;
			if (behaviours.ContainsKey(bt))
			{
				SteeringBehaviour.Destroy(behaviour);
				DeregisterBehaviour(behaviour);
			}
			behaviours[bt] = behaviour;
		}
		
		internal void DeregisterBehaviour (SteeringBehaviour behaviour)
		{
			if (behaviours == null)
				return;

			BehaviourType bt = behaviour.type;
			if (behaviours.ContainsKey(bt))
			{
				if (behaviours[bt] == behaviour)
					behaviours.Remove(bt);
			}
		}
		
		private void UpdateBehaviours ()
		{
			bool _idle = true;
			if (behaviours != null)
			{
				foreach (KeyValuePair<BehaviourType, SteeringBehaviour> kvp in behaviours)
				{
					SteeringBehaviour behaviour = kvp.Value;
					if (behaviour != null && behaviour.enabled && behaviour.active)
					{
						if (!behaviour.idle)
							_idle = false;
						behaviour.Behave();
					}
				}
			}
			if (_idle)
			{
				AddDesiredVelocity(Vector3.zero, 1);
			}
		}

		public bool idle
		{
			get
			{
				if (behaviours != null)
				{
					foreach (KeyValuePair<BehaviourType, SteeringBehaviour> kvp in behaviours)
					{
						if (kvp.Value == null)
							continue;
						if (!kvp.Value.idle)
							return false;
					}
				}
				return true;
			}
		}
	}
}
