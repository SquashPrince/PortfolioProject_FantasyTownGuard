using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VisitorDataCsvLoader : MonoBehaviour
{
    public static VisitorDataCsvLoader instance = null;

    [SerializeField] private string csvFileName;

    public ItemScrollManager scroll;

    public List<VisitorData> visitors { get; private set; } = new();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
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

        visitors = ParseCsv(request.downloadHandler.text);
    }

    private List<VisitorData> ParseCsv(string csvText)
    {
        List<VisitorData> result = new();

        string[] lines = csvText.Split(
            new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries
        );

        if (lines.Length <= 1)
            return result;

        List<string> headers = SplitCsvLine(lines[0]);

        List<infoType?> types = new();

        foreach (string header in headers)
        {
            if (Enum.TryParse(header.Trim(), true, out infoType parsedType))
            {
                types.Add(parsedType);
            }
            else
            {
                Debug.LogWarning($"Unknown CSV Header: {header}");
                types.Add(null);
            }
        }

        for (int i = 1; i < lines.Length; i++)
        {
            List<string> values = SplitCsvLine(lines[i]);

            VisitorData visitor = new VisitorData();

            for (int j = 0; j < Mathf.Min(values.Count, types.Count); j++)
            {
                if (types[j] == null)
                    continue;

                visitor.infos.Add(new Infodata
                {
                    type = types[j].Value,
                    value = values[j].Trim()
                });
            }

            result.Add(visitor);
        }

        return result;
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
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());

        return result;
    }

    public VisitorData GetVisitorData()
    {
        if (visitors == null || visitors.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, visitors.Count);

        VisitorData original = visitors[index];

        VisitorData result = new VisitorData();
        result.infos = new List<Infodata>();

        foreach (Infodata info in original.infos)
        {
            string finalValue = info.value;

            string init = finalValue;

            GameManager.instance.CollectAnswerSetter(info.type, true);

            float changePercent = 0.1f;

            if(!PlayerPrefs.HasKey("TutoClear"))
            {
                if (TutorialManager.Instance.currentStepIndex != 6)
                {
                    changePercent = 0.0f;
                }
                else
                {
                    changePercent = 1.0f;
                }
            }

            if (UnityEngine.Random.value <= changePercent)
            {
                GameManager.instance.CollectAnswerSetter(info.type, false);

                List<string> candidates = new List<string>();

                foreach (VisitorData otherVisitor in visitors)
                {
                    foreach (Infodata otherInfo in otherVisitor.infos)
                    {
                        if (otherInfo.type == info.type &&
                            !string.IsNullOrEmpty(otherInfo.value) &&
                            otherInfo.value != info.value)
                        {
                            candidates.Add(otherInfo.value);
                        }
                    }
                }

                if (candidates.Count > 0)
                {
                    int rand =
                        UnityEngine.Random.Range(0, candidates.Count);

                    finalValue = candidates[rand];
                }
            }

            if (info.value != finalValue)
            {
                Debug.Log("원본값 : " + info.value + " / 바뀐값 : " + finalValue);
            }

            result.infos.Add(new Infodata
            {
                type = info.type,
                value = info.value
            });

            scroll.ScrollInfoSetter(info.type, finalValue);
        }

        return result;
    }

    public string GetValue(VisitorData visitor, infoType type)
    {
        foreach (Infodata info in visitor.infos)
        {
            if (info.type == type)
                return info.value;
        }

        return string.Empty;
    }
}