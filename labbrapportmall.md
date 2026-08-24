# Labbrapport: praktisk laboration

*Kunskapskontroll 2, IT-säkerhet för utvecklare. Fyll i mallen och lämna in som PDF tillsammans med länken till ditt repo. Riktlängd två till tre sidor.*

**Namn:** Dalmar Abdi
**Datum:** 2026-08-24
**Repo (länk till din fork):** [Klistra in länk till din GitHub-fork här]
**Applikation som analyserades:** SakerLabb.Web (ASP.NET Core / Blazor)

---

## 1. Kort om applikationen och analysen

Applikationen *SakerLabb* är en webbapplikation utvecklad i ASP.NET Core och Blazor med funktionalitet för bland annat inloggning/autentisering, ärendehantering, uppladdning/läsning av bilagor samt diagnostikvyer. 

Säkerhetsanalysen genomfördes i två steg:
1. **Statisk kodanalys (SAST):** Genomfördes med GitHub CodeQL (default setup för C#) mot repots källkod för att identifiera sårbarheter i mönster, bristfällig indatavalidering samt osäkra inställningar.
2. **Dynamisk analys (DAST):** Genomfördes med OWASP ZAP (både passiv och aktiv skanning) lokalt mot applikationens instans på `http://localhost:5080` för att analysera HTTP-responser, exponerade headers och felkonfigurationer i runtime.

---

## 2. Fem fynd

| Nr | Källa (CodeQL/ZAP) | Regel-id eller alert | Allvarlighet (+ confidence för ZAP) | Fil och rad eller URL | Verkligt eller falskt positivt | Motivering (2–4 meningar) |
|:---|:---|:---|:---|:---|:---|:---|
| 1 | CodeQL | `cs/path-injection` (Uncontrolled data used in path expression #36) | High | `SakerLabb.Web/Services/FileService.cs:19` | Verkligt positivt | Metoden tar emot opålitlig indata (`name`) och slår samman den med `_root` via `Path.Combine`. I .NET förhindrar inte `Path.Combine` användning av relativa sökvägar (`../`), vilket öppnar för Path Traversal (CWE-22). En angripare kan utnyttja detta för att läsa godtyckliga och känsliga filer på servern utanför den avsedda mappen. |
| 2 | CodeQL | `cs/web/cookie-httponly-not-set` (Cookie 'HttpOnly' attribute is not set to true #33) | Medium | `SakerLabb.Web/Services/AuthService.cs:25` (rad 27) | Verkligt positivt | Autentiseringskakan sätts explicit med `HttpOnly = false`. Detta medför att sessionskakan kan läsas ut direkt av JavaScript på klientsidan via `document.cookie`. Vid en potentiell XSS-sårbarhet kan en angripare därmed omedelbart stjäla användarens sessionsuppgifter (CWE-1004). |
| 3 | CodeQL | `cs/web/cookie-secure-not-set` (Cookie 'Secure' attribute is not set to true #32) | Medium | `SakerLabb.Web/Services/AuthService.cs:25` (rad 28) | Verkligt positivt | Cookien konfigureras med `Secure = false` och `SameSite = SameSiteMode.None`. Detta gör att webbläsaren tillåts sända autentiseringskakan i klartext över okrypterad HTTP. En angripare på samma nätverk kan genom en Man-in-the-Middle-attack (MitM) avlyssna och kapa användarsessionen (CWE-319, CWE-614). |
| 4 | ZAP | `Cross-Domain Misconfiguration` (Alert Ref: 10098) | Medium (Confidence: Medium) | `http://localhost:5080/account/login` | Verkligt positivt | Responsen returnerar headern `Access-Control-Allow-Origin: *` på autentiseringsrelaterade anrop. Denna alltför tillåtande CORS-policy innebär att externa domäner kan utföra cross-domain-anrop och potentiellt läsa känslig data från applikationens oautentiserade endpoints (CWE-264). |
| 5 | ZAP | `Content Security Policy (CSP) Header Not Set` (Alert Ref: 10038-1) | Medium (Confidence: High) | `http://localhost:5080/_framework/blazor.web.js` | Verkligt positivt | Applikationen saknar `Content-Security-Policy`-headern i sina HTTP-svar. Detta innebär att applikationen saknar ett centralt skyddslager (defense-in-depth) som instruerar webbläsaren att blockera skadliga externa skript och inline-exekveringar vid eventuella XSS-angrepp (CWE-693). |

### Bevis (skärmbilder eller utdrag), numrerade efter fyndet ovan:

* **Bevis Fynd 1:** CodeQL-skanning Alert #36 (`Skärmbild 2026-08-24 091400.png` och `091426.png`) – Visar `Uncontrolled data used in path expression` på rad 19 i `FileService.cs`.
* **Bevis Fynd 2:** CodeQL-skanning Alert #33 (`Skärmbild 2026-08-24 090653.png` och `090726.png`) – Visar `Cookie 'HttpOnly' attribute is not set to true` på rad 25/27 i `AuthService.cs`.
* **Bevis Fynd 3:** CodeQL-skanning Alert #32 (`Skärmbild 2026-08-24 090837.png` och `090904.png`) – Visar `Cookie 'Secure' attribute is not set to true` på rad 25/28 i `AuthService.cs`.
* **Bevis Fynd 4:** OWASP ZAP Alert (`Skärmbild 2026-08-24 101726.png` och `101753.png`) – Visar `Cross-Domain Misconfiguration` och responsheader `Access-Control-Allow-Origin: *` på `/account/login`.
* **Bevis Fynd 5:** OWASP ZAP Alert (`Skärmbild 2026-08-24 101847.png`) – Visar `Content Security Policy (CSP) Header Not Set` på `blazor.web.js`.

---

## 3. Prioritering

### Rangordning av fynden:
1. **Fynd 1: Path Injection / Directory Traversal (`FileService.cs`)**
2. **Fynd 2: Cookie 'HttpOnly' saknas (`AuthService.cs`)**
3. **Fynd 3: Cookie 'Secure' saknas (`AuthService.cs`)**
4. **Fynd 4: Cross-Domain Misconfiguration (CORS Wildcard)**
5. **Fynd 5: Content Security Policy (CSP) Header Not Set**

### Motivering till ordningen:
* **Allvarlighetsgrad och utnyttjbarhet:**  
  Jag prioriterar och åtgärdar **Fynd 1 (`cs/path-injection`) först**. Detta är rapportens enda fynd med allvarlighetsgrad **High**. Sårbarheten har hög utnyttjbarhet eftersom en angripare direkt kan manipulera filnamnsparametern med t.ex. `../../appsettings.json` eller systemfiler för att läsa ut känslig konfigurationsdata och applikationshemligheter utan att behöva kringgå ytterligare skydd (direkt påverkan på konfidentialitet).
* **Exponering och autentiseringsskydd:**  
  **Fynd 2 och Fynd 3** prioriteras som tvåa och trea då de direkt berör applikationens sessionshantering. Att sätta `HttpOnly = true` förhindrar att stöld av sessionskakor kan ske via klientsidans JavaScript vid XSS, och `Secure = true` säkerställer att autentiseringstokens aldrig exponeras över okrypterade nätverkskanaler.
* **Sekundära försvarslager:**  
  **Fynd 4 (CORS)** och **Fynd 5 (CSP)** rankas sist. Wildcard-CORS är en risk men begränsas av webbläsarens Same-Origin Policy gällande autentiserade anrop, och CSP utgör ett kompletterande djupförsvar (defense-in-depth) snarare än en direkt exploaterbar sårbarhet i sig.

---

## 4. Åtgärder (minst tre)

### Åtgärd 1

```
Fynd:        Fynd 1 – cs/path-injection (CodeQL #36)
Plats:       SakerLabb.Web/Services/FileService.cs:19
Bevis före:  Skärmbild 2026-08-24 091400.png (Visar "Uncontrolled data used in path expression")
Bedömning:   Verkligt positivt. Saknade validering och skydd mot sökvägsmanipulation (Path Traversal).
Åtgärd:      Använde Path.GetFileName() för att strippa bort otillåtna katalogseparatorer samt Path.GetFullPath() för att verifiera att den slutgiltiga sökvägen strikt börjar inom _root-katalogen.
Commit-hash: [Klistra in din commit-hash här, t.ex. 4a1b2c3]
Bevis efter: Ny körning av CodeQL på GitHub Actions visar att Alert #36 automatiskt har stängts och markerats som "Fixed".
```

### Åtgärd 2,3

```
Fynd:        Fynd 2 & Fynd 3 – cs/web/cookie-httponly-not-set (#33) och cs/web/cookie-secure-not-set (#32)
Plats:       SakerLabb.Web/Services/AuthService.cs:25-31
Bevis före:  Skärmbild 2026-08-24 090653.png och 090837.png (Visar HttpOnly = false och Secure = false)
Bedömning:   Verkligt positivt. Autentiseringskakan saknade skydd mot XSS-exfiltrering och avlyssning över HTTP.
Åtgärd:      Uppdaterade CookieOptions så att HttpOnly sattes till true, Secure till true och SameSite ändrades till SameSiteMode.Lax.
Commit-hash: [Klistra in din commit-hash här, t.ex. 8f2d9e1]
Bevis efter: Ny körning av CodeQL på GitHub Actions visar att både Alert #32 och Alert #33 är markerade som "Fixed".
```
### Åtgärd 3

```

```

---

## 5. Eventuella bortval

Om du valt att inte åtgärda ett fynd, skriv ned tre saker per bortval: risken, motivet och den kompenserande kontrollen. Sätt gärna ett datum för omprövning.

*Skriv här, eller skriv "inga bortval".*
