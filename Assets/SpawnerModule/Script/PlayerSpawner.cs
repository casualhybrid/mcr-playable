using TheKnights.SaveFileSystem;
using TheKnights.SceneLoadingSystem;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Tooltip("The Base Module the skeleton will spawn in")]
    [SerializeField] private Transform carBaseT;

    [SerializeField] private GameEvent skeletonSpawned;
    [SerializeField] private GameEvent newCarSelected;

    [SerializeField] private SceneLoader sceneLoader;

    [SerializeField] private SaveManager saveManager;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private CarsDataBase carsDataBase;
    [SerializeField] private PlayerCarLoadingHandler carLoadingHandler;

    private GameObject currentlySpawnedCar;

    private void Awake()
    {
        newCarSelected.TheEvent.AddListener(HandleNewCarSelected);
    }

    private void OnDisable()
    {
        newCarSelected.TheEvent.RemoveListener(HandleNewCarSelected);
    }

    private void Start()
    {
        PlayerSpawnFunc();
    }

    // Loads the skelton into the pre-existing car module in the scene
    private void PlayerSpawnFunc()
    {
        int selectedCarIndexKey = saveManager.MainSaveFile.currentlySelectedCar;
        CarConfigurationData carConfigurationData = carsDataBase.GetCarConfigurationData(selectedCarIndexKey);
        string carKey = carConfigurationData.GetLoadingKeyGamePlay;

        // Request car assets load
        var configData = carsDataBase.GetCarConfigurationData(selectedCarIndexKey);

        var handle = carLoadingHandler.LoadAssets(configData); 

        handle.AssetsLoaded += (assets) =>
        {
            if (assets == null)
            {
                UnityEngine.Console.LogWarning($"Failed to load player car {carKey}");
                return;
            }

            GameObject assetGameObject = assets.GamePlayGameObject;
            SpawnTheCar(assetGameObject);
        };

        //ResourceRequest resourceRequest = Resources.LoadAsync<GameObject>($"{carKey}");

        //resourceRequest.completed += (asyncOperation) =>
        //{
        //    if (resourceRequest.asset == null)
        //    {
        //        UnityEngine.Console.LogWarning($"Failed to load player car {carKey}");
        //        return;
        //    }

        //    GameObject assetGameObject = resourceRequest.asset as GameObject;
        //    SpawnTheCar(assetGameObject);
        //};

        #region OnlyRequiredForAddressables

        //AddressableOperationHandle<GameObject> skeletonHandle = AddressableLoader.LoadTheAsset<GameObject>(saveManager.MainSaveFile.CarName + "Skeleton");

        //skeletonHandle.Completed += (skeletonRes) =>
        //{
        //    if (skeletonRes.Result == null)
        //    {
        //        throw new System.Exception($"Car skeleton failed to load! {SaveManager.Instance.MainSaveFile.CarName}");
        //    }

        //    GameObject skeleton = Instantiate(skeletonRes.Result, carBaseT);

        //    skeletonSpawned.RaiseEvent();
        //    playerSpawned.RaiseEvent();
        //    sceneLoader.TurnOffLoadingCanvas();
        //};

        #endregion OnlyRequiredForAddressables
    }

    private void SpawnTheCar(GameObject asset)
    {
        GameObject skeleton = Instantiate(asset, carBaseT);

        CarSkeleton carSkelteon = skeleton.GetComponent<CarSkeleton>();
        playerSharedData.CarSkeleton = carSkelteon;

        // Set References
        currentlySpawnedCar = skeleton;
        playerSharedData.PlayerAnimator = carSkelteon.TheAnimator;
        playerSharedData.DiamondTargetPoint = carSkelteon.DiamondTargetPoint;

        playerSharedData.PlayerAnimator.logWarnings = false;

        skeletonSpawned.RaiseEvent();
    }

    private void HandleNewCarSelected(GameEvent gameEvent)
    {
        Transform currentlySpawnedCharacter = playerSharedData.CharacterAnimator.transform;

        currentlySpawnedCharacter.SetParent(null);
        Destroy(currentlySpawnedCar);

        var configData = carsDataBase.GetCarConfigurationData(saveManager.MainSaveFile.currentlySelectedCar);
        var asset = carLoadingHandler.GetLoadedPlayerAssets(configData.GetName);

        SpawnTheCar(asset.GamePlayGameObject);

        CarSkeleton carSkeleton = currentlySpawnedCar.GetComponent<CarSkeleton>();
        currentlySpawnedCharacter.SetParent(carSkeleton.GetBodyTransform);
        var carConfig = carsDataBase.GetCarConfigurationData(saveManager.MainSaveFile.currentlySelectedCar);
        currentlySpawnedCharacter.localPosition = carConfig.GetCharacterSittingPosition(saveManager.MainSaveFile.currentlySelectedCharacter);
    }
}