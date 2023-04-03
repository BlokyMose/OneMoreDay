using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.SceneMasters
{
    public class Demo_Proto_Tutorial : SceneMaster
    {
        public override void Init(InitialSettings settings)
        {
            base.Init(settings);


            StartCoroutine(Delay(3));
            IEnumerator Delay(float delay)
            {
                GameManager.Instance.EnableUICornerTools(false, false);
                yield return new WaitForSeconds(delay);
                GameManager.Instance.EnableUICornerTools(true, false);
            }
        }
    }
}
