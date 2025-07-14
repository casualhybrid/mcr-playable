using Firebase.Firestore;
using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace TheKnights.LeaderBoardSystem.FaceBook
{
    [FirestoreData]
    [Preserve]
    public class FaceBookLeaderBoardUserData : IComparable<FaceBookLeaderBoardUserData>
    {
        [Preserve] [FirestoreProperty] public int score { get; set; }
        [Preserve] [FirestoreProperty] public string name { get; set; }
        [Preserve] [FirestoreProperty] public Blob profilePic { get; set; }

        public Texture2D profilePicTex { get; private set; }

        public int CompareTo(FaceBookLeaderBoardUserData other)
        {
            return score.CompareTo(other.score);
        }

        public void LoadProfilePictureFromBytes()
        {
            try
            {
                if (profilePic == null || profilePic.Length == 0)
                    return;


                Texture2D tex = new Texture2D(75, 75);
                tex.LoadImage(profilePic.ToBytes());

                profilePicTex = tex;
            }
            catch (Exception e)
            {
                UnityEngine.Console.Log($"Error while loading facebook leaderboard profile picture for user {name}. Exception: {e}");
            }
        }

        public void WipeData()
        {
            if (profilePicTex != null)
            {
                MonoBehaviour.Destroy(profilePicTex);
            }
        }

        [Preserve]
        public FaceBookLeaderBoardUserData()
        {

        }

    }
}