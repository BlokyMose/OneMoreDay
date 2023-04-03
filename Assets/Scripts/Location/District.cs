using System.Collections;
using UnityEngine;

namespace Encore.Locations
{
    [CreateAssetMenu(menuName = "SO/Location/District", fileName = "District_")]
    public class District : ScriptableObject
    {
        [SerializeField]
        string districtName;

        [SerializeField]
        string desc;

        public string DistrictName { get { return districtName; } }
        public string Desc { get { return desc; } }
    }
}