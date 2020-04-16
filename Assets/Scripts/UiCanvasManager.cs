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
        MenuScreen
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

    public void ChangeState(GameState newState)
    {
        if (newState != GameState.ScanQrScreen)
            lastRecentState = newState;
        Debug.LogWarning("Last recent state: " + lastRecentState);
        currentState = newState;
        StartCoroutine(BgStateTransition(newState, isCompleted => {
            StartCoroutine(newState.ToString() + "State");
        }));
    }

    public void ParcelationVisScreen()
    {
        ChangeState(GameState.ParcelationVisScreen);
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
