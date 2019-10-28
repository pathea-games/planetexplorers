using UnityEngine;
using WhiteCat;

public class CreateObjectsAlongPath : MonoBehaviour
{
	public GameObject original;
	public int amount = 20;


	void Awake()
	{
		Path path = GetComponent<Path>();
		int splineIndex = 0;
		float splineTime = 0;

		for(int i=0; i<amount; i++)
		{
			float pathLength = i * path.pathTotalLength / amount;
			path.GetPathPositionAtPathLength(pathLength, ref splineIndex, ref splineTime);

			Vector3 position = path.GetSplinePoint(splineIndex, splineTime);
			Quaternion rotation = path.GetSplineRotation(splineIndex, splineTime);

			Instantiate(original, position, rotation);
		}
	}
}