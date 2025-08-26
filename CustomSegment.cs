using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MapLoader
{
    public class CustomSegment
    {
        private AssetBundle _bundle;
        private GameObject _asset;
        private string _assetPath;
        private PEAKCustomSegment _segment = null;

        public CustomSegment(AssetBundle assetBundle, string assetPath)
        {
            _bundle = assetBundle;
            _assetPath = assetPath;

            _asset = _bundle.LoadAsset<GameObject>(_assetPath);
            SetupMaterials(_asset);
        }

        [Obsolete("Use CustomSegment.CustomSegment(AssetBundle) instead")]
        public CustomSegment(string assetBundlePath, string assetPath)
        {
            _bundle = LoadBundle(assetBundlePath);
            _assetPath = assetPath;

            _asset = _bundle.LoadAsset<GameObject>(_assetPath);
            SetupMaterials(_asset);
        }

        public void Start()
        {
            _asset = UnityEngine.Object.Instantiate(_asset);
            MapLoader.Log.LogInfo($"{PCS.Name} has instantiated.");
        }

        public PEAKCustomSegment PCS
        {
            get
            {
                return _segment ?? (_segment = _asset.GetComponent<PEAKCustomSegment>());
            }
        }

        public GameObject EndAnchor { get { return _segment.EndAnchor; } }

        public GameObject StartAnchor { get { return _segment.StartAnchor; } }

        internal void MoveTo(Vector3 pos) { _asset.transform.position = pos; }
        internal void MoveDelta(Vector3 delta) { _asset.transform.Translate(delta, Space.World); }

        [Obsolete("This has very little error checking, please use your own implementation!")]
        internal static AssetBundle LoadBundle(string bundlePath)
        {
            Debug.Log("Loading AssetBundle...");
            
            if (!File.Exists(bundlePath))
            {
                Debug.LogError("AssetBundle not found @ " + bundlePath);
                return null;
            }
            Debug.Log("Found AssetBundle @ " + bundlePath);
            AssetBundle Bundle = AssetBundle.LoadFromFile(bundlePath);
            if (Bundle == null)
            {
                Debug.LogError("Failed to load AssetBundle @ " + bundlePath);
                return null;
            }
            Debug.Log("AssetBundle @ " + bundlePath + " loaded successfully!");
            return Bundle;
        }

        private static Dictionary<string, Shader> _peakShaders;

        private static Dictionary<string, Shader> PeakShaders
        {
            get
            {
                if (_peakShaders == null)
                {
                    List<string> shaders = new List<string>
                    {
                        "W/Peak_Standard",
                        "W/Character",
                        "W/Peak_Transparent",
                        "W/Peak_Glass",
                        "W/Peak_Clip",
                        "W/Peak_glass_liquid",
                        "W/Peak_GroundTransition",
                        "W/Peak_Guidebook",
                        "W/Peak_Honey",
                        "W/Peak_Ice",
                        "W/Peak_Rock",
                        "W/Peak_Rope",
                        "W/Peak_Splash",
                        "W/Peak_Waterfall",
                        "W/Vine",
                        "Universal Render Pipeline/Lit"
                    };
                    _peakShaders = new Dictionary<string, Shader>();
                    foreach (string sh in shaders)
                    {
                        Shader shader = Shader.Find(sh);
                        if (shader == null)
                        {
                            Debug.LogWarning("Shader " + sh + " not found, skipping.");
                        }
                        else
                        {
                            _peakShaders[sh] = shader;
                        }
                    }
                }
                return _peakShaders;
            }
        }

        private static void SetupMaterials(GameObject gameObject)
        {
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in renderer.materials)
                {
                    Shader peakShader;
                    if (PeakShaders.TryGetValue(mat.shader.name, out peakShader))
                    {
                        MapLoader.Log.LogInfo("Replacing '" + mat.shader.name + "' with '" + peakShader.name);
                        mat.shader = peakShader;
                    }
                }
            }
        }
    }
}
