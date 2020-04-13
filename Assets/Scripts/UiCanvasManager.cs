using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiCanvasManager : MonoBehaviour
{
    [SerializeField]
    private GameObject selectLocation;
    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private string[] unitTypes;
    [SerializeField]
    private DialogManager dialogManager;

    public static UiCanvasManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    //TODO: animation transitions
    // Start is called before the first frame update
    void Start()
    {
        /*//setup selectUnitType child objects
        GameObject unitTypeButtons = GetChildWithName(selectUnitType, "UnitTypeButtons");
        for (int i = 0; i < unitTypes.Length; i++)
        {
            int unitType = i + 1;
            GameObject unitTypeButton = Instantiate(buttonPrefab, unitTypeButtons.transform, false);
            unitTypeButton.GetComponent<RectTransform>().localPosition = new Vector3(0, i * -37, 0);
            unitTypeButton.GetComponent<Button>().onClick.AddListener(() => { BuildingBlockManager.Instance.UnitTypeSelection = unitType; });
            Text text = GetChildWithName(unitTypeButton, "Text").GetComponent<Text>();
            text.text = unitTypes[i];
        }
        Button button = GetChildWithName(selectUnitType, "ButtonCancel").GetComponent<Button>();
        button.onClick.AddListener(CloseAll);

        //setup selectLocation child objects
        button = GetChildWithName(selectLocation, "ButtonBack").GetComponent<Button>();
        button.onClick.AddListener(SelectUnitTypeState);
        button = GetChildWithName(selectLocation, "ButtonConfirm").GetComponent<Button>();
        button.onClick.AddListener(SubmissionFormState);

        //setup submissionForm child objects
        button = GetChildWithName(submissionForm, "ButtonCancel").GetComponent<Button>();
        button.onClick.AddListener(SelectUnitTypeState);
        button = GetChildWithName(submissionForm, "ButtonSubmit").GetComponent<Button>();
        button.onClick.AddListener(BuildingBlockManager.Instance.PublishConfirmedBlocks);
        button.onClick.AddListener(CloseAll);

        openUnitSelectionButton.SetActive(true);
        selectUnitType.SetActive(false);
        selectLocation.SetActive(false);
        submissionForm.SetActive(false);
        //*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    GameObject GetChildWithName(GameObject obj, string name)
    {
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(name);
        if (childTrans != null)
        {
            return childTrans.gameObject;
        }
        else
        {
            return null;
        }
    }
}
