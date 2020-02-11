using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RegisterForm : MonoBehaviour
{
    [SerializeField]
    private Image avatarImage;
    private int avatarSelector = 0;
    [SerializeField]
    private string[] avatarResourceStrings;
    [SerializeField]
    private InputField nameInput;
    [SerializeField]
    private InputField paxInput;
    [SerializeField]
    private Dropdown favPlaceInput;
    [SerializeField]
    private Button submit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckForm()
    {
        int pax;
        if (nameInput.text != "" && paxInput.text != "" && int.TryParse(paxInput.text, out pax)) 
        {
            ConnectionManager.Instance.StartNewUserInput(nameInput.text, pax, avatarSelector, favPlaceInput.itemText.text);
            submit.interactable = true;
        }
        else
        {
            submit.interactable = false;
        }
    }

    public void NextAvatarSelection()
    {
        avatarSelector = (avatarSelector + 1) % avatarResourceStrings.Length;
        avatarImage.sprite = Resources.Load<Sprite>("Sprites/Tenants/" + avatarResourceStrings[avatarSelector]);
    }

    public void PrevAvatarSelection()
    {
        if (--avatarSelector < 0) avatarSelector = avatarResourceStrings.Length - 1;
        avatarImage.sprite = Resources.Load<Sprite>("Sprites/Tenants/" + avatarResourceStrings[avatarSelector]);
    }
}
