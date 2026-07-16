using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace RPGDragon.Editor
{
    /// <summary>
    /// One-time project setup tool. Run from Window -> RPGDragon -> Project Setup
    /// after opening the project in Unity for the first time.
    /// </summary>
    public class ProjectSetup : EditorWindow
    {
        [MenuItem("Window/RPGDragon/Project Setup")]
        public static void ShowWindow()
        {
            GetWindow<ProjectSetup>("RPGDragon Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("RPGDragon Project Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("1. Create Player Prefab", GUILayout.Height(30)))
            {
                CreatePlayerPrefab();
            }

            if (GUILayout.Button("2. Create EventSystem Prefab", GUILayout.Height(30)))
            {
                CreateEventSystemPrefab();
            }

            if (GUILayout.Button("3. Create Canvas Prefab (HUD)", GUILayout.Height(30)))
            {
                CreateHUDPrefab();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("4. Build All Scenes to Build Settings", GUILayout.Height(30)))
            {
                AddScenesToBuild();
            }

            GUILayout.Space(20);
            EditorGUILayout.HelpBox(
                "After setup, open MainMenu scene, drag HUDCanvas + EventSystem into it, " +
                "then press Play. The GameManager + AudioManager + SceneLoader singletons will " +
                "auto-create themselves.",
                MessageType.Info
            );
        }

        private void CreatePlayerPrefab()
        {
            string path = "Assets/Prefabs/Player.prefab";

            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Player");

            // Components
            player.AddComponent<Rigidbody2D>();
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            player.AddComponent<CapsuleCollider2D>();
            player.AddComponent<SpriteRenderer>();
            player.AddComponent<Animator>();

            // Scripts
            player.AddComponent<RPGDragon.Player.PlayerController>();
            player.AddComponent<RPGDragon.Player.PlayerStats>();
            player.AddComponent<RPGDragon.Player.PlayerCombat>();
            player.AddComponent<RPGDragon.Player.PlayerUpgrade>();

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(player, path);
            DestroyImmediate(player);

            Debug.Log($"[Setup] Player prefab created at {path}");
        }

        private void CreateEventSystemPrefab()
        {
            string path = "Assets/Prefabs/Systems/EventSystem.prefab";
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            PrefabUtility.SaveAsPrefabAsset(es, path);
            DestroyImmediate(es);
            Debug.Log($"[Setup] EventSystem prefab created at {path}");
        }

        private void CreateHUDPrefab()
        {
            string path = "Assets/Prefabs/UI/HUDCanvas.prefab";
            GameObject canvas = new GameObject("HUDCanvas");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            PrefabUtility.SaveAsPrefabAsset(canvas, path);
            DestroyImmediate(canvas);
            Debug.Log($"[Setup] HUDCanvas prefab created at {path}");
        }

        private void AddScenesToBuild()
        {
            var scenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Map1_Forest.unity",
                "Assets/Scenes/Map2_Dungeon.unity",
                "Assets/Scenes/Map3_BossCastle.unity"
            };

            var buildScenes = new EditorBuildSettingsScene[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                buildScenes[i] = new EditorBuildSettingsScene(scenes[i], true);
            }
            EditorBuildSettings.scenes = buildScenes;
            Debug.Log("[Setup] Added all scenes to Build Settings");
        }
    }
}
