using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Windows;

namespace MapLoader
{
    internal class PlacementHelper
    {

        private struct SegmentMask
        {
            public List<string> includes;
            public List<string> excludes;

            public SegmentMask(IEnumerable<string> i, IEnumerable<string> e)
            {
                includes = i != null ? new List<string>(i) : new List<string>();
                excludes = e != null ? new List<string>(e) : new List<string>();
            }
        }

        private static readonly Dictionary<MapSegment, string> _anchorPaths = new Dictionary<MapSegment, string>
        {
            [MapSegment.Beach] = "Map/Biome_1/Beach/Beach_Campfire/WallTop",
            [MapSegment.Tropics] = "Map/Biome_2/Jungle/Jungle_Campfire/WallTop",
            [MapSegment.Alpine] = "Map/Biome_3/Snow/Snow_Campfire/WallSnow_Left_Top[+7];Map/Biome_3/Snow/Snow_Campfire/WallSnow_Right_Top[+7]",
            [MapSegment.Mesa] = "Map/Biome_3/Desert/Desert_Campfire/WallMesh[+20]",
            [MapSegment.Caldera] = "Map/Biome_4/Volcano/Caldera_Segment/Enterance/LavaTemple/Ruin[+10M]",
            [MapSegment.Kiln] = "Map/Biome_4/VolcanoModel/VolcanoModel[+85M];Map/Biome_4/Volcano/Caldera_Segment/Enterance/LavaTemple/Ruin[+0M]",
            [MapSegment.Peak] = "Map/Biome_4/Volcano/Peak/Rock_Platform[+25M];Map/Biome_4/Volcano/Peak/Bridge/Cliff[+0M]",
        };

        // this could definitely be simpler and more memory efficient but i dont give a shit
        private static readonly Dictionary<MapSegment, SegmentMask> _segments = new Dictionary<MapSegment, SegmentMask>
        {
            [MapSegment.Beach] = new SegmentMask(new[] {
                "Map/Biome_1/Beach",
                "Misc/SpawnPoints",
                "Misc/WaterZone",
                "Misc/Water",
                "Misc/WaterMesh",
                "FogSphereSystem/FogOrigins/Beach",
            }, null),
            [MapSegment.Tropics] = new SegmentMask(new[] {
                "Map/Biome_2/Jungle",
                "FogSphereSystem/FogOrigins/Tropics",
            }, null),
            [MapSegment.Alpine] = new SegmentMask (new[] {
                "Map/Biome_3/Snow",
                "FogSphereSystem/FogOrigins/Tundra",
            }, null),
            [MapSegment.Mesa] = new SegmentMask (new[] {
                "Map/Biome_3/Desert",
                "FogSphereSystem/FogOrigins/Tundra",
            }, null),
            [MapSegment.Caldera] = new SegmentMask(new[] {
                "Map/Biome_4/Volcano/Caldera_Segment",
                "Map/Biome_4/Volcano/SpecialDay Caldera (1)",
                "Map/Biome_4/Volcano/EruptionSpawner",
                "Map/Biome_4/Volcano/HelperObjects",
                "FogSphereSystem/FogOrigins/Caldera",
            }, new[] {
                "Map/Biome_4/Volcano/Caldera_Segment/LavaRivers", // Lava flowing on volcano should be moved with kiln
            }),
            [MapSegment.Kiln] = new SegmentMask(new[] {
                "Map/Biome_4/VolcanoModel/VolcanoModel",
                "Map/Biome_4/Volcano/Volcano_Segment",
                "Map/Biome_4/Volcano/Volcano_Campfire",
                "Map/Biome_4/Volcano/SpecialDay Volcano",
                "Map/Biome_4/Volcano/Respawn_Kiln",
                "Map/Biome_4/Volcano/Caldera_Segment/LavaRivers", // see caldera
            }, null),
            [MapSegment.Peak] = new SegmentMask(new[] { "Map/Biome_4/Volcano/Peak" }, null),
        };

        private static Vector3 _GetAnchor(MapSegment biome)
        {
            string anchorPath = _anchorPaths.GetValueSafe(biome);

            if (anchorPath.Contains(";"))
            {
                string[] paths = anchorPath.Split(';');
                GameObject MeshGO1 = GameObject.Find(paths[0]);
                GameObject MeshGO2 = GameObject.Find(paths[1]);

                MeshRenderer mc1 = MeshGO1.GetComponent<MeshRenderer>();
                MeshRenderer mc2 = MeshGO2.GetComponent<MeshRenderer>();

                return Vector3.Lerp(
                    new Vector3(mc1.bounds.center.x, (mc1.bounds.center.y + mc1.bounds.max.y) * 0.45f, (mc1.bounds.center.z + mc1.bounds.max.z) * 0.5f),
                    new Vector3(mc2.bounds.center.x, (mc2.bounds.center.y + mc2.bounds.max.y) * 0.45f, (mc2.bounds.center.z + mc2.bounds.max.z) * 0.5f),
                    0.5f);
            }


            GameObject MeshGO = GameObject.Find(anchorPath);
            MeshRenderer mc = MeshGO.GetComponent<MeshRenderer>();
            return new Vector3(mc.bounds.center.x, (mc.bounds.center.y + mc.bounds.max.y) * 0.45f, (mc.bounds.center.z + mc.bounds.max.z) * 0.5f);
        }

        internal static Vector3 GetAnchor(MapSegment biome)
        {
            string anchorPath = _anchorPaths.GetValueSafe(biome);

            string[] paths = anchorPath.Split(';');

            List<Bounds> MeshBounds = new List<Bounds>();
            foreach (string path in paths)
            {
                string _path = path;
                bool zMax = false;
                float yOffset = 1f;
                Match match = Regex.Match(path, @"^(.*?)(?:\[([-+]?\d*\.?\d+)(M)?\])?$");
                if (match.Success)
                {
                    _path = match.Groups[1].Value;
                    string _offset = match.Groups[2].Value;
                    if(!string.IsNullOrEmpty(match.Groups[3].Value))
                        zMax = true;

                    if (!string.IsNullOrEmpty(_offset))
                    {
                        yOffset = float.Parse(_offset) / 100f + 1f;
                    }
                }

                GameObject MeshGO = GameObject.Find(_path);
                MeshRenderer GOMeshRenderer = MeshGO.GetComponent<MeshRenderer>();

                MeshBounds.Add(new Bounds(
                        new Vector3(
                            GOMeshRenderer.bounds.center.x,
                            GOMeshRenderer.bounds.center.y * yOffset,
                            zMax ? (GOMeshRenderer.bounds.center.z + GOMeshRenderer.bounds.max.z) * 0.5f : GOMeshRenderer.bounds.center.z 
                        ),
                        GOMeshRenderer.bounds.size
                    )
                );
            }

            Vector3 ret = Vector3.zero;
            foreach (Bounds bounds in MeshBounds)
            {
                ret += new Vector3(bounds.center.x, (bounds.center.y + bounds.max.y) * 0.45f, (bounds.center.z + bounds.max.z) * 0.5f);
            }
            return ret / MeshBounds.Count;
        }

        internal static void MoveBiomeDelta(MapSegment biome, Vector3 delta)
        {
            SegmentMask mask = _segments.GetValueSafe(biome);

            foreach(string path in mask.includes)
            {
                GameObject.Find(path).transform.Translate(delta, Space.World);
            }

            // janky way to exclude for now
            foreach(string path in mask.excludes)
            {
                GameObject.Find(path).transform.Translate(-delta, Space.World);
            }
        }
    }
}
