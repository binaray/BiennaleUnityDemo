using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParcelationVisScreen : MonoBehaviour
{
    [SerializeField]
    private Button toggleRoomModeButton;

    public void ToggleRoomMode()
    {
        ParcelationManager.Instance.RoomMode = !ParcelationManager.Instance.RoomMode;
    }
}
