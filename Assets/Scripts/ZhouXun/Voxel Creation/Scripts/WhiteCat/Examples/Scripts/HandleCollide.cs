using UnityEngine;
using System.Collections;
using WhiteCat;

public class HandleCollide : MonoBehaviour
{
	public GameObject bang;


	void OnCollisionEnter()
	{
		Time.timeScale = 0;
		bang.SetActive(true);
	}
}