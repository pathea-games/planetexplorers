using UnityEngine;
using System.Collections;

public class TutorStepItem : MonoBehaviour {
    public UILabel label;
    public int itemId;

    public string Text
    {
        set
        {
            label.text = value;
        }
    }

    public Vector2 Size
    {
        get { return label.relativeSize*label.transform.localScale.x; }
    }

    public void Selected()
    {
        //label.color = Color.blue;
        label.color = new Color32(255, 200, 70, 255);
    }

    void Start()
    {
    //    //UIPanel panel = transform.parent.GetComponent<UIPanel>();
    //    Vector2 size = Size;

    //    Debug.Log(size);

        BoxCollider bc = gameObject.GetComponent<BoxCollider>();

        bc.size = new Vector3(bc.size.x, Size.y, bc.size.z);

    //    //UILabel label = gameObject.GetComponentInChildren<UILabel>();
    //    //label.transform.localScale = new Vector3(24f, 24f, 1f);
    //    //label.transform.localPosition = new Vector3(-size.x / 2 + 10f, 0f, 0f);

    }
}
