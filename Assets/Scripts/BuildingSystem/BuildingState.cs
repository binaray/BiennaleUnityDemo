using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//json serializable which is used to compare states with the server's

[System.Serializable]
public class BuildingState
{
    public List<Unit> occupiedUnits = new List<Unit>();
    public static BuildingState CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<BuildingState>(jsonString);
    }
}

[System.Serializable]
public class Unit
{
    public int id; //takes in the leftmost (smallest) blockIndex as anchor for graphics and effects
    public int blockAnchor;
    public int unitType;
    public string props;

    public Unit(int id, int blockAnchor, int unitId, string message)
    {
        this.id = id;
        this.blockAnchor = blockAnchor;
        this.unitType = unitId;
        this.props = message;
    }
}