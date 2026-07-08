using System.Linq;
using UnityEngine;

public class VisitorReceiver : ItemReceiver
{
    public VisitorController visitCtrl;
    public bool[] VisitorGetItems;
    bool allTrue = true;

    private void Start()
    {
        VisitorGetItems = new bool[acceptTags.Count];
    }

    public override void ScrollInTraySlot(ItemManager item)
    {
        if (!acceptTags.Contains(item.itemTag))
            return;

        item.transform.SetParent(traySlot);
        item.transform.localPosition = Vector3.zero;

        item.ResetChildPosition();

        OnReceive(item);
    }

    protected override void OnReceive(ItemManager item)
    {
        item.transform.gameObject.SetActive(false);

        VisitorGetItems[acceptTags.IndexOf(item.itemTag)] = true;

        for (int i = 0; i < VisitorGetItems.Length; i++)
        {
            if (!VisitorGetItems[i])
            {
                allTrue = false;
                break;
            }
            else
            {
                allTrue = true;
            }
        }

        if (allTrue)
        {
            GameManager.instance.CheckListEndCor();
        }
    }

    public void ResetCheckers()
    {
        allTrue = true;

        for (int i = 0; i < VisitorGetItems.Length; i++)
        {
            VisitorGetItems[i] = false;
        }
    }
}
