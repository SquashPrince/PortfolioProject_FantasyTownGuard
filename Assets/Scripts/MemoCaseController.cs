using System.Collections;
using UnityEngine;

public class MemoCaseController : MonoBehaviour
{
    public Animator caseAnim;
    public Collider2D caseCol;

    void Start()
    {
        caseAnim.Play("Close", -1, 1);
    }

    public void MemoIsArrive()
    {
        caseCol.enabled = false;
        caseAnim.SetBool("isOpen", true);
    }

    public void MemoCaseBtn()
    {
        if (TutorialManager.Instance.currentStepIndex == 8)
        {
            DialogCsvLoader.instance.ShowDialogueSequence("TutorialDialog", "book_close_1", TextBoxType.Boss, 2.0f, false, () => TutorialManager.Instance.OnTutorialDialogueEnd("book_close"));
        }

        caseCol.enabled = false;
        caseAnim.SetBool("isOpen", false);
    }

    public void MemoCaseColliderControll()
    {
        caseCol.enabled = true;
    }
}
