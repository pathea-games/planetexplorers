using UnityEngine;
using UnityEngine.Events;

namespace WhiteCat
{
	public class UIArmorSuitEvent : MonoBehaviour
	{
		public UnityEvent enterEvent;
		public UnityEvent exitEvent;
		public UnityEvent clickEvent;

		[Header("foreground")]
		public UIWidget[] foreground;
		public UIButton foregroundButton;
		public Color selectedForegroundColor;
		public Color normalForegroundColor;

		[Header("background")]
		public UIWidget background;
		public Color selectedBackgroundColor;
		public Color normalBackgroundColor;


		public void CallEnterEvent()
		{
			if (enterEvent != null) enterEvent.Invoke();
        }


		public void CallExitEvent()
		{
			if (exitEvent != null) exitEvent.Invoke();
		}


		public void CallClickEvent()
		{
			if (clickEvent != null) clickEvent.Invoke();
		}


		public void SetSelected(bool selected)
		{
			Color color = selected ? selectedForegroundColor : normalForegroundColor;

			for (int i=0; i< foreground.Length; i++)
			{
				foreground[i].color = color;
            }
			foregroundButton.defaultColor = color;

			background.color = selected ? selectedBackgroundColor : normalBackgroundColor;
        }
    }
}