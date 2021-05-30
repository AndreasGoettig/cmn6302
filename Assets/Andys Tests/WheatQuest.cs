using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheatQuest : MonoBehaviour
{
    public GameObject Wheat1;
    public GameObject Wheat2;
    public GameObject Wheat3;
    public Quest quest;
    public QuestManager manager;

    public void Start()
    {
        Wheat1.SetActive(false);
        Wheat2.SetActive(false);
        Wheat3.SetActive(false);
    }
    public void Update()
    {
    }

    public void InitQuest()
    {
        Wheat1.SetActive(true);
        Wheat2.SetActive(true);
        Wheat3.SetActive(true);
        manager.LoadQuestData(quest);
    }
    public void OnCollect()
    {
        manager.UpdateQuestStatus();
    }

}

