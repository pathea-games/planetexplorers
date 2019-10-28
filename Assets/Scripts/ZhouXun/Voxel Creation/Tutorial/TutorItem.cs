using UnityEngine;
using System.Collections;

public class TutorItem : MonoBehaviour {

    public int itemId;

    void Start()
    {
        Reset();
    }

    void Reset()
    {
        UIGrid grid = transform.parent.GetComponent<UIGrid>();
        Vector2 size = new Vector2(grid.cellWidth, grid.cellHeight);

        BoxCollider bc = gameObject.GetComponent<BoxCollider>();
        bc.size = new Vector3(size.x, size.y, 1f);
        
        UILabel label = gameObject.GetComponentInChildren<UILabel>();
        //label.transform.localScale = new Vector3(24f, 24f, 1f);
        label.transform.localPosition = new Vector3(-size.x / 2 + 10f, 0f, 0f);

        UISlicedSprite sprite = gameObject.GetComponentInChildren<UISlicedSprite>();
        sprite.transform.localScale = new Vector3(size.x, size.y, 1f);

        UICheckbox checkBox = gameObject.GetComponent<UICheckbox>();
        checkBox.radioButtonRoot = transform.parent;
    }

    public void SetText(string text)
    {
        UILabel label = gameObject.GetComponentInChildren<UILabel>();
        label.text = text.ToLocalizationString();
    }

    public void Checked()
    {
        UICheckbox checkBox = gameObject.GetComponent<UICheckbox>();
        checkBox.isChecked = true;
    }
}
