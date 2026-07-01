# Pocket Garden — Improvements & Roadmap

## 🔴 Blockers (przed release)

- [x] **GemSystem persistence** — PlayerPrefs restored (was hardcoded to 10000 for testing).
- [x] **Tutorial tylko raz** — shows only on first run (`PG_TutorialSeen` PlayerPrefs flag). "❓ How to Play" button in Settings re-triggers it.
- [x] **Save versioning** — `GridSaveData.saveVersion` (currently v2) + sequential `Migrate()` method. Old saves with "twig_1" auto-convert to "wood_1".

---

## 🟡 Gameplay / UX (wysoki priorytet)

- [x] **Animacja dostarczenia** — item leci łukiem do karty questa → kurczy się → checkmark + haptic (`UI/DeliveryAnimation.cs`).
- [x] **Haptic feedback** — wibracja przy merge, dostarczeniu, tap generatora (`Handheld.Vibrate()`).
- [x] **Board full handling** — potrząśnięcie siatki + popup "Plansza pełna!" z podpowiedzią (`UI/BoardFullPopup.cs`).
- [ ] **Podgląd wymagań** — miniatura sprite'a wymaganego itemu na karcie questa.
- [ ] **Undo / recycle** — tap+hold na item → "Recycle za 1💎?" Zapobiega dead-lockom.
- [ ] **Streak multiplier** — szybkie merge'y w ciągu 2s → combo ×2/×3 na monety.
- [ ] **Offline production** — generatory produkują offline (capped do 3 itemów max). Powód do powrotu.

---

## 🟢 Visual / Polish

- [x] **Idle bounce** — itemy na planszy delikatnie "oddychają" (sin scaleY ±2%, fazowane per item).
- [x] **Particle trail** — podczas drag, delikatny ślad złotych iskier za przeciąganym itemem.
- [ ] **Generator progress ring** — kołowy pasek postępu zamiast żółtego kwadratu.
- [ ] **Tło z parallaxem** — 3 warstwy (chmury, wzgórza, trawa) z minimalnym ruchem.
- [ ] **Transition wipe** — fade/circle-wipe przy otwieraniu Shop/Settings.
- [ ] **Quest complete celebration** — confetti + krótka animacja gwiazdek po ukończeniu.

---

## 🔵 Monetyzacja / Retention

- [ ] **Rewarded ad → extra slot** — tymczasowo +1 rząd planszy na 30s po obejrzeniu reklamy.
- [ ] **Weekly challenge** — specjalne tygodniowe zadanie z exclusive nagrodą (skin generatora).
- [ ] **Collection book** — album odkrytych itemów (sylwetki nie odblokowanych → motywacja).
- [ ] **Daily login calendar** — 7-dniowy kalendarz z rosnącymi nagrodami (zamiast obecnego prostego streaka).
- [ ] **Season pass lite** — free + premium track z dekoracjami ogrodu jako nagrodami.

---

## 🟣 Meta-game (Garden Decoration)

- [ ] **Osobny ekran ogrodu** — 2D grid dekoracji odblokowanych za monety z questów.
- [ ] **Dekoracje** — ławka, lampa, staw, łuk z różami, gnomek, fontanna (prompty w ART_PROMPTS.md).
- [ ] **Star rating** — każdy quest daje ⭐, gwiazdki odblokowują nowe pola w ogrodzie.
- [ ] **Snapshot & share** — zrzut ekranu ogrodu z watermarkiem "Pocket Garden" → social share.

---

## ⚙️ Techniczne / Infrastruktura

- [ ] **Analytics (Firebase)** — eventy: `quest_complete`, `chain_unlocked`, `purchase`, `ad_watched`, `energy_depleted`, `session_start`.
- [ ] **Remote Config** — balans (cooldowny, regen, ceny) server-side bez aktualizacji appki.
- [ ] **Push Notifications** — "Twoja energia jest pełna!", "Generator gotowy!", daily reminder.
- [ ] **Accessibility** — większe hit-boxy generatorów, opcja powiększenia UI, opis alt dla screen readerów.
- [ ] **Localization** — multi-language (PL/EN/DE/ES/FR) z tabelą string keys.
- [ ] **CI/CD** — GitHub Actions: build → test → deploy to Google Play Internal Track.
- [ ] **Crashlytics** — Firebase Crashlytics dla raportów crash.

---

## Priorytety (sugerowana kolejność)

1. ~~GemSystem fix~~ ✅
2. ~~Tutorial only once~~ ✅
3. ~~Animacja dostarczenia~~ ✅
4. ~~Haptic feedback~~ ✅
5. Quest item preview (30 min)
6. Analytics setup (1h, dane do decyzji)
7. Background music (asset generation)
8. Garden decoration meta-game (duży feature, osobny sprint)
