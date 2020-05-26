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
        //StartCoroutine(IERetrieveServerState());
        //StartCoroutine(IEGetMessages());
        string pTest = "{\"parcelation\": \"[{\\\"user_id\\\":1590361929.0672345161,\\\"floor\\\":11,\\\"loc\\\":[0,7],\\\"type\\\":\\\"CoupleWoChildren\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CoupleWoChildren\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":2,\\\"SharedBedroom\\\":3,\\\"Study\\\":1},\\\"location\\\":[0,0],\\\"preferredSharedSpaces\\\":[\\\"Farm\\\",\\\"CoWorking\\\"]}},{\\\"user_id\\\":1.0,\\\"floor\\\":9,\\\"loc\\\":[3,12],\\\"type\\\":\\\"Lounge\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Lounge\\\",\\\"ss\\\":1}},{\\\"user_id\\\":4.0,\\\"floor\\\":12,\\\"loc\\\":[0,9],\\\"type\\\":\\\"Clinic\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Clinic\\\",\\\"ss\\\":1}},{\\\"user_id\\\":1590361957.1218485832,\\\"floor\\\":19,\\\"loc\\\":[3,10],\\\"type\\\":\\\"CoupleWoChildren\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CoupleWoChildren\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":2,\\\"SharedBedroom\\\":3,\\\"Study\\\":1},\\\"location\\\":[0,0],\\\"preferredSharedSpaces\\\":[\\\"Farm\\\",\\\"CoWorking\\\"]}},{\\\"user_id\\\":0.0,\\\"floor\\\":18,\\\"loc\\\":[0,9],\\\"type\\\":\\\"Farm\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Farm\\\",\\\"ss\\\":1}},{\\\"user_id\\\":1590361931.173833847,\\\"floor\\\":29,\\\"loc\\\":[3,10],\\\"type\\\":\\\"CoupleWoChildren\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CoupleWoChildren\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":2,\\\"SharedBedroom\\\":3,\\\"Study\\\":1},\\\"location\\\":[0,0],\\\"preferredSharedSpaces\\\":[\\\"Farm\\\",\\\"CoWorking\\\"]}},{\\\"user_id\\\":2.0,\\\"floor\\\":27,\\\"loc\\\":[0,9],\\\"type\\\":\\\"Makerspace\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Makerspace\\\",\\\"ss\\\":1}},{\\\"user_id\\\":3.0,\\\"floor\\\":25,\\\"loc\\\":[1,10],\\\"type\\\":\\\"Cafes\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Cafes\\\",\\\"ss\\\":1}}]\", \"ssCount\": \"{\\\"Farm\\\": 3, \\\"Lounge\\\": 0, \\\"Makerspace\\\": 0, \\\"Cafes\\\": 0, \\\"Clinic\\\": 0, \\\"Eldercare\\\": 0, \\\"Childcare\\\": 0, \\\"Salon\\\": 0, \\\"Gym\\\": 0, \\\"Market\\\": 0, \\\"Library\\\": 0, \\\"Playscape\\\": 0, \\\"CoWorking\\\": 3}\"}";
        GetParcelationResult result = Newtonsoft.Json.JsonConvert.DeserializeObject<GetParcelationResult>(pTest);
        List<BuildingUnit> newState = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUnit>>(result.parcelation);
        Dictionary<string, int> ssCount = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(result.ssCount);
        ParcelationManager.Instance.UpdateParcelation(newState);
        CreateUnitScreen.Instance.UpdateQ6Counters(ssCount);

        string mTest = "[{\"topic\": \"How do we live together?\", \"topicId\": 1, \"messages\": [{\"messageId\": 2, \"message\": \"Sport Utiliy Wear\", \"reply\": -1, \"timestamp\": 1590364108}, {\"messageId\": 1, \"message\": \"Hello everyone!\", \"reply\": -1, \"timestamp\": 1590364108}]}, {\"topic\": \"What will the world be in 20 years?\", \"topicId\": 2, \"messages\": [{\"messageId\": 5, \"message\": \"Magnificient bear cubs\", \"reply\": -1, \"timestamp\": 1590364108}, {\"messageId\": 4, \"message\": \"Save the turtles\", \"reply\": \"Sport Utiliy Wear\", \"timestamp\": 1590364108}, {\"messageId\": 3, \"message\": \"Tiger skin for sale\", \"reply\": -1, \"timestamp\": 1590364108}]}]";
        List<MessageTopic> newMessageTopics = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MessageTopic>>(mTest);
        ParcelationManager.Instance.messageTopics = newMessageTopics;
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
        //test override code
        string pTest = "{\"parcelation\":\"[{\\\"user_id\\\":1590466911.5417859554,\\\"floor\\\":4,\\\"loc\\\":[0,7],\\\"type\\\":\\\"CoupleWoChildren\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CoupleWoChildren\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":2,\\\"SharedBedroom\\\":3,\\\"Study\\\":1},\\\"location\\\":[0,0],\\\"preferredSharedSpaces\\\":[\\\"Farm\\\",\\\"CoWorking\\\"]}},{\\\"user_id\\\":1590466913.0383877754,\\\"floor\\\":4,\\\"loc\\\":[8,15],\\\"type\\\":\\\"CoupleWoChildren\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CoupleWoChildren\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":2,\\\"SharedBedroom\\\":3,\\\"Study\\\":1},\\\"location\\\":[0,0],\\\"preferredSharedSpaces\\\":[\\\"Farm\\\",\\\"CoWorking\\\"]}},{\\\"user_id\\\":1590466913.5904693604,\\\"floor\\\":6,\\\"loc\\\":[3,10],\\\"type\\\":\\\"CoupleWoChildren\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CoupleWoChildren\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":2,\\\"SharedBedroom\\\":3,\\\"Study\\\":1},\\\"location\\\":[0,0],\\\"preferredSharedSpaces\\\":[\\\"Farm\\\",\\\"CoWorking\\\"]}},{\\\"user_id\\\":3.0,\\\"floor\\\":3,\\\"loc\\\":[0,9],\\\"type\\\":\\\"Cafes\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Cafes\\\",\\\"ss\\\":1}},{\\\"user_id\\\":1590466914.1744806767,\\\"floor\\\":16,\\\"loc\\\":[3,10],\\\"type\\\":\\\"CoupleWoChildren\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CoupleWoChildren\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":2,\\\"SharedBedroom\\\":3,\\\"Study\\\":1},\\\"location\\\":[0,0],\\\"preferredSharedSpaces\\\":[\\\"Farm\\\",\\\"CoWorking\\\"]}},{\\\"user_id\\\":1.0,\\\"floor\\\":12,\\\"loc\\\":[0,9],\\\"type\\\":\\\"Lounge\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Lounge\\\",\\\"ss\\\":1}},{\\\"user_id\\\":1590466910.3780748844,\\\"floor\\\":21,\\\"loc\\\":[0,7],\\\"type\\\":\\\"CoupleWoChildren\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CoupleWoChildren\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":2,\\\"SharedBedroom\\\":3,\\\"Study\\\":1},\\\"location\\\":[0,0],\\\"preferredSharedSpaces\\\":[\\\"Farm\\\",\\\"CoWorking\\\"]}},{\\\"user_id\\\":1590466911.942027092,\\\"floor\\\":21,\\\"loc\\\":[8,15],\\\"type\\\":\\\"CoupleWoChildren\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CoupleWoChildren\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":2,\\\"SharedBedroom\\\":3,\\\"Study\\\":1},\\\"location\\\":[0,0],\\\"preferredSharedSpaces\\\":[\\\"Farm\\\",\\\"CoWorking\\\"]}},{\\\"user_id\\\":2.0,\\\"floor\\\":23,\\\"loc\\\":[0,9],\\\"type\\\":\\\"Makerspace\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Makerspace\\\",\\\"ss\\\":1}},{\\\"user_id\\\":4.0,\\\"floor\\\":19,\\\"loc\\\":[1,10],\\\"type\\\":\\\"Clinic\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Clinic\\\",\\\"ss\\\":1}},{\\\"user_id\\\":1590466912.5323684216,\\\"floor\\\":33,\\\"loc\\\":[0,7],\\\"type\\\":\\\"CoupleWoChildren\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CoupleWoChildren\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":2,\\\"SharedBedroom\\\":3,\\\"Study\\\":1},\\\"location\\\":[0,0],\\\"preferredSharedSpaces\\\":[\\\"Farm\\\",\\\"CoWorking\\\"]}},{\\\"user_id\\\":0.0,\\\"floor\\\":26,\\\"loc\\\":[0,9],\\\"type\\\":\\\"Farm\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Farm\\\",\\\"ss\\\":1}}]\",\"ssCount\":\"{\\\"Farm\\\": 7, \\\"Lounge\\\": 0, \\\"Makerspace\\\": 0, \\\"Cafes\\\": 0, \\\"Clinic\\\": 0, \\\"Eldercare\\\": 0, \\\"Childcare\\\": 0, \\\"Salon\\\": 0, \\\"Gym\\\": 0, \\\"Market\\\": 0, \\\"Library\\\": 0, \\\"Playscape\\\": 0, \\\"CoWorking\\\": 7}\"}";
        GetParcelationResult result = Newtonsoft.Json.JsonConvert.DeserializeObject<GetParcelationResult>(pTest);
        List<BuildingUnit> newState = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUnit>>(result.parcelation);
        Dictionary<string, int> ssCount = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(result.ssCount);
        ParcelationManager.Instance.UpdateParcelation(newState);
        CreateUnitScreen.Instance.UpdateQ6Counters(ssCount);
        UiCanvasManager.Instance.CongratulatoryScreen();
        //Debug.LogError(jsonInput);
        //Show loader
        //StartCoroutine(IEUploadUserInput(jsonInput, callback: result =>
        //{
        //    Debug.LogWarning(result);
        //    InputResult res = Newtonsoft.Json.JsonConvert.DeserializeObject<InputResult>(result);
        //    List<BuildingUnit> newState = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUnit>>(res.parcelation);
        //    ParcelationManager.Instance.currentUserId = res.userId;
        //    Debug.LogWarning("UserId: " + res.userId.ToString());
        //    ParcelationManager.Instance.UpdateParcelation(newState);
        //    //Hide loader overlay
        //    UiCanvasManager.Instance.CongratulatoryScreen();
        //}));
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