using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Sirenix.OdinInspector;
using Cinemachine;
using Encore.Environment;
using Encore.Utility;

namespace Encore.CharacterControllers
{
    [AddComponentMenu("Encore/Character Controllers/Player Controller")]
    public class PlayerController : CharacterController
    {
        [Title("External Components")]
        [SerializeField] GameObject vCamPrefab;
        CinemachineVirtualCamera vCam;

        [Title("Camera")]
        [SerializeField] float zoomInMargin = 2;
        [SerializeField] float zoomInDuration = 2;
        [SerializeField] float originialDeadZoneWidth = 0.2f;

        public AudioListener AudioListener { get; private set; }
        public RigControllerPlayer RigControllerPlayer { get; private set; }
        public override RigController RigController { get => RigControllerPlayer; protected set => RigControllerPlayer = value as RigControllerPlayer; }

        Coroutine corCameraZooming = null;
        float originalOrthographicSize;

        protected override void Awake()
        {
            base.Awake();

            // Create VCam if doesn't exist
            vCam = Instantiate(vCamPrefab).GetComponent<CinemachineVirtualCamera>();
            vCam.m_Follow = transform;

            vCam.transform.position = transform.position;

            // Create AudioListener
            GameObject audioListenerGO = new GameObject("Player_AudioListener");
            AudioListener = audioListenerGO.AddComponent<AudioListener>();
            AlwaysFollow alwaysFollow = audioListenerGO.AddComponent<AlwaysFollow>();
            alwaysFollow.Setup(transform, new Vector3(0, 0, 0));

            RigControllerPlayer = GetComponent<RigControllerPlayer>();
            originalOrthographicSize = vCam.m_Lens.OrthographicSize;
        }

        public void CameraZoomIn(bool zoomingIn)
        {
            corCameraZooming = this.RestartCoroutine(Zooming());
            IEnumerator Zooming()
            {
                AnimationCurve animationCurve;
                if (zoomingIn)
                {
                    animationCurve = AnimationCurve.EaseInOut(0, vCam.m_Lens.OrthographicSize, zoomInDuration, originalOrthographicSize - zoomInMargin);
                }
                else
                {
                    animationCurve = AnimationCurve.EaseInOut(0, vCam.m_Lens.OrthographicSize, zoomInDuration, originalOrthographicSize);
                    vCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = originialDeadZoneWidth;
                }

                float time = 0;
                while (true)
                {
                    if (time > zoomInDuration) break;

                    vCam.m_Lens.OrthographicSize = animationCurve.Evaluate(time);
                    vCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth -= Time.deltaTime * 0.1f;

                    time += Time.deltaTime;
                    yield return null;
                }
                corCameraZooming = null;

            }
        }
    }
}