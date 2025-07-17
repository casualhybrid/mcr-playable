using Cinemachine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheKnights.SaveFileSystem;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutSceneCore : SerializedMonoBehaviour
{
    [SerializeField] private Dictionary<int, GameObject> cutSceneFolderDictionary;

    [SerializeField] private CarsCutSceneConfiguration carsCutSceneConfiguration;
    [SerializeField] private Transform carGamePlayT;
    [SerializeField] private Transform chaserT;

    [SerializeField] private SaveManager saveManager;

    public GameObject roadPooling;

    [SerializeField] private GameEvent dependenciesDownloaded;
    [SerializeField] private GameEvent cutscenestarted;
    [SerializeField] private GameEvent gameHasStarted;
    [SerializeField] private GameEvent timelineHasFinished;

    [SerializeField] private GameEvent newCarIsGoingToBeSpawned;

    [SerializeField] public Rigidbody[] objToPool;
    [SerializeField] private float objToPoolSpeed;

    [SerializeField] private CinemachineVirtualCamera vcam1;

    [SerializeField] private PlayerSharedData playerSharedData;

    [SerializeField] private float minimumPatchDistanceAvailableWhenCutSceneStarts;

    [SerializeField] private SpeedHandler speedHandler;

    private bool cutSceneInitialize;
    private bool isInitialized;

    public int counter { get; private set; }
    private int endPatchIndex;

    private PlayableDirector playableDirector;

    private void Awake()
    {
        cutscenestarted.TheEvent.AddListener(Gamesarted);
        timelineHasFinished.TheEvent.AddListener(HandleTimeLineFinished);
        dependenciesDownloaded.TheEvent.AddListener(Initialize);
        newCarIsGoingToBeSpawned.TheEvent.AddListener(HandleNewCarGoingToBeSpawned);

        SetupCarsTransformForCutScene();

        EnableSelectedCarCutSceneSetup(null);
    }

    private void OnDestroy()
    {
        cutscenestarted.TheEvent.RemoveListener(Gamesarted);
        timelineHasFinished.TheEvent.RemoveListener(HandleTimeLineFinished);
        dependenciesDownloaded.TheEvent.RemoveListener(Initialize);
        newCarIsGoingToBeSpawned.TheEvent.RemoveListener(HandleNewCarGoingToBeSpawned);
    }

    private void Initialize(GameEvent gameEvent)
    {
        isInitialized = true;
    }

    private void SetupCarsTransformForCutScene()
    {
        CutSceneConfiguration config = carsCutSceneConfiguration.GetCarConfiguration(saveManager.MainSaveFile.currentlySelectedCar);

        carGamePlayT.position = config.GetPlayerCarStartingPosition;
        carGamePlayT.rotation = config.GetPlayerCarStartingRotation;

        chaserT.rotation = config.GetChaserCarStartingRotation;
        chaserT.position = config.GetChaserCarStartingPosition;
    }

    private void HandleNewCarGoingToBeSpawned(GameEvent gameEvent)
    {
        SetupCarsTransformForCutScene();
        EnableSelectedCarCutSceneSetup(gameEvent);
    }

    private void EnableSelectedCarCutSceneSetup(GameEvent gameEvent)
    {
        // isInitialized = true;

        int currentCar = saveManager.MainSaveFile.currentlySelectedCar;

        foreach (var item in cutSceneFolderDictionary.Values)
        {
            item.SetActive(false);
        }

        cutSceneFolderDictionary[currentCar].SetActive(true);
    }

    private void FixedUpdate()
    {
        if (isInitialized && !cutSceneInitialize)
        {
            for (int i = 0; i < objToPool.Length; i++)
            {
                Rigidbody rb = objToPool[i];
                Vector3 pos = rb.position + (Vector3.forward * objToPoolSpeed * Time.fixedDeltaTime);
                rb.MovePosition(pos);
                //objToPool[i].transform.Translate(-Vector3.forward * objToPoolSpeed * Time.fixedDeltaTime);
            }
        }
    }

    public void ChangeRushPlayerAfterCutSceneState(bool status)
    {
        if (status)
        {
            TimelineAsset timelineAsset = playableDirector.playableAsset as TimelineAsset;

            var bindings = timelineAsset.outputs.ToArray();

            PlayableBinding playableBinding = bindings[1];

            playableDirector.SetGenericBinding(playableBinding.sourceObject, null);
            playableDirector.Evaluate();
        }
        else
        {
            if (saveManager.MainSaveFile.TutorialHasCompleted)
            {
                gameHasStarted.RaiseEvent();
                playerSharedData.isStumbled = true;
            }

            StartCoroutine(DisableObjectAndDestroyCutSceneEnv());
        }
    }

    public void FlipCutSceneEnv()
    {
        for (int i = 0; i < objToPool.Length; i++)
        {
            objToPool[i].transform.rotation = Quaternion.identity;
        }
    }

    private void Gamesarted(GameEvent theEvent)
    {
        //roadPooling.GetComponent<RoadPooling>().CutSceneEnv = objToPool[counter].transform.gameObject;

        // roadPooling.SetActive(true);

        float value = objToPoolSpeed;

        cutSceneInitialize = true;

        ExtendCutSceneRoadIfRequired();
    }

    public void MoveToTopOfPriority()
    {
        vcam1.gameObject.SetActive(true);
        vcam1.MoveToTopOfPrioritySubqueue();
    }

    public void ResetCarPivotToSkeleton()
    {
        Transform carSkeletonT = playerSharedData.CarSkeleton.transform;
        Transform carGamePlayT = playerSharedData.PlayerTransform;

        carSkeletonT.parent = null;

        carGamePlayT.position = carSkeletonT.position;

        carSkeletonT.SetParent(playerSharedData.PivotT);
    }

    public void RestartTimeLineCutScene()
    {
        FindObjectOfType<PlayableDirector>().playableGraph.GetRootPlayable(0).SetTime(0);
    }

    public void PoolRoad(bool force = false)
    {
        if (cutSceneInitialize && !force)
            return;

        Rigidbody objectToPoolRB = objToPool[counter];
        Transform objectToPoolTransform = objectToPoolRB.transform;

        objectToPoolRB.interpolation = RigidbodyInterpolation.None;
        Rigidbody lastPatchRb = objToPool.OrderBy(x => x.position.z).First();

        float targetZ = lastPatchRb.GetComponent<Collider>().bounds.min.z;

        float diffBWPivotAndCollider = objectToPoolRB.GetComponent<Collider>().bounds.max.z - objectToPoolRB.position.z;
        objectToPoolTransform.position = new Vector3(objectToPoolTransform.position.x, objectToPoolTransform.position.y, targetZ - diffBWPivotAndCollider);
        objectToPoolRB.interpolation = RigidbodyInterpolation.Interpolate;

        Vector3 lastPatchPos = lastPatchRb.transform.position;
        lastPatchRb.interpolation = RigidbodyInterpolation.None;
        lastPatchRb.transform.position = lastPatchPos;
        lastPatchRb.interpolation = RigidbodyInterpolation.Interpolate;

        Physics.SyncTransforms();

        endPatchIndex = counter;
        counter++;

        if (counter >= objToPool.Length)
        {
            counter = 0;
        }
    }

    private void ExtendCutSceneRoadIfRequired()
    {
        float endingPoint = objToPool[endPatchIndex].GetComponent<Collider>().bounds.min.z;

        float distanceBetweenPlayer_EndPatchPoint = Mathf.Abs(playerSharedData.PlayerTransform.position.z - endingPoint);

        //    UnityEngine.Console.Log($"Distance Between player end patch {distanceBetweenPlayer_EndPatchPoint}");

        if (distanceBetweenPlayer_EndPatchPoint <= minimumPatchDistanceAvailableWhenCutSceneStarts)
        {
            //    UnityEngine.Console.Log("Shifting CutScene Env");
            PoolRoad(true);
        }
    }

    private void HandleTimeLineFinished(GameEvent gameEvent)
    {
        playerSharedData.PlayerAnimator.applyRootMotion = false;
        ResetCarPivotToSkeleton();
    }

    public void CutSceneEnded()
    {
        cutSceneInitialize = false;
        gameHasStarted.RaiseEvent();
    }

    private IEnumerator DisableObjectAndDestroyCutSceneEnv()
    {
        yield return new WaitForSeconds(5);

        float lastPatchZ = objToPool.OrderByDescending(i => i.position.z).First().GetComponent<Collider>().bounds.max.z;
        float playerZ;

        yield return new WaitUntil(() =>
        {
            playerZ = playerSharedData.PlayerTransform.position.z;
            return playerZ - lastPatchZ > 20f;
        });

        for (int i = 0; i < objToPool.Length; i++)
        {
            Destroy(objToPool[i].gameObject);
        }

        gameObject.SetActive(false);
    }
}