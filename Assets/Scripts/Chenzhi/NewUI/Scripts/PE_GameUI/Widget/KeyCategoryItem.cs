using UnityEngine;
using System.Collections;

public class KeyCategoryItem : MonoBehaviour
{

    public int mStringID;

    UILabel mLable;

    bool mHasContent = false;

    void Start()
    {
        mLable = GetComponentInChildren<UILabel>();
    }

    void TryLocalize()
    {
        if (mLable)
        {
            if (PELocalization.GetString(mStringID) != "")
            {
                mHasContent = true;
                mLable.text = PELocalization.GetString(mStringID);
            }
        }
    }
    void Update()
    {
        if (!mHasContent)
        {
            TryLocalize();
        }
    }

}
