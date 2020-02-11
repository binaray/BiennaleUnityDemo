using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField]
    private float UpdateTimeDiv = 1.0f;
    private UserInput userInput;

    //singleton
    private static ConnectionManager _instance;
    public static ConnectionManager Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    public void StartNewUserInput(string name, int pax, int avatarId, string favLocIndex) => userInput = new UserInput(name, pax, avatarId, favLocIndex);

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
