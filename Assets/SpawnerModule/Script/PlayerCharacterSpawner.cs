using TheKnights.SaveFileSystem;
using UnityEngine;

public class PlayerCharacterSpawner : MonoBehaviour
{
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private CharactersDataBase charactersDataBase;
    [SerializeField] private CarsDataBase carDataBase;
    [SerializeField] private PlayerCharacterLoadingHandler characterLoadingHandler;

    [SerializeField] private GameEvent onPlayerCharacterHasSpawned;
    [SerializeField] private GameEvent onSkeletonSpawned;
    [SerializeField] private GameEvent newCharacterSelected;

    // Character asset cached after loading
    private GameObject loadedCharacterAsset;

    private GameObject currentlySpawnedCharacter;

    private void Awake()
    {
        onSkeletonSpawned.TheEvent.AddListener(HandleSkeletonSpawned);
        newCharacterSelected.TheEvent.AddListener(HandleNewCharacterSelected);
    }

    private void Start()
    {
        // We start loading character without waiting for the car skeleton to finish loading
        LoadPlayerCharacter();
    }

    private void OnDestroy()
    {
        onSkeletonSpawned.TheEvent.RemoveListener(HandleSkeletonSpawned);
        newCharacterSelected.TheEvent.RemoveListener(HandleNewCharacterSelected);
    }

    private void LoadPlayerCharacter()
    {
        int selectedCharacterIndexKey = saveManager.MainSaveFile.currentlySelectedCharacter;
        CharacterConfigData characterConfigurationData = charactersDataBase.GetCharacterConfigurationData(selectedCharacterIndexKey);
        string characterKey = characterConfigurationData.GetLoadingKeyGamePlay;

        ResourceRequest resourceRequest = Resources.LoadAsync<GameObject>($"{characterKey}");

        resourceRequest.completed += (asyncOperation) =>
        {
            if (resourceRequest.asset == null)
            {
                UnityEngine.Console.LogWarning($"Failed to load player character {characterKey}");
                return;
            }

            // Cache the loaded asset
            loadedCharacterAsset = resourceRequest.asset as GameObject;

            SpawnPlayerCharacter();
        };
    }

    private void SpawnPlayerCharacter()
    {
        CarSkeleton carSkeleton = playerSharedData.CarSkeleton;

        // If the character asset hasn't been loaded yet.
        if (loadedCharacterAsset == null)
            return;

        // If the car skeleton hasn't been loaded yet.
        if (carSkeleton == null)
            return;

        GameObject spawnedCharacter = Instantiate(loadedCharacterAsset, carSkeleton.GetBodyTransform);

        CharacterSkeleton characterSkeleton = spawnedCharacter.GetComponent<CharacterSkeleton>();
        playerSharedData.CharacterSkeleton = characterSkeleton;

        if(characterSkeleton == null)
        {
            throw new System.Exception($"Character skeleton is not attached to player character {spawnedCharacter.name}");
        }

        // Set character local position
        var carConfig = carDataBase.GetCarConfigurationData(saveManager.MainSaveFile.currentlySelectedCar);

        spawnedCharacter.transform.localPosition = carConfig.GetCharacterSittingPosition(saveManager.MainSaveFile.currentlySelectedCharacter);

        Animator animator = spawnedCharacter.GetComponent<Animator>();

        if (animator == null)
        {
            throw new System.Exception($"No animator attached to the spawned character {spawnedCharacter.name}");
        }

        // Set References
        playerSharedData.CharacterAnimator = animator;

        playerSharedData.CharacterAnimator.logWarnings = false;

        currentlySpawnedCharacter = spawnedCharacter;

        onPlayerCharacterHasSpawned.RaiseEvent();
    }

    private void HandleSkeletonSpawned(GameEvent gameEvent)
    {
        if (currentlySpawnedCharacter != null)
            return;

        // Car skeleton has been loaded so attempt to spawn player character in it
        SpawnPlayerCharacter();
    }

    private void HandleNewCharacterSelected(GameEvent gameEvent)
    {
        int selectedCharacter = saveManager.MainSaveFile.currentlySelectedCharacter;
        var configData = charactersDataBase.GetCharacterConfigurationData(selectedCharacter);
        var assets = characterLoadingHandler.GetLoadedPlayerAssets(configData.GetName);
        loadedCharacterAsset = assets.GamePlayGameObject;

        Destroy(currentlySpawnedCharacter);

        SpawnPlayerCharacter();
    }
}