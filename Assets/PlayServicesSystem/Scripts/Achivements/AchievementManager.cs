using System;
using System.Collections;
using System.Collections.Generic;
//using TheKnights.FaceBook;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace TheKnights.PlayServicesSystem.Achievements
{
    [CreateAssetMenu(fileName = "AchievementManager", menuName = "ScriptableObjects/PlayGames/AchievementManager", order = 1)]
    public class AchievementManager : ScriptableObject
    {
        //[Tooltip("Will reload the achievemeants data every time it's requested")]
        //[SerializeField] private bool reloadDataEachTime = true;

        ///// <summary>
        ///// The current loading state of achievements data
        ///// </summary>
        //public AchievementLoadingState AchievementLoadingState { get; private set; }

        //// Reference to Cached Achivements
        //private LinkedList<CustomAchivementData> AchivementsLinkedListRef;

        //// Queue containing all pending listeners waiting for the operation to complete
        //private Queue<AchievementAsyncOperation> PendingListnersQueue = new Queue<AchievementAsyncOperation>();

        //private void OnEnable()
        //{
        //    AchievementLoadingState = AchievementLoadingState.Stale;
        //    ResetAchivementCache();
        //}

        ///// <summary>
        ///// Shows the achievements panel through play services UI
        ///// </summary>
        //public void ShowAchievementsOriginalUI()
        //{
        //    if (PlayServicesController.isLoggedIn)
        //    {
        //        PlayGamesPlatform.Instance.ShowAchievementsUI();
        //    }
        //    else
        //    {
        //        UnityEngine.Console.Log("Cannot show Achievements, not logged in");
        //    }
        //}

        ///// <summary>
        ///// Unlocks a single one time achievement
        ///// </summary>
        ///// <param name="achievementID">The id for the achievement as specified in the console</param>
        ///// <param name="progress">A progress of 100.0 means the achivement has been completed</param>
        //public void UnlockOneTimeAchivement(string achievementID, float progress = 100.0f)
        //{
        //    if (!PlayServicesController.isLoggedIn)
        //    {
        //        UnityEngine.Console.Log("Failed unlocking achivement. Player not logged in");
        //        return;
        //    }

        //    // Unlocking it multiple times is ok, only the first time matters.
        //    PlayGamesPlatform.Instance.ReportProgress(
        //        achievementID,
        //        progress, (bool success) =>
        //        {
        //            if (!success)
        //            {
        //                UnityEngine.Console.LogWarning("Failed to unlock achivement. " + achievementID);
        //            }
        //            else
        //            {
        //                UnityEngine.Console.Log("Achivement Unlocked: " + achievementID);
        //            }
        //        });
        //}

        ///// <summary>
        ///// Reveals a previously hidden achievement
        ///// </summary>
        ///// <param name="id">The id for the achievement as specified in the console</param>
        //public void RevealAnAchivement(string id)
        //{
        //    UnlockOneTimeAchivement(id, 0f);
        //}

        ///// <summary>
        ///// Increments an achivement by the specified steps
        ///// </summary>
        ///// <param name="achievementID">The id for the achievement as specified in the console</param>
        ///// <param name="steps">The number of steps to increment the achievement by</param>
        //public void UpdateIncrementalAchievement(string achievementID, int steps)
        //{
        //    PlayGamesPlatform.Instance.IncrementAchievement(
        //        achievementID, steps, (bool success) =>
        //        {
        //            if (!success)
        //            {
        //                UnityEngine.Console.LogWarning("Failed to increment achivement. " + achievementID);
        //            }
        //            else
        //            {
        //                UnityEngine.Console.Log("Achivement ID " + achievementID + " incremented by : " + steps);
        //            }
        //        });
        //}

        ///// <summary>
        ///// Start the achievenents data load operation
        ///// </summary>
        ///// <param name="forceReload">Tries to reset the cache and loads achievements data from scratch.
        ///// May fail if the operation is already is in loading state</param>
        //public AchievementAsyncOperation LoadAchievementsData(bool forceReload = false)
        //{
        //    AchievementAsyncOperation handle = new AchievementAsyncOperation();

        //    Action ActionToPerform = () =>
        //    {
        //        PendingListnersQueue.Enqueue(handle);

        //        if (forceReload || reloadDataEachTime)
        //        {
        //            // Will still fail if the operation is in loading state
        //            ResetAchivementCache();
        //        }

        //        if (AchievementLoadingState == AchievementLoadingState.Loading)
        //        {
        //            UnityEngine.Console.Log("Achievement data is already loading");
        //            return;
        //        }

        //        // Check for cached data First
        //        if (AchivementsLinkedListRef != null && AchivementsLinkedListRef.Count > 0)
        //        {
        //            //try
        //            //{
        //            //    ShowAchivementData(_AchivementsLinkedListRef, true, tempDictionary);
        //            //}
        //            //catch { }

        //            UnityEngine.Console.Log("Sending Cached Achievements Data");
        //            LinkedList<CustomAchivementData> copy = new LinkedList<CustomAchivementData>(AchivementsLinkedListRef);
        //            SendAchievementDataRetreivedSignal(copy, Status.Succeeded);

        //            return;
        //        }

        //        try
        //        {
        //            AchievementLoadingState = AchievementLoadingState.Loading;
        //            PlayGamesPlatform.Instance.LoadAchievements((userAchivementData) =>
        //            {
        //                //Saftey Check
        //                if (userAchivementData == null)
        //                {
        //                    UnityEngine.Console.LogWarning("Failed to retrieve achivement status for the user");
        //                    HandleFailedToGetAchivementData();
        //                    return;
        //                }

        //                //User Achivement Data Has Been Loaded. Now loading achivements description
        //                PlayGamesPlatform.Instance.LoadAchievementDescriptions((achievementsDescription) =>
        //                    {
        //                    //Saftey Check
        //                    if (achievementsDescription == null || achievementsDescription.Length <= 0)
        //                        {
        //                            UnityEngine.Console.Log("Not enough achivement data to display. Faild to get achivement descriptions");
        //                            HandleFailedToGetAchivementData();
        //                            return;
        //                        }

        //                        LinkedList<CustomAchivementData> AchivementDecriptionLinkedList = new LinkedList<CustomAchivementData>();

        //                        foreach (IAchievementDescription AD in achievementsDescription)
        //                        {
        //                            if (AD == null)
        //                            {
        //                                UnityEngine.Console.LogWarning("A desciprtion in achivement description array was null");
        //                                continue;
        //                            }

        //                        // Find the corresponding user achivement status
        //                        IAchievement IA = GetAchivementProgressData(AD.id, userAchivementData);

        //                        //SafteyCheck, No need to terminate process. That particular achivement won't be added to the list anyways
        //                        if (IA == null)
        //                            {
        //                                UnityEngine.Console.LogWarning("Error finding respecive player achivement data. Defaulting to not completed");
        //                                continue;
        //                            }

        //                            if (IA.completed)
        //                            {
        //                                AchivementDecriptionLinkedList.AddFirst(new CustomAchivementData(AD.title, AD.achievedDescription, IA.completed, AD.points.ToString(), AD.id));
        //                            }
        //                            else
        //                            {
        //                                AchivementDecriptionLinkedList.AddLast(new CustomAchivementData(AD.title, AD.unachievedDescription, IA.completed, AD.points.ToString(), AD.id));
        //                            }

        //                            UnityEngine.Console.Log("Achievment Added: Title: " + AD.title + ", Description: " + AD.unachievedDescription + ", is completed? " + IA.completed + ", Points " + AD.points.ToString());
        //                        }

        //                    // Cache the list
        //                    AchivementsLinkedListRef = AchivementDecriptionLinkedList;

        //                        LinkedList<CustomAchivementData> copy = new LinkedList<CustomAchivementData>(AchivementDecriptionLinkedList);

        //                        AchievementLoadingState = AchievementLoadingState.Loaded;

        //                        SendAchievementDataRetreivedSignal(copy, Status.Succeeded);

        //                    //try { ShowAchivementData(AchivementDecriptionLinkedList, true, tempDictionary); }
        //                    //catch { }
        //                });
        //            });
        //        }
        //        catch
        //        {
        //            HandleFailedToGetAchivementData();
        //        }
        //    };

        //    CoroutineRunner.Instance.StartCoroutine(WaitForAFrameAndExecute(ActionToPerform));

        //    return handle;
        //}

        //private IEnumerator WaitForAFrameAndExecute(Action ActionToPerform)
        //{
        //    yield return null;

        //    ActionToPerform();
        //}

        //private void HandleFailedToGetAchivementData()
        //{
        //    AchievementLoadingState = AchievementLoadingState.Stale;

        //    UnityEngine.Console.LogWarning("Failed to fetch Achivements data ");

        //    SendAchievementDataRetreivedSignal(null, Status.Failed);
        //    //try
        //    //{
        //    //    ShowAchivementData(null, false, null);
        //    //}
        //    //catch { }
        //}

        //private void SendAchievementDataRetreivedSignal(LinkedList<CustomAchivementData> data, Status status)
        //{
        //    UnityEngine.Console.Log("Achievement Data Retreived. Status: " + status);

        //    int count = PendingListnersQueue.Count;

        //    for (int i = 0; i < count; i++)
        //    {
        //        AchievementAsyncOperation handle = PendingListnersQueue.Dequeue();
        //        handle.isDone = true;
        //        handle.SendAchievementDataAcquredEvent(data, status);
        //    }
        //}

        //private IAchievement GetAchivementProgressData(string id, IAchievement[] data)
        //{
        //    foreach (IAchievement IA in data)
        //    {
        //        if (IA.id == id)
        //            return IA;
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// Resets the achievement data cache. The operation will fail if the data is currently being loaded.
        ///// </summary>
        ///// <returns>Whether the reset operation was succesfull or not</returns>
        //public bool ResetAchivementCache()
        //{
        //    if (AchievementLoadingState == AchievementLoadingState.Loading)
        //    {
        //        UnityEngine.Console.LogWarning("Trying to reset the achievement data when the loading oepration is in progress");
        //        return false;
        //    }

        //    AchivementsLinkedListRef?.Clear();
        //    AchievementLoadingState = AchievementLoadingState.Stale;
        //    ResetAchievementDataListnersQueue();

        //    UnityEngine.Console.Log("Achievement Data Reset Successfully");
        //    return true;
        //}

        //private void ResetAchievementDataListnersQueue()
        //{
        //    if (PendingListnersQueue == null)
        //    {
        //        PendingListnersQueue = new Queue<AchievementAsyncOperation>();
        //        return;
        //    }

        //    if (PendingListnersQueue.Count > 0)
        //    {
        //        UnityEngine.Console.LogWarning("Resetting the achievements data queue even though it contained pending handlers. Count: " + PendingListnersQueue.Count);
        //        SendAchievementDataRetreivedSignal(null, Status.Failed);
        //        PendingListnersQueue.Clear();
        //    }
        //}
    }
}