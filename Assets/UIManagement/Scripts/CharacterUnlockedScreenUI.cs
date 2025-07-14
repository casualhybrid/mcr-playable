using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using TheKnights.SaveFileSystem;
using UnityEngine.UI;

[System.Serializable]
public class CharactersAvailableProperties : WindowProperties
{
    [HideInInspector] public List<PlayableObjectDataWithIndex> CharactersConfigData;
}


public class CharacterUnlockedScreenUI : AWindowController<CharactersAvailableProperties>
{
    [SerializeField] private TextMeshProUGUI characterNameTxt;
    [SerializeField] private PlayerCharacterLoadingHandler loadingHandler;
    [SerializeField] private Transform parentObjForCharacter;
    [SerializeField] private Camera cam;
    [SerializeField] private RawImage characterRawImage;

    private AsyncOperationSpawning<PlayerCharacterAssets> handler;
    private GameObject instantiatedCharacter;
    private RenderTexture renderTexture;

    private int characterIndex;

    private void OnEnable()
    {
        renderTexture = RenderTexture.GetTemporary(1024, 1024, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
        cam.targetTexture = renderTexture;
        characterRawImage.texture = renderTexture;
    }

    private void OnDisable()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            renderTexture = null;
        }

        CleanUp();

    }

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        characterIndex = 0;
        LoadAndShowCharacter();
    }

    private void Handler_AssetsLoaded(PlayerCharacterAssets obj)
    {
        instantiatedCharacter = Instantiate(obj.DisplayGameObject, parentObjForCharacter, false);
    }

    private void LoadAndShowCharacter()
    {
        PlayableObjectDataWithIndex characterConfig = Properties.CharactersConfigData[characterIndex];
        string characterName = characterConfig.basicAssetInfo.GetName;
        characterNameTxt.text = characterName;

        handler = loadingHandler.LoadAssets(characterConfig.basicAssetInfo);
        handler.AssetsLoaded += Handler_AssetsLoaded;

        characterIndex++;
    }

    private void CleanUp()
    {
        if (handler != null)
        {
            handler.AssetsLoaded -= Handler_AssetsLoaded;
            handler = null;
        }

        if (instantiatedCharacter != null)
        {
            Destroy(instantiatedCharacter);
        }
    }

    public void ShowNextCarUnlockedOrCloseIfNone()
    {
        if (characterIndex >= Properties.CharactersConfigData.Count)
        {
            UI_Close();
            return;
        }

        CleanUp();
        LoadAndShowCharacter();
    }
}
