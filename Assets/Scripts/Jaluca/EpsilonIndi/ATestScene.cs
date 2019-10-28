using UnityEngine;
using System.Collections;

public class ATestScene : MonoBehaviour {
	public GameObject[] disable;
	void Awake () {
		foreach(GameObject a in disable)
			a.SetActive(false);
	}
}
