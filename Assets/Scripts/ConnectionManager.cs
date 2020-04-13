using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField]
    private float updateDuration = 5.0f;
    [SerializeField]
    private int maxRetries = 3;
    private UserInput userInput;
    public int SuggestedUnitTypeIndex { get; set; }
    private bool isConnecting = false;

    public static ConnectionManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    public void StartNewUserInput(string avatarName, int avatarIndex, int pax, int sharedSpaceIndex)
    {
        userInput = new UserInput(avatarName, avatarIndex, pax, sharedSpaceIndex);
        SuggestedUnitTypeIndex = Constants.PaxToUnitTypeIndex(pax);
    }
    public void SetUserInputLocation(int unitTypeIndex, int floorRangeIndex, int sectionIndex)
    {
        userInput.unitTypeIndex = unitTypeIndex;
        userInput.floorRangeIndex = floorRangeIndex;
        userInput.sectionIndex = sectionIndex;
    }

    public void UploadUserInput()
    {
        StartCoroutine(IEUploadUserInput(callback: result =>
        {
            Debug.LogWarning(result);
            UserInputResult res = Newtonsoft.Json.JsonConvert.DeserializeObject<UserInputResult>(result);
            Debug.LogError(res.unitId);
            //Debug.LogError(res.state);
            BuildingStateManager.Instance.UpdateBuildingState(res.state);
        }));
    }

    void Start()
    {
        StartCoroutine(IERetrieveServerState());
    }

    private IEnumerator IERetrieveServerState()
    {
        yield return new WaitForSeconds(updateDuration);
        if (!isConnecting)
        {
            isConnecting = true;
            WWWForm form = new WWWForm();

            UnityWebRequest www = UnityWebRequest.Post(Constants.ServerEnpoint + "state.php", form);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                // TODO: error handle if return is invalid
                Debug.Log("Server state response: "+ www.downloadHandler.text);
                if (string.IsNullOrEmpty(www.downloadHandler.text))
                    BuildingStateManager.Instance.ClearBuildingState();
                else
                {
                    Dictionary<int, Unit> newState = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, Unit>>(www.downloadHandler.text);
                    BuildingStateManager.Instance.UpdateBuildingState(newState);
                }
                StartCoroutine(IERetrieveServerState());
            }
            isConnecting = false;
        }
        StartCoroutine(IERetrieveServerState());
    }

    private IEnumerator IEUploadUserInput(System.Action<string> callback = null)
    {
        yield return new WaitForSeconds(1.0f);
        if (!isConnecting)
        {
            isConnecting = true;
            WWWForm form = new WWWForm();
            Debug.LogWarning(JsonUtility.ToJson(userInput));
            form.AddField("UserInput", JsonUtility.ToJson(userInput));

            UnityWebRequest www = UnityWebRequest.Post(Constants.ServerEnpoint + "input.php", form);
            //UnityWebRequest www = UnityWebRequest.Post("https://ptsv2.com/t/biennale2020/post", form);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                StartCoroutine(IEUploadUserInput());
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);
                callback(www.downloadHandler.text);
            }
            isConnecting = false;
        }
        else StartCoroutine(IEUploadUserInput());
    }
}
