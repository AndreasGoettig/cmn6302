using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public enum Quests
{
    wheat,
    deliver,

}
public class NpcInteraction : MonoBehaviour
{
    public WheatQuest wheatQuest;
    public bool HasQuest;
    public bool QuestInProgress;
    public bool CandeliverQuest;

    public TMP_Text buttonText;
    public void Awake()
    {
    }
    public void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            buttonText.gameObject.SetActive(true);
            print("enter NPC");

        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                buttonText.gameObject.SetActive(false);
                GetQuest();
                print("accept Quest");
            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            buttonText.gameObject.SetActive(false);
            print("exit NPC");
        }
    }

    public void GetQuest()
    {
        //get quest, update npc status, track quest progress
        wheatQuest.InitQuest();
    }
}
