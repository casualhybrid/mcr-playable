using UnityEngine;
using deVoid.UIFramework;
using TheKnights.SaveFileSystem;

public class SaveWindowController : AWindowController
{
    [SerializeField] private SaveManager saveManagerObj;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SaveAndClosePanel() {
       // saveManagerObj.SaveGame(0, false);
        this.UI_Close();
    }


}
