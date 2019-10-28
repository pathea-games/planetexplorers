using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace CustomCharactor
{
	public static class CustomUtils
	{
		public static Transform FindInChildren(Transform transform, string name) {
			if (transform.name == name)
				return transform;

			int count = transform.childCount;
			for(int i = 0; i < count; i++) {
				Transform subChild = FindInChildren(transform.GetChild(i), name);
				if(subChild != null) return subChild;
			}
			return null;
		}
		public static List<Transform> FindSmrBonesByName(List<Transform> allBonesList, SkinnedMeshRenderer smr)
		{
			Transform[] bones = smr.bones;
			int n = bones.Length;
			List<Transform> ret = new List<Transform> (n);
			for (int i = 0; i < n; i++) {
				Transform t = allBonesList.Find(delegate(Transform tt){
					return (0 == string.Compare(bones[i].name, tt.name, true));
				});
				if(t != null){
					ret.Add(t);
				}else{
					Debug.LogError("[FindSmrBonesByName]Cant find bone:" + bones[i].name);
				}
			}
			return ret;
		}
	}
}
