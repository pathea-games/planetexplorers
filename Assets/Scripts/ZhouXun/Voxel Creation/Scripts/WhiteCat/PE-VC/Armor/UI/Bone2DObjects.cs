using UnityEngine;
using Pathea;
using System;

namespace WhiteCat
{
	public class Bone2DObjects : MonoBehaviour
	{
		static int[] groupFirst = new int[5] { 0, 1, 4, 12, 0 };
		static int[] groupLast = new int[5] { 0, 3, 11, 15, 15 };
		static int[] arrayIndexToBoneGroup = new int[16] { 0, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3 };
		static int[] arrayIndexToBoneIndex = new int[16] { 0, 0, 1, 2, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3 };

		static Color outlineColor1 = new Color(1f, 0.1f, 0f, 1f);
		static Color outlineColor2 = new Color(1f, 0.9f, 0f, 1f);
		static Color normalLightColor = new Color(1f, 1f, 1f, 1f);
		static Color darkLightColor = new Color(0.3f, 0.25f, 0.4f, 1f);

		static Vector2 viewSize = new Vector2(310f, 480f);
		const float lightDamping = 5f;

		static Vector3 minPosition = new Vector3(-0.2f, -0.2f, -0.2f);
		static Vector3 maxPosition = new Vector3(0.2f, 0.2f, 0.2f);
		static Vector3 minScale = new Vector3(0.25f, 0.25f, 0.25f);
		static Vector3 maxScale = new Vector3(2f, 2f, 2f);

		[SerializeField] UIBoneFocus[] _boneUIs;
		[SerializeField] TweenInterpolator _tools;
		[SerializeField] ArmorToolTip _toolTip;

		Camera _uiCamera;
		Camera _viewCamera;
		Light _light;
		ArmorBones _armorBones;
		PlayerArmorCmpt _armorCmpt;
		PeViewController _vc;

		ArmorType _activeGroup = ArmorType.None;
		int _highlightIndex = -1;
		int _mouseDownIndex = -1;

		bool _armorsVisible = false;
		bool _focused = false;
		bool _showHandles = false;
		bool _isDecoration = false;

		Transform _target;
		OutlineObject _outline;
		float _outlineStartTime;


		Vector2 _lastRightButtonDownPosition;


		int highlightIndex
		{
			set
			{
				if (_highlightIndex != value)
				{
					if (_highlightIndex >= 0)
					{
						_boneUIs[_highlightIndex].highlight = false;
					}
					_highlightIndex = value;

					if (_highlightIndex >= 0)
					{
						_boneUIs[_highlightIndex].highlight = true;
					}
				}
			}
		}


		void SetOutLine(Transform target)
		{
			if (!target)
			{
				if (_outline) Destroy(_outline);
				_outline = null;
            }
			else
			{
				if (_outline)
				{
					if (_outline.transform == target) return;
					Destroy(_outline);
				}
				_outline = target.gameObject.AddComponent<OutlineObject>();
				_outline.color = outlineColor1;
				_outlineStartTime = 0f;
            }
		}


		void SetMoveHandle(Transform target)
		{
			_target = null;
            if (_vc)
			{
				_target = target;
                _showHandles = target;
				_vc.moveHandle.gameObject.SetActive(_showHandles);
				_vc.moveHandle.Targets.Clear();
				if (_showHandles) _vc.moveHandle.Targets.Add(target);
				SetOutLine(target);
			}
		}


		void SetScaleHandle(Transform target)
		{
			_target = null;
			if (_vc)
			{
				_target = target;
				_showHandles = target;
				_vc.scaleHandle.gameObject.SetActive(_showHandles);
				_vc.scaleHandle.Targets.Clear();
				if (_showHandles) _vc.scaleHandle.Targets.Add(target);
				SetOutLine(target);
			}
		}


		void SetRotateHandle(Transform target)
		{
			_target = null;
			if (_vc)
			{
				_target = target;
				_showHandles = target;
				_vc.rotateHandle.gameObject.SetActive(_showHandles);
				_vc.rotateHandle.Targets.Clear();
				if (_showHandles) _vc.rotateHandle.Targets.Add(target);
				SetOutLine(target);
			}
		}


		void OnMoveHandle(Vector3 position)
		{
			if (_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
			{
				Vector3 value = _target.localPosition;
				value.x = Mathf.Clamp(value.x, minPosition.x, maxPosition.x);
				value.y = Mathf.Clamp(value.y, minPosition.y, maxPosition.y);
				value.z = Mathf.Clamp(value.z, minPosition.z, maxPosition.z);

				_target.localPosition = value;

				_armorCmpt.SetArmorPartPosition(
					arrayIndexToBoneGroup[_highlightIndex],
					arrayIndexToBoneIndex[_highlightIndex],
					_isDecoration,
					value);
			}
		}


		void OnRotateHandle(Quaternion rotation)
		{
			if (_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
			{
				Quaternion value = _target.localRotation;

				_armorCmpt.SetArmorPartRotation(
					arrayIndexToBoneGroup[_highlightIndex],
					arrayIndexToBoneIndex[_highlightIndex],
					_isDecoration,
					value);
			}
		}


		void OnScaleHandle(Vector3 scale)
		{
			if (_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
			{
				Vector3 value = _target.localScale;
				value.x = Mathf.Clamp(value.x, minScale.x, maxScale.x);
				value.y = Mathf.Clamp(value.y, minScale.y, maxScale.y);

				if (value.z > 0f)
				{
					value.z = Mathf.Clamp(value.z, minScale.z, maxScale.z);
				}
				else
				{
					value.z = Mathf.Clamp(value.z, -maxScale.z, -minScale.z);
				}

				_target.localScale = value;

				_armorCmpt.SetArmorPartScale(
					arrayIndexToBoneGroup[_highlightIndex],
					arrayIndexToBoneIndex[_highlightIndex],
					_isDecoration,
					value);
			}
		}


		public bool focused
		{
			set
			{
				if (_focused != value)
				{
					_focused = value;
					_mouseDownIndex = -1;

					if (value)
					{
						_boneUIs[_highlightIndex].enabled = true;
						_tools.transform.SetParent(_boneUIs[_highlightIndex].transform, false);
						_tools.speed = 1f;
						_tools.isPlaying = true;

						var node = _armorBones.nodes(_highlightIndex);
                        SetOutLine(node.normal ? node.normal : node.decoration);
					}
					else
					{
                        if (_highlightIndex >= 0)
                        {
                            _boneUIs[_highlightIndex].enabled = false;
                        }
                        _tools.speed = -1f;
						_tools.isPlaying = true;
						SetOutLine(null);
                    }
				}
			}
		}


		bool isDecoration
		{
			set
			{
				if (_highlightIndex >= 0)
				{
					var node = _armorBones.nodes(_highlightIndex);
					if (node.decoration && node.normal)
					{
						_isDecoration = value;
						SetOutLine(_isDecoration ? node.decoration : node.normal);
					}
					else
					{
						_isDecoration = node.decoration;
						SetOutLine(_isDecoration ? node.decoration : node.normal);
					}
				}
            }
		}


		public bool GetHoverBone(out int boneGroup, out int boneIndex)
		{
			if (_highlightIndex >= 0)
			{
				boneGroup = arrayIndexToBoneGroup[_highlightIndex];
				boneIndex = arrayIndexToBoneIndex[_highlightIndex];
                return true;
			}
			else
			{
				boneGroup = -1;
				boneIndex = -1;
                return false;
			}
		}


		void OnEnable()
		{
			HideAll();
        }


		void OnDisable()
		{
			HideAll();
			if (_light) _light.color = normalLightColor;
		}


		public bool dragWindowEnabled
		{
			set
			{
				GameUI.Instance.mUIPlayerInfoCtrl.dragObject.enabled = value;
			}
		}


		/// <summary>
		/// 初始化
		/// </summary>
		public void Init(PeViewController viewController, GameObject viewModel, PlayerArmorCmpt armorCmpt)
		{
			if (_vc != viewController)
			{
				_vc = viewController;

				_vc.moveHandle.OnTargetMove += OnMoveHandle;
				_vc.rotateHandle.OnTargetRotate += OnRotateHandle;
				_vc.scaleHandle.OnTargetScale += OnScaleHandle;

				_vc.moveHandle.OnBeginTargetMove += value => dragWindowEnabled = false;
				_vc.rotateHandle.OnBeginTargetRotate += value => dragWindowEnabled = false;
				_vc.scaleHandle.OnBeginTargetScale += value => dragWindowEnabled = false;

				_vc.moveHandle.OnEndTargetMove += value => dragWindowEnabled = true;
				_vc.rotateHandle.OnEndTargetRotate += value => dragWindowEnabled = true;
				_vc.scaleHandle.OnEndTargetScale += value => dragWindowEnabled = true;

				// 在线模式需要同步服务器数据
				if (PeGameMgr.IsMulti)
				{
					_vc.moveHandle.OnEndTargetMove += value =>
					{
						if (_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
						{
							_armorCmpt.C2S_SyncArmorPartPosition(
								arrayIndexToBoneGroup[_highlightIndex],
								arrayIndexToBoneIndex[_highlightIndex],
								_isDecoration,
								_target.localPosition);
						}
					};

					_vc.rotateHandle.OnEndTargetRotate += value =>
					{
						if (_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
						{
							_armorCmpt.C2S_SyncArmorPartRotation(
								arrayIndexToBoneGroup[_highlightIndex],
								arrayIndexToBoneIndex[_highlightIndex],
								_isDecoration,
								_target.localRotation);
						}
					};

					_vc.scaleHandle.OnEndTargetScale += value =>
					{
						if (_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
						{
							_armorCmpt.C2S_SyncArmorPartScale(
								arrayIndexToBoneGroup[_highlightIndex],
								arrayIndexToBoneIndex[_highlightIndex],
								_isDecoration,
								_target.localScale);
						}
					};
				}
			}

			_viewCamera = viewController.viewCam;
			_light = _viewCamera.GetComponentInParent<Light>();
            _armorBones = viewModel.GetComponent<ArmorBones>();
			_armorCmpt = armorCmpt;
        }


		public void HideAll()
		{
			SetMoveHandle(null);
			SetScaleHandle(null);
			SetRotateHandle(null);
			focused = false;
			SetArmorsVisible(false);
			SetActiveGroup(ArmorType.None);
			highlightIndex = -1;
		}


		/// <summary>
		/// 设置激活的骨骼 UI 组
		/// </summary>
		public void SetActiveGroup(ArmorType group)
		{
			if (group != _activeGroup)
			{
				if (_activeGroup != ArmorType.None)
				{
					for (int i = groupFirst[(int)_activeGroup]; i <= groupLast[(int)_activeGroup]; i++)
					{
						_boneUIs[i].enabled = false;
					}
				}

				_activeGroup = group;

				if (_activeGroup != ArmorType.None)
				{
					for (int i = groupFirst[(int)_activeGroup]; i <= groupLast[(int)_activeGroup]; i++)
					{
						_boneUIs[i].enabled = true;
					}
				}
            }

		}


		void SetArmorsVisible(bool visible)
		{
            if (visible != _armorsVisible)
			{
				_armorsVisible = visible;

				if (visible)
				{
                    _armorCmpt.ForEachBone
					(
						(boneGroup, boneIndex, hasNormal, hasDecoration) =>
						{
							_boneUIs[groupFirst[boneGroup] + boneIndex].enabled = hasNormal || hasDecoration;
						}
					);
				}
				else
				{
					for (int i=0; i<_boneUIs.Length; i++)
					{
						_boneUIs[i].enabled = false;
                    }
				}
            }
        }


		public void OnSettingsButtonClick()
		{
			if (_armorCmpt.hasRequest) return;

			SetMoveHandle(null);
			SetScaleHandle(null);
			SetRotateHandle(null);
			focused = false;
			SetActiveGroup(ArmorType.None);
			highlightIndex = -1;
			SetArmorsVisible(!_armorsVisible);
        }


		public void OnRemoveArmorButtonClick()
		{
			if (_highlightIndex >= 0)
			{
				Action<bool> callback = s =>
				{
					focused = false;
					SetArmorsVisible(true);
				};

				if (PeGameMgr.IsMulti)
				{
					if (_armorCmpt.hasRequest) return;

					_armorCmpt.C2S_RemoveArmorPart(
						arrayIndexToBoneGroup[_highlightIndex],
						arrayIndexToBoneIndex[_highlightIndex],
						_isDecoration,
						callback);
				}
				else
				{
					callback(
						_armorCmpt.RemoveArmorPart(
							arrayIndexToBoneGroup[_highlightIndex],
							arrayIndexToBoneIndex[_highlightIndex],
							_isDecoration));
                }
            }
		}


		public void OnMoveArmorButtonClick()
		{
			if (_armorCmpt.hasRequest) return;

			if (_highlightIndex >= 0)
			{
				focused = false;
                SetMoveHandle(
					_armorBones.GetArmorPart(_highlightIndex, _isDecoration));
            }
		}


		public void OnRotateArmorButtonClick()
		{
			if (_armorCmpt.hasRequest) return;

			if (_highlightIndex >= 0)
			{
				focused = false;
				SetRotateHandle(
					_armorBones.GetArmorPart(_highlightIndex, _isDecoration));
			}
		}


		public void OnScaleArmorButtonClick()
		{
			if (_armorCmpt.hasRequest) return;

			if (_highlightIndex >= 0)
			{
				focused = false;
				SetScaleHandle(
					_armorBones.GetArmorPart(_highlightIndex, _isDecoration));
			}
		}


		public void OnMirrorArmorButtonClick()
		{
			if (_highlightIndex >= 0)
			{
				Transform part = _armorBones.GetArmorPart(_highlightIndex, _isDecoration);

				Action<bool> callback = s =>
				{
					if (s && part)
					{
						Vector3 scale = part.localScale;
						scale.z = -scale.z;
						part.localScale = scale;
					}
				};

				if (PeGameMgr.IsMulti)
				{
					if (_armorCmpt.hasRequest) return;

					_armorCmpt.C2S_SwitchArmorPartMirror(
						arrayIndexToBoneGroup[_highlightIndex],
						arrayIndexToBoneIndex[_highlightIndex],
						_isDecoration,
						callback);
                }
				else
				{
					callback(
						_armorCmpt.SwitchArmorPartMirror(
							arrayIndexToBoneGroup[_highlightIndex],
							arrayIndexToBoneIndex[_highlightIndex],
							_isDecoration));
				}
            }
		}


		void LateUpdate()
		{
			if (_viewCamera && _armorBones && _armorCmpt)
			{
				if (_armorCmpt.hasRequest) return;


				// 计算鼠标在界面上的位置
				if (_uiCamera == null) _uiCamera = GetComponentInParent<Camera>();
				Vector2 cursor = Input.mousePosition - _uiCamera.WorldToScreenPoint(transform.position);
				_toolTip.UpdateToolTip(cursor);

				// 右键点击检查
				bool rightClick = false;
				if (Input.GetMouseButtonDown(1)) _lastRightButtonDownPosition = cursor;
				if (Input.GetMouseButtonUp(1)) rightClick = (_lastRightButtonDownPosition - cursor).sqrMagnitude <= 2.1f;

				// 显示激活的骨骼组
				if (_activeGroup != ArmorType.None)
				{
					bool hasHover = false;

					for (int i = groupFirst[(int)_activeGroup]; i <= groupLast[(int)_activeGroup]; i++)
					{
						// 更新骨骼 UI 的位置
						_boneUIs[i].UpdatePosition(_armorBones.nodes(i).bone, _viewCamera);

						// 更新高亮骨骼
						if (!hasHover && _boneUIs[i].isHover(cursor))
						{
							hasHover = true;
							highlightIndex = i;
						}
					}

					if (!hasHover) highlightIndex = -1;
				}

				// 显示已装备的装甲
				else if (_armorsVisible)
				{
					bool hasHover = false;

					for (int i = 0; i < 16; i++)
					{
						if (_boneUIs[i].enabled)
						{
							// 更新骨骼 UI 的位置
							_boneUIs[i].UpdatePosition(_armorBones.nodes(i).bone, _viewCamera);

							// 更新高亮骨骼
							if (!hasHover && _boneUIs[i].isHover(cursor))
							{
								hasHover = true;
								highlightIndex = i;
							}
						}
					}

					if (!hasHover)
					{
						highlightIndex = -1;
						if (rightClick) OnSettingsButtonClick();
					}
					else
					{
						_boneUIs[_highlightIndex].pressing = Input.GetMouseButton(0);

						if (Input.GetMouseButtonDown(0))
						{
							_mouseDownIndex = _highlightIndex;
						}

						if (Input.GetMouseButtonUp(0) && _mouseDownIndex == _highlightIndex)
						{
							SetArmorsVisible(false);
							isDecoration = false;
							focused = true;
						}
					}
				}

				// 显示选中的装甲
				else if (_focused)
				{
					_boneUIs[_highlightIndex].UpdatePosition(_armorBones.nodes(_highlightIndex).bone, _viewCamera);
					if (_boneUIs[_highlightIndex].isHover(cursor))
					{
						_boneUIs[_highlightIndex].pressing = Input.GetMouseButton(0);
						if (Input.GetMouseButtonUp(0))
						{
							isDecoration = !_isDecoration;
						}
					}
					else
					{
						if (rightClick) OnSettingsButtonClick();
					}
                }

				// 显示操作控件
				else if (_showHandles)
				{
					Vector3 screenPosition = new Vector3(
						cursor.x * _viewCamera.targetTexture.width / viewSize.x,
						cursor.y * _viewCamera.targetTexture.height / viewSize.y);

					_vc.moveHandle.customMousePosition = screenPosition;
					_vc.rotateHandle.customMousePosition = screenPosition;
					_vc.scaleHandle.customMousePosition = screenPosition;

					if (rightClick)
					{
						SetMoveHandle(null);
						SetRotateHandle(null);
						SetScaleHandle(null);
						focused = true;
					}
				}

				if (_focused || _showHandles)
				{
					// 描边动画
					if (_outline)
					{
						_outlineStartTime += Time.unscaledDeltaTime;
						_outlineStartTime %= 1f;
						_outline.color = Interpolation.Parabolic(_outlineStartTime) * (outlineColor2 - outlineColor1) + outlineColor1;
					}
				}
			}

			if (_light)
			{
				if (_activeGroup != ArmorType.None || _armorsVisible || _showHandles || _focused)
				{
					_light.color = Color.Lerp(_light.color, darkLightColor, Time.unscaledDeltaTime * lightDamping);
				}
				else
				{
					_light.color = Color.Lerp(_light.color, normalLightColor, Time.unscaledDeltaTime * lightDamping);
				}
			}
		}
	}
}