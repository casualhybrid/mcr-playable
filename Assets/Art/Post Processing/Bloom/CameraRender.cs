using UnityEngine;

public class CameraRender : MonoBehaviour
{
    public int[] layer;
    public int[] Distance;

    // Start is called before the first frame update
    private void Start()
    {
        Camera camera = GetComponent<Camera>();
        float[] distances = new float[32];

        for(int i = 0; i<layer.Length;i++)
        {
            distances[layer[i]] = Distance[i];
        }

      
        camera.layerCullDistances = distances;
    }
}