using UnityEngine;
using UnityEditor;
using System.Collections;

public class PrefabCopy : EditorWindow
{
    public GameObject source;
    public GameObject target;
    public float scale = 1.0f;

    [MenuItem("Window/Prefab Copy")]
    static void Init()
    {
        //PrefabCopy window = (PrefabCopy)EditorWindow.GetWindow(typeof(PrefabCopy));
    }

    void OnGUI()
    {
		source = EditorGUILayout.ObjectField("source : ", source, typeof(GameObject), true) as GameObject;
		target = EditorGUILayout.ObjectField("target : ", target, typeof(GameObject), true) as GameObject;

        scale = EditorGUILayout.FloatField("scale : ", scale);

        if (GUILayout.Button("Copy"))
        {
            Copy();
        }
    }

    void Copy()
    {
        if (source == null || target == null)
            return;

        CopySerialized(source.transform, target.transform);

        Transform[] transforms = source.GetComponentsInChildren<Transform>();

        foreach (Transform tr in transforms)
        {
            CopySerialized(tr, AiUtil.GetChild(target.transform, tr.name));
        }
    }

    void CopySerialized(Transform src, Transform tar)
    {
        if (src == null || tar == null)
            return;

        Rigidbody rigid = src.GetComponent<Rigidbody>();
        if (rigid != null)
        {
            Rigidbody rigid1 = tar.GetComponent<Rigidbody>();
            if (rigid1 == null)
                rigid1 = tar.gameObject.AddComponent<Rigidbody>();

            EditorUtility.CopySerialized(rigid, rigid1);
        }

        CapsuleCollider cap1 = src.GetComponent<CapsuleCollider>();
        if (cap1 != null)
        {
            CapsuleCollider cap2 = tar.GetComponent<CapsuleCollider>();
            if (cap2 == null)
                cap2 = tar.gameObject.AddComponent<CapsuleCollider>();
            EditorUtility.CopySerialized(cap1, cap2);

            cap2.center *= scale;
            cap2.radius *= scale;
            cap2.height *= scale;
        }

        SphereCollider s1 = src.GetComponent<SphereCollider>();
        if (s1 != null)
        {
            SphereCollider s2 = tar.GetComponent<SphereCollider>();
            if (s2 == null)
                s2 = tar.gameObject.AddComponent<SphereCollider>();
            EditorUtility.CopySerialized(s1, s2);

            s2.center *= scale;
            s2.radius *= scale;
        }

        BoxCollider box1 = src.GetComponent<BoxCollider>();
        if (box1 != null)
        {
            BoxCollider box2 = tar.GetComponent<BoxCollider>();
            if (box2 == null)
                box2 = tar.gameObject.AddComponent<BoxCollider>();
            EditorUtility.CopySerialized(box1, box2);

            box2.center *= scale;
            box2.size *= scale;
        }

        CharacterJoint joint1 = src.GetComponent<CharacterJoint>();
        if (joint1 != null)
        {
            CharacterJoint joint2 = tar.GetComponent<CharacterJoint>();
            if (joint2 == null)
                joint2 = tar.gameObject.AddComponent<CharacterJoint>();
            EditorUtility.CopySerialized(joint1, joint2);

            joint2.connectedBody = AiUtil.GetChild(target.transform, joint2.connectedBody.name).GetComponent<Rigidbody>();
        }

        LegController leg1 = src.GetComponent<LegController>();
        if (leg1 != null)
        {
            LegController leg2 = tar.GetComponent<LegController>();
            if (leg2 == null)
                leg2 = tar.gameObject.AddComponent<LegController>();
            EditorUtility.CopySerialized(leg1, leg2);

            leg2.groundedPose = target.GetComponent<Animation>()[leg2.groundedPose.name].clip;
            leg2.rootBone = AiUtil.GetChild(target.transform, leg2.rootBone.name);

            foreach (LegInfo leg in leg2.legs)
            {
                leg.ankle = AiUtil.GetChild(target.transform, leg.ankle.name);
                leg.hip = AiUtil.GetChild(target.transform, leg.hip.name);
                leg.toe = AiUtil.GetChild(target.transform, leg.toe.name);

                leg.footLength *= scale;
                leg.footWidth *= scale;
                leg.footOffset.x *= scale;
                leg.footOffset.y *= scale;
            }

            foreach (MotionAnalyzer motion in leg2.sourceAnimations)
            {
                motion.animation = target.GetComponent<Animation>()[motion.animation.name].clip;
            }
        }

        AlignmentTracker al1 = src.GetComponent<AlignmentTracker>();
        if (leg1 != null)
        {
            AlignmentTracker al2 = tar.GetComponent<AlignmentTracker>();
            if (al2 == null)
                al2 = tar.gameObject.AddComponent<AlignmentTracker>();
            EditorUtility.CopySerialized(al1, al2);
        }

        LegAnimator legA1 = src.GetComponent<LegAnimator>();
        if (legA1 != null)
        {
            LegAnimator legA2 = tar.GetComponent<LegAnimator>();
            if (legA2 == null)
                legA2 = tar.gameObject.AddComponent<LegAnimator>();
            EditorUtility.CopySerialized(legA1, legA2);
        }
    }
}
