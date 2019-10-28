using UnityEngine;
using System.Collections;

namespace TrainingScene{
	public class TrainingTransmitter : MonoBehaviour {
		//bool disable;
		void OnTriggerEnter()
		{return;
//			if(disable)
//				return;
//			disable = true;
//			if(TrainingTaskManager.isNewGame)
//			{
//				MessageBox_N.ShowYNBox(PELocalization.GetString(8000153), LoadStoryScene);
//			}
//			else
//			{
//				MessageBox_N.ShowYNBox(PELocalization.GetString(8000153), LoadMenuScene);
//			}
		}
		void OnTriggerExit()
		{
			//disable = false;
		}
		public void LoadStoryScene()
		{
//			GameConfig.IsNewGame = true;
//			GameConfig.GameMode = GameConfig.EGameMode.Singleplayer_Story;
//			TrainingTaskManager.Instance.CompleteMission();
//			LogoGui_N.Instance.StartMovie();
		}
		public void LoadMenuScene()
		{
//			GameConfig.IsNewGame = true;
//			GameConfig.GameMode = GameConfig.EGameMode.None;
//			LogoGui_N.Instance.SetNextScene(GameConfig.MainMenuSceneName, 0f);
		}
	}

}
