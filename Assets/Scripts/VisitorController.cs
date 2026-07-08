using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class VisitorController : MonoBehaviour
{
    public Animator visitorAnim;

    public SpriteRenderer sp;
    public Sprite[] charSp;

    public Collider2D bell;

    public VisitorData visitorData;
    public VisitorReceiver visitRcv;

    public Transform handTr;

    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.05f;

    [SerializeField] private float normalGlow = 0f;
    [SerializeField] private float hoverGlow = 2f;

    [SerializeField] private float speed = 8f;

    private void Start()
    {
        visitorAnim.Play("Out", -1, 1);

        bell.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(1, 1, 0, 0));

        bell.GetComponent<SpriteRenderer>().material.SetFloat("_Scale", normalScale);
        bell.GetComponent<SpriteRenderer>().material.SetFloat("_GlowPower", normalGlow);
    }

    public void BellActiveSetOn()
    {
        bell.enabled = true;

        visitRcv.ResetCheckers();
    }

    public void VisitCheckEndReset()
    {
        GameManager.instance.VisitorCheckComplete = true;
    }

    public void CallVIsitorToIn()
    {
        if(!GameManager.instance.VisitorCheck())
        {
            if (GameManager.instance.VisitorCheckComplete)
            {
                GameManager.instance.ResetCheckList();

                bell.enabled = false;

                visitorAnim.Play("In");
                visitorAnim.SetBool("isVisit", true);

                int rand = UnityEngine.Random.Range(0, charSp.Length);

                sp.sprite = charSp[rand];

                visitorData = VisitorDataCsvLoader.instance.GetVisitorData();

                GameManager.instance.VIsitorCounting(1);
            }
            else
            {
                Debug.Log("방문자 체크리스트 미완료");
            }
        }
        else
        {
            Debug.Log("방문자 완료");
        }
    }

    public void OnPonterEnter()
    {
        bell.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(1, 1, 0.25f, 1));

        bell.GetComponent<SpriteRenderer>().material.SetFloat("_Scale", hoverScale);
        bell.GetComponent<SpriteRenderer>().material.SetFloat("_GlowPower", hoverGlow);
    }

    public void OnPonterExit()
    {
        bell.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(1, 1, 0, 0));

        bell.GetComponent<SpriteRenderer>().material.SetFloat("_Scale", normalScale);
        bell.GetComponent<SpriteRenderer>().material.SetFloat("_GlowPower", normalGlow);
    }

    public void VisitorCheckEnd()
    {
        bell.enabled = false;

        visitorAnim.SetBool("isVisit", false);
    }

    public void VisitorStart()
    {
        StartCoroutine(VisitorDialogStart());
    }

    IEnumerator VisitorDialogStart()
    {
        DialogCsvLoader.instance.StartDialogue("Dialog1", $"Visitor_Hello", TextBoxType.Visitor, 2.0f, false);

        yield return new WaitForSeconds(3.0f);

        VisitorHandOff();
    }

    public void VisitorHandOff()
    {
        for (int i = handTr.childCount - 1; i >= 0; i--)
        {
            Transform item = handTr.GetChild(i);

            item.gameObject.SetActive(true);
            item.SetParent(null);

            Vector3 pos = item.localPosition;
            pos.z = item.GetComponent<ItemManager>().zPos;
            item.localPosition = pos;

            item.GetComponentInChildren<Rigidbody2D>().AddForce(new Vector2(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(2, 4)), ForceMode2D.Impulse);
        }

        if (!PlayerPrefs.HasKey("TutoClear"))
        {
            if (TutorialManager.Instance.currentStepIndex == 0)
            {
                DialogCsvLoader.instance.ShowDialogueSequence("TutorialDialog", "scroll_close_1", TextBoxType.Boss, 2.0f, true, () => TutorialManager.Instance.OnTutorialDialogueEnd("scroll_close"));
            }
            else if (TutorialManager.Instance.currentStepIndex == 6)
            {
                DialogCsvLoader.instance.ShowDialogueSequence("TutorialDialog", "visitor_wrong_1", TextBoxType.Boss, 2.0f, true, () => TutorialManager.Instance.OnTutorialDialogueEnd("visitor_wrong"));
            }
        }
    }

    public void VisitorDataCheck(infoType type, string value)
    {
        Infodata data = visitorData.infos.Find(x => x.type == type);

        if (data == null)
            return;

        // ',' 기준 분리 + 공백 제거
        string[] valueWords = value
            .Split(',')
            .Select(x => x.Trim())
            .ToArray();

        string[] dataWords = data.value
            .Split(',')
            .Select(x => x.Trim())
            .ToArray();

        // 순서 포함 완전 일치 비교
        /*bool isMatch =
            valueWords.OrderBy(x => x)
            .SequenceEqual(dataWords.OrderBy(x => x));

        if (isMatch)
        {
            Debug.Log("o");
        }
        else
        {
            Debug.Log("x");
        }*/
    }
}
