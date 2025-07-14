//using TheKnights.FaceBook;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardPrefab : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private RawImage profileRawImage;
    [SerializeField] private Image backGroundImage;

    public void SetInformation(string _name, string _score, Texture2D _profileTex, Color backGroundColor,  bool isLocal = false, bool isAnonymous = false)
    {
        backGroundImage.color = backGroundColor;
        nameText.text = _name;
        scoreText.text = _score;
        profileRawImage.texture = _profileTex;

        if(isLocal && !isAnonymous && nameText.text != null)
        {
            nameText.text += " (Me)";
        }
        
        if(isLocal && isAnonymous)
        {
            nameText.text = "(You)";
        }
    }
 
}