using UnityEngine;
using System.Collections;

public class UIFontTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		UILabel label = gameObject.GetComponent<UILabel>();
		Debug.Log( label.font.size);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
