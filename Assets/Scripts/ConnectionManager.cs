using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionManager : MonoBehaviour
{
    private string PARCELATION_URL = "http://127.0.0.1:5000/parcelation";

    [SerializeField]
    private float updateDuration = 5.0f;
    [SerializeField]
    private int maxRetries = 5;
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

            UnityWebRequest www = UnityWebRequest.Get(PARCELATION_URL);
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
                    Debug.LogError("Error");
                List<BuildingUnit> newState = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUnit>>(www.downloadHandler.text);
                foreach (BuildingUnit u in newState)
                {
                    Debug.LogWarning(u.ToString());
                }
                ParcelationManager.Instance.UpdateParcelation(newState);
            }
            isConnecting = false;
        }
        StartCoroutine(IERetrieveServerState());
    }

    public void UploadUserInput(string jsonInput)
    {
        Debug.LogError(jsonInput);
        StartCoroutine(IEUploadUserInput(jsonInput, callback: result =>
        {
            Debug.LogWarning(result);
            BuildingUnit res = Newtonsoft.Json.JsonConvert.DeserializeObject<BuildingUnit>(result);
            //Debug.LogError(res.unitId);
            //Debug.LogError(res.state);
            //BuildingStateManager.Instance.UpdateBuildingState(res.state);
            UiCanvasManager.Instance.CongratulatoryScreen();
        }));
    }

    private IEnumerator IEUploadUserInput(string jsonInput, int retries = 0, System.Action<string> callback = null)
    {
        if (!isConnecting)
        {
            isConnecting = true;
            WWWForm form = new WWWForm();
            form.AddField("UserInput", jsonInput);

            //UnityWebRequest www = UnityWebRequest.Post(Constants.ServerEnpoint + "input.php", form);
            UnityWebRequest www = UnityWebRequest.Post(PARCELATION_URL, form);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                StartCoroutine(IEUploadUserInput(jsonInput, ++retries));
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);
                callback(www.downloadHandler.text);
            }
            isConnecting = false;
        }
        else if (retries > maxRetries)
        {
            callback(null);
        }
        else
        {
            yield return new WaitForSeconds(.5f);
            StartCoroutine(IEUploadUserInput(jsonInput, retries));
        }
    }
}

[System.Serializable]
public class InputResult
{
    InputResult()
    {

    }
}