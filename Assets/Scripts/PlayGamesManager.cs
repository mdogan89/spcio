using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class PlayGamesManager : MonoBehaviour
{
    public void Start()
{
        SignIn();
}

    void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }


internal void ProcessAuthentication(SignInStatus status)
{
    if (status == SignInStatus.Success)
    {
        // Continue with Play Games Services
        string name = PlayGamesPlatform.Instance.GetUserDisplayName();
        string id = PlayGamesPlatform.Instance.GetUserId();
            Debug.Log($"User authenticated successfully: {name} (ID: {id})");
        //string ImageUrl = PlayGamesPlatform.Instance.GetUserImageUrl();
        PlayerManager.Instance.nick = name;
        PlayerManager.Instance.nickInputField.text = name;
        }
    else
    {
        // Disable your integration with Play Games Services or show a login button
        // to ask users to sign-in. Clicking it should call
        // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
        Debug.Log("User authentication failed: " + status);
        }
}

    public void PlayGamesSignInClicked()
    {
        PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
    }
    }
