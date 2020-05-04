using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParcelationManager : MonoBehaviour
{
    [SerializeField]
    private GameObject floorPrefab;
    [SerializeField]
    private GameObject bubbleLeftPrefab;
    [SerializeField]
    private GameObject bubbleRightPrefab;
    [SerializeField]
    float yOffset = 0.0454f;
    [SerializeField]
    int floorCount = 34;
    //floors generated at runtime which index corresponds to level
    private List<Floor> floors = new List<Floor>();
    private bool _roomMode;
    public bool RoomMode
    {
        get
        {
            return _roomMode;
        }
        set
        {
            foreach (Floor f in floors)
            {
                f.SetRoomMode(value);
            }
            _roomMode = value;
        }
    }
    [SerializeField]
    private float bubbleSpanProb = 0.1f;
    [SerializeField]
    private float generationTime = 5;

    private Dictionary<int, BuildingUnit> currentBuildingState = new Dictionary<int, BuildingUnit>();

    private static Color[] bubbleColors = 
    {
        new Color(0.031f, 0.612f, 0.533f),
        new Color(0.643f, 0.137f, 0.506f),
        new Color(1f, 0.773f, 0.235f),
        new Color(0.922f, 0.329f, 0.467f),
        new Color(0.388f, 0.647f, 0.329f)
    };

    public static ParcelationManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        for (int i = 0; i < transform.childCount; i++)
            Destroy(transform.GetChild(i).gameObject);

        float yPos = 0;

        for (int i = 0; i < floorCount; i++)
        {
            GameObject o = Instantiate(floorPrefab);
            o.transform.SetParent(this.transform);
            o.transform.localPosition = new Vector3(0, yPos, 0);
            floors.Add(o.GetComponent<Floor>());

            List<int> roomUnitTypes = new List<int>();
            for (int j = 0; j < 16; j++)
            {
                int t = j % Floor.unitColor.Count;
                roomUnitTypes.Add(t);
                print(t);
            }
            floors[i].SetUnitArray(roomUnitTypes);
            yPos += yOffset;
        }        
    }

    private void Start()
    {
        StartCoroutine(GenerateSpeechBubble());
        UpdateParcelation();
    }

    IEnumerator GenerateSpeechBubble()
    {
        int colorSeed = Random.Range(0, bubbleColors.Length);
        for (int i = 0; i < floorCount; i++)
        {
            //List<string> data = new List<string>();
            for (int j = 0; j < 16; j++)
            {
                if (Random.value < bubbleSpanProb)
                {
                    GameObject o;
                    if (j < 8)
                    {
                        o = Instantiate(bubbleLeftPrefab);
                        o.transform.SetParent(floors[i].roomUnits[j].transform);
                        o.transform.localPosition = new Vector3(-0.112f, 0.062f, -0.111f);
                        if (j == 2 || j == 3 || j == 7) 
                            o.transform.localPosition = new Vector3(-0.112f, 0.062f, -0.189f);
                    }
                    else
                    {
                        //print(string.Format("x={0} y={1}", j, i));
                        o = Instantiate(bubbleRightPrefab);
                        o.transform.SetParent(floors[i].roomUnits[j].transform);
                        o.transform.localPosition = new Vector3(0.112f, 0.062f, -0.111f);
                        if (j == 8 || j == 12 || j == 13)
                            o.transform.localPosition = new Vector3(0.112f, 0.062f, -0.189f);
                    }
                    o.transform.localRotation = Quaternion.Euler(-90, 0, 180);
                    o.GetComponent<Bubble>().SetText("Bob ran down the hill to catch snails.");
                    int c = colorSeed++ % bubbleColors.Length;
                    o.GetComponent<Renderer>().material.SetColor("_Color", bubbleColors[c]);
                    o.GetComponent<Renderer>().material.SetColor("_EmissionColor", bubbleColors[c]);
                }
            }
            //floors[i].SetBubbleArray(data);
        }
        
        yield return new WaitForSeconds(generationTime);
        StartCoroutine(GenerateSpeechBubble());
    }

    public void UpdateParcelation()
    {
        List<BuildingUnit> l = new List<BuildingUnit>();
        //generate test data
        for (int i = 0; i < 4; i++)
        {
            BuildingUnit u = new BuildingUnit(i, i, i, 3, "Single");
            l.Add(u);
        }
        Dictionary<int, BuildingUnit> newState = l.ToDictionary(u => u.index, u => u);

        HashSet<int> newKeys = new HashSet<int>(newState.Keys);
        HashSet<int> oldKeys = new HashSet<int>(currentBuildingState.Keys);
        HashSet<int> toModify = new HashSet<int>(oldKeys);

        //Delete
        toModify.ExceptWith(newKeys);
        foreach(int k in toModify)
        {
            int i = currentBuildingState[k].location[0];
            int j = currentBuildingState[k].location[1];
            floors[i].DeleteUnit(currentBuildingState[k]);
        }

        //Check and Edit
        toModify = new HashSet<int>(newKeys);
        toModify.IntersectWith(oldKeys);
        foreach (int k in toModify)
        {
            int i_o = currentBuildingState[k].location[0];
            int j_o = currentBuildingState[k].location[1];
            int i_n = newState[k].location[0];
            int j_n = newState[k].location[1];

            if (i_n != i_o || j_n != j_o || newState[k].roomCount != currentBuildingState[k].roomCount)
            {
                floors[i_o].DeleteUnit(currentBuildingState[k]);
                floors[i_n].AddUnit(newState[k]);
            }
        }

        //New insertion
        toModify = new HashSet<int>(newKeys);
        toModify.ExceptWith(oldKeys);
        foreach (int k in toModify)
        {
            int i = newState[k].location[0];
            int j = newState[k].location[1];
            floors[i].AddUnit(newState[k]);
        }

        currentBuildingState = newState;
    }
}

[System.Serializable]
public class BuildingUnit
{
    public int index;
    public int[] location = { -1, -1 };
    public int roomCount = -1;
    public string type = "";

    public BuildingUnit()
    {

    }
    public BuildingUnit(int index, int row, int col, int roomCount, string type)
    {
        this.index = index;
        location[0] = row;
        location[1] = col;
        this.roomCount = roomCount;
        this.type = type;
    }
}