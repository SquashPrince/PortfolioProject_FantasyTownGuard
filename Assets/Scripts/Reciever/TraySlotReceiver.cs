using UnityEngine;

public class TraySlotReceiver : ItemReceiver
{
    public TrayController trayCtrl;

    protected override void OnReceive(ItemManager item)
    {
        trayCtrl.scroll = item as ItemScrollManager;
    }
}
