using TMPro;
using UnityEngine;

public class AnimManager : MonoBehaviour
{
    TextMeshPro tmpText;

    public void AnimEnd()
    {
        Destroy(gameObject);
    }

    public void AnimEndSetOff()
    {
        gameObject.SetActive(false);
    }

    public void TutorialClearCheck()
    {
        if (!PlayerPrefs.HasKey("TutoClear"))
        {
            DialogCsvLoader.instance.ShowDialogueSequence("TutorialDialog", "bell_1", TextBoxType.Boss, 2.0f, true, () => TutorialManager.Instance.OnTutorialDialogueEnd("bell"));
        }
    }
}
