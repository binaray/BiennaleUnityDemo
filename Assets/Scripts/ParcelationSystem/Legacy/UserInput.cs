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

    public UserInput(string avatarName, int avatarIndex, int unitTypeIndex, int sectionIndex, int floorRangeIndex, int pax, int sharedSpaceIndex)
    {
        props = new Props(avatarName, avatarIndex);
        sharedSpaceInput = new SharedSpaceInput(sharedSpaceIndex);
        this.unitTypeIndex = unitTypeIndex;
        this.sectionIndex = sectionIndex;
        this.floorRangeIndex = floorRangeIndex;
        this.pax = pax;
    }
    public UserInput(string avatarName, int avatarIndex, int pax, int sharedSpaceIndex)
    {
        props = new Props(avatarName, avatarIndex);
        sharedSpaceInput = new SharedSpaceInput(sharedSpaceIndex);
        this.pax = pax;
    }
}
[System.Serializable]
public class SharedSpaceInput
{
    public int SharedSpaceIndex;

    public SharedSpaceInput(int SharedSpaceIndex)
    {
        this.SharedSpaceIndex = SharedSpaceIndex;
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

// For storing request result
[System.Serializable]
public class UserInputResult
{
    public int unitId = -1;
    public int returnCode = -1;
    public Dictionary<int, Unit> state;
}