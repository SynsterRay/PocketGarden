using UnityEngine;
using UnityEngine.InputSystem;
using PocketGarden.Core;
using PocketGarden.Quests;

namespace PocketGarden.Grid
{
    public class DragDropHandler : MonoBehaviour
    {
        private Camera _cam;
        private MergeGrid _grid;
        private MergeGridItem _dragging;
        private GridCell _originCell;
        private Vector3 _offset;
        private float _lastTapTime;
        private MergeGridItem _lastTapItem;

        // UI drag proxy so the dragged item renders ABOVE Canvas panels (delivery zone etc.)
        private Canvas _canvas;
        private GameObject _dragProxy;
        private UnityEngine.UI.Image _dragProxyImage;

        // 🟢 Particle trail during drag
        private ParticleSystem _dragTrail;

        private void Start()
        {
            _cam = Camera.main;
            _grid = FindAnyObjectByType<MergeGrid>();
            _canvas = FindAnyObjectByType<Canvas>();
        }

        private void Update()
        {
            bool down = false, held = false, up = false;
            Vector2 pos = Vector2.zero;

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                pos = Touchscreen.current.primaryTouch.position.ReadValue();
                down = Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
                held = true;
                up = Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
            }
            else if (Mouse.current != null)
            {
                pos = Mouse.current.position.ReadValue();
                down = Mouse.current.leftButton.wasPressedThisFrame;
                held = Mouse.current.leftButton.isPressed;
                up = Mouse.current.leftButton.wasReleasedThisFrame;
            }

            if (down) BeginDrag(pos);
            else if (held && _dragging != null) ContinueDrag(pos);
            if (up && _dragging != null) EndDrag(pos);
        }

        private void BeginDrag(Vector2 screenPos)
        {
            Vector3 worldPos = _cam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            var hits = Physics2D.OverlapPointAll(worldPos);
            if (hits.Length == 0) return;

            GridCell cellWithItem = null;
            Generator readyGen = null;
            Generator anyGen = null;

            foreach (var h in hits)
            {
                var cell = h.GetComponent<GridCell>();
                if (cell != null && !cell.IsEmpty) cellWithItem = cell;
                var gen = h.GetComponent<Generator>();
                if (gen != null)
                {
                    anyGen = gen;
                    if (gen.IsReady) readyGen = gen;
                }
            }

            // Item takes priority
            if (cellWithItem != null && EnergySystem.HasEnergy)
            {
                var item = cellWithItem.Item;

                if (item == _lastTapItem && Time.time - _lastTapTime < 0.4f)
                {
                    var qm = FindAnyObjectByType<QuestManager>();
                    if (qm != null && qm.TryDeliver(item.Data))
                    {
                        // Delivery animation instead of instant destroy.
                        PlayDeliveryAnim(item.gameObject, screenPos, item.GetComponent<SpriteRenderer>()?.sprite);
                        cellWithItem.Item = null;
                        _lastTapItem = null;
                        SaveSystem.SaveGrid(_grid);
                        return;
                    }
                }
                _lastTapItem = item;
                _lastTapTime = Time.time;

                _dragging = item;
                _originCell = cellWithItem;
                _offset = _dragging.transform.position - worldPos;

                var sr = _dragging.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = 20;
                _dragging.transform.localScale = Vector3.one * 1.15f;

                // Create a UI proxy on the Canvas so the item renders ABOVE all panels.
                CreateDragProxy(sr, screenPos);

                // 🟢 Start particle trail
                StartDragTrail(worldPos);
                return;
            }

            // No item — tap generator
            if (readyGen != null)
            {
                readyGen.TryProduce();
                TriggerHaptic();
            }
        }

        private void ContinueDrag(Vector2 screenPos)
        {
            Vector3 worldPos = _cam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;
            _dragging.transform.position = worldPos + _offset;

            // Move UI proxy to follow finger (screen space).
            if (_dragProxy != null)
            {
                var rt = _dragProxy.GetComponent<RectTransform>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.GetComponent<RectTransform>(), screenPos, null, out var local);
                rt.anchoredPosition = local;
            }

            // 🟢 Move particle trail
            if (_dragTrail != null)
                _dragTrail.transform.position = worldPos + _offset;

            var targetCell = _grid.GetCellAt(worldPos);
            for (int r = 0; r < _grid.Rows; r++)
                for (int c = 0; c < _grid.Cols; c++)
                    _grid.Cells[r, c].SetHighlight(false);
            if (targetCell != null)
                targetCell.SetHighlight(true);
        }

        private void EndDrag(Vector2 screenPos)
        {
            // Destroy the UI proxy and particle trail.
            DestroyDragProxy();
            StopDragTrail();

            for (int r = 0; r < _grid.Rows; r++)
                for (int c = 0; c < _grid.Cols; c++)
                    _grid.Cells[r, c].SetHighlight(false);

            // Check if dropped on quest delivery zone
            var questUI = UI.QuestUI.Instance;
            if (questUI != null && questUI.IsInDropZone(screenPos) && _dragging != null)
            {
                var qm = FindAnyObjectByType<Quests.QuestManager>();
                if (qm != null && qm.TryDeliver(_dragging.Data))
                {
                    _originCell.Item = null;
                    // 🟡 Delivery animation
                    var sprite = _dragging.GetComponent<SpriteRenderer>()?.sprite;
                    PlayDeliveryAnim(_dragging.gameObject, screenPos, sprite);
                    _dragging = null;
                    _originCell = null;
                    SaveSystem.SaveGrid(_grid);
                    return;
                }
            }

            Vector3 worldPos = _cam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            var targetCell = _grid.GetCellAt(worldPos);

            bool success = false;
            if (targetCell != null && targetCell != _originCell)
            {
                success = _grid.TryMerge(_dragging, targetCell);
                if (success) TriggerHaptic(); // 🟡 Haptic on merge
            }

            // Tap (no move/merge) on a ready producer item → produce
            if (!success && _dragging != null
                && (targetCell == null || targetCell == _originCell)
                && _dragging.IsProducerReady)
            {
                if (_dragging.TryProduce())
                {
                    TriggerHaptic();
                    SaveSystem.SaveGrid(_grid);
                }
            }

            if (!success && _dragging != null)
                _dragging.transform.position = _originCell.transform.position;

            if (_dragging != null)
            {
                var sr = _dragging.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = 10;
                _dragging.transform.localScale = Vector3.one;
            }

            if (success) SaveSystem.SaveGrid(_grid);

            _dragging = null;
            _originCell = null;
        }

        // --- Delivery animation integration ----------------------------------

        private void PlayDeliveryAnim(GameObject itemGo, Vector2 screenPos, Sprite sprite)
        {
            var anim = UI.DeliveryAnimation.Instance;
            if (anim != null && sprite != null)
            {
                // Target: approximate position of the quest card (left-center of screen).
                var targetScreen = new Vector2(Screen.width * 0.3f, Screen.height * 0.15f);
                anim.Play(itemGo, screenPos, targetScreen, sprite);
            }
            else
            {
                Destroy(itemGo);
            }
        }

        // --- Haptic feedback -------------------------------------------------

        private static void TriggerHaptic()
        {
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif
        }

        // --- Drag Proxy (renders above Canvas UI panels) ---------------------

        private void CreateDragProxy(SpriteRenderer sr, Vector2 screenPos)
        {
            if (_canvas == null || sr == null || sr.sprite == null) return;

            _dragProxy = new GameObject("DragProxy");
            _dragProxy.transform.SetParent(_canvas.transform, false);
            _dragProxy.transform.SetAsLastSibling(); // topmost in Canvas

            _dragProxyImage = _dragProxy.AddComponent<UnityEngine.UI.Image>();
            _dragProxyImage.sprite = sr.sprite;
            _dragProxyImage.preserveAspect = true;
            _dragProxyImage.raycastTarget = false;

            var rt = _dragProxy.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100f, 100f); // approximate screen-size of the item
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.GetComponent<RectTransform>(), screenPos, null, out var local);
            rt.anchoredPosition = local;

            // Make the world-space sprite semi-transparent so player sees both feedback.
            sr.color = new Color(1f, 1f, 1f, 0.35f);
        }

        private void DestroyDragProxy()
        {
            if (_dragProxy != null)
            {
                Destroy(_dragProxy);
                _dragProxy = null;
                _dragProxyImage = null;
            }

            // Restore full opacity on the world-space sprite.
            if (_dragging != null)
            {
                var sr = _dragging.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;
            }
        }

        // --- 🟢 Particle trail during drag -----------------------------------

        private void StartDragTrail(Vector3 worldPos)
        {
            var go = new GameObject("DragTrail");
            go.transform.position = worldPos;
            _dragTrail = go.AddComponent<ParticleSystem>();
            _dragTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _dragTrail.main;
            main.duration = 10f;
            main.startLifetime = 0.4f;
            main.startSpeed = 0.3f;
            main.startSize = 0.08f;
            main.startColor = new Color(1f, 0.9f, 0.4f, 0.8f); // warm gold sparkle
            main.gravityModifier = 0.3f;
            main.maxParticles = 40;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = _dragTrail.emission;
            emission.rateOverTime = 30f;

            var shape = _dragTrail.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.08f;

            var sizeOverLife = _dragTrail.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            var colorOverLife = _dragTrail.colorOverLifetime;
            colorOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.8f, 0.2f), 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLife.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 25;

            _dragTrail.Play();
        }

        private void StopDragTrail()
        {
            if (_dragTrail != null)
            {
                _dragTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(_dragTrail.gameObject, 1f); // let existing particles fade
                _dragTrail = null;
            }
        }
    }
}
