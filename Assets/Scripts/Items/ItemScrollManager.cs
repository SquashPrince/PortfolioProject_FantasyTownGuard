using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemScrollManager : ItemManager
{
    [SerializeField] protected LayerMask dropableLayer;

    [Header("Scroll Option")]
    public EventTrigger tmp_Name;
    public EventTrigger tmp_Job;
    public EventTrigger tmp_Country;
    public EventTrigger tmp_Reason;

    public List<TextMeshPro> tmp_Texts;

    private ItemReceiver lastReceiver;
    public GameObject passmark;

    protected override void Start()
    {
        base.Start();

        AddClickEvent(tmp_Name, infoType.Name, tmp_Name.GetComponent<TextMeshPro>());
        AddClickEvent(tmp_Job, infoType.Job, tmp_Job.GetComponent<TextMeshPro>());
        AddClickEvent(tmp_Country, infoType.Country, tmp_Country.GetComponent<TextMeshPro>());
        AddClickEvent(tmp_Reason, infoType.Reason, tmp_Reason.GetComponent<TextMeshPro>());

        icon_Open.gameObject.SetActive(false);
        icon_Close.gameObject.SetActive(true);

        gameObject.SetActive(false);
    }

    private void AddClickEvent(EventTrigger trigger,infoType type, TextMeshPro tmpText)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;

        entry.callback.AddListener((data) =>
        {
            ScrollInfoInteract(type, tmpText, (PointerEventData)data);
        });

        trigger.triggers.Add(entry);
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

            GameManager.instance.trayCtrl.ResetTray();
            lastReceiver = null;

            return;
        }

        lastReceiver = hit.GetComponent<ItemReceiver>();

        if (lastReceiver is VisitorReceiver visitor)
        {
            GameManager.instance.trayCtrl.ResetTray();
            passmark.SetActive(false);
        }

        if (lastReceiver == null)
            return;

        lastReceiver.ScrollInTraySlot(this);
    }

    public void ScrollInfoInteract(infoType type, TextMeshPro tmpText, PointerEventData data)
    {
        if (RaycastManager.instance.blockClickThisFrame)
            return;

        GameManager.instance.visitorCtrl.VisitorDataCheck(type, tmpText.text);
        GameManager.instance.isQuestionToVisitor = true;

        if (!PlayerPrefs.HasKey("TutoClear"))
        {
            if (TutorialManager.Instance.currentStepIndex == 2)
            {
                DialogCsvLoader.instance.ShowDialogueSequence("TutorialDialog", "tray_1", TextBoxType.Boss, 2.0f, true, () => TutorialManager.Instance.OnTutorialDialogueEnd("tray"));
            }
            return;
        }

        switch (type)
        {
            case infoType.Name:
                GameManager.instance.AnswerChecker(type);
                DialogCsvLoader.instance.SetVariable(tmp_Name.name, GameManager.instance.visitorCtrl.visitorData.infos.Find(x => x.type == type)?.value);
                DialogCsvLoader.instance.StartDialogue("Dialog1", "Player_Question_Name", TextBoxType.Player, 2.0f, true);
                break;

            case infoType.Job:
                GameManager.instance.AnswerChecker(type);
                DialogCsvLoader.instance.SetVariable(tmp_Job.name, GameManager.instance.visitorCtrl.visitorData.infos.Find(x => x.type == type)?.value);
                DialogCsvLoader.instance.StartDialogue("Dialog1", "Player_Question_Job", TextBoxType.Player, 2.0f, true);
                break;

            case infoType.Country:
                GameManager.instance.AnswerChecker(type);
                DialogCsvLoader.instance.SetVariable(tmp_Country.name, GameManager.instance.visitorCtrl.visitorData.infos.Find(x => x.type == type)?.value);
                DialogCsvLoader.instance.StartDialogue("Dialog1", "Player_Question_Country", TextBoxType.Player, 2.0f, true);
                break;

            case infoType.Reason:
                GameManager.instance.AnswerChecker(type);
                DialogCsvLoader.instance.SetVariable(tmp_Reason.name, GameManager.instance.visitorCtrl.visitorData.infos.Find(x => x.type == type)?.value);
                DialogCsvLoader.instance.StartDialogue("Dialog1", "Player_Question_Reason", TextBoxType.Player, 2.0f, true);
                break;
        }


    }

    public void ScrollInfoSetter(infoType type, string value)
    {
        TextMeshPro targetTmp = tmp_Texts.Find(x => x.name == type.ToString());

        targetTmp.text = value;
    }
}
