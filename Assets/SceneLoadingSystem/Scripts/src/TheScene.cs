using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheKnights.SceneLoadingSystem
{
    /// <summary>
    /// Contains all the information required to load a certain scene.
    /// </summary>
    [CreateAssetMenu(fileName = "TheScene", menuName = "ScriptableObjects/SceneLoading/TheScene", order = 1)]
    public class TheScene : ScriptableObject
    {
        [Tooltip("Would the scene be additive or single")]
        [SerializeField] protected LoadSceneMode LoadSceneMode;
        [SerializeField] protected string SceneKey;
        [Tooltip("The sprite to be shown during loading of this scene")]
        [SerializeField] protected Sprite LoadingSprite;
        [Tooltip("Should this scene progress be shown in progress bar")]
        [SerializeField] protected bool includeLoadingProgress = true;

        /// <summary>
        /// The key for this particular scene as specified in addressables or the scene name if using unity scene loading
        /// </summary>
        public string GetSceneKey { get { return SceneKey; } protected set { } }

        /// <summary>
        /// The sprite to show while loading this scene if relevant
        /// </summary>
        public Sprite GetLoadingSprite { get { return LoadingSprite; } protected set { } }

        /// <summary>
        /// The mode in which this scene should be loaded
        /// </summary>
        public LoadSceneMode GetSceneLoadMode { get { return LoadSceneMode; } protected set { } }

        /// <summary>
        /// Should the loading progress be shown in progress bar
        /// </summary>
        public bool IncludeLoadingProgress { get { return includeLoadingProgress; } protected set { } }
    }
}