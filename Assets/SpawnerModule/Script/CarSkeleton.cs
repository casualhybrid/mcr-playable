using UnityEngine;

public class CarSkeleton : MonoBehaviour
{
   
    [SerializeField] private Animator theAnimator;

    [SerializeField] private Transform bodyTransform;
    [SerializeField] private GameObject wingsTransform;
    [SerializeField] private Transform midPointAnimatedT;

    [SerializeField] private Transform diamondTargetPoint;

    public Transform leftSteeringTarget;
    public Transform rightSteeringTarget;
    public Transform leftSteeringHint;
    public Transform rightSteeringHint;

    public RequestTogglePlayerMeshChannel RequestTogglePlayerMeshChannel;


    public GameObject[] GameObjectsToDisableDuringBlinking;

    public Renderer[] Meshes { get; private set; }
    public SkinnedMeshRenderer[] SkinneddMeshRenderers;

    public Transform GetBodyTransform => bodyTransform;

    public Animator TheAnimator => theAnimator;

    public GameObject WingsTransform => wingsTransform;

    public Transform MidPointAnimatedT => midPointAnimatedT;

    public Transform DiamondTargetPoint => diamondTargetPoint;

    private void Awake()
    {
        var meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        SkinneddMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);

        int meshesCount = meshRenderers.Length + SkinneddMeshRenderers.Length;

        Meshes = new Renderer[meshesCount];

        int k = 0;

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            Meshes[k++] = meshRenderers[i];
        }

        for (int i = 0; i < SkinneddMeshRenderers.Length; i++)
        {
            Meshes[k++] = SkinneddMeshRenderers[i];
        }

        RequestTogglePlayerMeshChannel.OnRequestToDisableSkinnedMeshes.AddListener(DisableAllSkinnedMeshes);
        RequestTogglePlayerMeshChannel.OnRequestToEnableSkinnedMeshes.AddListener(EnableAllSkinnedMeshes);

    }
   

    private void OnDestroy()
    {
        RequestTogglePlayerMeshChannel.OnRequestToDisableSkinnedMeshes.RemoveListener(DisableAllSkinnedMeshes);
        RequestTogglePlayerMeshChannel.OnRequestToEnableSkinnedMeshes.RemoveListener(EnableAllSkinnedMeshes);
    }

    public void DisableAllSkinnedMeshes()
    {
        for (int i = 0; i < SkinneddMeshRenderers.Length; i++)
        {
            SkinneddMeshRenderers[i].enabled = false;
        }
    }

    public void EnableAllSkinnedMeshes()
    {
        for (int i = 0; i < SkinneddMeshRenderers.Length; i++)
        {
            SkinneddMeshRenderers[i].enabled = true;
        }
    }

    public void DestroyTheObject(GameObject obj)
    {
        UnityEngine.Object.Destroy(obj,5);
    }
}