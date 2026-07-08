using System.Collections;
using UnityEngine;

public class TrayController : MonoBehaviour
{
    public Animator trayAnim;
    public Collider2D trayCol;

    public GameObject completeMark;
    public ItemScrollManager scroll;

    void Start()
    {
        trayAnim.Play("Close", -1, 1);
    }

    public void TrayBtn()
    {
        if (!PlayerPrefs.HasKey("TutoClear"))
        {
            if (TutorialManager.Instance.currentStepIndex == 3)
            {
                TutorialManager.Instance.RaycastFilterSet(false);

                if (GameManager.instance.isGetPassmark)
                {
                    DialogCsvLoader.instance.ShowDialogueSequence("TutorialDialog", "money_close_1", TextBoxType.Boss, 2.0f, true, () => TutorialManager.Instance.OnTutorialDialogueEnd("money_close"));
                }
            }
        }

        trayCol.enabled = false;
        trayAnim.SetBool("isOpen", !trayAnim.GetBool("isOpen"));

        if(completeMark.activeSelf)
            completeMark.SetActive(false);
    }

    public void TrayColliderControll()
    {
        if (!trayAnim.GetBool("isOpen") && scroll != null && !scroll.passmark.activeSelf)
        {
            StartCoroutine(ScrollCertification());
        }
        else
        {
            trayCol.enabled = true;
        }
    }

    IEnumerator ScrollCertification()
    {
        yield return new WaitForSeconds(3.0f);

        scroll.passmark.SetActive(true);
        GameManager.instance.isGetPassmark = true;
        completeMark.SetActive(true);

        trayCol.enabled = true;
    }

    public void ResetTray()
    {
        scroll = null;
    }
}
