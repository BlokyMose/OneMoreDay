using Encore.CharacterControllers;
using Encore.Interactables;
using Encore.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore
{
    [AddComponentMenu("Encore/Inventory/Item Preview")]
    public class ActionPreview : MonoBehaviour
    {

        #region [Classes]

        public enum AnimationMode { Hide = -1, None, PutDown }

        #endregion


        #region [Components]

        [SerializeField]
        SpriteRenderer sr;

        Animator animator;

        #endregion


        #region [Data Handlers]

        int int_mode, boo_show;

        #endregion


        void Awake()
        {
            gameObject.name = nameof(ActionPreview);
            animator = GetComponent<Animator>();
            int_mode = Animator.StringToHash(nameof(int_mode));
            boo_show = Animator.StringToHash(nameof(boo_show));
        }

        public void SetupSprite(SpriteRenderer sourceSR, Material mat)
        {
            sr.sprite = sourceSR.sprite;
            sr.material = Instantiate(mat);
            sr.sortingLayerID = sourceSR.sortingLayerID;
            sr.sortingLayerName = sourceSR.sortingLayerName;
            sr.sortingOrder = sourceSR.sortingOrder + 1;
        }

        public void PlayAnimation(AnimationMode animation)
        {
            animator.SetInteger(int_mode, (int)animation);
        }
    }
}
