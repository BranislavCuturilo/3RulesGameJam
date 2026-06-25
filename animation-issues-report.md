# Analiza animacija — problemi za regeneraciju

> Pregled generisanih karaktera i animacija. Lista koje animacije treba popraviti/regenerisati.
> Datum: 2026-06-25

---

## ❌ PERUN
- **idle — North East**: loš, dizajn nije ujednačen sa svim ostalim animacijama. Regenerisati da bude konzistentan sa ostalim pravcima/animacijama.

## ❌ DAZBOG
- **idle — South West**: oreol/frizura se pojavljuje i nestaje (treperi). Treba stabilan oreol/frizura kroz sve frame-ove.
- **napad kopljem (sve pravce)**: animacije nisu ujednačene — u svakom pravcu je drugačiji napad. Treba isti tip napada kopljem u svim pravcima.

## ❌ MORANA
- **napadi kosom — neujednačeni**: nije svaka animacija ista u svakom pravcu.
  - **West napad**: udara iza sebe (pogrešan smer).
  - **North West napad**: udara iza sebe (pogrešan smer).
  - **North East napad**: udara rukom umesto kosom, i onda kosa zatreperi (bug).
  - Treba: konzistentan zamah kosom u ispravnom smeru za svaki pravac.

## ❌ VELES
- **idle — West**: 1-frame bug — drži staff, pa staff nestane i pojavi se zmija ispod, animacija treperi. Inače odličan. Samo popraviti taj jedan frame.

## ✅ SVAROG
- Odličan, bez greške. Ne dirati.

## ❌ JARILO
- **idle/animacije — North West (obe)**: izbagovane. Regenerisati.
- **attack — West**: izbagovana. Regenerisati.

## ❌ STRIBOG — bow attack (pogrešan smer gađanja po pravcima)
- **South**: gađa ka West umesto ka South, i NEMA strelu. Regenerisati — mora gađati na South sa vidljivom strelom.
- **North**: gađa ka East umesto ka North, i izbagovan je. Regenerisati.
- **North East**: gađa ka East umesto ka NE, i izbagovan je. Regenerisati.
- **South East**: gađa ka East — *ali je dobar* (prihvatljivo, blizu cilja).
- **South West**: gađa negde između SW i West — *ali je dobar* (prihvatljivo).
- **North West**: gleda ka NW ali gađa u West pravcu — *dobar* (prihvatljivo).
- **Napomena**: glavni problemi su S, N, NE (pogrešan smer + bug + nedostaje strela kod S).

---

## Rezime — šta regenerisati
| Karakter | Animacija | Pravac | Problem |
|----------|-----------|--------|---------|
| Perun    | idle      | NE     | dizajn neujednačen |
| Dazbog   | idle      | SW     | oreol/frizura treperi |
| Dazbog   | napad kopljem | svi pravci | napad različit po pravcu |
| Morana   | napad kosom | W   | udara iza sebe |
| Morana   | napad kosom | NW  | udara iza sebe |
| Morana   | napad kosom | NE  | udara rukom + kosa treperi |
| Veles    | idle      | W      | 1-frame bug (staff→zmija treperi) |
| Jarilo   | idle/anim | NW (obe) | izbagovane |
| Jarilo   | attack    | W      | izbagovana |
| Stribog  | bow attack | S     | gađa W umesto S + nema strelu |
| Stribog  | bow attack | N     | gađa E umesto N + izbagovan |
| Stribog  | bow attack | NE    | gađa E umesto NE + izbagovan |

## Bez izmena (OK)
- **Svarog** — sve animacije OK.
- **Veles** — sve osim West idle OK.
- **Stribog** — SE, SW, NW bow attack — prihvatljivi (gađaju malo ulevo ali OK). Ne dirati.

---

## ✅ STATUS POPRAVKI (2026-06-25)

Sve regenerisano preko PixelLab-a, novi frejmovi zamenjeni u `Assets\Art\Towers\<Lik>\animations\...`.

**REŠENO (animacijski bugovi):**
- ✅ **Veles** idle W — staff↔zmija treperenje popravljeno (template re-gen, sva 4 frejma stabilna).
- ✅ **Dazbog** idle SW — oreol više ne treperi (stabilan kroz frejmove).
- ✅ **Dazbog** napad kopljem — svih 8 smerova UJEDNAČENO: isti pokret "diže koplje uvis + blistavi zrak" (kao south).
- ✅ **Morana** napad kosom W/NW/NE — više ne udara iza sebe / rukom; zamah kosom napred u ispravnom smeru.
- ✅ **Stribog** bow attack S/N/NE — smer gađanja ispravljen (S gađa jug, NE dijagonalno gore-desno, N poboljšan). Strela sitna ali poza ispravna.
- ✅ **Jarilo** attack W — distorzija popravljena, čist zamah srpom napred-levo.

**KLJUČNA LEKCIJA:** v3 ne poštuje smer osim ako prompt eksplicitno kaže "toward the LEFT/west", "downward toward viewer", "diagonally upper-right" itd. Smer-specifični promptovi rešavaju wrong-aim bugove.

**Perun:**
- ✅ **Perun NE** — KORISNIK je sam regenerisao problematičnu idle animaciju. (Moj `create_character_state` pokušaj bio nepotreban, obrisan.)

**Jarilo (drugi prolaz — udarac išao iza modela):**
- ✅ **Jarilo W attack** — srp je išao iza leđa (lik okrenut leđima u W rotaciji). Regenerisan v3 sa eksplicitnim "srp ISPRED, ka cilju levo, NE iza leđa" promptom. Zamah se sad završava srpom ispred-levo ka cilju (zapadu) sa motion trailom.
- ✅ **Jarilo NW attack** — isto popravljeno; srp se zamahuje gore-levo ispred ka cilju (NW) sa motion trailom.
- Napomena: koren je W/NW rotacija koja prikazuje lik s leđa, pa v3 teško stavlja oružje "ispred". Eksplicitan anti-"iza leđa" prompt je dao prihvatljiv zamah ka cilju.

**Sve stavke iz izveštaja su obrađene.** Originalni PixelLab likovi netaknuti (regeneracije idu na clean/state varijante ili zamenjuju samo problematične smerove).
