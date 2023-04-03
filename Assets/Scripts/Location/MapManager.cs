using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Locations
{
    [AddComponentMenu("Encore/Locations/Map Manager")]
    public class MapManager : MonoBehaviour
    {
        [SerializeField]
        List<MapDistrict> districts = new List<MapDistrict>();

        public List<MapDistrict> Districts { get { return districts; } }

        [SerializeField]
        List<MapPoint> points = new List<MapPoint>();

        public List<MapPoint> Points { get { return points; } }

        public void SelectDistrict(District district)
        {
            foreach (var dis in districts)
            {
                if(dis.District == district)
                    dis.Select(true);
                else
                    dis.Select(false);
            }
        }

        #region [Methods: Inspector Utility]

#if UNITY_EDITOR

        [TitleGroup("Utility"),SerializeField]
        MapPoint mapPointPrefab;

        [TitleGroup("Utility"),SerializeField]
        Transform mapPointsParent;

        [TitleGroup("Utility")]
        [Button(ButtonSizes.Large), GUIColor("@Color.green")]
        void AddMapPoint()
        {
            var dataArray = UnityEditor.AssetDatabase.FindAssets(nameof(MapPoint));
            if(dataArray != null && dataArray.Length > 0)
            {
                var newMapPoint = Instantiate(mapPointPrefab, mapPointsParent);
                newMapPoint.gameObject.name = "New_MapPoint";
                points.Add(newMapPoint);

                UnityEditor.Selection.activeGameObject = newMapPoint.gameObject;
                UnityEditor.EditorGUIUtility.PingObject(newMapPoint);
            }
        }

#endif

#endregion
    }
}
