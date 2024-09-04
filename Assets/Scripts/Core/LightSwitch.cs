using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public class LightSwitch : InteractableBase
    {
        [Space, Header("Image Loading Settings")]
        [SerializeField] private float loadingSpeed = 2.0f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

        public SnapshotCamera m_camera;
        public Renderer m_renderer;

        private IEnumerator m_loading;


        private bool m_isLoading;

        public override void OnInteract()
        {
            base.OnInteract();

            HandleLoading();
        }

        private void HandleLoading()
        {
            m_camera.CallTakeSnapshot();
            InvokeLoading();
        }

        private void InvokeLoading()
        {
            if (m_loading != null)
                StopCoroutine(m_loading);

            m_loading = Loading();
            StartCoroutine(m_loading);
        }

        private IEnumerator Loading()
        {
            m_isLoading = true;

            float t_percent = 0.0f;
            float t_speed = 1.0f / loadingSpeed;

            while(t_percent < 1.0f)
            {
                t_percent += Time.deltaTime * t_speed;
                float t_smoothPercent = transitionCurve.Evaluate(t_percent);

                m_renderer.material.SetFloat("_Progress", t_smoothPercent);

                yield return null;
            }

            m_isLoading = false;
        }
    }
}