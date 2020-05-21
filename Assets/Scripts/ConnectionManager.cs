using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionManager : MonoBehaviour
{
    private string PARCELATION_URL = "http://127.0.0.1:5000/parcelation";
    private string MESSAGES_URL = "http://127.0.0.1:5000/message";

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
        StartCoroutine(IEGetMessages());
    }

    private IEnumerator IERetrieveServerState()
    {        
        UnityWebRequest www = UnityWebRequest.Get(PARCELATION_URL);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            if (string.IsNullOrEmpty(www.downloadHandler.text))
                Debug.LogError("No parcelation found..");
            else
            {
                //Debug.LogWarning(result);
                GetParcelationResult result = Newtonsoft.Json.JsonConvert.DeserializeObject<GetParcelationResult>(www.downloadHandler.text);
                List<BuildingUnit> newState = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUnit>>(result.parcelation);
                Dictionary<string, int> ssCount = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(result.ssCount);
                //foreach (BuildingUnit u in newState)
                //{
                //    Debug.LogWarning(u.ToString());
                //}
                ParcelationManager.Instance.UpdateParcelation(newState);
                CreateUnitScreen.Instance.UpdateQ6Counters(ssCount);
            }
        }
        yield return new WaitForSeconds(updateDuration);
        StartCoroutine(IERetrieveServerState());
    }

    public void UploadUserInput(string jsonInput)
    {
        //Debug.LogError(jsonInput);
        //Show loader
        StartCoroutine(IEUploadUserInput(jsonInput, callback: result =>
        {
            Debug.LogWarning(result);
            InputResult res = Newtonsoft.Json.JsonConvert.DeserializeObject<InputResult>(result);
            List<BuildingUnit> newState = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUnit>>(res.parcelation);
            ParcelationManager.Instance.currentUserId = res.userId;
            Debug.LogWarning("UserId: " + res.userId.ToString());
            ParcelationManager.Instance.UpdateParcelation(newState);
            //Hide loader overlay
            UiCanvasManager.Instance.CongratulatoryScreen();
        }));
    }

    private IEnumerator IEUploadUserInput(string jsonInput, int retries = 0, System.Action<string> callback = null)
    {
        if (!isConnecting)
        {
            Debug.LogError("Uploading: " + jsonInput);
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

    private IEnumerator IEGetMessages()
    {
        UnityWebRequest www = UnityWebRequest.Get(MESSAGES_URL);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            if (string.IsNullOrEmpty(www.downloadHandler.text))
                Debug.LogError("No messages found..");
            else
            {
                Debug.LogWarning(www.downloadHandler.text);
                List<MessageTopic> newMessageTopics = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MessageTopic>>(www.downloadHandler.text);
                if (!ParcelationManager.Instance._messageLock)
                {
                    ParcelationManager.Instance.messageTopics = newMessageTopics;
                }
            }
        }
        yield return new WaitForSeconds(updateDuration);
        StartCoroutine(IEGetMessages());
    }
}

[System.Serializable]
public class GetParcelationResult
{
    public string parcelation;
    public string ssCount;
}

[System.Serializable]
public class InputResult
{
    public double userId;
    public string parcelation;
}

[System.Serializable]
public class MessageTopic
{
    public string topic;
    public int topicId;
    public List<Message> messages;
}