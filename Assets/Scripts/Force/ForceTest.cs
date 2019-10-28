using UnityEngine;
using System.Collections;

public class ForceTest : MonoBehaviour 
{
    public TextAsset text;
	// Use this for initialization
	void Start () {
        ForceSetting.Instance.Load(text);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
