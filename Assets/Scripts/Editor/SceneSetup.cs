using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PocketGarden.Editor
{
    public static class SceneSetup
    {
        [MenuItem("PocketGarden/Setup Scene")]
        public static void Setup()
        {
            // Camera
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 5.5f;
                cam.transform.position = new Vector3(0f, 0f, -10f);
                cam.backgroundColor = new Color(0.95f, 0.98f, 0.90f);
                cam.clearFlags = CameraClearFlags.SolidColor;
            }

            // GameManager
            if (GameObject.Find("GameManager") == null)
            {
                var gm = new GameObject("GameManager");
                gm.AddComponent<Core.GameManager>();
                gm.AddComponent<Grid.MergeGrid>();
                gm.AddComponent<Grid.DragDropHandler>();
                gm.AddComponent<Quests.QuestManager>();
            }

            // Canvas
            if (FindAnyObjectByType<Canvas>() == null)
            {
                var canvasGo = new GameObject("Canvas");
                var canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 1f;
                canvasGo.AddComponent<GraphicRaycaster>();
                canvasGo.AddComponent<UI.EnergyUI>();
                canvasGo.AddComponent<UI.QuestUI>();
                canvasGo.AddComponent<UI.TutorialOverlay>();
                canvasGo.AddComponent<UI.DailyBonus>();
            }

            // EventSystem (required for UI clicks)
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[PocketGarden] Scene setup complete!");
        }

        private static T FindAnyObjectByType<T>() where T : Object
        {
            return Object.FindAnyObjectByType<T>();
        }
    }
}
