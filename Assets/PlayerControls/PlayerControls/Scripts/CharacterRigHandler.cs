using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Rig))]
public class CharacterRigHandler : MonoBehaviour
{
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private Animator_controller animator_Controller;

    [SerializeField] private TwoBoneIKConstraint leftTwoBoneIKConstraint;
    [SerializeField] private TwoBoneIKConstraint rightTwoBoneIKConstraint;

    [SerializeField] private GameEvent onSkeletonSpawned;

    private Rig rig;

    private void Awake()
    {
        rig = GetComponent<Rig>();

        animator_Controller.OnIKSteerWeightChange += ChangeIKRigWeight;
        onSkeletonSpawned.TheEvent.AddListener(SetIKTargets);
    }

    private void OnDestroy()
    {
        animator_Controller.OnIKSteerWeightChange -= ChangeIKRigWeight;
        onSkeletonSpawned.TheEvent.RemoveListener(SetIKTargets);
    }

    private void Start()
    {
        SetIKTargets(null);
    }

    private void SetIKTargets(GameEvent gameEvent)
    {
        CarSkeleton carSkeleton = playerSharedData.CarSkeleton;

        leftTwoBoneIKConstraint.data.target = carSkeleton.leftSteeringTarget;
        leftTwoBoneIKConstraint.data.hint = carSkeleton.leftSteeringHint;

        rightTwoBoneIKConstraint.data.target = carSkeleton.rightSteeringTarget;
        rightTwoBoneIKConstraint.data.hint = carSkeleton.rightSteeringHint;

     //   GetComponentInParent<RigBuilder>().Build();

    }

    private void ChangeIKRigWeight(int weight)
    {
        rig.weight = weight;
    }
}