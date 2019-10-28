using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using Behave.Assets;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;
using Convert = System.Convert;

[CustomEditor(typeof(AiBehave))]
public class AiBehaveEditor : Editor
{
    protected AiBehave script;
    //BLLibrary.TreeType treeType;
	Behave.Assets.Tree assetTree;

    public static Dictionary<string, CustomImpelementEditor> implementEditorTypes;
    public static Dictionary<string, System.Type> implementTypes;
	
	private List<string> components;

    public void OnEnable()
    {
        script = target as AiBehave;
		
        FindImplement();
		
		UpdateTreeAsset();
    }
	
    //private bool ClearImplement(AiImplement imp)
    //{
    //    //if(treeType == BLLibrary.TreeType.Unknown || assetTree == null)
    //    //    return true;
		
    //    //System.Object[] att = imp.GetType().GetCustomAttributes(false);
    //    //foreach (System.Object attribute in att)
    //    //{
    //    //    CustomImplementType implementAttr = attribute as CustomImplementType;
    //    //    if(implementAttr != null && components.Contains(implementAttr.implementName))
    //    //    {
    //    //        return false;
    //    //    }
    //    //}
		
    //    return true;
    //}
	
	private void UpdateTreeAsset()
	{
        try
        {
            //treeType = (BLLibrary.TreeType)System.Enum.Parse(typeof(BLLibrary.TreeType), script.treeName, true);

            ////if (treeType != script.treeType)
            //{
            //    //script.treeType = treeType;

            //    if (treeType == BLLibrary.TreeType.Unknown)
            //        assetTree = null;
            //    else
            //    {
            //        int number = GetCollection(treeType);
            //        int index = GetCollectionIndex(treeType);

            //        if (number >= 0 && index >= 0)
            //        {
            //            assetTree = BLLibrary.TreeAssetData(number, index, null) as Behave.Assets.Tree;
            //        }
            //    }

            //    AnalysisTreeComponents();

            //    AiImplement[] imps = script.GetComponents<AiImplement>();
            //    for (int i = imps.Length - 1; i >= 0; i--)
            //    {
            //        if (ClearImplement(imps[i]))
            //            DestroyImmediate(imps[i], true);
            //    }

            //    foreach (string com in components)
            //    {
            //        if (!implementTypes.ContainsKey(com))
            //        {
            //            Debug.LogError("Component error : " + com);
            //            continue;
            //        }

            //        if (script.gameObject.GetComponent(implementTypes[com]) != null)
            //            continue;

            //        script.gameObject.AddComponent(implementTypes[com]);
            //    }
            //}
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return;
        }
	}

    public override void OnInspectorGUI()
    {
        DrawBehaveTreeControl();
    }

    private void FindImplement()
    {
        //implementTypes = new Dictionary<string, System.Type>();

        //Assembly asm = Assembly.GetAssembly(typeof(AiImplement));
        //System.Type[] types = asm.GetTypes();

        //foreach (System.Type type in types)
        //{
        //    System.Type baseType = type.BaseType;               
        //    while (baseType != null)
        //    {
        //        if (baseType == typeof(AiImplement))
        //        {

        //            System.Object[] att = type.GetCustomAttributes(false);

        //            //Loop through the attributes for the graph editor class
        //            foreach (System.Object attribute in att)
        //            {
        //                CustomImplementType cit = attribute as CustomImplementType;

        //                if (cit != null && cit.implementType != null)
        //                {
        //                    implementTypes.Add(cit.implementName, cit.implementType);
        //                }

        //            }
        //            break;
        //        }

        //        baseType = baseType.BaseType;
        //    }
        //}
    }

    private void DrawBehaveTreeControl()
    {
        EditorGUILayout.BeginVertical();

        script.treeName = EditorGUILayout.TextField("Tree Name : ", script.treeName);

        if (GUILayout.Button("Update")) 
            UpdateTreeAsset();

        DrawAgent();

        EditorGUILayout.EndVertical();

        EditorUtility.SetDirty(target);
    }

    protected virtual void DrawAgent()
    {
 
    }

    //public int GetCollection(BLLibrary.TreeType treeType)
    //{
    //    string treeName = treeType.ToString();
    //    if (treeName.Contains("C0"))
    //    {
    //        return 0;
    //    }
    //    else if (treeName.Contains("C1"))
    //    {
    //        return 1;
    //    }
    //    else if (treeName.Contains("C2"))
    //    {
    //        return 2;
    //    }
    //    else if (treeName.Contains("C3"))
    //    {
    //        return 3;
    //    }
    //    else if (treeName.Contains("C4"))
    //    {
    //        return 4;
    //    }

    //    return -1;
    //}

    //private int GetCollectionIndex(BLLibrary.TreeType treeType)
    //{
    //    int index = -1;

    //    if (treeType != BLLibrary.TreeType.Unknown)
    //    {
    //        string treeName = treeType.ToString();
    //        if (treeName.Contains("C0"))
    //        {
    //            index = (int)treeType;
    //        }
    //        else if(treeName.Contains("C1"))
    //        {
    //            string[] treeNames = System.Enum.GetNames(typeof(BLLibrary.TreeType));
    //            List<string> treeList = new List<string>(treeNames);
    //            treeList = treeList.FindAll(ret => ret.Contains("C0"));

    //            index = (int)treeType - treeList.Count;
    //        }
    //        else if (treeName.Contains("C2"))
    //        {
    //            string[] treeNames = System.Enum.GetNames(typeof(BLLibrary.TreeType));
    //            List<string> treeList = new List<string>(treeNames);
    //            treeList = treeList.FindAll(ret => ret.Contains("C0") || ret.Contains("C1"));

    //            index = (int)treeType - treeList.Count;
    //        }
    //        else if (treeName.Contains("C3"))
    //        {
    //            string[] treeNames = System.Enum.GetNames(typeof(BLLibrary.TreeType));
    //            List<string> treeList = new List<string>(treeNames);
    //            treeList = treeList.FindAll(ret => ret.Contains("C0") || ret.Contains("C1") || ret.Contains("C2"));

    //            index = (int)treeType - treeList.Count;
    //        }
    //        else if (treeName.Contains("C4"))
    //        {
    //            string[] treeNames = System.Enum.GetNames(typeof(BLLibrary.TreeType));
    //            List<string> treeList = new List<string>(treeNames);
    //            treeList = treeList.FindAll(ret => ret.Contains("C0") || ret.Contains("C1") || ret.Contains("C2") || ret.Contains("C3"));

    //            index = (int)treeType - treeList.Count;
    //        }
    //    }
    //    return index;
    //}

    //public void AnalysisTreeComponents()
    //{
    //    if(components == null) components = new List<string>();
		
    //    components.Clear();
		
    //    if(treeType == BLLibrary.TreeType.Unknown || assetTree == null)
    //        return;
		
    //    AnalysisTreeAsset(assetTree);
    //}
	
    //private void AnalysisTreeAsset(Behave.Assets.Tree argAssetTree)
    //{
    //    foreach (Behave.Assets.Component component in argAssetTree.Components)
    //    {
    //        AnalysisComonent(component);
    //    }
    //}

    //private void AnalysisComonent(Behave.Assets.Component component)
    //{
    //    if (component is Behave.Assets.Action)
    //    {
    //        Behave.Assets.Action act = component as Behave.Assets.Action;
    //        components.Add(act.Name);		
    //    }
    //    else if (component is Behave.Assets.Decorator)
    //    {
    //        Behave.Assets.Decorator dec = component as Behave.Assets.Decorator;
    //        components.Add(dec.Name);
    //    }
    //    else if (component is Behave.Assets.Reference)
    //    {
    //        Behave.Assets.Reference refer = component as Behave.Assets.Reference;
    //        AnalysisTreeAsset(refer.Target);
    //    }
    //}
}

[System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public class CustomImpelementEditor : System.Attribute
{
    public System.Type implementType;
    public string displayName;
    public System.Type editorType;

    public CustomImpelementEditor(System.Type t, string displayName)
    {
        implementType = t;
        this.displayName = displayName;
    }
}

public class AiImplementEditor
{
    //public AiImplement target;
    public AiBehaveEditor editor;

//    bool drawInspectorGUI = false;

    public virtual void OnEnable()
    {

    }

    public virtual void OnDisable()
    {

    }

    public virtual void OnDestroy()
    {
    }

    ///** Override to implement graph inspectors */
    //public virtual void OnInspectorGUI(AiImplement implement)
    //{

    //}

    //public void OnDrawInspectorGUI(AiImplement implement)
    //{
    //    if (GUILayout.Button(implement.GetType().ToString()))
    //        drawInspectorGUI = !drawInspectorGUI;

    //    //if (drawInspectorGUI)
    //    //{
    //    //    target.active = EditorGUILayout.Toggle("active", target.active);
    //    //    OnInspectorGUI(implement);
    //    //}
    //}
}
