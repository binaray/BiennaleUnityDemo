using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Unit class for storing and handling unit states
[System.Serializable]
public class Unit
{
    public int id; //takes in the leftmost (smallest) blockIndex as anchor for graphics and effects
    public int anchorIndex;
    public int floorIndex;
    public int unitTypeIndex;
    public Props props;

    public Unit(int unitTypeIndex, int floorIndex,  int anchorIndex, Props props)
    {
        this.unitTypeIndex = unitTypeIndex;
        this.floorIndex = floorIndex;
        this.anchorIndex = anchorIndex;
        this.props = props;
    }
}