using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ObjectInteractibleSetting/*<T>*/ : MonoBehaviour
{
    //[SerializeField] private List<T> listOfObjs;
    [SerializeField] private List<Image> listImgs;
    [SerializeField] private List<TextMeshProUGUI> listTxts;

    public void MakeObjInteractable(bool isInteractable) {
        //foreach (T obj in listOfObjs)
        //    obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, 1);

        if (isInteractable) {
            foreach (Image obj in listImgs)
                obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, 1);
            foreach (TextMeshProUGUI obj in listTxts)
                obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, 1);
        } else {
            foreach (Image obj in listImgs)
                obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, 0.5f);
            foreach (TextMeshProUGUI obj in listTxts)
                obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, 0.5f);
        }
    }
}
