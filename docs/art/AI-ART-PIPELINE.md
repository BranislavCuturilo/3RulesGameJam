# 3TD — AI Art Pipeline & Setup

Vodič za **kompletan redizajn arta** igre 3TD i za **stalni razvoj novih asseta**, lokalno i besplatno, na AMD hardveru.

- **Cilj:** jedinstven, kohezivan vizuelni stil za celu igru (tornjevi, neprijatelji, projektili, UI, mapa, rule karte) + ponovljiv alat za buduće assete.
- **Hardver:** AMD Ryzen 5 9600X + Radeon RX 7800 XT (16 GB VRAM), Windows.
- **Princip:** generisanje radimo lokalno (besplatno, neograničeno). Trošimo eventualno < $1 jednom, samo za LoRA trening u oblaku ako zatreba.

> Napomena o AMD-u: RX 7800 XT (čip `gfx1101`, 16 GB) je sasvim sposobna za SDXL i Flux. Caka je samo što AMD nema CUDA, pa koristimo **ZLUDA** (prevodi CUDA→ROCm na Windows-u) ili **ROCm na Linux-u** za maksimalne performanse.

---

## Pregled pipeline-a

```
[Faza 0] Instalacija alata  →  [Faza 1] Zaključavanje stila (style bible)
      →  [Faza 2] Konzistentna produkcija (IPAdapter + seed + img2img + ControlNet)
      →  [Faza 3] Post (uklanjanje pozadine, upscale, sečenje)
      →  [Faza 4] Unity integracija
      →  [Ongoing] Novi asseti uvek prolaze kroz istu referencu
```

Sve faze koriste **iste 2-3 "hero" reference** koje napraviš u Fazi 1 — to je tajna doslednosti kroz ceo redizajn.

---

## Faza 0 — Instalacija toolchaina

Imaš tri opcije; preporuka je **A** (najbolji odnos lakoće/performansi na Windows-u).

### Opcija A (preporuka): ComfyUI-ZLUDA na Windows-u

ComfyUI je čvorovni (node) editor — daje ti potpunu kontrolu (IPAdapter, ControlNet, img2img), što nam treba za doslednost.

**Preduslovi:**
1. **Git** — https://git-scm.com/download/win
2. **Python 3.11** (ne 3.12+ zbog kompatibilnosti) — https://www.python.org/downloads/ — čekiraj "Add to PATH".
3. **AMD HIP SDK 6.1 ili 6.2 za Windows** — https://www.amd.com/en/developer/resources/rocm-hub/hip-sdk.html
   - RX 7800 XT (`gfx1101`) je zvanično podržana u 6.1+. Posle instalacije dodaj `C:\Program Files\AMD\ROCm\6.1\bin` u PATH (instaler obično doda).
4. Najnoviji **Adrenalin** drajveri za grafičku.

**Instalacija:**
```powershell
# 1. Kloniraj ZLUDA fork (patientx) — održavan za AMD/Windows
git clone https://github.com/patientx/ComfyUI-Zluda
cd ComfyUI-Zluda

# 2. Pokreni instalacioni skript (pravi venv, skida ZLUDA, patch-uje torch)
.\install.bat

# 3. Pokreni
.\start.bat
```
Prvo pokretanje je sporo (ZLUDA kešira kernele 5-15 min). Otvori `http://127.0.0.1:8188`.

> Ako vidiš grešku tipa `rocBLAS` / `gfx1101 not found`: proveri da je HIP SDK 6.1+ instaliran i da je u PATH-u. Za `gfx1101` obično ne treba ručno kopiranje biblioteka (za razliku od starijih kartica).

### Opcija B (najlakša): Amuse AI

Zvanična AMD/Stability desktop aplikacija (DirectML, optimizovana za Radeon). Klikneš-instaliraš-radi. Dobra da **odmah** testiraš stil pre nego što se baviš ComfyUI-jem. Mana: manje kontrole (slabiji IPAdapter/ControlNet workflow).
- https://www.amuse-ai.com/

### Opcija C (maks. performanse): Linux + ROCm

Ako želiš najbrže generisanje i lak LoRA trening lokalno: dual-boot Ubuntu 22.04/24.04 + **ROCm 6.x** + obična ComfyUI/Automatic1111. `gfx1101` je ROCm-podržan. Više setup-a, ali nativno (bez ZLUDA sloja). Razmotri tek ako ti Windows perf ne bude dovoljan.

### Modeli koje skidaš (u `models/checkpoints/`)

| Model | Za šta | Gde |
|-------|--------|-----|
| **SDXL** baza ili stilizovani checkpoint (npr. *Illustrious*, *Pony*, neki "cartoon/anime" SDXL) | brzo, lako, 16 GB ga lako vrti | civitai.com / huggingface |
| **Flux.1 [schnell]** (GGUF Q5/Q8) | viši kvalitet, 4 koraka | huggingface.co (city96 GGUF) |
| **Retro Diffusion** ili pixel-art LoRA | ako biraš pixel stil | civitai.com |

### Custom čvorovi (kroz ComfyUI-Manager)

Instaliraj **ComfyUI-Manager** prvo (https://github.com/ltdrdata/ComfyUI-Manager), pa kroz njega:
- **ComfyUI_IPAdapter_plus** — style referenca (ključno za doslednost).
- **comfyui_controlnet_aux** — ControlNet (silueta/poza).
- **ComfyUI-Impact-Pack** — pomoćni alati.
- **ComfyUI rembg** (ili koristi `rembg` CLI zasebno) — uklanjanje pozadine.
- **Ultimate SD Upscale** — uvećavanje.

Zaseban alat za pozadinu (jednostavno i pouzdano):
```powershell
pip install rembg[gpu]
# kasnije: rembg i ulaz.png izlaz.png
```

---

## Faza 1 — Zaključavanje stila (NAJVAŽNIJI korak)

Pošto radiš **pun redizajn**, prvo definišeš stil, pa SVE izvlačiš iz njega. Bez ovog koraka assets ispadnu nedosledni.

### 1.1 Napravi "Style Bible" (1 strana)
Zapiši i fiksiraj:
- **Perspektiva:** top-down 2D (ovo ti je već odlučeno).
- **Stil:** pixel art *ili* cartoon/vector 2D (vidi odluku ispod).
- **Paleta:** 5-8 boja (npr. preko coolors.co). Igra ima zlatno/tamni fantasy ton na UI-u — možeš ga zadržati.
- **Debljina linije, senčenje (cel-shaded/flat), nivo detalja, osvetljenje (odakle pada svetlo).**
- **Rezolucija sprajta:** npr. 256×256 ili 512×512 za cartoon; 32×32/48×48/64×64 za pixel.

### 1.2 Odluči stil jeftinim testom
Trenutni art igre **nije** pixel (tornjevi i slime su cartoon, rule karte slikarske). Dve čiste rute:
- **Cartoon/2D** → kohezivnije sa nekim postojećim assetima, manje "ručnog" pixel doterivanja.
- **Pixel art** → najjeftinije po assetu, najlakše animirati, ali moraš redizajnirati apsolutno sve (uključujući UI) da ne bude sudara.

**Test (15 min):** generiši isti toranj (npr. Sniper) u oba stila, stavi pored mape/UI-a, izaberi šta "seda". Zaključi i upiši u Style Bible. **Ne menjaj stil posle ovog koraka.**

### 1.3 Napravi 2-3 "hero" reference
Izgeneriši i ručno odaberi 2-3 najbolja asseta u finalnom stilu (npr. jedan toranj, jedan neprijatelj). Sačuvaj ih u `art-source/style-refs/`. **Ovo su tvoje IPAdapter reference za sve ostalo.**

---

## Faza 2 — Konzistentna produkcija (bez treniranja)

Za ceo redizajn (deseci asseta) ovo drži stil ujednačenim, besplatno:

1. **IPAdapter (style transfer):** u workflow ubaci `IPAdapter` čvor i daj mu hero referencu iz Faze 1 (style weight ~0.6-0.8). Svaki novi asset izlazi u istom stilu.
2. **Fiksni seed + isti checkpoint + isti prompt template:** menjaš samo opis subjekta ("sniper tower" → "frost tower"), ostalo isto.
3. **img2img za tier-ove:** napraviš tier-1 toranj, pa img2img (denoise ~0.4-0.55) za tier 2/3 sa promenom "more barrels / bigger / glowing core".
4. **ControlNet (scribble/canny):** grubo nacrtaš siluetu/orijentaciju da svi tornjevi gledaju isto (top-down). Drži kompoziciju doslednom.
5. **Batch:** generiši 4-8 varijanti po assetu, ručno biraš najbolju.

### Kada (i samo tada) LoRA
Ako praviš jako mnogo asseta i hoćeš savršenu doslednost — istreniraj LoRA na 15-30 svojih odabranih slika. **Ne treniraj lokalno na AMD-u** (mučno) nego:
- **civitai.com** on-site trainer (plaća se "buzz", može besplatno/jeftino), ili
- iznajmi **RTX 4090** na **runpod.io / vast.ai** ~1h (≈ $0.40-0.70) i istreniraj SDXL LoRA jednom (<$1 ukupno).
Gotov `.safetensors` LoRA ubaciš u lokalni ComfyUI (`models/loras/`) i koristiš besplatno zauvek.

---

## Faza 3 — Post-processing

1. **Pozadina → transparentno:**
   ```powershell
   rembg i art-source\raw\sniper_t1.png art-source\clean\sniper_t1.png
   ```
   (ili rembg čvor u ComfyUI). Generiši na ravnoj/jednobojnoj pozadini radi čistog reza.
2. **Upscale ako treba:** Ultimate SD Upscale ili `realesrgan`.
3. **Pixel art doterivanje (ako pixel stil):** spusti na ciljnu rezoluciju + ograniči paletu (npr. u Aseprite/Photopea), da bude "pravi" pixel a ne mutan AI.
4. **Animacije (slime i sl.):** za pixel — PixelLab.ai/EbSynth za frejmove; za cartoon — generiši ključne poze (idle/move/hit/die) pa sečeš u sprite sheet.

---

## Faza 4 — Unity integracija

1. Ubaci PNG-ove u `Assets/` (npr. `Assets/Art/Towers/`).
2. Import settings na sprajtu:
   - **Texture Type:** Sprite (2D and UI)
   - **Pixels Per Unit:** uskladi sa igrom (npr. 100; za pixel art koristi tačan PPU = visina sprajta u px)
   - **Filter Mode:** *Point (no filter)* za pixel art; *Bilinear* za cartoon
   - **Compression:** None/High po potrebi
3. **Sprite sheet** (animacije): Sprite Mode = Multiple → Sprite Editor → Slice (Grid by Cell Size) → napravi Animation Clip.
4. Za pixel-perfect: dodaj **Pixel Perfect Camera** komponentu (paket `com.unity.2d.pixel-perfect`).
5. Zameni reference u prefab-ima (`Assets/Prefabs/Towers/*.prefab`, `Assets/Slimes/...`) novim sprajtovima.

> Posle redizajna, regeneriši i cheatsheet slike: zameni fajlove u `cheatsheet/img/...` (vidi `cheatsheet/README.md`), ili ponovo iskopiraj iz Unity asseta.

---

## Workflow po tipu asseta

| Asset | Količina | Metod |
|-------|----------|-------|
| **Toweri** (bogovi: Svarog, Perun, Dažbog, Veles, Vila, Domovoj, Baba Jaga) | 7 × 3 tiera = 21 | IPAdapter hero ref → base bog; img2img za tier 2/3 (jača aura/ornament) |
| **Projektili** (vatra, grom, sunčev zrak, magija...) | po toweru | mali sprajt, isti stil, ControlNet za oblik |
| **Neprijatelji** (Drekavac, Bauk, Karakondžula, Vampir, Psoglav, Čuma, Aždaja-boss) | 7+ + animacije | hero ref → varijante; ključne poze → sprite sheet |
| **UI** (store, cart, dugmad, rule okvir) | ~10 | flat, isti paleta; tile- abilni paneli |
| **Mapa / staza** | 1+ | tileset ili velika ilustracija staze |
| **Rule karte** | 25 | ilustracije po imenu pravila (AttackMode, Boss, Tanky...) — isti okvir/format |

---

## Ongoing art development (posle redizajna)

Da svaki budući asset bude konzistentan:
1. Drži `art-source/style-refs/` (hero reference) i jedan **sačuvan ComfyUI workflow** (`.json`) kao "template".
2. Novi asset = otvoriš template → promeniš prompt subjekta → IPAdapter već ima referencu → generiši → rembg → Unity.
3. Verziuj `art-source/` (raw + clean + workflow .json) da uvek možeš da reprodukuješ stil.
4. Ako stil evoluira, ažuriraj hero reference i (po potrebi) ponovo istreniraj LoRA.

---

## Troškovi

| Stavka | Cena |
|--------|------|
| Generisanje lokalno (ComfyUI-ZLUDA, RX 7800 XT) | **$0**, neograničeno |
| Amuse / modeli / custom čvorovi | **$0** |
| rembg / upscale / Unity | **$0** |
| (Opciono) LoRA trening u oblaku, jednom | **< $1** |
| (Opciono) PixelLab/Scenario pretplata ako hoćeš zero-setup | $0 (free tier) – ~$10/mes |

---

## Troubleshooting (AMD specifično)

- **ZLUDA prva generacija jako spora** → normalno, kešira kernele; sledeće su brze.
- **`HIP error` / kartica nije nađena** → HIP SDK 6.1+ instaliran? PATH? Najnoviji Adrenalin?
- **Nedostaje VRAM na Flux-u** → koristi GGUF Q5/Q8 kvant + `--lowvram` ili tiled VAE.
- **Out of memory na velikim rezolucijama** → generiši na 768-1024 pa upscale.
- **DirectML (Amuse) sporiji od ZLUDA** → očekivano; ZLUDA je brži put za ozbiljan rad.

---

## Checklist

- [ ] Instaliran Git, Python 3.11, HIP SDK 6.1+, najnoviji drajveri
- [ ] ComfyUI-ZLUDA radi (`start.bat` → localhost:8188)
- [ ] Skinut bar jedan checkpoint (SDXL ili Flux schnell) + ComfyUI-Manager + IPAdapter + ControlNet + rembg
- [ ] Napisan Style Bible (perspektiva, stil, paleta, rezolucija)
- [ ] Urađen test pixel-vs-cartoon i **odlučen stil**
- [ ] Napravljene 2-3 hero reference u `art-source/style-refs/`
- [ ] Sačuvan ComfyUI workflow `.json` kao template
- [ ] Probni asset prošao ceo lanac: generisanje → rembg → Unity import

---

## Dodatak A — Amuse AI: detaljne instrukcije za generisanje

Amuse je najlakši način da odmah generišeš art na Radeon kartici (radi offline, optimizovan za AMD). Idealan za testiranje stila i pravljenje sprajtova. Tačni nazivi dugmadi se malo razlikuju po verziji — opisujem po funkciji.

### A.1 Instalacija i prvo pokretanje
1. Skini i instaliraj sa https://www.amuse-ai.com/ (Windows, AMD).
2. Pri prvom pokretanju ponudiće skidanje modela — skini bar jedan **SDXL** model (bolji kvalitet od SD 1.5). Sačekaj download.
3. Ako postoji izbor **EZ / Advanced (Pro)** mod — prebaci na **Advanced/Pro** da dobiješ kontrolu nad seed-om, koracima i image-to-image.

### A.2 Podešavanja koja su bitna za sprajtove
| Parametar | Vrednost | Zašto |
|-----------|----------|-------|
| Model | SDXL | kvalitetniji sprajt |
| Rezolucija | 1024×1024 | SDXL native; kasnije skaliraš dole |
| Steps | 25-30 | dovoljno za čist rezultat |
| Guidance / CFG | 5-7 | prati prompt bez "prepečenosti" |
| Sampler | DPM++ 2M (Karras) ili Euler a | stabilno |
| Batch | 4 | generiši 4, biraš najbolji |
| **Seed** | **fiksiraj kad nađeš dobar** | ključ doslednosti |

### A.3 Format prompta (top-down 2D)
Drži **isti "stilski rep"** u svakom promptu, menjaj samo subjekt.

**Pozitivni prompt (cartoon primer):**
```
top-down view, 2D game sprite, <SUBJEKT>, centered, cartoon style, clean cel shading,
vibrant colors, soft top light, plain flat light-grey background, game asset, high detail
```
**Negativni prompt:**
```
isometric, perspective, 3d render, photorealistic, photo, multiple objects, text, watermark,
busy background, ground shadow, blurry, low quality, cropped, jpeg artifacts
```
> Bitno: traži **ravnu jednobojnu pozadinu** (light-grey/white) da bi kasnije čisto skinuo pozadinu sa `rembg`. Amuse ne pravi pouzdano transparentan PNG.

### A.4 Subjekti za ovu igru (zameni `<SUBJEKT>`)

> **Tematika:** slovenska/srpska mitologija. **Toweri = bogovi/dobri duhovi**, **neprijatelji = zla mitska bića/čudovišta.** Subjekti su izvedeni iz `slovenska_bica_prosireno.csv`.

**Toweri (bogovi / zaštitnički duhovi — tier 1):**
- Svarog (vatra): `Svarog the Slavic god of fire and celestial blacksmith, glowing forge hammer, anvil base, golden-orange fire aura, sun disk halo`
- Perun (grom): `Perun the Slavic thunder god, raised axe and lightning bolt, storm clouds, crackling blue electric arcs, oak and eagle motifs`
- Dažbog (sunce): `Dazhbog the Slavic sun god, radiant golden solar crown, warm light beams, shining bronze armor`
- Veles (magija/zemlja): `Veles the Slavic god of earth and magic, horned shaman figure, serpent coils, green underworld glow, druidic staff`
- Vila (priroda/support): `a graceful Slavic forest fairy Vila, flowing white gown, glowing wings, wind and leaves, soft nature aura`
- Domovoj (zaštita): `Domovoj a small bearded Slavic house spirit guardian, cozy earthy tones, protective glowing rune circle`
- Baba Jaga (prizivač): `Baba Yaga the Slavic witch, hunched figure with staff, glowing cauldron, arcane purple smoke, hut-on-chicken-legs motif`

**Neprijatelji (zla bića / čudovišta):**
- Drekavac: `Drekavac a creepy nocturnal Slavic demon, gaunt screaming creature, shadowy body, glowing eyes`
- Bauk: `Bauk a lurking dark Slavic monster, hulking shadow beast hiding in darkness`
- Karakondžula: `Karakondzula a feral Slavic night demon, clawed leaping assassin, dark fur, sharp fangs`
- Aždaja (boss): `Azdaja a massive multi-headed Slavic dragon, fire breath, scaled hulking body, menacing`
- Vampir: `a Slavic vampire undead, pale gaunt nobleman, blood-red eyes, dark cloak`
- Psoglav: `Psoglav a dog-headed Slavic giant ogre, brutish armored body, heavy build`
- Čuma (kuga): `Cuma the Slavic plague demon, hooded skeletal figure, sickly green poison cloud`

### A.5 Doslednost i tier-ovi (image-to-image u Amuse)
1. Generiši tornjeve dok ne dobiješ **jedan koji ti je "to"** → to je tvoj **hero** stil. Zapamti njegov **seed**.
2. Za ostale tornjeve koristi **isti seed + isti stilski rep**, menjaj samo subjekt — izlaze u istom maniru.
3. **Tier 2/3:** prebaci na **Image-to-Image**, ubaci tier-1 sliku, postavi **denoise/strength ~0.45-0.6**, i u prompt dodaj:
   - tier 2: `upgraded, extra barrel, reinforced armor`
   - tier 3: `heavily upgraded, triple barrel, glowing energy core, ornate gold trim`
4. Ako Amuse ima **AI Filter / Style** opciju — možeš provući SVE postojeće slike kroz isti filter da ih "poravnaš" na jedan look.

### A.6 Posle Amuse-a
1. **Skini pozadinu:**
   ```powershell
   pip install rembg[gpu]
   rembg i ulaz.png izlaz.png
   ```
2. (Pixel stil) spusti rezoluciju + ograniči paletu u Aseprite/Photopea.
3. Ubaci u Unity (vidi Fazu 4): Sprite, PPU, Point filter za pixel / Bilinear za cartoon.

### A.7 Brzi tok rada (sažeto)
```
Amuse (Advanced) → SDXL, 1024, 28 steps, CFG 6, batch 4
   → fiksiraj seed na hero stilu
   → generiši svaki toranj/neprijatelja (isti rep, menjaj subjekt)
   → Image-to-Image (denoise ~0.5) za tier 2/3
   → rembg (transparentno) → Unity import → zameni u prefab-ima
```

---

## Dodatak B — Konkretni promptovi (slovenska mitologija)

Setup: **SDXL + LoRA `Hyper-SDXL-8step-cfg`**, 16-bit, **768×768**, **8 steps**, **guidance 5**, **LoRA 100%**.

> 🎯 **ODLUČEN STIL: Blasphemous** — mračni gotički sakralni **HD pixel art** (detaljni pikseli, NE krupni retro), zagasita paleta sa zlatnim i krvavo-crvenim akcentima, dramatično chiaroscuro svetlo, makabrično/melanholično, ornamentika u duhu pravoslavne/slovenske ikonografije. Idealno za slovensku mitologiju.
>
> ⚠️ **Stil dolazi iz CHECKPOINT/LoRA, ne iz prompta.** Realistični SDXL daje "3D statuu" — neupotrebljivo. Za Blasphemous look:
> 1. Skini **pixel-art SDXL LoRA** (npr. **Pixel Art XL** by nerijs, civitai.com) i koristi je UZ Hyper LoRA (ili umesto). Po želji dodaj dark-fantasy/gothic checkpoint.
> 2. Generiši na **1024×1024** (više detalja), pa u **Aseprite/Photopea** spusti rezoluciju i ograniči paletu → "pravi" pikseli (Faza 3).
> 3. Ako ti čist pixel bude previše mučan, alternativa je **ne-pixel painterly gotik** (isti mračni vibe, bez piksela) — takođe validan Blasphemous-ish look.
>
> **Drži OVAJ stil za sve assete** (bogove-tornjeve i čudovišta).

**Stilski rep** (lepi se na KRAJ svakog pozitivnog prompta — drži doslednost):
```
dark gothic 2D pixel art, Blasphemous game art style, highly detailed pixel sprite, hand-crafted sprite,
somber muted palette with gold and deep crimson accents, dramatic chiaroscuro lighting, ornate religious iconography,
macabre melancholic atmosphere, full body, single character, isolated, centered, plain dark neutral background, no frame
```

> **VAŽNO — dva različita asseta po bogu:**
> 1. **Shop / card art** — portret bога, lik gleda u kameru, može imati ram i pozadinu. Koristi se u prodavnici/UI.
> 2. **Map asset (toranj)** — pogled **odozgo (top-down 3/4)**, **struktura/oltar/idol** (NE pozirajući lik), bez rama, izolovan objekat na ravnoj pozadini, kompaktna baza. Ovo se spušta na pločicu mape.
>
> Greška koju lako napraviš: "majestic deity standing" → model napravi portret-karticu, neupotrebljivo za mapu. Za mapu traži **shrine / altar / idol / forge structure** + **top-down 3/4 view**.

### Svarog — SHOP / card art (portret)

**Pozitivan:**
```
Svarog the Slavic god of fire and the celestial blacksmith, majestic bearded deity portrait,
holding a glowing forge hammer, molten metal and sparks, golden-orange fire aura,
sun disk halo behind head, ornate bronze and gold armor with Slavic pagan ornaments, embers and flames,
fantasy card illustration, vibrant warm colors, high detail, sharp focus
```
**Negativan:**
```
3d render, photorealistic, photo, multiple characters, deformed hands, extra fingers, extra limbs,
mutated, ugly, blurry, low quality, jpeg artifacts, text, watermark, modern clothing, gun, firearm
```

### Svarog — MAP ASSET (lik-toranj na mapi) ⭐

> Cilj: **bog kao jedinica/toranj**. Cela figura na **maloj okrugloj kamenoj bazi**, izolovana, BEZ arhitekture/luka/rama i BEZ scene iza.
>
> ⚠️ **Glavni problem ovog stila:** gotički/sakralni model PO PRIRODI dodaje luk/arkadu/ukrasni ram (pravi "ikonu/oltarsku sliku"). Frontalna poza lika je OK — **ram + pozadina su ono što kvari map asset.** Dva rešenja:
> - **A) Iseci lika (najbrže):** nateraj ravnu jednobojnu pozadinu (prompt ispod), pa `rembg` → lik na transparentnom. To je toranj; ram/scenu baciš.
> - **B) ControlNet (pouzdano):** pređi na **ComfyUI-ZLUDA + ControlNet** (Opcija A) i zadaj siluetu/pozu — model mora da je poštuje, bez luka. Amuse je preslab za ovu kontrolu.

> ✅ **ZAKLJUČAN HERO RECEPT (slika `sbmp0wd2`, najbolji rezultat).** Ne preteruj sa doteranjem — duži prompt + CFG 5 + dugačak negativ je empirijski pobedio "kraće/težine/CFG gore". Egzaktni parametri:
>
> | Parametar | Vrednost |
> |---|---|
> | Seed | **611421324** |
> | Guidance / CFG | **5** |
> | Steps | **8** |
> | Sampler | **Euler Ancestral (Euler a)** |
> | Veličina | 768×768 |
> | LoRA | Hyper-SDXL-8steps-CFG @ 1.0 |
>
> Za varijacije/druge bogove: drži isti negativ + parametre, menjaj seed (ili zadrži za isti lik) i samo subjekt u pozitivu. Halucinacije koje ostanu su uvek OKO lika → **iseci lika** (rembg/Photopea).

**Pozitivan (HERO — proveren):**
```
Svarog the Slavic god of fire, solemn bearded warrior deity, ornate gilded gothic armor,
holding a glowing forge hammer, dim molten glow and drifting embers, dark golden accents,
standing alone on a small round stone base,
dark gothic 2D pixel art, Blasphemous game art style, highly detailed pixel sprite, hand-crafted sprite,
somber muted palette with gold and deep crimson accents, dramatic side lighting,
full body, single character, isolated cutout, standalone game sprite,
plain solid light-grey background, empty background, nothing behind the character
```
**Negativan (HERO — proveren):**
```
cathedral, archway, arch, alcove, niche, stone wall, room interior, throne, altar, altarpiece, stained glass, pillars,
symmetrical frame, ornate border, decorative frame, vignette, card frame, trading card, background scenery, landscape,
flat vector, cartoon, cute, chibi, bright cheerful colors, pastel, anime,
3d render, 3d model, statue, figurine, sculpture, clay, photorealistic, photo, smooth gradient, antialiased, modern,
multiple characters, asset sheet, sprite sheet, collage, grid, scattered props,
portrait, bust, close-up, face only, text, watermark, signature,
blurry, low quality, cropped, out of frame, jpeg artifacts, deformed hands, extra fingers, extra limbs, mutated, ugly
```
> **Lekcija (zapamti):** ne juri "savršen" prompt beskonačno. Kad nešto radi → zaključaj seed+parametre i samo iseci lika. Dodatno doterivanje češće pogorša nego popravi.

---

### Animacije — svi uglovi (turnaround + stanja)

> ⚠️ **Doslednost:** text-to-image u Amuse-u NEĆE zadržati identičan lik kroz uglove (oklop/lice odluta). Prompt-uglovi daju tačnu **orijentaciju**, ali za frejm-na-frejm doslednu animaciju koristi jedno od:
> - **img2img** od jedne "hero" baze, denoise **0.3–0.4**, + promeni samo `<VIEW>` → zadržava identitet, menja ugao.
> - **ControlNet OpenPose** (ComfyUI): zadaš kostur poze po frejmu, lik ostaje isti. Najpouzdanije za prave animacije.
> - Fiksiran seed smanjuje drift, ali ne garantuje.

**Template** (hero prompt + menjaš samo `<VIEW>`; negativ i parametri = HERO recept gore):
```
Svarog the Slavic god of fire, solemn bearded warrior deity, ornate gilded gothic armor,
holding a glowing forge hammer, dim molten glow and drifting embers, dark golden accents,
standing alone on a small round stone base, <VIEW>,
dark gothic 2D pixel art, Blasphemous game art style, highly detailed pixel sprite, hand-crafted sprite,
somber muted palette with gold and deep crimson accents, dramatic side lighting,
full body, single character, isolated cutout, standalone game sprite,
plain solid light-grey background, empty background, nothing behind the character
```

**8 uglova (turnaround za kretanje):** zameni `<VIEW>`:
| Smer (top-down) | `<VIEW>` |
|---|---|
| Dole / ka kameri (S) | `front view, character facing forward toward the viewer` |
| Gore / od kamere (N) | `back view, character seen from behind, facing away` |
| Desno (E) | `right side profile view, character facing right` |
| Levo (W) | `left side profile view, character facing left` |
| Dole-desno (SE) | `3/4 front view, character facing forward-right` |
| Dole-levo (SW) | `3/4 front view, character facing forward-left` |
| Gore-desno (NE) | `3/4 back view, character facing away to the right` |
| Gore-levo (NW) | `3/4 back view, character facing away to the left` |

> **Trik:** generiši samo 4 smera (S, E, N + SE) a **levu stranu dobiješ horizontalnim flip-om desne** (W = flip E, SW = flip SE) — pola posla, savršena simetrija. Standardno za 2D igre.

**Stanja animacije (dodaj na kraj `<VIEW>` ili zameni pozu):**
| Stanje | dodatak u prompt |
|---|---|
| Idle | `idle standing pose` |
| Attack | `dynamic attack pose, swinging the hammer overhead` |
| Hit / pogođen | `recoiling hurt pose, flinching` |
| Death | `falling defeated pose, collapsing` |

> Za **toranj** verovatno ti NE trebaju sva 4 kretanja — toranj često ima samo `idle` + `attack` (i eventualno rotira gornji deo ka meti). Pun 8-smerni turnaround + walk ciklus treba pre svega **čudovištima (neprijateljima)** koja hodaju po stazi.

**Ako i dalje dobijaš pogrešno:**
- **Izlazi realistično / "statua" umesto piksela** → nije uključen pixel-art LoRA. Stil NE dolazi iz prompta — dodaj **Pixel Art XL** LoRA (i/ili dark-fantasy checkpoint), pa downscale u Aseprite.
- **Previše svetlo/veselo (ne-Blasphemous)** → pojačaj `dark, somber, grim, low-key lighting` i drži `bright cheerful colors, pastel, cartoon` u negativu.
- **Asset sheet / više objekata** → pojačaj `single character, one figure` na početak pozitivnog; `asset sheet, sprite sheet, collage, grid` su već u negativu.
- **Prazna zgrada bez lika** → izbaci reči `building/shrine/altar`; naglasi `full body character ... standing on pedestal`.
- **I dalje portret/bust** → dodaj `full body, head to toe` u pozitivan, drži `portrait, bust, close-up` u negativu.
- **Lik predaleko/sićušan** → dodaj `large centered figure filling the frame`.

> Alternativa (ako ti se sviđa **čista građevina-toranj**, npr. ona okrugla kovačnica): to je takođe validan TD stil — samo budi dosledan, ili lik ili građevina za SVE bogove, ne mešaj.

### Saveti za ovaj setup
- **Guidance/CFG:** drži **5** (tako je nastao hero `sbmp0wd2`). Empirijski je 5 + dugačak negativ bilo bolje od dizanja CFG-a. Halucinacije se NE rešavaju CFG-om nego se **iseca lik** posle.
- Sampler: **DPM++ SDE** ili **Euler a** (Hyper LoRA ih voli).
- **Fiksiraj seed** kad dobiješ Svaroga koji ti je "to" → to je hero referenca za ostale bogove (isti rep + IPAdapter, vidi Fazu 2).
- Batch **4–6**, biraš najbolji, pa `rembg` za transparentnu pozadinu.
- Za ostale bogove/čudovišta: uzmi subjekt iz **A.4** + zalepi stilski rep gore.

---

## Resursi

- ComfyUI-ZLUDA (AMD/Windows): https://github.com/patientx/ComfyUI-Zluda
- ComfyUI-Manager: https://github.com/ltdrdata/ComfyUI-Manager
- IPAdapter plus: https://github.com/cubiq/ComfyUI_IPAdapter_plus
- Amuse AI (AMD): https://www.amuse-ai.com/
- AMD HIP SDK: https://www.amd.com/en/developer/resources/rocm-hub/hip-sdk.html
- Modeli/LoRA: https://civitai.com  ·  GGUF Flux: https://huggingface.co/city96
- rembg: https://github.com/danielgatis/rembg
- Cloud GPU za LoRA: https://runpod.io · https://vast.ai
