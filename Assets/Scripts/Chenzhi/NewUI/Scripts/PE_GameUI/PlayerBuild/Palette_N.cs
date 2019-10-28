using UnityEngine;
using System.Collections;

public class Palette_N : MonoBehaviour
{
    public delegate void ChangeColorEvent(Color col);
    public event ChangeColorEvent e_ChangeColor = null;

    public UITexture mMask;
    public Camera mUICam;
    public UIScrollBar mAlphaScrollBar; //lz-2016.08.19 用滑动条控制取色的Alpha值

    float mClolorScale_x;

    Texture2D mColorTex;

    //Vector3 mTexPos;

    Vector3 mMaskPos;
    [HideInInspector]
    public Color CurCol;

    // Use this for initialization
    void Start()
    {
        mColorTex = GetComponent<UITexture>().mainTexture as Texture2D;
        //mTexPos = UIRoot.list[0].activeHeight / Screen.height * mUICam.WorldToScreenPoint(transform.position);

        mClolorScale_x = gameObject.transform.localScale.x / mColorTex.width;
        mMaskPos = mMask.transform.localPosition;

        mAlphaScrollBar.onChange = OnAlphaScroll;
    }

    //lz-2016.08.19 滑动条改变的时候调用改变事件
    void OnAlphaScroll(UIScrollBar sb)
    {
        ChangeColor();
    }

    void OnPress(bool isDown)
    {
        //mTexPos = UIRoot.list[0].activeHeight / Screen.height * mUICam.WorldToScreenPoint(transform.position);
        if (isDown && Input.GetMouseButtonDown(0))
            GetColor();
    }

    void OnDrag(Vector2 delta)
    {
        //Vector3 mousePos = Input.mousePosition;

        GetColor();
    }

    void GetColor()
    {
        RaycastHit hitInfo;

        Ray ray = mUICam.ScreenPointToRay(Input.mousePosition);
        Vector3 offsetPos;
        if (GetComponent<BoxCollider>().Raycast(ray, out hitInfo, 100))
        {
            offsetPos = transform.InverseTransformPoint(hitInfo.point);
            offsetPos.x *= mColorTex.width;
            offsetPos.y *= mColorTex.height;
        }
        else
            return;
        mMask.transform.localPosition = new Vector3((int)(mMaskPos.x + offsetPos.x * mClolorScale_x),
                                                     (int)(mMaskPos.y + offsetPos.y - transform.localScale.y), -6f);

        Color col = mColorTex.GetPixel((int)offsetPos.x, (int)offsetPos.y);
        col.a = 1f;
        CurCol = col;
        ChangeColor();
    }

    void ChangeColor()
    {
        float value = Mathf.Clamp01(mAlphaScrollBar.scrollValue);
        Color col = CurCol;
        col.r *= value;
        col.g *= value;
        col.b *= value;
        col.a = 1;
        if (e_ChangeColor != null)
            e_ChangeColor(col);
    }
}
