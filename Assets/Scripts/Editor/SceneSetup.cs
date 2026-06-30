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

            // GameManager — create if missing, then additively ensure each system component
            // (so re-running Setup wires in newly added systems like SFXManager on old scenes).
            var gm = GameObject.Find("GameManager");
            if (gm == null) gm = new GameObject("GameManager");
            Ensure<Core.GameManager>(gm);
            Ensure<Grid.MergeGrid>(gm);
            Ensure<Grid.DragDropHandler>(gm);
            Ensure<Quests.QuestManager>(gm);
            Ensure<Core.IAPManager>(gm);
            Ensure<Ads.AdManager>(gm);
            Ensure<Audio.SFXManager>(gm);

            // Canvas
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 1f;
                canvasGo.AddComponent<GraphicRaycaster>();
            }
            var c = canvas.gameObject;
            Ensure<UI.HudBar>(c);
            Ensure<UI.QuestUI>(c);
            Ensure<UI.TutorialOverlay>(c);
            Ensure<UI.DailyBonus>(c);
            Ensure<UI.OfferManager>(c);
            Ensure<UI.CoinPopup>(c);
            Ensure<UI.SplashScreen>(c);

            // EventSystem (required for UI clicks)
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[PocketGarden] Scene setup complete!");
        }

        private static void Ensure<T>(GameObject go) where T : Component
        {
            if (go.GetComponent<T>() == null) go.AddComponent<T>();
        }
    }
}
