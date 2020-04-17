using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateUnitScreen : MonoBehaviour
{
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

    //q0 params
    private Dictionary<int, string> livingArrangement = new Dictionary<int, string>()
    {
        {0, "single"},
        {1, "coupleW/oChildren"},
        {2, "singleParentFamily"},
        {3, "nuclearFamily"},
        {4, "assistedLiving"},
        {5, "flatshare/coHousing"},
        {6, "nuclearFamily"}
    };
    private Dictionary<string, Color> buttonStateColors = new Dictionary<string, Color>()
    {
        { "default", new Color(.75f, .75f, .75f, 1)},
        { "selected", new Color(0, 0, 0, 1)},
        { "unselected", new Color(.5f, .5f, .5f, .5f)}
    };

    // To prevent bug where things get accessed before OnEnable is called
    void Awake()
    {
        questionCount = questionView.transform.childCount;
        for (int i = 0; i < questionCount; ++i)
        {
            questions.Add(questionView.transform.GetChild(i).gameObject);
        }
        Refresh();
        //Debug.LogError(UiCanvasManager.Instance.GetComponent<RectTransform>().GetComponent<RectTransform>().sizeDelta.x);
    }

    void Refresh()
    {
        currentQuestionNum = 0;
        foreach (GameObject q in questions)
        {
            q.SetActive(false);
        }
        transitionLock = false;
    }

    void ChangeQuestion(int newQuestionNum)
    {
        if (!transitionLock)
        {
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
        Debug.LogWarning(currentQuestionNum);
        Debug.LogWarning(newQuestionNum);
        transitionLock = true;
        float canvasWidth = UiCanvasManager.Instance.GetComponent<RectTransform>().GetComponent<RectTransform>().sizeDelta.x;
        RectTransform r0 = questions[currentQuestionNum].GetComponent<RectTransform>();
        RectTransform r1 = questions[newQuestionNum].GetComponent<RectTransform>();
        Vector3 r0Target;
        Vector3 r1Target = new Vector3(0, 0, 0); ;

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
