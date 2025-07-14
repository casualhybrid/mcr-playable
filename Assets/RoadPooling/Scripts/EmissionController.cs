using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
[RequireComponent(typeof(Renderer))]
public class EmissionController : MonoBehaviour
{
    [SerializeField] private float speed = 1;
    [SerializeField] private float maxEmission = 2;
    [SerializeField] private float minEmission = 1;

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        _renderer.GetPropertyBlock(_propBlock);

      //  Material mat = GetComponent<MeshRenderer>().material;
        float val = Mathf.PingPong(Time.time * speed, 1);
        float emission = Mathf.Lerp(minEmission, maxEmission, val);
        //    Color color = mat.GetColor("_EmissionColor");
        //  mat.SetColor("_EmissionColor", color * emission);
        _propBlock.SetFloat("_Emission", emission);
        // mat.SetFloat("_Emission", emission);

        _renderer.SetPropertyBlock(_propBlock);
    }
}
