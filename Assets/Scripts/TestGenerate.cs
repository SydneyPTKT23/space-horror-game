using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGenerate : MonoBehaviour
{
    public Renderer m_renderer;

    private void LateUpdate()
    {

        float t_timer = Mathf.PingPong(Time.time / 2, 1.0f);
        //m_renderer.sharedMaterial.SetFloat("_Progress", t_timer);
    }
}