using UnityEngine;
using WhiteCat;

[RequireComponent(typeof(BuildMeshAlongPath))]
public class CreatePathFromCode : MonoBehaviour
{
	[Range(4, 32)] public int nodesCount = 16;
	[Range(30, 60)] public float minRadius = 30;
	[Range(30, 60)] public float maxRadius = 60;


	public void CreateRandomPath()
	{
		CardinalPath path = gameObject.AddComponent<CardinalPath>();
		path.isCircular = true;
		

		for(int i = path.nodesCount; i < nodesCount; i++)
		{
			path.InsertNode(0);
		}


		Vector3 position = Vector3.zero;
		float radian;
		float radius;


		for(int i = 0; i < nodesCount; i++)
		{
			radian = 2 * Mathf.PI / nodesCount * i;
			radius = UnityEngine.Random.Range(minRadius, maxRadius);

			position.x = Mathf.Cos(radian) * radius;
			position.z = Mathf.Sin(radian) * radius;

			path.SetNodeLocalPosition(i, position);
		}


		BuildMeshAlongPath builder = GetComponent<BuildMeshAlongPath>();
		builder.path = path;
		builder.Bulid();

		Destroy(path);
	}
}
