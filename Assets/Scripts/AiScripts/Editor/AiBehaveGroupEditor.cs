using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AiBehaveGroup))]
public class AiBehaveGroupEditor : AiBehaveEditor
{
    AiBehaveGroup group;

    new public void OnEnable()
    {
        base.OnEnable();

        group = script as AiBehaveGroup;
    }

    protected override void DrawAgent()
    {
        base.DrawAgent();

		if (group.aiGroup == null)
			return;

        foreach (AiObject obj in group.aiGroup.aiObjects)
        {
            if(obj != null)
				EditorGUILayout.ObjectField("Ai Object : ", obj, typeof(AiObject), true);
        }
    }
}
