using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AirplaneWindTrail : MonoBehaviour
{
    #region Variables
    [Header("Properties")]
    [SerializeField] private Transform _trailCreationPoint;
    [Tooltip("Can't modify segment size at runtime")]
    [SerializeField] private int _maxSegments = 30;
    [SerializeField] private float _segmentCreationFrequency = 120;
    [SerializeField] private Vector3 _worldSpaceMovementSpeed = new Vector3(0, 0, -1.6f);

    [Header("Property Trackers")]
    private Vector3[] _currentVertices;
    private float _intervalTracker;

    [Header("Components")]
    private LineRenderer _selfLR;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        GetRequiredComponents();
        InitializeVertices();
    }

    private void Update()
    {
        _intervalTracker += Time.deltaTime;

        for (int i = 0; i < _maxSegments; i++)
        {
            _currentVertices[i] += _worldSpaceMovementSpeed * Time.deltaTime;
        }

        while (_intervalTracker > 1 / _segmentCreationFrequency)
        {
            _intervalTracker -= (1 / _segmentCreationFrequency);

            for (int i = _maxSegments - 1; i > 0; i--)
            {
                _currentVertices[i] = _currentVertices[i - 1]; 
            }

            _currentVertices[0] = _trailCreationPoint.position - transform.position;
        }

        _selfLR.SetPositions(_currentVertices);
    }
    #endregion

    #region Initialization
    private void GetRequiredComponents()
    {
        _selfLR = GetComponent<LineRenderer>();
    }

    private void InitializeVertices()
    {
        _selfLR.positionCount = _maxSegments;
        _currentVertices = new Vector3[_maxSegments];

        for (int i = 0; i < _maxSegments; i++)
        {
            _currentVertices[i] = (_trailCreationPoint.position - transform.position) + ((_worldSpaceMovementSpeed * Time.fixedDeltaTime) * i);
        }
    }
    #endregion
}
