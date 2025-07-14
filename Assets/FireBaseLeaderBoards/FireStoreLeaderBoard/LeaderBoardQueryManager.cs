using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace TheKnights.LeaderBoardSystem
{
    /// <summary>
    /// Class containing the web servers constants including URLs, Query URLs etc.
    /// </summary>
    public static partial class WebServerConstants
    {
        /// <summary>
        /// The web server main URL
        /// </summary>
        public const string URL = "http://35.154.97.11:600";

        /// <summary>
        /// The Query URL to get information about players group details
        /// </summary>
        public const string LeaderBoardGroupQuery = "leaderBoard/GroupDetails";

        /// <summary>
        /// The URL to request a highscore update in the players group
        /// </summary>
        public const string LeaderBoardHighScoreUpdateRequest = "leaderBoard/InfoUpdate/HighScore";

        /// <summary>
        /// The URL to request a group profile update in the players group
        /// </summary>
        public const string LeaderBoardGroupProfileUpdateRequest = "leaderBoard/InfoUpdate/Profile";

        /// <summary>
        /// The URL to request a reward claim/deletion
        /// </summary>
        public const string LeaderBoardPendingRewardDeletionRequest = "leaderBoard/InfoUpdate/Rewards";
    }

    /// <summary>
    /// The result of a http request containing the status and response body
    /// </summary>
    public readonly struct LeaderBoardQueryResult<T>
    {
        public bool IsValid { get; }
        public T ResponseBody { get; }
        public bool IsError { get; }
        public Exception Exception { get; }

        public long ResponseCode { get; }

        public LeaderBoardQueryResult(T _responseBody, bool _isError, Exception _exception, long _responseCode)
        {
            IsValid = true;
            ResponseBody = _responseBody;
            IsError = _isError;
            Exception = _exception;
            ResponseCode = _responseCode;
        }
    }

    public class LeaderBoardQueryManager : Singleton<LeaderBoardQueryManager>
    {
        [System.Serializable]
        private class PlayerGroupQueryRequestBody
        {
            public string idToken;
        }

        [System.Serializable]
        private class PlayerHighScoreSubmissionRequestBody
        {
            public string idToken;
            public int score;
        }

        [System.Serializable]
        private class PlayerClaimRewardSubmissionRequestBody
        {
            public string idToken;
            public string stamp;
        }

        /// <summary>
        /// Invoked whenever a leaderboard group query operation is performed irrelevant of the result
        /// </summary>
        public static event Action<LeaderBoardQueryResult<string>> OnLeaderBoardGroupInfoOperationDone;

        /// <summary>
        /// Invoked whenever a leaderboard high score update request operation is done irrelevant of the result
        /// </summary>
        public static event Action<LeaderBoardQueryResult<string>> OnLeaderBoardHighScoreOperationDone;

        /// <summary>
        /// Invoked whenever a leaderboard group profile update request operation is done irrelevant of the result
        /// </summary>
        public static event Action<LeaderBoardQueryResult<string>> OnLeaderBoardGroupProfileUpdateOperationDone;

        /// <summary>
        /// Invoked whenever a leaderboard a reward claim_delete request operation is done
        /// </summary>
        public static event Action<LeaderBoardQueryResult<string>> OnLeaderBoardRewardClaimOperationDone;

        private bool isPerformingGroupDetailsQuery;
        private bool isPerformingHighScoreRequestQuery;

        #region GroupDetailsQuery

        /// <summary>
        /// Post an HTTP request to query about the user's current group information
        /// </summary>
        /// <param name="playerToken">The authentication token of the player</param>
        public void RequestPlayersGroupInformation(string playerToken)
        {
            //if (isPerformingGroupDetailsQuery)
            //{
            //    return;
            //}

            if (string.IsNullOrEmpty(playerToken))
            {
                string message = "Request to query group information was recieved but the provided token is either null or empty";
                UnityEngine.Console.LogWarning(message);

                var result = new LeaderBoardQueryResult<string>(null, true, new Exception(message), -1);
                OnLeaderBoardGroupInfoOperationDone?.Invoke(result);
                return;
            }

            isPerformingGroupDetailsQuery = true;

            StartCoroutine(RequestPlayerGroupInfoRoutine(playerToken));
        }

        private IEnumerator RequestPlayerGroupInfoRoutine(string playerToken)
        {
            LeaderBoardQueryResult<string> result = default;

            using (UnityWebRequest request = new UnityWebRequest($"{WebServerConstants.URL}/{WebServerConstants.LeaderBoardGroupQuery}", "POST"))
            {
                try
                {
                    string playerRequestBodyJson = JsonUtility.ToJson(new PlayerGroupQueryRequestBody() { idToken = playerToken });
                    PrepareJsonWebRequest(request, playerRequestBodyJson);
                }
                catch (Exception e)
                {
                    isPerformingGroupDetailsQuery = false;
                    UnityEngine.Console.LogError($"Something went wrong when executing group request query. Reason: {e}");
                    result = new LeaderBoardQueryResult<string>(null, true, e, request.responseCode);
                    OnLeaderBoardGroupInfoOperationDone?.Invoke(result);

                    yield break;
                }

                yield return request.SendWebRequest();

                try
                {
                    UnityEngine.Console.Log($"LeaderBoard group query response recieved. Status Code: ${request.responseCode}");

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string responseText = request.downloadHandler.text;

                        UnityEngine.Console.Log("Res " + responseText);

                        result = new LeaderBoardQueryResult<string>(responseText, false, null, request.responseCode);
                    }
                    else
                    {
                        UnityEngine.Console.Log($"Leaderboard group query failed. Reason : {request.error}");

                        result = new LeaderBoardQueryResult<string>(null, true, new Exception(request.error), request.responseCode);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Console.LogError($"Something went wrong when executing group request query. Reason: {e}");
                    result = new LeaderBoardQueryResult<string>(null, true, e, request.responseCode);
                }
                finally
                {
                    isPerformingGroupDetailsQuery = false;
                    OnLeaderBoardGroupInfoOperationDone?.Invoke(result);
                }
            }
        }

        #endregion GroupDetailsQuery

        #region HighScoreSubmission

        /// <summary>
        /// Post an HTTP request to request a highscore update for the player current group
        /// </summary>
        /// <param name="playerToken">The authentication token of the player</param>
        ///  /// <param name="scoreToUpdate">The highscore to be updated</param>
        public void RequestHighScoreSubmissionToGroup(string playerToken, int scoreToUpdate)
        {
            //if (isPerformingHighScoreRequestQuery)
            //{
            //    return;
            //}

            if (string.IsNullOrEmpty(playerToken))
            {
                string message = "Request to submit high score information was recieved but the provided token is either null or empty";
                UnityEngine.Console.LogWarning(message);
                var result = new LeaderBoardQueryResult<string>(null, true, new Exception(message), -1);
                OnLeaderBoardHighScoreOperationDone?.Invoke(result);
                return;
            }

            isPerformingHighScoreRequestQuery = true;

            StartCoroutine(RequestHighScoreSubmissionToGroupRoutine(playerToken, scoreToUpdate));
        }

        private IEnumerator RequestHighScoreSubmissionToGroupRoutine(string playerToken, int scoreToUpdate)
        {
            LeaderBoardQueryResult<string> result = default;

            using (UnityWebRequest request = new UnityWebRequest($"{WebServerConstants.URL}/{WebServerConstants.LeaderBoardHighScoreUpdateRequest}", "PUT"))
            {
                try
                {
                    string playerRequestBodyJson = JsonUtility.ToJson(new PlayerHighScoreSubmissionRequestBody() { idToken = playerToken, score = scoreToUpdate });
                    PrepareJsonWebRequest(request, playerRequestBodyJson);
                }
                catch (Exception e)
                {
                    isPerformingHighScoreRequestQuery = false;
                    UnityEngine.Console.LogError($"Something went wrong when executing high score submission request. Reason: {e}");
                    result = new LeaderBoardQueryResult<string>(null, true, e, request.responseCode);
                    OnLeaderBoardHighScoreOperationDone?.Invoke(result);
                    yield break;
                }

                yield return request.SendWebRequest();

                try
                {
                    UnityEngine.Console.Log($"LeaderBoard high score submission response recieved. Status Code: ${request.responseCode}");

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        result = new LeaderBoardQueryResult<string>(request.result.ToString(), false, null, request.responseCode);
                    }
                    else
                    {
                        UnityEngine.Console.Log($"Leaderboard highscore submission failed. Reason : {request.error}");
                        result = new LeaderBoardQueryResult<string>(request.result.ToString(), true, new Exception(request.error), request.responseCode);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Console.LogError($"Something went wrong when executing high score submission request. Reason: {e}");
                    result = new LeaderBoardQueryResult<string>(null, true, e, request.responseCode);
                }
                finally
                {
                    isPerformingHighScoreRequestQuery = false;
                    OnLeaderBoardHighScoreOperationDone?.Invoke(result);
                }
            }
        }

        #endregion HighScoreSubmission

        #region RewardClaimRequest

        /// <summary>
        /// PUTS an HTTP delete request to claim the provided reward and deleting it
        /// </summary>
        /// <param name="playerToken">The authentication token of the player</param>
        ///  /// <param name="rewardStamp">The reward stamp to be claimed and deleted</param>
        public void RequestPendingRewardDeletion(string playerToken, string rewardStamp)
        {
            //if (isPerformingHighScoreRequestQuery)
            //{
            //    return;
            //}

            if (string.IsNullOrEmpty(playerToken))
            {
                string message = $"Request to claim a reward with stamp {rewardStamp} was recieved but the provided token is either null or empty";
                UnityEngine.Console.LogWarning(message);
                var result = new LeaderBoardQueryResult<string>(null, true, new Exception(message), -1);
                OnLeaderBoardRewardClaimOperationDone?.Invoke(result);
                return;
            }

            //   isPerformingHighScoreRequestQuery = true;

            StartCoroutine(RequestRewardClaimRoutine(playerToken, rewardStamp));
        }

        private IEnumerator RequestRewardClaimRoutine(string playerToken, string rewardStamp)
        {
            LeaderBoardQueryResult<string> result = default;

            using (UnityWebRequest request = new UnityWebRequest($"{WebServerConstants.URL}/{WebServerConstants.LeaderBoardPendingRewardDeletionRequest}", "DELETE"))
            {
                try
                {
                    string playerRequestBodyJson = JsonUtility.ToJson(new PlayerClaimRewardSubmissionRequestBody() { idToken = playerToken, stamp = rewardStamp });
                    PrepareJsonWebRequest(request, playerRequestBodyJson);
                }
                catch (Exception e)
                {
                    //isPerformingHighScoreRequestQuery = false;
                    UnityEngine.Console.LogError($"Something went wrong when executing reward claim request. Reason: {e}");
                    result = new LeaderBoardQueryResult<string>(null, true, e, request.responseCode);
                    OnLeaderBoardRewardClaimOperationDone?.Invoke(result);
                    yield break;
                }

                yield return request.SendWebRequest();

                try
                {
                    UnityEngine.Console.Log($"LeaderBoard rewarda claim response recieved. Status Code: ${request.responseCode}");

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        result = new LeaderBoardQueryResult<string>(request.result.ToString(), false, null, request.responseCode);
                    }
                    else
                    {
                        UnityEngine.Console.Log($"Leaderboard reward claim failed. Reason : {request.error}");
                        result = new LeaderBoardQueryResult<string>(request.result.ToString(), true, new Exception(request.error), request.responseCode);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Console.LogError($"Something went wrong when executing reward claim request. Reason: {e}");
                    result = new LeaderBoardQueryResult<string>(null, true, e, request.responseCode);
                }
                finally
                {
                    //  isPerformingHighScoreRequestQuery = false;
                    OnLeaderBoardRewardClaimOperationDone?.Invoke(result);
                }
            }
        }

        #endregion RewardClaimRequest

        #region ProfileUpdateRequest

        /// <summary>
        /// Post an HTTP request to request a highscore update for the player current group
        /// </summary>
        /// <param name="playerToken">The authentication token of the player</param>
        ///  /// <param name="scoreToUpdate">The highscore to be updated</param>
        public void RequestUpdateGroupProfile(string playerToken)
        {
            //if (isPerformingHighScoreRequestQuery)
            //{
            //    return;
            //}

            if (string.IsNullOrEmpty(playerToken))
            {
                string message = "Request to update group profile was recieved but the provided token is either null or empty";
                UnityEngine.Console.LogWarning(message);
                var result = new LeaderBoardQueryResult<string>(null, true, new Exception(message), -1);
                OnLeaderBoardGroupProfileUpdateOperationDone?.Invoke(result);
                return;
            }

            // isPerformingHighScoreRequestQuery = true;

            StartCoroutine(RequsetGroupProfileUpdateRoutine(playerToken));
        }

        private IEnumerator RequsetGroupProfileUpdateRoutine(string playerToken)
        {
            LeaderBoardQueryResult<string> result = default;

            using (UnityWebRequest request = new UnityWebRequest($"{WebServerConstants.URL}/{WebServerConstants.LeaderBoardGroupProfileUpdateRequest}", "PUT"))
            {
                try
                {
                    string playerRequestBodyJson = JsonUtility.ToJson(new PlayerGroupQueryRequestBody() { idToken = playerToken });
                    PrepareJsonWebRequest(request, playerRequestBodyJson);
                }
                catch (Exception e)
                {
                    //   isPerformingHighScoreRequestQuery = false;
                    UnityEngine.Console.LogError($"Something went wrong when executing group profile update request. Reason: {e}");
                    result = new LeaderBoardQueryResult<string>(null, true, e, request.responseCode);
                    OnLeaderBoardGroupProfileUpdateOperationDone?.Invoke(result);
                    yield break;
                }

                yield return request.SendWebRequest();

                try
                {
                    UnityEngine.Console.Log($"LeaderBoard group profile update response recieved. Status Code: ${request.responseCode}");

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string responseText = request.downloadHandler.text;
                        result = new LeaderBoardQueryResult<string>(responseText, false, null, request.responseCode);
                    }
                    else
                    {
                        UnityEngine.Console.Log($"Leaderboard group profile update failed. Reason : {request.error}");
                        result = new LeaderBoardQueryResult<string>(request.result.ToString(), true, new Exception(request.error), request.responseCode);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Console.LogError($"Something went wrong when executing group profile update request. Reason: {e}");
                    result = new LeaderBoardQueryResult<string>(null, true, e, request.responseCode);
                }
                finally
                {
                    // isPerformingHighScoreRequestQuery = false;
                    OnLeaderBoardGroupProfileUpdateOperationDone?.Invoke(result);
                }
            }
        }

        #endregion ProfileUpdateRequest

        private void PrepareJsonWebRequest(UnityWebRequest request, string reqJsonBody)
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(reqJsonBody);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        }
    }
}