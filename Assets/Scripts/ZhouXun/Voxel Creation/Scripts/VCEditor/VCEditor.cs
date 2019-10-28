using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// VCEditor Life Cycle
/*
 * 		  <<<<<<<<< Init (Launch Game)             Load resources, Create objects for editor ( mat, ui, etc. )
 * 		  v                 
 * 		  v         Open >>>>> Ready >>>>          Open editor in game, enable editor objects.
 *        v                             v
 *        v     <<<<< NewScene          v          Create or load an iso document, create scene.
 *        v     v                       v
 *        v     v      (Edit)           v          Edit something.
 *        v     v                       v
 *        v     >>>> CloseScene         v          Shut an iso document. destroy scene, history, reset tools etc.
 *        v                             v 
 *        v    QuitFinally <<<< Quit <<<<          Close editor, disable editor objects, (resources not free here)
 *        v                   
 *        >>>>>>>> Destroy (Shut Game)             Free resources, Destroy objects
 */

public class VCEditor : MonoBehaviour
{
	private static VCEditor s_Instance = null;
	public static VCEditor Instance { get { return s_Instance; } }
	
	// State
	public static bool s_Active = false;
	public static bool s_Ready = false;
	private static float s_OpenTime = -1;
	private static float s_QuitTime = -1;

	// Scene
	public static int s_SceneId = 0;
	public static VCEScene s_Scene = null;

	// Mirror
	public static VCEMirrorSetting s_Mirror = null;

	// Status
	public static bool s_ProtectLock0 = false;
	public static bool s_ProtectLock1 = false;
	public static bool s_ProtectLock2 = false;
	public static bool s_ProtectLock3 = false;

	//
	// Select tool vars  ---------------------------------------------
	//
	// Select Material
	private static VCMaterial s_SelectedMaterial = null;
	private static int s_SelectedVoxelType = 0;
	public static int SelectedVoxelType { get { return s_SelectedVoxelType; } }
	public static VCMaterial SelectedMaterial
	{
		get { return s_SelectedMaterial; }
		set
		{
			s_SelectedMaterial = value;
			if ( value == null )
			{
				s_SelectedVoxelType = -1;
				//Debug.Log("Deselect material.");
			}
			else
			{
				s_SelectedVoxelType = s_Scene.m_IsoData.QueryVoxelType(value);
				//Debug.Log("Select material " + value.GUIDString 
				//	      + " ;  Voxel type = " + s_SelectedVoxelType.ToString());
			}
		}
	}
	// Select Decal
	private static ulong s_SelectedDecalGUID = 0;
	private static int s_SelectedDecalIndex = 0;
	public static int SelectedDecalIndex { get { return s_SelectedDecalIndex; } }
	public static ulong SelectedDecalGUID
	{
		get { return s_SelectedDecalGUID; }
		set
		{
			s_SelectedDecalGUID = value;
			if ( value == 0 )
			{
				s_SelectedDecalIndex = -1;
			}
			else
			{
				VCDecalAsset d = VCEAssetMgr.GetDecal(value);
				s_SelectedDecalIndex = s_Scene.m_IsoData.QueryNewDecalIndex(d);
			}
		}
	}
	public static VCDecalAsset SelectedDecal
	{
		get { return VCEAssetMgr.GetDecal(s_SelectedDecalGUID); }
		set
		{
			if ( value == null )
			{
				s_SelectedDecalGUID = 0;
				s_SelectedDecalIndex = -1;
			}
			else
			{
				s_SelectedDecalGUID = value.m_Guid;
				s_SelectedDecalIndex = s_Scene.m_IsoData.QueryNewDecalIndex(value);
			}
		}
	}

	// Select Brush
	public static VCEBrush[] SelectedBrushes
	{
		get
		{
			VCEBrush[] brs = new VCEBrush[0];
			if (s_Instance != null)
				brs = s_Instance.m_BrushGroup.GetComponentsInChildren<VCEBrush>();
			return brs;
		}
	}
	public static VCESelectComponent SelectComponentBrush
	{
		get
		{
			return s_Instance.m_BrushGroup.GetComponentInChildren<VCESelectComponent>();
		}
	}
	public static bool SelectedGeneralBrush
	{
		get
		{
			VCEBrush[] brs = SelectedBrushes;
			foreach (VCEBrush br in brs)
				if (br.m_Type == EVCEBrushType.General)
					return true;
			return false;
		}
	}
	public static void DeselectBrushes()
	{
		VCEBrush[] brs = SelectedBrushes;
		foreach ( VCEBrush br in brs )
			br.Cancel();
	}
	public static void DestroyBrushes()
	{
		VCEBrush[] brs = SelectedBrushes;
		foreach ( VCEBrush br in brs )
			GameObject.Destroy(br.gameObject);
	}

	// Select Transform Type
	public static EVCETransformType TransformType
	{
		get { return s_TransformType; }
		set { s_TransformType = value; }
	}
	private static EVCETransformType s_TransformType = EVCETransformType.None;

	// Select Part
	private static VCPartInfo s_SelectedPart = null;
	public static VCPartInfo SelectedPart
	{
		get { return s_SelectedPart; }
		set { s_SelectedPart = value; }
	}

	// Select Color
	public static Color32 SelectedColor
	{
		get
		{
			if ( Instance == null )
				return VCIsoData.BLANK_COLOR;
			if ( Instance.m_UI == null )
				return VCIsoData.BLANK_COLOR;
			Color rgb = Instance.m_UI.m_PaintColorPick.FinalColor;
			float a = Instance.m_UI.m_PaintBlendMethodSlider.sliderValue * 0.8f + 0.1f;
			return (Color32)(new Color(rgb.r, rgb.g, rgb.b, a));
		}
		set
		{
			if ( Instance == null )
				return;
			if ( Instance.m_UI == null )
				return;
			Instance.m_UI.m_PaintColorPick.FinalColor = value;
			Instance.m_UI.m_PaintBlendMethodSlider.sliderValue = Mathf.Clamp01((value.a - 0.2f) / 0.65f);
		}
	}

	// ..
	// Deselect all
	public static void DeselectAllTools()
	{
		if ( DocumentOpen() )
		{
			SelectedMaterial = null;
			SelectedPart = null;
			SelectedColor = Color.white;
			DeselectBrushes();
		}
	}
	public static void ClearSelections()
	{
		VCESelect[] sls = s_Instance.m_BrushGroup.GetComponentsInChildren<VCESelect>();
		foreach ( VCESelect sl in sls )
			sl.ClearSelection();
		if ( Instance != null && Instance.m_VoxelSelection != null )
			Instance.m_VoxelSelection.ClearSelection();
	}

	// End Select tool vars ------------------------------------------
	
	// Selection
	public static Dictionary<int, byte> VoxelSelection { get { return Instance.m_VoxelSelection.m_Selection; } }

	// Misc..
	public static int s_OutsideCameraCullingMask = 0;

	// Events --------------------------------------------------------
	public delegate void DNoParam ();
	public delegate void DSceneParam ( VCEScene scene );
	public static event DNoParam OnOpen;
    public static event DNoParam OnMakeCreation;
	public static event DNoParam OnReady;
	public static event DNoParam OnCloseComing;
	public static event DNoParam OnCloseFinally;
	public static event DSceneParam OnSceneCreate;
	public static event DSceneParam OnSceneClose;

	// Reset the Voxel Creation Editor - When GameLoading
	private static void Init()
	{
		s_Active = false;
		s_Ready = false;
		VCEAssetMgr.Init();
		VCEHistory.Init();
		GameObject ui = GameObject.Instantiate(s_Instance.m_UIPrefab) as GameObject;
		ui.transform.parent = s_Instance.m_EditorGO.transform;
		ui.transform.localPosition = Vector3.zero;
		ui.transform.localRotation = Quaternion.identity;
		ui.SetActive(true);
		s_Instance.m_UI = ui.GetComponent<VCEUI>();
		s_Instance.m_UI.Init();
	}
	// Reset the Voxel Creation Editor - When GameShutting
	private static void Destroy()
	{
		if ( s_Active ) QuitFinally();
		VCEAssetMgr.Destroy();
		VCEHistory.Destroy();
	}
	
	// Open the Voxel Creation Editor
	public static void Open()
	{
		if ( s_Instance == null ) return;
		if ( s_Active ) return;

		Debug.Log("VCE open.");
		// Material Icons
		// GenerateAllMaterialIcons();
		
		// Begin a default scene
		NewScene(VCConfig.FirstSceneSetting);
		
		s_Instance.m_EditorGO.SetActive(true);
		s_Instance.m_GLGroup.SetActive(true);
		s_Instance.m_UI.ShowUI();

		s_Active = true;
		s_OpenTime = 0f;
		
		// Close outside camera
		if ( Camera.main != null )
		{
			Debug.Log("VCE set main camera layer nothing.");
			VCGameMediator.CloseGameMainCamera();
		}

		if ( OnOpen != null )
			OnOpen();
	}

	// Quit the Voxel Creation Editor
	// <Just an action, run the close procedure, not immediately close>
	public static void Quit()
	{
		if ( !s_Active ) return;
		if ( s_QuitTime < 0 )
		{
			s_QuitTime = 0f;
			s_Ready = false;
			s_Instance.m_UI.AllBoxTweenOut();
			Debug.Log("VCE will close.");
			if ( OnCloseComing != null )
				OnCloseComing();
		}
	}
	// Quit the Voxel Creation finally
	public static void QuitFinally()
	{
		if ( s_Instance == null ) return;
		if ( !s_Active ) return;
		
		// close current scene
		CloseScene();
		
		// Reset gameobjects
		Debug.Log("VCE hide ui.");
		s_Instance.m_UI.HideUI();
		Debug.Log("VCE disable editor go.");
		s_Instance.m_GLGroup.SetActive(false);
		s_Instance.m_EditorGO.SetActive(false);

		// Material Icons
		// FreeAllMaterialIcons();
		
		// Revert outside camera
		Debug.Log("VCE trying to revert outside camera.");
		VCGameMediator.RevertGameMainCamera();
		Debug.Log("VCE revert outside camera succeed.");

		// Reset states
		s_Active = false;
		s_Ready = false;

		if ( OnCloseFinally != null )
			OnCloseFinally();
		Debug.Log("VCE is closed.");
	}
	// begin a blank scene
	public static void NewScene(VCESceneSetting setting)
	{
		if ( s_Instance == null ) return;

		// Close current scene first
		CloseScene();

		// New VCEScene
		s_SceneId++;
		s_Scene = new VCEScene (setting);
		s_Scene.BuildScene();
		Debug.Log("VCE new scene. sceneid = " + s_SceneId.ToString());

		// After Scene Changed
		AfterSceneChanged(setting);
		if ( OnSceneCreate != null )
			OnSceneCreate(s_Scene);

		// Show status
		string s = "New scene".ToLocalizationString() + " ";
		string[] scenepaths = s_Scene.m_IsoData.m_HeadInfo.ScenePaths();
		foreach ( string sp in scenepaths )
			s += "[" + sp.ToLocalizationString() + "] ";
		s += "is ready".ToLocalizationString() + " !";
		VCEStatusBar.ShowText(s, 10, true);
	}

	// begin a template scene
	public static void NewScene(VCESceneSetting setting, int template)
	{
		if ( s_Instance == null ) return;

		// Close current scene first
		CloseScene();

		// New VCEScene
		s_SceneId++;
		s_Scene = new VCEScene (setting, template);
		s_Scene.BuildScene();
		Debug.Log("VCE new scene. sceneid = " + s_SceneId.ToString());

		// After Scene Changed
		AfterSceneChanged(setting);
		if ( OnSceneCreate != null )
			OnSceneCreate(s_Scene);

		// Show status
		string s = "New scene".ToLocalizationString() + " ";
		string[] scenepaths = s_Scene.m_IsoData.m_HeadInfo.ScenePaths();
		foreach ( string sp in scenepaths )
			s += "[" + sp.ToLocalizationString() + "] ";
		s += "is ready".ToLocalizationString() + " !";
		VCEStatusBar.ShowText(s, 10, true);
	}
	
	// load an iso
	public static void LoadIso(string path)
	{
		if ( s_Instance == null ) return;
		
		// Close current scene first
		CloseScene();
		
		// Load a VCEScene from an specified iso
		try
		{
			s_SceneId++;
			s_Scene = new VCEScene (path);

			// Build scene
			s_Scene.BuildScene();

			// After Scene Changed
			AfterSceneChanged(s_Scene.m_Setting);
		}
		catch (Exception)
		{
			NewScene(VCConfig.FirstSceneSetting);
			VCEMsgBox.Show(VCEMsgBoxType.CORRUPT_ISO);
			return;
		}
		
		if ( OnSceneCreate != null )
			OnSceneCreate(s_Scene);

		Debug.Log("VCE load iso " + path + " sceneid = " + s_SceneId.ToString());
		// Show status
		VCEStatusBar.ShowText("Load".ToLocalizationString() + " ISO [" + path + "] " + "Complete".ToLocalizationString() + " !", 10, true);
	}
	
	private static void AfterSceneChanged(VCESceneSetting setting)
	{
		// Mirror
		s_Mirror = new VCEMirrorSetting ();
		s_Mirror.Reset(setting.m_EditorSize.x, setting.m_EditorSize.y, setting.m_EditorSize.z);

		// Mesh manager
		VCEditor.Instance.m_MeshMgr.m_VoxelSize = s_Scene.m_Setting.m_VoxelSize;
		VCEditor.Instance.m_MeshMgr.m_MeshMat = s_Scene.m_TempIsoMat.m_EditorMat;
		
		// Set gameobject parameters
		// Camera
		float scale = setting.m_EditorSize.ToVector3().magnitude * setting.m_VoxelSize;
		s_Instance.m_MainCamera.nearClipPlane = setting.m_VoxelSize * 0.5f;
		s_Instance.m_MainCamera.farClipPlane = scale * 5F;
		VCECamera vce_camera = s_Instance.m_MainCamera.GetComponent<VCECamera>();
		if (s_Scene.m_IsoData.m_Voxels.Count == 0)
		{
			vce_camera.BeginTarget = setting.m_EditorSize.ToVector3() * setting.m_VoxelSize * 0.5F;
			vce_camera.BeginTarget.y = 0;
			vce_camera.BeginDistance = new Vector2 (setting.m_EditorSize.x, setting.m_EditorSize.z).magnitude * setting.m_VoxelSize * 1.2F;
			vce_camera.MinDistance = setting.m_VoxelSize * 0.5f;
			vce_camera.MaxDistance = scale * 3F;
			vce_camera.Reset();
		}
		else
		{
			vce_camera.BeginTarget = setting.m_EditorSize.ToVector3() * setting.m_VoxelSize * 0.5F;
			vce_camera.BeginDistance = setting.m_EditorSize.ToVector3().magnitude * setting.m_VoxelSize * 1.2F;
			vce_camera.MinDistance = setting.m_VoxelSize * 0.5f;
			vce_camera.MaxDistance = scale * 3F;
			vce_camera.Reset();
		}
		// GLs
		GLGridPlane[] gps = s_Instance.m_GLGroup.GetComponentsInChildren<GLGridPlane>(true);
		GLBound[] gbs = s_Instance.m_GLGroup.GetComponentsInChildren<GLBound>(true);
		foreach ( GLGridPlane gp in gps )
		{
			gp.m_CellCount = new IntVector3 (setting.m_EditorSize.x, setting.m_EditorSize.y, setting.m_EditorSize.z);
			gp.m_CellSize = Vector3.one * setting.m_VoxelSize;
			gp.m_MajorGridInterval = setting.m_MajorInterval;
			gp.m_MinorGridInterval = setting.m_MinorInterval;
            gp.m_Fdisk = setting.m_Category == EVCCategory.cgDbSword;
		}
		foreach ( GLBound gb in gbs )
		{
			gb.m_Bound = new Bounds (Vector3.zero, Vector3.zero);
			gb.m_Bound.SetMinMax(Vector3.zero, setting.m_EditorSize.ToVector3() * setting.m_VoxelSize);
		}
		
		// UIs
		s_Instance.m_UI.OnSceneCreate();
		
		// VARS
		// Recent vars
		VCESelectMethod_Box.s_RecentDepth = 1;
		VCESelectMethod_Box.s_RecentFeatherLength = 0;
		VCESelectMethod_Box.s_RecentPlaneFeather = true;

		// Selected
		SelectedColor = Color.white;
	}
	
	// close current scene
	private static void CloseScene()
	{
		if ( s_Scene != null )
		{
			Debug.Log("VCE close scene. sceneid = " + s_SceneId.ToString());
			if ( OnSceneClose != null )
				OnSceneClose(s_Scene);
		}

		// Selection
		DeselectAllTools();
		DestroyBrushes();
		ClearSelections();
		
		// Destroy scene data
		DestroySceneData();
		
		// Temporary materials
		VCEAssetMgr.ClearTempMaterials();

		// Temporary decals
		VCEAssetMgr.ClearTempDecals();
		
		// Reset Some vars
		// Ref plane
		s_Instance.m_UI.OnSceneClose();
		VCERefPlane.Reset();
		
		// History manager
		VCEHistory.Clear();
		VCEHistory.s_Modified = false;

		// Mirror
		s_Mirror = null;
	}
	// destroy current scene data - called by CloseScene()
	private static void DestroySceneData()
	{
		if ( s_Scene != null )
		{
			s_Scene.Destroy();
			s_Scene = null;
		}
	}
	public static bool DocumentOpen()
	{
		if ( VCEditor.s_Scene == null )
			return false;
		if ( VCEditor.s_Scene.m_IsoData == null )
			return false;
		if ( VCEditor.s_Scene.m_IsoData.m_Components == null )
			return false;
		if ( VCEditor.s_Scene.m_IsoData.m_Voxels == null )
			return false;
		if ( VCEditor.s_Scene.m_IsoData.m_Colors == null )
			return false;
		return true;
	}
	
	public static bool s_ConnectedToGame = false;
	public static bool s_MultiplayerMode = false;
	public static int MakeCreation()
	{
		if ( !s_ConnectedToGame )
		{
			Debug.LogWarning("You can not make creation outside the game!");
			return -2;
		}
		// Multi player
		if ( s_MultiplayerMode )
		{
			if ( !VCConfig.s_Categories.ContainsKey(s_Scene.m_IsoData.m_HeadInfo.Category) )
				return -1;
			byte[] isodata =   s_Scene.m_IsoData.Export();
			if ( isodata == null || isodata.Length <= 0 )
			{
				return -1;
			}
            ulong hash = CRC64.Compute(isodata);
            ulong fileId = SteamWorkShop.GetFileHandle(hash);
            VCGameMediator.SendIsoDataToServer(s_Scene.m_IsoData.m_HeadInfo.Name, s_Scene.m_IsoData.m_HeadInfo.SteamDesc,
			                                   s_Scene.m_IsoData.m_HeadInfo.SteamPreview, isodata, SteamWorkShop.AddNewVersionTag(s_Scene.m_IsoData.m_HeadInfo.ScenePaths()),true, fileId);

			return 0;
		}
		// Single player
		else
		{
			CreationData new_creation = new CreationData ();
			new_creation.m_ObjectID = CreationMgr.QueryNewId();
			new_creation.m_RandomSeed = UnityEngine.Random.value;
			new_creation.m_Resource = s_Scene.m_IsoData.Export();
			new_creation.ReadRes();
			
			// Attr
			new_creation.GenCreationAttr();
			if ( new_creation.m_Attribute.m_Type == ECreation.Null )
			{
				Debug.LogWarning("Creation is not a valid type !");
				new_creation.Destroy();
				return -1;
			}
			
			// SaveRes
			if ( new_creation.SaveRes() )
			{
				new_creation.BuildPrefab();
				new_creation.Register();
				CreationMgr.AddCreation(new_creation);
                ItemAsset.ItemObject item;
                int send_retval = new_creation.SendToPlayer(out item);
				
				Debug.Log("Make creation succeed !");
				if ( send_retval == 0 )
					return -1;	// Error
                else if (send_retval == -1)
                    return -4;	// Item Package Full
                else
                {
                    if (OnMakeCreation != null)
                        OnMakeCreation();
                    return 0;	// Succeed
                }
			}
			else
			{
				Debug.LogWarning("Save creation resource file failed !");
				new_creation.Destroy();
				return -3;
			}
		}
	}

    public static int MakeCreation(string path)
    {
        TextAsset aseet = Resources.Load<TextAsset>(path);
        VCIsoData iso = new VCIsoData();
        iso.Import(aseet.bytes, new VCIsoOption(false));
        // Multi player
        if (s_MultiplayerMode)
        {
            if (!VCConfig.s_Categories.ContainsKey(iso.m_HeadInfo.Category))
                return -1;
            byte[] isodata = iso.Export();
            if (isodata == null || isodata.Length <= 0)
            {
                return -1;
            }
            ulong hash = CRC64.Compute(isodata);
            ulong fileId = SteamWorkShop.GetFileHandle(hash);
            VCGameMediator.SendIsoDataToServer(iso.m_HeadInfo.Name, iso.m_HeadInfo.SteamDesc,
                                               iso.m_HeadInfo.SteamPreview, isodata, SteamWorkShop.AddNewVersionTag(iso.m_HeadInfo.ScenePaths()), true, fileId,true);

            return 0;
        }
        else
        {
            CreationData new_creation = new CreationData();
            new_creation.m_ObjectID = CreationMgr.QueryNewId();
            new_creation.m_RandomSeed = UnityEngine.Random.value;
            new_creation.m_Resource = iso.Export();
            new_creation.ReadRes();

            // Attr
            new_creation.GenCreationAttr();
            if (new_creation.m_Attribute.m_Type == ECreation.Null)
            {
                Debug.LogWarning("Creation is not a valid type !");
                new_creation.Destroy();
                return -1;
            }

            // SaveRes
            if (new_creation.SaveRes())
            {
                new_creation.BuildPrefab();
                new_creation.Register();
                CreationMgr.AddCreation(new_creation);
                ItemAsset.ItemObject item;
                int send_retval = new_creation.SendToPlayer(out item);

                Debug.Log("Make creation succeed !");
                if (send_retval == 0)
                    return -1;  // Error
                else if (send_retval == -1)
                    return -4;  // Item Package Full
                else
                {
                    if (OnMakeCreation != null)
                        OnMakeCreation();
                    return 0;   // Succeed
                }
            }
            else
            {
                Debug.LogWarning("Save creation resource file failed !");
                new_creation.Destroy();
                return -3;
            }
        }       
    }

    public static void CopyCretion(ECreation type)
    {


        Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
        if (null == pkg)
            return;
        List<int> creationInstanceid = pkg.package.GetCreationInstanceId(type);
        if (creationInstanceid == null || creationInstanceid.Count == 0)
            return;
        CreationData cd = CreationMgr.GetCreation(creationInstanceid[0]);

        if (Pathea.PeGameMgr.IsMulti)
        {
            ulong hash = CRC64.Compute(cd.m_Resource);
            ulong fileId = SteamWorkShop.GetFileHandle(hash);
            VCGameMediator.SendIsoDataToServer(cd.m_IsoData.m_HeadInfo.Name, cd.m_IsoData.m_HeadInfo.SteamDesc,
                                               cd.m_IsoData.m_HeadInfo.SteamPreview, cd.m_Resource, SteamWorkShop.AddNewVersionTag(cd.m_IsoData.m_HeadInfo.ScenePaths()), true, fileId,true);
        }
        else
        {
            CreationData new_creation = new CreationData();
            new_creation.m_ObjectID = CreationMgr.QueryNewId();
            new_creation.m_RandomSeed = UnityEngine.Random.value;
            new_creation.m_Resource = cd.m_Resource;
            new_creation.ReadRes();

            // Attr
            new_creation.GenCreationAttr();
            if (new_creation.m_Attribute.m_Type == ECreation.Null)
            {
                Debug.LogWarning("Creation is not a valid type !");
                new_creation.Destroy();
                return;
            }

            // SaveRes
            if (new_creation.SaveRes())
            {
                new_creation.BuildPrefab();
                new_creation.Register();
                CreationMgr.AddCreation(new_creation);
                ItemAsset.ItemObject item;
                int send_retval = new_creation.SendToPlayer(out item);

                Debug.Log("Make creation succeed !");
                if (send_retval == 0)
                    return; // Error
                else if (send_retval == -1)
                    return; // Item Package Full
                else
                    return; // Succeed
            }
            else
            {
                Debug.LogWarning("Save creation resource file failed !");
                new_creation.Destroy();
                return;
            }
        }

    }

    public static void CopyCretion(int instanceID) 
    {
        CreationData new_creation = new CreationData();
        new_creation.m_ObjectID = CreationMgr.QueryNewId();
        new_creation.m_RandomSeed = UnityEngine.Random.value;
        CreationData cd = CreationMgr.GetCreation(instanceID);
        if(null == cd)
            return;
        new_creation.m_Resource = cd.m_Resource;
        new_creation.ReadRes();

        // Attr
        new_creation.GenCreationAttr();
        if (new_creation.m_Attribute.m_Type == ECreation.Null)
        {
            Debug.LogWarning("Creation is not a valid type !");
            new_creation.Destroy();
            return;
        }

        // SaveRes
        if (new_creation.SaveRes())
        {
            new_creation.BuildPrefab();
            new_creation.Register();
            CreationMgr.AddCreation(new_creation);
            ItemAsset.ItemObject item;
            int send_retval = new_creation.SendToPlayer(out item);

            Debug.Log("Make creation succeed !");
            if (send_retval == 0)
                return;	// Error
            else if (send_retval == -1)
                return;	// Item Package Full
            else
                return;	// Succeed
        }
        else
        {
            Debug.LogWarning("Save creation resource file failed !");
            new_creation.Destroy();
            return;
        }
    }

    
	
	#region INSPECTOR_VALS
	public GameObject m_EditorGO;
	public GameObject m_UIPrefab;
	public VCEUI m_UI;
	public GUISkin m_GUISkin;
	public Camera m_MainCamera;
	public Camera m_CaptureCamera;
	public GameObject m_GLGroup;
	public GameObject m_MirrorGroup;
	public GameObject m_BrushGroup;
	public GLNearVoxelIndicator m_NearVoxelIndicator;
	public VCEVoxelSelection m_VoxelSelection;
	public GameObject m_PartGroup;
	public GameObject m_DecalGroup;
	public GameObject m_EffectGroup;
	public Material m_TempIsoMat;
	public Material m_HolographicMat;
	public GameObject m_WaterMaskPrefab;
	public VCMeshMgr m_MeshMgr;
	public VCEUISceneGizmoCam m_SceneGizmo;
	public GameObject m_CreationGroup;
	public Transform m_MassCenterTrans;
	public bool m_CheatWhenMakeCreation = false;
	#endregion
	
	#region U3D_INTERNAL_FUNCS
	// Use this for initialization
	void Awake ()
	{
		DontDestroyOnLoad(this.gameObject);
		s_Instance = this;
		VCEditor.Init();
	}

	// Use this for initialization
	void Start ()
	{
	
	}

	// Update is called once per frame
	void Update ()
	{
		VCGameMediator.Update();	// Update mediator
		if ( !s_Active ) return;
		RunOpenCloseTime();

		RenderSettings.fog = false;
		RenderSettings.ambientLight = Color.white * 0.35f;
		if ( DocumentOpen() )
		{
			UserInput();
			UpdateBrushLogic();
			UpdateMirror();
			s_Scene.PreventRenderTextureLost();
			s_Scene.m_MeshComputer.ReqMesh();
		}
#if false
		if ( Input.GetKeyDown(KeyCode.C) && s_Ready )
			Debug.LogWarning("Color count: " + s_Scene.m_IsoData.m_Colors.Count);
#endif
	}

	void UpdateBrushLogic ()
	{
		if ( SelectedGeneralBrush )
			ClearSelections();
	}

	void UpdateMirror ()
	{
		if ( Instance.m_UI.m_PartTab.isChecked )
		{
			if ( SelectedPart != null )
			{
				s_Mirror.m_Mask = (byte)SelectedPart.m_MirrorMask;
			}
			else
			{
				VCESelectComponent sel = null;
				foreach ( VCEBrush brush in SelectedBrushes )
				{
					if ( brush is VCESelectComponent )
					{
						sel = brush as VCESelectComponent;
						break;
					}
				}
				if ( sel != null )
				{
					if ( sel.m_Selection.Count == 0 )
					{
						s_Mirror.m_Mask = 7;
					}
					else
					{
						int mask = 7;
						foreach ( VCESelectComponent.SelectInfo si in sel.m_Selection )
						{
							VCPartData part = si.m_Component.m_Data as VCPartData;
							if ( part != null )
								mask &= (VCConfig.s_Parts[part.m_ComponentId].m_MirrorMask);
						}
						s_Mirror.m_Mask = (byte)mask;
					}
				}
				else
				{
					s_Mirror.m_Mask = 7;
				}
			}
		}
		else if ( Instance.m_UI.m_MaterialTab.isChecked )
		{
			s_Mirror.m_Mask = 7;
		}
		else if ( Instance.m_UI.m_PaintTab.isChecked )
		{
			s_Mirror.m_Mask = 7;
		}
		else if ( Instance.m_UI.m_DecalTab.isChecked )
		{
			s_Mirror.m_Mask = 7;
		}
		else
		{
			s_Mirror.m_Mask = 0;
		}
	}

	// Input detect
	void UserInput ()
	{
		if ( s_Ready )
		{
			KeyEscape();
			KeyFocus();
			KeyResetCam();
			UIHotKeys();
		}
	}
	
	void KeyEscape ()
	{
		if ( Input.GetKeyDown(KeyCode.Escape) && !SelectedGeneralBrush && !s_ProtectLock0 )
			m_UI.OnQuitClick();
	}
	
	void KeyFocus ()
	{
		if ( ( Input.GetKeyDown(KeyCode.F) || VCEInput.s_RightDblClick ) && !UICamera.inputHasFocus && !Input.GetMouseButton(0) && !s_ProtectLock0 )
		{
			VCEMath.DrawTarget dtar;
			if ( VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out dtar, VCEMath.MC_ISO_VALUE) )
			{
				Vector3 newtar = (dtar.snapto.ToVector3() + dtar.cursor.ToVector3() + Vector3.one) * 0.5f * s_Scene.m_Setting.m_VoxelSize;
				// New look distance to keep the origin distance
				VCECamera cam_scr = m_MainCamera.GetComponent<VCECamera>();
				float dist = cam_scr.Distance;
				// Maximum distance
				float maxdist = s_Scene.m_Setting.m_VoxelSize * 10.0f;
				if ( dist > maxdist )
					dist = maxdist;
				cam_scr.SetTarget(newtar);
				cam_scr.SetDistance(dist);
			}
		}
	}
	
	void KeyResetCam ()
	{
		if ( Input.GetKeyDown(KeyCode.R) && !UICamera.inputHasFocus && !Input.GetMouseButton(0) && !s_ProtectLock0 )
		{
			VCECamera cam_scr = m_MainCamera.GetComponent<VCECamera>();
			cam_scr.SmoothReset();
		}
	}

	void UIHotKeys ()
	{
		if ( !Input.GetMouseButton(0) && !s_ProtectLock0 )
		{
			if ( VCEInput.s_Undo )
			{
				if ( m_UI.m_UndoButton.enabled )
					m_UI.OnUndoClick();
			}
			if ( VCEInput.s_Redo )
			{
				if ( m_UI.m_RedoButton.enabled )
					m_UI.OnRedoClick();
			}
			if ( VCEInput.s_Delete )
			{
				if ( m_UI.m_DeleteButton.enabled )
					m_UI.OnDeleteClick();
			}
			if ( Input.GetKeyDown(KeyCode.F1) )
			{
				m_UI.OnTutorialClick();
			}
		}
	}
	
	// Destructor
	void OnDestroy ()
	{
		VCEditor.Destroy();
		s_Instance = null;
	}
	#endregion
		
	void RunOpenCloseTime()
	{
		if ( s_OpenTime >= 0f )
		{
			s_OpenTime += Time.deltaTime;
			if ( s_OpenTime > 1.6f )
			{
				s_Ready = true;
				s_OpenTime = -1f;
				if ( OnReady != null )
					OnReady();
			}
		}
		if ( s_QuitTime >= 0f )
		{
			s_QuitTime += Time.deltaTime;
			if ( s_QuitTime > 1.0f )
			{
				QuitFinally();
				s_QuitTime = -1f;
			}
		}
	}
	
	private static void GenerateAllMaterialIcons()
	{
		foreach ( KeyValuePair<ulong, VCMaterial> kvp in VCEAssetMgr.s_Materials )
		{
			VCMatGenerator.Instance.GenMaterialIcon(kvp.Value);
		}
	}
	private static void FreeAllMaterialIcons()
	{
		foreach ( KeyValuePair<ulong, VCMaterial> kvp in VCEAssetMgr.s_Materials )
		{
			kvp.Value.FreeIcon();
		}		
	}
}
