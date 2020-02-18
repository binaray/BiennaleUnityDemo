using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//json serializable class for server request

[System.Serializable]
public class UserInput
{
    public int unitTypeIndex;
    public int sectionIndex;
    public int floorRangeIndex;
    public int pax;
    public SharedSpaceInput sharedSpaceInput;
    public Props props;

    public UserInput(string avatarName, int avatarIndex, int unitTypeIndex, int sectionIndex, int floorRangeIndex, int pax, string sharedSpaceName)
    {
        props = new Props(avatarName, avatarIndex);
        sharedSpaceInput = new SharedSpaceInput(sharedSpaceName);
        this.unitTypeIndex = unitTypeIndex;
        this.sectionIndex = sectionIndex;
        this.floorRangeIndex = floorRangeIndex;
        this.pax = pax;
    }
    public UserInput(string avatarName, int avatarIndex, int pax, string sharedSpaceName)
    {
        props = new Props(avatarName, avatarIndex);
        sharedSpaceInput = new SharedSpaceInput(sharedSpaceName);
        this.pax = pax;
    }
}
[System.Serializable]
public class SharedSpaceInput
{
    public string locationName;

    public SharedSpaceInput(string locationName)
    {
        this.locationName = locationName;
    }
}

[System.Serializable]
public class Props
{
    public string avatarName;
    public int avatarIndex;     //image index

    public Props(string avatarName, int avatarIndex)
    {
        this.avatarName = avatarName;
        this.avatarIndex = avatarIndex;
    }
}