using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheKnights.SceneLoadingSystem
{
    public class LoadTheSceneMonoBehaviour : MonoBehaviour, ISceneLoadCallBacks
    {
        [SerializeField] private UnityEvent LoadingAppeared;
        [SerializeField] private UnityEvent enteredMainMenu;

        [Tooltip("The canvas that handles loading image including fill bar")]
        [SerializeField] private Canvas loadingCanvas;

        [Tooltip("The main scene loading manager")]
        [SerializeField] private SceneLoader sceneLoader;

        // [Tooltip("The fill bar that would be filled during scene loading")]
        // [SerializeField] private Image loadingFillBar;

        //[Tooltip("The slider that would be filled during scene loading")]
        //[SerializeField] private Slider loadingSlider;

        //[Tooltip("The slider would follow the filling bar during scene loading")]
        //[SerializeField] private Slider loadingSlider;

        [Tooltip("The Text that would represent loading fill amount")]
        [SerializeField] private TextMeshProUGUI loadingFillText;

        [SerializeField] private float loadingBarFillSpeed;

        [SerializeField] private GameEvent loadingHasStarted;
        [SerializeField] private GameEvent loadingHasAppeared;
        [SerializeField] private GameEvent dependenciesHasBeenLoaded;

        // [SerializeField] private Animator tyreAnimatorAnimator;
        [SerializeField] private Image imageToFill;

        private bool isLoading = false;

        /// <summary>
        /// The canvas that handles loading image including fill bar
        /// </summary>
        public Canvas GetTheLoadingCanvas => loadingCanvas;

        private static LoadTheSceneMonoBehaviour instance;

        private float currentLoadingValue;
        private float totalRealProgress;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this.gameObject);

            if (loadingCanvas == null)
            {
                loadingCanvas = GetComponentInChildren<Canvas>();
            }

            // loadingHasStarted.TheEvent.AddListener(HandleLoadingStarted);
            loadingHasAppeared.TheEvent.AddListener(HandleLoadingAppeared);
            dependenciesHasBeenLoaded.TheEvent.AddListener(SendEnteredMainMenuEvent);
        }

        private void OnEnable()
        {
            totalRealProgress = 0;
            currentLoadingValue = 0;
        }

        public void SceneOperationCompleted()
        {
            // tyreAnimatorAnimator.enabled = false;

            isLoading = false;
        }

        public void SceneOperationFailed()
        {
        }

        public void OnLoadingProgressChanged(float progress)
        {
            //  if (loadingSlider != null)
            // {
            float _progress;

            if (PluginsInitializer.IsPluginInitCompleted)
            {
                progress = ((1 - PluginsInitializer.MaxPluginInitLoadingVal) * progress) + PluginsInitializer.MaxPluginInitLoadingVal;
            }

            _progress = progress <= totalRealProgress ? totalRealProgress : progress;
            totalRealProgress = _progress;

            float temp = _progress * 100f;

            // Smoothly animate the bar fill using DOTween over 4 seconds
            float targetFillAmount = temp / 100f;

            // Stop any existing tween on this image to prevent overlapping tweens
            imageToFill.DOKill();

            // Start new tween
            imageToFill.DOFillAmount(targetFillAmount, 2f).SetEase(Ease.Linear);

            // Animate the % text as well
            DOTween.To(() => currentLoadingValue, x => {
                currentLoadingValue = x;
                loadingFillText.text = currentLoadingValue.ToString("n0") + "%";
            }, temp, 2f).SetEase(Ease.Linear);
        }

        private void HandleLoadingStarted(/*GameEvent gameEvent*/)
        {
            if (isLoading)
                return;

            isLoading = true;

            //  tyreAnimatorAnimator.enabled = true;
            // loadingSlider.value = 0;
            imageToFill.fillAmount = 0;
            totalRealProgress = 0;
            currentLoadingValue = 0;
        }

        private void HandleLoadingAppeared(GameEvent gameEvent)
        {
            LoadingAppeared.Invoke();
        }

        private void SendEnteredMainMenuEvent(GameEvent gameEvent)
        {
            enteredMainMenu.Invoke();
            ChaserActiveAnimation.Instance.ChaserActive();
            Debug.LogError("MainMenu");
        }

        public void SceneLoadingStarted()
        {
            HandleLoadingStarted();
        }
    }
}