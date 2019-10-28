using System;
using UnityEngine;

namespace WhiteCat
{
	//[RequireComponent(typeof(SkinnedMeshRenderer))] //PeViewStudio line 121 DestroyImmediate(renderer);
	public class SkinnedMeshRendererHelper : MonoBehaviour, ISerializationCallbackReceiver
	{
		[SerializeField] Transform[] _bones;
		[SerializeField] SkinnedMeshRenderer _renderer;


		void Awake()
		{
			_renderer = GetComponent<SkinnedMeshRenderer>();
		}


		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (_renderer) _bones = _renderer.bones;
		}


		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (_renderer) _renderer.bones = _bones;
		}
	}
}