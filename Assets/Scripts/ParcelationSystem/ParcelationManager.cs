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

    private Dictionary<double, BuildingUnit> currentBuildingState = new Dictionary<double, BuildingUnit>();

    private static Color[] bubbleColors = 
    {
        new Color(0.031f, 0.612f, 0.533f),
        new Color(0.643f, 0.137f, 0.506f),
        new Color(1f, 0.773f, 0.235f),
        new Color(0.922f, 0.329f, 0.467f),
        new Color(0.388f, 0.647f, 0.329f)
    };

    [SerializeField]
    private float spriteUpdateTime = 1f;
    public static List<Sprite> sprites = new List<Sprite>();

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

        for (int i = 0; i < 9; i++)
        {
            string str = "Sprites/Parcelation/" + (i).ToString();
            sprites.Add(Resources.Load<Sprite>(str));
        }

        for (int i = 0; i < floorCount; i++)
        {
            GameObject o = Instantiate(floorPrefab);
            o.transform.SetParent(this.transform);
            o.transform.localPosition = new Vector3(0, yPos, 0);
            floors.Add(o.GetComponent<Floor>());

            //List<int> roomUnitTypes = new List<int>();
            //for (int j = 0; j < 16; j++)
            //{
            //    int t = j % 7;
            //    roomUnitTypes.Add(t);
            //    print(t);
            //}
            //floors[i].SetUnitArray(roomUnitTypes);
            floors[i].SetEmptyState();

            yPos += yOffset;
        }        
    }

    private void Start()
    {
        //TODO: running check
        StartCoroutine(GenerateSpeechBubble());
        StartCoroutine(UpdateParcelationSprites());

        ////debugging test code-implementation at connection routine
        //string res = "[{\"user_id\":0.0,\"floor\":1,\"loc\":[0,9],\"type\":\"Farm\",\"user_input\":{\"livingArrangement\":\"Farm\"}},{\"user_id\":1589892632.7520728111,\"floor\":1,\"loc\":[10,14],\"type\":\"Flatshare\",\"user_input\":{\"livingArrangement\":\"Flatshare\",\"ageGroup\":\"Elderly\",\"pax\":2,\"affordable\":true,\"requiredRooms\":{\"SingleBedroom\":0,\"SharedBedroom\":0,\"Study\":0},\"location\":[\"0\",\"1\"],\"preferredSharedSpaces\":[]}},{\"user_id\":3.0,\"floor\":15,\"loc\":[3,12],\"type\":\"Cafes\",\"user_input\":{\"livingArrangement\":\"Cafes\"}},{\"user_id\":4.0,\"floor\":17,\"loc\":[0,9],\"type\":\"Clinic\",\"user_input\":{\"livingArrangement\":\"Clinic\"}},{\"user_id\":1.0,\"floor\":32,\"loc\":[3,12],\"type\":\"Lounge\",\"user_input\":{\"livingArrangement\":\"Lounge\"}},{\"user_id\":2.0,\"floor\":33,\"loc\":[3,12],\"type\":\"Makerspace\",\"user_input\":{\"livingArrangement\":\"Makerspace\"}}]";
        //List<BuildingUnit> newState = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUnit>>(res);
        //UpdateParcelation(newState);
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

    IEnumerator UpdateParcelationSprites()
    {
        foreach (BuildingUnit u in currentBuildingState.Values)
        {
            floors[u.floor].RandomizeUnitSprite(u);
        }
        yield return new WaitForSeconds(spriteUpdateTime);
        StartCoroutine(UpdateParcelationSprites());
    }

    public void UpdateParcelation(List<BuildingUnit> l)
    {
        //List<BuildingUnit> l = new List<BuildingUnit>();
        ////generate test data
        //for (int i = 0; i < 4; i++)
        //{
        //    BuildingUnit u = new BuildingUnit(i, i, i, 3, "Single");
        //    l.Add(u);
        //}
        //for (int i = 0; i < 6; i++)
        //{
        //    BuildingUnit u = new BuildingUnit(i+4, i+7, i, 6, "CoupleWoChildren");
        //    l.Add(u);
        //}

        Dictionary<double, BuildingUnit> newState = new Dictionary<double, BuildingUnit>();
        foreach (BuildingUnit u in l)
        {
            while (newState.ContainsKey(u.user_id))
            {
                u.user_id += 0.01;
            }
            newState.Add(u.user_id, u);
            Debug.LogWarning(u.ToString());
        }
        
        HashSet<double> newKeys = new HashSet<double>(newState.Keys);
        HashSet<double> oldKeys = new HashSet<double>(currentBuildingState.Keys);
        HashSet<double> toModify = new HashSet<double>(oldKeys);

        //Delete
        toModify.ExceptWith(newKeys);
        foreach(double k in toModify)
        {
            int i = currentBuildingState[k].floor;
            int j = currentBuildingState[k].loc[0];
            floors[i].DeleteUnit(currentBuildingState[k]);
        }

        //Check and Edit
        toModify = new HashSet<double>(newKeys);
        toModify.IntersectWith(oldKeys);
        foreach (double k in toModify)
        {
            int i_o = currentBuildingState[k].floor;
            int j_o = currentBuildingState[k].loc[0];
            int i_n = newState[k].floor;
            int j_n = newState[k].loc[0];

            if (i_n != i_o || j_n != j_o || newState[k].loc[1] != currentBuildingState[k].loc[1])
            {
                floors[i_o].DeleteUnit(currentBuildingState[k]);
                floors[i_n].AddUnit(newState[k]);
            }
        }

        //New insertion
        toModify = new HashSet<double>(newKeys);
        toModify.ExceptWith(oldKeys);
        foreach (double k in toModify)
        {
            int i = newState[k].floor;
            int j = newState[k].loc[0];
            floors[i].AddUnit(newState[k]);
        }

        currentBuildingState = newState;
    }
}

[System.Serializable]
public class BuildingUnit
{
    public double user_id;
    public int floor;
    public int[] loc;
    //public int[] location = { -1, -1 };
    //public int roomCount = -1;
    public string type = "";
    public QuestionResults user_input;

    public BuildingUnit()
    {

    }
    public BuildingUnit(double user_id, int row, int col, int roomCount, string type)
    {
        this.user_id = user_id;
        this.floor = row;
        this.loc = new int[] { col, col + roomCount - 1 };
        //location[0] = row;
        //location[1] = col;
        //this.roomCount = roomCount;
        this.type = type;
    }

    public override string ToString()
    {
        LivingArrangement lv;
        if (System.Enum.TryParse<LivingArrangement>(type, out lv))
            return string.Format("id: {0}, loc: [{1},{2}-{3}], type: {4}, Votes: ", user_id, floor, loc[0], loc[1], type) + user_input.ToString();
        else
            return string.Format("id: {0}, loc: [{1},{2}-{3}], type: {4}", user_id, floor, loc[0], loc[1], type);
    }
}