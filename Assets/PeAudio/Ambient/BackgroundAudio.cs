using UnityEngine;
using System.Collections.Generic;

namespace PeAudio
{
	public partial class BackgroundAudio : MonoBehaviour
	{
		private Dictionary<int, SectionDesc> _sectionDescs = null;

		private BGMInst bgm;
		private Dictionary<int, BGAInst> bga;
		//private List<SPOTInst> spot;


		public int sectionId
		{
			get
			{
				string sceneName = GlobalBehaviour.currentSceneName;
				if (sceneName == "GameStart")
					return -1;
				if (sceneName == "GameMainMenu")
					return -2;
				if (sceneName == "GameRoleCustom")
					return -3;
				if (sceneName == "GameLobby")
					return -4;
				if (sceneName == "MLoginScene")
					return -5;
				if (sceneName == "PeGame")
				{
					if (Pathea.PeCreature.Instance != null && 
					    Pathea.PeCreature.Instance.mainPlayer != null)
					{
						//Vector3 playerPos = Pathea.PeCreature.Instance.mainPlayer.position;

					}
				}
				return 0;
			}
		}

		public string sectionName
		{
			get
			{
				int id = sectionId;
				if (_sectionDescs.ContainsKey(id))
					return _sectionDescs[id].name;
				return "";
			}
		}
		
		public SectionDesc currentSectionDesc
		{
			get
			{
				int id = sectionId;
				if (_sectionDescs.ContainsKey(id))
					return _sectionDescs[id];
				return null;
			}
		}
		
		GameObject bga_group_go;
		GameObject spot_group_go;

		void Init ()
		{
			LoadConfig();
			GameObject bgm_go = new GameObject ("BGM");
			bgm_go.transform.parent = this.transform;
			bgm_go.transform.localPosition = Vector3.zero;
			bga_group_go = new GameObject ("BGA");
			bga_group_go.transform.parent = this.transform;
			bga_group_go.transform.localPosition = Vector3.zero;
			spot_group_go = new GameObject ("SPOT");
			spot_group_go.transform.parent = this.transform;
			spot_group_go.transform.localPosition = Vector3.zero;

			bgm = bgm_go.AddComponent<BGMInst>();
			bgm.audioSrc.path = "";
			bgm.audioSrc.playOnReset = true;

			bga = new Dictionary<int, BGAInst>();
			//spot = new List<SPOTInst> ();
		}

		void Start ()
		{
			Init();
		}

		int prevSectionId = 0;
		void Update ()
		{
			if (sectionId != prevSectionId)
			{
				float prewarm = 0;
				float postwarm = 0;
				float predamp = 0.05f;
				float postdamp = 0.05f;
				if (prevSectionId > 0 && sectionId > 0)
				{
					prewarm = 15;
					postwarm = 10;
					predamp = 0.005f;
					postdamp = 0.05f;
				}
				else if (prevSectionId > 0 && sectionId < 0)
				{
					prewarm = 0;
					postwarm = 2;
					predamp = 0.08f;
					postdamp = 0.08f;
				}
				else if (prevSectionId < 0 && sectionId > 0)
				{
					prewarm = 0;
					postwarm = 20;
					predamp = 0.02f;
					postdamp = 0.08f;
				}
				else if (prevSectionId < 0 && sectionId < 0)
				{
					prewarm = 0;
					postwarm = 0;
					predamp = 0.03f;
					postdamp = 0.03f;
				}


				if (currentSectionDesc != null)
					bgm.ChangeBGM(currentSectionDesc.bgmDesc.path, prewarm, postwarm, predamp, postdamp);
				else
					bgm.ChangeBGM("", prewarm, postwarm, predamp, postdamp);
			}
			prevSectionId = sectionId;
		}

		BGAInst CreateBGA (int id)
		{
			if (_sectionDescs.ContainsKey(id))
			{
				SectionDesc sectiondesc = _sectionDescs[id];

				GameObject bga_go = new GameObject ("BGA " + id.ToString() + " : " + sectiondesc.name);
				bga_go.transform.parent = bga_group_go.transform;
				bga_go.transform.localPosition = Vector3.zero;

				BGAInst inst = bga_go.AddComponent<BGAInst>();
				inst.audioSrc.path = sectiondesc.bgaDesc.path;
				inst.audioSrc.playOnReset = false;

				bga[id] = inst;
				return inst;
			}
			return null;
		}

		void CreateSPOT (int id)
		{

		}
	}
}
