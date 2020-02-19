using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingBlockManager : MonoBehaviour
{
    private const string UNIT_ID_COUNT_PREF = "UNIT_ID_COUNT_PREF";
    private const string UNIT_DB_STRING_PREF = "UNIT_DB_STRING_PREF";

    [SerializeField]
    private bool RefreshPrefs = false;

    [SerializeField]
    private BlockPrefab blockPrefab;
    [SerializeField]
    private float[] xOffset = { 1.0f };
    [SerializeField]
    private float[] yOffset = { 1.0f };

    //location mapping: input space which must be mapped according to server definition
    [SerializeField]
    private int leftSection = 4;
    [SerializeField]
    private int middleSection = 5;
    [SerializeField]
    private int rightSection = 4;
    [SerializeField]
    private int floorRange = 5;
    [SerializeField]
    private int floors = 30;
    private int blocksPerFloor = 0;

    //runtime reference to blocks in the following format [floor][Section][blocks] for easier selection
    private List<BlockPrefab> blocks = new List<BlockPrefab>();
    [SerializeField]
    private Color[] unitTypeColor;

    //selector vars
    private int[] prevSelection = new int[] { -1, -1 };
    private int unitTypeSelection = 0;
    public int UnitTypeSelection
    {
        get { 
            return this.unitTypeSelection; 
        }
        set
        {
            this.unitTypeSelection = value;
            //UiCanvasManager.Instance.SelectLocationState();
            //print(string.Format("Selection {0} set", this.unitTypeSelection));
        }
    }
    private bool _selectionLock;
    public bool SelectionLock {
        get 
        {
            return this._selectionLock;
        }
        set
        {
            //if (value)
            //    foreach (int i in selectedBlocks)
            //        blocks[i].SetColor(confirmedColor);
            //else
            //    if (!ConfirmationLock)
            //        ClearSelectedBlocks();
            this._selectionLock = value;
        }
    }
    public bool IsSelectionState { get; set; }
    private int unitIdCount;
    private BuildingState ownState; //Collection of unit positions to be synchronized to server
    private SortedSet<int> selectedBlocks = new SortedSet<int>();
    private SortedSet<int> confirmedBlocks = new SortedSet<int>();
    [SerializeField]
    private Color defaultColor = Color.white;
    [SerializeField]
    private Color selectionColor = Color.red;
    [SerializeField]
    private Color confirmedColor = Color.green;

    public static BuildingBlockManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (RefreshPrefs)
            PlayerPrefs.DeleteAll();

        unitIdCount = PlayerPrefs.GetInt(UNIT_ID_COUNT_PREF, 0);
        string savedJson = PlayerPrefs.GetString(UNIT_DB_STRING_PREF, "");
        if (savedJson != "")
            ownState = BuildingState.CreateFromJson(savedJson);
        else
            ownState = new BuildingState();

        //create and index blocks to their input selection space; left/mid/right, 1-5/6-10/..
        blocksPerFloor = middleSection + rightSection + leftSection;
        SelectionLock = false;
        IsSelectionState = false;
        if (blockPrefab != null)
        {
            float xPos = this.gameObject.transform.position.x;
            float yPos = this.gameObject.transform.position.y;
            for (int y = 0; y < floors; y++)
            {
                for (int x = 0; x < blocksPerFloor; x++)
                {
                    BlockPrefab block = Instantiate(blockPrefab, new Vector3(xPos, yPos, this.gameObject.transform.position.z), Quaternion.identity, this.transform);
                    block.Index = x + blocksPerFloor * y;
                    block.SetColor(defaultColor);
                    blocks.Add(block);
                    xPos += xOffset[x % xOffset.Length];
                }
                xPos = this.gameObject.transform.position.x;
                yPos += yOffset[y % yOffset.Length];
            }
        }

        //foreach (Unit unit in unitWrapper.occupiedUnits)
        //{
        //    int blocksLeft = unit.unitType;
        //    for (int i = 0; i < blocksLeft; i++)
        //    {
        //        int blockIndex = unit.blockIndex + i;
        //        blocks[blockIndex].UnitId = unit.unitId;
        //        blocks[blockIndex].SetColor(unitTypeColor[unit.unitType - 1]);
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {        
        if (Input.GetMouseButtonUp(0))
        {            
            SelectionLock = false;
        }

        if (!SelectionLock && IsSelectionState)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000))
            {
                Debug.DrawLine(ray.origin, hit.point);
                //Debug.Log(hit.transform.gameObject.name);
                switch (hit.collider.tag)
                {
                    case "BuildingBlock":
                        BlockPrefab block = hit.transform.gameObject.GetComponent<BlockPrefab>();
                        int blockIndex = block.Index;
                        Debug.Log(string.Format("index: {0}; unitId: {1}", block.Index, block.UnitId));
                        SelectUnitBlocks(blockIndex);
                        break;
                    default:
                        break;
                }

                //Debug.Log(blockIndex);
                //Debug.Log("hit");

            }
            else
            {
                ClearSelectedBlocks();
            }
        }
    }

    void SelectUnitBlocks(int blockIndex)
    {
        ClearSelectedBlocks();
        //int blocksLeft;
        //switch (UnitTypeSelection)
        //{
        //    case 1:
        //        blocksLeft = 1;
        //        break;
        //    case 2:
        //    case 3:
        //        blocksLeft = 2;
        //        break;
        //    case 4:
        //    case 5:
        //        blocksLeft = 3;
        //        break;
        //    default:
        //        return;
        //}
        //blocksLeft = UnitTypeSelection;

        int row = blockIndex / blocksPerFloor;
        int col = blockIndex - row * blocksPerFloor;
        int rangeIndex = row / floorRange;
        int sectionIndex;
        int floorLowerBound = rangeIndex * floorRange;
        int sectionLowerBound;
        int sectionUpperBound;

        if (col < leftSection)
        {
            sectionIndex = 0;    //left
            sectionLowerBound = 0;
            sectionUpperBound = leftSection;
        }
        else if (col < leftSection + middleSection)
        {
            sectionIndex = 1;    //middle
            sectionLowerBound = leftSection;
            sectionUpperBound = leftSection + middleSection;
        }
        else
        {
            sectionIndex = 2;  //right
            sectionLowerBound = leftSection + middleSection;
            sectionUpperBound = blocksPerFloor;
        }
 
        if (prevSelection[0] != sectionLowerBound || prevSelection[1] != rangeIndex)
        {
            if (prevSelection[0] > -1)
            {
                int prevfloorLowerBound = prevSelection[1] * floorRange;
                int prevSectionUpperBound = (prevSelection[0] == 0) ? leftSection : (prevSelection[0] == leftSection) ? leftSection + middleSection : blocksPerFloor;
                for (int j = prevfloorLowerBound; j < prevfloorLowerBound + floorRange; j++)
                {
                    for (int i = prevSelection[0]; i < prevSectionUpperBound; i++)
                    {
                        blocks[i + j * blocksPerFloor].SetColor(defaultColor);
                        //selectedBlocks.Add(i + j * blocksPerFloor);
                    }
                }
            }
        }

        for (int j = floorLowerBound; j < floorLowerBound + floorRange; j++)
        {
            for (int i = sectionLowerBound; i < sectionUpperBound; i++)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    SelectionLock = true;
                    blocks[i + j * blocksPerFloor].SetColor(confirmedColor);
                    ConnectionManager.Instance.SetUserInputLocation(ConnectionManager.Instance.SuggestedUnitTypeIndex, rangeIndex, sectionIndex);
                }
                else
                    blocks[i + j * blocksPerFloor].SetColor(selectionColor);
                //selectedBlocks.Add(i + j * blocksPerFloor);
            }
        }
        Debug.Log(string.Format("rangeIndex: {0} sectionIndex: {1}", rangeIndex, sectionIndex));

        prevSelection[0] = sectionLowerBound;
        prevSelection[1] = rangeIndex;
    }

    public void ClearSelectedBlocks()
    {
        //foreach (int i in selectedBlocks)
        //{
        //    blocks[i].SetColor(defaultColor);
        //}
        selectedBlocks.Clear();
    }

}
