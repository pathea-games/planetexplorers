using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AiBehaveSingle))]
public class AiBehaveSingleEditor : AiBehaveEditor
{
    AiBehaveSingle single;

    new public void OnEnable()
    {
        base.OnEnable();

        single = script as AiBehaveSingle;
    }

    protected override void DrawAgent()
    {
        base.DrawAgent();

        if(single.aiObject != null)
			EditorGUILayout.ObjectField("Ai Object : ", single.aiObject, typeof(AiObject), true);
    }
}
