#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;
using RootMotion.FinalIK;
using AnimFollow;
using PEIK;
using WhiteCat;
using Pathea;

using WhiteCat.UnityExtension;
using PETools;

namespace Pathea
{
	public class BiologyViewRoot : MonoBehaviour 
	{
		public PEModelController modelController;
		public PERagdollController ragdollController;
		public IK[] ikArray;
		public IKFlashLight ikFlashLight;
		public FullBodyBipedIK fbbik;
		public GrounderFBBIK grounderFBBIK;
		public HumanPhyCtrl humanPhyCtrl;
		public IKAimCtrl	ikAimCtrl;
		public IKAnimEffectCtrl ikAnimEffectCtrl;
		public IKDrive ikDrive;
		public PEDefenceTrigger defenceTrigger;
		public PEPathfinder pathFinder;
		public PEMotor	motor;
		public Steer3D.SteerAgent steerAgent;
		public AnimFollow_AF animFollow_AF;
		public BeatParam beatParam;
		public MoveParam moveParam;
		public PEBarrelController barrelController;
		public BillBoard billBoard;
		public ArmorBones armorBones;
		public PEVision[] visions;
		public PEHearing[] hears;
		public PENative native;
		public PEMonster monster;

		public void Reset()
		{
			modelController = PEUtil.GetCmpt<PEModelController>(transform);
			ragdollController = PEUtil.GetCmpt<PERagdollController>(transform);
			ikArray = PEUtil.GetCmpts<IK>(transform);
			ikFlashLight = PEUtil.GetCmpt<IKFlashLight>(transform);
			fbbik = PEUtil.GetCmpt<FullBodyBipedIK>(transform);
			grounderFBBIK = PEUtil.GetCmpt<GrounderFBBIK>(transform);
			humanPhyCtrl = PEUtil.GetCmpt<HumanPhyCtrl>(transform);
			ikAimCtrl = PEUtil.GetCmpt<IKAimCtrl>(transform);
			ikAnimEffectCtrl = PEUtil.GetCmpt<IKAnimEffectCtrl>(transform);
			ikDrive = PEUtil.GetCmpt<IKDrive>(transform);
			defenceTrigger = PEUtil.GetCmpt<PEDefenceTrigger>(transform);
			pathFinder = PEUtil.GetCmpt<PEPathfinder>(transform);
			motor = PEUtil.GetCmpt<PEMotor>(transform);
			steerAgent = PEUtil.GetCmpt<Steer3D.SteerAgent>(transform);
			animFollow_AF = PEUtil.GetCmpt<AnimFollow_AF>(transform);
			beatParam = PEUtil.GetCmpt<BeatParam>(transform);
			moveParam = PEUtil.GetCmpt<MoveParam>(transform);
			barrelController = PEUtil.GetCmpt<PEBarrelController>(transform);
			billBoard = PEUtil.GetCmpt<BillBoard>(transform);
			armorBones = PEUtil.GetCmpt<ArmorBones>(transform);
			visions = PEUtil.GetCmpts<PEVision>(transform);
			hears = PEUtil.GetCmpts<PEHearing>(transform);
			native = PEUtil.GetCmpt<PENative>(transform);
			monster = PEUtil.GetCmpt<PEMonster>(transform);
			if(null != modelController)
				modelController.ResetModelInfo();

			if(null != animFollow_AF)
				animFollow_AF.ResetModelInfo();

            if (null != ragdollController)
                ragdollController.ResetRagdoll();
		}
	}
}

#if UNITY_EDITOR
public partial class PeCustomMenu : EditorWindow
{	
	[MenuItem("Assets/ResetViewRoot")]
	static void ResetViewRoot()
	{
		GameObject[] selectedObjArray = Selection.gameObjects;

		for (int i = 0; i < selectedObjArray.Length; ++i) 
		{			
			BiologyViewRoot viewRoot = selectedObjArray[i].GetComponent<BiologyViewRoot>();

			if(null == viewRoot)
				viewRoot = Undo.AddComponent<BiologyViewRoot>(selectedObjArray[i]);

			viewRoot.Reset();
			UnityEditor.EditorUtility.SetDirty(selectedObjArray[i]);
		}
	}
}
#endif