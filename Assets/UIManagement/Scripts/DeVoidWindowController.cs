using UnityEngine;
using deVoid.UIFramework;

public class DeVoidWindowController : MonoBehaviour
{
    [SerializeField] private AWindowController windowControllerObj;

    public void OpenTheWindow(string panelID) {
        windowControllerObj.OpenTheWindow(panelID);
    }
}
