using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace TheKnights.LeaderBoardSystem
{
    [System.Serializable]
    public class GroupInfo : ISerializationCallbackReceiver
    {
        public string userCountry;
        public string userCountryIso;
        public string userGroupID;
        public string userID;
        public string stamp;
        public int daysLeftInWeek;
        public string timeSpawnLeftInWeek;
        public string weekNumber;

        [System.NonSerialized] public List<LeaderBoardGroupUserData> usersData;
        [System.NonSerialized] public Sprite countrySprite;
        [System.NonSerialized] public LeaderBoardGroupUserData localPlayerData;
        [System.NonSerialized] public int[] leaderBoardRankScoreThresholds;

        [System.NonSerialized] private float timeInSecWhenCreated;
        [System.NonSerialized] TimeSpan timeSpanRemainingWhenCreated;

        public bool IsValid => usersData != null && usersData.Count > 0 && localPlayerData != null;

        public void OnAfterDeserialize()
        {
            if (userGroupID == null || userGroupID == "")
                return;

            weekNumber = stamp.Substring(stamp.IndexOf("_") + 1);

            try
            {
                TimeSpan t = TimeSpan.ParseExact(timeSpawnLeftInWeek, "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
                t = new TimeSpan(24, 0, 0).Subtract(t);

                timeSpawnLeftInWeek = $"{t.Hours}h:{t.Minutes}m";

                timeInSecWhenCreated = Time.time;
                timeSpanRemainingWhenCreated = t;
            }
            catch (Exception e)
            {
                UnityEngine.Console.LogWarning($"Exception while trying to parse timeSpanLeftInWeek {timeSpawnLeftInWeek}. Error: {e}");
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public string GetRemainingTimeTillLeaderBoardRefresh()
        {
            int secondsPassedSinceGroupCreation = (int)(Time.time - timeInSecWhenCreated);
            TimeSpan remainingTimeNow = timeSpanRemainingWhenCreated.Subtract(new TimeSpan(0,0, secondsPassedSinceGroupCreation));

            TimeSpan zeroTimeSpan = TimeSpan.Zero;

            if(remainingTimeNow <= zeroTimeSpan)
            {
                remainingTimeNow = zeroTimeSpan;
            }

            return $"{remainingTimeNow.Hours}h:{remainingTimeNow.Minutes}m";
        }


        /// <summary>
        /// Cleans the native data like loaded textures from the memory. Call this before wiping out the leaderboard data
        /// </summary>
        public void WipeData()
        {
            if (usersData != null)
            {
                for (int i = 0; i < usersData.Count; i++)
                {
                    usersData[i].DestroyLoadedTextures();
                }
            }

            //if (countrySprite != null)
            //{
            //    Resources.UnloadAsset(countrySprite);
            //}

        }
    }

    [System.Serializable]
    public class LeaderBoardGroupQueryResponseData
    {
        public GroupInfo currentGroupInfo;
        public GroupInfo pendingRewardInfo;

        public int Version { get; set; }

        public bool IsValid => (currentGroupInfo != null && currentGroupInfo.IsValid) || (pendingRewardInfo != null && pendingRewardInfo.IsValid);
    }
}