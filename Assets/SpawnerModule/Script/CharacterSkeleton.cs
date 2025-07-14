using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkeleton : MonoBehaviour
{
    public Renderer[] Meshes { get; private set; }
    public SkinnedMeshRenderer[] SkinneddMeshRenderers { get; private set; }
    public Transform rightHandBoneT;
    public GameObject tutorialPhone;
    public RequestTogglePlayerMeshChannel RequestTogglePlayerMeshChannel;

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




}
