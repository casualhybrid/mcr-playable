using deVoid.UIFramework;
using TheKnights.FaceBook;
using UnityEngine;

public class LeaderBoardManager : AWindowController
{
    [SerializeField] private GameObject faceBookButtonGameObject;
    [SerializeField] private RectTransform globalLeaderBoardButtonRectT;

    [SerializeField] private GameObject globalLeaderBoard;
    [SerializeField] private GameObject faceBookLeaderBoard;

    [SerializeField] private Canvas globalLeaderBoardCanvas;
    [SerializeField] private Canvas faceBookLeaderBoardCanvas;

    [SerializeField] private Transform globalLeaderBoardBtnT;
    [SerializeField] private Transform facebookLeaderBoardBtnT;

    [SerializeField] private GameObject facebookBarLowerUI;
    [SerializeField] private GameObject normalBackButton;

    private void OnEnable()
    {
        OpenGlobalLeaderBoard();

        //if (FaceBookManager.isInitialized)
        //{
        //    SetupBackUI();

        //    FaceBookManager.OnUserLoggedInToFaceBook.AddListener(SetupBackUI);
        //}
        //else
        //{
        //    faceBookButtonGameObject.SetActive(false);
        //    Vector2 pos = globalLeaderBoardButtonRectT.anchoredPosition;
        //    pos.x = 0;
        //    globalLeaderBoardButtonRectT.anchoredPosition = pos;
        //}
    }

    private void OnDisable()
    {
        faceBookLeaderBoard.SetActive(false);

        FaceBookManager.OnUserLoggedInToFaceBook.RemoveListener(SetupBackUI);
    }

    public void OpenGlobalLeaderBoard()
    {
        faceBookLeaderBoardCanvas.enabled = false;
        globalLeaderBoard.SetActive(true);
        globalLeaderBoardCanvas.enabled = true;

        ChangeScalesOfButtons(globalLeaderBoardBtnT.transform, facebookLeaderBoardBtnT.transform);
    }

    public void OpenFaceBookLeaderBoard()
    {
        globalLeaderBoardCanvas.enabled = false;
        faceBookLeaderBoard.SetActive(true);
        faceBookLeaderBoardCanvas.enabled = true;

        ChangeScalesOfButtons(facebookLeaderBoardBtnT.transform, globalLeaderBoardBtnT.transform);
    }

    private void ChangeScalesOfButtons(Transform selectedBtn, Transform otherBtn)
    {
        selectedBtn.localScale = Vector3.one;
        otherBtn.localScale = Vector3.one * .86f;
    }

    private void SetupBackUI()
    {
       // facebookBarLowerUI.SetActive(!FaceBookManager.isLoggedInToFaceBook);
       // normalBackButton.SetActive(FaceBookManager.isLoggedInToFaceBook);
    }
}