using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using NodeCanvas.DialogueTrees;

namespace Encore.CharacterControllers
{
    [AddComponentMenu("Encore/Character Controllers/NPC Controller")]
    public class NPCController : CharacterController
    {
        [Title("NPC Controller")]
        [SerializeField]
        bool enablePhysics = true;

        protected override void Awake()
        {
            base.Awake();

            if (enablePhysics)
            {
                rb2d.bodyType = RigidbodyType2D.Dynamic;
            }
            else
            {
                rb2d.bodyType = RigidbodyType2D.Static;
            }

        }

        protected override void FixedUpdate()
        {
            if (!enablePhysics) return;
            base.FixedUpdate();
        }
    }
}