using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using ICON.Utilities;

namespace FCG
{
    [CustomEditor(typeof(FCGWaypointsContainer))]
    public class FCGWPEditor : Editor
    {
        private FCGWaypointsContainer wpScript;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            wpScript = (FCGWaypointsContainer)target;

            if (wpScript != null && GUI.changed)
            {
                wpScript.RefreshAllWayPoints();
            }
        }

        void OnSceneGUI()
        {
            wpScript = (FCGWaypointsContainer)target;

            if (wpScript == null)
                return;

            Event e = Event.current;
            if (e == null)
                return;

            //-----------------------------------------
            // SHIFT + CLICK → ADD WAYPOINT
            //-----------------------------------------
            if (e.isMouse && e.shift && e.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
                {
                    GetWaypoints();

                    Vector3 pos = hit.point;

                    GameObject wp = new GameObject(wpScript.name + " - " + (wpScript.waypoints.Count + 1).ToString("00"));
                    wp.transform.position = pos + Vector3.up * 0.1f;
                    wp.transform.SetParent(wpScript.transform);

                    GetWaypoints();
                    wpScript.RefreshAllWayPoints();

                    Selection.activeObject = wpScript.gameObject;
                }
            }

            //-----------------------------------------
            // CLICK → SELECT WAYPOINT
            //-----------------------------------------
            else if (e.isMouse && !e.shift && e.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
                {
                    Vector3 clickPos = hit.point;

                    // ✅ FIXED (no warning)
                    var containers = Object.FindObjectsByType<FCGWaypointsContainer>(FindObjectsSortMode.None);

                    foreach (var container in containers)
                    {
                        if (container.transform.childCount > 1)
                        {
                            Transform[] points = container.GetComponentsInChildren<Transform>();

                            foreach (var p in points)
                            {
                                if (Vector3.Distance(p.position, clickPos) < 1f)
                                {
                                    Selection.activeObject = p;
                                    wpScript.RefreshAllWayPoints();
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            //-----------------------------------------
            // KEEP SELECTED OBJECT
            //-----------------------------------------
            Selection.activeGameObject = wpScript.gameObject;
        }

        //-----------------------------------------
        // GET WAYPOINTS
        //-----------------------------------------
        public void GetWaypoints()
        {
            if (wpScript == null)
                return;

            wpScript.waypoints = new List<Transform>();

            Transform[] all = wpScript.GetComponentsInChildren<Transform>();

            for (int i = 1; i < all.Length; i++)
            {
                all[i].name = wpScript.name + " - " + i.ToString("00");

                wpScript.waypoints.Add(all[i]);

                if (all[i] != null)
                    all[i].gameObject.SetIcon(LabelIcon.Yellow);
            }

            wpScript.WaypointsSetAngle();
        }
    }
}