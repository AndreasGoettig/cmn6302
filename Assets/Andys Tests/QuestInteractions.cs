using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestInteractions : MonoBehaviour
{
    public TMP_Text buttonText;
    public WheatQuest quest;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            buttonText.gameObject.SetActive(true);
            print("enter Quest");
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Collect();
                print("collected");
            }

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            buttonText.gameObject.SetActive(false);
            print("exit Quest");
        }
    }

    public void Collect()
    {
        this.gameObject.SetActive(false);
        quest.OnCollect();
    }
}
