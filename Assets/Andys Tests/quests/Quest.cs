using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "testquest", menuName = "ScriptableObjects/questData", order = 1)]
public class Quest : ScriptableObject
{
    public string description;
    public int questMax;
    public int questMin;
    public int Belohnung;
}
