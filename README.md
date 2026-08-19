# SakerLabb Support

Internt ärendehanteringssystem byggt i Blazor på .NET 10. Appen används som laborationsmiljö i kursen **IT-säkerhet för utvecklare** vid TUC Yrkeshögskola, i den praktiska laborationen (kunskapskontroll 2) där du kör statisk och dynamisk kodanalys, åtgärdar fynden i kod och verifierar att verktygen inte längre flaggar dem.

> **Läs det här först.** SakerLabb Support innehåller medvetna säkerhetsbrister. Den är byggd för att gå sönder. Kör den bara lokalt på din egen maskin, publicera den aldrig på en nåbar adress och lägg aldrig in riktiga personuppgifter eller riktiga hemligheter i den.

## Domän

Appen är ett litet supportsystem. Kunder och intern drift skickar in ärenden, agenter söker i listan, kommenterar och byter status. Det finns bilagor, en importfunktion för partnersystem och en enkel administrationsvy för användare och roller. Precis den sortens verksamhetsnära .NET-app du möter i arbetslivet, med precis den sortens brister som smyger sig in när säkerheten inte prioriteras.

## Kom igång

Du behöver **.NET SDK 10** och ett **GitHub-konto med pushrättigheter till din egen fork**.

```bash
# 1. Forka detta repo till ditt eget konto på GitHub (knappen Fork uppe till höger).
#    Du behöver en egen fork för att kunna pusha och för att kunna slå på code scanning.

# 2. Klona din fork
git clone https://github.com/<ditt-konto>/SakerLabb.git
cd SakerLabb

# 3. Bygg och starta
dotnet run --project SakerLabb.Web
```

Appen svarar på **http://localhost:5080**. Databasen är en lokal SQLite-fil (`sakerlabb.db`) som skapas och fylls med testdata första gången appen startar. Vill du börja om från rent bord tar du bort filen och startar appen igen.

### Testkonton

| Användarnamn | Lösenord    | Roll  |
|--------------|-------------|-------|
| `admin`      | `admin123`  | Admin |
| `mohamed`    | `Sommar2026!` | Agent |
| `peter`      | `Passw0rd`  | User  |

## Så här används appen i laborationen

1. **Forka** repot och slå på **code scanning** med default setup och språket **C#** (Settings → Advanced Security → Code scanning). Den första CodeQL-körningen är din statiska analys.
2. Starta appen lokalt och kör **OWASP ZAP** som proxy mot `http://localhost:5080`. Det är din dynamiska analys. Kör ZAP mot den här appen, inte mot Juice Shop.
3. Dokumentera dina fynd, prioritera dem, **åtgärda minst tre i kod** med en commit per åtgärd och **verifiera** varje åtgärd med en ny körning av samma verktyg.

Fullständiga krav och bedömning står i uppgiften i Learnpoint.

## Teknik

- Blazor Web App med statisk server-rendering (.NET 10)
- Klassiska controllers för formulär och ett litet JSON-API
- SQLite via `Microsoft.Data.Sqlite`
- Ren HTML och CSS, inga externa frontend-beroenden utöver Bootstrap

## Licens

Utbildningsmaterial för TUC Yrkeshögskola. Fri att använda i undervisning.
