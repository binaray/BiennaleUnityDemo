using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiCanvasManager : MonoBehaviour
{
    public enum GameState
    {
        ScanQrScreen,
        StartScreen,
        ParcelationVisScreen,
        MenuScreen,
        CreateUnitScreen,
        CongratulatoryScreen
    }
    [HideInInspector]
    public GameState currentState;
    [HideInInspector]
    public GameState lastRecentState;  //used as a saved state to return when qr screen pops up
                                        //return state is handled by ArTrackableEventHandler

    [SerializeField]
    private GameObject background;
    private int bgState = 0;
    [SerializeField]
    private float bgTransitionTime = .5f;
    private IEnumerator BgStateTransition(GameState newBgState, System.Action<bool> callback)
    {
        RawImage r = background.GetComponent<RawImage>();
        float currentBlur = r.material.GetFloat("_Size");
        Vector4 currentAdditive_Color = r.material.GetColor("_AdditiveColor");
        Vector4 currentMultiply_Color = r.material.GetColor("_MultiplyColor");
        float targetBlur;
        Vector4 targetAdditive_Color;
        Vector4 targetMultiplyColor;

        switch (newBgState)
        {
            case GameState.ParcelationVisScreen:
            case GameState.CongratulatoryScreen:
                targetBlur = 0;
                targetAdditive_Color = new Color(0, 0, 0);
                targetMultiplyColor = new Color(1, 1, 1);
                break;
            case GameState.MenuScreen:
            case GameState.ScanQrScreen:
                targetBlur = 1;
                targetAdditive_Color = new Color(0.5f, 0.5f, 0.5f);
                targetMultiplyColor = new Color(0.5f, 0.5f, 0.5f);
                break;
            default:
                targetBlur = 3;
                targetAdditive_Color = new Color(0.5f, 0.5f, 0.5f);
                targetMultiplyColor = new Color(0.5f, 0.5f, 0.5f);
                break;
            case GameState.StartScreen:
                targetBlur = 30;
                targetAdditive_Color = new Color(0.6f, 0.6f, 0.6f);
                targetMultiplyColor = new Color(0.5f, 0.5f, 0.5f);
                break;
        }
        float deltaBlur = (targetBlur - currentBlur) / bgTransitionTime;
        Vector4 deltaAdditive_Color = (targetAdditive_Color - currentAdditive_Color) / bgTransitionTime;
        Vector4 deltaMultiply_Color = (targetMultiplyColor - currentMultiply_Color) / bgTransitionTime;

        float totalTime = 0;
        while (totalTime <= bgTransitionTime)
        {
            //countdownImage.fillAmount = totalTime / bgTransitionTime;
            r.material.SetFloat("_Size", currentBlur += deltaBlur * Time.deltaTime);
            r.material.SetColor("_AdditiveColor", currentAdditive_Color += deltaAdditive_Color * Time.deltaTime);
            r.material.SetColor("_MultiplyColor", currentMultiply_Color += deltaMultiply_Color * Time.deltaTime);
            totalTime += Time.deltaTime;
            yield return null;
        }
        r.material.SetFloat("_Size", targetBlur);
        r.material.SetColor("_AdditiveColor", targetAdditive_Color);
        r.material.SetColor("_MultiplyColor", targetMultiplyColor);
        Debug.Log("UI background changed");
        callback(true);
    }

    [SerializeField]
    private GameObject startScreen;
    [SerializeField]
    private GameObject scanQrScreen;
    [SerializeField]
    private GameObject parcelationVisScreen;
    [SerializeField]
    private GameObject menuScreen;
    [SerializeField]
    private GameObject createUnitScreen;
    [SerializeField]
    private GameObject congratulatoryScreen;

    private bool transitionLock = false;

    public static UiCanvasManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        //Turn off any stray screens left on from debugging
        int screenCount = background.transform.childCount;
        for (int i = 0; i < screenCount; ++i)
        {
            background.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    IEnumerator ScanQrScreenState()
    {
        scanQrScreen.SetActive(true);
        while (currentState == GameState.ScanQrScreen)
        {
            yield return null;
        }
        scanQrScreen.SetActive(false);
    }

    IEnumerator StartScreenState()
    {
        startScreen.SetActive(true);
        while (currentState == GameState.StartScreen)
        {
            yield return null;
        }
        startScreen.SetActive(false);
    }

    IEnumerator ParcelationVisScreenState()
    {
        parcelationVisScreen.SetActive(true);
        while (currentState == GameState.ParcelationVisScreen)
        {
            yield return null;
        }
        parcelationVisScreen.SetActive(false);
    }

    IEnumerator MenuScreenState()
    {
        menuScreen.SetActive(true);
        Animator anim = menuScreen.GetComponent<Animator>();
        anim.SetBool("isMenuShown", true);
        while (currentState == GameState.MenuScreen)
        {
            yield return null;
        }
        transitionLock = true;
        anim.SetBool("isMenuShown", false);

        //yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length + anim.GetCurrentAnimatorStateInfo(0).normalizedTime); //--does not work as intended
        //[hack] because animation speed is negative we use the ! here
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("MenuScreenExit"))
        {
            yield return null;
        }
        menuScreen.SetActive(false);
        transitionLock = false;
    }

    IEnumerator CreateUnitScreenState()
    {
        createUnitScreen.SetActive(true);
        while (currentState == GameState.CreateUnitScreen)
        {
            yield return null;
        }
        createUnitScreen.SetActive(false);
    }

    IEnumerator CongratulatoryScreenState()
    {
        congratulatoryScreen.SetActive(true);
        while (currentState == GameState.CongratulatoryScreen)
        {
            yield return null;
        }
        congratulatoryScreen.SetActive(false);
    }

    public void ChangeState(GameState newState)
    {
        if (transitionLock) return;
        if (newState != GameState.ScanQrScreen)
            lastRecentState = newState;
        //Debug.LogWarning("Last recent state: " + lastRecentState);
        currentState = newState;
        StartCoroutine(BgStateTransition(newState, isCompleted => {
            StartCoroutine(newState.ToString() + "State");
        }));
        Debug.Log("Current state: " + currentState);
    }

    public void StartScreen()
    {
        ChangeState(GameState.StartScreen);
    }

    public void ParcelationVisScreen()
    {
        ChangeState(GameState.ParcelationVisScreen);
    }

    public void MenuScreen()
    {
        ChangeState(GameState.MenuScreen);
    }

    public void CreateUnitScreen()
    {
        ChangeState(GameState.CreateUnitScreen);
    }

    public void CongratulatoryScreen()
    {
        ChangeState(GameState.CongratulatoryScreen);
    }
    
}
