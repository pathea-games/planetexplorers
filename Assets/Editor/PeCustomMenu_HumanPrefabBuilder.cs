using UnityEditor;
using UnityEngine;
using System.IO;

public partial class PeCustomMenuc : EditorWindow
{	
    [MenuItem("Assets/HumanPrefab/BuildHuman_Male")]
    static void BuildMalePrefabs()
	{
		BuildHumanPrefab("Prefab/PlayerPrefab/MaleModel");
    }

	[MenuItem("Assets/HumanPrefab/BuildHuman_Female")]
	static void BuildFemalePrefabs()
	{
		BuildHumanPrefab("Prefab/PlayerPrefab/FemaleModel");
	}

	static void BuildHumanPrefab(string templatePath)
	{
		GameObject template = Resources.Load(templatePath) as GameObject;
		if(null == template)
		{
			Debug.LogError("Can't find model:" + templatePath);
			return;
		}

		GameObject[] selectedObjArray = Selection.gameObjects;
		
		if (selectedObjArray == null && selectedObjArray.Length == 0)
		{
			Debug.LogError("no model selected.");
			return;
		}
		
		foreach(GameObject obj in selectedObjArray)
		{
			if(null != obj)
			{
				BuildHumanPrefab(obj, template);
			}
		}
	}

	static void BuildHumanPrefab(GameObject modelAsset, GameObject template)
    {
		if(File.Exists("Assets/Resources/Prefab/Human/" + modelAsset.name + ".prefab"))
		{
			Debug.LogError(modelAsset.name + "already exist.");
			return;
		}

		bool successful = true;

		GameObject humanPrefab = GameObject.Instantiate(template);
		humanPrefab.name = modelAsset.name;

        GameObject model = GameObject.Instantiate(modelAsset);
        model.name = "Model";

		if (!ProcessModel(humanPrefab, model))
		{
			Debug.LogError("process model failed.");
			successful = false;
		}

		if(successful)
		{
			humanPrefab.GetComponent<WhiteCat.BoneCollector>().Reset();
			humanPrefab.AddComponent<ReplaceShaderU3D>();
			PrefabUtility.CreatePrefab("Assets/Resources/Prefab/Human/" + humanPrefab.name + ".prefab", humanPrefab);
		}

		GameObject.DestroyImmediate(humanPrefab);
		GameObject.DestroyImmediate(model);
    }

	static bool SynBone(Transform sourceTrans, Transform destinationRoot)
	{
		Transform childBone = PETools.PEUtil.GetChild(destinationRoot, sourceTrans.name, true);
		if(null == childBone)
		{
			if(null == sourceTrans.parent)
				return true;

			Transform parentBone = PETools.PEUtil.GetChild(destinationRoot, sourceTrans.parent.name, true);
			if(null == parentBone)
			{
				if("Model" == sourceTrans.parent.name)
				{
					parentBone = PETools.PEUtil.GetChild(destinationRoot, "Ragdoll", true);
					if(null == parentBone)
					{
						Debug.LogError("Can't find parent:" + sourceTrans.parent.name);
						return false;
					}
				}
			}
			childBone = GameObject.Instantiate(sourceTrans.gameObject).transform;
			childBone.name = sourceTrans.transform.name;
			childBone.parent = parentBone;
		}
		childBone.localPosition = sourceTrans.localPosition;
		childBone.localRotation = sourceTrans.localRotation;
		childBone.localScale = sourceTrans.localScale;

		return true;
	}

	static bool SynMeshRenderer(Transform destinationRoot)
	{
		SkinnedMeshRenderer[] meshRenderers = destinationRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
		for(int renderIndex = 0; renderIndex < meshRenderers.Length; renderIndex++)
		{
			SkinnedMeshRenderer renderer = meshRenderers[renderIndex];
			renderer.rootBone = PETools.PEUtil.GetChild(destinationRoot, renderer.rootBone.name, true);
			if(null == renderer.bones)
				continue;
			Transform[] newBones = new Transform[renderer.bones.Length];
			for(int i = 0; i < renderer.bones.Length; i++)
			{
				if(null == renderer.bones[i])
					continue;
				Transform findBone = PETools.PEUtil.GetChild(destinationRoot, renderer.bones[i].name, true);
				if(null == findBone)
				{
					Debug.LogError("Can't find renderer'bone:" + renderer.bones[i].name);
					return false;
				}
				newBones[i] = findBone;
			}
			renderer.bones = newBones;
		}

		return true;
	}

	static bool ProcessModel(GameObject prefabObj, GameObject model)
	{
		Transform modelRoot = prefabObj.GetComponentInChildren<PEModelController>().transform;
		Transform ragdollRoot = prefabObj.GetComponentInChildren<PERagdollController>().transform;

		modelRoot.GetComponent<Animator>().avatar = model.GetComponent<Animator>().avatar;

		Transform[] modelTrans = model.GetComponentsInChildren<Transform>();
		for(int i = 0; i < modelTrans.Length; i++)
		{
			if(!SynBone(modelTrans[i], modelRoot)) return false;
			if(!SynBone(modelTrans[i], ragdollRoot)) return false;
		}

		if(!SynMeshRenderer(modelRoot)) return false;
		if(!SynMeshRenderer(ragdollRoot)) return false;
		return true;
	}

//    static bool ProcessIk(GameObject prefabObj, GameObject model)
//    {
//        model.transform.parent = prefabObj.transform;
//        return true;
//    }
//
//    static bool ProcessRagdoll(GameObject prefabObj, GameObject model)
//    {
//        return true;
//    }
//
//    static bool ProcessOther(GameObject prefabObj, GameObject model)
//    {
//        return true;
//    }
}
