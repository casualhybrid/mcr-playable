using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadScene : MonoBehaviour
{
    bool curve = true;
    [SerializeField] GameObject curveobj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void LoadMyScene(int sceneno)
    {
        SceneManager.LoadScene(sceneno);
    }

    public void curvebutton()
    {
        curve = !curve;
        curveobj.SetActive(curve ? true : false);
    }
}
