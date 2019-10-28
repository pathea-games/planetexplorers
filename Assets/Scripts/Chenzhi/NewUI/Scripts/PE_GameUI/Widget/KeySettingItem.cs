#define ConflictMsgOn	// else auto resolve conflict
using UnityEngine;
using System.Collections;
using InControl;

public interface IKeyJoyAccessor
{
    // Return: 0: not exist; +i: exist not locked; -i : exist locked
    int FindInArray(KeySettingItem[] items, KeyCode key);
    void Set(KeySettingItem item, KeyCode keyToSet);
    KeyCode Get(KeySettingItem item);
}

public class KeySettingItem : MonoBehaviour
{
    public int _keySettingName = 0;
    public PeInput.KeyJoySettingPair _keySetting;

    public UILabel mFunctionContent;
    public UILabel mKeyContent;
    public UILabel mCtrlContent;
    public UISprite mLockSpr;

    public UICheckbox mKeyCheckBox;
    public UICheckbox mCtrlCheckBox;

    [SerializeField]
    private GameObject m_OpKeyBtnsParent;
    [SerializeField]
    private UIButton m_ApplyKeyBtn;
    [SerializeField]
    private UIButton m_CancelKeyBtn;
    [SerializeField]
    private GameObject m_OpCtrlBtnsParent;
    [SerializeField]
    private UIButton m_ApplyCtrlBtn;
    [SerializeField]
    private UIButton m_CancelCtrlBtn;
    

    KeyCode _keyToSet;
	InputControlType _joyToSet;
	bool _bHover;
	bool _bApply;

	KeyCode _ctrlKeyDown;

    void Start()
    {
        mKeyCheckBox.radioButtonRoot = transform.parent;
        mCtrlCheckBox.radioButtonRoot = transform.parent;
        _keyToSet = KeyCode.None;
		_joyToSet = InputControlType.None;
		_bHover = false;
		_bApply = false;
        mKeyCheckBox.onStateChange += SetKey;
        mCtrlCheckBox.onStateChange += SetJoy;

        this.mKeyCheckBox.IsUseSelfOnClick = false;
        this.mCtrlCheckBox.IsUseSelfOnClick = false;
		UIEventListener.Get(this.mKeyCheckBox.gameObject).onClick =(go)=> this.OnClick(mKeyCheckBox);
		UIEventListener.Get(this.mCtrlCheckBox.gameObject).onClick = (go) => this.OnClick(mCtrlCheckBox);
        UIEventListener.Get(this.m_ApplyKeyBtn.gameObject).onClick =(go)=> this.ApplyKeyChange();
        UIEventListener.Get(this.m_CancelKeyBtn.gameObject).onClick = (go) => this.CancelKeyChange();
        //lz-2016.12.07 添加手柄修改按键操作
        UIEventListener.Get(this.m_ApplyCtrlBtn.gameObject).onClick = (go) => this.ApplyCtrlChange();
        UIEventListener.Get(this.m_CancelCtrlBtn.gameObject).onClick = (go) => this.CancelCtrlChange();
        UIEventListener.Get (this.mKeyCheckBox.gameObject).onHover = (go, bHover) => _bHover = bHover;
    }
	void OnClick (UICheckbox item) 
	{
		if (item.enabled) {
			if(Input.GetMouseButtonUp(0) && !item.isChecked)	
				item.isChecked = !item.isChecked; 
		}
	}

	void Update()
	{
		_ctrlKeyDown = KeyCode.None;
		for(KeyCode key = KeyCode.JoystickButton0; key <= KeyCode.JoystickButton19; ++key)
		{
			if(Input.GetKeyDown(key))
			{
				_ctrlKeyDown = key;
				break;
			}
		}
	}

    // Update is called once per frame
    void OnGUI()
    {
        if (_keySetting == null)
            return;
        if (mKeyCheckBox.isChecked && _keySetting._keyLock)
            mKeyCheckBox.isChecked = false;
        if (mCtrlCheckBox.isChecked && _keySetting._joyLock)
            mCtrlCheckBox.isChecked = false;
        if (mLockSpr.enabled == false && _keySetting._keyLock)
            mLockSpr.enabled = true;

        if (_keySettingName != 0)
            mFunctionContent.text = PELocalization.GetString(_keySettingName);
        //mKeyContent.text = _keySetting._key.ToStr().Replace("Button","").Replace("Alpha","");
        //mCtrlContent.text = _keySetting._joy.ToStr().Replace("Button","").Replace("Alpha","");

		if (mCtrlCheckBox.isChecked || mKeyCheckBox.isChecked)
        {
			KeyCode curKey = _ctrlKeyDown;
			if(Event.current != null)
			{
				if(Event.current.type == EventType.KeyDown){
					curKey = Event.current.keyCode;
				} else if(Event.current.type == EventType.MouseDown && _bHover){
					curKey = KeyCode.Mouse0 + Event.current.button;
				}
			}

			if (mCtrlCheckBox.isChecked)
            {
				for(InputControlType type = InputControlType.Action1; type < InputControlType.Options; ++type)
				{
					if(InputManager.ActiveDevice.GetControl(type).WasPressed)
					{
						_joyToSet = type;
						mCtrlContent.text = _joyToSet.ToString();
						break;
					}
				}
                //mCtrlCheckBox.isChecked = false; // Uncheck to confirm because we want to support cimbination-key
            }
			else if (mKeyCheckBox.isChecked && curKey != KeyCode.None && curKey > KeyCode.Escape && curKey < KeyCode.JoystickButton0)
            {
				_keyToSet = curKey;
				if (Event.current.alt && curKey != KeyCode.LeftAlt && curKey != KeyCode.RightAlt) _keyToSet = PeInput.AltKey(_keyToSet);
				if (Event.current.shift && curKey != KeyCode.LeftShift && curKey != KeyCode.RightShift) _keyToSet = PeInput.ShiftKey(_keyToSet);
				if (Event.current.control && curKey != KeyCode.LeftControl && curKey != KeyCode.RightControl) _keyToSet = PeInput.CtrlKey(_keyToSet);

                mKeyContent.text = _keyToSet.ToStr();
                //mKeyCheckBox.isChecked = false; // Uncheck to confirm because we want to support cimbination-key
            }
        }
    }

    void ApplyKeyChange()
    {
		_bApply = true;
        mKeyCheckBox.isChecked = false;
		_bApply = false;
    }
    void CancelKeyChange()
    {
        _keyToSet = _keySetting._key;
        mKeyCheckBox.isChecked = false;
    }
	void ApplyCtrlChange()
	{
		_bApply = true;
		mCtrlCheckBox.isChecked = false;
		_bApply = false;
	}
	void CancelCtrlChange()
	{
		_joyToSet = _keySetting._joy;
		mCtrlCheckBox.isChecked = false;
	}

    void SetKey(bool state)
    {
        this.m_OpKeyBtnsParent.SetActive(state);
        if (!state && _bApply)
        {
            // set key into _keySetting
            if (_keyToSet != KeyCode.None && _keyToSet != _keySetting._key)
            {
#if ConflictMsgOn
				KeySettingItem itemConflict = UIOption.Instance.TestConflict(this, _keyToSet, s_keyAccessor);
				if (itemConflict != null){
                    //if(_keyToSet != KeyCode.Mouse0 && _keyToSet != KeyCode.Mouse1)
                    //{
                    //    //lw:2017.7.10:快捷键冲突时，忽略玩家输入，即设置必定不成功
                    //    MessageBox_N.ShowOkBox(PELocalization.GetString(8000178));
                    //}
                    //else
                    //{
                     MessageBox_N.ShowYNBox(PELocalization.GetString(8000178), ()=>{s_keyAccessor.Set(this, _keyToSet);});
                   // }
                } else {
					s_keyAccessor.Set(this, _keyToSet);
				}
#else
                if (!UIOption.Instance.TrySetKey(this, _keyToSet, s_keyAccessor))
                {
                    if (OptionUIHintFadeCtrl.Instance != null)
                    {
                        OptionUIHintFadeCtrl.Instance.AddOneHint(PELocalization.GetString(8000172));
                    }
                }
#endif
            }
        }
        else
        {
            _keyToSet = _keySetting._key;
        }
        mKeyContent.text = _keySetting._key.ToStr();
    }
	void SetJoy(bool state)
    {
        m_OpCtrlBtnsParent.SetActive(state);
        if (!state && _bApply)
        {
            // set joy into _keySetting
			if (_joyToSet != InputControlType.None && _joyToSet != _keySetting._joy)
            {
#if ConflictMsgOn
				KeySettingItem itemConflict = null;// UIOption.Instance.TestConflict(this, _joyToSet, s_joyAccessor);
				if (itemConflict != null){
					MessageBox_N.ShowYNBox(PELocalization.GetString(8000178), ()=>{s_joyAccessor.Set(this, _joyToSet);});
				} else {
					s_joyAccessor.Set(this, _joyToSet);
				}
#else
				if (!UIOption.Instance.TrySetKey(this, _joyToSet, s_joyAccessor))
                {
                    if (OptionUIHintFadeCtrl.Instance != null)
                    {
                        OptionUIHintFadeCtrl.Instance.AddOneHint(PELocalization.GetString(8000172));
                    }
                }
#endif
            }
        }
        else
        {
			_joyToSet = _keySetting._joy;
        }
        mCtrlContent.text = _keySetting._joy.ToString();
    }

    // Utils
    class KeyAccessor : IKeyJoyAccessor
    {
		public int FindInArray(KeySettingItem[] items, KeyCode key)
        {
            int n = items.Length;
            for (int i = 0; i < n; i++)
            {
                if (items[i]._keySetting._key == key)
                {
                    return items[i]._keySetting._keyLock ? -(i + 1) : (i + 1);
                }
            }
            return 0;
        }
        public void Set(KeySettingItem item, KeyCode keyToSet)
        {
            item._keySetting._key = keyToSet;
            item.mKeyContent.text = keyToSet.ToStr();
        }
        public KeyCode Get(KeySettingItem item)
        {
            return item._keySetting._key;
        }
    }
    class JoyAccessor : IKeyJoyAccessor
    {
		public int FindInArray(KeySettingItem[] items, InputControlType key)
        {
            int n = items.Length;
            for (int i = 0; i < n; i++)
            {
				if (items[i]._keySetting._joy == key)
                {
                    return items[i]._keySetting._joyLock ? -(i + 1) : (i + 1);
                }
            }
            return 0;
        }
		public void Set(KeySettingItem item, InputControlType keyToSet)
        {
            item._keySetting._joy = keyToSet;
            item.mCtrlContent.text = keyToSet.ToString();
        }

		#region IKeyJoyAccessor implementation

		public int FindInArray (KeySettingItem[] items, KeyCode key)
		{
			return 0;
		}


		public void Set (KeySettingItem item, KeyCode keyToSet)
		{
		}

		KeyCode IKeyJoyAccessor.Get (KeySettingItem item)
		{
			return KeyCode.None;
		}

		#endregion

        public InputControlType Get(KeySettingItem item)
        {
            return item._keySetting._joy;
        }
    }
    static KeyAccessor s_keyAccessor = new KeyAccessor();
    static JoyAccessor s_joyAccessor = new JoyAccessor();
}
