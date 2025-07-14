using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderBoardRankEntity : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreToAchieveRankText;
    [SerializeField] private GameObject rankAwardPanel;

    private static GameObject currentlyActiveRankAwardPanel;

    public void OpenRankAwardPanel()
    {
        if(currentlyActiveRankAwardPanel != null)
        {
            currentlyActiveRankAwardPanel.SetActive(false);
        }

        rankAwardPanel.SetActive(true);
        currentlyActiveRankAwardPanel = rankAwardPanel;

        this.enabled = true;
    }

    public void CloseRankAwardPanel()
    {
        currentlyActiveRankAwardPanel = null;
        rankAwardPanel.SetActive(false);

        this.enabled = false;

    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            CloseRankAwardPanel();
        }
    }

    private void OnDisable()
    {
        rankAwardPanel.SetActive(false);
    }
}
