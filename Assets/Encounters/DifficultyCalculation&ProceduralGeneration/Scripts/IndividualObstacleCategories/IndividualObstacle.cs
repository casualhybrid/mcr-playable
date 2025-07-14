using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IndividualObstacle
{
    [System.Serializable]
    public class ObstacleProperties
    {
        [Header("Approaches")]
        public bool isJumpable;
        public bool isDuckable;
        public bool isDashable;
        public bool isShockwaveable;

        [Header("Other Properties")]
        public bool isMovingObstacle;
    }

    public string obstacleName;
    public GameObject obstaclePrefab;
    public ObstacleProperties obstacleProperties;
}
