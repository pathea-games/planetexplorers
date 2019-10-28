using UnityEngine;

namespace PeCustom
{
	public class CustomGUIMgr
	{
		public void DrawGUI ()
		{
			GUI.color = Color.white;
			if (OnGUIDrawing != null)
				OnGUIDrawing();
			TitleGUI();
		}

		public delegate void DNotify ();
		public delegate void DStringNotify (string str);
		public event DNotify OnGUIDrawing;
		public event DStringNotify OnGUIResponse;
		public void GUIResponse (string eventname)
		{
			if (OnGUIResponse != null)
				OnGUIResponse(eventname);
		}

		private string _title = "";
		private string _subtitle = "";
		private float _title_starttime = 0;
		private float _title_showtime = 0;
		private float _title_lifetime = 0;
		private Color _title_color = Color.clear;

		public void ShowTitle (string title, string subtitle, float time, Color color)
		{
			_title = title;
			_subtitle = subtitle;
			_title_starttime = Time.time;
			_title_showtime = time;
			_title_lifetime = time;
			_title_color = color;
		}

		void TitleGUI ()
		{
			_title_lifetime = _title_showtime - (Time.time - _title_starttime);

			float a1 = Mathf.Clamp01((_title_showtime - _title_lifetime) * 2);
			float a2 = Mathf.Clamp01(_title_lifetime * 2);
			Color finalColor = _title_color;
			finalColor.a = Mathf.Min(a1, a2);

			if (_title_lifetime > 0.001f)
			{
				Color old = GUI.color;
				GUI.color = finalColor;
				GUI.skin = Resources.Load<GUISkin>("GUISkin/CustomGames");
				GUI.Label(new Rect (0, 160, Screen.width, 100), _title, "TitleText");
				GUI.Label(new Rect (0, 260, Screen.width, 50), _subtitle, "SubtitleText");
				GUI.color = old;
			}
		}
	}
}
