using UnityEngine;
using System.Collections;

public class ItemDraggingTorch : ItemDraggingArticle
{
    public override void OnDragOut()
    {
        base.OnDragOut();

        transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
    }
}
