using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceShaderContoller : MonoBehaviour
{
    public Material DistanceMaterial;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DistanceMaterial.SetVector("_PlayerPosition", transform.position);
    }
}
