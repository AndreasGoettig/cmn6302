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

    public Quest currentQuest;
    public Quest LastQuest;

    private bool QuestInProgress = false;

    public WheatQuest wheatQuest;

    private int ProgressMin;

    public void Update()
    {
        CheckCurrentStatus();
        CheckCurrentQuestStatus();
    }

    public void CheckCurrentStatus()
    {
        if (currentQuest != LastQuest && QuestInProgress)
        {

        }
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
        ProgressMin++;
        progress.text = ProgressMin + "/" + currentQuest.questMax;
        CheckCurrentQuestStatus();
    }

    public void DeliverQuest()
    {

    }
    public void LoadQuestData(Quest data)
    {
        if (data != defaultQuest)
        {
            QuestInProgress = true;
        }
        else
        {
            QuestInProgress = false;
        }
        description.text = data.description;
        progress.text = data.questMin + "/" + data.questMax;
        ProgressMin = data.questMin;
        if (progress.text == "0/0")
        {
            doneCheck.gameObject.SetActive(false);
            progress.text = "";
        }
        LastQuest = currentQuest;
        currentQuest = data;
        if (currentQuest == wheat)
        {
            wheatQuest.InitQuest();
        }
    } 
}
