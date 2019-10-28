// (c)2011 Unity Park. All Rights Reserved.

using UnityEngine;
using uLink;

[AddComponentMenu("uLink Utilities/Object Label")]
public class ProxyLabel : uLink.MonoBehaviour
{
	public float minDistance = 1;
	public float maxDistance = 96;
	public float clampBorderSize = 0.05f;  // How much viewport space to leave at the borders when a label is being clamped
	public Color color = Color.green;
	public Vector3 offset = new Vector3(0, 2, 0);    // Units in world space to offset; 1 unit above object by default
	public GUIText prefabLabel;
	private GUIText myGuiText = null;
	//public bool clampToScreen = false;  // If true, label will be visible even if object is off screen

	void Awake()
	{
		GameObject tmp = Resources.Load("Prefab/PlayerPrefab/PlayerLabelText") as GameObject;
		prefabLabel = tmp.GetComponent<GUIText>();
	}

	void OnDestroy()
	{
		if (myGuiText != null)
		{
			Destroy(myGuiText);
			myGuiText = null;
		}
	}

	void LateUpdate()
	{
		ManualUpdate();
	}
	
	public void SetName(string name, int group)
	{
		myGuiText = Instantiate(prefabLabel, Vector3.zero, Quaternion.identity) as GUIText;
		if (null != myGuiText)
		{
			myGuiText.text = name;

			if (group == BaseNetwork.MainPlayer.TeamId)
			{
				myGuiText.material.color = Color.green;
			}
			else
			{
                Color32 col32=ForceSetting.Instance.GetForceColor(group);
                Color col=new Color(col32.r/255f,col32.g/255f,col32.b/255f,col32.a/255f);
                myGuiText.material.color = col;
			}
		}
	}
	
	public void ManualUpdate()
	{
		if (myGuiText == null || Camera.main == null)
			return;

		Vector3 pos = Camera.main.WorldToViewportPoint(transform.position + offset);
		myGuiText.transform.position = pos;
		myGuiText.enabled = (pos.z >= minDistance && pos.z <= maxDistance);
	}
	
	public static void ManualUpdateAll()
	{
		ProxyLabel[] labels = FindObjectsOfType(typeof(ProxyLabel)) as ProxyLabel[];
		foreach (ProxyLabel label in labels)
		{
			label.ManualUpdate();
		}
	}
}
