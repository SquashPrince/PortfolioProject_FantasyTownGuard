using TMPro;
using UnityEngine;

public class ItemMemoManager : ItemManager
{
    [SerializeField] protected LayerMask dropableLayer;

    [Header("Memo Option")]
    public TextMeshPro memoText;

    protected override void Start()
    {
        base.Start();

        icon_Close.gameObject.SetActive(true);
        icon_Open.gameObject.SetActive(false);
    }

    public void MemoTextSet(string text)
    {
        memoText.text = text;
    }

    public override void EndDrag()
    {
        base.EndDrag();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Collider2D hit = Physics2D.OverlapPoint(mousePos, dropableLayer);

        if (hit == null)
        {
            transform.SetParent(null);

            return;
        }
    }
}
