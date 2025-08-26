using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using System.Reflection;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Sirenix.Serialization.Utilities;
using System.Collections;
using System.Linq;

// TODO map is done - vector3.distance end points, move GO by that distance

namespace MapLoader
{
    [BepInPlugin(GUID, NAME, VER)]
    //[BepInDependency(modGUID)]
    public class MapLoader : BaseUnityPlugin
    {
        public const string GUID = "xyz.poogle.peak_maploader";
        public const string NAME = "MapLoader";
        public const string VER = "1.0.0";
        public readonly Harmony harmony = new Harmony(GUID);

        public static MapLoader Instance;
        public static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource(GUID);

        internal static List<CustomSegment> _segments = new List<CustomSegment>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Patches.InitPatches();

            string bundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "maps", "outer_space");

            CustomSegment bigLink = new CustomSegment(CustomSegment.LoadBundle(bundlePath), "assets/poof/cmap os.prefab");

            RegisterSegment(bigLink);


            Log.LogInfo("Initialized!");
        }

        public static void RegisterSegment(CustomSegment segment)
        {
            _segments.Add(segment);
        }
    }

    internal class Patches
    {
        internal static void InitPatches()
        {
            MapLoader.Instance.harmony.PatchAll();
        }

        [HarmonyPatch(typeof(AchievementManager), "ThrowAchievement")]
        [HarmonyPrefix]
        private static bool AchievementManager_ThrowAchievement_Prefix(AchievementManager __instance)
        {
            return false;
        }

        [HarmonyPatch]
        public class LoadingScreenHandler_Load
        {
            private static bool _didFirstLoad = false;

            public static MethodBase TargetMethod()
            {
                return typeof(LoadingScreenHandler).GetMethod("Load", BindingFlags.Public | BindingFlags.Instance);
            }

            public static void Prefix(LoadingScreen.LoadingScreenType type, ref Action runAfter)
            {
                if(_didFirstLoad || type != LoadingScreen.LoadingScreenType.Plane)
                    return;

                MapLoader.Log.LogError($"1 SpawnPoint.allSpawnPoints = {SpawnPoint.allSpawnPoints.Count}");

                Action orig = runAfter;

                runAfter = () =>
                {
                    orig?.Invoke();

                    MapLoader.Log.LogError($"2 SpawnPoint.allSpawnPoints = {SpawnPoint.allSpawnPoints.Count}");

                    MapLoader._segments.ForEach(segment =>
                    {
                        MapLoader.Log.LogInfo($"Loading {segment.PCS.Name}...");

                        Vector3 endAnchorSnap = PlacementHelper.GetAnchor(segment.PCS.ReferenceBiome);
                        MapLoader.Log.LogInfo($"{segment.PCS.ReferenceBiome} anchor @ {endAnchorSnap}");

                        Vector3 delta; 

                        switch (segment.PCS.Placement)
                        {
                            case PEAKCustomSegment._ReferenceOperation.Replace:
                                MapLoader.Log.LogInfo("Proceeding in replace mode!");

                                delta = endAnchorSnap - segment.EndAnchor.transform.position;

                                segment.MoveDelta(delta);

                                segment.Start();

                                MapLoader.Log.LogInfo("Moved segment into place!");

                                for (int i = (int)segment.PCS.ReferenceBiome - 1; i >= 0; i--)
                                {
                                    MapLoader.Log.LogInfo($"{(MapSegment)i} = {i}");
                                    PlacementHelper.MoveBiomeDelta((MapSegment)i, segment.StartAnchor.transform.position - PlacementHelper.GetAnchor((MapSegment)i));
                                }

                                MapLoader.Log.LogInfo("Moved all previous segments into place!");

                                break;
                            case PEAKCustomSegment._ReferenceOperation.Before:
                            case PEAKCustomSegment._ReferenceOperation.After:
                            default:
                                break;
                        }


                    });

                    

                    if (false) {
                        Shader urp = Shader.Find("Universal Render Pipeline/Lit");
                        Material pink = new Material(urp);
                        pink.SetColor("_BaseColor", Color.magenta);

                        if (true)
                        {
                            Vector3 _pos = PlacementHelper.GetAnchor(MapSegment.Beach);

                            GameObject _base_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            _base_sphere.transform.localScale = new Vector3(5, 5, 5);
                            _base_sphere.GetComponent<Renderer>().sharedMaterial = pink;
                            _base_sphere.transform.position = _pos;
                        }
                        if (true)
                        {
                            Vector3 _pos = PlacementHelper.GetAnchor(MapSegment.Tropics);

                            GameObject _base_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            _base_sphere.transform.localScale = new Vector3(5, 5, 5);
                            _base_sphere.GetComponent<Renderer>().sharedMaterial = pink;
                            _base_sphere.transform.position = _pos;
                        }
                        if (true)
                        {
                            Material cyan = new Material(urp);
                            cyan.SetColor("_BaseColor", Color.cyan);
                            Vector3 _pos = PlacementHelper.GetAnchor(MapSegment.Alpine);

                            GameObject _base_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            _base_sphere.transform.localScale = new Vector3(5, 5, 5);
                            _base_sphere.GetComponent<Renderer>().sharedMaterial = cyan;
                            _base_sphere.transform.position = _pos;
                        }
                        if (true)
                        {
                            Vector3 _pos = PlacementHelper.GetAnchor(MapSegment.Mesa);

                            GameObject _base_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            _base_sphere.transform.localScale = new Vector3(5, 5, 5);
                            _base_sphere.GetComponent<Renderer>().sharedMaterial = pink;
                            _base_sphere.transform.position = _pos;
                        }
                        if (true)
                        {
                            Vector3 _pos = PlacementHelper.GetAnchor(MapSegment.Caldera);

                            GameObject _base_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            _base_sphere.transform.localScale = new Vector3(5, 5, 5);
                            _base_sphere.GetComponent<Renderer>().sharedMaterial = pink;
                            _base_sphere.transform.position = _pos;
                        }
                        if (true)
                        {
                            Vector3 _pos = PlacementHelper.GetAnchor(MapSegment.Kiln);

                            GameObject _base_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            _base_sphere.transform.localScale = new Vector3(5, 5, 5);
                            _base_sphere.GetComponent<Renderer>().sharedMaterial = pink;
                            _base_sphere.transform.position = _pos;
                        }
                        if (true)
                        {
                            Vector3 _pos = PlacementHelper.GetAnchor(MapSegment.Peak);

                            GameObject _base_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            _base_sphere.transform.localScale = new Vector3(5, 5, 5);
                            _base_sphere.GetComponent<Renderer>().sharedMaterial = pink;
                            _base_sphere.transform.position = _pos;
                        }
                    }
                };
            }
        }
    }
}
