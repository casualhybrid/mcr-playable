using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using TheKnights.SaveFileSystem;
using UnityEngine.UI;

[System.Serializable]
public class CarsAvailableProperties : WindowProperties
{
    [HideInInspector] public List<PlayableObjectDataWithIndex> CarsConfigData;
}


public class CarUnlockedScreenUI : AWindowController<CarsAvailableProperties>
{
    [SerializeField] private TextMeshProUGUI carNameTxt;
    [SerializeField] private PlayerCarLoadingHandler loadingHandler;
    [SerializeField] private Transform parentObjForCars;
    [SerializeField] private Camera cam;
    [SerializeField] private RawImage carRawImage;

    private AsyncOperationSpawning<PlayerCarAssets> handler;
    private GameObject instantiatedCar;
    private RenderTexture renderTexture;

    private int carIndex;

    private void OnEnable()
    {
        renderTexture = RenderTexture.GetTemporary(1024, 1024, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
        cam.targetTexture = renderTexture;
        carRawImage.texture = renderTexture;
    }

    private void OnDisable()
    {
        if(renderTexture != null)
        {
            renderTexture.Release();
            renderTexture = null;
        }

        CleanUp();

    }

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        carIndex = 0;
        LoadAndShowCar();
    }

    private void Handler_AssetsLoaded(PlayerCarAssets obj)
    {
        instantiatedCar = Instantiate(obj.DisplayGameObject, parentObjForCars.transform, false);
        instantiatedCar.GetComponent<Animator>().SetBool("carEnter", true);
    }

    private void LoadAndShowCar()
    {
        PlayableObjectDataWithIndex carConfig = Properties.CarsConfigData[carIndex];
        string carName = carConfig.basicAssetInfo.GetName;
        carNameTxt.text = carName;

        handler = loadingHandler.LoadAssets(carConfig.basicAssetInfo);
        handler.AssetsLoaded += Handler_AssetsLoaded;

        carIndex++;
    }

    private void CleanUp()
    {
        if (handler != null)
        {
            handler.AssetsLoaded -= Handler_AssetsLoaded;
            handler = null;
        }

        if (instantiatedCar != null)
        {
            Destroy(instantiatedCar);
        }
    }

    public void ShowNextCarUnlockedOrCloseIfNone()
    {
        if(carIndex >= Properties.CarsConfigData.Count)
        {
            UI_Close();
            return;
        }

        CleanUp();
        LoadAndShowCar();
    }

 
}
