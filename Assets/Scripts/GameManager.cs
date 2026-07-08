using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class Infodata
{
    public infoType type;
    public string value;
}

[Serializable]
public class VisitorData
{
    public List<Infodata> infos = new List<Infodata>();
}
public enum infoType { Name, Job, Country, Reason }

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public LevelDatabase lvDatabase;
    public TextMeshProUGUI tmpDataText;

    public VisitorController visitorCtrl;

    public TrayController trayCtrl;

    [Serializable]
    public class VisitorAnnounceCheck
    {
        public infoType type;
        public bool correct;
        public bool isCheck;
    }

    public List<VisitorAnnounceCheck> collectAnswerCheck;
    public List<infoType> playerAnswerDialogList;

    public int getCoin;

    public bool isGetPassmark = false;
    public bool isQuestionToVisitor = false;
    public bool VisitorCheckComplete = false;

    public TextMeshProUGUI tmp_VisitorCount;
    public int currentVisitorCount;
    public int maxVisitorNum;

    public MemoCaseController memocaseCtrl;
    public GameObject cautionMemo;

    public ResultData resultData;

    public int completeScore = 0;
    public int failScore = 0;
    public int checkListScore = 0;
    public int correctCoinScore = 0;
    public int failCoinScore = 0;

    public int memo_VIsitorData = 0;
    public int memo_PassMark = 0;
    public int memo_VisitInfoCheck = 0;
    public int memo_IncorrectCoin = 0;

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
        ResetCheckList();
        LoadLevelData();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }  
    }
#endif

    public void LoadLevelData()
    {
        int currentLv = 0;

        if (PlayerPrefs.HasKey("Lv"))
        {
            currentLv = PlayerPrefs.GetInt("Lv");
            if (currentLv >= lvDatabase.levels.Count)
                currentLv = lvDatabase.levels.Count - 1;
        }
        else
        {
            PlayerPrefs.SetInt("Lv", 0);
            PlayerPrefs.Save();
        }

        tmpDataText.text = lvDatabase.levels[currentLv].lvName;

        maxVisitorNum = lvDatabase.levels[currentLv].maxVisitor;
        tmp_VisitorCount.text = $"{currentVisitorCount}/{maxVisitorNum}";
    }

    public void ResetCheckList()
    {
        getCoin = 0;

        isGetPassmark = false;
        isQuestionToVisitor = false;
        VisitorCheckComplete = false;

        VIsitorCounting(0);

        playerAnswerDialogList.Clear();

        foreach (var checkList in collectAnswerCheck)
        {
            checkList.isCheck = false;
        }
    }

    public bool VisitorCheck()
    {
        return maxVisitorNum == currentVisitorCount;
    }

    public void VIsitorCounting(int addNum)
    {
        currentVisitorCount += addNum;

        tmp_VisitorCount.text = $"{currentVisitorCount}/{maxVisitorNum}";
    }

    public int CalculateScore()
    {
        int result =
            (completeScore * 100)
            + (failScore * -50)
            + (checkListScore * 10)
            + (correctCoinScore)
            + (-Math.Abs(failCoinScore))
            + (-(memo_VIsitorData + memo_PassMark + memo_VisitInfoCheck + memo_IncorrectCoin) * 25);

        Debug.Log("최종 점수 : " + result.ToString());

        resultData.textL = $"완료한 방문자 수 : {maxVisitorNum}" +
            $"\n\n통과한 방문자 수 : {completeScore} x 100G" +
            $"\n\n돌아간 방문자 수 : {failScore} * -50G" +
            $"\n\n방문자 정보 확인 수 : {checkListScore} * 10G" +
            $"\n\n입수한 골드 수량 : {correctCoinScore}G" +
            $"\n\n초과/부족한 골드 수량 : {failCoinScore}G";

        resultData.textR = $"방문증 정보 확인 부족 : {memo_VIsitorData} 회" +
            $"\n\n인증 마크 받지 않음 : {memo_PassMark} 회" +
            $"\n\n제대로된 설명을 하지 않고 보냄 : {memo_VisitInfoCheck} 회" +
            $"\n\n정확한 가격을 받지 않음 : {memo_IncorrectCoin} 회" +
            $"\n\n총합 : -{(memo_VIsitorData + memo_PassMark + memo_VisitInfoCheck + memo_IncorrectCoin) * 25}G" +
            $"\n\n최종 급여 : {result}G";

        resultData.gameObject.SetActive(true);

        return result;
    }

    public void GetCoin()
    {
        getCoin += 100;

        if (!PlayerPrefs.HasKey("TutoClear"))
        {
            if (getCoin == 800)
            {
                DialogCsvLoader.instance.ShowDialogueSequence("TutorialDialog", "visitor_1", TextBoxType.Boss, 2.0f, true, () => TutorialManager.Instance.OnTutorialDialogueEnd("visitor"));
            }
        }
    }

    public void CollectAnswerSetter(infoType type, bool setValue)
    {
        var target = collectAnswerCheck.Find(x => x.type == type);

        if (target != null)
            target.correct = setValue;
    }

    public void AnswerChecker(infoType type)
    {
        var target = collectAnswerCheck.Find(x => x.type == type);

        target.isCheck = true;

        if (!target.correct)
        {
            CheckListAdd(target.type);
        }
    }

    public void CheckListAdd(infoType addType)
    {
        if (!playerAnswerDialogList.Contains(addType))
        {
            playerAnswerDialogList.Add(addType);
        }
    }
    public void CheckListEndCor()
    {
        StartCoroutine(CheckListEnd());
    }
    IEnumerator CheckListEnd()
    {
        for (int i = 0; i < playerAnswerDialogList.Count; i++)
        {
            DialogCsvLoader.instance.StartDialogue("Dialog1", $"Player_Answer_{playerAnswerDialogList[i]}", TextBoxType.Player, 1.0f, false);

            yield return new WaitForSeconds(2.0f);
        }

        if(playerAnswerDialogList.Count != 0)
        {
            DialogCsvLoader.instance.StartDialogue("Dialog1", $"Visitor_Understand", TextBoxType.Visitor, 1.0f, false);
        }
        else
        {
            DialogCsvLoader.instance.StartDialogue("Dialog1", $"Visitor_Thanks", TextBoxType.Visitor, 1.0f, false);
        }

        yield return new WaitForSeconds(1.0f);

        visitorCtrl.VisitorCheckEnd();

        yield return new WaitForSeconds(1.0f);

        CompleteAnswerCheck();
    }


    public void CompleteAnswerCheck()
    {
        bool finalCheck = true;
        int checkCount = 0;
        int totalCoin = 300;

        foreach (var check in collectAnswerCheck)
        {
            if (check.isCheck)
            {
                checkCount++;
            }

            if (!check.correct)
                finalCheck = false;
        }

        if (isGetPassmark)
        {
            if (finalCheck)
            {
                totalCoin += 500;

                completeScore++;
                checkListScore += checkCount;
            }
            else
            {
                InstanceMemo("방문증 정보가 다름");
                memo_VIsitorData++;
                failScore++;
            }
        }
        else
        {
            if (finalCheck)
            {
                InstanceMemo("인증 마크를 받지 않음");
                memo_PassMark++;
                failScore++;
            }
            else
            {
                if (isQuestionToVisitor)
                {
                    completeScore++;
                    checkListScore += checkCount;
                }
                else
                {
                    if (TutorialManager.Instance.currentStepIndex == 7)
                    {
                        DialogCsvLoader.instance.ShowDialogueSequence("TutorialDialog", "memocase_1", TextBoxType.Boss, 2.0f, true, () => TutorialManager.Instance.OnTutorialDialogueEnd("memocase"));
                    }

                    InstanceMemo("제대로된 설명을 하지 않음");
                    memo_VisitInfoCheck++;
                    completeScore++;
                }
            }
        }

        if (totalCoin == getCoin)
        {
            correctCoinScore += totalCoin;
        }
        else
        {
            failCoinScore += (getCoin - totalCoin);

            InstanceMemo("정확한 가격을 받지 않음");
            memo_IncorrectCoin++;
        }

        if (VisitorCheck())
        {
            CalculateScore();
        }
    }

    public void InstanceMemo(string text)
    {
        ItemMemoManager memo = Instantiate(cautionMemo, memocaseCtrl.transform).GetComponent<ItemMemoManager>();
        memo.transform.localPosition = Vector3.zero;
        memo.MemoTextSet(text);

        memocaseCtrl.MemoIsArrive();
    }
}
