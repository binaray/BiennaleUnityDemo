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
    [SerializeField]
    private string endpoint;
    private UserInput userInput;
    private bool isConnecting = false;

    public static ConnectionManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    public void StartNewUserInput(string name, int pax, int avatarId, string favLocIndex) => userInput = new UserInput(name, pax, avatarId, favLocIndex);

    void Start()
    {
        StartCoroutine(RetrieveServerState());
    }

    private IEnumerator RetrieveServerState(System.Action<string> callback = null)
    {
        yield return new WaitForSeconds(updateDuration);
        if (!isConnecting)
        {
            isConnecting = true;
            WWWForm form = new WWWForm();

            UnityWebRequest www = UnityWebRequest.Post(endpoint + "state.php", form);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);
                callback(www.downloadHandler.text);
                Debug.Log("Form upload complete!");
                StartCoroutine(RetrieveServerState());
            }
            isConnecting = false;
        }
        StartCoroutine(RetrieveServerState());
    }

    private IEnumerator UploadUserInput(System.Action<string> callback = null)
    {
        yield return new WaitForSeconds(1.0f);
        if (!isConnecting)
        {
            isConnecting = true;
            WWWForm form = new WWWForm();
            form.AddField("UserInput", JsonUtility.ToJson(userInput));

            UnityWebRequest www = UnityWebRequest.Post(endpoint + "input.php", form);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                StartCoroutine(UploadUserInput());
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);
                callback(www.downloadHandler.text);
                Debug.Log("Form upload complete!");
            }
            isConnecting = false;
        }
        else StartCoroutine(UploadUserInput());
    }
}
