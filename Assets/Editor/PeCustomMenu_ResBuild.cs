using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public partial class PeCustomMenuc : EditorWindow
{
	static bool bNoTrack = false;
	//static bool bRaw = true;

    static void ChangeCharacter2RigidbodyCollider()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            GameObject obj = Selection.objects[i] as GameObject;

            if (obj != null)
            {
                AiPhysicsCharacterMotor physicMotor = obj.GetComponent<AiPhysicsCharacterMotor>();
                if (!physicMotor)
                {
                    physicMotor = obj.AddComponent<AiPhysicsCharacterMotor>();

                    CapsuleCollider capsule = obj.GetComponent<CapsuleCollider>();
                    CharacterController controller = obj.GetComponent<CharacterController>();
                    if (capsule != null && controller != null)
                    {
                        capsule.center = controller.center;
                        capsule.radius = controller.radius;
                        capsule.height = controller.height;
                    }

                    GameObject.DestroyImmediate(obj.GetComponent<AiNormalCharacterMotor>(), true);
                    GameObject.DestroyImmediate(obj.GetComponent<CharacterController>(), true);
                }

                physicMotor.useCentricGravity = true;
            }
        }
    }
	[MenuItem("Assets/Test/ToShellShader")]
	static void ChangeToShellShader(){
		CorruptShaderReplacement.ReadShaderPairsTbl (true, false);
		for(int i = 0; i < Selection.gameObjects.Length; i++){
			if(CorruptShaderReplacement.ReplaceShaderEntityWithShell(Selection.gameObjects[i])){
				Debug.Log("Succeed in ToShellShader:"+Selection.gameObjects[i].name);
			}
		}
	}
	[MenuItem("Assets/Test/ToEntityShader")]
	static void ChangeToEntityShader(){
		CorruptShaderReplacement.ReadShaderPairsTbl (false, true);
		for(int i = 0; i < Selection.gameObjects.Length; i++){
			if(CorruptShaderReplacement.ReplaceShaderShellWithEntity(Selection.gameObjects[i])){
				Debug.Log("Succeed in ToEntityShader:"+Selection.gameObjects[i].name);
			}
		}
	}

	static void CreateAssetBundlesEach(string dirName)
	{
		string dirSubPath = dirName == null ? "" : (dirName+"/");
		string dirPath = GameConfig.AssetBundlePath+dirSubPath;
		
        if(!Directory.Exists(dirPath))
			Directory.CreateDirectory(dirPath);

		int nGo = Selection.gameObjects.Length;
		CorruptShaderReplacement.ReadShaderPairsTbl (true, true);
		for (int i = 0; i < nGo; i++) {
			CorruptShaderReplacement.ReplaceShaderEntityWithShell(Selection.gameObjects[i]);
		}

        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
		int nObj = Selection.objects.Length;
		for(int i = 0; i < nObj; i++)
		{
			//ExportResource(dirPath, Selection.objects[i]);
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = Selection.objects[i].name + ".unity3d";
            build.assetNames = new string[] { AssetDatabase.GetAssetPath(Selection.objects[i]) };
            builds.Add(build);
		}
        BuildPipeline.BuildAssetBundles(dirPath, builds.ToArray());

		for (int i = 0; i < nGo; i++) {
			CorruptShaderReplacement.ReplaceShaderShellWithEntity(Selection.gameObjects[i]);
		}
	}

    [MenuItem("PeCustomMenu/ChangeCharacter2Rigidbody")]
    static void ChangeCharacter2RigidbodyColliderMenu()
    {
        ChangeCharacter2RigidbodyCollider();
    }

    [MenuItem("Assets/AssetBundles/Clean Cache")]
    static void CleanAssetBundleCache()
    {
        Caching.CleanCache();
    }

    [MenuItem("Assets/AssetBundles/Build AssetBundles for Item")]
    static void CreateItemAssetBundlesEach()
    {
        CreateAssetBundlesEach(GameConfig.AssetsManifest_Item);
    }

    [MenuItem("Assets/AssetBundles/Build AssetBundles for Monster")]
    static void CreateMonsterAssetBundlesEach()
    {
        CreateAssetBundlesEach(GameConfig.AssetsManifest_Monster);
    }

    [MenuItem("Assets/AssetBundles/Build AssetBundles for Tower")]
    static void CreateTowerAssetBundlesEach()
    {
        CreateAssetBundlesEach(GameConfig.AssetsManifest_Tower);
    }

    [MenuItem("Assets/AssetBundles/Build AssetBundles for Native")]
    static void CreatePujaAssetBundlesEach()
    {
        CreateAssetBundlesEach(GameConfig.AssetsManifest_Puja);
    }

    [MenuItem("Assets/AssetBundles/Build AssetBundles for Alien")]
    static void CreateAlienAssetBundlesEach()
    {
        CreateAssetBundlesEach(GameConfig.AssetsManifest_Alien);
    }

    [MenuItem("Assets/AssetBundles/Build AssetBundles for Group")]
    static void CreateGroupAssetBundlesEach()
    {
        CreateAssetBundlesEach(GameConfig.AssetsManifest_Group);
    }
	
    [MenuItem("Assets/AssetBundles/Build AssetBundles for Player")]
    static void CreatePlayerAssetBundlesEach()
    {
        CreateAssetBundlesEach(GameConfig.AssetsManifest_Player);
    }
	
    [MenuItem("Assets/AssetBundles/Build AssetBundles for NPC")]
    static void CreateNpcAssetBundlesEach()
    {
        CreateAssetBundlesEach(GameConfig.AssetsManifest_Npc);
    }

	static void ExportResource (string dir, UnityEngine.Object justThisObj) {        // Bring up save panel 
		UnityEngine.Object mainAsset;
		UnityEngine.Object[] assets; 
		if(justThisObj == null)
		{
			mainAsset = Selection.activeObject;
			assets = bNoTrack ? Selection.objects : Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
		}
		else
		{
			mainAsset = justThisObj;
			assets = new UnityEngine.Object[]{justThisObj};
		}

		string path = dir+mainAsset.name+".unity3d";
		// Build the resource file from the active selection.
		if(bNoTrack)
		{
			BuildPipeline.BuildAssetBundle(mainAsset, assets , path); 
		}
		else
		{
			BuildPipeline.BuildAssetBundle(mainAsset, assets, path, 
				BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.StandaloneWindows64);
		}
		//Selection.objects = assets;
	}
}
