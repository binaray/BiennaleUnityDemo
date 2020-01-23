using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private float amplitudeX = 5.0f;
    private float amplitudeY = 5.0f;
    private float omegaX = 1.0f;
    private float omegaY = 5.0f;
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
                ClearSelectedBlocks();
            this.selectionLock = value;
        }
    }
    public bool ConfirmationLock { get; set; }
    private HashSet<int> selectedBlocks = new HashSet<int>();
    //TODO: decide where to put this
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
                    block.Id = x + xBlockCount * y;
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
        //for (int y = 0; y < yBlockCount; y++)
        //{
        //    for (int x = 0; x < xBlockCount; x++)
        //    {
        //        int i = x + xBlockCount * y;
        //        blocks[i].Offset += Time.deltaTime;
        //        blocks[i].gameObject.transform.localPosition = blocks[i].Position + new Vector3(0, 0, amplitudeX * Mathf.Cos(omegaX * blocks[i].Offset));
        //    }
        //}
        if (Input.GetMouseButtonDown(0))
        {
            confirmedBlocks = new HashSet<int>(selectedBlocks); /// TODO::come back here tomorrow
            SelectionLock = false;
        }

        if (!SelectionLock && !ConfirmationLock)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                int blockIndex = hit.transform.gameObject.GetComponent<BlockPrefab>().Id;
                SelectUnitBlocks(blockIndex);
                Debug.DrawLine(ray.origin, hit.point);
                
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
        Debug.Log(string.Format("row: {0} col: {1}", row, col));

        //int midIndex = (UnitTypeSelection % 2 == 0) ? UnitTypeSelection / 2 - 1 : UnitTypeSelection / 2;
        selectedBlocks.Add(blockIndex);
        blocksLeft--;
        for (int i = 1; i < xBlockCount; i++) 
        {
            if (blocksLeft < 1) break;  //sanity check- breaks the 1 unit case;
            int blockColIndexToAdd = col + i;
            if (blockColIndexToAdd < xBlockCount)
            {
                selectedBlocks.Add(blockColIndexToAdd + row * xBlockCount);
                blocksLeft--;
                if (blocksLeft < 1) break;
            }

            blockColIndexToAdd = col - i;
            if (blockColIndexToAdd >= 0)
            {
                selectedBlocks.Add(blockColIndexToAdd + row * xBlockCount);
                blocksLeft--;
                if (blocksLeft < 1) break;
            }
        }

        foreach (int i in selectedBlocks)
        {
            blocks[i].SetColor(selectionColor);
        }

        if (Input.GetMouseButtonUp(0))
        {
            SelectionLock = true;
        }
    }

    public void ClearSelectedBlocks()
    {
        foreach (int i in selectedBlocks)
        {
            blocks[i].SetColor(defaultColor);
        }
        selectedBlocks.Clear();
    }
}
