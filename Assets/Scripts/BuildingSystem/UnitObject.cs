using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Unit class for storing and handling unit states
public class UnitObject : MonoBehaviour
{
    public int anchorIndex;
    public int floorIndex;
    public int unitTypeIndex;
    public Props props;

    public void SetUnitProperties(Unit unit)
    {
        //TODO: set display properties reflecting such here.
        this.unitTypeIndex = unit.unitTypeIndex;
        this.floorIndex = unit.floorIndex;
        this.anchorIndex = unit.anchorIndex;
        this.props = unit.props;
    }

    public UnitObject(int unitTypeIndex, int floorIndex, int anchorIndex, Props props)
    {
        this.unitTypeIndex = unitTypeIndex;
        this.floorIndex = floorIndex;
        this.anchorIndex = anchorIndex;
        this.props = props;
    }

    public int GetAnchorIndex()
    {
        return floorIndex * BuildingBlockManager.Instance.blocksPerFloor + anchorIndex;
    }

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}
