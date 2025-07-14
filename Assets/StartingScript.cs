using TheKnights.SceneLoadingSystem;
using UnityEngine;

public class StartingScript : MonoBehaviour
{
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private TheScene initializationScene;

    private void Awake()
    {
        sceneLoader.LoadTheScene(new TheScene[] { initializationScene }, null, true, true);
    }

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //        sceneLoader.LoadTheScene(new TheScene[] { initializationScene }, null, true, true);

    //}

    private void Start()
    {
        if (!PlayerPrefs.HasKey("haptic"))
            PlayerPrefs.SetFloat("haptic", 1);
        if (!PlayerPrefs.HasKey("music"))
            PlayerPrefs.SetFloat("music", 1);
        if (!PlayerPrefs.HasKey("sound"))
            PlayerPrefs.SetFloat("sound", 1);
    }
}