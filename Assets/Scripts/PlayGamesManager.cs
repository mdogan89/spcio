using UnityEngine;
#if UNITY_ANDROID_API
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
public class PlayGamesManager : MonoBehaviour
{
    public void Start()
{
        SignIn();
}

    void SignIn()
    {
#if UNITY_ANDROID_API || UNITY_IOS
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
#endif
    }

#if UNITY_ANDROID_API || UNITY_IOS

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
#endif
public void PlayGamesSignInClicked()
    {
#if UNITY_ANDROID_API || UNITY_IOS
        PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
#endif
    }
}
