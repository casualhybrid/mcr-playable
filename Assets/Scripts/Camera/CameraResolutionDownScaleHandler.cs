using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraResolutionDownScaleHandler : MonoBehaviour
{
    [SerializeField] private CameraResolutionDownScaleSO cameraResolutionDownScale;

    private Camera cam;
    private RenderTexture myRenderTexture;

    private void OnEnable()
    {
        if (cam == null)
            cam = GetComponent<Camera>();
    }

    //private void OnPreRender()
    //{
    //    myRenderTexture = RenderTexture.GetTemporary((int)(Screen.width * /*cameraResolutionDownScale.RenderScaleValue*/ 1), (int)(Screen.height * /*cameraResolutionDownScale.RenderScaleValue*/ 1), 24);
    //    cam.targetTexture = myRenderTexture;
    //}

    //private void OnPostRender()
    //{
    //    cam.targetTexture = null;

    //    if (cameraResolutionDownScale.DisablePostProcessing)
    //    {
    //        Graphics.Blit(myRenderTexture, null as RenderTexture);
    //    }

    //    RenderTexture.ReleaseTemporary(myRenderTexture);
    //}
}
