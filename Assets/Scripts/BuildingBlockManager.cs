using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingBlockManager : MonoBehaviour
{
    [SerializeField]
    private BlockPrefab blockPrefab;
    [SerializeField]
    private int xBlockCount = 1;
    [SerializeField]
    private int yBlockCount = 1;
    [SerializeField]
    private float[] xOffset = { 1.0f };
    [SerializeField]
    private float[] yOffset = { 1.0f };

    //runtime reference to blocks
    private List<BlockPrefab> blocks = new List<BlockPrefab>();
    /*//sine simulation
    private float amplitudeX = 5.0f;
    private float amplitudeY = 5.0f;
    private float omegaX = 1.0f;
    private float omegaY = 5.0f;
    //*/
    [SerializeField]
    private Color[] unitTypeColor;

    //selector vars
    private int unitTypeSelection = 0;
    public int UnitTypeSelection
    {
        get { 
            return this.unitTypeSelection; 
        }
        set
        {
            this.unitTypeSelection = value;
            UiCanvasManager.Instance.SelectLocationState();
            //print(string.Format("Selection {0} set", this.unitTypeSelection));
        }
    }
    private bool selectionLock;
    public bool SelectionLock {
        get 
        {
            return this.selectionLock;
        }
        set
        {
            if (value)
                foreach (int i in selectedBlocks)
                    blocks[i].SetColor(confirmedColor);
            else
                if (!ConfirmationLock)
                    ClearSelectedBlocks();
            this.selectionLock = value;
        }
    }
    public bool ConfirmationLock { get; set; }
    private int unitIdCount;
    private HashSet<int> selectedBlocks = new HashSet<int>();
    private HashSet<int> confirmedBlocks = new HashSet<int>();
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
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        unitIdCount = 0; //TODO: retrieve from player pref
        SelectionLock = false;
        ConfirmationLock = false;
        if (blockPrefab != null)
        {
            float xPos = .0f;
            float yPos = .0f;
            for (int y = 0; y < yBlockCount; y++)
            {
                for (int x = 0; x < xBlockCount; x++)
                {
                    BlockPrefab block = Instantiate(blockPrefab, new Vector3(xPos, yPos, 0), Quaternion.identity, this.transform);
                    block.Index = x + xBlockCount * y;
                    block.SetColor(defaultColor);
                    blocks.Add(block);
                    xPos += xOffset[x % xOffset.Length];
                }
                xPos = .0f;
                yPos += yOffset[y % yOffset.Length];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*//sine simulation
        for (int y = 0; y < yBlockCount; y++)
        {
            for (int x = 0; x < xBlockCount; x++)
            {
                int i = x + xBlockCount * y;
                blocks[i].Offset += Time.deltaTime;
                blocks[i].gameObject.transform.localPosition = blocks[i].Position + new Vector3(0, 0, amplitudeX * Mathf.Cos(omegaX * blocks[i].Offset));
            }
        }
        //*/
        
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
                Debug.Log(hit.transform.gameObject.name);
                switch (hit.collider.tag)
                {
                    case "BuildingBlock":
                        int blockIndex = hit.transform.gameObject.GetComponent<BlockPrefab>().Index;
                        SelectUnitBlocks(blockIndex);
                        break;
                    case "Button":
                        confirmedBlocks = new HashSet<int>(selectedBlocks);
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
        switch (UnitTypeSelection)
        {
            case 1:
                blocksLeft = 1;
                break;
            case 2:
            case 3:
                blocksLeft = 2;
                break;
            case 4:
            case 5:
                blocksLeft = 3;
                break;
            default:
                return;
        }
        blocksLeft = UnitTypeSelection;

        int row = blockIndex / xBlockCount;
        int col = blockIndex - row * xBlockCount;
        //Debug.Log(string.Format("row: {0} col: {1}", row, col));

        //int midIndex = (UnitTypeSelection % 2 == 0) ? UnitTypeSelection / 2 - 1 : UnitTypeSelection / 2;
        selectedBlocks.Add(blockIndex);
        blocksLeft--;
        for (int i = 1; i < xBlockCount; i++) 
        {
            if (blocksLeft < 1) break;  //sanity check- breaks the 1 unit case;
            int blockColIndexToAdd = col + i;
            if (blockColIndexToAdd < xBlockCount)
            {
                int indexToAdd = blockColIndexToAdd + row * xBlockCount;
                if (blocks[indexToAdd].UnitId < 0)
                {
                    selectedBlocks.Add(indexToAdd);
                    blocksLeft--;
                    if (blocksLeft < 1) break;
                }
            }

            blockColIndexToAdd = col - i;
            if (blockColIndexToAdd >= 0)
            {
                int indexToAdd = blockColIndexToAdd + row * xBlockCount;
                if (blocks[indexToAdd].UnitId < 0)
                {
                    selectedBlocks.Add(indexToAdd);
                    blocksLeft--;
                    if (blocksLeft < 1) break;
                }
            }
        }
        if (blocksLeft < 1)
        {
            foreach (int i in selectedBlocks)
                blocks[i].SetColor(selectionColor);
            if (Input.GetMouseButtonUp(0))
                SelectionLock = true;
        }
        //else no space :(
    }

    public void ClearSelectedBlocks()
    {
        foreach (int i in selectedBlocks)
        {
            blocks[i].SetColor(defaultColor);
        }
        selectedBlocks.Clear();
    }

    public void SetConfirmedBlocks()
    {
        confirmedBlocks = new HashSet<int>(selectedBlocks);
    }

    public void PublishConfirmedBlocks()
    {
        //add to player pref
        //set block unit color
        foreach (int i in selectedBlocks)
        {
            blocks[i].UnitId = unitIdCount++;
            blocks[i].SetColor(unitTypeColor[UnitTypeSelection - 1]);
        }
        selectedBlocks.Clear();
        confirmedBlocks.Clear();
    }
}
