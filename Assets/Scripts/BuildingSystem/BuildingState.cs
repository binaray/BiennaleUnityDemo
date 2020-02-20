using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//json serializable which is used to compare states with the server's
//MAY NOT BE USED

[System.Serializable]
public class BuildingState
{
    public List<Unit> occupiedUnits = new List<Unit>();
    public static BuildingState CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<BuildingState>(jsonString);
    }
}
