using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FaceBookLeaderBoardPrefab : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private RawImage profileRawImage;

    public void SetInformation(string _name, string _score, Texture2D _profileTex, bool isLocal = false)
    {
        nameText.text = _name;
        scoreText.text = _score;
        profileRawImage.texture = _profileTex;

        if (isLocal && nameText.text != null)
        {
            nameText.text += " (Me)";
        }

    }
}