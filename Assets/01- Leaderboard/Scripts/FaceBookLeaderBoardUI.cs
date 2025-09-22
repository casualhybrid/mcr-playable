using deVoid.UIFramework;
using Knights.UISystem;
using System;
using TheKnights.FaceBook;
using TheKnights.LeaderBoardSystem.FaceBook;
using UnityEngine;
using UnityEngine.UI;

public class FaceBookLeaderBoardUI : MonoBehaviour
{
    [SerializeField] private AWindowController leaderboardController;
    [SerializeField] private FaceBookLeaderBoard faceBookLeaderBoard;
    [SerializeField] private FaceBookLeaderBoardPrefab faceBookLeaderBoardPrefab;
    [SerializeField] private GameObject connectionErrorObject;
    [SerializeField] private Text connectionErrorTxt;
    [SerializeField] private GameObject loadingObject;
    [SerializeField] private GameObject tryAgainObject;
    [SerializeField] private GameObject loginObject;

    [SerializeField] private Transform contentT;

    private bool isLoading;
    private bool isLoaded;

    private bool hasShownLoginToFacebookPopup;

    private bool isDestroyed;

    private void OnEnable()
    {
        FaceBookManager.OnUserLoggedInToFaceBook.AddListener(LoadFaceBookLeaderBoard);
        LoadFaceBookLeaderBoard();
    }

    private void OnDisable()
    {
        FaceBookManager.OnUserLoggedInToFaceBook.RemoveListener(LoadFaceBookLeaderBoard);
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void LoadFaceBookLeaderBoard()
    {
        if (isLoaded)
        {
            if (faceBookLeaderBoard.CachedLeaderBoardData == null)
            {
                ResetTheLeaderBoard();
                isLoaded = false;
            }
            else
            {
                return;
            }
        }

        if (isLoading)
        {
            return;
        }

        ResetTheLeaderBoard();

        DisableConnectionErrorDetails();
        EnableLoadingDetails();
        LoadTheLeaderBoard();
    }

    private async void LoadTheLeaderBoard()
    {
        try
        {
            FaceBookLeaderBoardData data = await faceBookLeaderBoard.GetFaceBookLeaderBoardData();

            // Maybe scene changed while we were waiting
            if (isDestroyed)
                return;

            if (data == null)
            {
                throw new System.Exception("Data is null");
            }

            foreach (var userData in data.Data)
            {
                FaceBookLeaderBoardPrefab entity = Instantiate(faceBookLeaderBoardPrefab, contentT);
              //  entity.SetInformation(userData.name, userData.score.ToString(), userData.profilePicTex, userData == data.LocalPlayerData);
            }

            DisableLoadingDetails();
            isLoaded = true;
        }
        catch (Exception e)
        {
            // Maybe scene changed while we were waiting
            if (isDestroyed)
                return;

            UnityEngine.Console.Log($"Something went wrong when trying to display facebook leaderboard UI. Exception: {e}");

            ResetTheLeaderBoard();
            DisableLoadingDetails();
            EnableConnectionErrorDetails(e);

            bool hasInternetConnection = e == null || e.GetType() != typeof(InternetNotAvailableException);

            //if (hasInternetConnection && !FaceBookManager.isLoggedInToFaceBook && !hasShownLoginToFacebookPopup)
            //{
            //    hasShownLoginToFacebookPopup = true;
            //    leaderboardController.OpenTheWindow(ScreenIds.FaceBookLoginPopup);

            //    return;
            //}
        }
    }

    private void ResetTheLeaderBoard()
    {
        int childCount = contentT.childCount - 1;

        if (childCount < 0)
            return;

        for (int i = childCount; i >= 0; i--)
        {
            Destroy(contentT.GetChild(i).gameObject);
        }
    }

    private void EnableLoadingDetails()
    {
        isLoading = true;
        loadingObject.SetActive(true);
    }

    private void DisableLoadingDetails()
    {
        isLoading = false;
        loadingObject.SetActive(false);
    }

    private void EnableConnectionErrorDetails(Exception e = null)
    {
        connectionErrorObject.SetActive(true);

        bool hasInternetConnection = e == null || e.GetType() != typeof(InternetNotAvailableException);

        //if (hasInternetConnection && !FaceBookManager.isLoggedInToFaceBook)
        //{
        //    connectionErrorTxt.text = "Not logged in to FaceBook";
        //    tryAgainObject.SetActive(false);
        //    loginObject.SetActive(true);
        //}
        //else
        //{
        //    connectionErrorTxt.text = "Ooops! Connection Error. Please try again later";
        //    tryAgainObject.SetActive(true);
        //    loginObject.SetActive(false);
        //}
    }

    private void DisableConnectionErrorDetails()
    {
        connectionErrorObject.SetActive(false);
    }
}