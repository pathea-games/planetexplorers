using UnityEngine;
using System.Collections;

namespace Steer3D
{
	public class PathFollow : SteeringBehaviour
	{
		public Vector3[] path = new Vector3[0];
		public float weight = 1f;
		public float slowingRadius = 5f;
		public float arriveRadius = 0.2f;

		public int followIndex = 0;
		public Vector3 followTarget
		{
			get
			{
				if (path.Length == 0)
					return Vector3.zero;
				int fi = followIndex;
				if (fi >= path.Length)
					fi = path.Length - 1;
				return path[fi];
			}
		}

		public bool drawPathInScene = false;

		public override bool idle
		{
			get { return !active || followIndex == path.Length; }
		}

		public void Reset (Vector3[] _path)
		{
			path = new Vector3[_path.Length] ;
			System.Array.Copy(_path, path, _path.Length);
			followIndex = 0;
		}
		
		public override void Behave ()
		{
			if (!idle)
			{
				if (followIndex < path.Length)
				{
					Vector3 target = followTarget;
					Vector3 to_target = target - position;
					Vector3 desired_vel = to_target.normalized;
					float strength = 1;
					if (followIndex == path.Length - 1)
					{
						if (slowingRadius > arriveRadius)
							strength = Mathf.Clamp01(Mathf.InverseLerp(arriveRadius, slowingRadius, to_target.magnitude));
						else
							strength = to_target.magnitude > arriveRadius ? 1f : 0f;
					}
					desired_vel *= strength;
					agent.AddDesiredVelocity(desired_vel, weight, 0.75f);
					if (to_target.magnitude < arriveRadius)
						followIndex++;
				}
			}
		}

		void LateUpdate ()
		{
			if (drawPathInScene)
			{
				if (path.Length > 0)
				{
					for (int i = 0; i < path.Length; ++i)
					{
						Color c;
						if (i > followIndex)
							c = Color.red;
						else if (i == followIndex)
							c = Color.white;
						else
							c = Color.green;
						Debug.DrawLine(path[i] + Vector3.left * arriveRadius, path[i] + Vector3.right * arriveRadius, c);
						Debug.DrawLine(path[i] + Vector3.up * arriveRadius, path[i] + Vector3.down * arriveRadius, c);
						Debug.DrawLine(path[i] + Vector3.back * arriveRadius, path[i] + Vector3.forward * arriveRadius, c);
					}
					for (int i = 1; i < path.Length; ++i)
					{
						Color c;
						if (i > followIndex)
							c = Color.red;
						else if (i == followIndex)
							c = Color.white;
						else
							c = Color.green;
						Debug.DrawLine(path[i-1], path[i], c);
					}
				}
			}
		}
	}
}
