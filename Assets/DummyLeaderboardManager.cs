using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class LeaderboardRow
{
    public TMP_Text nameText;
    public TMP_Text scoreText;
    public Image dpImage;
}

public class DummyLeaderboardManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;

    [Header("UI Rows")]
    public LeaderboardRow[] rows;

    [Header("Dummy Data Settings")]
    public string[] randomNames;
    public Sprite[] dpSprites;
    public int minScore = 10000;
    public int maxScore = 999999;

    [Header("Current Player")]
    public int myScore = 123456;

    private void OnEnable()
    {
        SetupDummyLeaderboard();
    }

    void SetupDummyLeaderboard()
    {
        int totalPlayers = rows.Length;

        // Get actual player score from gameplay inventory
        myScore = gamePlaySessionInventory.GetPlayerHighestScore();

        // Random index where "Me" will be placed initially (before sorting)
        int myIndex = Random.Range(0, totalPlayers);

        // Step 1: Build a list of random entries
        List<LeaderboardEntry> allEntries = new List<LeaderboardEntry>();

        for (int i = 0; i < totalPlayers; i++)
        {
            LeaderboardEntry entry = new LeaderboardEntry();

            if (i == myIndex)
            {
                entry.name = "Me";
                entry.score = myScore;
            }
            else
            {
                entry.name = randomNames[Random.Range(0, randomNames.Length)];
                entry.score = Random.Range(minScore, maxScore);
            }

            entry.dp = dpSprites[Random.Range(0, dpSprites.Length)];

            allEntries.Add(entry);
        }

        // Step 2: Sort the list by score (descending)
        allEntries = allEntries.OrderByDescending(e => e.score).ToList();

        // Step 3: Populate sorted data into UI
        for (int i = 0; i < totalPlayers; i++)
        {
            rows[i].nameText.text = allEntries[i].name;
            rows[i].scoreText.text = allEntries[i].score.ToString("N0");
            rows[i].dpImage.sprite = allEntries[i].dp;
        }
    }

    // Inner class for dummy leaderboard entries
    private class LeaderboardEntry
    {
        public string name;
        public int score;
        public Sprite dp;
    }
}
