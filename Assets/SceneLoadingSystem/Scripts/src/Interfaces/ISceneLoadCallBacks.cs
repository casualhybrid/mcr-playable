namespace TheKnights.SceneLoadingSystem
{
    public interface ISceneLoadCallBacks
    {
        void SceneLoadingStarted();
        void SceneOperationCompleted();

        void SceneOperationFailed();

        void OnLoadingProgressChanged(float progress);
    }
}