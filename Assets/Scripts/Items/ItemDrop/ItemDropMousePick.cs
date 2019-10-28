using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MousePickableChildCollider))]
public class ItemDropMousePick : ItemDrop
{
    void Start()
    {
        ////test
        //AddItem(1277, 1);
        //AddItem(1034, 2);

        MousePickableChildCollider mousePickable = GetComponent<MousePickableChildCollider>();
        if (mousePickable != null)
        {
            mousePickable.eventor.Subscribe((object sender, MousePickable.RMouseClickEvent e) =>
                                            {
                                                OpenGui(e.mousePickable.transform.position);
                                            });
        }
    }
}
