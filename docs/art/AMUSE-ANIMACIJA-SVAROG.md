# Amuse — Svarog u svim pravcima (img2img workflow)

> ⚠️ **Iskreno:** Amuse nema sistem za doslednu animaciju lika. Svaki pravac/frejm se generiše zasebno → lik **drifta** (oklop/lice se menjaju). Za prave turnaround/animacije je **PixelLab** mnogo bolji (`create_character` daje svih 8 pravaca odjednom, `animate_character` prave frejmove). Koristi Amuse za ovo samo ako baš moraš. Dole je najbolji mogući Amuse pristup.

---

## Princip
Ne generišeš iz nule — uzmeš **jednu baznu sliku Svaroga** (npr. `Assets/Art/Towers/Svarog/svarog_south.png`) i kroz **Image→Image** je "okreneš" u drugi pravac, uz **nizak Strength** da zadržiš identitet.

---

## Podešavanja (tačno za tvoj UI)

| Polje | Vrednost |
|---|---|
| Mod (leva traka) | **Image → Image** |
| Model | StableDiffusionXL |
| LoRA | ✔ **Hyper-SDXL-8steps-CFG** |
| ✔ **Extract** (BiRefNet) | uključi → automatski skida pozadinu (čist sprite) |
| Upscale | opciono (AnimeSharpV2) |
| Steps | **8** |
| Guidance | **5** |
| **Strength (denoise)** | **0.35–0.45** (nisko = blizak originalu) |
| Seed | **fiksiraj** (klikni lock/strelicu, ne Random) |
| Scheduler | **Euler a** (EulerAncestral) |
| Resolution | 768×768 |

---

## Koraci — turnaround (8 pravaca) ⭐ PAMETAN PRISTUP

> Ključ: **koristi odgovarajući PixelLab sprajt kao img2img bazu za svaki pravac.** Imaš sve u `Assets/Art/Towers/Svarog/svarog_<pravac>.png`. Orijentacija je tako već tačna, a Strength 0.4 samo prefarba u Amuse stil → bez driftovanja poze. (Ako kreneš od JEDNE slike + `<VIEW>` reči, na 0.4 se lik ne okrene dovoljno.)

1. **Image → Image** mod. ✔ **Extract** UKLJUČI (transparentno, nema crne pozadine).
2. Podesi tabelu gore. **Strength 0.4**, **Seed fiksiran**.
3. Za svaki pravac: učitaj bazu `svarog_<pravac>.png` (npr. `svarog_east.png`).
4. **Prompt = hero prompt BEZ `<VIEW>`** (baza već daje ugao).
5. **Generate** → Extract vrati transparentno → sačuvaj kao `svarog_<pravac>_amuse.png`.
6. Ponovi za svih 8.

**Flip trik (ušteda):** generiši samo desnu stranu (S, E, N, SE, NE = 5 kom), pa W/SW/NW dobij **horizontalnim flip-om** u Aseprite/Photopea.

> Stari način (jedna baza + `<VIEW>` reč) radi samo na visokom Strength-u (0.5–0.6), ali tada identitet jako drifta. Zato baza-po-pravcu.

---

## Hero prompt (zalepi + dodaj `<VIEW>`)

**Pozitivan:**
```
Svarog the Slavic god of fire, solemn bearded warrior deity, ornate gilded gothic armor,
holding a glowing forge hammer, dark golden accents, <VIEW>,
dark gothic 2D pixel art, Blasphemous game art style, highly detailed pixel sprite,
somber muted palette with gold and deep crimson accents, full body, single character,
plain solid light-grey background, empty background
```
**Negativan:**
```
multiple characters, asset sheet, sprite sheet, grid, text, nameplate, frame, border, arch, pedestal,
scattered items, glowing particles, deformed hands, extra fingers, extra weapons, blurry, low quality,
3d render, photorealistic, photo, cartoon, cute, chibi
```

---

## Animacija idle + napad u svim pravcima (BESPLATNO, bez PixelLab-a)

> ⚠️ **Ne generišu se animacioni frejmovi u Amuse-u** — bili bi nedosledni i treperili. Umesto toga: animaciju praviš **u Unity-ju iz statičnih sprajtova** (8 pravaca koje već imaš). Besplatno, radi u svim pravcima. Tako se animira većina 2D tornjeva.

### Šta ti treba
- 8 statičnih sprajtova (`Assets/Art/Towers/Svarog/svarog_<pravac>.png`) — imaš.
- 2 Unity Animation clip-a (idle, napad) — animiraju **Transform**, ne sliku → isti clip radi za sve pravce.
- Logika: prema tome kuda toranj gleda, zameniš koji se sprite prikazuje (SpriteRenderer.sprite).

### Idle clip (loop ~1.2s)
1. Stavi sprite u scenu (SpriteRenderer). Window → Animation → Create clip `Svarog_Idle`.
2. Animiraj **Transform**:
   - Position Y: `0 → +0.03 → 0` (blago disanje)
   - Scale: `1.0 → 1.02 → 1.0`
3. (Opc) treptaj baklje: animiraj SpriteRenderer **Color** (blago ka žutom) ili zaseban "flame" sprite koji pulsira.
4. Loop = ON.

### Napad clip (~0.3s, NE loop)
1. Novi clip `Svarog_Attack`.
2. Brz udarac: Scale `1.0 → 1.1 → 1.0` + Position pomak napred + Rotation `0 → -8° → 0`.
3. Okini iz koda kad toranj puca (Animator trigger / `Play`).

### Svi pravci
- Idle i Napad clip su isti za sve — **samo menjaš sprite** prema pravcu (8 svojih PNG-ova).
- U Animator-u: blend po pravcu ILI prosto set `spriteRenderer.sprite = svarogDir[smer]` u kodu.

---

## (Opciono) Pravi zamah čekićem — RUČNO u Aseprite (NE img2img)

> ❌ **img2img za zamah NE radi.** Velika promena poze traži visok Strength → SDXL tada ignoriše baznu sliku, duplira lika i odluta (dobijaš dva lika / smeće). Dokazano. Velika promena poze i doslednost se isključuju u img2img.

✅ **Umesto toga — ručna izmena (~15 min, 100% dosledno):**
1. Otvori `svarog_south.png` u Aseprite (ili Photopea).
2. Selektuj **ruku + čekić**, rotiraj/pomeri gore → frejm `attack1` (zamah).
3. Frejm `attack2` (udar): čekić dole + dodaj par piksela vatre/iskri.
4. Animacija napada = `base → attack1 → attack2 → base`.

Zašto ručno: koristiš ISTI originalni lik, samo pomeraš čekić → savršena doslednost. AI ne može to da pobedi za sitne pomeraje na postojećem sprajtu.

> Idle uopšte ne treba frejmove — Unity transform (gore-dole + scale). Vidi sekciju "Animacija idle + napad" gore.

---

## ControlNet (preciznija poza, napredno)
1. ✔ **Control** → Control = **Canny**.
2. Učitaj referencu čije ivice definišu pozu/siluetu (npr. skica poze).
3. Amuse će ispoštovati siluetu, a prompt/stil popunjava ostalo. Najbliže "kontroli" u Amuse-u, ali treba ti pose-referenca po frejmu.

---

## Posle generisanja
1. Ako nisi koristio Extract → `rembg i ulaz.png izlaz.png` (transparentno).
2. **Downscale u Aseprite** na ciljnu rezoluciju + ograniči paletu → čist pixel + briše sitne halucinacije.
3. Sastavi frejmove u sprite sheet → Unity Animation Clip (vidi `AI-ART-PIPELINE.md` Faza 4).

---

## ⚠️ NE koristi "Video" tab za ovo
Video tab daje kratak **video klip** (zamućeni inter-frejmovi), ne čiste game sprajtove. Beskorisno za pixel-art animaciju.
