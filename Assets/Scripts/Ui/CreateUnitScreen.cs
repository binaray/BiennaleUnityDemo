using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateUnitScreen : MonoBehaviour
{
    private string[] questionTexts = {
        "Choose your preferred living arrangement:",
        "What is your age group?",
        "How many people are staying including yourself?",
        "Do you want it affordable?",
        "Select your required rooms"
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
                });
                buttonNo = questions[questionNum].transform.GetChild(1);
                buttonNo.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectedAffordable = ButtonState.Unselected;
                });
                break;
            default:
                break;
        }
    }

    void Refresh()
    {
        currentQuestionNum = 0;
        foreach (GameObject q in questions)
            q.SetActive(false);
        SelectedLivingArrangement = LivingArrangement.None;
        SelectedAgeGroup = AgeGroup.None;
        SelectedAffordable = ButtonState.None;
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
            }
            else if (newQuestionNum < questionCount - 1)
            {
                nextButton.localPosition = new Vector3(100, 0, 0);
                prevButton.localPosition = new Vector3(-100, 0, 0);
                nextButton.gameObject.SetActive(true);
                prevButton.gameObject.SetActive(true);
            }
            else
            {
                prevButton.localPosition = new Vector3(0, 0, 0);
                nextButton.gameObject.SetActive(false);
                prevButton.gameObject.SetActive(true);
            }
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
    None,
    Selected,
    Unselected
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

[System.Serializable]
public class QuestionResults
{
    public string livingArrangement;
    public string ageGroup;
    public int pax;
    public bool affordable;
    public Dictionary<string, int> roomsRequired = new Dictionary<string, int>()
    {
        { RequiredRooms.SingleBedroom.ToString(), 0 },
        { RequiredRooms.SharedBedroom.ToString(), 0 },
        { RequiredRooms.Study.ToString(), 0 }
    };
    public string[] location = new string[2];
}