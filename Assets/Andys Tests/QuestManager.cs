using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public TMP_Text description;
    public TMP_Text progress;
    public Image doneCheck;

    public Quest defaultQuest;
    public Quest wheat;
    public Quest battle;
    public Quest deliver;
    public Quest goTo;

    private Quest currentQuest;
    private bool QuestInProgress = false;

    public void Update()
    {
        CheckCurrentStatus();
        CheckCurrentQuestStatus();
    }

    public void CheckCurrentStatus()
    {
        if (!QuestInProgress)
        {
            LoadQuestData(defaultQuest);
        }
    }
    public void CheckCurrentQuestStatus()
    {
        if (progress.text == currentQuest.questMax+"/"+currentQuest.questMax && progress.text != "0/0")
        {
            //Quest done
        }
    }
    public void UpdateQuestStatus()
    {

    }

    public void LoadQuestData(Quest data)
    {
        description.text = data.description;
        progress.text = data.questMin + "/" + data.questMax;
        if (progress.text=="0/0")
        {
            doneCheck.gameObject.SetActive(false);
            progress.text = "";
        }
        currentQuest = defaultQuest;
    }
}
