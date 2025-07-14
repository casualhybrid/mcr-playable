using DG.Tweening;
using System.Collections;
using UnityEngine;

public class CutSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCar;
    [SerializeField] private GameObject chaserCar, Truck, normalCar;
    [SerializeField] private GameEvent cutscenestarted;
    [SerializeField] private GameEvent gameHasStarted;
    [SerializeField] public GameObject[] objToPool;
    [SerializeField] private float objToPoolSpeed;
    [SerializeField] private float valueToAdd;

    [SerializeField] private float playerCarSpeed, chasercarSpeed;
    [SerializeField] private Rigidbody playerRigidBody;
    [SerializeField] private PlayerSharedData PlayerSharedData;

    // GameEvents
    [SerializeField] private GameEvent playerHasSpawnedEvent;

    public GameObject roadPooling;

    private bool cutSceneInitialize;
    private bool isInitialized;
    public int counter;
    public bool gameStarted;

    private void Awake()
    {
        cutSceneInitialize = false;
        gameStarted = false;
        counter = 0;
    }

    private void OnEnable()
    {
        cutscenestarted.TheEvent.AddListener(Gamesarted);
        playerHasSpawnedEvent.TheEvent.AddListener(HandlePlayerSpawned);
        StartCoroutine(RandomizeObjects());
    }

    private IEnumerator RandomizeObjects()
    {
        Truck.SetActive((UnityEngine.Random.value <= 0.5f));
        normalCar.SetActive((UnityEngine.Random.value <= 0.5f));

        yield return new WaitForSecondsRealtime(3f);
        StartCoroutine(RandomizeObjects());
    }

    private void OnDisable()
    {
        cutscenestarted.TheEvent.RemoveListener(Gamesarted);
        playerHasSpawnedEvent.TheEvent.RemoveListener(HandlePlayerSpawned);
    }

    // Update is called once per frame
    private void Update()
    {
        if (PlayerSharedData.ChaserWasHit)
        {
            Rotation();
            PlayerSharedData.ChaserWasHit = false;
        }
        if (cutSceneInitialize)
        {
            playerCar.transform.position -= Vector3.forward * playerCarSpeed * Time.deltaTime;
        }

        if (gameStarted)
        {
            chaserCar.transform.localPosition -= Vector3.forward * chasercarSpeed * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (isInitialized && !cutSceneInitialize)
        {
            for (int i = 0; i < objToPool.Length; i++)
            {
                objToPool[i].transform.Translate(-Vector3.forward * objToPoolSpeed * Time.deltaTime);
            }
        }
    }

    private void Gamesarted(GameEvent theEvent)
    {
        roadPooling.GetComponent<RoadPooling>().CutSceneEnv = objToPool[counter].transform.gameObject;

        roadPooling.SetActive(true);

        cutSceneInitialize = true;
        gameStarted = true;

        chaserCar.gameObject.SetActive(true);
        chaserCar.transform.eulerAngles = new Vector3(0, 180, 0);
        chaserCar.transform.parent.gameObject.transform.position = new Vector3(playerCar.transform.position.x + 0.2f, playerCar.transform.position.y, playerCar.transform.position.z - 3f);
    }

    private void Rotation()
    {
        chaserCar.transform.DOLocalMoveZ(chaserCar.transform.localPosition.z - 3f, 0.2f).From(chaserCar.transform.localPosition.z).SetEase(Ease.Linear).SetAutoKill(true);
        playerCarSpeed = 0;
        chaserCar.transform.DOLocalRotate(new Vector3(0, 360f, 0), 1, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(1).SetAutoKill(true);
        playerCar.transform.DOLocalRotate(new Vector3(0, 720f, 0), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(1).SetAutoKill(true).OnComplete(() =>
        {
            //
          //  UnityEngine.Console.Log("Tutorial Ended");
            chaserCar.GetComponent<ChaserRunner>().enabled = true;
            chaserCar.gameObject.SetActive(false);
            cutSceneInitialize = false;
            gameHasStarted.RaiseEvent();
            gameObject.SetActive(false);
        });
    }

    public void PoolRoad(GameObject objToMove)
    {
        if (!gameStarted)
        {
            objToPool[counter].transform.position = new Vector3(objToPool[counter].transform.position.x, objToPool[counter].transform.position.y, objToPool[counter].transform.position.z + valueToAdd);

            counter++;
            if (counter >= objToPool.Length)
                counter = 0;
        }
    }

    private void HandlePlayerSpawned(GameEvent theEvent)
    {
        isInitialized = true;
    }
}