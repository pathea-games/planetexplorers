using UnityEngine;
using System.Collections;

public class MissionStateTipItem : MonoBehaviour
{

    [SerializeField]
    UILabel mState;
    [SerializeField]
    UILabel mContent;
    [SerializeField]
    UISprite mSprite;

    private float mTimer;
    private float mExistTime = 3.5f;

    // Use this for initialization
    void Start()
    {
        mTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        mTimer += Time.deltaTime;
        if (mTimer >= mExistTime)
            GameObject.Destroy(this.gameObject);
    }


    public void SetContent(string _state, string _content)
    {
        mState.text = _state;
        mContent.text = _content;

        if (StateSplit(_state) == "New Mission: ")
        {
            mSprite.spriteName = "system_b";
        }
        else if (StateSplit(_state) == "Mission Failed: ")
        {
            mSprite.spriteName = "system_r";
        }
        else if (StateSplit(_state) == "Mission Completed: ")
        {
            mSprite.spriteName = "system_y";
        }

        //Bounds bound = NGUIMath.CalculateRelativeWidgetBounds(mContent.transform);
        //mSprite.transform.localScale = new Vector3(bound.size.x, mSprite.transform.localScale.y, mSprite.transform.localScale.z);
    }

    string StateSplit(string splitedStr)
    {
        string result = "";
        result = splitedStr.Split(']')[1].Split('[')[0];
        return result;
    }

}
