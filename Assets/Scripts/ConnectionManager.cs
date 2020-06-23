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
    public CreateUnitScreen createUnitScreen;

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
        StartCoroutine(IEGetMessages());

        string pTest = "{\"parcelation\": \"[{\\\"user_id\\\":0.0,\\\"floor\\\":0,\\\"loc\\\":[0,9],\\\"type\\\":\\\"Cafeteria\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Cafeteria\\\",\\\"ss\\\":1}},{\\\"user_id\\\":1591783415.5496323109,\\\"floor\\\":0,\\\"loc\\\":[13,15],\\\"type\\\":\\\"Assisted\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Assisted\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":true,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":0,\\\"SharedBedroom\\\":0,\\\"Study\\\":2},\\\"location\\\":[\\\"0\\\",\\\"1\\\"],\\\"preferredSharedSpaces\\\":[\\\"Lounge\\\",\\\"Salon\\\",\\\"Cafeteria\\\"]}},{\\\"user_id\\\":2.0,\\\"floor\\\":13,\\\"loc\\\":[3,12],\\\"type\\\":\\\"FitnessCentre\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"FitnessCentre\\\",\\\"ss\\\":1}},{\\\"user_id\\\":4.0,\\\"floor\\\":17,\\\"loc\\\":[0,9],\\\"type\\\":\\\"Lounge\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Lounge\\\",\\\"ss\\\":1}},{\\\"user_id\\\":1.0,\\\"floor\\\":31,\\\"loc\\\":[1,10],\\\"type\\\":\\\"CommunityFarm\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CommunityFarm\\\",\\\"ss\\\":1}},{\\\"user_id\\\":3.0,\\\"floor\\\":32,\\\"loc\\\":[1,10],\\\"type\\\":\\\"SportsHall\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"SportsHall\\\",\\\"ss\\\":1}}]\", \"ssCount\": \"{\\\"Cafeteria\\\": 1, \\\"CommunityFarm\\\": 0, \\\"FitnessCentre\\\": 0, \\\"SportsHall\\\": 0, \\\"Lounge\\\": 1, \\\"Salon\\\": 1, \\\"Library\\\": 0, \\\"Tailor\\\": 0, \\\"Market\\\": 0, \\\"Playscape\\\": 0, \\\"PlayRoom\\\": 0, \\\"Restaurant\\\": 0, \\\"MultiGenCenter\\\": 0, \\\"HealthcareClinic\\\": 0, \\\"Makerspace\\\": 0, \\\"Childcare\\\": 0}\", \"inputCount\": 1}";
        GetParcelationResult result = Newtonsoft.Json.JsonConvert.DeserializeObject<GetParcelationResult>(pTest);
        List<BuildingUnit> newState = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUnit>>(result.parcelation);
        Dictionary<string, int> ssCount = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(result.ssCount);
        ParcelationManager.Instance.UpdateParcelation(newState);
        CreateUnitScreen.Instance.UpdateQ6Counters(ssCount);

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
        string pTest = "{\"parcelation\": \"[{\\\"user_id\\\":2.0,\\\"floor\\\":2,\\\"loc\\\":[3,12],\\\"type\\\":\\\"FitnessCentre\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"FitnessCentre\\\",\\\"ss\\\":1}},{\\\"user_id\\\":1591783415.5496323109,\\\"floor\\\":2,\\\"loc\\\":[13,15],\\\"type\\\":\\\"Assisted\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Assisted\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":2,\\\"affordable\\\":true,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":0,\\\"SharedBedroom\\\":0,\\\"Study\\\":2},\\\"location\\\":[\\\"0\\\",\\\"1\\\"],\\\"preferredSharedSpaces\\\":[\\\"Lounge\\\",\\\"Salon\\\",\\\"Cafeteria\\\"]}},{\\\"user_id\\\":3.0,\\\"floor\\\":9,\\\"loc\\\":[0,9],\\\"type\\\":\\\"SportsHall\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"SportsHall\\\",\\\"ss\\\":1}},{\\\"user_id\\\":1591787245.2690422535,\\\"floor\\\":9,\\\"loc\\\":[13,15],\\\"type\\\":\\\"Multi\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Multi\\\",\\\"ageGroup\\\":\\\"Elderly\\\",\\\"pax\\\":3,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":0,\\\"SharedBedroom\\\":0,\\\"Study\\\":1},\\\"location\\\":[\\\"2\\\",\\\"1\\\"],\\\"preferredSharedSpaces\\\":[\\\"Salon\\\",\\\"SportsHall\\\",\\\"Cafeteria\\\"]}},{\\\"user_id\\\":1591787369.0874810219,\\\"floor\\\":20,\\\"loc\\\":[4,11],\\\"type\\\":\\\"Multi\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Multi\\\",\\\"ageGroup\\\":\\\"Midlife\\\",\\\"pax\\\":7,\\\"affordable\\\":false,\\\"requiredRooms\\\":{\\\"SingleBedroom\\\":2,\\\"SharedBedroom\\\":2,\\\"Study\\\":2},\\\"location\\\":[\\\"1\\\",\\\"0\\\"],\\\"preferredSharedSpaces\\\":[\\\"Salon\\\",\\\"FitnessCentre\\\",\\\"CommunityFarm\\\"]}},{\\\"user_id\\\":0.0,\\\"floor\\\":23,\\\"loc\\\":[0,9],\\\"type\\\":\\\"Cafeteria\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Cafeteria\\\",\\\"ss\\\":1}},{\\\"user_id\\\":1.0,\\\"floor\\\":31,\\\"loc\\\":[0,9],\\\"type\\\":\\\"CommunityFarm\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"CommunityFarm\\\",\\\"ss\\\":1}},{\\\"user_id\\\":4.0,\\\"floor\\\":27,\\\"loc\\\":[3,12],\\\"type\\\":\\\"Lounge\\\",\\\"user_input\\\":{\\\"livingArrangement\\\":\\\"Lounge\\\",\\\"ss\\\":1}}]\", \"ssCount\": \"{\\\"Cafeteria\\\": 2, \\\"CommunityFarm\\\": 1, \\\"FitnessCentre\\\": 1, \\\"SportsHall\\\": 1, \\\"Lounge\\\": 1, \\\"Salon\\\": 3, \\\"Library\\\": 0, \\\"Tailor\\\": 0, \\\"Market\\\": 0, \\\"Playscape\\\": 0, \\\"PlayRoom\\\": 0, \\\"Restaurant\\\": 0, \\\"MultiGenCenter\\\": 0, \\\"HealthcareClinic\\\": 0, \\\"Makerspace\\\": 0, \\\"Childcare\\\": 0}\", \"inputCount\": 3}";
        GetParcelationResult result = Newtonsoft.Json.JsonConvert.DeserializeObject<GetParcelationResult>(pTest);
        List<BuildingUnit> newState = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuildingUnit>>(result.parcelation);
        Dictionary<string, int> ssCount = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(result.ssCount);
        ParcelationManager.Instance.UpdateParcelation(newState);
        CreateUnitScreen.Instance.UpdateQ6Counters(ssCount);
        UiCanvasManager.Instance.CongratulatoryScreen();
        Debug.LogError(jsonInput);

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
            string mTest = "[{\"topic\": \"How do we live together?\", \"topicId\": 1, \"messages\": [{\"messageId\": 2, \"message\": \"Sport Utiliy Wear\", \"reply\": -1, \"timestamp\": 1590364108}, {\"messageId\": 1, \"message\": \"Hello everyone!\", \"reply\": -1, \"timestamp\": 1590364108}]}, {\"topic\": \"What will the world be in 20 years?\", \"topicId\": 2, \"messages\": [{\"messageId\": 5, \"message\": \"Magnificient bear cubs\", \"reply\": -1, \"timestamp\": 1590364108}, {\"messageId\": 4, \"message\": \"Save the turtles\", \"reply\": \"Sport Utiliy Wear\", \"timestamp\": 1590364108}, {\"messageId\": 3, \"message\": \"Tiger skin for sale\", \"reply\": -1, \"timestamp\": 1590364108}]}]";
            List<MessageTopic> newMessageTopics = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MessageTopic>>(mTest);
            ParcelationManager.Instance.messageTopics = newMessageTopics;
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