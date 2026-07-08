using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBoxReader : MonoBehaviour
{
    public TutorialManager TutoMng;

    [SerializeField] private TextMeshProUGUI tmpText;
    [SerializeField] private float charDelay = 0.03f;

    public Action nextDialogAction;

    private Coroutine typingCoroutine;
    private Coroutine destroyCoroutine;

    private string currentText;
    private bool isCanSkip;
    private bool isTyping;
    private bool isClosed;

    public void TmpTextSet(string text, float delayEndTime, bool _isCanSkip)
    {
        isCanSkip = _isCanSkip;
        tmpText.text = text;
        Play(text, delayEndTime);
    }

    public void Play(string message, float delayEndTime)
    {
        currentText = message;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeRoutine(delayEndTime));
    }

    private IEnumerator TypeRoutine(float delayEndTime)
    {
        if(TutoMng != null)
        {
            TutoMng.DialogBgSet(true);
        }

        isTyping = true;

        tmpText.text = currentText;
        tmpText.maxVisibleCharacters = 0;
        tmpText.ForceMeshUpdate();

        int totalChars = tmpText.textInfo.characterCount;

        for (int i = 0; i <= totalChars; i++)
        {
            tmpText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
        typingCoroutine = null;

        destroyCoroutine = StartCoroutine(DelayClose(delayEndTime));
    }

    private IEnumerator DelayClose(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Close();
    }

    public void Skip()
    {
        if (isClosed)
            return;

        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            tmpText.maxVisibleCharacters = tmpText.textInfo.characterCount;

            isTyping = false;
            typingCoroutine = null;

            destroyCoroutine = StartCoroutine(DelayClose(2.0f));
            return;
        }

        Close();
    }

    private void Close()
    {
        if (isClosed)
            return;

        isClosed = true;

        if (destroyCoroutine != null)
            StopCoroutine(destroyCoroutine);

        if (nextDialogAction != null)
            nextDialogAction.Invoke();
        else 
            DialogCsvLoader.instance.EndDialogue();

        Destroy(gameObject);
    }

    public void OnClick()
    {
        if (isCanSkip)
        {
            Skip();
        }
    }

    public void SkipableSet(bool value)
    {
        isCanSkip = value;
    }
}
