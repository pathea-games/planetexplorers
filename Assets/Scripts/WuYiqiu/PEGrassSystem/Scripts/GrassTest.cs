using UnityEngine;
using System.Collections;

public class GrassTest : MonoBehaviour 
{

	public GameObject mNewPrefab;

	public GameObject mRandomPrefab;

	void OnGUI()
	{
		if (GUI.Button(new Rect(300, 50, 100, 50), "New Grass"))
		{
			GameObject inst = Instantiate(mNewPrefab) as GameObject;
			inst.SetActive(false);
		}

		if (GUI.Button(new Rect(300, 110, 100, 50), "GC"))
		{
			System.GC.Collect();
		}

		if (GUI.Button(new Rect(300, 170, 100, 50), "Random Grass"))
		{
			GameObject inst = Instantiate(mRandomPrefab) as GameObject;
			inst.SetActive(false);
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
