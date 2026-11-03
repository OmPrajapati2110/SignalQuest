using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace SignalQuest.Editor
{
    public static class CreateDemoScene
    {
        private const string Base = "Assets/SignalQuest/";
        private const string Generated = Base + "Generated/";

        [MenuItem("Signal Quest/Create Demo Scene")]
        public static void Create()
        {
            if (Application.isPlaying) { Debug.LogWarning("Stop Play mode before generating the scene."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Generated); AssetDatabase.Refresh();
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Base + "UI/SignalQuestUI.uxml");
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(Base + "UI/SignalQuestTheme.tss");
            if (tree == null || theme == null) throw new InvalidOperationException("Copy the whole Assets/SignalQuest folder into the Unity project's Assets folder.");
            var definitions = new MissionDefinition[4];
            var defaults = CircuitLogic.Builtins();
            for (int i = 0; i < definitions.Length; i++)
            {
                string path = Generated + defaults[i].Id + ".asset";
                definitions[i] = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
                if (definitions[i] == null)
                {
                    definitions[i] = ScriptableObject.CreateInstance<MissionDefinition>(); definitions[i].SetData(defaults[i]);
                    AssetDatabase.CreateAsset(definitions[i], path);
                }
                CircuitLogic.Validate(definitions[i].ToData());
            }
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(Generated + "PanelSettings.asset");
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                panel.scaleMode = PanelScaleMode.ScaleWithScreenSize; panel.referenceResolution = new Vector2Int(1280, 900);
                panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight; panel.match = 0.5f;
                panel.clearColor = false; panel.themeStyleSheet = theme;
                AssetDatabase.CreateAsset(panel, Generated + "PanelSettings.asset");
            }
            var material = MaterialAsset();
            var sprite = SpriteAsset();
            var prefab = ModulePrefab(sprite, material);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Station Camera");
            var camera = cameraObject.AddComponent<Camera>(); cameraObject.tag = "MainCamera";
            camera.orthographic = true; camera.orthographicSize = 2f;
            camera.transform.position = new Vector3(0, 0, -10); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.08f, 0.13f);
            var stationObject = new GameObject("Relay Station"); var station = stationObject.AddComponent<StationView>();
            station.stationCamera = camera; station.modules = new StationModule[4];
            MakeSprite("Main power bus", stationObject.transform, sprite, material, new Vector3(0,-0.28f,0), new Vector3(12.3f,.12f,1), new Color(.20f,.32f,.43f), 0);
            for (int i = 0; i < 4; i++)
            {
                var module = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                module.name = defaults[i].Title; module.transform.SetParent(stationObject.transform);
                module.transform.localPosition = new Vector3(-4.5f + i*3f, 0, 0);
                station.modules[i] = module.GetComponent<StationModule>();
            }
            var ui = new GameObject("Signal Quest UI"); var document = ui.AddComponent<UIDocument>();
            document.panelSettings = panel; document.visualTreeAsset = tree;
            var game = ui.AddComponent<SignalQuestGame>(); game.missions = definitions; game.station = station;
            EditorSceneManager.SaveScene(scene, Generated + "SignalQuest.unity");
            AssetDatabase.SaveAssets(); Selection.activeGameObject = ui;
            Debug.Log("Scene created at " + Generated + "SignalQuest.unity. Open Game view (1280x900 recommended), then press Play.");
        }

        private static Material MaterialAsset()
        {
            string path = Generated + "StationUnlit.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("A sprite shader is required. Create the project using Universal 2D.");
            material = new Material(shader); AssetDatabase.CreateAsset(material, path); return material;
        }

        private static Sprite SpriteAsset()
        {
            string path = Generated + "StationPixel.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                var colors = new Color[64]; for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
                texture.SetPixels(colors); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 8; importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException("StationPixel.png must be imported as a single sprite.");
            return sprite;
        }

        private static GameObject ModulePrefab(Sprite sprite, Material material)
        {
            string path = Generated + "StationModule.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (existing != null) return existing;
            var root = new GameObject("Station Module");
            MakeSprite("Hull", root.transform, sprite, material, Vector3.zero, new Vector3(2.2f, .9f, 1), new Color(.17f,.26f,.37f), 1);
            MakeSprite("Upper rail", root.transform, sprite, material, new Vector3(0,.47f,0), new Vector3(2.35f,.09f,1), new Color(.36f,.51f,.64f), 2);
            var light = MakeSprite("Status light", root.transform, sprite, material, new Vector3(0,.09f,0), new Vector3(.75f,.22f,1), new Color(.85f,.43f,.28f), 3);
            root.AddComponent<StationModule>().indicator = light;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path); UnityEngine.Object.DestroyImmediate(root); return prefab;
        }

        private static SpriteRenderer MakeSprite(string name, Transform parent, Sprite sprite, Material material, Vector3 position, Vector3 scale, Color color, int order)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent); obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            var renderer = obj.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sharedMaterial = material;
            renderer.color = color; renderer.sortingOrder = order; return renderer;
        }
    }
}
