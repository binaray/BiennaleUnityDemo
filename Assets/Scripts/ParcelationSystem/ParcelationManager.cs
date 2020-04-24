using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParcelationManager : MonoBehaviour
{
    [SerializeField]
    private GameObject floorPrefab;
    [SerializeField]
    private GameObject bubblePrefab;
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
    }

    IEnumerator GenerateSpeechBubble()
    {
        for (int i = 0; i < floorCount; i++)
        {
            //List<string> data = new List<string>();
            for (int j = 0; j < 16; j++)
            {
                float r = Random.value;
                print(r);
                if (r < bubbleSpanProb)
                {
                    GameObject o = Instantiate(bubblePrefab);
                    o.transform.SetParent(floors[i].roomUnits[j].transform);
                    if (j < 8)
                    {
                        o.transform.localPosition = new Vector3(-0.316f, 0.132f, -0.111f);
                        if (j==2 || j==3 || j == 7 || j == 8 || j == 12 || j == 13)
                            o.transform.localPosition = new Vector3(-0.316f, 0.132f, -0.189f);
                    }
                    else
                    {
                        o.transform.localPosition = new Vector3(0.321f, 0.132f, -0.111f);
                        if (j == 2 || j == 3 || j == 7 || j == 8 || j == 12 || j == 13)
                            o.transform.localPosition = new Vector3(0.321f, 0.132f, -0.189f);
                    }
                    o.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    o.GetComponent<Bubble>().SetText("Bob ran down the hill to catch snails.");                    
                }
            }
            //floors[i].SetBubbleArray(data);
        }
        
        yield return new WaitForSeconds(generationTime);
        StartCoroutine(GenerateSpeechBubble());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

