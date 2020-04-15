using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParcelationManager : MonoBehaviour
{
    [SerializeField]
    private GameObject floorPrefab;
    [SerializeField]
    float yOffset = 0.0454f;
    [SerializeField]
    int floorCount = 34;

    private Dictionary<string, RoomUnitColor> roomUnitColor = new Dictionary<string, RoomUnitColor>()
        {
            { "0", new RoomUnitColor(new Color(1,0,0,0.5f),new Color(1,0,0)) },
            { "1", new RoomUnitColor(new Color(0,1,0,0.5f),new Color(0,1,0)) },
            { "2", new RoomUnitColor(new Color(0,0,1,0.5f),new Color(0,0,1)) },
        };

    //floors generated at runtime which index corresponds to level
    private List<Floor> floors = new List<Floor>();

    // Start is called before the first frame update
    void Awake()
    {
        float yPos = 0;

        for (int i = 0; i < floorCount; i++)
        {
            Debug.Log(yOffset);
            GameObject o = Instantiate(floorPrefab);
            o.transform.SetParent(this.transform);
            o.transform.localPosition = new Vector3(0, yPos, 0);
            floors.Add(o.GetComponent<Floor>());

            List<RoomUnitColor> roomUnitColors = new List<RoomUnitColor>();
            for (int j = 0; j < 16; j++)
            {
                roomUnitColors.Add(roomUnitColor["1"]);
            }
            floors[i].SetUnitColorArray(roomUnitColors);
            yPos += yOffset;
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
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
