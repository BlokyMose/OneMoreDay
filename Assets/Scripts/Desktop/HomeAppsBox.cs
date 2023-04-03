using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Desktop
{
    [AddComponentMenu("Encore/Desktop/Home Apps Box")]
    public class HomeAppsBox : MonoBehaviour
    {
        public void UpdateBox(string s)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform item = transform.GetChild(i);
                item.gameObject.SetActive(item.name.ToLower().Contains(s.ToLower()));
            }
        }
    }
}