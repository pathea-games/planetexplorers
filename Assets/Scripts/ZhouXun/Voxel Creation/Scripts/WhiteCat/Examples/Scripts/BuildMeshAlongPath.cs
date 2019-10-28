using System.Collections.Generic;
using UnityEngine;
using WhiteCat;

[RequireComponent(typeof(MeshFilter))]
public class BuildMeshAlongPath : MonoBehaviour
{
	public Path path;

	[Space(8)]

	[Range(0.1f, 100)] public float width = 6;
	[Range(1, 30)] public float deltaAngle = 3;
	[Range(0.01f, 1)] public float angleError = 0.1f;
	[Range(0.01f, 1)] public float uvxPerUnit = 0.1f;

	[Space(8)]

	public string directory = "Assets/WhiteCat/Examples/Res";
	public string fileName = "mesh.asset";


	public void Bulid()
	{
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();

		int index = 0;
		float time = 0;
		Vector3 temp;
		float uvLength;
		Vector3 position = path.GetSplinePoint(0, 0);
		Quaternion rotation = path.GetSplineRotation(0, 0);

		vertices.Add(position + (temp = rotation * Vector3.left * (width * 0.5f)));
		vertices.Add(position - temp);
		normals.Add(temp = rotation * Vector3.up);
		normals.Add(temp);
		uv.Add(new Vector2(0, 1));
		uv.Add(new Vector2(0, 0));

		bool hasNext = true;
		while (hasNext)
		{
			hasNext = !path.GetNextSplinePosition(ref index, ref time, deltaAngle - angleError, deltaAngle + angleError);

			position = path.GetSplinePoint(index, time);
			rotation = path.GetSplineRotation(index, time);
			uvLength = path.GetPathLength(index, time) * uvxPerUnit;

			vertices.Add(position + (temp = rotation * Vector3.left * (width * 0.5f)));
			vertices.Add(position - temp);
			normals.Add(temp = rotation * Vector3.up);
			normals.Add(temp);
			uv.Add(new Vector2(uvLength, 1));
			uv.Add(new Vector2(uvLength, 0));
			triangles.Add(vertices.Count - 4);
			triangles.Add(vertices.Count - 2);
			triangles.Add(vertices.Count - 3);
			triangles.Add(vertices.Count - 3);
			triangles.Add(vertices.Count - 2);
			triangles.Add(vertices.Count - 1);
		}

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.normals = normals.ToArray();
		mesh.uv = uv.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateBounds();

		GetComponent<MeshFilter>().sharedMesh = mesh;
	}
}
