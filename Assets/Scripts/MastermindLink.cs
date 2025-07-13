using UnityEngine;

public class MastermindLink : MonoBehaviour
{
    static readonly string mastermindUrl = "https://play.google.com/store/apps/details?id=com.md89.mastermind";
    public void OnMastermindLinkClicked(){
        Application.OpenURL(mastermindUrl);
}
}