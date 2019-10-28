using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;

public partial class PeCustomMenuc : EditorWindow 
{
    [MenuItem("PeCustomMenu/Compare Skeleton")]
    static void CompareSkeleton()
    {
        Transform[] tranArray = Selection.GetTransforms(SelectionMode.TopLevel);
        if (2 != tranArray.Length)
        {
            Debug.LogError("Not 2 transform selected.");
            return;
        }

        //Debug.Log(tranArray[0].name);
        //Debug.Log(tranArray[1].name);
        if (CompareTrans(tranArray[0], tranArray[1], ""))
        {
            Debug.Log("skeleton is same.");
        }
    }

    static bool CompareTrans(Transform tran1, Transform tran2, string prefix)
    {
        string tmpPrefix = prefix + "/";

        if (tran1.name != tran2.name)
        {
            Debug.LogError(tmpPrefix + tran1.name + ":different name");
            return false;
        }

        if (tran1.childCount != tran2.childCount)
        {
            Debug.LogError(tmpPrefix + tran1.name + ":different child count");
            return false;
        }

        Debug.Log(prefix);

        for (int i = 0; i < tran1.childCount; i++)
        {
            Transform child1 = tran1.GetChild(i);
            Transform child2 = tran2.FindChild(child1.name);

            if (null != child2)
            {
                if (!CompareTrans(child1, child2, tmpPrefix + child1.name))
                {
                    //Debug.LogError("skeleton[" + child1.name + "] has no identical.");
                    return false;
                }
            }
            else
            {
                Debug.LogError("skeleton[" + tmpPrefix + child1.name + "] has no identical.");
                return false;
            }
        }

        return true;
    }
}

