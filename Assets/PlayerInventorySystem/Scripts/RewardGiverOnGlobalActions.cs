using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKnights.SaveFileSystem;
using TheKnights.FaceBook;
public class RewardGiverOnGlobalActions : Singleton<RewardGiverOnGlobalActions>
{
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private InventorySystem playerInventory;

    protected override void Awake()
    {
        base.Awake();
        FaceBookManager.OnUserLoggedInToFaceBook.AddListener(GrantFaceBookLogInReward);
    }

    private void OnDestroy()
    {
        FaceBookManager.OnUserLoggedInToFaceBook.RemoveListener(GrantFaceBookLogInReward);
    }

    private void GrantFaceBookLogInReward()
    {
        if (saveManager.MainSaveFile == null || saveManager.MainSaveFile.HasSignedInToFaceBookOnce)
            return;

        AnalyticsManager.CustomData("SignedInToFaceBookFirstTime");
        saveManager.MainSaveFile.HasSignedInToFaceBookOnce = true;

        playerInventory.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("GameMysteryBox", 1, true) }, true, true, "FaceBookLoginReward");

        UnityEngine.Console.Log("Player was rewarded on facebook login");

        FaceBookManager.OnUserLoggedInToFaceBook.RemoveListener(GrantFaceBookLogInReward);

    }
}
