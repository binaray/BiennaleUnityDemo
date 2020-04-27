using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CongratulatoryScreen : MonoBehaviour
{
    [SerializeField]
    private Firework fireworkSystem;
    [SerializeField]
    private TMPro.TextMeshProUGUI returnText;
    private bool irq;

    public void EndCountdown()
    {
        irq = true;
    }

    IEnumerator UpdateReturnText()
    {
        int returnIn = 30;
        
        while (returnIn > 0)
        {
            returnText.text = string.Format("Return ({0})", returnIn);
            yield return new WaitForSeconds(1);
            returnIn--;
            if (irq) break;
        }
        UiCanvasManager.Instance.StartScreen();
    }

    private void OnEnable()
    {
        fireworkSystem.Play();
        irq = false;
        StartCoroutine(UpdateReturnText());
    }

    private void OnDisable()
    {
        fireworkSystem.stopFun = true;
        irq = true;
    }
}
