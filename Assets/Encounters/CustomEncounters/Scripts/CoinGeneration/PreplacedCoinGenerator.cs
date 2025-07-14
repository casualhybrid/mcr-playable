using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PreplacedCoinGenerator : MonoBehaviour // Implement IValidatable
{
#region Coin Generation Types
    [System.Serializable]
    public class CoinRow
    {
        [Header("Coin Row")]
        public CoinConnectablePoint coinSpawnStartPoint;
        public CoinConnectablePoint coinSpawnEndPoint;
        public bool generateCoinAtStartPoint = false;
    }

    [System.Serializable]
    public class CoinArc
    {
        [Header("Coin Arc")]
        public CoinConnectablePoint coinSpawnStartPoint;
        public CoinConnectablePoint coinSpawnEndPoint; // Z-Value of this point is considered
        public bool includeAscend = true;
    }
#endregion

#region Variables
    [Header("References")]
    [SerializeField] private CoinsArray coinArrayPrefab;
    [SerializeField] private CoinGenerationData coinGenerationData;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerSharedData playerSharedData;

    [Space]
    [SerializeField] private GameObject coinPrefabRoot;
    [SerializeField] private Mesh coinPrefabMesh;
    private Transform coinMeshObjectTransform;
    
    [Header("Generation Points")]
    [SerializeField] private CoinRow[] coinRows;
    [SerializeField] private CoinArc[] coinArcs;

    [SerializeField] private bool isSpringJump = false;

    [Header("Status")]
    private bool areCoinsSpawned;

    private readonly List<CoinsArray> coinsArrayList = new List<CoinsArray>();
#endregion

#if UNITY_EDITOR

#region Gizmos
    private void OnDrawGizmos()
    {
        SpringRampHandler springRampHandler = GetComponent<SpringRampHandler>();

        if (coinArrayPrefab == null || coinPrefabMesh == null || coinPrefabRoot == null || coinGenerationData == null || areCoinsSpawned
            || (springRampHandler != null && !springRampHandler.ForceSpawnSpringRamp && transform.root.GetComponent<CustomEncounter>()) || Application.isPlaying)
            return;

        Gizmos.color = new Color(1, 0.9f, 0.2f, 1f);

        if (coinMeshObjectTransform == null)
            coinMeshObjectTransform = coinPrefabRoot.GetComponentInChildren<MeshRenderer>().transform;

        Vector3 coinPosOffset = coinMeshObjectTransform.position;
        Vector3 coinMeshSize = coinMeshObjectTransform.lossyScale;

        int counter = 0;

        foreach(CoinRow coinRow in coinRows)
        {
            if (coinRow.coinSpawnStartPoint == null || coinRow.coinSpawnEndPoint == null)
                continue;

            if (!coinRow.coinSpawnStartPoint.isActiveAndEnabled || !coinRow.coinSpawnEndPoint.isActiveAndEnabled)
                continue;

            foreach (Vector3 generationPoint in coinArrayPrefab.GenerateRowCoinPoints(0, false, -1, GetLengthToGenerateCoinsForRows(coinRow), false, coinRow.generateCoinAtStartPoint))
            {
                Gizmos.DrawWireMesh
                (coinPrefabMesh,
                coinRow.coinSpawnStartPoint.transform.position + generationPoint + coinPosOffset + (Vector3.up * coinGenerationData.coinSpawnYOffset) - coinArrayPrefab.transform.position,
                Quaternion.Euler(new Vector3(0, ((- 25 /*coin rotation offset*/ * counter++) + ((float)EditorApplication.timeSinceStartup * 234 /*Coin rotation speed*/)) % 360, 0)),
                coinMeshSize);
            } 
        }

        if (playerData == null)
            return;

        foreach(CoinArc coinArc in coinArcs)
        {
            if (coinArc.coinSpawnStartPoint == null || coinArc.coinSpawnEndPoint == null)
                continue;

            if (!coinArc.coinSpawnStartPoint.isActiveAndEnabled || !coinArc.coinSpawnEndPoint.isActiveAndEnabled)
                continue;

            float jHeight = playerData.PlayerInformation[0].jump_height;
            if (isSpringJump)
                jHeight = playerData.PlayerInformation[0].SpringJumpHeight;
            foreach (Vector3 generationPoint in coinArrayPrefab.GenerateCurvedCoinPoints(jHeight, playerData.PlayerInformation[0].JumpDurationInitialValue, 1, false, default, Mathf.Max(0, coinArc.coinSpawnEndPoint.transform.position.z - coinArc.coinSpawnStartPoint.transform.position.z), false, coinArc.includeAscend))
            {
                Gizmos.DrawWireMesh
                (coinPrefabMesh,
                coinArc.coinSpawnStartPoint.transform.position + generationPoint + coinPosOffset + (Vector3.up * coinGenerationData.coinSpawnYOffset) - coinArrayPrefab.transform.position,
                Quaternion.Euler(new Vector3(0, ((- 25 /*coin rotation offset*/ * counter++) + ((float)EditorApplication.timeSinceStartup * 234 /*Coin rotation speed*/)) % 360, 0)),
                coinMeshSize);
            } 
        }

        SceneView.RepaintAll();
    }
#endregion

#endif

#region General Coin Generation
    public void GenerateCoins()
    {
        coinsArrayList.Clear();

        foreach (CoinRow coinRow in coinRows)
        {
            SpawnRowCoins(coinRow);
        }

        foreach(CoinArc coinArc in coinArcs)
        {
            SpawnArcCoins(coinArc);
        }

        areCoinsSpawned = true;
    }

    public void ReturnCoins()
    {
        for (int i = 0; i < coinsArrayList.Count; i++)
        {
            if (coinsArrayList[i] != null)
            {
                coinsArrayList[i].ReturnAllCoinsAndPickupsThenDestroyCoinsArray(true);
            }
        }
    }
#endregion

#region Row Coin Generation
    public float GetLengthToGenerateCoinsForRows(CoinRow coinRow)
    {
        return Mathf.Clamp(coinRow.coinSpawnEndPoint.transform.position.z - coinRow.coinSpawnStartPoint.transform.position.z, 0, Mathf.Infinity);
    }

    public void SpawnRowCoins(CoinRow coinRow)
    {
        if (coinRow.coinSpawnStartPoint == null || coinRow.coinSpawnEndPoint == null)
            return;

        if (!coinRow.coinSpawnStartPoint.gameObject.activeInHierarchy || !coinRow.coinSpawnStartPoint.enabled ||
            !coinRow.coinSpawnEndPoint.gameObject.activeInHierarchy || !coinRow.coinSpawnEndPoint.enabled)
            return;

        CoinsArray theCoinArray = Instantiate(coinArrayPrefab, coinRow.coinSpawnStartPoint.transform.position + (Vector3.up * coinGenerationData.coinSpawnYOffset), coinArrayPrefab.transform.rotation);
        theCoinArray.transform.SetParent(this.transform);
        theCoinArray.ShoudNotOffsetOnRest = true;

        List<Vector3> coinSpawnPoints = theCoinArray.GenerateRowCoinPoints(0, false, -1, GetLengthToGenerateCoinsForRows(coinRow), false, coinRow.generateCoinAtStartPoint);
        theCoinArray.SpawnRowCoins(coinSpawnPoints); // Doesn't support double coin spawning yet
        theCoinArray.IsInitialized = true;

        coinsArrayList.Add(theCoinArray);
    }
#endregion

#region Arc Coin Generation
    public void SpawnArcCoins(CoinArc coinArc)
    {
        if (coinArc.coinSpawnStartPoint == null || coinArc.coinSpawnEndPoint == null)
            return;

        if (!coinArc.coinSpawnStartPoint.gameObject.activeInHierarchy || !coinArc.coinSpawnStartPoint.enabled ||
            !coinArc.coinSpawnEndPoint.gameObject.activeInHierarchy || !coinArc.coinSpawnEndPoint.enabled)
            return;

        CoinsArray theCoinArray = Instantiate(coinArrayPrefab, coinArc.coinSpawnStartPoint.transform.position + (Vector3.up * coinGenerationData.coinSpawnYOffset), coinArrayPrefab.transform.rotation);
        theCoinArray.transform.SetParent(this.transform);
        theCoinArray.ShoudNotOffsetOnRest = true;

        float jHeight = isSpringJump ? playerData.PlayerInformation[0].SpringJumpHeight : playerData.PlayerInformation[0].jump_height;
        List<Vector3> coinSpawnPoints = theCoinArray.GenerateCurvedCoinPoints(jHeight, playerSharedData.JumpDuration, 1, false, default, Mathf.Max(0, coinArc.coinSpawnEndPoint.transform.position.z - coinArc.coinSpawnStartPoint.transform.position.z), false, coinArc.includeAscend);
        theCoinArray.SpawnCurvedCoins(coinSpawnPoints); // Doesn't support double coin spawning yet
        theCoinArray.IsInitialized = true;

        coinsArrayList.Add(theCoinArray);
    }
    #endregion
}
