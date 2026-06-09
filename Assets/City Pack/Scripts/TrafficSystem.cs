using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FCG
{
    public class TrafficSystem : MonoBehaviour
    {
        public Transform player = null;

        [Header("Traffic Light:  0=Right  1=Left  2=Japan")]
        [Range(0, 2)]
        public int trafficLightHand = 0;

        public GameObject[] IaCars;

        public int nVehicles = 0;
        public int maxVehiclesWithPlayer = 50;

        [Range(100, 200)]
        public float around = 150;

        private bool firstTime = true;

        private Transform downTowmPosition;

        private List<WpDataSpawn> wpDataSpawn = new List<WpDataSpawn>();

        [System.Serializable]
        public class WpData
        {
            public bool[] tsActive;
            public Vector3[] tf01;
            public FCGWaypointsContainer[] tsParent;
            public bool[] tsOneway;
            public bool[] tsOnewayDoubleLine;
            public int[] tsSide;
        }

        private WpData wpData = new WpData();

        [System.Serializable]
        public class WpDataSpawn
        {
            public Vector3 position;
            public Quaternion rotation;
            public float locateZ;
            public int side;
            public int node;
            public FCGWaypointsContainer wayScript;
        }

        void Start()
        {
            GameObject dt = GameObject.Find("DTPosition");
            downTowmPosition = dt ? dt.transform : null;

            LoadCars(trafficLightHand);
        }

        public void UpdateAllWayPoints()
        {
            var tArray = FindObjectsByType<FCGWaypointsContainer>(FindObjectsSortMode.None);

            foreach (var t in tArray)
            {
                t.ResetWay();
                t.GetWaypoints();
            }

            GetWpData();

            foreach (var t in tArray)
            {
                if (t.transform.childCount > 1)
                {
                    t.wpData = wpData;
                    t.NextWaysCloseOnly();
                    t.NextWays();
                }
            }
        }

        public void GetWpData()
        {
            var ts = FindObjectsByType<FCGWaypointsContainer>(FindObjectsSortMode.None);

            int len = ts.Length * 2;

            wpData.tsActive = new bool[len];
            wpData.tf01 = new Vector3[len];
            wpData.tsParent = new FCGWaypointsContainer[len];
            wpData.tsOneway = new bool[len];
            wpData.tsOnewayDoubleLine = new bool[len];
            wpData.tsSide = new int[len];

            int t = -1;

            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i].waypoints.Count > 1)
                {
                    t++;

                    if (!ts[i].oneway || ts[i].doubleLine)
                    {
                        wpData.tsActive[t] = true;
                        wpData.tf01[t] = ts[i].Node(0, 0);
                        wpData.tsParent[t] = ts[i];
                        wpData.tsSide[t] = 0;
                        wpData.tsOneway[t] = ts[i].oneway;
                        wpData.tsOnewayDoubleLine[t] = ts[i].oneway && ts[i].doubleLine;
                    }
                    else
                        wpData.tsActive[t] = false;

                    t++;
                    wpData.tsActive[t] = true;
                    wpData.tf01[t] = ts[i].Node(1, 0);
                    wpData.tsParent[t] = ts[i];
                    wpData.tsSide[t] = 1;
                    wpData.tsOneway[t] = ts[i].oneway;
                    wpData.tsOnewayDoubleLine[t] = ts[i].oneway && ts[i].doubleLine;
                }
                else
                {
                    t++;
                    wpData.tsActive[t] = false;
                    t++;
                    wpData.tsActive[t] = false;
                }
            }
        }

        public void LoadCars(int right_Hand)
        {
            if (maxVehiclesWithPlayer == 0)
            {
                Debug.LogError("Set maxVehiclesWithPlayer!");
                return;
            }

            var ts = FindObjectsByType<FCGWaypointsContainer>(FindObjectsSortMode.None);

            foreach (var t in ts)
            {
                if (t.transform.childCount == 0)
                {
                    if (Application.isPlaying)
                        Destroy(t.gameObject);
                    else
                        DestroyImmediate(t.gameObject);
                }
            }

            UpdateAllWayPoints();

            if (!player)
                Debug.LogWarning("Player not assigned!");

            GameObject carContainer = GameObject.Find("CarContainer");

            if (!carContainer)
            {
                carContainer = new GameObject("CarContainer");
                nVehicles = 0;
            }
            else
            {
                nVehicles = carContainer.transform.childCount;
            }

            trafficLightHand = right_Hand;
            DeffineDirection(right_Hand);

            wpDataSpawn.Clear();

            ts = FindObjectsByType<FCGWaypointsContainer>(FindObjectsSortMode.None);

            foreach (var t in ts)
            {
                if (!t.bloked && t.waypoints.Count > 1)
                {
                    for (int side = 0; side <= 1; side++)
                    {
                        if ((!t.oneway || t.doubleLine) || (side == 1 && trafficLightHand == 0) || (side == 0 && trafficLightHand != 0))
                        {
                            for (int node = 0; node < t.waypoints.Count - 1; node++)
                            {
                                float dist = Vector3.Distance(t.Node(side, node), t.Node(side, node + 1));

                                if (dist > 20)
                                    PlaceSpawnPoint(t, side, node, dist / 2);
                            }
                        }
                    }
                }
            }

            if (player && Application.isPlaying)
                InvokeRepeating(nameof(LoadCars2), 0f, 5f);
            else
                LoadCars2();
        }

        private void PlaceSpawnPoint(FCGWaypointsContainer f, int side, int node, float locate)
        {
            wpDataSpawn.Add(new WpDataSpawn
            {
                locateZ = locate,
                position = f.AvanceNode(side, node, locate),
                rotation = f.NodeRotation(side, node),
                side = side,
                node = node,
                wayScript = f
            });
        }

        public void LoadCars2()
        {
            if (player == null && !firstTime)
                return;

            GameObject carContainer = GameObject.Find("CarContainer");
            nVehicles = carContainer ? carContainer.transform.childCount : 0;

            if (player && nVehicles >= maxVehiclesWithPlayer)
                return;

            bool invert = Random.value < 0.5f;

            for (int j = 0; j < wpDataSpawn.Count; j++)
            {
                int i = invert ? wpDataSpawn.Count - 1 - j : j;

                if (player)
                {
                    float dist = Vector3.Distance(wpDataSpawn[i].position, player.position);

                    if (dist > around || (!firstTime && dist < 80))
                        continue;

                    if (!firstTime && InTheFieldOfVision(player.position, wpDataSpawn[i].position))
                        continue;
                }

                bool canSpawn = true;

                if (!firstTime)
                {
                    canSpawn = !Physics.Linecast(
                        wpDataSpawn[i].wayScript.Node(wpDataSpawn[i].side, wpDataSpawn[i].node + 1) + Vector3.up,
                        wpDataSpawn[i].wayScript.Node(wpDataSpawn[i].side, wpDataSpawn[i].node) + Vector3.up
                    );
                }

                if (canSpawn)
                {
                    GameObject vehicle = Instantiate(
                        IaCars[Random.Range(0, IaCars.Length)],
                        wpDataSpawn[i].position + Vector3.up * 0.1f,
                        wpDataSpawn[i].rotation
                    );

                    vehicle.transform.SetParent(GameObject.Find("CarContainer").transform);

                    TrafficCar car = vehicle.GetComponent<TrafficCar>();

                    car.sideAtual = wpDataSpawn[i].side;
                    car.atualWay = wpDataSpawn[i].wayScript.transform;
                    car.atualWayScript = wpDataSpawn[i].wayScript;
                    car.currentNode = wpDataSpawn[i].node + 1;

                    if (player)
                    {
                        car.distanceToSelfDestroy = around;
                        car.player = player;
                        car.tSystem = this;
                        car.ActivateSelfDestructWhenAwayFromThePlayer();
                    }

                    nVehicles++;
                }
            }

            firstTime = false;
        }

        bool InTheFieldOfVision(Vector3 source, Vector3 target)
        {
            return !Physics.Linecast(source + Vector3.up * 2f, target + Vector3.up * 2f);
        }

        public void DeffineDirection(int hand_Right)
        {
            trafficLightHand = hand_Right;

            var TLs = FindObjectsByType<TFShiftHand2>(FindObjectsSortMode.None);
            foreach (var t in TLs)
                t.RightHand(trafficLightHand);

            var ts = FindObjectsByType<FCGWaypointsContainer>(FindObjectsSortMode.None);
            foreach (var t in ts)
                t.InvertNodesDirection(trafficLightHand);

            UpdateAllWayPoints();
        }
    }
}