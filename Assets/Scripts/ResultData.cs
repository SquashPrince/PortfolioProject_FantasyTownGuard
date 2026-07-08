using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpTextL;
    public string textL;
    [SerializeField] private TextMeshProUGUI tmpTextR;
    public string textR;
    [SerializeField] private float charDelay = 0.03f;

    public GameObject nextDayBtn;

    public Action nextDialogAction;

    private Coroutine typingCoroutine;

    private string currentText;
    private bool isTyping;
    private bool isPlayNextAction = true;

    public void Init()
    {
        nextDayBtn.SetActive(false);

        tmpTextL.text = tmpTextR.text = "";
    }

    public void PlaySelf()
    {
        Play(tmpTextL, textL);
    }

    public void Play(TextMeshProUGUI targetText, string text)
    {
        currentText = text;
        targetText.text = "";

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeRoutine(targetText));
    }

    private IEnumerator TypeRoutine(TextMeshProUGUI targetText)
    {
        isTyping = true;

        targetText.text = currentText;
        targetText.maxVisibleCharacters = 0;
        targetText.ForceMeshUpdate();

        int totalChars = targetText.textInfo.characterCount;

        for (int i = 0; i <= totalChars; i++)
        {
            targetText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
        typingCoroutine = null;

        if(isPlayNextAction)
        {
            isPlayNextAction = false;
            Play(tmpTextR, textR);
        }
        else
        {
            nextDayBtn.SetActive(true);
        }
    }

    public void Btn_NextDay()
    {
        int lvUp = PlayerPrefs.GetInt("Lv") + 1;

        PlayerPrefs.SetInt("Lv", lvUp);
        PlayerPrefs.Save();

        StartCoroutine(LoadNextDay());
    }

    IEnumerator LoadNextDay()
    {
        yield return new WaitForEndOfFrame();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
