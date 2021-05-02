using Obi;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColliderDistanceField : MonoBehaviour
{

    public List<GameObject> colliders;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tree"))
        {
            print("entered tree");
            Transform child = other.transform.GetChild(0);
            print(child.name);
            child.gameObject.SetActive(true);
            colliders.Add(child.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tree"))
        {
            print("left tree");
            Transform child = other.transform.GetChild(0);
            child.gameObject.SetActive(false);
            colliders.Remove(child.gameObject);
        }
    }
}
