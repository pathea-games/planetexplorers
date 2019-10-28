using UnityEngine;
using System.Collections;
using Pathea;

public class TeammateAnimationCtrl : MonoBehaviour
{
    public UISprite ArrayBtnBg;
    public UIButton ArrayBtn;
    public TweenPosition TweenPos;
    public TweenScale TweenScale;
    public GameObject TeammateGo;
    public TeammateCtrl MyTeammateCtrl;

    private bool m_Tweening;
    private bool m_Open;

    void Awake()
    {
        this.TeammateGo.SetActive(PeGameMgr.IsMulti);
        this.m_Open = false;
        this.m_Tweening = false;
    }

    void Start()
    {
        UIEventListener.Get(this.ArrayBtn.gameObject).onClick = (go) => this.ArrayClickEvent();
        this.TweenScale.onFinished = (tween) => this.FinishedCalled();
    }

    void FinishedCalled()
    {
        this.m_Tweening = false;
        this.m_Open = !this.m_Open;
    }

    void PlayTween(bool play)
    {
        this.TweenPos.Play(play);
        this.TweenScale.Play(play);
    }

    void UpdateBtnState(bool isOpen)
    {
        this.ArrayBtnBg.spriteName = isOpen ? "leftbutton" : "rightbutton";
        this.ArrayBtnBg.MakePixelPerfect();
    }

    void ArrayClickEvent()
    {
        if (!this.m_Tweening)
        {
            this.m_Tweening = true;
            this.PlayTween(this.m_Open);
            this.UpdateBtnState(this.m_Open);
            if (this.m_Open)
            {
                this.MyTeammateCtrl.RefreshTeammate();
            }
        }
    }
}
