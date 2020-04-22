using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> roomUnits;

    public static Dictionary<int, RoomUnitColor> unitColor = new Dictionary<int, RoomUnitColor>()
        {
            { 0, new RoomUnitColor(new Color(1,0,0,0.5f),new Color(1,0.15f,0.15f)) },
            { 1, new RoomUnitColor(new Color(0,1,0,0.5f),new Color(0,1,0)) },
            { 2, new RoomUnitColor(new Color(0,0.5f,1,0.5f),new Color(0,0.5f,1)) },
            { 3, new RoomUnitColor(new Color(0.95f,1,0.4f,0.5f),new Color(1,1,0)) },
            { 4, new RoomUnitColor(new Color(0.9f,0.15f,1,0.5f),new Color(0.85f,0.12f,1)) }
        };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUnitArray(List<int> roomUnitTypes)
    {
        if (roomUnitTypes.Count != roomUnits.Count)
        {
            Debug.LogError(string.Format("Array count mismatch!: \n{0} != {1}", roomUnitTypes.Count, roomUnits.Count));
            return;
        }
        for (int i = 0; i < roomUnitTypes.Count; i++)
        {
            Renderer r = roomUnits[i].transform.GetChild(1).GetComponent<Renderer>();
            r.material.SetColor("_Color", unitColor[roomUnitTypes[i]]._Color);
            r.material.SetColor("_EmissionColor", unitColor[roomUnitTypes[i]]._EmissionColor);
        }
    }
}

public class RoomUnitColor
{
    public Color _Color { get; set; }
    public Color _EmissionColor { get; set; }

    public RoomUnitColor(Color color, Color emissionColor)
    {
        _Color = color;
        _EmissionColor = emissionColor;
    }
}