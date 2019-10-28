using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[ExecuteInEditMode]
public class CutsceneClip : MonoBehaviour
{
	WhiteCat.Path _mainPath;
	WhiteCat.TweenInterpolator _interpolator;

	WhiteCat.Path mainPath
	{
		get
		{
			if (_mainPath != null)
				return _mainPath;
			_mainPath = GetComponent<WhiteCat.Path>();
			return _mainPath;
		}
	}

	WhiteCat.TweenInterpolator interpolator
	{
		get
		{
			if (_interpolator != null)
				return _interpolator;
			_interpolator = GetComponent<WhiteCat.TweenInterpolator>();
			return _interpolator;
		}
	}

	WhiteCat.PathDriver targetPathDriver;
	WhiteCat.TweenPathDriver targetTweenPathDriver;

	public CutsceneRotationNode[] Rotations = new CutsceneRotationNode[0];
	private List<CutsceneRotationNode> rots;

	public float fadeInSpeed = 0.1f;
	public float fadeOutSpeed = 0.1f;
	public UnityEvent onArriveAtBeginning = new UnityEvent();
	public UnityEvent onArriveAtEnding = new UnityEvent();

	public bool testClip = false;
	private bool _lastTrackClip = false;
	public bool trackClip = false;
	public float trackTime = 0;

	[HideInInspector]
	[System.NonSerialized]
	public bool isEditMode = true;

	public float normalizedTime
	{
		get { return interpolator.normalizedTime; }
	}

	public float time
	{
		get { return interpolator.normalizedTime * interpolator.duration; }
	}

	void Start ()
	{
		if (!isEditMode)
		{
			BeginClip();
		}
	}

	void Update ()
	{
		if (isEditMode)
		{
			if (testClip)
			{
				if (!trackClip)
					BeginClip();
				testClip = false;
			}

			if (trackClip && !_lastTrackClip)
			{
				CreateDriver();
				interpolator.enabled = false;
				FadeIn(1f);
			}
			if (!trackClip && _lastTrackClip)
			{
				FadeOut(1f);
				DestroyDriver();
			}
			if (trackClip)
			{
				TrackClip(trackTime);
			}
			_lastTrackClip = trackClip;

			PrepareRotation();
			foreach (CutsceneRotationNode rot in rots)
			{
				float len = interpolator.Interpolate(rot.time) * mainPath.pathTotalLength;
				Vector3 pos;
				int idx = 0;
				float st = 0f;
				mainPath.GetPathPositionAtPathLength(len, ref idx, ref st);
				pos = mainPath.GetSplinePoint(idx, st);
				rot.rotation.position = pos;

				Debug.DrawLine(rot.rotation.position, rot.rotation.position + rot.rotation.forward * 0.65f, Color.yellow);
				Debug.DrawLine(rot.rotation.position, rot.rotation.position + rot.rotation.up * 0.2f, Color.white);
				Debug.DrawLine(rot.rotation.position, rot.rotation.position - rot.rotation.up * 0.15f, Color.white);
				Debug.DrawLine(rot.rotation.position, rot.rotation.position + rot.rotation.right * 0.3f, Color.white);
				Debug.DrawLine(rot.rotation.position, rot.rotation.position - rot.rotation.right * 0.25f, Color.white);
				Debug.DrawLine(rot.rotation.position - rot.rotation.right * 0.25f + rot.rotation.up * 0.15f, 
				               rot.rotation.position + rot.rotation.right * 0.25f + rot.rotation.up * 0.15f, Color.white * 0.7f);
				Debug.DrawLine(rot.rotation.position - rot.rotation.right * 0.25f - rot.rotation.up * 0.15f, 
				               rot.rotation.position + rot.rotation.right * 0.25f - rot.rotation.up * 0.15f, Color.white * 0.7f);
				Debug.DrawLine(rot.rotation.position + rot.rotation.right * 0.25f + rot.rotation.up * 0.15f, 
				               rot.rotation.position + rot.rotation.right * 0.25f - rot.rotation.up * 0.15f, Color.white * 0.7f);
				Debug.DrawLine(rot.rotation.position - rot.rotation.right * 0.25f + rot.rotation.up * 0.15f, 
				               rot.rotation.position - rot.rotation.right * 0.25f - rot.rotation.up * 0.15f, Color.white * 0.7f);
			}
		}
		if (interpolator.isPlaying)
			UpdateRotation();
	}

	void OnDestroy ()
	{
		EndClip();
	}

	void CreateDriver ()
	{
		if (targetTweenPathDriver == null && targetPathDriver == null)
		{
			targetPathDriver = PeCamera.cutsceneTransform.gameObject.AddComponent<WhiteCat.PathDriver>();
			targetPathDriver.path = mainPath;
			targetPathDriver.location = 0f;
			targetTweenPathDriver = PeCamera.cutsceneTransform.gameObject.AddComponent<WhiteCat.TweenPathDriver>();
			targetTweenPathDriver.interpolator = interpolator;
			targetTweenPathDriver.from = 0f;
			targetTweenPathDriver.to = mainPath.pathTotalLength;
		}
	}

	void DestroyDriver ()
	{
		if (targetTweenPathDriver != null && targetPathDriver != null)
		{
			WhiteCat.TweenPathDriver.Destroy(targetTweenPathDriver);
			WhiteCat.PathDriver.Destroy(targetPathDriver);
			targetTweenPathDriver = null;
			targetPathDriver = null;
		}
	}

	void BeginClip ()
	{
		if (PeCamera.cutsceneTransform != null)
		{
			if (targetTweenPathDriver == null && targetPathDriver == null)
			{
				CreateDriver();
				interpolator.enabled = true;
				interpolator.Replay();
				if (isEditMode)
					interpolator.onArriveAtEnding.AddListener(EndClip);
				else
					interpolator.onArriveAtEnding.AddListener(SelfDestroy);
				FadeIn(fadeInSpeed);
				onArriveAtBeginning.Invoke();
			}
		}
	}

	void EndClip ()
	{
		interpolator.enabled = false;
		if (targetTweenPathDriver != null && targetPathDriver != null)
		{
			DestroyDriver();
			FadeOut(fadeOutSpeed);
			onArriveAtEnding.Invoke();
		}
	}

	void TrackClip (float t)
	{
		interpolator.normalizedTime = t / interpolator.duration;
		UpdateRotation();
	}

	void PrepareRotation ()
	{
		if (rots == null)
			rots = new List<CutsceneRotationNode> ();
		rots.Clear();
		foreach (CutsceneRotationNode rot in Rotations)
		{
			if (rot.rotation != null)
				rots.Add(rot);
		}
		rots.Sort(CutsceneRotationNode.Compare);
	}

	void UpdateRotation ()
	{
		if (!isEditMode)
		{
			if (rots == null)
				PrepareRotation();
		}

		if (PeCamera.cutsceneTransform != null)
		{
			if (rots.Count == 0)
			{
				PeCamera.cutsceneTransform.rotation = Quaternion.identity;
			}
			else
			{
				if (normalizedTime <= rots[0].time)
				{
					PeCamera.cutsceneTransform.rotation = rots[0].rotation.rotation;
				}
				else if (normalizedTime >= rots[rots.Count - 1].time)
				{
					PeCamera.cutsceneTransform.rotation = rots[rots.Count - 1].rotation.rotation;
				}
				else
				{
					Quaternion p = Quaternion.identity;
					Quaternion a = Quaternion.identity;
					Quaternion b = Quaternion.identity;
					Quaternion q = Quaternion.identity;
					float t = 0;
					for (int i = 0; i < rots.Count - 1; ++i)
					{
						if (normalizedTime >= rots[i].time &&
						    normalizedTime < rots[i+1].time)
						{
							a = rots[i == 0 ? i : i - 1].rotation.rotation;
							p = rots[i].rotation.rotation;
							q = rots[i + 1].rotation.rotation;
							b = rots[i == rots.Count - 2 ? i + 1 : i + 2].rotation.rotation;
							t = Mathf.InverseLerp(rots[i].time, rots[i + 1].time, normalizedTime);
							break;
						}
					}
					PeCamera.cutsceneTransform.rotation = Pathea.SplineUtils.CalculateCubic(p,a,b,q,t);
				}
			}
		}
	}

	void SelfDestroy ()
	{
		EndClip();
		GameObject.Destroy(this.gameObject);
	}

	void FadeIn (float speed)
	{
		PeCamera.CrossFade("Cutscene Blend", 1, speed);
	}

	void FadeOut (float speed)
	{
		PeCamera.CrossFade("Cutscene Blend", 0, speed);
	}
}
