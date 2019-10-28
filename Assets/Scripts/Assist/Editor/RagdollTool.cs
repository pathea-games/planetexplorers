using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using PETools;

public class RagdollTool
{
    const string NpcRagdollTemplate = "Assets/Models/Ragdoll/Npc/";
    const string NpcRagdollFileName = "NpcRagdollTemplate.prefab";

    [MenuItem("Assets/Ragdoll/UpdateRagdoll")]
    public static void BatchRagdoll()
    {
        foreach (Object obj in Selection.objects)
        {
            UpdateRagdollObj(obj);
        }
    }

    [MenuItem("Assets/Ragdoll/UpdateRagdollModel")]
    public static void BatchRagdollModel()
    {
        foreach (Object obj in Selection.objects)
        {
            UpdateRagdollModel(obj);
        }
    }

    [MenuItem("Assets/Ragdoll/UpdateConfigurableJoint")]
    public static void UpdateConfigurableJoint()
    {
        foreach (Object obj in Selection.objects)
        {
            UpdateRagdollObjTmp(obj);
        }
    }

    [MenuItem("Assets/Ragdoll/CreateNpcRagdoll")]
    public static void CreateNpcRagdoll()
    {
        foreach (Object obj in Selection.objects)
        {
            CreateNpcRagdoll(obj);
        }
    }

    static void UpdateRagdollObj(Object obj)
    {
        string path = AssetDatabase.GetAssetPath(obj);
        GameObject go = GameObject.Instantiate(obj) as GameObject;
        //GameObject go = obj as GameObject;
        if (go != null)
        {
            Debug.Log("Update ragdoll prefab : " + path);

            CharacterJoint[] oldJoints = go.GetComponentsInChildren<CharacterJoint>();
            foreach (CharacterJoint oldJoint in oldJoints)
            {
                ConfigurableJoint newJoint = oldJoint.gameObject.AddComponent<ConfigurableJoint>();

                newJoint.connectedBody = oldJoint.connectedBody;

                newJoint.xMotion = ConfigurableJointMotion.Locked;
                newJoint.yMotion = ConfigurableJointMotion.Locked;
                newJoint.zMotion = ConfigurableJointMotion.Locked;

                newJoint.angularXMotion = ConfigurableJointMotion.Limited;
                newJoint.angularYMotion = ConfigurableJointMotion.Limited;
                newJoint.angularZMotion = ConfigurableJointMotion.Limited;

                SoftJointLimit limt = new SoftJointLimit();

                limt.limit = -30.0f;
                newJoint.lowAngularXLimit = limt;

                limt.limit = 30.0f;
                newJoint.highAngularXLimit = limt;
                newJoint.angularYLimit = limt;
                newJoint.angularZLimit = limt;

                newJoint.rotationDriveMode = RotationDriveMode.Slerp;

                GameObject.DestroyImmediate(oldJoint);
            }

            Animator[] anims = go.GetComponentsInChildren<Animator>();
            foreach (Animator anim in anims)
            {
                GameObject.DestroyImmediate(anim);
            }

            Animation[] animations = go.GetComponentsInChildren<Animation>();
            foreach (Animation animation in animations)
            {
                GameObject.DestroyImmediate(animation);
            }

            LegAnimator[] animators = go.GetComponentsInChildren<LegAnimator>();
            foreach (LegAnimator animator in animators)
            {
                GameObject.DestroyImmediate(animator);
            }

            AlignmentTracker[] trackers = go.GetComponentsInChildren<AlignmentTracker>();
            foreach (AlignmentTracker tracker in trackers)
            {
                GameObject.DestroyImmediate(tracker);
            }

            LegController[] controllers = go.GetComponentsInChildren<LegController>();
            foreach (LegController controller in controllers)
            {
                GameObject.DestroyImmediate(controller);
            }

            PrefabUtility.ReplacePrefab(go, obj);

            GameObject.DestroyImmediate(go);
        }
    }

    static void UpdateRagdollModel(Object oldPrefab)
    {
        string path = AssetDatabase.GetAssetPath(oldPrefab);
        if (path.Contains("Ragdoll"))
        {
            string path1 = path.Substring(0, path.IndexOf("Ragdoll"));
            string path2 = path.Substring(path.IndexOf("Ragdoll"), path.Length - path.IndexOf("Ragdoll")).Remove(0, ("Ragdoll/").Length);

            string newPath = Path.ChangeExtension(path1 + path2, ".FBX");

            Object newModel = AssetDatabase.LoadAssetAtPath(newPath, typeof(Object)) as Object;

            GameObject oldObj = GameObject.Instantiate(oldPrefab) as GameObject;
            GameObject newPrefab = GameObject.Instantiate(newModel) as GameObject;

            Rigidbody[] rigids = oldObj.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rigid in rigids)
            {
                Transform tr = PETools.PEUtil.GetChild(newPrefab.transform, rigid.name);
                if (tr != null)
                {
                    Rigidbody newRigid = tr.gameObject.AddComponent<Rigidbody>();

                    EditorUtility.CopySerialized(rigid, newRigid);
                }
            }

            Collider[] colliders = oldObj.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                Transform tr = PETools.PEUtil.GetChild(newPrefab.transform, collider.name);
                if (tr != null)
                {
                    Collider newCollider = null;
                    if(collider is BoxCollider)
                        newCollider = tr.gameObject.AddComponent<BoxCollider>();
                    else if(collider is SphereCollider)
                        newCollider = tr.gameObject.AddComponent<SphereCollider>();
                    else if(collider is CapsuleCollider)
                        newCollider = tr.gameObject.AddComponent<CapsuleCollider>();

                    EditorUtility.CopySerialized(collider, newCollider);
                }
            }

            ConfigurableJoint[] joints = oldObj.GetComponentsInChildren<ConfigurableJoint>();
            foreach (ConfigurableJoint joint in joints)
            {
                Transform tr = PETools.PEUtil.GetChild(newPrefab.transform, joint.name);
                if (tr != null)
                {
                    ConfigurableJoint newJoint = tr.gameObject.AddComponent<ConfigurableJoint>();

                    EditorUtility.CopySerialized(joint, newJoint);

                    if(joint.connectedBody != null)
                    {
                        Transform rigidTrans = PETools.PEUtil.GetChild(newPrefab.transform, joint.connectedBody.name);
                        if (rigidTrans != null)
                        {
                            newJoint.connectedBody = rigidTrans.GetComponent<Rigidbody>();
                        }
                    }
                }
            }

            Animator[] anims = newPrefab.GetComponentsInChildren<Animator>();
            foreach (Animator anim in anims)
            {
                GameObject.DestroyImmediate(anim);
            }

            Animation[] animations = newPrefab.GetComponentsInChildren<Animation>();
            foreach (Animation animation in animations)
            {
                GameObject.DestroyImmediate(animation);
            }

            PrefabUtility.ReplacePrefab(newPrefab, oldPrefab);

            GameObject.DestroyImmediate(oldObj);
            GameObject.DestroyImmediate(newPrefab);
        }
    }

    static void UpdateRagdollObjTmp(Object obj)
    {
        /*string path = */AssetDatabase.GetAssetPath(obj);
        GameObject go = GameObject.Instantiate(obj) as GameObject;
        if (go != null)
        {
            ConfigurableJoint[] joints = go.GetComponentsInChildren<ConfigurableJoint>();
            foreach (ConfigurableJoint joint in joints)
            {
                joint.angularXMotion = ConfigurableJointMotion.Limited;
                joint.angularYMotion = ConfigurableJointMotion.Limited;
                joint.angularZMotion = ConfigurableJointMotion.Limited;
            }

            PrefabUtility.ReplacePrefab(go, obj);
            GameObject.DestroyImmediate(go);
        }
    }

    static void CreateNpcRagdoll(Object obj)
    {
        string path = NpcRagdollTemplate + NpcRagdollFileName;
        Object templateAsset = AssetDatabase.LoadAssetAtPath(path, typeof(Object)) as Object;

        if (templateAsset != null)
        {
            GameObject template = GameObject.Instantiate(templateAsset) as GameObject;
            GameObject prefab = GameObject.Instantiate(obj) as GameObject;

            Animator[] anims = prefab.GetComponentsInChildren<Animator>();
            foreach (Animator anim in anims)
            {
                GameObject.DestroyImmediate(anim);
            }

            Animation[] animations = prefab.GetComponentsInChildren<Animation>();
            foreach (Animation animation in animations)
            {
                GameObject.DestroyImmediate(animation);
            }

            Rigidbody[] rigids = template.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rigid in rigids)
            {
                Transform tr = PETools.PEUtil.GetChild(prefab.transform, rigid.name);
                if (tr != null)
                {
                    Rigidbody newRigid = tr.gameObject.AddComponent<Rigidbody>();

                    EditorUtility.CopySerialized(rigid, newRigid);
                }
            }

            Collider[] colliders = template.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                Transform tr = PETools.PEUtil.GetChild(prefab.transform, collider.name);
                if (tr != null)
                {
                    Collider newCollider = null;
                    if (collider is BoxCollider)
                        newCollider = tr.gameObject.AddComponent<BoxCollider>();
                    else if (collider is SphereCollider)
                        newCollider = tr.gameObject.AddComponent<SphereCollider>();
                    else if (collider is CapsuleCollider)
                        newCollider = tr.gameObject.AddComponent<CapsuleCollider>();

                    EditorUtility.CopySerialized(collider, newCollider);
                }
            }

            ConfigurableJoint[] joints = template.GetComponentsInChildren<ConfigurableJoint>();
            foreach (ConfigurableJoint joint in joints)
            {
                Transform tr = PETools.PEUtil.GetChild(prefab.transform, joint.name);
                if (tr != null)
                {
                    ConfigurableJoint newJoint = tr.gameObject.AddComponent<ConfigurableJoint>();

                    EditorUtility.CopySerialized(joint, newJoint);

                    if (joint.connectedBody != null)
                    {
                        Transform rigidTrans = PETools.PEUtil.GetChild(prefab.transform, joint.connectedBody.name);
                        if (rigidTrans != null)
                        {
                            newJoint.connectedBody = rigidTrans.GetComponent<Rigidbody>();
                        }
                    }
                }
            }

            PrefabUtility.CreatePrefab(NpcRagdollTemplate + PEUtil.ToPrefabName(prefab.name) + ".prefab", prefab);

            GameObject.DestroyImmediate(prefab);
            GameObject.DestroyImmediate(template);
        }
        else
        {
            Debug.LogError("Can't find template prefab!");
        }
    }
}
