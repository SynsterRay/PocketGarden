using UnityEngine;
using UnityEngine.UI;
using PocketGarden.Core;

namespace PocketGarden.UI
{
    /// <summary>
    /// The single top HUD bar. Owns the three currency chips (Energy / Coins / Gems) and the
    /// Shop + Settings buttons in one evenly-spaced row so nothing overlaps. Also drives energy
    /// regen ticking. Icons load from Resources/UIButtons (run "PocketGarden → Generate
    /// Placeholder Art"); falls back to emoji glyphs if the sprites are not present yet.
    /// </summary>
    public class HudBar : MonoBehaviour
    {
        private Text _energyText;
        private Text _coinText;
        private Text _gemText;
        private Canvas _canvas;

        private void Start()
        {
            _canvas = FindAnyObjectByType<Canvas>();
            if (_canvas == null) return;

            UIFactory.Panel(_canvas.transform, new Vector2(0f, 0.925f), new Vector2(1f, 1f),
                new Color(1f, 1f, 1f, 0.96f), "HudBar");

            _energyText = Chip("EnergyChip", "icon_energy", "⚡", UIFactory.EnergyTeal, 0.015f, 0.245f);
            _coinText   = Chip("CoinChip",   "icon_coin",   "🪙", UIFactory.Gold,       0.255f, 0.485f);
            _gemText    = Chip("GemChip",    "icon_gem",    "💎", UIFactory.Gem,        0.495f, 0.725f);

            // Tap the energy chip to refill with gems.
            var energyChip = _energyText.transform.parent.gameObject;
            var refillBtn = energyChip.AddComponent<Button>();
            refillBtn.transition = Selectable.Transition.None;
            refillBtn.onClick.AddListener(() =>
            {
                if (EnergySystem.IsFull) return;
                GemConfirmPopup.Show("Refill energy to full?", GemEconomy.EnergyRefillCost,
                    () => GemEconomy.TryRefillEnergy());
            });

            IconButton("icon_fill", "📦", new Color(0.2f, 0.6f, 0.2f), 0.68f, 0.735f, OnFillBoardClick);
            IconButton("icon_shop", "🛒", UIFactory.Leaf, 0.75f, 0.86f, () =>
            {
                var s = FindAnyObjectByType<ShopUI>() ?? _canvas.gameObject.AddComponent<ShopUI>();
                s.Toggle();
            });
            IconButton("icon_gem", "⏭", UIFactory.Gem, 0.87f, 0.935f, OnGemSkipClick);
            IconButton("icon_settings", "⚙", new Color(0.6f, 0.6f, 0.6f), 0.945f, 1f, () =>
            {
                var m = FindAnyObjectByType<MainMenu>() ?? _canvas.gameObject.AddComponent<MainMenu>();
                m.Toggle();
            });

            EnergySystem.OnEnergyChanged += UpdateEnergy;
            CoinSystem.OnCoinsChanged += UpdateCoins;
            GemSystem.OnGemsChanged += UpdateGems;

            UpdateEnergy(EnergySystem.Energy);
            UpdateCoins(CoinSystem.Coins);
            UpdateGems(GemSystem.Gems);
        }

        private void OnDestroy()
        {
            EnergySystem.OnEnergyChanged -= UpdateEnergy;
            CoinSystem.OnCoinsChanged -= UpdateCoins;
            GemSystem.OnGemsChanged -= UpdateGems;
        }

        private void Update() => EnergySystem.Tick();

        private static Sprite Icon(string name) => Resources.Load<Sprite>($"UIButtons/{name}");

        /// <summary>Chip = rounded bg + icon (sprite or emoji) + value text. Returns the value Text.</summary>
        private Text Chip(string name, string iconName, string glyph, Color iconColor, float xMin, float xMax)
        {
            var chip = UIFactory.Panel(_canvas.transform, new Vector2(xMin, 0.935f), new Vector2(xMax, 0.995f),
                new Color(0.96f, 0.97f, 0.92f, 1f), name);

            var sprite = Icon(iconName);
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(chip.transform, false);
            var img = iconGo.AddComponent<Image>();
            UIFactory.Stretch(iconGo.GetComponent<RectTransform>(), new Vector2(0.04f, 0.12f), new Vector2(0.34f, 0.88f));
            img.raycastTarget = false;
            if (sprite != null)
            {
                img.sprite = sprite;
                img.preserveAspect = true;
                img.color = Color.white;
            }
            else
            {
                // fallback: tinted rounded tile + emoji glyph
                img.sprite = UIFactory.RoundedSprite();
                img.type = Image.Type.Sliced;
                img.color = iconColor;
                var g = UIFactory.Text(iconGo.transform, glyph, Vector2.zero, Vector2.one, 26, Color.white);
                g.raycastTarget = false;
            }

            return UIFactory.Text(chip.transform, "0", new Vector2(0.36f, 0f), new Vector2(0.97f, 1f),
                26, UIFactory.Ink, TextAnchor.MiddleLeft);
        }

        /// <summary>Rounded button with an icon sprite (or emoji fallback).</summary>
        private void IconButton(string iconName, string glyph, Color bg, float xMin, float xMax, System.Action onClick)
        {
            var btn = UIFactory.Button(_canvas.transform, "", new Vector2(xMin, 0.94f),
                new Vector2(xMax, 0.99f), bg, 26);
            btn.onClick.AddListener(() => onClick());

            var sprite = Icon(iconName);
            if (sprite != null)
            {
                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(btn.transform, false);
                var img = iconGo.AddComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;
                img.raycastTarget = false;
                UIFactory.Stretch(iconGo.GetComponent<RectTransform>(), new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.88f));
            }
            else
            {
                var t = btn.GetComponentInChildren<Text>();
                if (t != null) t.text = glyph;
            }
        }

        private void UpdateEnergy(int v)
        {
            if (_energyText == null) return;
            _energyText.text = $"{v}/{EnergySystem.Max}";
            _energyText.color = v == 0 ? UIFactory.Danger : UIFactory.Ink;
        }

        private void UpdateCoins(int v) { if (_coinText != null) _coinText.text = v.ToString(); }
        private void UpdateGems(int v) { if (_gemText != null) _gemText.text = v.ToString(); }

        private void OnGemSkipClick()
        {
            var grid = FindAnyObjectByType<Grid.MergeGrid>();
            if (grid == null) return;

            var readyGen = grid.Generators.Find(g => g != null && g.IsReady);
            if (readyGen != null)
            {
                readyGen.TryProduce();
                return;
            }

            var anyGen = grid.Generators.Find(g => g != null && g.CanSkip);
            if (anyGen != null)
            {
                GemConfirmPopup.Show("Skip generator cooldown?", GemEconomy.GeneratorSkipCost, () =>
                {
                    if (GemSystem.Spend(GemEconomy.GeneratorSkipCost))
                    {
                        anyGen.SkipCooldown();
                        anyGen.TryProduce();
                        SaveSystem.SaveGrid(grid);
                    }
                });
                return;
            }

            // No generator available
            GemConfirmPopup.Show("No generator available", 0, null);
        }

        private void OnFillBoardClick()
        {
            var grid = FindAnyObjectByType<Grid.MergeGrid>();
            if (grid == null) return;

            int cost = grid.Rows * grid.Cols * 10;
            GemConfirmPopup.Show($"Fill board with random items? ({cost} 💎)", cost, () =>
            {
                if (GemSystem.Spend(cost))
                {
                    grid.FillBoardWithRandomItems();
                    SaveSystem.SaveGrid(grid);
                }
            });
        }
    }
}
