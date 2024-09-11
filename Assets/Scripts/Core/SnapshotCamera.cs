using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    [RequireComponent(typeof(Camera))]
    public class SnapshotCamera : MonoBehaviour
    {
        private Camera m_camera;

        int width = 256;
        int height = 256;

        private void Start()
        {
            m_camera = GetComponent<Camera>();

            if (m_camera.targetTexture == null)
            {
                m_camera.targetTexture = new RenderTexture(width, height, 24);
            }
            else
            {
                width = m_camera.targetTexture.width;
                height = m_camera.targetTexture.height;
            }

            m_camera.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (m_camera.gameObject.activeInHierarchy)
            {
                Texture2D t_imageTexture = new(width, height, TextureFormat.ARGB32, false);
                m_camera.Render();
                RenderTexture.active = m_camera.targetTexture;
                t_imageTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);

                m_camera.gameObject.SetActive(false);
            }
        }

        public void CallTakeSnapshot()
        {
            m_camera.gameObject.SetActive(true);
        }
    }
}