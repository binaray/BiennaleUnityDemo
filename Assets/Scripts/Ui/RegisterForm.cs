using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RegisterForm : MonoBehaviour
{
    [SerializeField]
    private Image avatarImage;
    private int avatarIndex = 0;
    [SerializeField]
    private InputField avatarNameInput;
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
        if (avatarNameInput.text != "" && paxInput.text != "" && int.TryParse(paxInput.text, out pax)) 
        {
            ConnectionManager.Instance.StartNewUserInput(avatarNameInput.text, avatarIndex, pax, favPlaceInput.itemText.text);
            submit.interactable = true;
        }
        else
        {
            submit.interactable = false;
        }
    }

    public void NextAvatarSelection()
    {
        avatarIndex = (avatarIndex + 1) % Constants.AvatarResourceStrings.Length;
        avatarImage.sprite = Resources.Load<Sprite>("Sprites/Tenants/" + Constants.AvatarResourceStrings[avatarIndex]);
    }

    public void PrevAvatarSelection()
    {
        if (--avatarIndex < 0) avatarIndex = Constants.AvatarResourceStrings.Length - 1;
        avatarImage.sprite = Resources.Load<Sprite>("Sprites/Tenants/" + Constants.AvatarResourceStrings[avatarIndex]);
    }
}
