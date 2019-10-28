using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomCharactor
{
	public class CustomTmplGo
	{
		public int _refCnt = 0;
		public float _tStamp;
		public GameObject _go;
	}
	public class CustomModelTmpls
	{
		private int _nTmpls;
		private Dictionary<AvatarData, CustomTmplGo> _tmplGos = new Dictionary<AvatarData, CustomTmplGo>();

		static CustomModelTmpls s_Inst = null;
		public static CustomModelTmpls Instance{
			get{
				if(s_Inst == null){
					s_Inst = new CustomModelTmpls();
				}
				return s_Inst;
			}
		}

		public void Init(int nTmpl)
		{
			_nTmpls = nTmpl;
			List<CustomTmplGo> ctGos = _tmplGos.Values.Cast<CustomTmplGo> ().ToList ();
			_tmplGos.Clear ();
			foreach (CustomTmplGo ctGo in ctGos) {
				if(ctGo._go != null){
					GameObject.Destroy(ctGo._go);
				}
			}
		}
		public CustomTmplGo GetTmplGo(AvatarData avdata)
		{
			if (avdata == null)
				return null;

			CustomTmplGo ctGo;
			if (!_tmplGos.TryGetValue (avdata, out ctGo)) {
				// Not found then create new
				ctGo = new CustomTmplGo ();
				ctGo._go = CustomModelTmpls.CreateTmplGo (avdata);
				if (_tmplGos.Count > _nTmpls) {	// Remove the oldest template
					float oldTime = ctGo._tStamp;
					AvatarData oldAvdata = avdata;
					foreach (KeyValuePair<AvatarData,CustomTmplGo> pair in _tmplGos) {
						if (pair.Value._tStamp <= oldTime) {
							oldTime = pair.Value._tStamp;
							oldAvdata = pair.Key;
						}
					}
					_tmplGos.Remove (oldAvdata);
				}
				_tmplGos.Add (avdata, ctGo);
			}
			ctGo._refCnt++;
			ctGo._tStamp = Time.time;
			return ctGo;
		}
		public void DismissTmplGo(AvatarData avdata)
		{
			if (avdata == null)
				return;			

			CustomTmplGo ctGo;
			if (_tmplGos.TryGetValue (avdata, out ctGo)) {
				ctGo._refCnt--;
				if(ctGo._refCnt <= 0){
					_tmplGos.Remove(avdata);
				}
			}
		}

		static GameObject CreateTmplGo(AvatarData avdata)
		{
			string baseModel = avdata._baseModel;
			GameObject go = GameObject.Instantiate(Resources.Load(baseModel)) as GameObject;
			PEModelController mCtrl = go.GetComponentInChildren<PEModelController>();
			if (mCtrl == null) {
				Debug.LogError("[CreateTmplGo]Cannot find PEModelController in baseModel:"+baseModel);
				return null;
			}
			PERagdollController rCtrl = go.GetComponentInChildren<PERagdollController>();
			if (rCtrl == null) {
				Debug.LogError("[CreateTmplGo]Cannot find PERagdollController in baseModel:"+baseModel);
				return null;
			}
			Debug.LogError("[CreateTmplGo]create baseModel:"+baseModel);
			// Model
			SkinnedMeshRenderer mSmr = mCtrl.GetComponent<SkinnedMeshRenderer> ();				
			if (null == mSmr) {
				mSmr = mCtrl.gameObject.AddComponent<SkinnedMeshRenderer> ();
			}				
			if (mSmr.sharedMesh == null) {
				mSmr.sharedMesh = new Mesh ();
				mSmr.sharedMesh.name = "PlayerAppearance1";
			} else {
				mSmr.sharedMesh.Clear ();
			}
			List<Material> materials = new List<Material> ();
			List<Transform> bones = new List<Transform> ();
			List<Transform> mAllBoneList = new List<Transform> (mCtrl.GetComponentsInChildren<Transform> (true));
			foreach (string partPathName in avdata) {
				if (string.IsNullOrEmpty (partPathName))
					continue;
				
				SkinnedMeshRenderer tmpSmr = new CustomPartInfo (partPathName).Smr;
				if (null == tmpSmr)
					continue;
				
				List<Transform> tmpBones = CustomUtils.FindSmrBonesByName (mAllBoneList, tmpSmr);
				int nSub = tmpSmr.sharedMesh.subMeshCount;
				for (int iSub = 0; iSub < nSub; iSub++) {
					//all sub mesh must find bones
					bones.AddRange (tmpBones);
				}
				materials.AddRange (tmpSmr.sharedMaterials); // smr.materials
			}
			mSmr.bones = bones.ToArray ();
			mSmr.materials = materials.ToArray ();

			//Ragdoll
			SkinnedMeshRenderer rSmr = rCtrl.GetComponent<SkinnedMeshRenderer> ();				
			if (null == rSmr) {
				rSmr = rCtrl.gameObject.AddComponent<SkinnedMeshRenderer> ();
			}
			rSmr.sharedMesh = mSmr.sharedMesh;
			List<Transform> rAllBoneList = new List<Transform> (rCtrl.GetComponentsInChildren<Transform> (true));
			bones = CustomUtils.FindSmrBonesByName (rAllBoneList, mSmr);
			rSmr.bones = bones.ToArray();
			rSmr.sharedMaterials = mSmr.sharedMaterials;

			go.SetActive (false);
			return go;
		}
	}
}
