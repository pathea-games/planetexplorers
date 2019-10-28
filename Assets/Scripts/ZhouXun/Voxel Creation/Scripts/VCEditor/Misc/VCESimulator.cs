using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCESimulator : MonoBehaviour
{
	// Simulate VCE
	
	void OnGUI ()
	{
		if ( !VCEditor.s_Active )
		{
			if (GUI.Button( new Rect( (Screen.width - 300)*0.5f, (Screen.height - 70) * 0.5f, 300, 70 ), "Voxel Creation Editor"))
			{
				VCEditor.Open();
			}
		}
	}
}
