using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
	public class AvatarCmpt : BiologyViewCmpt
	{
        const int VERSION_0000 = 0;
        const int CURRENT_VERSION = VERSION_0000;
		const string MaleModelPath = "Model/PlayerModel/Male";
		const string FemaleModelPath = "Model/PlayerModel/Female";
		const string MaleClothes01Path = "Model/PlayerModel/male01";
		const string FemaleClothes01Path = "Model/PlayerModel/female01";
		const string MaleModelPrefabPath = "Prefab/PlayerPrefab/MaleModel";
		const string FemaleModelPrefabPath = "Prefab/PlayerPrefab/FemaleModel";
		const string MaleAnimatorPrefabPath = "Prefab/PlayerPrefab/MaleM";
		const string FemaleAnimatorPrefabPath = "Prefab/PlayerPrefab/FemaleM";
		static GameObject s_preloadMaleAnim;
		static GameObject s_preloadFemaleAnim;
		public static void CachePrefab()
		{
			AssetsLoader.Instance.LoadPrefabImm(MaleModelPath, true);
			AssetsLoader.Instance.LoadPrefabImm(FemaleModelPath, true);
			AssetsLoader.Instance.LoadPrefabImm(MaleClothes01Path, true);
			AssetsLoader.Instance.LoadPrefabImm(FemaleClothes01Path, true);
			AssetsLoader.Instance.LoadPrefabImm(MaleModelPrefabPath, true);
			AssetsLoader.Instance.LoadPrefabImm(FemaleModelPrefabPath, true);

			s_preloadMaleAnim = Instantiate(AssetsLoader.Instance.LoadPrefabImm(MaleModelPrefabPath, false)) as GameObject;
			s_preloadMaleAnim.transform.parent = AssetsLoader.Instance.transform;
			s_preloadMaleAnim.SetActive (false);
			s_preloadFemaleAnim = Instantiate(AssetsLoader.Instance.LoadPrefabImm(FemaleModelPrefabPath, false)) as GameObject;
			s_preloadFemaleAnim.transform.parent = AssetsLoader.Instance.transform;
			s_preloadFemaleAnim.SetActive (false);
		}

        AppearBlendShape.AppearData _appearData = null;
		CustomCharactor.AvatarData _nudeAvatarData = null;
        CustomCharactor.AvatarData _clothedAvatarData = new CustomCharactor.AvatarData();
		CustomCharactor.AvatarData _curAvatarData = null;
		bool _isDirty = false;
		bool _asynUpdatingSmr = false;
		bool NotCustomizable{ get { return null == _nudeAvatarData; } }	// use legacy code, a little weird

		public AppearBlendShape.AppearData apperaData{ get { return _appearData; } }

        public bool HasData()
        {
            return null != _appearData;
        }
        public void SetData(AppearBlendShape.AppearData appearData, CustomCharactor.AvatarData nudeAvatarData)
		{
            _appearData = appearData;
            _nudeAvatarData = nudeAvatarData;
			// Add onConstruct to preload
			if (_nudeAvatarData != null) {
				Entity.lodCmpt.onConstruct += e=>e.StartCoroutine(PreLoad());
			}
		}
        public override void Deserialize(System.IO.BinaryReader r)
        {
            base.Deserialize(r);

            int version = r.ReadInt32();
            if (version > CURRENT_VERSION)
            {
                Debug.LogError("version error");
                return;
            }

            byte[] appearBuff = PETools.Serialize.ReadBytes(r);
            byte[] nudeBuff = PETools.Serialize.ReadBytes(r);

            if (appearBuff != null && appearBuff.Length > 0 && nudeBuff != null && nudeBuff.Length > 0)
            {
                AppearBlendShape.AppearData appearData = new AppearBlendShape.AppearData();
                appearData.Deserialize(appearBuff);

				CustomCharactor.AvatarData nudeAvatarData = new CustomCharactor.AvatarData();
                nudeAvatarData.Deserialize(nudeBuff);
                SetData(appearData, nudeAvatarData);
            }
        }

        public override void Serialize(System.IO.BinaryWriter w)
        {
            base.Serialize(w);

            w.Write(CURRENT_VERSION);

            byte[] appearBuff = null;
            if (null != _appearData)
            {
                appearBuff = _appearData.Serialize();
            }

            PETools.Serialize.WriteBytes(appearBuff, w);

            byte[] avatarBuff = null;
            if (null != _nudeAvatarData)
            {
				avatarBuff = _nudeAvatarData.Serialize();
            }
            PETools.Serialize.WriteBytes(avatarBuff, w);
        }

		public override void AddPart (int partMask, string info)
		{
			foreach(CustomCharactor.AvatarData.ESlot slot in CustomCharactor.AvatarData.GetSlot(partMask))
            {
				if(!SystemSettingData.Instance.HideHeadgear
				   || (slot != CustomCharactor.AvatarData.ESlot.HairB
				    && slot != CustomCharactor.AvatarData.ESlot.HairF
				    && slot != CustomCharactor.AvatarData.ESlot.HairT))
					_clothedAvatarData.SetPart(slot, info);
            }
			_isDirty = true;
		}
		
		public override void RemovePart (int partMask)
		{
            foreach (CustomCharactor.AvatarData.ESlot slot in CustomCharactor.AvatarData.GetSlot(partMask))
            {
                _clothedAvatarData.SetPart(slot, null);
            }
			_isDirty = true;
		}

		IEnumerator PreLoad()
		{
			//Object asset;
			_curAvatarData = CustomCharactor.AvatarData.Merge (_clothedAvatarData, _nudeAvatarData);
			foreach (string partInfo in _curAvatarData) {
				if (string.IsNullOrEmpty (partInfo))
					continue;
				string strModelFilePath = CustomCharactor.CustomPartInfo.GetModelFilePath(partInfo);
				AssetsLoader.Instance.AddReq(new AssetReq(strModelFilePath));
				yield return new WaitForSeconds(0.2f);
			}
		}

        string GetSkeleton()
        {
            CommonCmpt info = Entity.GetCmpt<CommonCmpt>();
			return (null != info && info.sex == PeSex.Male) ? MaleModelPrefabPath : FemaleModelPrefabPath;
		}
		
		protected override void OnBoneLoad(GameObject obj)
		{
			_coroutineMeshProc = UpdateSmrAsync;
			base.OnBoneLoad(obj);
		}

		protected override void OnBoneLoadSync(GameObject modelObject)
		{
			base.OnBoneLoadSync(modelObject);

			_curAvatarData = CustomCharactor.AvatarData.Merge(_clothedAvatarData, _nudeAvatarData);
			SkinnedMeshRenderer smr = AppearBlendShape.AppearBuilder.Build(modelObject, _appearData, _curAvatarData);
			//bool oldState = smr.enabled;
			smr.enabled = true;
		}

		public override void Build()
		{
            if (string.IsNullOrEmpty(prefabPath) || prefabPath.Equals("0"))
            {
                SetViewPath(GetSkeleton());
				ResetAvatarInfo();
            }
			base.Build();
		}

		IEnumerator UpdateSmrAsync()
		{
			_asynUpdatingSmr = true;
			_curAvatarData = CustomCharactor.AvatarData.Merge (_clothedAvatarData, _nudeAvatarData);
			IEnumerator eBuildAsync = AppearBlendShape.AppearBuilder.BuildAsync (modelTrans.gameObject, _appearData, _curAvatarData);
			while (_asynUpdatingSmr && eBuildAsync.MoveNext()) 	yield return 0;
			if (!_asynUpdatingSmr)								yield break;
			//yield return biologyViewRoot.StartCoroutine (AppearBlendShape.AppearBuilder.BuildAsync (modelTrans.gameObject, mAppearData, mCurAvatarData));

			SkinnedMeshRenderer smr = AppearBlendShape.AppearBuilder.GetTargetSmr (modelTrans.gameObject);
			bool oldState = smr.enabled;
			smr.enabled = true;
			SkinnedMeshRenderer clonedSmr = AppearBlendShape.AppearBuilder.CloneSmr(monoRagdollCtrlr.gameObject, smr);   
			clonedSmr.enabled = true;
			monoRagdollCtrlr.SmrBuild(clonedSmr);			
			smr.enabled = oldState;
			//AppearBlendShape.AppearBuilder.Instance.Clear();
		}
		public void UpdateSmr()
		{
			if (NotCustomizable || null == modelTrans || null == monoRagdollCtrlr)
				return;

			_asynUpdatingSmr = false;
			_curAvatarData = CustomCharactor.AvatarData.Merge (_clothedAvatarData, _nudeAvatarData);
			SkinnedMeshRenderer smr = AppearBlendShape.AppearBuilder.Build(modelTrans.gameObject, _appearData, _curAvatarData);
			bool oldState = smr.enabled;
			smr.enabled = true;
			
			SkinnedMeshRenderer clonedSmr = AppearBlendShape.AppearBuilder.CloneSmr(monoRagdollCtrlr.gameObject, smr);   
			clonedSmr.enabled = true;
			monoRagdollCtrlr.SmrBuild(clonedSmr);

			smr.enabled = oldState;
			//AppearBlendShape.AppearBuilder.Instance.Clear();
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
            if (_isDirty)
            {
				CustomCharactor.AvatarData avData = CustomCharactor.AvatarData.Merge(_clothedAvatarData, _nudeAvatarData);
				if(_curAvatarData != avData){
					_curAvatarData = avData;
					UpdateSmr(); // TODO : check code
				}
                _isDirty = false;
            }
		}

		public override GameObject CloneModel ()
		{
			if (string.IsNullOrEmpty(prefabPath))
			{
				SetViewPath(GetSkeleton());
			}
			return base.CloneModel ();
		}

		void ResetAvatarInfo()
		{
			if(null != Entity.commonCmpt && (null == _nudeAvatarData || _nudeAvatarData.IsInvalid()))
				_nudeAvatarData = Entity.commonCmpt.sex == PeSex.Male ? CustomCharactor.CustomData.DefaultMale().nudeAvatarData : CustomCharactor.CustomData.DefaultFemale().nudeAvatarData;
		}
    }
}