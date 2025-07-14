using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System;
using UnityEngine.Scripting;

namespace TheKnights.LeaderBoardSystem
{
    [FirestoreData][Preserve]
    public class LeaderBoardGroupUserData : IComparable<LeaderBoardGroupUserData> /*: ISerializationCallbackReceiver*/
    {
        [Preserve] [FirestoreProperty] public string myID { get; set; }
        [Preserve] [FirestoreProperty] public Blob profilePic { get; set; }
        [Preserve] [FirestoreProperty] public int score { get; set; }
        [Preserve] [FirestoreProperty] public string userName { get; set; }
        [Preserve] [FirestoreProperty] public string userPhotoUrl { get; set; }

        [Preserve] [FirestoreProperty] public bool isAnonymous { get; set; }

        public Texture2D ProfileImageTexture { get;  set; }

        public LeaderBoardRank LeaderBoardRank { get; set; }

        public int CompareTo(LeaderBoardGroupUserData other)
        {
            return score.CompareTo(other.score);
        }

        /// <summary>
        /// Removes all loaded textures from memory. Do this before wiping out the data since the
        /// texture won't be collected by GC
        /// </summary>
        public void DestroyLoadedTextures()
        {
            if(ProfileImageTexture != null)
            {
                MonoBehaviour.Destroy(ProfileImageTexture);
            }
        }


        /// <summary>
        /// Reads the obtained blob bytes and load them into a profile 2D texture
        /// </summary>
        public void LoadUserProfileImageTextureFromBytes()
        {
            try
            {
                Texture2D tex = new Texture2D(75, 75);
                tex.LoadImage(profilePic.ToBytes());

                ProfileImageTexture = tex;
            }
            catch
            {
                UnityEngine.Console.LogWarning($"Error converting bytes to leaderboard profile image texture for user {userName}");
            }
        }

        [Preserve]
        public LeaderBoardGroupUserData()
        {


        }

        //public void OnAfterDeserialize()
        //{

        //}

        //public void OnBeforeSerialize()
        //{

        //}
    }
}