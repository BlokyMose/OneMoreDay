using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Playables;
using Encore.CharacterControllers;

namespace Encore.SceneMasters
{
    [RequireComponent(typeof(PlayableDirector))]
    [AddComponentMenu("Encore/Scene Masters/Demo/Hospital Entrance")]
    public class Demo_HospitalEntrance : SceneMaster
    {
        [Title("Scene Hospital", "Car & Border")]
        [SerializeField] Collider2D colTriggerCar;
        [SerializeField] GameObject car;
        [SerializeField] Collider2D colBorderLeft;
        PlayableDirector director;

        protected override void Awake()
        {
            base.Awake();
            director = GetComponent<PlayableDirector>();
        }

        public override void Init(InitialSettings settings)
        {
            base.Init(settings);

            // Hide UI Tools
            GameManager.Instance.InventoryManager.SetCanShow(false, false);
            GameManager.Instance.DoomclockManager.SetCanShow(false, false);
            GameManager.Instance.PhoneManager.SetCanShow(false, false);

            // Car & Border
            car.SetActive(false);
            colBorderLeft.isTrigger = true;
            StartCoroutine(CheckingColTriggerCar());
            IEnumerator CheckingColTriggerCar()
            {
                bool isUpdating = true;
                while (isUpdating)
                {
                    ContactFilter2D contactFilter2D = new ContactFilter2D();
                    List<Collider2D> overlappingCols = new List<Collider2D>();
                    colTriggerCar.OverlapCollider(contactFilter2D, overlappingCols);

                    foreach (Collider2D collider in overlappingCols)
                    {
                        if (collider.gameObject.name == PlayerBrain.PLAYER_NAME)
                        {
                            car.SetActive(true);
                            colBorderLeft.isTrigger = false;
                            isUpdating = false;
                            break;
                        }
                    }

                    yield return null;
                }
            }
        }

        public void MakePlayerWalkRight(float duration)
        {
            StartCoroutine(Update());
            IEnumerator Update()
            {
                float time = duration;
                while (true)
                {
                    Debug.Log(time);
                    GameManager.Instance.Player.Move(new Vector2(0.75f, 0));
                    time -= Time.deltaTime;
                    if (time < 0)
                    {
                        GameManager.Instance.Player.Move(new Vector2(0f, 0));
                        break;
                    }

                    yield return null;
                }
            }

        }
    }
}
