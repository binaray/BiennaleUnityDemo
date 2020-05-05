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

    public void SetEmptyState()
    {
        foreach (GameObject o in roomUnits)
        {
            o.SetActive(false);
        }
    }

    public void DeleteUnit(BuildingUnit u)
    {
        int j = u.location[1];
        for (int x = 0; x < u.roomCount; x++)
        {
            roomUnits[j + x].SetActive(false);
        }
    }

    public void AddUnit(BuildingUnit u)
    {
        int j = u.location[1];
        int unitTypeIndex;
        
        if (System.Enum.TryParse(u.type, out LivingArrangement lv))
        {
            unitTypeIndex = (int)lv;
        }
        else
        {
            if (System.Enum.TryParse(u.type, out SharedSpace ss))
                unitTypeIndex = (int)ss;
            else
                unitTypeIndex = -1;
        }

        for (int x = 0; x < u.roomCount; x++)
        {
            int col = j + x;
            roomUnits[col].SetActive(true);
            Renderer r = roomUnits[col].transform.GetChild(1).GetComponent<Renderer>();
            r.material.SetColor("_Color", unitColor[unitTypeIndex]._Color);
            r.material.SetColor("_EmissionColor", unitColor[unitTypeIndex]._EmissionColor);
            r.material.renderQueue = 3002;
            SpriteRenderer s = roomUnits[col].transform.GetChild(2).GetComponent<SpriteRenderer>();
            s.sprite = Resources.Load<Sprite>("Sprites/UI/add");
            s.material.renderQueue = 3002;
        }
    }

    //To be deprecated- use add delete-edit-add unit instead
    public void SetUnitArray(List<int> roomUnitTypes)
    {
        if (roomUnitTypes.Count != roomUnits.Count)
        {
            Debug.LogError(string.Format("Array count mismatch!: \n{0} != {1}", roomUnitTypes.Count, roomUnits.Count));
            return;
        }
        for (int i = 0; i < roomUnitTypes.Count; i++)
        {
            //roomUnits[0].transform.GetChild(1).GetComponent<Renderer>().material.SetColor("_Color", unitColor[roomUnitTypes[i]]._Color);
            Renderer r = roomUnits[i].transform.GetChild(1).GetComponent<Renderer>();
            r.material.SetColor("_Color", unitColor[roomUnitTypes[i]]._Color);
            r.material.SetColor("_EmissionColor", unitColor[roomUnitTypes[i]]._EmissionColor);
        }
    }
    


    public void SetRoomMode(bool b)
    {
        foreach (GameObject o in roomUnits)
        {
            o.transform.GetChild(0).gameObject.SetActive(b);
            o.transform.GetChild(1).gameObject.SetActive(!b);
            o.transform.GetChild(2).gameObject.SetActive(!b);
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