using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiCanvasManager : MonoBehaviour
{
    [SerializeField]
    private GameObject openUnitSelectionButton;
    [SerializeField]
    private GameObject selectUnitType;
    [SerializeField]
    private GameObject selectLocation;
    [SerializeField]
    private GameObject submissionForm;
    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private string[] unitTypes;


    //singleton
    private static UiCanvasManager _instance;
    public static UiCanvasManager Instance { get { return _instance; } }
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

    //TODO: animation transitions
    // Start is called before the first frame update
    void Start()
    {
        //setup selectUnitType child objects
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
        button.onClick.AddListener(SelectUnitTypeState);

        //setup selectLocation child objects
        button = GetChildWithName(selectLocation, "ButtonBack").GetComponent<Button>();
        button.onClick.AddListener(SelectUnitTypeState);
        button = GetChildWithName(selectLocation, "ButtonConfirm").GetComponent<Button>();
        button.onClick.AddListener(SubmissionFormState);

        //setup submissionForm child objects
        button = GetChildWithName(submissionForm, "ButtonCancel").GetComponent<Button>();
        button.onClick.AddListener(SelectUnitTypeState);
        button = GetChildWithName(submissionForm, "ButtonSubmit").GetComponent<Button>();
        button.onClick.AddListener(CloseAll);

        openUnitSelectionButton.SetActive(true);
        selectUnitType.SetActive(false);
        selectLocation.SetActive(false);
        submissionForm.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CloseAll()
    {
        openUnitSelectionButton.SetActive(true);
        selectUnitType.SetActive(false);
        selectLocation.SetActive(false);
        submissionForm.SetActive(false);
    }

    public void SelectUnitTypeState()
    {
        BuildingBlockManager.Instance.UnitTypeSelection = 0;
        BuildingBlockManager.Instance.SelectionLock = false;
        openUnitSelectionButton.SetActive(false);
        selectUnitType.SetActive(true);
        selectLocation.SetActive(false);
        submissionForm.SetActive(false);
    }
    public void SelectLocationState()
    {
        BuildingBlockManager.Instance.ConfirmationLock = false;
        openUnitSelectionButton.SetActive(false);
        selectUnitType.SetActive(false);
        selectLocation.SetActive(true);
        submissionForm.SetActive(false);
    }

    public void SubmissionFormState()
    {
        BuildingBlockManager.Instance.ConfirmationLock = true;
        openUnitSelectionButton.SetActive(false);
        selectUnitType.SetActive(false);
        selectLocation.SetActive(false);
        submissionForm.SetActive(true);
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
