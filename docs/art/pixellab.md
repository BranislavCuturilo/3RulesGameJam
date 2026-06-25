# PixelLab — cenovnik, korišćenje i limiti (MCP)

PixelLab je AI alat za game pixel-art (likovi, rotacije, animacije, tilesetovi).
Izabran kao glavni alat za 3TD likove jer rešava ono što Amuse ne može:
čist lik na transparentnoj pozadini, svih 8 pravaca odjednom, doslednu animaciju.

> Cene su približne — zvanična pricing stranica se ne učitava uvek čisto.
> Proveri na sajtu pre kupovine. Stanje: jun 2026.

---

## 💰 Cenovnik (pretplate)

| Tier | Cena/mes | Generacija/mes | Za šta |
|---|---|---|---|
| **Free trial** | $0 (bez kartice) | **40** (jednokratno, NE obnavlja se) | proba |
| **Tier 1 – Pixel Apprentice** | ~$12 (lojalnost do $9) | ~1000 | do 320×320, animacije, mape |
| **Tier 2** | ~$25 (lojalnost do $22) | ~3000 | viši prioritet |
| **Tier 3 – Pixel Architect** | ~$50 | ~6000 | 20 paralelnih jobova, tim |

### Alternativa: API pay-per-call
Umesto pretplate, plaćaš po generaciji:
- Mali sprajt 64×64: ~$0.006–0.007
- **Character v3 64×64: ~$0.041** (ovako je napravljen Svarog)
- Pro alati (character/animation): ~$0.185 po pozivu
- Transparentna pozadina: +5–10%
- Cena raste sa veličinom slike i kvalitetom modela (nelinearno).

---

## 🔌 Kako se naplaćuje preko MCP-a

- MCP troši **isti „generation" pool** kao pretplata (sa credit fallback-om).
- Ne plaća se po pozivu zasebno — skida iz mesečne kvote.
- `get_balance` uvek pokazuje stanje (remaining / used / total).

---

## 📊 Tačni troškovi po operaciji (izmereno)

| Operacija | Cena (generacija) |
|---|---|
| `create_character` **standard** | **1** |
| `create_character` **v3** (64px) | **2**  ← Svarog |
| `create_character` **pro** | 20–40 |
| `animate_character` template/v3 | **1 po pravcu** (8 pravaca = 8) |
| `animate_character` **pro** | 20–40 po pravcu |

---

## ⚠️ Ograničenja preko MCP

- **Async:** svaki job traje ~3–15 min, vraća ID, pa se proverava status (`get_character`).
- **„Heavy load" greške:** server zna da odbije pod opterećenjem → samo **retry**.
- **Paralelni jobovi:** ograničeni tierom (trial/Tier1 malo, Tier3 do 20).
- **Trial 40 gen je jednokratno** — ne obnavlja se mesečno.

---

## 🎯 Šta to znači za 3TD

**Trial (40 besplatnih) je DOVOLJAN za kompletnog Svaroga:**
- Svarog lik (v3): ~2 (potrošeno)
- Idle (8 pravaca): 8
- Napad (8 pravaca): 8
- **Ukupno ~18 od 40** → ostaje ~19 za eksperimente.

Pretplata (Tier 1, ~$12/mes, ~1000 gen) treba tek za **celu igru**
(svi bogovi + čudovišta × animacije = stotine/hiljade generacija).

---

## 🛠️ Kako se koristi (MCP je već povezan)

Server `pixellab` je dodat u lokalni `.claude.json` (HTTP, https://api.pixellab.ai/mcp).
Provera: `claude mcp list` → `pixellab ... ✔ Connected`.
Posle dodavanja MCP-a treba **restart sesije** da se alati učitaju.

Glavni alati (Claude ih poziva umesto tebe):
- `get_balance` — stanje kredita/generacija
- `create_character` — lik sa 8 pravaca (mode: standard/v3/pro, view: low top-down, size, itd.)
- `get_character` — status + download linkovi rotacija
- `animate_character` — animacija preko svih pravaca (template ili custom v3)
- `list_characters`, `delete_character`, tileset/object alati...

### Tipičan tok za jedan lik (toranj/čudovište)
1. `create_character` (v3, 64px, low top-down) → ~2 gen, ~10 min
2. `get_character` → proveri, skini 8 rotacija u `Assets/Art/...`
3. `animate_character` (template `breathing-idle`) → idle u 8 pravaca (8 gen)
4. `animate_character` (custom: „swinging a war hammer") → napad u 8 pravaca (8 gen)
5. Skini frejmove → Unity sprite sheet → Animation Clip

### Prvi lik (referenca)
Svarog: `character_id = 796eb7bf-58b8-4a53-8000-54cdbcffdf45`
8 rotacija skinuto u `Assets/Art/Towers/Svarog/svarog_<pravac>.png`.

---

## 💵 Procena troška za CELU igru (10 towera + 3 čudovišta)

> ✅ **Broj frejmova (8 ili 16) NE poskupljuje generisanje.** Naplaćuje se **po pravcu / po animaciji**, ne po frejmu. Jedna animacija u jednom pravcu = 1 generacija, bilo 4 ili 16 frejmova. Samo biraš `frame_count`.

### Pretpostavke
- Likovi: **64px, v3 mod** (kao Svarog), svih **8 pravaca**
- **Toweri (10):** idle + napad = 2 animacije
- **Čudovišta (3):** idle + hod + napad + smrt = 4 animacije (kreću se po stazi)
- Cena: `create v3` ≈ $0.041/lik; `animate v3` ≈ ~$0.02/pravac → ~$0.16 po animaciji (8 pravaca)

### Broj generacija
| Stavka | Računica | Generacija |
|---|---|---|
| Kreiranje likova | 13 × 2 | 26 |
| Tower animacije | 10 × 2 anim × 8 pravaca | 160 |
| Enemy animacije | 3 × 4 anim × 8 pravaca | 96 |
| **Ukupno** | | **~282** |

### Koliko para (API pay-per-call)
| Kvalitet | Bazno (bez re-roll) | Realno (re-roll ~1.7×) |
|---|---|---|
| **v3** (preporuka) | **~$6** | **~$10–15** |
| **pro** (najviši) | ~$50 | ~$70+ |

### 💡 Najpametnije: pretplata 1 mesec, NE pay-per-call
**Tier 1 (~$12/mes, lojalnost do $9) = ~1000 generacija.** Cela igra ~282 gen + re-rolls (~500) = i dalje upola kvote. Za **~$9–12** dobiješ svih 13 likova sa punim animacijama u 8 pravaca + prostor za eksperimente, pa otkažeš posle mesec dana. Bolje od pay-per-call (pokriva re-rollove, v3 kvalitet, jedna uplata).

**Pro mod (~$50) se ne preporučuje** — v3 je dao odličnog Svaroga; pro je ~5× skuplje za marginalno bolji rezultat.

**Zaključak:** ~**$9–12 (jedan mesec Tier 1)** za celu igru u v3 kvalitetu.

---

## ⚡ Paralelno generisanje, queue i brzina (po tieru)

### Queue? — async DA, ali ima TVRD limit paralelnih jobova (NEMA auto-queue preko toga)
MCP/API je **asinhroni** (svaki poziv odmah vrati job ID), ALI postoji **hard cap na broj ISTOVREMENIH background jobova**, i preko toga server **ODBIJA** zahtev (`429: Maximum N concurrent background jobs`). Ne stoji u redu — mora ponovo da se pošalje kad se mesto oslobodi.

**IZMERENO na Tier 1: max 8 paralelnih background jobova.**
- `create_character` v3 = **2 joba/lik** → stane **~4 lika odjednom**.
- `animate_character` (8 pravaca) = **do 8 jobova** → **1 animacija popuni ceo cap** (radi se jedna po jedna).

→ Posledica: ceo roster se NE može ispaliti odjednom na Tier 1. Mora **u talasima** — pošalješ ~4 lika, sačekaš da se završe, pa sledeća 4, itd. **Claude mora da bude upaljen da bi slao sledeće talase** (server ne nastavlja sam preko cap-a).

### Koliko ih radi ISTOVREMENO (concurrency) — zavisi od tiera
Queue postoji svuda, ali **brzina** (koliko jobova radi paralelno) raste sa tierom:

| Tier | Cena/mes | Paralelno (concurrency) | Brzina za ceo roster |
|---|---|---|---|
| **Trial / Tier 1** | $0 / ~$12 | **8 background jobova** (≈4 v3 lika ili 1 animacija) | u talasima, Claude šalje sledeći talas |
| **Tier 2** | ~$24 | **priority queue** (brže raspoređivanje) | osetno brže |
| **Tier 3 – Architect** | ~$50 | **do 20 paralelnih jobova** | **najbrže** — ceo roster za ~30–60 min |

### Šta da uzmeš za "što pre sve assete"
- **Ako ti je brzina presudna (ASAP):** **Tier 3** (~$50). 20 paralelnih → ceo roster (~50–60 jobova, ~280 gen) se izgenerише za desetak/dvadesetak minuta do sat. Pretplatiš 1 mesec, ispališ sve, otkažeš.
- **Ako možeš da sačekaš (jeftino):** **Tier 1** (~$9–12). Queue i dalje radi automatski — **ispališ svih 10+ zahteva i pustiš da se vrte** (može potrajati satima jer ide malo-po-malo), ne moraš da nadgledaš. Pokriva ~1000 gen.
- **Tier 2** (~$24) je sredina: priority queue, brže od Tier 1, jeftinije od Tier 3.

**Preporuka:** pošto ti treba ASAP **i** ceo roster → **Tier 3 jedan mesec, pa otkaži.** Najbrže, a ispadne jeftino jer ga držiš samo dok ne izgenerišeš sve. Ako nisi u žurbi, Tier 1 + batch queue radi isti posao, samo sporije.

> Napomena: i na trial-u možeš da queue-uješ više zahteva, ali nizak prioritet + „heavy load" odbijanja znače da je sporo i nepouzdano za veliki batch.

---

## Linkovi
- FAQ / pricing: https://www.pixellab.ai/docs/faq
- API pricing: https://www.pixellab.ai/pixellab-api
- MCP (GitHub): https://github.com/pixellab-code/pixellab-mcp
- Review: https://www.jonathanyu.xyz/2025/12/31/pixellab-review-the-best-ai-tool-for-2d-pixel-art-games/
