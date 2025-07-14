using TheKnights.FaceBook;
using TMPro;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [SerializeField] private FaceBookManager faceBookManager;
    [SerializeField] private TextMeshProUGUI facebookConnectButtonText;

    private void OnEnable()
    {
        FaceBookManager.OnUserLoggedInToFaceBook.AddListener(UpdateFaceBookLogInStatusUI);
        FaceBookManager.OnUserLoggedOutFromFaceBook.AddListener(UpdateFaceBookLogInStatusUI);

        UpdateFaceBookLogInStatusUI();
    }

    private void OnDisable()
    {
        FaceBookManager.OnUserLoggedInToFaceBook.RemoveListener(UpdateFaceBookLogInStatusUI);
        FaceBookManager.OnUserLoggedOutFromFaceBook.RemoveListener(UpdateFaceBookLogInStatusUI);
    }

    public void Terms()
    {
        Application.OpenURL("https://frolicsfreegames.com/privacy-policy.html");
    }

    public void SendEmail()
    {
        string email = "tera.bitegames.official.com";

        string subject = MyEscapeURL("Mini Car Rush 2 Feedback");

        string body = MyEscapeURL("");

        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }

    private string MyEscapeURL(string url)
    {
        return WWW.EscapeURL(url).Replace("+", "%20");
    }

    private void UpdateFaceBookLogInStatusUI()
    {
        if (FaceBookManager.isLoggedInToFaceBook)
        {
            facebookConnectButtonText.text = "Connected";
        }
        else
        {
            facebookConnectButtonText.text = "Connect To FaceBook";
        }
    }

    /// <summary>
    /// Signs in the user if the user is logged out and signs out if it's the other way around
    /// </summary>
    public void ToggleFaceBookButton()
    {
        faceBookManager.ToggleUserAuthentication();
    }
}