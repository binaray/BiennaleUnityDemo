using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    [SerializeField]
    private GameObject speakerPortrait;
    [SerializeField]
    private UnityEngine.UI.Text text;

    private bool _isDialogActive;
    public bool IsDialogActive {
        get 
        {
            return _isDialogActive;
        }
        set
        {
            speakerPortrait.SetActive(value);
            text.enabled = value;
            _isDialogActive = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
