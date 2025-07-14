using deVoid.UIFramework;
using UnityEngine.Events;

public enum ScreenType
{
    Window, PopUp, Panel
}

public class ScreenEvent : UnityEvent<string, ScreenOperation, ScreenType> { }

public class WindowEvent : UnityEvent<string, ScreenOperation, IWindowProperties> { }
public class PanelEvent : UnityEvent<string, ScreenOperation, IPanelProperties> { }

public enum ScreenOperation
{
    Open, Close
}

public class UIScreenEvents
{
    public static readonly ScreenEvent OnScreenOperationEventAfterAnimation = new ScreenEvent();
    public static readonly ScreenEvent OnScreenOperationEventBeforeAnimation = new ScreenEvent();

    public PanelEvent PanelEvent = new PanelEvent();

    public WindowEvent WindowEvent = new WindowEvent();

    public void RaisePanelOpenEvent(string id, IPanelProperties panelProperties = null)
    {
        PanelEvent.Invoke(id, ScreenOperation.Open, panelProperties);
    }

    public void RaiseWindowOpenEvent(string id, IWindowProperties windowProperties = null)
    {
        WindowEvent.Invoke(id, ScreenOperation.Open, windowProperties);
    }

    public void RaisePanelCloseEvent(string id)
    {
        PanelEvent.Invoke(id, ScreenOperation.Close, null);
    }

    public void RaiseWindowCloseEvent(string id)
    {
        WindowEvent.Invoke(id, ScreenOperation.Close, null);
    }
}