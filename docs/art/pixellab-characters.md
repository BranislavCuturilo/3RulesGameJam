# PixelLab — manifest likova (character_id + status)

Slike: `Assets/Art/Towers/<Ime>/` i `Assets/Art/Enemies/<Ime>/`. 8 pravaca.
Animacije u `<Ime>/animations/<naziv>/<pravac>/frame_NNN.png`.

> **ODLUKA (korisnik):** SVI napadi = **v3 custom** (thematski), NE `fireball` template (izgledao nezgrapno). Idle ostaje `breathing-idle` template. **Svarog preskočen** (zamah čekićem već dobar). Custom = 16 gen/napad.
> Stari `casting_a_fireball` folderi (Perun/Dažbog/Morana) — obrisati na kraju (zamenjeni custom-om).

## Toweri — status
| Ime | character_id | idle | napad (custom) | u Assets |
|---|---|---|---|---|
| Svarog | `796eb7bf-58b8-4a53-8000-54cdbcffdf45` | ✅ | ✅ hammer swing (zadržan) | ✅ |
| Perun | `b773294d-0505-44fb-b103-8442432ce010` | ✅ | ✅ lightning | ✅ |
| Dazbog | `ce433c66-562f-4a7c-a3be-d960e19b08fc` | ✅ | 🔄 solar beam | idle da, napad u toku |
| Morana | `c4bcc212-eeee-4443-8ed8-6591f465035a` | ✅ | ⏳ frost (treba redo, ima stari fireball) | idle da |
| Veles | `16f002e9-121a-433a-a57a-51ee31aeb3e9` | ✅ | ✅ poison cloud | ✅ 8/8 |
| Stribog | `e2318ce5-916c-47bc-bfc3-7f1135ccba5a` | ⏳ | ⏳ wind gust | ne |
| Jarilo | `3e905301-28bc-44ff-ae52-622bc1e26022` | ✅ | ✅ sickle slash (melee) | ✅ 8/8 |
| Mokos | `1b0fed4e-b1ab-450b-8c06-6eaf1c1be3cb` | ⏳ | ⏳ earth roots | ne |
| Svetovid | `951f0efd-ca96-43c5-ace4-bc6f4038c4e4` | ⏳ | ⏳ sword sweep (melee) | ne |
| Triglav | `7f601f58-bb35-4bd2-84a8-5b0f70425059` | ⏳ | ⏳ arcane burst | ne |

## Čudovišta — status (idle + walk + attack-custom + death)
| Ime | character_id | status |
|---|---|---|
| Drekavac | `11930099-260e-44aa-ae65-50599223e4fa` | rotacije ✅, animacije ⏳ (attack: claw lunge) |
| Psoglav | `42380397-197d-4f37-9a5d-2719b9e6c4ef` | rotacije ✅, animacije ⏳ (attack: overhead smash) |
| Azdaja (quadruped) | `303b7d89-32e4-49ed-b9bb-c1ffa56a5109` | proveriti da je kreiran; animacije ⏳ (attack: fire breath) |

## Custom napad opisi (action_description za animate_character mode=v3, directions=svih 8)
- Perun: raising the battle axe overhead and hurling a bolt of lightning forward
- Dazbog: raising the spear and unleashing a beam of radiant golden sunlight forward
- Morana: raising the staff and casting a freezing frost blast forward
- Veles: raising the staff and conjuring a cloud of toxic green poison
- Stribog: swinging the horn and blasting a gust of wind forward
- Jarilo: slashing forward quickly with the sickle
- Mokos: raising both hands to summon binding earth roots forward
- Svetovid: swinging the great sword forward in a wide arc
- Triglav: raising the staff and blasting a burst of purple arcane energy forward
- Drekavac: lunging forward with a claw swipe
- Psoglav: smashing down with a heavy overhead blow
- Azdaja: rearing up and breathing fire forward

## Tok rada
1. idle = `animate_character(template_animation_id="breathing-idle", animation_name="idle")` (8 jobova).
2. attack = `animate_character(mode="v3", action_description=..., animation_name="attack", directions=svih 8, frame_count=8)` (16 gen, 8 jobova).
3. Tier 1 cap = 8 jobova → JEDNA animacija odjednom; čekaj da se oslobode slotovi.
4. download: `GET https://api.pixellab.ai/mcp/characters/<id>/download` sa `Authorization: Bearer <token>` (423 dok nije gotovo) → unzip → robocopy u `Assets/Art/.../<Ime>`.
