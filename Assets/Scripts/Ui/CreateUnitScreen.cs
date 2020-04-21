using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Non-dynamic class handler for questions. Add new questions by adding child into questionView and setup in QuestionSetup
/// </summary>
public class CreateUnitScreen : MonoBehaviour
{
    private string[] questionTexts = {
        "Choose your preferred living arrangement:",
        "What is your age group?",
        "How many people are staying including yourself?",
        "Do you want it affordable?",
        "Select your required rooms. \nNote you can have a maximum total of 6.",
        "Select your preferred residential location.",
        "Vote for up to 3 preferred shared spaces."
    };
    [SerializeField]
    private TMPro.TextMeshProUGUI questionTextMesh;
    [SerializeField]
    private GameObject questionView;
    private int questionCount = 0;
    [SerializeField]
    private float questionTransitionTime = 0.2f;
    private bool transitionLock = false;

    [SerializeField]
    private RectTransform nextButton;
    [SerializeField]
    private RectTransform prevButton;
    [SerializeField]
    private RectTransform submitButton;

    private int currentQuestionNum = 0;
    private List<GameObject> questions = new List<GameObject>();

    //general UI properties
    private Dictionary<ButtonState, Color> buttonStateColors = new Dictionary<ButtonState, Color>()
    {
        { ButtonState.None, new Color(.75f, .75f, .75f, 1)},
        { ButtonState.Selected, new Color(0, 0, 0, 1)},
        { ButtonState.Unselected, new Color(.75f, .75f, .75f, .5f)}
    };

    //q0 params
    private List<Transform> q0ButtonTranforms = new List<Transform>();
    private LivingArrangement _selectedLivingArrangement;
    public LivingArrangement SelectedLivingArrangement {
        get
        {
            return _selectedLivingArrangement;
        }
        private set
        {
            //Debug.LogWarning(((LivingArrangement)value).ToString());
            if (value != LivingArrangement.None)
                for (int i = 0; i < q0ButtonTranforms.Count; i++)
                {
                    if (i == (int)value)
                        SetButtonState(q0ButtonTranforms[i], ButtonState.Selected);
                    else
                        SetButtonState(q0ButtonTranforms[i], ButtonState.Unselected);
                }
            else
                foreach (Transform button in q0ButtonTranforms)
                    SetButtonState(button, ButtonState.None);
            switch (value)
            {
                case LivingArrangement.Single:
                    SelectedPax = 1;
                    skipQ2Flag = true;
                    break;
                case LivingArrangement.CoupleWoChildren:
                    SelectedPax = 2;
                    skipQ2Flag = true;
                    break;
                default:
                case LivingArrangement.AssistedLiving:
                case LivingArrangement.FlatshareCoHousing:
                case LivingArrangement.SingleParentFamily:
                    q2Slider.value = 2;
                    q2Slider.minValue = 2;
                    skipQ2Flag = false;
                    break;
                case LivingArrangement.NuclearFamily:
                case LivingArrangement.MultigenerationalExtended:
                    q2Slider.value = 3;
                    q2Slider.minValue = 3;
                    skipQ2Flag = false;
                    break;
            }
            Debug.LogWarning(string.Format("Living arranement pax: {0}", SelectedPax));
            _selectedLivingArrangement = value;
        }
    }

    //q1 params
    private List<Transform> q1ButtonTranforms = new List<Transform>();
    private AgeGroup _selectedAgeGroup;
    public AgeGroup SelectedAgeGroup
    {
        get
        {
            return _selectedAgeGroup;
        }
        private set
        {
            if (value != AgeGroup.None)
                for (int i = 0; i < q1ButtonTranforms.Count; i++)
                {
                    if (i == (int)value)
                        SetButtonState(q1ButtonTranforms[i], ButtonState.Selected);
                    else
                        SetButtonState(q1ButtonTranforms[i], ButtonState.Unselected);
                }
            else
                foreach (Transform button in q1ButtonTranforms)
                    SetButtonState(button, ButtonState.None);
            _selectedAgeGroup = value;
        }
    }

    //q2 params
    Slider q2Slider;
    public int SelectedPax { get; private set; }
    private bool skipQ2Flag = false;

    //q3 params
    Transform buttonYes, buttonNo;
    private ButtonState _selectedAffordable;
    public ButtonState SelectedAffordable   //Button state is used here as a tristate for boolean.
    {
        get
        {
            return _selectedAffordable;
        }
        private set
        {
            if (value == ButtonState.Selected) 
            {
                SetButtonState(buttonYes, ButtonState.Selected);
                SetButtonState(buttonNo, ButtonState.Unselected);
            }
            else if (value == ButtonState.Unselected)
            {
                SetButtonState(buttonYes, ButtonState.Unselected);
                SetButtonState(buttonNo, ButtonState.Selected);
            }
            else
            {
                SetButtonState(buttonYes, ButtonState.None);
                SetButtonState(buttonNo, ButtonState.None);
            }
            _selectedAffordable = value;
        }
    }

    //q4 params //NO GETTER SETTER FOR THIS. STATIC IMPLEMENTATION IN SETUP
    private int maxRoomCount = 6;
    TMPro.TMP_Dropdown singleBedroomsDropdown;
    TMPro.TMP_Dropdown sharedBedroomsDropdown;
    TMPro.TMP_Dropdown studyroomsDropdown;
    public Dictionary<RequiredRooms, int> SelectedRequiredRooms = new Dictionary<RequiredRooms, int>()
    {
        { RequiredRooms.SingleBedroom, 0 },
        { RequiredRooms.SharedBedroom, 0 },
        { RequiredRooms.Study, 0 }
    };

    //q5 params
    //int rowCount = 4, colCount = 3;
    string[] rowSt = { "1-8","9-16","17-24","25-34" };
    string[] colSt = { "left", "mid", "right" };
    List<Transform> q5ButtonTranforms = new List<Transform>();
    private int _selectedLocation;
    public int SelectedLocation {
        get
        {
            return _selectedLocation;
        }
        private set
        {
            if (value > -1)
            {
                for (int i = 0; i < q5ButtonTranforms.Count; i++)
                {
                    if (i == value)
                        SetQ5ButtonState(q5ButtonTranforms[i], true);
                    else
                        SetQ5ButtonState(q5ButtonTranforms[i], false);
                }
            }
            else
                foreach (Transform button in q5ButtonTranforms)
                    SetQ5ButtonState(button, false);
            //Debug.LogWarning("Selected location index: " + value);
            _selectedLocation = value;
        }
    }   //1 dimensional index representing 2d index

    //q6 params //NO GETTER SETTER FOR THIS. STATIC IMPLEMENTATION IN SETUP
    private int maxSharedSpaceSelectionCount = 3;
    private HashSet<SharedSpace> SelectedSharedSpaces = new HashSet<SharedSpace>();
    private List<Transform> q6ButtonTransforms = new List<Transform>();

    // To prevent bug where things get accessed before OnEnable is called
    void Awake()
    {
        questionCount = questionView.transform.childCount;
        for (int i = 0; i < questionCount; ++i)
        {
            questions.Add(questionView.transform.GetChild(i).gameObject);
            QuestionSetup(i);
        }
        Refresh();
        //Debug.LogError(UiCanvasManager.Instance.GetComponent<RectTransform>().GetComponent<RectTransform>().sizeDelta.x);
    }

    void QuestionSetup(int questionNum)
    {
        switch (questionNum)
        {
            case 0:
                for (int i = 0; i < questions[questionNum].transform.childCount; i++)
                {
                    int tempInt = i;
                    Transform button = questions[questionNum].transform.GetChild(tempInt);
                    button.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SelectedLivingArrangement = (LivingArrangement)tempInt;
                        //Debug.LogWarning(((LivingArrangement)tempInt).ToString());
                        NextQuestion();
                    });
                    q0ButtonTranforms.Add(button);
                }
                break;
            case 1:
                for (int i = 0; i < questions[questionNum].transform.childCount; i++)
                {
                    int tempInt = i;
                    Transform button = questions[questionNum].transform.GetChild(tempInt);
                    button.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SelectedAgeGroup = (AgeGroup)tempInt;
                        NextQuestion();
                    });
                    q1ButtonTranforms.Add(button);
                }
                break;
            case 2:
                TMPro.TextMeshProUGUI txt = questions[questionNum].transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
                q2Slider = questions[questionNum].transform.GetChild(1).GetComponent<Slider>();
                q2Slider.onValueChanged.AddListener((value) => 
                {
                    txt.text = value.ToString();
                    SelectedPax = (int)value;
                    Debug.LogWarning(string.Format("Selected pax: {0}", SelectedPax));
                });
                break;
            case 3:
                buttonYes = questions[questionNum].transform.GetChild(0);
                buttonYes.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectedAffordable = ButtonState.Selected;
                    NextQuestion();
                });
                buttonNo = questions[questionNum].transform.GetChild(1);
                buttonNo.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectedAffordable = ButtonState.Unselected;
                    NextQuestion();
                });
                break;
            case 4:
                {
                    singleBedroomsDropdown = questions[questionNum].transform.GetChild(0).GetComponent<TMPro.TMP_Dropdown>();
                    sharedBedroomsDropdown = questions[questionNum].transform.GetChild(1).GetComponent<TMPro.TMP_Dropdown>();
                    studyroomsDropdown = questions[questionNum].transform.GetChild(2).GetComponent<TMPro.TMP_Dropdown>();

                    singleBedroomsDropdown.onValueChanged.AddListener((value) =>
                    {
                        int sharedBedroomsMax = maxRoomCount - value - studyroomsDropdown.value;
                        List<TMPro.TMP_Dropdown.OptionData> aData = new List<TMPro.TMP_Dropdown.OptionData>();
                        for (int i = 0; i < sharedBedroomsMax + 1; i++)
                            aData.Add(new TMPro.TMP_Dropdown.OptionData(i.ToString()));
                        sharedBedroomsDropdown.options = aData;
                        sharedBedroomsDropdown.value = SelectedRequiredRooms[RequiredRooms.SharedBedroom];

                        int studyroomMax = maxRoomCount - value - sharedBedroomsDropdown.value;
                        List<TMPro.TMP_Dropdown.OptionData> bData = new List<TMPro.TMP_Dropdown.OptionData>();
                        for (int i = 0; i < studyroomMax + 1; i++)
                            bData.Add(new TMPro.TMP_Dropdown.OptionData(i.ToString()));
                        studyroomsDropdown.options = bData;
                        studyroomsDropdown.value = SelectedRequiredRooms[RequiredRooms.Study];
                        SelectedRequiredRooms[RequiredRooms.SingleBedroom] = value;
                    });
                    sharedBedroomsDropdown.onValueChanged.AddListener((value) =>
                    {
                        int singleBedroomsMax = maxRoomCount - value - studyroomsDropdown.value;
                        List<TMPro.TMP_Dropdown.OptionData> newData = new List<TMPro.TMP_Dropdown.OptionData>();
                        for (int i = 0; i < singleBedroomsMax + 1; i++)
                            newData.Add(new TMPro.TMP_Dropdown.OptionData(i.ToString()));
                        singleBedroomsDropdown.options = newData;
                        singleBedroomsDropdown.value = SelectedRequiredRooms[RequiredRooms.SingleBedroom];

                        int studyroomMax = maxRoomCount - value - singleBedroomsDropdown.value;
                        List<TMPro.TMP_Dropdown.OptionData> bData = new List<TMPro.TMP_Dropdown.OptionData>();
                        for (int i = 0; i < studyroomMax + 1; i++)
                            bData.Add(new TMPro.TMP_Dropdown.OptionData(i.ToString()));
                        studyroomsDropdown.options = bData;
                        studyroomsDropdown.value = SelectedRequiredRooms[RequiredRooms.Study];
                        SelectedRequiredRooms[RequiredRooms.SharedBedroom] = value;
                    });
                    studyroomsDropdown.onValueChanged.AddListener((value) =>
                    {
                        int singleBedroomsMax = maxRoomCount - value - sharedBedroomsDropdown.value;
                        List<TMPro.TMP_Dropdown.OptionData> newData = new List<TMPro.TMP_Dropdown.OptionData>();
                        for (int i = 0; i < singleBedroomsMax + 1; i++)
                            newData.Add(new TMPro.TMP_Dropdown.OptionData(i.ToString()));
                        singleBedroomsDropdown.options = newData;
                        singleBedroomsDropdown.value = SelectedRequiredRooms[RequiredRooms.SingleBedroom];

                        int sharedBedroomsMax = maxRoomCount - value - singleBedroomsDropdown.value;
                        List<TMPro.TMP_Dropdown.OptionData> bData = new List<TMPro.TMP_Dropdown.OptionData>();
                        for (int i = 0; i < sharedBedroomsMax + 1; i++)
                            bData.Add(new TMPro.TMP_Dropdown.OptionData(i.ToString()));
                        sharedBedroomsDropdown.options = bData;
                        sharedBedroomsDropdown.value = SelectedRequiredRooms[RequiredRooms.SharedBedroom];
                        SelectedRequiredRooms[RequiredRooms.Study] = value;
                    });
                }
                break;
            case 5:
                for (int i = 0; i < questions[questionNum].transform.childCount; i++)
                {
                    int tempInt = i;
                    Transform button = questions[questionNum].transform.GetChild(tempInt);
                    button.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SelectedLocation = tempInt;
                        NextQuestion();
                    });
                    q5ButtonTranforms.Add(button);
                }
                break;
            case 6:
                for (int i = 0; i < questions[questionNum].transform.childCount; i++)
                {
                    int tempInt = i;
                    Transform button = questions[questionNum].transform.GetChild(tempInt);
                    button.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if (SelectedSharedSpaces.Contains((SharedSpace)tempInt))
                        {
                            SelectedSharedSpaces.Remove((SharedSpace)tempInt);
                            SetQ5ButtonState(button, false);
                        }
                        else
                        {
                            if (SelectedSharedSpaces.Count < maxSharedSpaceSelectionCount)
                            {
                                SelectedSharedSpaces.Add((SharedSpace)tempInt);
                                SetQ5ButtonState(button, true);
                            }
                        }
                    });
                    q6ButtonTransforms.Add(button);
                }
                break;
            default:
                break;
        }
    }



    void SetQ5ButtonState(Transform button,bool state)
    {
        if (state)
        {
            button.GetChild(0).GetComponent<RawImage>().color = Color.white;
        }
        else
        {
            button.GetChild(0).GetComponent<RawImage>().color = Color.black;
        }
    }

    public void SubmitData()
    {
        QuestionResults userInput = new QuestionResults();
        userInput.livingArrangement = SelectedLivingArrangement.ToString();
        userInput.ageGroup = SelectedAgeGroup.ToString();
        userInput.pax = SelectedPax;
        userInput.affordable = (SelectedAffordable == 0 ? false : true);
        userInput.requiredRooms[RequiredRooms.SingleBedroom.ToString()] = SelectedRequiredRooms[RequiredRooms.SingleBedroom];
        userInput.requiredRooms[RequiredRooms.SharedBedroom.ToString()] = SelectedRequiredRooms[RequiredRooms.SharedBedroom];
        userInput.requiredRooms[RequiredRooms.Study.ToString()] = SelectedRequiredRooms[RequiredRooms.Study];
        int row = SelectedLocation / colSt.Length;
        int col = SelectedLocation - row * colSt.Length + 1;
        userInput.location[0] = rowSt[row]; //row
        userInput.location[1] = colSt[col]; //col
        foreach (SharedSpace ss in SelectedSharedSpaces)
            userInput.preferredSharedSpaces.Add(ss.ToString());
        string inputJson = Newtonsoft.Json.JsonConvert.SerializeObject(userInput);
        Debug.LogError(inputJson);
        ConnectionManager.Instance.UploadUserInput(inputJson);
    }

    void Refresh()
    {
        currentQuestionNum = 0;
        for (int i = 0; i < questions.Count; i++)
        {
            questions[i].SetActive(false);
        }
        SelectedLivingArrangement = LivingArrangement.None;
        SelectedAgeGroup = AgeGroup.None;
        //q2Slider's value is set in SelectedLivingArrangement so we don't have to call refresh it again here;
        SelectedAffordable = ButtonState.None;
        singleBedroomsDropdown.value = 0;
        sharedBedroomsDropdown.value = 0;
        studyroomsDropdown.value = 0;
        SelectedLocation = -1;
        SelectedSharedSpaces.Clear();
        foreach (Transform t in q6ButtonTransforms)
            SetQ5ButtonState(t, false);
        transitionLock = false;
    }

    private void SetButtonState(Transform button, ButtonState state)
    {
        RawImage img = button.GetChild(0).GetComponent<RawImage>();
        GameObject txt = button.GetChild(1).gameObject;

        img.color = buttonStateColors[state];
        if (state == ButtonState.Selected)
            txt.SetActive(true);
        else
            txt.SetActive(false);
    }

    void ChangeQuestion(int newQuestionNum)
    {
        if (!transitionLock)
        {
            questionTextMesh.text = questionTexts[newQuestionNum];
            StartCoroutine(QuestionTransition(newQuestionNum));
            if (newQuestionNum < 1)
            {
                nextButton.localPosition = new Vector3(0, 0, 0);
                nextButton.gameObject.SetActive(true);
                prevButton.gameObject.SetActive(false);
                submitButton.gameObject.SetActive(false);
            }
            else if (newQuestionNum < questionCount - 1)
            {
                nextButton.localPosition = new Vector3(100, 0, 0);
                prevButton.localPosition = new Vector3(-100, 0, 0);
                nextButton.gameObject.SetActive(true);
                prevButton.gameObject.SetActive(true);
                submitButton.gameObject.SetActive(false);
            }
            else
            {
                prevButton.localPosition = new Vector3(0, 0, 0);
                nextButton.gameObject.SetActive(false);
                prevButton.gameObject.SetActive(true);
                submitButton.gameObject.SetActive(true);
            }
            //Debug.LogWarning("single: "+ SelectedRequiredRooms[RequiredRooms.SingleBedroom]);
            //Debug.LogWarning("shared: " + SelectedRequiredRooms[RequiredRooms.SharedBedroom]);
            //Debug.LogWarning("study:  " + SelectedRequiredRooms[RequiredRooms.Study]);
        }
    }

    IEnumerator QuestionTransition(int newQuestionNum)
    {
        //Debug.LogWarning(currentQuestionNum);
        //Debug.LogWarning(newQuestionNum);
        transitionLock = true;
        float canvasWidth = UiCanvasManager.Instance.GetComponent<RectTransform>().GetComponent<RectTransform>().sizeDelta.x;
        RectTransform r0 = questions[currentQuestionNum].GetComponent<RectTransform>();
        RectTransform r1 = questions[newQuestionNum].GetComponent<RectTransform>();
        Vector3 r0Target;
        Vector3 r1Target = new Vector3(0, 0, 0);

        if (newQuestionNum >= currentQuestionNum)
        {
            //new question slides in from right, old question exits to the left
            r0Target = new Vector3(-canvasWidth, 0, 0);
            r1.localPosition = new Vector3(canvasWidth, 0, 0);
        }
        else
        {
            //new question slides in from left, old question exits to the right
            r0Target = new Vector3(canvasWidth, 0, 0);
            r1.localPosition = new Vector3(-canvasWidth, 0, 0);
        }

        r1.gameObject.SetActive(true);
        Vector3 dx = (r1Target - r1.localPosition) / questionTransitionTime;
        
        float totalTime = 0;
        while (totalTime <= questionTransitionTime)
        {
            if (r0 != r1)
                r0.localPosition += dx * Time.deltaTime;
            r1.localPosition += dx * Time.deltaTime;
            totalTime += Time.deltaTime;
            yield return null;
        }
        r1.localPosition = r1Target;
        if (newQuestionNum != currentQuestionNum)
        {
            r0.gameObject.SetActive(false);
            currentQuestionNum = newQuestionNum;
        }
        transitionLock = false;
    }

    public void NextQuestion()
    {
        if (currentQuestionNum < questionCount - 1)
            ChangeQuestion(currentQuestionNum + 1);
    }

    public void PreviousQuestion()
    {
        if (currentQuestionNum > 0)
            ChangeQuestion(currentQuestionNum - 1);
    }

    void OnEnable()
    {
        //clear inputs
        //reset gui
        Refresh();
        //goto question 0
        ChangeQuestion(0);
    }
}

public enum ButtonState
{
    None = -1,
    Unselected,
    Selected
}

public enum LivingArrangement
{
    None = -1,
    Single,
    CoupleWoChildren,
    SingleParentFamily,
    NuclearFamily,
    AssistedLiving,
    FlatshareCoHousing,
    MultigenerationalExtended
}

public enum AgeGroup
{
    None = -1,
    Youth,
    Midlife,
    Elderly
}

public enum RequiredRooms
{
    SingleBedroom,
    SharedBedroom,
    Study
}

public enum SharedSpace
{
    Retail,
    SharedLivingRoom,
    Playscape,
    EventSpace,
    MakerSpace,
    EbikeStation,
    HealthClinic,
    Farm,
    CoWorking,
    SharedKitchen,
    SensoryGarden,
    Fitness
}

[System.Serializable]
public class QuestionResults
{
    public string livingArrangement;
    public string ageGroup;
    public int pax;
    public bool affordable;
    public Dictionary<string, int> requiredRooms = new Dictionary<string, int>()
    {
        { RequiredRooms.SingleBedroom.ToString(), 0 },
        { RequiredRooms.SharedBedroom.ToString(), 0 },
        { RequiredRooms.Study.ToString(), 0 }
    };
    public string[] location = new string[2];
    public List<string> preferredSharedSpaces = new List<string>();
}