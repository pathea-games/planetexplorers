#define IgnoreCaseOfBoneName
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;
using CustomCharactor;

namespace AppearBlendShape
{
    public class AppearBuilder
    {
		// Mesh/Bones/Color helpers
		static List<Transform> _tmpOutBones = new List<Transform> ();
		static List<CombineInstance> _tmpCombineInstances = new List<CombineInstance> ();
		static int _idxTmpMesh = 0;
		static List<int> _tmpWeightIdxs = new List<int> ();
		static List<float> _tmpWeightVals = new List<float> ();
		static List<Mesh> _tmpMeshs = new List<Mesh>();
		static void ResetTmpMeshes()
		{
			_idxTmpMesh = 0;
		}
		static Mesh PeekTmpMesh()
		{
			if (_idxTmpMesh >= _tmpMeshs.Count) {
				_idxTmpMesh = _tmpMeshs.Count;
				_tmpMeshs.Add (new Mesh ());
			}
			return _tmpMeshs[_idxTmpMesh++];
		}
		static Mesh BakeToMesh(CustomPartInfo partInfo, AppearData appearData)
		{
			SkinnedMeshRenderer smr = partInfo.Smr;
			if (string.IsNullOrEmpty(partInfo.ModelName) || smr.sharedMesh.blendShapeCount == 0)
			{
				return smr.sharedMesh;
			}
			
			EMorphItem indexStart = EMorphItem.Max;
			EMorphItem indexEnd = EMorphItem.Max;
			if (partInfo.ModelName.Contains("head"))
			{
				indexStart = EMorphItem.MinFace;
				indexEnd = EMorphItem.MaxFace;
			}
			else if (partInfo.ModelName.Contains("torso"))
			{
				indexStart = EMorphItem.MinUpperbody;
				indexEnd = EMorphItem.MaxUpperbody;
			}
			else if (partInfo.ModelName.Contains("legs"))
			{
				indexStart = EMorphItem.MinLowerbody;
				indexEnd = EMorphItem.MaxLowerbody;
			}
			else if (partInfo.ModelName.Contains("feet"))
			{
				indexStart = EMorphItem.MinFoot;
				indexEnd = EMorphItem.MaxFoot;
			}
			else if (partInfo.ModelName.Contains("hand"))
			{
				indexStart = EMorphItem.MinHand;
				indexEnd = EMorphItem.MaxHand;
			}
			
			if (indexStart != indexEnd)
			{
				_tmpWeightIdxs.Clear();
				_tmpWeightVals.Clear();
				for (int i = (int)indexStart; i < (int)indexEnd; i++)
				{
					float w = appearData.GetWeight((EMorphItem)i) * 100f;					
					int index = i - (int)indexStart;					
					if (w > 0f)
					{
						index = index * 2;
					}
					else
					{
						w = -w;
						index = index * 2 + 1;
					}
					
					if (smr.sharedMesh.blendShapeCount > index)
					{
						_tmpWeightIdxs.Add(index);
						_tmpWeightVals.Add(smr.GetBlendShapeWeight(index));
						smr.SetBlendShapeWeight(index, w);
					}
				}
				Mesh msh = PeekTmpMesh();
				smr.BakeMesh(msh);				
				msh.boneWeights = smr.sharedMesh.boneWeights;
				msh.bindposes = smr.sharedMesh.bindposes;
				if(_tmpWeightIdxs.Count > 0)
				{
					for(int i = 0; i < _tmpWeightIdxs.Count; i++){
						smr.SetBlendShapeWeight(_tmpWeightIdxs[i], _tmpWeightVals[i]);
					}
				}
				return msh;
			}
			return smr.sharedMesh;
		}
		static void CopyBonesByName(Transform[] srcBones, Dictionary<string, Transform> targetBones, List<Transform> outBones)
		{
			Transform t;
			foreach (Transform srcBone in srcBones)
			{
				#if IgnoreCaseOfBoneName
				string name = srcBone.name.ToLower();
				#else
				string name = srcBone.name;
				#endif
				if (targetBones.TryGetValue(name, out t)){
					outBones.Add(t);
				} else {
					Debug.LogError("cant find bone:" + name);
				}
			}
		}
		static void SetColor(Material pMat, AppearData appearData)
		{
			if (pMat.name.Contains("eye"))
			{
				pMat.SetColor("_SkinColor", appearData.mEyeColor);
			}
			else if (pMat.name.Contains("hair"))
			{
				pMat.SetColor("_Color", appearData.mHairColor);
			}
			else if (pMat.name.Contains("head"))
			{
				pMat.SetColor("_Color", appearData.skinColor);
			}
			else if (!pMat.name.Contains("Helmet"))
			{
				pMat.SetColor("_SkinColor", appearData.skinColor);
			}
			else if (pMat.name.Contains("Helmet_10A"))	// Special code for Gold helmet
			{
				/*
				pMat.SetFloat("_Mode", 2);
				pMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				pMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				pMat.SetInt("_ZWrite", 0);
				pMat.DisableKeyword("_ALPHATEST_ON");
				pMat.EnableKeyword("_ALPHABLEND_ON");
				pMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				pMat.renderQueue = 3000;
*/
				pMat.SetFloat("_Mode", 3);
				pMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				pMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				pMat.SetInt("_ZWrite", 0);
				pMat.DisableKeyword("_ALPHATEST_ON");
				pMat.DisableKeyword("_ALPHABLEND_ON");
				pMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				pMat.renderQueue = 3000;
			}
		}
		// Build Mesh/Mat
		static void BuildMesh(List<CustomPartInfo> partInfos, Dictionary<string, Transform> targetBones, SkinnedMeshRenderer targetSmr, AppearData appearData)
		{
			targetSmr.sharedMesh.Clear(false);

			CustomPartInfo partInfo;	
			Mesh mesh;
			ResetTmpMeshes ();
			_tmpOutBones.Clear ();
			_tmpCombineInstances.Clear ();
			//Profiler.BeginSample ("PartInfo");
			int nParts = partInfos.Count;
			for (int i = 0; i < nParts; i++) {
				partInfo = partInfos[i];
				mesh = BakeToMesh (partInfo, appearData);
				for (int sub = 0; sub < mesh.subMeshCount; sub++)
				{
					CombineInstance ci = new CombineInstance();
					ci.mesh = mesh;
					ci.subMeshIndex = sub;
					_tmpCombineInstances.Add(ci);
					
					//all sub mesh must find bones
					//Profiler.BeginSample("FindBones");
					CopyBonesByName(partInfo.Smr.bones, targetBones, _tmpOutBones);
					//Profiler.EndSample();
				}
			}
			//Profiler.EndSample ();
			//Profiler.BeginSample ("Combine");
			targetSmr.sharedMesh.CombineMeshes(_tmpCombineInstances.ToArray(), false, false);
			//Profiler.EndSample ();
			targetSmr.bones = _tmpOutBones.ToArray ();
		}
		static void BuildMat(List<CustomPartInfo> partInfos, SkinnedMeshRenderer targetSmr, AppearData appearData)
		{
			for (int i = 0; i < targetSmr.sharedMaterials.Length; i++) {
				GameObject.Destroy(targetSmr.sharedMaterials[i]);
			}

			List<Material> materials = new List<Material>();
			Material mat;
			CustomPartInfo partInfo;
			int nParts = partInfos.Count;
			for (int i = 0; i < nParts; i++) {
				partInfo = partInfos[i];
				//Profiler.BeginSample("MatAdd");
				for (int j = 0; j < partInfo.Smr.sharedMaterials.Length; j++) {
					mat = GameObject.Instantiate(partInfo.Smr.sharedMaterials[j]);
					SetColor(mat, appearData);
					materials.Add(mat);
				}
				//Profiler.EndSample();
			}
			targetSmr.materials = materials.ToArray ();
		}
		// Helpers to get ready to Build
		static void GetPartInfos(IEnumerable<string> strPartInfos, List<CustomPartInfo> outPartInfos)
		{
			//List<CustomPartInfo> infos = new List<CustomPartInfo> ();
			foreach (string partInfo in strPartInfos) {
				if (!string.IsNullOrEmpty (partInfo)) {
					CustomPartInfo info = new CustomPartInfo(partInfo);
					if(info.Smr != null){
						outPartInfos.Add(info);
					}
				}
			}
		}
		static void GetTargetBones(GameObject rootModel, Dictionary<string, Transform> outTargetBones)
		{
			_tmpOutBones.Clear ();
			rootModel.GetComponentsInChildren<Transform>(true, _tmpOutBones);	
			foreach (Transform t in _tmpOutBones) {
				#if IgnoreCaseOfBoneName
				string name = t.name.ToLower();
				#else
				string name = t.name;
				#endif
				if(outTargetBones.ContainsKey(name)){
#if UNITY_EDITOR
					//Debug.LogError("[SetRoot]Transform name duplicated:"+name);
#endif
				} else {
					outTargetBones.Add(name, t);
				}
			}
		}
		public static SkinnedMeshRenderer GetTargetSmr(GameObject rootModel)
		{
			SkinnedMeshRenderer r = rootModel.GetComponent<SkinnedMeshRenderer>();
			if (null == r) {
				r = rootModel.gameObject.AddComponent<SkinnedMeshRenderer>();
			}			
			if (r.sharedMesh == null) {
				r.sharedMesh = new Mesh();
				r.sharedMesh.name = "PlayerAppearance1";
			}			
			return r;
		}
		// Public functions
		public static IEnumerator BuildAsync(GameObject rootModel, AppearData appearData, IEnumerable<string> strPartInfos)
        {
			//Profiler.BeginSample ("Prepare");
			List<CustomPartInfo> partInfos = new List<CustomPartInfo>();
			Dictionary<string, Transform> targetBones = new Dictionary<string, Transform> ();
			SkinnedMeshRenderer targetSmr = GetTargetSmr(rootModel);
			GetPartInfos (strPartInfos, partInfos);
			GetTargetBones (rootModel, targetBones);
			//Profiler.EndSample ();

			//Profiler.BeginSample ("BuildMesh");
			BuildMesh(partInfos, targetBones, targetSmr, appearData);
			//Profiler.EndSample ();
			yield return 0;

			for (int i = 0; i < targetSmr.sharedMaterials.Length; i++) {
				GameObject.Destroy(targetSmr.sharedMaterials[i]);
			}
			
			List<Material> materials = new List<Material>();
			Material mat;
			CustomPartInfo partInfo;
			int nParts = partInfos.Count;
			for (int i = 0; i < nParts; i++) {
				partInfo = partInfos[i];
				//Profiler.BeginSample("MatN");
				int nMats = partInfo.Smr.sharedMaterials.Length;
				//Profiler.EndSample();
				yield return 0;
				for (int j = 0; j < nMats; j++) {
					//Profiler.BeginSample("MatAdd");
					mat = GameObject.Instantiate(partInfo.Smr.sharedMaterials[j]);
					SetColor(mat, appearData);
					materials.Add(mat);
					//Profiler.EndSample();
					yield return 0;
				}
			}
			targetSmr.materials = materials.ToArray ();
			////Profiler.BeginSample ("BuildMat");
			//BuildMat(partInfos, targetSmr, appearData);
			////Profiler.EndSample ();
        }
		public static SkinnedMeshRenderer Build(GameObject rootModel, AppearData appearData, IEnumerable<string> strPartInfos)
		{
			List<CustomPartInfo> partInfos = new List<CustomPartInfo>();
			Dictionary<string, Transform> targetBones = new Dictionary<string, Transform> ();
			SkinnedMeshRenderer targetSmr = GetTargetSmr(rootModel);
			GetPartInfos (strPartInfos, partInfos);
			GetTargetBones (rootModel, targetBones);
			
			//Profiler.BeginSample ("BuildMesh");
			BuildMesh(partInfos, targetBones, targetSmr, appearData);
			//Profiler.EndSample ();
			//Profiler.BeginSample ("BuildMat");
			BuildMat(partInfos, targetSmr, appearData);
			//Profiler.EndSample ();
			return targetSmr;
		}
		public static void ApplyColor(GameObject rootModel, AppearData appearData)
        {
			SkinnedMeshRenderer targetSmr = GetTargetSmr(rootModel);
            if (targetSmr == null){
                return;
            }
			for (int i = 0; i < targetSmr.sharedMaterials.Length; i++) {
				SetColor(targetSmr.sharedMaterials[i], appearData);
			}
        }
		public static SkinnedMeshRenderer CloneSmr(GameObject rootModel, SkinnedMeshRenderer smr)
        {
			SkinnedMeshRenderer targetSmr = GetTargetSmr(rootModel);
			Dictionary<string, Transform> targetBones = new Dictionary<string, Transform> ();
			GetTargetBones (rootModel, targetBones);
            
            targetSmr.sharedMesh = smr.sharedMesh;
            targetSmr.sharedMaterials = smr.sharedMaterials;

			_tmpOutBones.Clear();
			CopyBonesByName (smr.bones, targetBones, _tmpOutBones);
			targetSmr.bones = _tmpOutBones.ToArray();
            return targetSmr;
        }
    }

    //some part maybe overlap, Calf maybe ajusted by trousers or shoes
    public enum EMorphItem
    {
        Min = 0,
        MinFace = Min,
        //脸部宽窄
        FaceWidth = MinFace,
        //脸蛋胖瘦
        FaceThickness,
        //眉毛位置
        EyebrowLocation,
        //眉毛角度
        EyebrowDirection,
        //眼睛角度
        EyeDirection,
        //眼睛大小
        EyeSize,
        //鼻子位置
        NoseLocation,
        //鼻梁高度
        NoseHeight,
        //鼻头大小
        NoseSize,
        //嘴巴位置
        MouthLocation,
        //嘴巴大小
        MouthSize,
        //嘴角位置
        MouthShape,
        //下巴宽度
        ChinWidth,
        //下颌宽度
        JawWidth,

        //耳朵位置
        //EarLocation,
        //耳朵大小
        //EarSize,

        MaxFace,

        MinUpperbody = MaxFace,
        //肩部
        Shoulder = MinUpperbody,
        //胸部
        Breast,
        //上臂粗细
        UpperArm,
        //下臂粗细
        LowerArm,
        //肚子胖瘦
        Belly,
        //腰部粗细
        Waist,

        //有些长衣服包住了屁股，为避免穿插调整UpperLeg时，同时调整这个部位
        TorsoUpperLeg,

        MaxUpperbody,

        MinLowerbody = MaxUpperbody,

        //为避免接缝调整上半生腰部时，同时调整下半身
        LegBelly = MinLowerbody,

        LegWaist,

        //大腿粗细
        UpperLeg,
        //小腿粗细
        LowerLeg,

        MaxLowerbody,

        MinHand = MaxLowerbody,
        //手掌大小
        Hand = MinHand,

        MaxHand,

        MinFoot = MaxHand,
        //有些长筒靴需要根据小腿变化而变化
        Foot = MinFoot,
        MaxFoot,

        Max = MaxFoot
    }

    public class AppearData
    {
        const int VERSION_0000 = 0;
        const int VERSION_0001 = VERSION_0000 + 1;
        const int CURRENT_VERSION = VERSION_0001;
		static readonly Color[] s_skinColors = new Color[]{
			Color.white,//(ffffff),
			new Color(0.9098f,0.8471f,0.6941f),//(e8d8b1),
			new Color(0.4039f,0.3373f,0.0157f),//(675604),
			new Color(0.9255f,0.7725f,0.7216f),//(ecc5b8),
			new Color(0.2510f,0.2392f,0.1725f),//(403d2c),
			new Color(0.7961f,0.6784f,0.5255f),//(cbad86),
			new Color(0.7451f,0.6078f,0.5647f),//(be9b90),
			new Color(0.9333f,0.8941f,0.7960f),//(eee4cb),
			new Color(0.7804f,0.6941f,0.5843f),//(c7b195),
			new Color(0.8118f,0.7961f,0.7882f),//(cfcbc9),
		};
		static readonly Color[] s_hairColors = new Color[]{
			new Color(0.5255f,0.5255f,0.5255f),//(868686),
			new Color(0.8157f,0.4078f,0.1647f),//(d0682a),
			new Color(0.3451f,0.1294f,0.0157f),//(582104),
			new Color(0.8549f,0.4078f,0.4941f),//(da687e),
			new Color(0.9020f,0.6157f,0.4118f),//(e69d69),
			new Color(0.7922f,0.9059f,0.4196f),//(cae76b),
			new Color(0.6275f,0.4078f,0.8667f),//(a068dd),
			new Color(0.4353f,0.5451f,0.8783f),//(6f8be0),
			new Color(1.0000f,0.7843f,0.1255f),//(ffc820),
			new Color(0.7098f,0.2902f,0.3020f),//(b54a4d),
		};

		float[] mMorphWeightArray = new float[(int)EMorphItem.Max];
		float[] mSubMorphWeightArray = new float[(int)EMorphItem.Max];

        public Color mEyeColor;
        public Color mLipColor;
        public Color mSkinColor;
        public Color mHairColor;

		Color mSubSkinColor = Color.black;
		float mSubBodyWeight;

		public Color subSkinColor{ set { mSubSkinColor = value; } }
		public float[] subBodyWeight{ get { return mSubMorphWeightArray; } }

		public Color skinColor
		{
			get
			{
				Color retColor = mSkinColor;
				if(mSubSkinColor != Color.black)
					retColor = Color.Lerp(retColor, mSubSkinColor, 1f);
				return retColor;
			}
		}

        public AppearData()
        {
            Default();

            //RandomMorphWeight();
            //MinMorphWeight();
            //MaxMorphWeight();
        }

        public void Default()
        {
			mSkinColor = Color.white;
			mEyeColor = Color.white;
			mHairColor = Color.white;
            mLipColor = Color.red;

			for (int i=0;i<(int)EMorphItem.Max;i++)
				SetWeight((EMorphItem)i,0);
        }

        public void Random()
        {
			mSkinColor = s_skinColors[UnityEngine.Random.Range(0, s_skinColors.Length)];
            mEyeColor = Color.white;
			mHairColor = s_hairColors[UnityEngine.Random.Range(0, s_hairColors.Length)];
			mLipColor = Color.red;

            RandomMorphWeight();
        }

        public float GetWeight(EMorphItem eMorphItem)
        {
			return Mathf.Clamp(mMorphWeightArray[(int)eMorphItem] + mSubMorphWeightArray[(int)eMorphItem], -1f, 1f);
        }

        public void SetWeight(EMorphItem eMorphItem, float weight)
        {
            if (eMorphItem == EMorphItem.LegBelly
                || eMorphItem == EMorphItem.LegWaist
                || eMorphItem == EMorphItem.Foot
                || eMorphItem == EMorphItem.Hand
                || eMorphItem == EMorphItem.TorsoUpperLeg)
            {
                return;
            }

            mMorphWeightArray[(int)eMorphItem] = Mathf.Clamp(weight, -1f, 1f);

            if (eMorphItem == EMorphItem.Belly)
            {
                mMorphWeightArray[(int)EMorphItem.LegBelly] = mMorphWeightArray[(int)EMorphItem.Belly];
            }

            if (eMorphItem == EMorphItem.Waist)
            {
                mMorphWeightArray[(int)EMorphItem.LegWaist] = mMorphWeightArray[(int)EMorphItem.Waist];
            }

            if (eMorphItem == EMorphItem.LowerLeg)
            {
                mMorphWeightArray[(int)EMorphItem.Foot] = mMorphWeightArray[(int)EMorphItem.LowerLeg];
            }

            if (eMorphItem == EMorphItem.LowerArm)
            {
                mMorphWeightArray[(int)EMorphItem.Hand] = mMorphWeightArray[(int)EMorphItem.LowerArm];
            }

            if (eMorphItem == EMorphItem.UpperLeg)
            {
                mMorphWeightArray[(int)EMorphItem.TorsoUpperLeg] = mMorphWeightArray[(int)EMorphItem.UpperLeg];
            }
        }

        public void RandomMorphWeight()
        {
            for (int i = (int)EMorphItem.Min; i < (int)EMorphItem.Max; i++)
            {
                SetWeight((EMorphItem)i, UnityEngine.Random.Range(-1f, 1f));
            }
        }

        public void MaxMorphWeight()
        {
            for (int i = (int)EMorphItem.Min; i < (int)EMorphItem.Max; i++)
            {
                SetWeight((EMorphItem)i, 1f);
            }
        }

        public void MinMorphWeight()
        {
            for (int i = (int)EMorphItem.Min; i < (int)EMorphItem.Max; i++)
            {
                SetWeight((EMorphItem)i, -1f);
            }
        }

        public byte[] Serialize()
        {
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(100);
                using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
                {
                    bw.Write((int)CURRENT_VERSION);

                    bw.Write((int)mMorphWeightArray.Length);

                    for (int i = 0; i < mMorphWeightArray.Length; i++)
                    {
                        bw.Write((float)mMorphWeightArray[i]);
                    }

                    PETools.Serialize.WriteColor(bw, mEyeColor);
                    PETools.Serialize.WriteColor(bw, mLipColor);
                    PETools.Serialize.WriteColor(bw, mSkinColor);
                    PETools.Serialize.WriteColor(bw, mHairColor);
                }

                return ms.ToArray();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }

        public bool Deserialize(byte[] data)
        {
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(data, false);

                using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
                {
                    int version = br.ReadInt32();

                    if (version > CURRENT_VERSION)
                    {
                        Debug.LogError("version:" + version + " greater than current version:" + CURRENT_VERSION);
                        return false;
                    }

                    int dataCount = br.ReadInt32();
                    if (dataCount != (int)EMorphItem.Max)
                    {
                        return false;
                    }

                    for (int i = 0; i < dataCount; i++)
                    {
                        mMorphWeightArray[i] = br.ReadSingle();
                    }

                    mEyeColor = PETools.Serialize.ReadColor(br);
                    mLipColor = PETools.Serialize.ReadColor(br);
                    mSkinColor = PETools.Serialize.ReadColor(br);
                    mHairColor = PETools.Serialize.ReadColor(br);

                    return true;
                }
            }
            catch (System.Exception e)
            {
				Debug.LogWarning(e);
                return false;
            }

        }
    }
}