using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialActionType
{
    None,
    Click,
    PointerDown,
    PointerUp,
    PointerEnter,
    PonterExit
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public Transform tutoTextboxTr;

    [SerializeField] private GameObject DialogBg;

    private bool isWaitingAction;

    [Serializable]
    public class TutorialStep
    {
        public string stepId;

        [Header("Dialogue")]
        public string fileName;
        public string dialogueKey;
        public TextBoxType textBoxType;
        public float delayEndTime = 0.5f;
        public bool isCanSkip = true;

        [Header("Condition")]
        public TutorialActionType waitActionType;
        public GameObject targetObject;
        public bool waitForAction = true;

        [Header("Highlight")]
        public SpriteRenderer highlightTarget;
        public bool useHighlight = true;
    }

    [Header("Steps")]
    [SerializeField] public List<TutorialStep> steps = new();

    [Header("References")]
    [SerializeField] private TutorialRaycastFilter raycastFilter;
    [SerializeField] private DialogCsvLoader dialogueManager;

    public int currentStepIndex = -1;

    public TutorialStep currentStep;

    public bool IsTutorialActive { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void DialogBgSet(bool value)
    {
        DialogBg.SetActive(value);
    }

    public void RaycastFilterSet(bool value)
    {
        raycastFilter.gameObject.SetActive(value);
    }

    public void StartTutorial()
    {
        if (steps.Count == 0)
            return;

        IsTutorialActive = true;
        currentStepIndex = 0;

        PlayCurrentStep();
    }

    public void EndAllTutorial()
    {
        PlayerPrefs.SetInt("TutoClear", 1);
        PlayerPrefs.Save();

        int lvUp = PlayerPrefs.GetInt("Lv") + 1;

        PlayerPrefs.SetInt("Lv", lvUp);
        PlayerPrefs.Save();

        StartCoroutine(LoadTutoEnd());
    }
    System.Collections.IEnumerator LoadTutoEnd()
    {
        yield return new WaitForEndOfFrame();

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void OnTutorialDialogueEnd(string targetId)
    {
        DialogBg.SetActive(false);

        FindSetCurrentStep(targetId);

        if (currentStep.highlightTarget != null)
        {
            raycastFilter.gameObject.SetActive(true);
            raycastFilter.SetWorldTarget(currentStep.highlightTarget);
            raycastFilter.isDialogEnd = true;
        }

        isWaitingAction = true;
    }

    public void FindSetCurrentStep(string targetId)
    {
        currentStep = steps.Find(x => x.stepId == targetId);
        currentStepIndex = steps.FindIndex(x => x == currentStep);
    }

    private void PlayCurrentStep()
    {
        TutorialStep step = currentStep;

        if (step == null)
        {
            EndTutorial();
            return;
        }

        isWaitingAction = false;

        if (raycastFilter != null && step.highlightTarget != null)
        {
            raycastFilter.gameObject.SetActive(true);
            raycastFilter.SetWorldTarget(step.highlightTarget);
            raycastFilter.isDialogEnd = true;
        }

        dialogueManager.ShowDialogueSequence(step.fileName, step.dialogueKey, step.textBoxType, step.delayEndTime, step.isCanSkip, OnStepDialogueEnd);
    }

    private void OnStepDialogueEnd()
    {
        TutorialStep step = currentStep;

        if (step == null)
            return;

        if (step.waitForAction)
        {
            isWaitingAction = true;
        }
        else
        {
            CompleteCurrentStep();
        }
    }

    public void NotifyAction(TutorialActionType actionType, GameObject target, string stepId)
    {
        Debug.Log(1);

        currentStep = steps.Find(x => x.stepId == stepId);

        if (!isWaitingAction)
            return;

        if (currentStep.waitActionType != actionType)
            return;

        if (currentStep.targetObject != target)
            return;

        OnTutorialDialogueEnd(stepId);
    }

    private void CompleteCurrentStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= steps.Count)
        {
            EndTutorial();
            return;
        }

        PlayCurrentStep();
    }

    public void EndTutorial()
    {
        DialogBg.SetActive(false);

        IsTutorialActive = false;
        currentStepIndex = -1;

        if (raycastFilter != null)
            raycastFilter.gameObject.SetActive(false);

        Debug.Log("Tutorial End");
    }
}
