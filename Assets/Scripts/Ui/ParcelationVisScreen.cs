using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParcelationVisScreen : MonoBehaviour
{
    [SerializeField]
    private Transform toggleButtonTransform;
    private Texture2D toggleOn;
    private Texture2D toggleOff;

    private void Start()
    {
        toggleOff = Resources.Load<Texture2D>("Sprites/UI/toggle-off");
        toggleOn = Resources.Load<Texture2D>("Sprites/UI/toggle-on");

        //ParcelationManager.Instance.RoomMode = false;
        RawImage toggleImage = toggleButtonTransform.GetChild(0).GetComponent<RawImage>();
        TMPro.TextMeshProUGUI toggleText = toggleButtonTransform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        toggleImage.texture = toggleOff;
        toggleText.color = Color.black;
    }

    public void ToggleRoomMode()
    {
        //ParcelationManager.Instance.RoomMode = !ParcelationManager.Instance.RoomMode;
        RawImage toggleImage = toggleButtonTransform.GetChild(0).GetComponent<RawImage>();
        TMPro.TextMeshProUGUI toggleText = toggleButtonTransform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        if (ParcelationManager.Instance.RoomMode)
        {
            toggleImage.texture = toggleOn;
            toggleText.color = Color.white;
        }
        else
        {
            toggleImage.texture = toggleOff;
            toggleText.color = Color.black;
        }
    }
}
