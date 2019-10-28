using UnityEngine;
using System;
using System.Collections.Generic;

namespace WhiteCat
{

	public class BonePanelUI : MonoBehaviour
	{
		[SerializeField] GameObject _maleUI;
		[SerializeField] GameObject _femaleUI;
		[SerializeField] Color _maleNormalColor;
		[SerializeField] Color _femaleNormalColor;
		[SerializeField] Color _hilightColor;
		[SerializeField] List<ButtonGroup> _maleButtons;
		[SerializeField] List<ButtonGroup> _femaleButtons;

		bool _awaked;
		VCPArmorPivot _part;

        public int ArmorPartIndex
        {
            get
            {
                return _part != null ? _part.showIndex : 0;
            }
        }

		[Serializable]
		class ButtonGroup
		{
			[SerializeField] UIButton[] _buttonGroup;
			UIWidget[] _widgets;

			public bool disabled
			{
				set
				{
					foreach(var item in _buttonGroup)
					{
						item.isEnabled = !value;
                    }
				}
			}

			public void SetColor(int index, Color color)
			{
				_buttonGroup[index].defaultColor = color;
				_widgets[index].color = color;
				_buttonGroup[index].UpdateColor(_buttonGroup[index].isEnabled, true);
            }

			public void Init(BonePanelUI panel)
			{
				_widgets = new UIWidget[_buttonGroup.Length];
				
				for (int i=0; i< _buttonGroup.Length; i++)
				{
					_widgets[i] = _buttonGroup[i].GetComponentInChildren<UIWidget>();

					var listener = _buttonGroup[i].gameObject.AddComponent<UIEventListener>();
					int index = i;

					listener.onClick += go=> panel.SwitchBone(index);
                }
			}
		}


		void SwitchBone(int index)
		{
			_maleButtons[(int)_part.armorType].SetColor(_part.showIndex, _maleNormalColor);
			_femaleButtons[(int)_part.armorType].SetColor(_part.showIndex, _femaleNormalColor);
			_part.showIndex = index;
			_maleButtons[(int)_part.armorType].SetColor(_part.showIndex, _hilightColor);
			_femaleButtons[(int)_part.armorType].SetColor(_part.showIndex, _hilightColor);
		}


		void SwitchGender()
		{
			_part.isMale = !_part.isMale;
			_maleUI.SetActive(_part.isMale);
			_femaleUI.SetActive(!_part.isMale);

        }


		public void Show(VCPArmorPivot part)
		{
			if (gameObject.activeSelf) return;

			if (part.armorType != ArmorType.Decoration)
			{
				_part = part;
				transform.parent.gameObject.SetActive(true);
				gameObject.SetActive(true);
                _maleUI.SetActive(_part.isMale);
				_femaleUI.SetActive(!_part.isMale);

				for (int i = 0; i < 4; i++)
				{
					_maleButtons[i].disabled = (int)part.armorType != i;
					_femaleButtons[i].disabled = (int)part.armorType != i;
                }

				_maleButtons[(int)part.armorType].SetColor(_part.showIndex, _hilightColor);
				_femaleButtons[(int)part.armorType].SetColor(_part.showIndex, _hilightColor);
			}
		}


		public void Hide()
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(false);
				_maleButtons[(int)_part.armorType].SetColor(_part.showIndex, _maleNormalColor);
				_femaleButtons[(int)_part.armorType].SetColor(_part.showIndex, _femaleNormalColor);
			}
		}


		void Awake()
		{
				for (int i = 0; i < 4; i++)
				{
					_maleButtons[i].Init(this);
					_femaleButtons[i].Init(this);
				}
		}
	}

}