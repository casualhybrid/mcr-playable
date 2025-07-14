using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DiscardRenderTextureOnDisable : MonoBehaviour
{
    [SerializeField] private GameEvent cutSceneStarted;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cutSceneStarted.TheEvent.AddListener(MakeRenderTextureNull);
    }

    private void OnDestroy()
    {
        cutSceneStarted.TheEvent.RemoveListener(MakeRenderTextureNull);

    }

    private void OnDisable()
    {
        if(cam.targetTexture != null)
        {
            cam.targetTexture.Release();
        }
    }

    public void MakeRenderTextureNull(GameEvent gameEvent)
    {
        if (cam.targetTexture != null)
        {
            cam.targetTexture.Release();
            cam.targetTexture = null;
        }
    }
}
