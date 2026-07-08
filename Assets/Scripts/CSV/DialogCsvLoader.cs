using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum TextBoxType {Basic, Player, Visitor, Boss }

public class DialogCsvLoader : MonoBehaviour
{
    public static DialogCsvLoader instance;

    public Transform dialogTr;

    [Serializable]
    public class DialogueData
    {
        public string id;
        public string text;
        public string nextId;
        public TextBoxType nextBoxType;
    }
    [Serializable]
    public class TextBoxVariable
    {
        public TextBoxType type;
        public GameObject targetBoxPrefab;
    }
    public List<TextBoxVariable> textboxs;

    [SerializeField] private List<string> lines = new List<string>();
    private Dictionary<string, Dictionary<string, DialogueData>> dialogueMaps
        = new Dictionary<string, Dictionary<string, DialogueData>>(); private System.Action onDialogueEnd;

    [SerializeField] protected List<string> csvFileNames = new();
    [SerializeField] private string currentFileKey;
    protected string currentDialogueKey;

    private Dictionary<string, string> variables = new Dictionary<string, string>();

    public bool IsLoaded { get; private set; }

    protected virtual void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    protected virtual void Start()
    {
        StartCoroutine(LoadAllDialogs());
    }

    private IEnumerator LoadAllDialogs()
    {
        IsLoaded = false;

        foreach (string fileName in csvFileNames)
        {
            if (string.IsNullOrEmpty(fileName))
                continue;

            yield return StartCoroutine(LoadDialogueCsv(fileName));
        }

        IsLoaded = true;
    }

    public void LoadDialog(string fileName)
    {
        StartCoroutine(LoadDialogueCsv(fileName));
    }

    public IEnumerator LoadDialogueCsv(string fileName)
    {
        string path = $"{Application.streamingAssetsPath}/Dialog/{fileName}.csv";

        using UnityWebRequest request = UnityWebRequest.Get(path);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Dialogue CSV Load Failed: {request.error}");
            yield break;
        }

        ParseCsv(fileName, request.downloadHandler.text);
    }

    public void SetVariable(string key, string value)
    {
        variables[key] = value;
    }

    public void RemoveVariable(string key)
    {
        if (variables.ContainsKey(key))
            variables.Remove(key);
    }

    public void ClearVariables()
    {
        variables.Clear();
    }

    private void ParseCsv(string fileName, string csvText)
    {
        List<List<string>> rows = ParseCsvRows(csvText);

        Dictionary<string, DialogueData> map = new Dictionary<string, DialogueData>();

        for (int i = 1; i < rows.Count; i++)
        {
            if (rows[i].Count < 2)
                continue;

            string key = rows[i][0].Trim();
            string text = rows[i][1].TrimStart();
            string nextId = rows[i].Count >= 3 ? rows[i][2].Trim() : string.Empty;
            string nextBoxTypeText = rows[i].Count >= 4 ? rows[i][3].Trim() : string.Empty;

            TextBoxType nextBoxType = TextBoxType.Basic;

            if (!string.IsNullOrEmpty(nextBoxTypeText))
            {
                if (!Enum.TryParse(nextBoxTypeText, true, out nextBoxType))
                    nextBoxType = TextBoxType.Basic;
            }

            if (string.IsNullOrEmpty(key))
                continue;

            DialogueData data = new DialogueData
            {
                id = key,
                text = text,
                nextId = nextId,
                nextBoxType = nextBoxType
            };

            if (!map.ContainsKey(key))
                map.Add(key, data);
            else
                Debug.LogWarning($"Duplicate Dialogue Key: {key} in {fileName}");
        }

        dialogueMaps[fileName] = map;
    }

    public List<string> GetAllDialogues()
    {
        return lines;
    }

    private List<List<string>> ParseCsvRows(string csvText)
    {
        List<List<string>> rows = new List<List<string>>();
        List<string> currentRow = new List<string>();
        System.Text.StringBuilder currentValue = new System.Text.StringBuilder();

        bool insideQuote = false;

        for (int i = 0; i < csvText.Length; i++)
        {
            char c = csvText[i];

            if (c == '"')
            {
                if (insideQuote &&
                    i + 1 < csvText.Length &&
                    csvText[i + 1] == '"')
                {
                    currentValue.Append('"');
                    i++;
                }
                else
                {
                    insideQuote = !insideQuote;
                }

                continue;
            }

            if (c == ',' && !insideQuote)
            {
                currentRow.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else if ((c == '\n' || c == '\r') && !insideQuote)
            {
                if (c == '\r' &&
                    i + 1 < csvText.Length &&
                    csvText[i + 1] == '\n')
                {
                    i++;
                }

                if (currentValue.Length > 0 || currentRow.Count > 0)
                {
                    currentRow.Add(currentValue.ToString());
                    currentValue.Clear();

                    rows.Add(currentRow);
                    currentRow = new List<string>();
                }
            }
            else
            {
                currentValue.Append(c);
            }
        }

        if (currentValue.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentValue.ToString());
            rows.Add(currentRow);
        }

        return rows;
    }

    public virtual void StartDialogue(string fileName, string dialogueKey, TextBoxType textBoxType, float delayEndTime, bool isCanSkip, Action onEnd = null)
    {
        currentFileKey = fileName;
        currentDialogueKey = dialogueKey;
        onDialogueEnd = onEnd;

        ShowDialogue(currentFileKey, currentDialogueKey, textBoxType, delayEndTime, isCanSkip);
    }

    public void SetDialogueEndAction(System.Action action)
    {
        onDialogueEnd = action;
    }

    public virtual void ShowDialogue(string fileName, string dialogueKey, TextBoxType textBoxType, float delayEndTime, bool isCanSkip)
    {
        if (!dialogueMaps.TryGetValue(fileName, out var map))
        {
            Debug.LogWarning($"Dialogue File Not Loaded: {fileName}");
            EndDialogue();
            return;
        }

        if (!map.TryGetValue(dialogueKey, out DialogueData data))
        {
            Debug.LogWarning($"Dialogue Key Not Found: {dialogueKey} in {fileName}");
            EndDialogue();
            return;
        }

        bool isBgSet = textBoxType == TextBoxType.Boss;

        GameObject prefab =
            textboxs.Find(x => x.type == textBoxType)?.targetBoxPrefab;


        if (prefab == null)
        {
            Debug.LogWarning($"TextBox Prefab Not Found: {textBoxType}");
            EndDialogue();
            return;
        }

        Transform targetTr;

        TextBoxReader targetDialog = new TextBoxReader();

        if (isBgSet)
        {
            targetTr = TutorialManager.Instance.tutoTextboxTr;
            targetDialog = Instantiate(prefab, targetTr).GetComponent<TextBoxReader>();
            targetDialog.TutoMng = TutorialManager.Instance;
        }
        else
        {
            targetTr = dialogTr;
            targetDialog = Instantiate(prefab, targetTr).GetComponent<TextBoxReader>();
        }

        targetDialog.nextDialogAction = () =>
        {
            HandleNextAction(fileName, data.nextId, delayEndTime, isCanSkip);
        };

        string text = FormatText(data.text);

        targetDialog.TmpTextSet(text, delayEndTime, isCanSkip);
    }

    protected virtual void HandleNextAction(string fileName, string nextId, float delayEndTime, bool isCanSkip)
    {
        if (string.IsNullOrEmpty(nextId) || nextId == "#END")
        {
            EndDialogue();
            return;
        }

        if (!dialogueMaps.TryGetValue(fileName, out var map))
        {
            EndDialogue();
            return;
        }

        if (!map.TryGetValue(nextId, out DialogueData nextData))
        {
            Debug.LogWarning($"Next Dialogue Key Not Found: {nextId} in {fileName}");
            EndDialogue();
            return;
        }

        currentDialogueKey = nextId;

        ShowDialogue(
            fileName,
            nextId,
            nextData.nextBoxType,
            delayEndTime,
            isCanSkip
        );
    }

    public void ShowDialogueSequence(string fileName, string dialogueKey, TextBoxType textBoxType, float delayEndTime, bool isCanSkip, System.Action onSequenceEnd)
    {
        onDialogueEnd = onSequenceEnd;

        ShowDialogue(fileName, dialogueKey, textBoxType, delayEndTime, isCanSkip);
    }

    protected virtual void ExecuteDialogueAction(string actionName) { }

    protected string FormatText(string text)
    {
        foreach (var pair in variables)
        {
            text = text.Replace(
                $"{{{pair.Key}}}",
                pair.Value
            );
        }

        return text;
    }

    public void EndDialogue()
    {
        currentDialogueKey = string.Empty;

        onDialogueEnd?.Invoke();
        onDialogueEnd = null;
    }
}