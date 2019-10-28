using UnityEngine;
using UnityEditor;
using System.Collections;

public static class CutsceneEditorMenu
{
	[MenuItem("GameObject/Cutscene/Cutscene clip (Bezier)")]
	public static void CreateBezierCutsceneClip ()
	{
		GameObject go = new GameObject ("New Cutscene Clip");
		go.AddComponent<CutsceneClip>();
		go.AddComponent<WhiteCat.BezierPath>();
		WhiteCat.TweenInterpolator intp = go.AddComponent<WhiteCat.TweenInterpolator>();
		intp.enabled = false;
		intp.duration = 10;
		if (SceneView.lastActiveSceneView != null)
		{
			go.transform.position = SceneView.lastActiveSceneView.camera.transform.position + 
				SceneView.lastActiveSceneView.camera.transform.forward * 4f;
		}
	}

	[MenuItem("GameObject/Cutscene/Cutscene clip (Cardinal)")]
	public static void CreateCardinalCutsceneClip ()
	{
		GameObject go = new GameObject ("New Cutscene Clip");
		go.AddComponent<CutsceneClip>();
		go.AddComponent<WhiteCat.CardinalPath>();
		WhiteCat.TweenInterpolator intp = go.AddComponent<WhiteCat.TweenInterpolator>();
		intp.enabled = false;
		intp.duration = 10;
		if (SceneView.lastActiveSceneView != null)
		{
			go.transform.position = SceneView.lastActiveSceneView.camera.transform.position + 
				SceneView.lastActiveSceneView.camera.transform.forward * 4f;
		}
	}
}
