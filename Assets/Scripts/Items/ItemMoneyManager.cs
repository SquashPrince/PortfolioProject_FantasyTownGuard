using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemMoneyManager : ItemManager
{
    [SerializeField] protected LayerMask dropableLayer;


    [Header("Money Option")]
    public EventTrigger getCoinEvt;

    public Animator coinTextAnim;
    public GameObject coin;

    protected override void Start()
    {
        base.Start();

        AddClickEvent(getCoinEvt);

        icon_Open.gameObject.SetActive(false);
        icon_Close.gameObject.SetActive(true);

        gameObject.SetActive(false);
    }

    private void AddClickEvent(EventTrigger trigger)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;

        entry.callback.AddListener((data) =>
        {
            GetCoin((PointerEventData)data);
        });

        trigger.triggers.Add(entry);
    }

    public void GetCoin(PointerEventData data)
    {
        if (RaycastManager.instance.blockClickThisFrame)
            return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(data.position);
        mousePos.z = transform.position.z;

        Instantiate(coin, mousePos, Quaternion.identity);

        if (!PlayerPrefs.HasKey("TutoClear"))
        {
            if (GameManager.instance.getCoin < 800)
            {
                GameManager.instance.GetCoin();
            }
            else if (GameManager.instance.getCoin == 800)
            {
                if(TutorialManager.Instance.currentStepIndex == 5)
                {
                    DialogCsvLoader.instance.ShowDialogueSequence("TutorialDialog", "visitor_1", TextBoxType.Boss, 2.0f, true, () => TutorialManager.Instance.OnTutorialDialogueEnd("visitor"));
                }
            }
            else if (GameManager.instance.getCoin > 800)
            {
                GameManager.instance.getCoin = 800;
            }
        }
        else
        {
            GameManager.instance.GetCoin();
        }

        coinTextAnim.gameObject.SetActive(true);
        coinTextAnim.GetComponent<TextMeshPro>().text = $"+{GameManager.instance.getCoin}G";
        coinTextAnim.Play("GetCoin", -1, 0);
    }

    public override void EndDrag()
    {
        base.EndDrag();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Collider2D hit = Physics2D.OverlapPoint(mousePos, dropableLayer);

        if (!icon_Open.gameObject.activeInHierarchy)
        {
            coinTextAnim.gameObject.SetActive(false);
        }

        if (hit == null)
        {
            transform.SetParent(null);
            return;
        }

        ItemReceiver receiver = hit.GetComponent<ItemReceiver>();

        if (receiver == null)
            receiver = hit.GetComponentInParent<ItemReceiver>();

        if (receiver == null)
            return;

        receiver.ScrollInTraySlot(this);
    }
}
