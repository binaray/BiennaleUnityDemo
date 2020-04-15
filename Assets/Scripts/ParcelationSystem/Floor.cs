using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> roomUnits;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUnitColorArray(List<RoomUnitColor> roomUnitColors)
    {
        if (roomUnitColors.Count != roomUnits.Count)
        {
            Debug.LogError(string.Format("Array count mismatch!: \n{0} != {1}", roomUnitColors.Count, roomUnits.Count));
            return;
        }
        for (int i = 0; i < roomUnitColors.Count; i++)
        {
            Renderer r = roomUnits[i].GetComponent<Renderer>();
            r.material.SetColor("_Color", roomUnitColors[i]._Color);
            r.material.SetColor("_EmissionColor", roomUnitColors[i]._EmissionColor);
            Debug.Log(string.Format("Color set r={0}", roomUnitColors[i]._Color.r));
        }
    }
}
