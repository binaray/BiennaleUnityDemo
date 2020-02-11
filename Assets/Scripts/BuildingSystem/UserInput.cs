using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//json serializable class for server request

[System.Serializable]
public class UserInput
{
    public string name;
    public string sectionIndex;
    public int floorRangeIndex;
    public int pax;
    public int avatarId;
    public string favLocation;

    public UserInput() { }
    public UserInput(string name, int pax, int avatarId, string favLocIndex)
    {
        this.name = name;
        this.pax = pax;
        this.avatarId = avatarId;
        this.favLocation = favLocIndex;
    }
    public UserInput(string name, string sectionIndex, int floorRangeIndex, int pax, int avatarId, string favLocIndex)
    {
        this.name = name;
        this.sectionIndex = sectionIndex;
        this.floorRangeIndex = floorRangeIndex;
        this.pax = pax;
        this.avatarId = avatarId;
        this.favLocation = favLocIndex;
    }
}
