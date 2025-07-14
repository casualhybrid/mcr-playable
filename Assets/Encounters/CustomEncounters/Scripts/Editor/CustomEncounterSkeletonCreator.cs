using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class CustomEncounterSkeletonCreator : EditorWindow
{
    [SerializeField] private List<CustomEncounter> customEncounters = new List<CustomEncounter>();

    private SerializedProperty serializedPropertyCustomEncounters;
    private SerializedObject serializedObject;

    [MenuItem("Tools/CustomEncounterSkeletonCreator")]
    private static void Init()
    {
        CustomEncounterSkeletonCreator window = (CustomEncounterSkeletonCreator)EditorWindow.GetWindow(typeof(CustomEncounterSkeletonCreator));
        window.Show();
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical(GUILayout.MinHeight(10), GUILayout.MaxHeight(50), GUILayout.MinWidth(3), GUILayout.MaxWidth(Screen.width));
        EditorGUILayout.LabelField("Skeleton Creator", new GUIStyle() { fontStyle = FontStyle.Bold, fontSize = 33, alignment = TextAnchor.MiddleCenter, stretchWidth = true }, GUILayout.MinHeight(10), GUILayout.MaxHeight(50)
            , GUILayout.MinWidth(3), GUILayout.MaxWidth(Screen.width)); ;
        GUILayout.EndVertical();

        serializedPropertyCustomEncounters = serializedObject.FindProperty("customEncounters");

        EditorGUILayout.PropertyField(serializedPropertyCustomEncounters);

        var originalColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Create Skeleton"))
        {
            CreateSkeleton();
        }
        GUI.backgroundColor = originalColor;

        serializedObject.ApplyModifiedProperties();
    }

    private void CreateSkeleton()
    {
        for (int i = 0; i < customEncounters.Count; i++)
        {
            Debug.Log($"Creating skeleton for {customEncounters[i].name}");

            CustomEncounter customEncounter = customEncounters[i];

            string pathToPrefab = AssetDatabase.GetAssetPath(customEncounter.gameObject.GetInstanceID());

            GameObject loadedEncounter = PrefabUtility.LoadPrefabContents(pathToPrefab);

            CustomEncounterSkeleton customEncounterSkeleton = loadedEncounter.AddComponent<CustomEncounterSkeleton>();

            var childObstacles = loadedEncounter.GetComponentsInChildren<Obstacle>();

            CustomEncounterObstacleMetaData[] customEncounterObstacleMetaDatas = new CustomEncounterObstacleMetaData[childObstacles.Length];

            SerializedObject serializedSkeletonObject = new SerializedObject(customEncounterSkeleton);
            SerializedProperty serializedPropertyMetaData = serializedSkeletonObject.FindProperty("customEncounterObstacleMetaDatas");

            for (int k = 0; k < childObstacles.Length; k++)
            {
                Obstacle obstacleEntity = childObstacles[k];

                CustomEncounterObstacleMetaData metaDataEntry = new CustomEncounterObstacleMetaData();
                metaDataEntry.localPosition = obstacleEntity.transform.localPosition;
                metaDataEntry.localRotationEulers = obstacleEntity.transform.localRotation.eulerAngles;
                metaDataEntry.nameOfTheObstacle = obstacleEntity.GetComponent<ResetDependencyInjector>().OriginalGameObjectName;
                metaDataEntry.ParentTransform = obstacleEntity.transform.parent;
                metaDataEntry.ChildOrder = obstacleEntity.transform.GetSiblingIndex();

                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obstacleEntity.gameObject);
                GameObject originalObstacle = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
                metaDataEntry.OriginalPrefab = originalObstacle;

                customEncounterObstacleMetaDatas[k] = metaDataEntry;
            }

            serializedPropertyMetaData.ClearArray();
            serializedSkeletonObject.ApplyModifiedProperties();

            customEncounterSkeleton.GetType().GetField("customEncounterObstacleMetaDatas", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(customEncounterSkeleton, customEncounterObstacleMetaDatas);

            // Object destruction meta data

            SerializedProperty serializedPropertyObjectDestructionMetaData = serializedSkeletonObject.FindProperty("customEncounterObjectDestructionMetaData");
            var childObjectDestruction = loadedEncounter.GetComponentsInChildren<ObjectDestruction>();

            CustomEncounterObjectDestructionMetaData[] customEncounterObjectDestructionMetaData = new CustomEncounterObjectDestructionMetaData[childObjectDestruction.Length];

            for (int q = 0; q < childObjectDestruction.Length; q++)
            {
                var objectDestructionEntity = childObjectDestruction[q];

                customEncounterObjectDestructionMetaData[q] = new CustomEncounterObjectDestructionMetaData();
                int theInstanceID = objectDestructionEntity.transform.parent.GetInstanceID();
                customEncounterObjectDestructionMetaData[q].ParentCachedInstanceID = theInstanceID;
                customEncounterObjectDestructionMetaData[q].OriginalWordPosition = objectDestructionEntity.transform.parent.position;
            }

            serializedPropertyObjectDestructionMetaData.ClearArray();
            serializedSkeletonObject.ApplyModifiedProperties();

            customEncounterSkeleton.GetType().GetField("customEncounterObjectDestructionMetaData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(customEncounterSkeleton, customEncounterObjectDestructionMetaData);

            // Synced Obstacles meta data

            SerializedProperty serializedPropertySyncedObstaclesMetaData = serializedSkeletonObject.FindProperty("customEncounterSyncedObstacleMetaData");
            var childSyncedObstacles = loadedEncounter.GetComponentsInChildren<SyncedObstacle>();

            CustomEncounterSyncedObstacleMetaData[] customEncounterSyncedObstaclesMetaData = new CustomEncounterSyncedObstacleMetaData[childSyncedObstacles.Length];

            for (int q = 0; q < childSyncedObstacles.Length; q++)
            {
                var syncedObstacleEntity = childSyncedObstacles[q];
                int theInstanceID = syncedObstacleEntity.transform.GetInstanceID();
                customEncounterSyncedObstaclesMetaData[q] = new CustomEncounterSyncedObstacleMetaData();
                customEncounterSyncedObstaclesMetaData[q].ParentCachedInstanceID = theInstanceID;
                customEncounterSyncedObstaclesMetaData[q].OriginalWordPosition = syncedObstacleEntity.transform.position;
            }

            serializedPropertySyncedObstaclesMetaData.ClearArray();
            serializedSkeletonObject.ApplyModifiedProperties();

            customEncounterSkeleton.GetType().GetField("customEncounterSyncedObstacleMetaData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(customEncounterSkeleton, customEncounterSyncedObstaclesMetaData);

            // Action Time preserver data

            ActionTimePreserver[] actionTimePreservers = loadedEncounter.GetComponentsInChildren<ActionTimePreserver>();

            Dictionary<ActionTimePreserverSubject, ActionTimeSubjectMetaData> actionTimeSubjectsChildObjectDestructions = new Dictionary<ActionTimePreserverSubject, ActionTimeSubjectMetaData>();
            Dictionary<ActionTimePreserver, CustomEncounterSyncedObstacleMetaData[]> actionTimePreserversChildSyncedObstacles = new Dictionary<ActionTimePreserver, CustomEncounterSyncedObstacleMetaData[]>();

            foreach (ActionTimePreserver actionTimePreserver in actionTimePreservers)
            {
                foreach (ActionTimePreserverSubject actionTimePreserverSubject in actionTimePreserver.PreservedSubjects)
                {
                    ObjectDestruction[] preservedObjectDestructionComponents = actionTimePreserverSubject.GetComponentsInChildren<ObjectDestruction>();
                    SyncedObstacle[] preserverdSyncedObstacleComponents = actionTimePreserver.GetComponentsInChildren<SyncedObstacle>();

                    List<CustomEncounterObjectDestructionMetaData> metaDataTemp = new List<CustomEncounterObjectDestructionMetaData>();

                    foreach (ObjectDestruction preservedObjectDestructionComponent in preservedObjectDestructionComponents)
                    {
                        int parentInstancedID = preservedObjectDestructionComponent.transform.parent.GetInstanceID();
                        Vector3 parentWorldPos = preservedObjectDestructionComponent.transform.parent.position;
                        metaDataTemp.Add(new CustomEncounterObjectDestructionMetaData() { OriginalWordPosition = parentWorldPos, ParentCachedInstanceID = parentInstancedID });
                    }

                    if (!actionTimeSubjectsChildObjectDestructions.ContainsKey(actionTimePreserverSubject))
                    {
                        ActionTimeSubjectMetaData actionTimeSubjectMetaData = new ActionTimeSubjectMetaData();
                        actionTimeSubjectMetaData.actionTimeSubjectsChildObjectDestructions = metaDataTemp.ToArray();

                        var firstObstacleInGroup = actionTimePreserverSubject.firstObjectInGroupTransform.GetComponent<Obstacle>();
                        var lastObstacleInGroup = actionTimePreserverSubject.lastObjectInGroupTransform.GetComponent<Obstacle>();

                        //int firstObjectInGroupInstanceID = firstObstacleInGroup != null ? firstObstacleInGroup.InstanceID : -1;

                        //int lastObjectInGroupInstanceID = lastObstacleInGroup != null ? lastObstacleInGroup.InstanceID : -1;

                        var firstObjectInGroupMeta = firstObstacleInGroup != null ? new ParentAndChildOrderMeta() { ChildOrder = firstObstacleInGroup.transform.GetSiblingIndex(), ParentT = firstObstacleInGroup.transform.parent } : null;
                        var lastObjectInGroupMeta = lastObstacleInGroup != null ? new ParentAndChildOrderMeta() { ChildOrder = lastObstacleInGroup.transform.GetSiblingIndex(), ParentT = lastObstacleInGroup.transform.parent } : null;

                        //actionTimeSubjectMetaData.actionTimeSubjectFirstLastGroupMetaData =
                        //    new ActionTimeSubjectFirstLastGroupMetaData() { firstObjectGroupInstancedID = firstObjectInGroupInstanceID, lastObjectGroupInstancedID = lastObjectInGroupInstanceID };

                        actionTimeSubjectMetaData.actionTimeSubjectFirstLastGroupMetaData =
                            new ActionTimeSubjectFirstLastGroupMetaData() { firstObjectGroupMeta = firstObjectInGroupMeta, lastObjectGroupMeta = lastObjectInGroupMeta };

                        actionTimeSubjectsChildObjectDestructions.Add(actionTimePreserverSubject, actionTimeSubjectMetaData);
                    }

                    // ..

                    List<CustomEncounterSyncedObstacleMetaData> metaDataTemp1 = new List<CustomEncounterSyncedObstacleMetaData>();

                    foreach (SyncedObstacle preserverdSyncedObstacleComponent in preserverdSyncedObstacleComponents)
                    {
                        int parentInstancedID = preserverdSyncedObstacleComponent.transform.GetInstanceID();
                        Vector3 parentWorldPos = preserverdSyncedObstacleComponent.transform.position;
                        metaDataTemp1.Add(new CustomEncounterSyncedObstacleMetaData() { OriginalWordPosition = parentWorldPos, ParentCachedInstanceID = parentInstancedID });
                    }

                    if (!actionTimePreserversChildSyncedObstacles.ContainsKey(actionTimePreserver))
                        actionTimePreserversChildSyncedObstacles.Add(actionTimePreserver, metaDataTemp1.ToArray());
                }
            }

            customEncounterSkeleton.GetType().GetField("actionTimeSubjectsChildObjectDestructions", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(customEncounterSkeleton, actionTimeSubjectsChildObjectDestructions);
            customEncounterSkeleton.GetType().GetField("actionTimePreserversChildSyncedObstacles", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(customEncounterSkeleton, actionTimePreserversChildSyncedObstacles);

            // action preserver subjects data

            var actionPreserverSubjects = loadedEncounter.GetComponentsInChildren<ActionTimePreserverSubject>();

            foreach (var actionSubject in actionPreserverSubjects)
            {
                Obstacle firstObjectObstacle = actionSubject.firstObjectInGroupTransform.GetComponent<Obstacle>();
                Obstacle secondObjectObstacle = actionSubject.lastObjectInGroupTransform.GetComponent<Obstacle>();

                GameObject tempFirst = null;

                if (firstObjectObstacle != null)
                {
                    tempFirst = new GameObject("Temp_FirstSubjectChild");
                    tempFirst.transform.SetParent(actionSubject.firstObjectInGroupTransform.parent);

                    tempFirst.transform.localPosition = actionSubject.firstObjectInGroupTransform.localPosition;
                    tempFirst.transform.localRotation = actionSubject.firstObjectInGroupTransform.localRotation;
                     
                    actionSubject.firstObjectInGroupTransform = tempFirst.transform;
                }

                if (secondObjectObstacle != null && secondObjectObstacle != firstObjectObstacle)
                {
                    GameObject tempLast = new GameObject("Temp_LastSubjectChild");
                    tempLast.transform.SetParent(actionSubject.lastObjectInGroupTransform.parent);

                    tempLast.transform.localPosition = actionSubject.lastObjectInGroupTransform.localPosition;
                    tempLast.transform.localRotation = actionSubject.lastObjectInGroupTransform.localRotation;

                    actionSubject.lastObjectInGroupTransform = tempLast.transform;
                }
                else if (secondObjectObstacle != null)
                {
                    actionSubject.lastObjectInGroupTransform = tempFirst != null ? tempFirst.transform : null;
                }
            }

            // unique lane shuffle data

            var uniqueShuffleLanes = loadedEncounter.GetComponentsInChildren<ShuffleObstaclePairsToProvideUniqueEmptyLanes>();
            Dictionary<ShuffleObstaclePairsToProvideUniqueEmptyLanes, ShuffleObstaclePairUniqueEmptyLaneMetaData> shuffleObstaclePairData = new Dictionary<ShuffleObstaclePairsToProvideUniqueEmptyLanes, ShuffleObstaclePairUniqueEmptyLaneMetaData>();

            foreach (var item in uniqueShuffleLanes)
            {
                ShuffleObstaclePairUniqueEmptyLaneMetaData data = new ShuffleObstaclePairUniqueEmptyLaneMetaData();
                Dictionary<Transform, List<ParentAndChildOrderMeta>> folderToParentChildData = new Dictionary<Transform, List<ParentAndChildOrderMeta>>();

                foreach (var obstaclePair in item.GetObstaclePairs)
                {
                    List<ParentAndChildOrderMeta> parentAndChildOrderMetas = new List<ParentAndChildOrderMeta>();

                    Transform parentFolderT = obstaclePair.pairFolderTransform;

                    foreach (var child in obstaclePair.pairObjectTransforms)
                    {
                        int childOrder = child.GetSiblingIndex();
                        parentAndChildOrderMetas.Add(new ParentAndChildOrderMeta() { ChildOrder = childOrder, ParentT = parentFolderT });
                    }

                    folderToParentChildData.Add(parentFolderT, parentAndChildOrderMetas);
                }

                data.shuffleObstaclePairData = folderToParentChildData;

                shuffleObstaclePairData.Add(item, data);
            }

            customEncounterSkeleton.GetType().GetField("shuffleObstaclePairData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(customEncounterSkeleton, shuffleObstaclePairData);


            for (int k = 0; k < childObstacles.Length; k++)
            {
                Obstacle obstacleEntity = childObstacles[k];
                DestroyImmediate(obstacleEntity.gameObject);
            }

            CustomEncounter loadedCustomEncounter = loadedEncounter.GetComponent<CustomEncounter>();
            loadedCustomEncounter.IsSkeleton = true;
            loadedCustomEncounter.CustomEncounterSkeleton = customEncounterSkeleton;

            string pathToSkeletons = "Assets/Encounters/CustomEncounters/EncounterPrefabs/Skeletons";
            string pathToSpecificFolder = "Assets/Encounters/CustomEncounters/EncounterPrefabs/Skeletons/Shipit";

            if (!AssetDatabase.IsValidFolder(pathToSkeletons))
            {
                AssetDatabase.CreateFolder("Assets/Encounters/CustomEncounters/EncounterPrefabs", "Skeletons");
            }

            if (!AssetDatabase.IsValidFolder(pathToSpecificFolder))
            {
                AssetDatabase.CreateFolder("Assets/Encounters/CustomEncounters/EncounterPrefabs/Skeletons", "Shipit");
            }


            foreach (var item in loadedCustomEncounter.GetComponentsInChildren<ICustomEncounterEntity>())
            {
                item.CustomEncounter = loadedCustomEncounter;
            }

            PrefabUtility.SaveAsPrefabAsset(loadedEncounter, $"Assets/Encounters/CustomEncounters/EncounterPrefabs/Skeletons/Shipit/{customEncounter.name}_Skeleton.prefab");

            PrefabUtility.UnloadPrefabContents(loadedEncounter);

            Debug.Log($"Created skeleton for {customEncounters[i].name}");
        }
    }
}