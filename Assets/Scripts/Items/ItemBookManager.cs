using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class ItemBookManager : ItemManager
{
    [Header("Book Option")]
    public EventTrigger Btn_PrevPage;
    public EventTrigger Btn_NextPage;

    [SerializeField] private string csvFileName;
    [SerializeField] private List<string> tutorialPagss = new List<string>();

    public TextMeshPro pageL;
    public TextMeshPro pageR;

    private int currentPagenum = 0;
    [SerializeField] private int maxPagenum = 2;

    protected override void OnDisable()
    {
        base.OnDisable();

        currentPagenum = 0;

        PageSet(currentPagenum);
    }

    protected override void Start()
    {
        base.Start();

        AddClickEvent(Btn_NextPage, 1);
        AddClickEvent(Btn_PrevPage, -1);

        icon_Open.gameObject.SetActive(false);
        icon_Close.gameObject.SetActive(true);

        StartCoroutine(LoadCsv());

    }

    private IEnumerator LoadCsv()
    {
        string path = $"{Application.streamingAssetsPath}/{csvFileName}";

        using UnityWebRequest request = UnityWebRequest.Get(path);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"CSV Load Failed: {request.error}");
            yield break;
        }

        ParseCsv(request.downloadHandler.text);
    }

    private void ParseCsv(string csvText)
    {
        string[] lines = csvText.Split(
            new[] { '\r'},
            StringSplitOptions.RemoveEmptyEntries
        );

        for (int i = 1; i < lines.Length; i++)
        {
            List<string> values = SplitCsvLine(lines[i]);

            tutorialPagss.AddRange(values);
        }

        PageSet(currentPagenum);
    }

    private List<string> SplitCsvLine(string line)
    {
        List<string> result = new List<string>();

        bool insideQuote = false;

        System.Text.StringBuilder current =
            new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')
            {
                insideQuote = !insideQuote;
                continue;
            }

            if (c == ',' && !insideQuote)
            {
                result.Add(current.ToString().TrimStart());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString().TrimStart());

        return result;
    }

    private void AddClickEvent(EventTrigger trigger, int addPageNum)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;

        entry.callback.AddListener((data) =>
        {
            MovePage(addPageNum, (PointerEventData)data);
        });

        trigger.triggers.Add(entry);
    }

    public void MovePage(int addPageNum ,PointerEventData data)
    {
        if (RaycastManager.instance.blockClickThisFrame)
            return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(data.position);
        mousePos.z = transform.position.z;

        currentPagenum += addPageNum;

        if (currentPagenum <= 0)
        {
            currentPagenum = 0;
            Btn_PrevPage.gameObject.SetActive(false);
        }
        else if(currentPagenum >= maxPagenum)
        {
            currentPagenum = maxPagenum;
            Btn_NextPage.gameObject.SetActive(false);
        }
        else
        {
            Btn_PrevPage.gameObject.SetActive(true);
            Btn_NextPage.gameObject.SetActive(true);
        }

        PageSet(currentPagenum);
    }

    private void PageSet(int targetPageNum)
    {
        pageL.text = tutorialPagss[targetPageNum * 2];
        pageR.text = tutorialPagss[targetPageNum * 2 + 1];
    }
}
