using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemReceiver : MonoBehaviour
{
    public List<string> acceptTags;
    public Transform traySlot;

    public virtual void ScrollInTraySlot(ItemManager item)
    {
        if (!acceptTags.Contains(item.itemTag))
            return;

        item.transform.SetParent(traySlot);
        item.transform.localPosition = Vector3.zero;

        item.ResetChildPosition();

        OnReceive(item);
    }

    protected virtual void OnReceive(ItemManager item)
    {

    }
}
