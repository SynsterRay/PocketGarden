# Merge Garden — Game Design Document

## 1. Przegląd

**Tytuł:** Merge Garden (nazwa robocza)
**Gatunek:** Merge Puzzle + Dekoracja
**Platforma:** Android (iOS later)
**Engine:** Unity 6, URP, 2D
**Grupa docelowa:** Kobiety 25-45 lat, casual gamers
**Sesja:** 5-15 minut, 3-5 razy dziennie
**Monetyzacja:** Reklamy (banner, interstitial, rewarded) + IAP (energy, gems, paczki)
**Styl graficzny:** Cozy Flat 2D — kolorowe ikony, jasne tła, przyjazny wygląd

---

## 2. Core Loop

```
1. Merge przedmioty na planszy (siatka 5×7)
2. Uzyskaj wyższy level przedmiotu
3. Wypełnij zlecenia (quest: "dostarcz 2 drzewa")
4. Odblokuj nowy teren ogrodu
5. Postaw dekorację
6. Powtórz
```

**Retention hooks:**
- Energy system (wraca co 3 min)
- Daily bonus (streak → lepsze nagrody)
- Nowy quest co sesję
- Otwieranie nowych terenów (ciekawość: co tam jest?)

---

## 3. Mechanika Merge

### 3.1 Plansza

- Siatka 5×7 (35 slotów)
- Drag & drop — przeciągnij element na identyczny → merge
- Merge 2 identycznych = 1 wyższego poziomu
- Nowe elementy pojawiają się z **generatorów** (np. krzak co 30s daje liść)

### 3.2 Łańcuchy Merge (MVP — 3 łańcuchy)

**🌱 Ogród (Garden Chain)**
```
Nasionko (Lv1) → Sadzonka (Lv2) → Krzak (Lv3) → Kwiat (Lv4) → Drzewo (Lv5) → Wielkie Drzewo (Lv6) → Magiczne Drzewo (Lv7)
```

**🪵 Drewno (Wood Chain)**
```
Martwe Drzewo (Lv1) → Drewno (Lv2) → Deska (Lv3) → Skrzynka (Lv4) → Meble (Lv5) → Altanka (Lv6) → Domek (Lv7)
```

**🪨 Kamień (Stone Chain)**
```
Kamyk (Lv1) → Kamień (Lv2) → Cegła (Lv3) → Mur (Lv4) → Kolumna (Lv5) → Fontanna (Lv6) → Zamek (Lv7)
```

### 3.3 Generatory

| Generator | Produkuje | Cooldown | Odblokowanie |
|-----------|-----------|----------|--------------|
| Krzak | Nasionko | 30s | Start |
| Piła | Martwe Drzewo | 45s | Quest 3 |
| Kamieniołom | Kamyk | 60s | Quest 7 |

Generatory mają ograniczoną liczbę użyć → potem trzeba kupić nowy (gems) lub zdobyć z questa.

### 3.4 Zasady planszy

- Plansza pełna = nie można generować nowych elementów (motywacja do merge)
- Tap na element = info co z niego powstanie
- Merge 5 identycznych naraz = bonus (element +2 levele)
- Elementy można sprzedać za monety (nieopłacalne, ale czyści planszę)

---

## 4. System Questów

### 4.1 Struktura

Questy to główny driver progressji. Gracz widzi 1-3 aktywne questy na raz.

**Format questa:**
```
"Dostarcz: [ikona] x [ilość]"
np. "Dostarcz: 🌳 Drzewo x2"
```

### 4.2 Nagrody za questy

| Quest # | Wymaga | Nagroda |
|---------|--------|---------|
| 1 | 2× Kwiat | 50 monet + otwórz teren 1 |
| 2 | 1× Drzewo | 100 monet + nowy generator |
| 3 | 3× Deska | 150 monet + otwórz teren 2 |
| 4 | 2× Meble | 200 monet + dekoracja |
| 5 | 1× Fontanna | 300 monet + otwórz teren 3 |
| ... | Rosnące wymagania | Rosnące nagrody |

### 4.3 Dostarczanie

Gracz przeciąga gotowy przedmiot na ikonę questa → przedmiot znika → progress questa +1.

---

## 5. Ogród (Meta-game)

### 5.1 Mapa ogrodu

Ogród podzielony na **sekcje** (tereny) odblokowane questami:
- Teren 1: Trawnik (start)
- Teren 2: Ścieżka
- Teren 3: Staw
- Teren 4: Altanka
- Teren 5: Ogród różany
- ...

### 5.2 Dekoracje

Po odblokowaniu terenu gracz stawia dekoracje (zdobyte z questów lub sklep):
- Drzewa, kwiaty, ławki, fontanny, lampy, posągi
- Dekoracje są czysto wizualne (satysfakcja + progress feeling)
- Niektóre dekoracje premium (gems only)

### 5.3 Widok ogrodu

- Osobny ekran (przycisk "Ogród" / "Garden")
- Izometryczny lub top-down widok
- Gracz tap na wolne pole → wybiera dekorację z inventory

---

## 6. System Energii

### 6.1 Zasady

| Parametr | Wartość |
|----------|---------|
| Max energy | 30 |
| Koszt merge | 1 energy |
| Regeneracja | +1 co 3 minuty |
| Pełne naładowanie | 90 minut |
| Rewarded ad | +5 energy |
| IAP (mała paczka) | +30 energy za $0.99 |
| Daily bonus | +10-30 energy |

### 6.2 Dlaczego energy

- Ogranicza sesję → gracz wraca później (retencja D7+)
- Naturalny moment na reklamę ("Brak energii — obejrzyj reklamę?")
- Główny driver IAP (niecierpliwi płacą)

---

## 7. Ekonomia

### 7.1 Waluty

| Waluta | Źródło | Użycie |
|--------|--------|--------|
| Monety (coins) | Questy, daily bonus, sprzedaż | Kupowanie generatorów, dekoracji |
| Gemsy (gems) | IAP, rzadkie questy, rewarded ads | Energy, skip timer, premium dekoracje |

### 7.2 Balans

- Gracz F2P: ~20-30 merge'ów na sesję (5-10 min), 3-5 sesji/dzień
- Gracz płacący: nieograniczone sesje, szybsza progressja
- Nie pay-to-win (tylko szybkość, brak konkurencji PvP)

---

## 8. Monetyzacja

### 8.1 Reklamy

| Typ | Kiedy | Częstotliwość |
|-----|-------|---------------|
| Banner | Dolna krawędź (merge screen) | Stały |
| Interstitial | Po ukończeniu questa | Co 3 questy |
| Rewarded | "Brak energii" / "x2 nagroda" / "Darmowy generator" | Na żądanie |

### 8.2 IAP

| Produkt | Cena | Zawartość |
|---------|------|-----------|
| Garść energii | $0.99 | 30 energy |
| Worek energii | $4.99 | 200 energy + 50 gems |
| Starter Pack | $2.99 | 100 energy + 100 gems + premium dekoracja |
| Gem Pack S | $1.99 | 100 gems |
| Gem Pack M | $4.99 | 300 gems |
| Gem Pack L | $9.99 | 800 gems |
| VIP Pass (miesięczny) | $4.99 | +5 max energy/min regen, ekskluzywne dekoracje, no interstitials |

### 8.3 Prognoza ARPDAU

- F2P (reklamy): ~$0.05-0.10/dzień/gracz
- Mieszany: ~$0.15-0.30/dzień/gracz
- Wieloryb (whale): $5-50/miesiąc

---

## 9. Retention Systems

| System | Mechanizm |
|--------|-----------|
| Daily Bonus | Streak: dzień 1 = 10 energy, dzień 7 = 50 energy + rare generator |
| Generatory (timery) | "Twój krzak wyprodukował nasionko!" — push notification |
| Questy | Zawsze jest "co dalej" |
| Nowe tereny | Ciekawość — co jest za mgłą? |
| Kolekcjonowanie | Ogród się rozrasta — wizualny progress |
| Eventy sezonowe | Halloweenowy ogród, Świąteczny ogród (nowe łańcuchy) |

---

## 10. Przepływ ekranów

```
Splash Screen
    → Main Screen (plansza merge + pasek questów)
        ├── Ogród (dekorowanie)
        ├── Sklep (IAP + gems + energy)
        ├── Questy (lista aktywnych/ukończonych)
        ├── Daily Bonus (popup przy wejściu)
        ├── Ustawienia (język, muzyka, dźwięki)
        └── Kolekcja (odblokowane przedmioty)
```

### 10.1 Główny ekran (Merge Board)

```
┌─────────────────────────────┐
│ [⚡30] [💎50] [🪙1200] [⚙️] │  ← top bar
├─────────────────────────────┤
│                             │
│    SIATKA MERGE 5×7         │
│    (drag & drop)            │
│                             │
├─────────────────────────────┤
│ [Quest 1] [Quest 2] [Quest 3] │ ← bottom bar
├─────────────────────────────┤
│ [🌳Ogród] [🛒Sklep] [📋Quest] │ ← navigation
└─────────────────────────────┘
```

---

## 11. Dźwięk i muzyka

| Element | Styl |
|---------|------|
| Muzyka | Spokojna, akustyczna, "cozy garden" loop |
| Merge SFX | Przyjemny "pop" / "ding" rosnący z levelem |
| Quest complete | Fanfara |
| Generator ready | Delikatny dzwonek |
| UI clicks | Miękkie kliknięcia |

---

## 12. Tech Stack

| Komponent | Technologia |
|-----------|-------------|
| Engine | Unity 6 URP 2D |
| Ads | Google AdMob |
| IAP | Unity Purchasing |
| Analytics | Unity Analytics / Firebase |
| Notifications | com.unity.mobile.notifications |
| Save System | PlayerPrefs + JSON (local) |
| Języki | PL, EN, ES, PT, DE, FR (6 na start) |

---

## 13. Harmonogram MVP

| Tydzień | Zadania |
|---------|---------|
| 1 | Grid 5×7, drag & drop, merge logic, 3 łańcuchy, generatory |
| 2 | Questy, progressja, energy system, save/load |
| 3 | Ogród (widok + dekorowanie), sklep, IAP, ads, daily bonus |
| 4 | Grafiki (AI-generated), SFX, polish, UI |
| 5 | Testy, balans ekonomii, ASO, closed testing, publikacja |

---

## 14. KPI docelowe

| Metryka | Cel |
|---------|-----|
| D1 Retention | 45%+ |
| D7 Retention | 20%+ |
| D30 Retention | 8%+ |
| Avg. session | 8-12 min |
| Sessions/day | 3-5 |
| ARPDAU | $0.15-0.30 |
| IAP conversion | 3-5% graczy |

---

## 15. Ryzyka i mitygacja

| Ryzyko | Mitygacja |
|--------|-----------|
| Za dużo grafik | AI-generated sprites + prosty styl flat |
| Niska retencja | Testować quest pacing, energy balance |
| Niska monetyzacja | A/B test: ceny, ad frequency |
| Konkurencja (Merge Mansion) | Niższa bariera wejścia, szybszy progress, casualowy |
| Content drought | Planować eventy sezonowe, nowe łańcuchy co 2-4 tyg. |

---

## 16. Przyszłe rozszerzenia (po MVP)

- Nowe łańcuchy: Kwiaty, Jedzenie, Magia
- Eventy sezonowe (Halloween, Boże Narodzenie)
- Albumy kolekcjonerskie
- Social: odwiedzanie ogrodów znajomych
- Battle Pass miesięczny
- Mini-gry w ogrodzie (np. łapanie motyli)
