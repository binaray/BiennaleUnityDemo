﻿using System.Collections;
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

    public Camera cam;
    [SerializeField]
    private RectTransform messageOverlay;
    [SerializeField]
    private GameObject messageBubble2D;
    private bool _showMessages;
    public bool ShowMessages { get { return _showMessages; } set { messageOverlay.gameObject.SetActive(value); _showMessages = value; } }
    public bool _messageLock = false;
    public int TopicCursor { get; set; }
    public List<MessageTopic> messageTopics = null;
    [SerializeField]
    private float bubbleSpanProb = 0.1f;
    [SerializeField]
    private float generationTime = 5;

    private Dictionary<double, BuildingUnit> currentBuildingState = new Dictionary<double, BuildingUnit>();

    private static Color[] bubbleColors = 
    {
        new Color(0.91f, 0.00f, 0.02f),
        new Color(0.00f, 0.46f, 0.80f),
        new Color(1.00f, 0.69f, 0.82f),
        new Color(0.71f, 0.67f, 0.84f),
        new Color(1.00f, 0.80f, 0.10f),
        new Color(0.52f, 0.89f, 0.71f),
        new Color(0.65f, 0.73f, 0.78f),
        new Color(0.82f, 0.89f, 0.62f)
    };

    [SerializeField]
    private float spriteUpdateTime = 1f;
    // public static List<Sprite> sprites = new List<Sprite>();
    public static Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();

    [HideInInspector]
    public double currentUserId = -1;
    [HideInInspector]
    public List<Renderer> userUnitR = new List<Renderer>();
    private Vector4 _uColor, currentUColor;
    [HideInInspector]
    public Vector4 UColor { get { return _uColor; } set { currentUColor = value; _uColor = value; } }
    [HideInInspector]
    public Vector4 uDelta = Vector4.zero;

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
            sprites.Add(i, Resources.Load<Sprite>(str));
        }
        sprites.Add(9, Resources.Load<Sprite>("Sprites/Parcelation/work"));
        for (int i = 100; i < 116; i++)
        {
            string str = "Sprites/Parcelation/" + (i).ToString();
            sprites.Add(i, Resources.Load<Sprite>(str));
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
        //StartCoroutine(UpdateParcelationSprites());
        StartCoroutine(FlashUserUnit());

        ////debugging test code-implementation at connection routine
        //string res = "[{\"user_id\":0.0,\"floor\":1,\"loc\":[0,9],\"type\":\"Farm\",\"user_input\":{\"livingArrangement\":\"Farm\"}},{\"user_id\":1589892632.7520728111,\"floor\":1,\"loc\":[10,14],\"type\":\"Flatshare\",\"user_input\":{\"livingArrangement\":\"Flatshare\",\"ageGroup\":\"Elderly\",\"pax\":2,\"affordable\":true,\"requiredRooms\":{\"SingleBedroom\":0,\"SharedBedroom\":0,\"Study\":0},\"location\":[\"0\",\"1\"],\"preferredSharedSpaces\":[]}},{\"user_id\":3.0,\"floor\":15,\"loc\":[3,12],\"type\":\"Cafes\",\"user_input\":{\"livingArrangement\":\"Cafes\"}},{\"user_id\":4.0,\"floor\":17,\"loc\":[0,9],\"type\":\"Clinic\",\"user_input\":{\"livingArrangement\":\"Clinic\"}},{\"user_id\":1.0,\"floor\":32,\"loc\":[3,12],\"type\":\"Lounge\",\"user_input\":{\"livingArrangement\":\"Lounge\"}},{\"user_id\":2.0,\"floor\":33,\"loc\":[3,12],\"type\":\"Makerspace\",\"user_input\":{\"livingArrangement\":\"Makerspace\"}}]";
        //List<BuildingUnit> newState = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUnit>>(res);
        //UpdateParcelation(newState);
    }

    IEnumerator FlashUserUnit()
    {
        if (uDelta != Vector4.zero)
        {
            print(uDelta);
            currentUColor =  currentUColor + uDelta;
            if (currentUColor.magnitude <= UColor.magnitude || currentUColor.magnitude >= 1f)
            {
                uDelta *= -1;
                currentUColor = new Vector4(Mathf.Clamp(currentUColor.x, UColor.x, 1), Mathf.Clamp(currentUColor.y, UColor.y, 1), Mathf.Clamp(currentUColor.z, UColor.z, 1));
            }
            for(int i = 0; i < userUnitR.Count; i++)
            {
                userUnitR[i].material.SetColor("_EmissionColor", currentUColor);
            }
        }
        yield return new WaitForSeconds(.5f);
        StartCoroutine(FlashUserUnit());
    }

    public static IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
    {
        System.Random rand = new System.Random();
        List<TValue> values = Enumerable.ToList(dict.Values);
        int size = dict.Count;
        while (true)
        {
            yield return values[rand.Next(size)];
        }
    }

    IEnumerator GenerateSpeechBubble()
    {
        if (ShowMessages && messageTopics != null && messageTopics.Count != 0)
        {
            _messageLock = true;
            TopicCursor = (TopicCursor + 1) % messageTopics.Count;
            int colorSeed = Random.Range(0, bubbleColors.Length);
            int messageIndex = 0;
            int maxCount = 10;
            //TODO: set message limit?
            // Random with limit for units only
            List<double> rList = (currentBuildingState.Keys.ToList<double>()).OrderBy(a => System.Guid.NewGuid()).ToList(); ;
            foreach (double i in rList)
            {
                if (messageIndex < messageTopics[TopicCursor].messages.Count)
                {
                    BuildingUnit u = currentBuildingState[i];
                    System.Random n = new System.Random();
                    int j = Random.Range(u.loc[0], u.loc[1] + 1);
                    
                    Vector3 pos = floors[u.floor].roomUnits[j].transform.position;
                    Vector3 camPos = cam.WorldToScreenPoint(floors[u.floor].roomUnits[j].transform.position);
                    //Debug.LogError("ScreenPos:");
                    //Debug.LogError(camPos);

                    if (camPos.z > 0)
                    {
                        //TODO: update position in local script
                        GameObject m = Instantiate(messageBubble2D, cam.WorldToScreenPoint(floors[u.floor].roomUnits[j].transform.position), messageBubble2D.transform.rotation, messageOverlay);
                        m.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = messageTopics[TopicCursor].messages[messageIndex].message;
                        int c = colorSeed++ % bubbleColors.Length;
                        m.GetComponent<UnityEngine.UI.Image>().color = bubbleColors[c];//TopicCursor % bubbleColors.Length];
                        m.GetComponent<MessageBubble2D>().trackedPos = pos;
                        //GameObject m = Instantiate(messageBubble2D);
                        //m.transform.SetParent(messageOverlay);
                        //m.GetComponent<RectTransform>().anchoredPosition = screenPos;
                    }

                    //--Scene spawn mesh method
                    //GameObject o;
                    //if (j < 8)
                    //{
                    //    o = Instantiate(bubbleLeftPrefab);
                    //    o.transform.SetParent(floors[u.floor].roomUnits[j].transform);
                    //    o.transform.localPosition = new Vector3(-0.112f, 0.062f, -0.111f);
                    //    if (j == 2 || j == 3 || j == 7)
                    //        o.transform.localPosition = new Vector3(-0.112f, 0.062f, -0.189f);
                    //}
                    //else
                    //{
                    //    //print(string.Format("x={0} y={1}", j, i));
                    //    o = Instantiate(bubbleRightPrefab);
                    //    o.transform.SetParent(floors[u.floor].roomUnits[j].transform);
                    //    o.transform.localPosition = new Vector3(0.112f, 0.062f, -0.111f);
                    //    if (j == 8 || j == 12 || j == 13)
                    //        o.transform.localPosition = new Vector3(0.112f, 0.062f, -0.189f);
                    //}
                    //o.transform.localRotation = Quaternion.Euler(-90, 0, 180);
                    //o.GetComponent<Bubble>().SetText(messageTopics[TopicCursor].messages[messageIndex].message);
                    //print(string.Format("Message spawned at [{0},{1}]", i, j));
                    ////print(string.Format("message topic: {0}: {1}. {2}", TopicCursor, messageIndex, messageTopics[TopicCursor].messages[messageIndex].message));
                    //int c = colorSeed++ % bubbleColors.Length;
                    //o.GetComponent<Renderer>().material.SetColor("_Color", bubbleColors[c]);
                    //o.GetComponent<Renderer>().material.SetColor("_EmissionColor", bubbleColors[c]);

                    messageIndex++;
                    maxCount--;
                    if (maxCount <= 0) break;
                }
                else break;
            }

            // Random with limit for entire building
            //foreach (int i in Enumerable.Range(0, floorCount).OrderBy(x => r.Next()))
            //{
            //    if (messageIndex < messageTopics[TopicCursor].messages.Count)
            //    {
            //        System.Random n = new System.Random();
            //        int j = Random.Range(0, 16);
            //        GameObject o;
            //        if (j < 8)
            //        {
            //            o = Instantiate(bubbleLeftPrefab);
            //            o.transform.SetParent(floors[i].roomUnits[j].transform);
            //            o.transform.localPosition = new Vector3(-0.112f, 0.062f, -0.111f);
            //            if (j == 2 || j == 3 || j == 7)
            //                o.transform.localPosition = new Vector3(-0.112f, 0.062f, -0.189f);
            //        }
            //        else
            //        {
            //            //print(string.Format("x={0} y={1}", j, i));
            //            o = Instantiate(bubbleRightPrefab);
            //            o.transform.SetParent(floors[i].roomUnits[j].transform);
            //            o.transform.localPosition = new Vector3(0.112f, 0.062f, -0.111f);
            //            if (j == 8 || j == 12 || j == 13)
            //                o.transform.localPosition = new Vector3(0.112f, 0.062f, -0.189f);
            //        }
            //        o.transform.localRotation = Quaternion.Euler(-90, 0, 180);
            //        o.GetComponent<Bubble>().SetText(messageTopics[TopicCursor].messages[messageIndex].message);
            //        print(string.Format("Message spawned at [{0},{1}]", i, j));
            //        //print(string.Format("message topic: {0}: {1}. {2}", TopicCursor, messageIndex, messageTopics[TopicCursor].messages[messageIndex].message));
            //        int c = colorSeed++ % bubbleColors.Length;
            //        o.GetComponent<Renderer>().material.SetColor("_Color", bubbleColors[c]);
            //        o.GetComponent<Renderer>().material.SetColor("_EmissionColor", bubbleColors[c]);
            //        messageIndex++;
            //    }
            //    else break;
            //}

            // Binomial Randomization
            //for (int i = 0; i < floorCount; i++)
            //{
            //    //List<string> data = new List<string>();
            //    for (int j = 0; j < 16; j++)
            //    {
            //        if (Random.value < bubbleSpanProb)
            //        {
            //            GameObject o;
            //            if (j < 8)
            //            {
            //                o = Instantiate(bubbleLeftPrefab);
            //                o.transform.SetParent(floors[i].roomUnits[j].transform);
            //                o.transform.localPosition = new Vector3(-0.112f, 0.062f, -0.111f);
            //                if (j == 2 || j == 3 || j == 7)
            //                    o.transform.localPosition = new Vector3(-0.112f, 0.062f, -0.189f);
            //            }
            //            else
            //            {
            //                //print(string.Format("x={0} y={1}", j, i));
            //                o = Instantiate(bubbleRightPrefab);
            //                o.transform.SetParent(floors[i].roomUnits[j].transform);
            //                o.transform.localPosition = new Vector3(0.112f, 0.062f, -0.111f);
            //                if (j == 8 || j == 12 || j == 13)
            //                    o.transform.localPosition = new Vector3(0.112f, 0.062f, -0.189f);
            //            }
            //            o.transform.localRotation = Quaternion.Euler(-90, 0, 180);
            //            o.GetComponent<Bubble>().SetText("Bob ran down the hill to catch snails.");
            //            int c = colorSeed++ % bubbleColors.Length;
            //            o.GetComponent<Renderer>().material.SetColor("_Color", bubbleColors[c]);
            //            o.GetComponent<Renderer>().material.SetColor("_EmissionColor", bubbleColors[c]);
            //        }
            //    }
            //}
            _messageLock = false;
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

    public void ReloadParcelationVis()
    {
        foreach (BuildingUnit u in currentBuildingState.Values)
        {
            int i = u.floor;
            floors[i].AddUnit(u);
        }
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
            //Debug.LogWarning(u.ToString());
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

[System.Serializable]
public class Message
{
    public int messageId;
    public string message;
    public string reply;
    public int timestamp;

    public override string ToString()
    {
        return string.Format("{0}: {1}", messageId, message);
    }
}