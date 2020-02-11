using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//json serializable class for server request

[System.Serializable]
public class UserInput
{
    public string sectionIndex;
    public int floorRangeIndex;
    public int pax;
    public int avatarId;
    public int favLocIndex;

    public UserInput(string sectionIndex, int floorRangeIndex, int pax, int avatarId, int favLocIndex)
    {
        this.sectionIndex = sectionIndex;
        this.floorRangeIndex = floorRangeIndex;
        this.pax = pax;
        this.avatarId = avatarId;
        this.favLocIndex = favLocIndex;
    }
}
