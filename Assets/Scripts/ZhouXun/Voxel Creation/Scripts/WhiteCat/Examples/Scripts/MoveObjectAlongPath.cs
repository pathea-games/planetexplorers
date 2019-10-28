using UnityEngine;
using WhiteCat;

[RequireComponent(typeof(PathDriver))]
public class MoveObjectAlongPath : MonoBehaviour
{
	public float speed = 10;

	PathDriver driver;


	void Awake()
	{
		driver = GetComponent<PathDriver>();
	}


	void Update()
	{
		driver.location += speed * Time.deltaTime;
	}
}