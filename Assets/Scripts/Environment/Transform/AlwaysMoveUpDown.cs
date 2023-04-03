using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Environment
{
    [AddComponentMenu("Encore/Environment/Transform/Always Move Up Down")]
    public class AlwaysMoveUpDown : MonoBehaviour
    {
        [SerializeField] float duration = 3f;
        [SerializeField] float speed = 1f;

        int isUp = 1;
        float time = 0;

        // Update is called once per frame
        void Update()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + speed * Time.deltaTime * isUp);

            time += Time.deltaTime;
            if (time >= duration)
            {
                time = 0;
                isUp *= -1;
            }
        }
    }
}