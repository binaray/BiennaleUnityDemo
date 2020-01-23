using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiButton : MonoBehaviour, IPointerDownHandler, IPointerExitHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        BuildingBlockManager.Instance.ConfirmationLock = true;
        BuildingBlockManager.Instance.SetConfirmedBlocks();
        Debug.Log(this.gameObject.name + " Was Clicked.");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BuildingBlockManager.Instance.ConfirmationLock = false;
        Debug.Log(this.gameObject.name + " Exited.");
    }
}
