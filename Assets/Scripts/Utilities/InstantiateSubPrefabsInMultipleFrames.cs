using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class InstantiateSubPrefabsInMultipleFrames : MonoBehaviour
{
    public event Action OnInstantiationComplete;

    [System.Serializable]
    public struct InstantiationCombo
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform parentT;

        public Transform ParentT => parentT;
        public GameObject Prefab => prefab;
    }

    [System.Serializable]
    public struct InstantiationInfo
    {
        public InstantiationCombo[] instantiationCombos;
    }

    [SerializeField] private InstantiationInfo[] instantiationInfos;

#if UNITY_EDITOR
    [Button("Spawn")]
    public void SpawnInEditor()
    {
        for (int i = 0; i < instantiationInfos.Length; i++)
        {
            InstantiationInfo info = instantiationInfos[i];

            for (int k = 0; k < info.instantiationCombos.Length; k++)
            {
                InstantiationCombo instantiationCombo = info.instantiationCombos[k];

                PrefabUtility.InstantiatePrefab(instantiationCombo.Prefab, instantiationCombo.ParentT);
            }
        }
    }

    [Button("DeleteSpawned")]
    public void DeSpawnInEditor()
    {
        for (int i = 0; i < instantiationInfos.Length; i++)
        {
            InstantiationInfo info = instantiationInfos[i];

            for (int k = 0; k < info.instantiationCombos.Length; k++)
            {
                InstantiationCombo instantiationCombo = info.instantiationCombos[k];

                foreach (Transform t in instantiationCombo.ParentT)
                {
                    if (PrefabUtility.IsPartOfPrefabInstance(t.gameObject) && t.gameObject.name == instantiationCombo.Prefab.name)
                    {
                        DestroyImmediate(t.gameObject);
                    }
                }

            }
        }

        EditorUtility.SetDirty(this);
    }


#endif

    public Coroutine SpawnSubPrefabs()
    {
        return StartCoroutine(SpawnSubPrefabsRoutine());
    }

    private IEnumerator SpawnSubPrefabsRoutine()
    {

        for (int i = 0; i < instantiationInfos.Length; i++)
        {
            yield return new WaitForSecondsRealtime(0.08f);

            InstantiationInfo info = instantiationInfos[i];

            for (int k = 0; k < info.instantiationCombos.Length; k++)
            {
                InstantiationCombo instantiationCombo = info.instantiationCombos[k];

                Instantiate(instantiationCombo.Prefab, instantiationCombo.ParentT);
            }
        }

        OnInstantiationComplete?.Invoke();
    }
}