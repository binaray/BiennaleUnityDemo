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
    //private BlockPrefab[,,] blocks;
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
    public bool ConfirmationLock { get; set; }
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

    //singleton
    private static BuildingBlockManager _instance;
    public static BuildingBlockManager Instance { get { return _instance; } }    
    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
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
        ConfirmationLock = false;
        if (blockPrefab != null)
        {
            float xPos = .0f;
            float yPos = .0f;
            for (int y = 0; y < floors; y++)
            {
                for (int x = 0; x < blocksPerFloor; x++)
                {
                    BlockPrefab block = Instantiate(blockPrefab, new Vector3(xPos, yPos, 0), Quaternion.identity, this.transform);
                    block.Index = x + blocksPerFloor * y;
                    block.SetColor(defaultColor);
                    blocks.Add(block);
                    xPos += xOffset[x % xOffset.Length];
                }
                xPos = .0f;
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

        if (!SelectionLock && !ConfirmationLock)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
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
        int blocksLeft;
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
        blocksLeft = UnitTypeSelection;

        int row = blockIndex / blocksPerFloor;
        int col = blockIndex - row * blocksPerFloor;
        int rangeIndex = row / floorRange;
        string section;
        int floorLowerBound = rangeIndex * floorRange;
        int sectionLowerBound;
        int sectionUpperBound;

        if (col < leftSection)
        {
            section = "left";
            sectionLowerBound = 0;
            sectionUpperBound = leftSection;
        }
        else if (col < leftSection + middleSection)
        {
            section = "middle";
            sectionLowerBound = leftSection;
            sectionUpperBound = leftSection + middleSection;
        }
        else
        {
            section = "right";
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
                }
                else
                    blocks[i + j * blocksPerFloor].SetColor(selectionColor);
                //selectedBlocks.Add(i + j * blocksPerFloor);
            }
        }
        Debug.LogError(string.Format("rangeIndex: {0} section: {1}", rangeIndex, section));

        prevSelection[0] = sectionLowerBound;
        prevSelection[1] = rangeIndex;

        ///TO IMPORT TO SERVER SIDE
        //int midIndex = (UnitTypeSelection % 2 == 0) ? UnitTypeSelection / 2 - 1 : UnitTypeSelection / 2;
        //if (blocks[blockIndex].UnitId < 0)
        //{
        //    selectedBlocks.Add(blockIndex);
        //    blocksLeft--;
        //}
        //else return;

        //bool canInsertLeft = true;
        //bool canInsertRight = true;

        //for (int i = 1; i < blocksPerFloor; i++) 
        //{
        //    if (blocksLeft < 1 || (!canInsertRight && !canInsertLeft)) break;  //sanity check- breaks the 1 unit case;

        //    //try inserting to the right
        //    int blockColIndexToAdd = col + i;
        //    if (blockColIndexToAdd < blocksPerFloor && canInsertRight)
        //    {
        //        int indexToAdd = blockColIndexToAdd + row * blocksPerFloor;
        //        if (blocks[indexToAdd].UnitId < 0)
        //        {
        //            selectedBlocks.Add(indexToAdd);
        //            blocksLeft--;
        //            if (blocksLeft < 1) break;
        //        }
        //        else canInsertRight = false;
        //    }
        //    else canInsertRight = false;

        //    //try inserting to the left
        //    blockColIndexToAdd = col - i;
        //    if (blockColIndexToAdd >= 0 && canInsertLeft)
        //    {
        //        int indexToAdd = blockColIndexToAdd + row * blocksPerFloor;
        //        if (blocks[indexToAdd].UnitId < 0)
        //        {
        //            selectedBlocks.Add(indexToAdd);
        //            blocksLeft--;
        //            if (blocksLeft < 1) break;
        //        }
        //        else canInsertLeft = false;
        //    }
        //    else canInsertLeft = false;
        //}
        //if (blocksLeft < 1)
        //{
        //    foreach (int i in selectedBlocks)
        //        blocks[i].SetColor(selectionColor);
        //    if (Input.GetMouseButtonUp(0))
        //        SelectionLock = true;
        //}
        //else no space :(
    }

    public void ClearSelectedBlocks()
    {
        //foreach (int i in selectedBlocks)
        //{
        //    blocks[i].SetColor(defaultColor);
        //}
        selectedBlocks.Clear();
    }

    public void SetConfirmedBlocks()
    {
        confirmedBlocks = new SortedSet<int>(selectedBlocks);
    }

    public void PublishConfirmedBlocks()
    {
        //add to player pref
        //set block unit color
        Unit unit = new Unit(confirmedBlocks.Min, UnitTypeSelection, unitIdCount++, "Hello!");
        AddUnit(unit);
        selectedBlocks.Clear();
        confirmedBlocks.Clear();
    }

    void AddUnit(Unit unit)
    {
        //for (int i = 0; i < UnitTypeSelection; i++)
        //{
        //    int blockIdToShade = unit.blockIndex + i;
        //    blocks[blockIdToShade].UnitId = unitIdCount;
        //    blocks[blockIdToShade].SetColor(unitTypeColor[UnitTypeSelection - 1]);
        //}
        //unitWrapper.occupiedUnits.Add(unit);
        //unitIdCount++;
        //foreach (int i in confirmedBlocks)
        //{
        //    blocks[i].UnitId = unitIdCount++;
        //    blocks[i].SetColor(unitTypeColor[UnitTypeSelection - 1]);
        //}
        string json = JsonUtility.ToJson(ownState);
        Debug.LogError(json);
        PlayerPrefs.SetInt(UNIT_ID_COUNT_PREF, unitIdCount);
        PlayerPrefs.SetString(UNIT_DB_STRING_PREF, json);
    }
}
