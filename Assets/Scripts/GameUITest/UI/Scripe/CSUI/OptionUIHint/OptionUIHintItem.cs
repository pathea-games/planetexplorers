using UnityEngine;
using System.Collections;

public class OptionUIHintItem : MonoBehaviour
{

    public UILabel mLabel;

    public void SetHintInfo(string _info)
    {
        mLabel.text = _info;
    }

    public Bounds GetBounds()
    {
        Bounds finalBound = new Bounds();

        Bounds bound = NGUIMath.CalculateRelativeWidgetBounds(mLabel.transform);
        Vector3 scale = mLabel.transform.localScale;
        bound.min = Vector3.Scale(bound.min, scale);
        bound.max = Vector3.Scale(bound.max, scale);

        bound.center = mLabel.transform.localPosition;

        finalBound.Encapsulate(bound);
        return finalBound;
    }
}
