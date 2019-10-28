using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class N_ImageButton : UIComponent 
{
	UILabel lable;
	UISprite spr;
	BoxCollider boxCollider;
	UISpecularHandler texHandler;
	UIButtonEffect	effect;
	//public

	public float lbAlphaFlag = 2f;
	public float normalItensity = 0.35f;
	public float hoverItensity = 0.7f;
	public float pressedItensity = 0.8f;
	public float disableItensity = 0.15f;

	float lbNormal { get {return lbAlphaFlag * normalItensity;} } 
	float lbHover { get {return lbAlphaFlag * hoverItensity;} } 
	float lbPressed { get {return lbAlphaFlag * pressedItensity;} } 
	float lbDisable { get {return lbAlphaFlag * disableItensity;} }

    private bool m_Init = false;
	
	// events
	public UIConpomentEvent e_OnClick = new UIConpomentEvent();

	public bool isEnabled 
	{
		get {return !_disable;}
		set { disable = !value;}
	}

	public bool disable
	{
		get
		{
			return _disable;
		}
		set
		{
            //lz-2016.11.06 设置按钮状态的时候可能组件还没到位，按钮显示状态不对，这里检测一下
            Init();

            if (_disable == value)
			    return;
			_disable = value;

			if (boxCollider == null) 
				boxCollider = GetComponent<BoxCollider>();
			if (boxCollider != null) 
				boxCollider.enabled = !_disable;

			if (texHandler != null)
				texHandler.Intensity = _disable ? disableItensity : normalItensity;

			if (spr != null)
				spr.color = new Color(spr.color.r,spr.color.g,spr.color.b, (_disable ? lbDisable : lbNormal) );
			if (lable != null)
				lable.color = new Color(lable.color.r,lable.color.g,lable.color.b, (_disable ? lbDisable : lbNormal) );
            //log:lz-2016.05.26  如果鼠标悬浮在按钮上，按钮禁用的时候把选中效果去掉
            if (effect != null)
            {
                effect.enabled = !_disable;
            }
		}
	}

	private bool  _disable = false;
	

	void OnEnable ()
	{
        //lz-2016.11.08 设置按钮状态的时候可能组件还没到位，按钮显示状态不对，这里检测一下
        Init();

        if (!disable)
		{
            bool enter = UICamera.IsHighlighted(gameObject);
			if (spr != null)
				spr.color = new Color(spr.color.r,spr.color.g,spr.color.b, (enter ? lbHover : lbNormal) );
			if (lable != null)
				lable.color = new Color(lable.color.r,lable.color.g,lable.color.b, (enter ? lbHover : lbNormal) );
			if (texHandler != null)
				texHandler.Intensity =  enter ? hoverItensity : normalItensity;
			if (effect != null) 
			{
				if (enter)
					effect.MouseEnter();
				else 
					effect.MouseLeave();
			}
		}
	}

    void Awake()
    {
        Init();
    }

    void Init()
    {
        if (!m_Init)
        {
            if (spr == null)
            {
                UISprite[] sprs = GetComponentsInChildren<UISprite>(true);
                if (null != sprs && sprs.Length > 0) spr = sprs[0];
            }
            if (lable == null)
            {
                UILabel[] lbls = GetComponentsInChildren<UILabel>(true);
                if (null != lbls && lbls.Length > 0) lable = lbls[0];
            }
            if (texHandler == null)
            {
                UISpecularHandler[] handlers = GetComponentsInChildren<UISpecularHandler>(true);
                if (null != handlers && handlers.Length > 0) texHandler = handlers[0];
            }
            if (boxCollider == null) boxCollider = GetComponent<BoxCollider>();
            if (effect == null) effect = GetComponent<UIButtonEffect>();
            if (lable != null) lable.color = new Color(lable.color.r, lable.color.g, lable.color.b, lbNormal);
            if (texHandler != null) texHandler.Intensity = normalItensity;
            m_Init = true;
        }
    }

	void OnHover (bool isOver)
	{
		if (enabled && !_disable)
		{
			if (spr != null)
				spr.color = new Color(spr.color.r,spr.color.g,spr.color.b, (isOver ? lbHover : lbNormal) );
			if (lable != null)
				lable.color = new Color(lable.color.r,lable.color.g,lable.color.b, (isOver ? lbHover : lbNormal) );
			if (texHandler != null)
				texHandler.Intensity = isOver ? hoverItensity : normalItensity;
			if (effect != null) 
			{
				if (isOver)
					effect.MouseEnter();
				else 
					effect.MouseLeave();
			}
		}
	}
	
	void OnPress (bool pressed)
	{
		if (enabled && !_disable)
		{
			if (spr != null)
				spr.color = new Color(spr.color.r,spr.color.g,spr.color.b, (pressed ? lbPressed : lbNormal) );
			if (lable != null)
				lable.color = new Color(lable.color.r,lable.color.g,lable.color.b, (pressed ? lbPressed : lbNormal) );
			if (texHandler != null)
				texHandler.Intensity = pressed ? pressedItensity : normalItensity;
			if (effect != null) 
			{
				if (pressed)
					effect.MouseDown();
				else 
					effect.MouseUp();
			}
		}
	}

	void OnClick () 
	{
		if (eventReceiver!= null)
			e_OnClick.Send(eventReceiver,this);
	}
}
