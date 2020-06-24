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
        { ButtonState.None, new Color(0, 0, 0, 0.75f)},
        { ButtonState.Selected, new Color(1, 0, 0, 1)},
        { ButtonState.Unselected, new Color(.75f, .75f, .75f, .5f)}
    };
    private List<bool> isQuestionDone = new List<bool>();

    //q0 params
    private static string[] cohabitationSt =
    {
        "Single",
        "Couple without children",
        "Single Parent family",
        "Nuclear family",
        "Assisted Living",
        "Flatshare / Co-housing",
        "Multi-generational / Extended family"
    };
    TMPro.TextMeshProUGUI q0StDisp;
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
            {
                for (int i = 0; i < q0ButtonTranforms.Count; i++)
                {
                    if (i == (int)value)
                    {
                        print("lv =" + value.ToString());
                        SetButtonState(q0ButtonTranforms[i], ButtonState.Selected);
                        q0StDisp.text = cohabitationSt[i];
                    }
                    else
                        SetButtonState(q0ButtonTranforms[i], ButtonState.Unselected);
                }
            }
            else
            {
                q0StDisp.text = "";
                foreach (Transform button in q0ButtonTranforms)
                    SetButtonState(button, ButtonState.None);
            }
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
                case LivingArrangement.Assisted:
                case LivingArrangement.Flatshare:
                case LivingArrangement.SingleParent:
                    q2Slider.value = 2;
                    q2Slider.minValue = 2;
                    skipQ2Flag = false;
                    break;
                case LivingArrangement.Nuclear:
                case LivingArrangement.Multi:
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
    private static string[] AgeGroupSt =
    {
        "YOUTH & ADULT | <40",
        "MIDLIFE | 40-60",
        "ELDERLY | >60"
    };
    TMPro.TextMeshProUGUI q1StDisp;
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
            {
                for (int i = 0; i < q1ButtonTranforms.Count; i++)
                {
                    if (i == (int)value)
                    {
                        SetButtonState(q1ButtonTranforms[i], ButtonState.Selected);
                        q1StDisp.text = AgeGroupSt[i];
                        Debug.LogWarning(AgeGroupSt[i]);
                    }
                    else
                        SetButtonState(q1ButtonTranforms[i], ButtonState.Unselected);
                }
            }
            else
            {
                q1StDisp.text = "";
                Debug.LogWarning("AgeGroupSt unset");
                foreach (Transform button in q1ButtonTranforms)
                    SetButtonState(button, ButtonState.None);
            }
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
                SetTextButtonState(buttonYes, ButtonState.Selected);
                SetTextButtonState(buttonNo, ButtonState.Unselected);
            }
            else if (value == ButtonState.Unselected)
            {
                SetTextButtonState(buttonYes, ButtonState.Unselected);
                SetTextButtonState(buttonNo, ButtonState.Selected);
            }
            else
            {
                SetTextButtonState(buttonYes, ButtonState.None);
                SetTextButtonState(buttonNo, ButtonState.None);
            }
            _selectedAffordable = value;
        }
    }

    //q4 params //NO GETTER SETTER FOR THIS. STATIC IMPLEMENTATION IN SETUP
    private int maxRoomCount = 6;
    TMPro.TMP_Dropdown singleBedroomsDropdown, sharedBedroomsDropdown, studyroomsDropdown;
    List<Transform> singleBedroomButtons = new List<Transform>(), sharedBedroomButtons = new List<Transform>(), studyroomButtons = new List<Transform>();
    private int _singleBedroomCount = 0;
    int SingleBedroomCount {
        get { return _singleBedroomCount; }
        set
        {
            int singleBedroomsMax = maxRoomCount - SharedBedroomCount - StudyRoomCount;
            for (int i = 0; i < singleBedroomButtons.Count; i++)
            {
                int tempInt = i;
                if (tempInt == value)
                {
                    SetQ4ButtonState(singleBedroomButtons[tempInt], ButtonState.Selected);
                }
                else if (tempInt <= singleBedroomsMax)
                {
                    SetQ4ButtonState(singleBedroomButtons[tempInt], ButtonState.None);
                }
                else
                {
                    SetQ4ButtonState(singleBedroomButtons[tempInt], ButtonState.Unselected);
                }
            }
            int sharedBedroomsMax = maxRoomCount - value - StudyRoomCount;
            for (int i = 0; i < sharedBedroomButtons.Count; i++)
            {
                int tempInt = i;
                if (tempInt == SharedBedroomCount)
                {
                    SetQ4ButtonState(sharedBedroomButtons[tempInt], ButtonState.Selected);
                }
                else if (tempInt <= sharedBedroomsMax)
                {
                    SetQ4ButtonState(sharedBedroomButtons[tempInt], ButtonState.None);
                }
                else {
                    SetQ4ButtonState(sharedBedroomButtons[tempInt], ButtonState.Unselected);
                }
            }
            int studyroomMax = maxRoomCount - value - SharedBedroomCount;
            for (int i = 0; i < studyroomButtons.Count; i++)
            {
                int tempInt = i;
                if (tempInt == StudyRoomCount)
                {
                    SetQ4ButtonState(studyroomButtons[tempInt], ButtonState.Selected);
                }
                else if (tempInt <= studyroomMax)
                {
                    SetQ4ButtonState(studyroomButtons[tempInt], ButtonState.None);
                }
                else
                {
                    SetQ4ButtonState(studyroomButtons[tempInt], ButtonState.Unselected);
                }
            }
            print(value);
            _singleBedroomCount = value;
        }
    }
    private int _sharedBedroomCount = 0;
    int SharedBedroomCount
    {
        get { return _sharedBedroomCount; }
        set
        {
            int sharedBedroomsMax = maxRoomCount - SingleBedroomCount - StudyRoomCount;
            for (int i = 0; i < sharedBedroomButtons.Count; i++)
            {
                int tempInt = i;
                if (tempInt == value)
                {
                    SetQ4ButtonState(sharedBedroomButtons[tempInt], ButtonState.Selected);
                }
                else if (tempInt <= sharedBedroomsMax)
                {
                    SetQ4ButtonState(sharedBedroomButtons[tempInt], ButtonState.None);
                }
                else
                {
                    SetQ4ButtonState(sharedBedroomButtons[tempInt], ButtonState.Unselected);
                }
            }
            int singleBedroomMax = maxRoomCount - value - StudyRoomCount;
            for (int i = 0; i < singleBedroomButtons.Count; i++)
            {
                int tempInt = i;
                if (tempInt == SingleBedroomCount)
                {
                    SetQ4ButtonState(singleBedroomButtons[tempInt], ButtonState.Selected);
                }
                else if (tempInt <= singleBedroomMax)
                {
                    SetQ4ButtonState(singleBedroomButtons[tempInt], ButtonState.None);
                }
                else
                {
                    SetQ4ButtonState(singleBedroomButtons[tempInt], ButtonState.Unselected);
                }
            }
            int studyroomMax = maxRoomCount - value - SingleBedroomCount;
            for (int i = 0; i < studyroomButtons.Count; i++)
            {
                int tempInt = i;
                if (tempInt == StudyRoomCount)
                {
                    SetQ4ButtonState(studyroomButtons[tempInt], ButtonState.Selected);
                }
                else if (tempInt <= studyroomMax)
                {
                    SetQ4ButtonState(studyroomButtons[tempInt], ButtonState.None);
                }
                else
                {
                    SetQ4ButtonState(studyroomButtons[tempInt], ButtonState.Unselected);
                }
            }
            print(value);
            _sharedBedroomCount = value;
        }
    }
    private int _studyRoomCount = 0;
    int StudyRoomCount
    {
        get { return _studyRoomCount; }
        set
        {
            int studyRoomsMax = maxRoomCount - SingleBedroomCount - SharedBedroomCount;
            for (int i = 0; i < studyroomButtons.Count; i++)
            {
                int tempInt = i;
                if (tempInt == value)
                {
                    SetQ4ButtonState(studyroomButtons[tempInt], ButtonState.Selected);
                }
                else if (tempInt <= studyRoomsMax)
                {
                    SetQ4ButtonState(studyroomButtons[tempInt], ButtonState.None);
                }
                else
                {
                    SetQ4ButtonState(studyroomButtons[tempInt], ButtonState.Unselected);
                }
            }
            int singleBedroomMax = maxRoomCount - value - SharedBedroomCount;
            for (int i = 0; i < singleBedroomButtons.Count; i++)
            {
                int tempInt = i;
                if (tempInt == SingleBedroomCount)
                {
                    SetQ4ButtonState(singleBedroomButtons[tempInt], ButtonState.Selected);
                }
                else if (tempInt <= singleBedroomMax)
                {
                    SetQ4ButtonState(singleBedroomButtons[tempInt], ButtonState.None);
                }
                else
                {
                    SetQ4ButtonState(singleBedroomButtons[tempInt], ButtonState.Unselected);
                }
            }
            int sharedBedroomsMax = maxRoomCount - value - SingleBedroomCount;
            for (int i = 0; i < sharedBedroomButtons.Count; i++)
            {
                int tempInt = i;
                if (tempInt == SharedBedroomCount)
                {
                    SetQ4ButtonState(sharedBedroomButtons[tempInt], ButtonState.Selected);
                }
                else if (tempInt <= sharedBedroomsMax)
                {
                    SetQ4ButtonState(sharedBedroomButtons[tempInt], ButtonState.None);
                }
                else
                {
                    SetQ4ButtonState(sharedBedroomButtons[tempInt], ButtonState.Unselected);
                }
            }
            print(value);
            _studyRoomCount = value;
        }
    }


    public Dictionary<RequiredRooms, int> SelectedRequiredRooms = new Dictionary<RequiredRooms, int>()
    {
        { RequiredRooms.SingleBedroom, 0 },
        { RequiredRooms.SharedBedroom, 0 },
        { RequiredRooms.Study, 0 }
    };

    //q5 params
    //int rowCount = 4, colCount = 3;
    string[] rowSt = { "0","1","2","3" };
    string[] colSt = { "0", "1", "2" };
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
    private List<TMPro.TextMeshProUGUI> ssCounterText = new List<TMPro.TextMeshProUGUI>();

    // To prevent bug where things get accessed before OnEnable is called
    public static CreateUnitScreen Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
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
                q0StDisp = questions[questionNum].transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
                for (int i = 1; i < questions[questionNum].transform.childCount; i++)
                {
                    int tempInt = i;
                    Transform button = questions[questionNum].transform.GetChild(tempInt);
                    button.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SelectedLivingArrangement = (LivingArrangement)(tempInt-1);
                        //Debug.LogWarning(((LivingArrangement)tempInt).ToString());
                        //NextQuestion();
                        isQuestionDone[0] = true;
                        ToggleNav(0);
                    });
                    q0ButtonTranforms.Add(button);
                }
                isQuestionDone.Add(false);
                break;
            case 1:
                q1StDisp = questions[questionNum].transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
                for (int i = 1; i < questions[questionNum].transform.childCount; i++)
                {
                    int tempInt = i;
                    Transform button = questions[questionNum].transform.GetChild(tempInt);
                    button.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SelectedAgeGroup = (AgeGroup)(tempInt-1);
                        isQuestionDone[1] = true;
                        ToggleNav(1);
                        //NextQuestion();
                    });
                    q1ButtonTranforms.Add(button);
                }
                isQuestionDone.Add(false);
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
                isQuestionDone.Add(true);
                break;
            case 3:
                buttonYes = questions[questionNum].transform.GetChild(0);
                buttonYes.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectedAffordable = ButtonState.Selected;
                    isQuestionDone[3] = true;
                    ToggleNav(3);
                    //NextQuestion();
                });
                buttonNo = questions[questionNum].transform.GetChild(1);
                buttonNo.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectedAffordable = ButtonState.Unselected;
                    isQuestionDone[3] = true;
                    ToggleNav(3);
                    //NextQuestion();
                });
                isQuestionDone.Add(false);
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

                    for (int i = 1; i < questions[questionNum].transform.GetChild(3).childCount; i++)
                    {
                        singleBedroomButtons.Add(questions[questionNum].transform.GetChild(3).GetChild(i));
                        sharedBedroomButtons.Add(questions[questionNum].transform.GetChild(4).GetChild(i));
                        studyroomButtons.Add(questions[questionNum].transform.GetChild(5).GetChild(i));
                    }

                    for (int i = 0; i < singleBedroomButtons.Count; i++)
                    {
                        int tempInt = i;
                        singleBedroomButtons[tempInt].GetChild(0).GetComponent<Button>().onClick.AddListener(() => { SingleBedroomCount = tempInt; });
                        sharedBedroomButtons[tempInt].GetChild(0).GetComponent<Button>().onClick.AddListener(() => { SharedBedroomCount = tempInt; });
                        studyroomButtons[tempInt].GetChild(0).GetComponent<Button>().onClick.AddListener(() => { StudyRoomCount = tempInt; });
                    }
                }
                isQuestionDone.Add(true);
                break;
            case 5:
                for (int i = 0; i < questions[questionNum].transform.GetChild(1).childCount; i++)
                {
                    int tempInt = i;
                    Transform button = questions[questionNum].transform.GetChild(1).GetChild(tempInt);
                    button.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SelectedLocation = tempInt;
                        isQuestionDone[5] = true;
                        ToggleNav(5);
                        //NextQuestion();
                    });
                    q5ButtonTranforms.Add(button);
                }
                isQuestionDone.Add(false);
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
                            SetQ6ButtonState(button, ButtonState.Unselected);
                        }
                        else
                        {
                            if (SelectedSharedSpaces.Count < maxSharedSpaceSelectionCount)
                            {
                                SelectedSharedSpaces.Add((SharedSpace)tempInt);
                                SetQ6ButtonState(button, ButtonState.Selected);
                            }
                        }
                    });
                    q6ButtonTransforms.Add(button);
                    ssCounterText.Add(button.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>());
                }
                isQuestionDone.Add(true);
                break;
            default:
                break;
        }
    }
    void SetQ5ButtonState(Transform button, bool state)
    {
        if (state)
        {
            button.GetChild(0).GetComponent<RawImage>().color = new Color(1, 0, 0, 0.5803922f);
        }
        else
        {
            button.GetChild(0).GetComponent<RawImage>().color = Color.clear;
        }
    }
    void SetQ6ButtonState(Transform button,ButtonState state)
    {
        //if (state==ButtonState.Selected)
        {
            button.GetChild(0).GetComponent<RawImage>().color = buttonStateColors[state];
            try
            {
                button.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().color = buttonStateColors[state];
                button.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().color = buttonStateColors[state];
            }
            catch (System.Exception e) { }
        }
        //else
        //{
        //    button.GetChild(0).GetComponent<RawImage>().color = Color.black;
        //    try
        //    {
        //        button.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().color = Color.black;
        //        button.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().color = Color.black;
        //    }
        //    catch (System.Exception e) { }
        //}
    }

    public void UpdateQ6Counters(Dictionary<string, int> newCount)
    {
        foreach (KeyValuePair<string, int> item in newCount)
        {
            SharedSpace ss;
            if (System.Enum.TryParse<SharedSpace>(item.Key, out ss))
            {
                int i = (int)ss;
                if (i < ssCounterText.Count)
                {
                    ssCounterText[i].text = string.Format("{0} votes", item.Value);
                }
            }

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
        //bug fix to allow algorithm to allocate rooms
        if (SelectedRequiredRooms[RequiredRooms.SingleBedroom] + SelectedRequiredRooms[RequiredRooms.SharedBedroom] + SelectedRequiredRooms[RequiredRooms.Study] == 0)
            SelectedRequiredRooms[RequiredRooms.SingleBedroom] = 1;
        int row = SelectedLocation / colSt.Length;
        int col = SelectedLocation - row * colSt.Length;
        //print(string.Format("[{0},{1}]", row, col));
        userInput.location[0] = rowSt[row]; //row
        userInput.location[1] = colSt[col]; //col
        foreach (SharedSpace ss in SelectedSharedSpaces)
            userInput.preferredSharedSpaces.Add(ss.ToString());
        string inputJson = Newtonsoft.Json.JsonConvert.SerializeObject(userInput);
        ConnectionManager.Instance.UploadUserInput(inputJson);
        //UiCanvasManager.Instance.CongratulatoryScreen();
    }

    void Refresh()
    {
        currentQuestionNum = 0;
        for (int i = 0; i < questions.Count; i++)
        {
            questions[i].SetActive(false);
            if (i == 2 || i == 4 || i == 6)
                isQuestionDone[i] = true;
            else
                isQuestionDone[i] = false;
        }
        SelectedLivingArrangement = LivingArrangement.None;
        SelectedAgeGroup = AgeGroup.None;
        //q2Slider's value is set in SelectedLivingArrangement so we don't have to call refresh it again here;
        SelectedAffordable = ButtonState.None;
        singleBedroomsDropdown.value = 0;
        sharedBedroomsDropdown.value = 0;
        studyroomsDropdown.value = 0;
        SingleBedroomCount = 0;
        SharedBedroomCount = 0;
        StudyRoomCount = 0;
        SelectedLocation = -1;
        SelectedSharedSpaces.Clear();
        foreach (Transform t in q6ButtonTransforms)
            SetQ6ButtonState(t, ButtonState.Unselected);
        transitionLock = false;
    }
    
    private void SetShowTextButtonState(Transform button, ButtonState state)
    {
        RawImage img = button.GetChild(0).GetComponent<RawImage>();
        TMPro.TextMeshProUGUI txt = button.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        img.color = buttonStateColors[state];
        txt.color = buttonStateColors[state];
    }

    private void SetTextButtonState(Transform button, ButtonState state)
    {
        TMPro.TextMeshProUGUI txt = button.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        txt.color= buttonStateColors[state];
    }

    private void SetButtonState(Transform button, ButtonState state)
    {
        RawImage img = button.GetChild(0).GetComponent<RawImage>();
        //GameObject txt = button.GetChild(1).gameObject;
        print(img);
        img.color = buttonStateColors[state];
        //if (state == ButtonState.Selected)
        //    txt.SetActive(true);
        //else
        //    txt.SetActive(false);
    }

    private void SetQ4ButtonState(Transform button, ButtonState state)
    {
        //RawImage img = button.GetChild(0).GetComponent<RawImage>();
        //GameObject txt = button.GetChild(1).gameObject;
        button.transform.GetComponent<TMPro.TextMeshProUGUI>().color = buttonStateColors[state];
        if (state == ButtonState.None) button.transform.GetChild(0).gameObject.SetActive(true);
        else button.transform.GetChild(0).gameObject.SetActive(false);
    }

    void ToggleNav(int newQuestionNum)
    {
        if (isQuestionDone[newQuestionNum])
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
        else
            if (newQuestionNum < 1)
            {
                nextButton.gameObject.SetActive(false);
                prevButton.gameObject.SetActive(false);
                submitButton.gameObject.SetActive(false);
            }
            else if (newQuestionNum < questionCount - 1)
            {
                prevButton.localPosition = new Vector3(0, 0, 0);
                nextButton.gameObject.SetActive(false);
                prevButton.gameObject.SetActive(true);
                submitButton.gameObject.SetActive(false);
            }
            else
            {
                prevButton.localPosition = new Vector3(0, 0, 0);
                nextButton.gameObject.SetActive(false);
                prevButton.gameObject.SetActive(true);
                submitButton.gameObject.SetActive(false);
            }

    }

    void ChangeQuestion(int newQuestionNum)
    {
        if (!transitionLock)
        {
            if (newQuestionNum == 2 && skipQ2Flag)
            {
                if (currentQuestionNum < 2)
                    newQuestionNum += 1;
                else
                    newQuestionNum -= 1;
            }
            questionTextMesh.text = questionTexts[newQuestionNum];
            StartCoroutine(QuestionTransition(newQuestionNum));
            ToggleNav(newQuestionNum);
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
    SingleParent,
    Nuclear,
    Assisted,
    Flatshare,
    Multi
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
    Cafeteria,
    CommunityFarm,
    FitnessCentre,
    SportsHall,
    Lounge,
    Salon,
    Library,
    Tailor,
    Market,
    Playscape,
    PlayRoom,
    Restaurant,
    MultiGenCenter,
    HealthcareClinic,
    Makerspace,
    Childcare
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

    public override string ToString()
    {
        return string.Join(", ", preferredSharedSpaces);
    }
}