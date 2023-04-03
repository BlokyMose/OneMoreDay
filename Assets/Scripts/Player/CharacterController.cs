using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Encore.CharacterControllers
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(AnimationController))]
    [AddComponentMenu("Encore/Character Controllers/Character Controller")]
    public class CharacterController : MonoBehaviour
    {
        #region [Vars: Properties]

        [Title("Movement")]
        [SerializeField]
        protected float _speed = 5f;
        public float speed
        {
            get { return _speed; }
            set
            {
                _speed = value;
                OnWalkSpeedChanged?.Invoke(_speed);
            }
        }

        [SerializeField]
        protected float _smoothing = 0.2f;

        [Title("External Components")]
        protected Rigidbody2D rb2d;
        protected Collider2D col;
        protected CharacterBrain brain;

        [SerializeField]
        ColliderDetector colliderDetector;
        public ColliderDetector ColliderController { get { return colliderDetector; } }
        public AnimationController AnimationController { get; protected set; }

        protected RigController rigController;
        public virtual RigController RigController { get => rigController; protected set => rigController = value; }

        #endregion

        #region [Vars: Data Handlers]

        // Data handlers
        protected Vector2 moveDirection;
        protected Vector3 velocity = Vector3.zero;


        // Delegates
        public Action<Vector2> OnWalk;
        public Action<float> OnWalkSpeedChanged;

        #endregion

        protected virtual void Awake()
        {
            // Setup PlayerBrain
            brain = GetComponent<CharacterBrain>();
            if (brain != null)
            {
                brain.Click += Click;
                brain.Move += Move;
            }

            // Get external components
            rb2d = GetComponent<Rigidbody2D>();
            rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
            col = GetComponent<Collider2D>();
            AnimationController = GetComponent<AnimationController>();
            RigController = GetComponent<RigController>();
        }

        protected virtual void OnEnable()
        {
            AnimationController.Setup(this);
            if (brain!=null)
                AnimationController.OnSpecial += (isInSpecial) => { brain.SetCanMoveInput(!isInSpecial); };
        }

        protected virtual void OnDisable()
        {
            AnimationController animationController = GetComponent<AnimationController>();
            if (animationController)
            {
                animationController.Setup(this);
                if (brain!=null)
                 animationController.OnSpecial += (isInSpecial) => { brain.SetCanMoveInput(!isInSpecial); };
            }
        }

        protected virtual void FixedUpdate()
        {
            Vector3 targetVelocity = new Vector2(moveDirection.x * _speed, 0);

            rb2d.velocity = Vector3.SmoothDamp(rb2d.velocity, targetVelocity, ref velocity, _smoothing);
        }

        /// <summary>
        /// Set moveDirection; To stop movement: Move(Vector3.zero)
        /// </summary>
        public void Move(Vector2 moveDirection)
        {
            this.moveDirection = moveDirection;
            OnWalk?.Invoke(moveDirection);
            if (moveDirection.x > 0)
            {
                transform.rotation = Quaternion.identity;
            }
            else if (moveDirection.x < 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }

        public void Click(InputAction.CallbackContext context)
        {

        }

    }
}