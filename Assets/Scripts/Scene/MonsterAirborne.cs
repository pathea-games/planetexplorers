using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;

public class MonsterAirborne : MonoBehaviour
{
	public enum Type{
		Puja,
		Paja,
	}
	enum Step{
		Null,
		FadeIn,
		Running,
		FadeOut,
	}
	const float _initHeight = 150.0f;
	public float _endHeight = 20.0f;
	public float _endWaveAmp = 1.0f;
	public float _coefWave = 0.01f; //assume wight == 1
	public float _spdDown = 1.0f;
	public float _itvCreate = 3.0f;	// interval 3 seconds
	float _spdWave = 0.02f;

	Type _type = Type.Paja;
	Step _step = Step.Null;
	float _lastTime;
	Vector3 _dstPos = Vector3.zero;
	Transform _tAirborne = null;
	List<SceneEntityPosAgent> _agents = new List<SceneEntityPosAgent>();

	void Start()	// could use IEnumerator to create _airborneGo async
	{
		string path = _type == Type.Puja
				? "Item/scene_puja_aircraft.unity3d"
				: "Item/scene_paja_aircraft.unity3d";
		AssetReq req = AssetsLoader.Instance.AddReq (path, Vector3.zero, Quaternion.identity, Vector3.one);
		req.ReqFinishHandler += StartAirborne;
		//GameObject airborneGo = AssetsLoader.Instance.AddReq(path, Vector3.zero, Quaternion.identity, Vector3.one);
		//StartAirborne(airborneGo);
	}

	void StartAirborne(GameObject airborneGo)
	{
		_tAirborne = airborneGo.transform;
		_tAirborne.parent = transform;
		ReqFadeIn ();
		StartCoroutine (Exec ());
	}

	IEnumerator Exec()
	{
		while(true){
			switch (_step) {
			case Step.FadeIn:
				if(_tAirborne.position.y > _dstPos.y){
					_tAirborne.position -= _spdDown*Vector3.up;
				} else {
					_lastTime = -1.0f;
					_spdWave = 0.0f;
					_step = Step.Running;
				}
				break;
			case Step.Running:
				float a = _coefWave*(transform.position.y + _endHeight - _tAirborne.position.y);	// assume weight == 1
				_spdWave += a;
				_tAirborne.position += _spdWave*Vector3.up;

				if(_agents.Count > 0 && Time.realtimeSinceStartup > _lastTime + _itvCreate){
					Transform tSpawn = _tAirborne.Find("CreatMonster");
					_agents[0].Pos = tSpawn != null ? tSpawn.position : _tAirborne.position;
					_agents[0].protoId &= ~EntityProto.IdAirborneAllMask;
					MonsterEntityCreator.CreateMonster(_agents[0]);
					_agents.RemoveAt(0);
					_lastTime = Time.realtimeSinceStartup;
				}
				break;
			case Step.FadeOut:
				if(_tAirborne.position.y < _dstPos.y){
					_tAirborne.position += _spdDown*Vector3.up;
				} else {
					Destroy(gameObject);
				}
				break;
			}
			yield return 0;
		}
	}

	void ReqFadeIn()
	{
		_tAirborne.position = transform.position + _initHeight * Vector3.up;
		_dstPos = transform.position + (_endHeight-_endWaveAmp) * Vector3.up;
		_step = Step.FadeIn;
	}
	void ReqFadeOut(bool bImm)
	{
		_dstPos = bImm ? _tAirborne.position : (transform.position + _initHeight * Vector3.up);
		_step = Step.FadeOut;
	}
	public static MonsterAirborne CreateAirborne(Vector3 landPos, Type type)
	{
		GameObject go = new GameObject ("Airborne_" + type);
		MonsterAirborne mab = go.AddComponent<MonsterAirborne> ();
		mab._type = type;
		mab.transform.position = landPos;
		return mab;
	}
	public static void DestroyAirborne(MonsterAirborne mab, bool bImm = false)
	{
		mab.ReqFadeOut(bImm);
	}
	public void AddAirborneReq(SceneEntityPosAgent agent)
	{
		_agents.Add (agent);
	}
}