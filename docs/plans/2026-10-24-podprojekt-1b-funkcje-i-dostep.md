# Podprojekt 1B – Funkcje i model dostępu: plan implementacji

> **Dla agentów:** wymagany sub-skill: superpowers:executing-plans (lub superpowers:subagent-driven-development).
> Kroki mają checkboxy (`- [ ]`) do śledzenia postępu.

**Cel:** portfele, konta, kategorie, tagi i rekordy na rdzeniu modelu v2 z członkostwem w portfelu, udostępnianiem kont
i zaproszeniami – API + ekrany „Paper and ink”.

**Architektura:** encje i reguły w `BM2.Domain` (czysty C#, `Result`), DTO/walidatory/enumy w `BM2.Contracts`,
endpointy Minimal API per funkcja w `BM2.Server/Features`, dostęp przez filtr grupy `/api/wallets/{walletId}`
ładujący `WalletAccess` do scoped `CurrentWallet`. Klient: `ApiClient` + `WalletState` + własne komponenty.

**Stack:** .NET 10, EF Core 10 (SQL Server, tryb 110), FluentValidation 12 (Apache-2.0), xUnit v3, Shouldly, bUnit, Respawn.

**Spec:** [docs/specs/2026-10-24-podprojekt-1b-funkcje-i-dostep.md](../specs/2026-10-24-podprojekt-1b-funkcje-i-dostep.md)

## Ograniczenia globalne

- Wszystko z AGENTS.md; w szczególności: bez funkcji SQL Server 2016+ (strażnik `SqlServer2012Guard` w testach),
  paczki tylko stabilne i na licencjach MIT/Apache-2.0/BSD, bez MediatR/AutoMapper/FluentAssertions.
- Kwoty `decimal(19,4)`, > 0, ≤ `999 999 999 999,99`, maks. 2 miejsca po przecinku; sumy zawsze per waluta.
- Waluty: `varchar(3)` (`char(3)`), tylko kody z `Currencies.All`.
- Daty rekordów `DateOnly` (`date`); momenty `DateTimeOffset` w UTC z `TimeProvider` (wstrzykiwany, `TimeProvider.System`).
- Zakresy dat: `from` włącznie, `to` wyłącznie (`>= from AND < to`).
- Brak dostępu do portfela → 404 ProblemDetails; za niska rola → 403 ProblemDetails; walidacja → 400 `ValidationProblem`.
- Teksty UI po angielsku wprost w komponentach, bez sklejania stringów (i18n w 1C). Kwoty zawsze ze znakiem `+`/`−`
  (U+2212). Kolory i odstępy tylko z tokenów; animacje wyłączane przez `prefers-reduced-motion`; cele dotyku ≥ 44 px.
- Kod, identyfikatory, komentarze i commity po angielsku, commity w trybie rozkazującym, stopka
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`. Commity na `dev`, bez push.
- Oszczędność: w zadaniu uruchamiaj tylko jego testy (`dotnet test --project <proj> -- --filter-class <FQN>`);
  pełna suita i build `CI=true` na końcu plasterka. Działający serwer demo blokuje pliki Debug – zatrzymaj go przed
  budowaniem albo testuj w Release (`-c Release`).
- Demo po każdym plasterku: `dotnet run --project src/BM2.Server --launch-profile https` → https://localhost:7237
  (konto `demo@bm2.test` z [dev-setup](../dev-setup.md)).

## Na co uważać w review

1. **Edycja rekordu przenosząca go na konto bez prawa zapisu** (albo zmiana konta docelowego transferu) – serwer musi
   sprawdzić zapis na starych i nowych kontach → test w Task 11.
2. **Edycja rekordu z zarchiwizowaną kategorią lub na zarchiwizowanym koncie** – zapis bez zmiany kategorii/konta ma
   przejść; archiwum blokuje tylko nowe przypisania → testy w Task 10.
3. **Granice miesiąca** – rekord z ostatniego dnia miesiąca jest w zakresie, z pierwszego dnia następnego nie → test
   w Task 12.
4. **Kwota wpisana z przecinkiem w kulturze pl-PL („12,50”)** – `MoneyInput` parsuje ją poprawnie → test w Task 7.
5. **Przyjęcie zaproszenia przez kogoś z wyższą rolą** (Owner otwiera link Editora) – rola nie może spaść → test
   w Task 16.

## Mapa plików

```
src/BM2.Contracts/
  Common/Currencies.cs, Limits.cs, MoneyRules.cs
  Wallets/MemberRole.cs, WalletRequests.cs, WalletResponses.cs, WalletValidators.cs
  Accounts/AccountRequests.cs, AccountItem.cs, AccountValidators.cs
  Categories/CategoryType.cs, CategoryContracts.cs, CategoryValidators.cs
  Tags/TagContracts.cs, TagValidators.cs
  Records/RecordType.cs, RecordStatus.cs, SaveRecordRequest.cs, SaveRecordRequestValidator.cs,
          RecordItem.cs, RecordPage.cs, RecordFilter.cs, RecordSummary.cs, ExchangeMath.cs
  Members/MemberContracts.cs
  Invitations/InvitationContracts.cs, CreateInvitationRequestValidator.cs
src/BM2.Domain/
  Common/Error.cs, Result.cs
  Access/WalletPermission.cs, WalletAccess.cs
  Wallets/Wallet.cs, WalletMember.cs, DefaultCategories.cs
  Accounts/Account.cs, AccountMember.cs
  Categories/Category.cs
  Tags/Tag.cs
  Records/Record.cs, RecordTag.cs, RecordDraft.cs, RecordContext.cs
  Invitations/Invitation.cs, InvitationToken.cs
src/BM2.Server/
  Data/AppDbContext.cs (DbSety + ApplyConfigurationsFromAssembly), Data/Configurations/*.cs, Data/Migrations/*
  Infrastructure/ProblemResults.cs, ValidationFilter.cs, RegistrationGate.cs (zmiana)
  Features/ApiEndpoints.cs (zmiana), Features/FeatureServices.cs
  Features/Wallets/  CurrentWallet.cs, WalletAccessFilter.cs, WalletEndpoints.cs, ListWallets.cs, CreateWallet.cs,
                     GetWallet.cs, UpdateWallet.cs, DeleteWallet.cs
  Features/Accounts/ AccountEndpoints.cs, CreateAccount.cs, UpdateAccount.cs, DeleteAccount.cs, AccountBalances.cs
  Features/Categories/ CategoryEndpoints.cs, CreateCategory.cs, UpdateCategory.cs, DeleteCategory.cs
  Features/Tags/     TagEndpoints.cs, CreateTag.cs, UpdateTag.cs, DeleteTag.cs
  Features/Records/  RecordEndpoints.cs, RecordQueries.cs, RecordCursor.cs, RecordContextLoader.cs, ListRecords.cs,
                     GetRecordSummary.cs, GetRecord.cs, CreateRecord.cs, UpdateRecord.cs, DeleteRecord.cs
  Features/Members/  MemberEndpoints.cs, ListMembers.cs, UpdateMemberRole.cs, RemoveMember.cs, UpdateAccountShare.cs,
                     RemoveAccountShare.cs
  Features/Invitations/ InvitationEndpoints.cs, InvitationLookup.cs, CreateInvitation.cs, RevokeInvitation.cs,
                        GetInvitation.cs, AcceptInvitation.cs
  Components/Account/Pages/Register.razor (zmiana)
  wwwroot/css/components.css, motion.css (rozszerzenia)
src/BM2.Client/
  Program.cs (rejestracje), Layout/MainLayout.razor (przełącznik portfela, ToastHost)
  Services/ApiClient.cs, ApiResult.cs, BrowserStorage.cs, ToastService.cs
  State/WalletState.cs
  Components/Sheet.razor(+.razor.js), ConfirmDialog.razor, ToastHost.razor, SegmentedControl.razor, ChipGroup.razor,
             ListRow.razor, Amount.razor, WalletSwitcher.razor, Icon.razor (nowe ikony)
  Components/Forms/FluentValidator.cs, TextField.razor, SelectField.razor, DateField.razor, MoneyInput.razor
  Components/Wallets/CreateWalletForm.razor
  Components/Records/RecordForm.razor, RecordList.razor, RecordGrouping.cs, RecordFilterSheet.razor
  Pages/Home.razor, Records.razor, RecordEditor.razor, Wallets.razor, Accounts.razor, Categories.razor, Tags.razor,
        Members.razor, Invite.razor, More.razor (zmiana); QuickAdd.razor (usunięty – zastępuje go RecordEditor)
tests/BM2.UnitTests/      Access/, Wallets/, Accounts/, Records/, Invitations/, Contracts/
tests/BM2.IntegrationTests/Infrastructure/TestData.cs, Features/WalletEndpointAccessTests.cs,
                          Features/{Wallets,Accounts,Categories,Tags,Records,Members,Invitations}/*Tests.cs
tests/BM2.ClientTests/    Infrastructure/StubApi.cs, Components/*, Pages/*
```

Uwagi do speca przyjęte w planie: szybkie dodawanie zostaje pod istniejącą trasą `records/new` (FAB już tam prowadzi),
edycja rekordu to strona `records/{id}` w wyglądzie wysuwanego arkusza (głęboki link, prostszy formularz niż arkusz
w liście), API zaproszeń zwraca token, a link składa klient (`NavigationManager.BaseUri + "invite/" + token` – bez
zależności od nagłówka Host).

---

## Plasterek 1 – fundamenty dostępu i portfele

### Task 1: Result, role i `WalletAccess` (domena)

**Files:**
- Create: `src/BM2.Contracts/Wallets/MemberRole.cs`, `src/BM2.Contracts/Common/{Currencies,Limits,MoneyRules}.cs`
- Create: `src/BM2.Domain/Common/{Error,Result}.cs`, `src/BM2.Domain/Access/{WalletPermission,WalletAccess}.cs`
- Test: `tests/BM2.UnitTests/Access/WalletAccessTests.cs`, `tests/BM2.UnitTests/Contracts/MoneyRulesTests.cs`

**Interfaces:**
- Produces:
  - `enum MemberRole { Viewer = 1, Editor = 2, Owner = 3 }` (wartość = siła roli; dla kont dozwolone Viewer/Editor).
  - `static class Currencies { IReadOnlyList<string> All; bool IsSupported(string? code); }` – kolejność:
    PLN, EUR, USD, GBP, CHF, CZK, SEK, NOK, DKK, HUF, UAH, JPY, CAD, AUD.
  - `static class Limits { WalletName = 50, AccountName = 50, CategoryName = 50, TagName = 30, RecordName = 100 }` (const int).
  - `static class MoneyRules { const decimal MaxAmount = 999_999_999_999.99m; bool IsValidAmount(decimal v); bool HasAtMostTwoDecimals(decimal v); }`
    (`IsValidAmount` = `> 0 && <= MaxAmount && HasAtMostTwoDecimals`).
  - `enum ErrorKind { Validation, NotFound, Forbidden, Conflict, Gone }`;
    `sealed record Error(ErrorKind Kind, string Message, string? Field = null)` z fabrykami
    `Validation(string field, string message)`, `NotFound()`, `Forbidden()`, `Conflict(string message, string? field = null)`, `Gone(string message)`.
  - `readonly struct Result { bool IsSuccess; Error? Error; static Result Success; implicit operator Result(Error e) }`,
    `readonly struct Result<T> { bool IsSuccess; T Value; Error? Error; implicit operator Result<T>(T v); implicit operator Result<T>(Error e) }`.
  - `enum WalletPermission { ManageWallet, ManageAccounts, ManageCategories, ViewMembers, ManageMembers }`.
  - `sealed class WalletAccess`:
    `static WalletAccess Create(Guid walletId, MemberRole? walletRole, IEnumerable<(Guid AccountId, MemberRole? ShareRole)> accounts)`;
    `Guid WalletId`; `MemberRole? WalletRole`; `bool HasAnyAccess`; `bool IsLimited` (brak roli w portfelu);
    `bool Can(WalletPermission p)`; `MemberRole? RoleFor(Guid accountId)`; `bool CanReadAccount(Guid id)`;
    `bool CanWriteAccount(Guid id)`; `bool CanReadRecord(Guid accountId, Guid? transferAccountId)`;
    `bool CanWriteRecord(Guid accountId, Guid? transferAccountId)`; `IReadOnlyCollection<Guid> ReadableAccountIds`.

- [ ] **Step 1: Write the failing tests**

```csharp
public class WalletAccessTests
{
    static readonly Guid W = Guid.NewGuid(), A = Guid.NewGuid(), B = Guid.NewGuid();

    static WalletAccess Member(MemberRole role) => WalletAccess.Create(W, role, [(A, null), (B, null)]);
    static WalletAccess SharedA(MemberRole role) => WalletAccess.Create(W, null, [(A, role), (B, null)]);

    [Theory]
    [InlineData(MemberRole.Owner, true, true, true, true, true)]
    [InlineData(MemberRole.Editor, false, false, true, true, false)]
    [InlineData(MemberRole.Viewer, false, false, false, true, false)]
    public void Wallet_roles_follow_permission_matrix(MemberRole role, bool wallet, bool accounts, bool categories, bool viewMembers, bool manageMembers)
    {
        var access = Member(role);
        access.Can(WalletPermission.ManageWallet).ShouldBe(wallet);
        access.Can(WalletPermission.ManageAccounts).ShouldBe(accounts);
        access.Can(WalletPermission.ManageCategories).ShouldBe(categories);
        access.Can(WalletPermission.ViewMembers).ShouldBe(viewMembers);
        access.Can(WalletPermission.ManageMembers).ShouldBe(manageMembers);
    }

    [Fact] public void Viewer_reads_all_accounts_but_writes_none() { var a = Member(MemberRole.Viewer); a.ReadableAccountIds.ShouldBe([A, B], ignoreOrder: true); a.CanWriteAccount(A).ShouldBeFalse(); }
    [Fact] public void Shared_account_editor_sees_and_writes_only_that_account() { var a = SharedA(MemberRole.Editor); a.ReadableAccountIds.ShouldBe([A]); a.CanWriteAccount(A).ShouldBeTrue(); a.CanReadAccount(B).ShouldBeFalse(); }
    [Fact] public void Shared_account_user_has_no_wallet_permissions() { var a = SharedA(MemberRole.Editor); a.IsLimited.ShouldBeTrue(); Enum.GetValues<WalletPermission>().ShouldAllBe(p => !a.Can(p)); }
    [Fact] public void Higher_of_wallet_role_and_share_role_wins() => WalletAccess.Create(W, MemberRole.Viewer, [(A, MemberRole.Editor)]).CanWriteAccount(A).ShouldBeTrue();
    [Fact] public void Transfer_is_readable_with_either_side() => SharedA(MemberRole.Viewer).CanReadRecord(B, A).ShouldBeTrue();
    [Fact] public void Transfer_write_needs_both_sides() => SharedA(MemberRole.Editor).CanWriteRecord(A, B).ShouldBeFalse();
    [Fact] public void No_role_and_no_share_means_no_access() => WalletAccess.Create(W, null, [(A, null)]).HasAnyAccess.ShouldBeFalse();
    [Fact] public void Unknown_account_is_not_readable() => Member(MemberRole.Owner).CanReadAccount(Guid.NewGuid()).ShouldBeFalse();
}

public class MoneyRulesTests
{
    [Theory]
    [InlineData("0.01", true)] [InlineData("999999999999.99", true)] [InlineData("0", false)]
    [InlineData("-5", false)] [InlineData("1.005", false)] [InlineData("1000000000000", false)]
    public void Validates_amounts(string value, bool valid) =>
        MoneyRules.IsValidAmount(decimal.Parse(value, CultureInfo.InvariantCulture)).ShouldBe(valid);

    [Fact] public void Supported_currencies_start_with_pln() { Currencies.All[0].ShouldBe("PLN"); Currencies.IsSupported("XYZ").ShouldBeFalse(); }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --project tests/BM2.UnitTests -- --filter-class BM2.UnitTests.Access.WalletAccessTests`
Expected: FAIL (build error – types missing).

- [ ] **Step 3: Implement the types listed in Interfaces**

`WalletAccess` trzyma słownik `accountId → MemberRole?` (rola efektywna = max z roli w portfelu i roli udostępnienia).
`Can` dla `IsLimited` zawsze `false`; macierz jak w tabeli speca.

- [ ] **Step 4: Run tests to verify they pass** – oba filtry klas (`…Access.WalletAccessTests`, `…Contracts.MoneyRulesTests`): PASS.

- [ ] **Step 5: Commit** – `git commit -m "Add Result, member roles and wallet access matrix"`

### Task 2: Encje portfela i kont, członkostwo, migracja

**Files:**
- Create: `src/BM2.Domain/Wallets/{Wallet,WalletMember}.cs`, `src/BM2.Domain/Accounts/{Account,AccountMember}.cs`
- Create: `src/BM2.Server/Data/Configurations/{Wallet,WalletMember,Account,AccountMember}Configuration.cs`
- Modify: `src/BM2.Server/Data/AppDbContext.cs`; migracja `AddWalletsAndAccounts`
- Test: `tests/BM2.UnitTests/Wallets/WalletMembershipTests.cs`, `tests/BM2.UnitTests/Accounts/AccountTests.cs`

**Interfaces:**
- Consumes: `MemberRole`, `Error`, `Result` (Task 1).
- Produces (wszystkie właściwości `private set`, prywatny konstruktor dla EF):
  - `Wallet`: `Guid Id; string Name; string BaseCurrency; DateTimeOffset CreatedAt; IReadOnlyCollection<WalletMember> Members`;
    `static Wallet Create(string name, string baseCurrency, Guid ownerId, DateTimeOffset now)` (twórca = Owner);
    `void Update(string name, string baseCurrency)`; `void Grant(Guid userId, MemberRole role, DateTimeOffset now)`
    (dodaje lub podnosi, nigdy nie obniża); `Result ChangeMemberRole(Guid userId, MemberRole role)`;
    `Result RemoveMember(Guid userId)`.
  - `WalletMember`: `Guid WalletId; Guid UserId; MemberRole Role; DateTimeOffset JoinedAt`.
  - `Account`: `Guid Id; Guid WalletId; string Name; string Currency; decimal OpeningBalance; bool IsArchived;
    DateTimeOffset CreatedAt; IReadOnlyCollection<AccountMember> Members`;
    `static Account Create(Guid walletId, string name, string currency, decimal openingBalance, DateTimeOffset now)`;
    `Result Update(string name, string currency, decimal openingBalance, bool isArchived, bool hasRecords)`;
    `Result Grant(Guid userId, MemberRole role, DateTimeOffset now)` (Owner → Validation `"Role"`);
    `Result ChangeShareRole(Guid userId, MemberRole role)`; `Result RemoveShare(Guid userId)`.
  - `AccountMember`: `Guid AccountId; Guid UserId; MemberRole Role; DateTimeOffset JoinedAt`.
  - `AppDbContext`: `DbSet<Wallet> Wallets`, `DbSet<WalletMember> WalletMembers`, `DbSet<Account> Accounts`,
    `DbSet<AccountMember> AccountMembers`; `OnModelCreating` woła `base` i `ApplyConfigurationsFromAssembly`.

- [ ] **Step 1: Write the failing tests**

```csharp
public class WalletMembershipTests
{
    static readonly DateTimeOffset Now = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);
    static readonly Guid Owner = Guid.NewGuid(), Other = Guid.NewGuid();

    [Fact] public void Creator_becomes_owner() => Wallet.Create("Home", "PLN", Owner, Now).Members.Single().Role.ShouldBe(MemberRole.Owner);
    [Fact] public void Last_owner_cannot_leave() => Wallet.Create("Home", "PLN", Owner, Now).RemoveMember(Owner).Error!.Kind.ShouldBe(ErrorKind.Conflict);
    [Fact] public void Last_owner_cannot_be_demoted() => Wallet.Create("Home", "PLN", Owner, Now).ChangeMemberRole(Owner, MemberRole.Editor).Error!.Kind.ShouldBe(ErrorKind.Conflict);
    [Fact] public void Owner_can_leave_when_another_owner_exists() { var w = Wallet.Create("Home", "PLN", Owner, Now); w.Grant(Other, MemberRole.Owner, Now); w.RemoveMember(Owner).IsSuccess.ShouldBeTrue(); }
    [Fact] public void Grant_never_downgrades() { var w = Wallet.Create("Home", "PLN", Owner, Now); w.Grant(Owner, MemberRole.Viewer, Now); w.Members.Single().Role.ShouldBe(MemberRole.Owner); }
    [Fact] public void Removing_unknown_member_is_not_found() => Wallet.Create("Home", "PLN", Owner, Now).RemoveMember(Other).Error!.Kind.ShouldBe(ErrorKind.NotFound);
    [Fact] public void Names_are_trimmed() => Wallet.Create("  Home ", "PLN", Owner, Now).Name.ShouldBe("Home");
}

public class AccountTests
{
    static readonly DateTimeOffset Now = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);
    static Account New() => Account.Create(Guid.NewGuid(), "Cash", "PLN", 0, Now);

    [Fact] public void Currency_cannot_change_when_account_has_records() => New().Update("Cash", "EUR", 0, false, hasRecords: true).Error!.Field.ShouldBe("Currency");
    [Fact] public void Currency_can_change_without_records() => New().Update("Cash", "EUR", 0, false, hasRecords: false).IsSuccess.ShouldBeTrue();
    [Fact] public void Account_cannot_be_shared_as_owner() => New().Grant(Guid.NewGuid(), MemberRole.Owner, Now).Error!.Field.ShouldBe("Role");
    [Fact] public void Share_grant_never_downgrades() { var a = New(); var u = Guid.NewGuid(); a.Grant(u, MemberRole.Editor, Now); a.Grant(u, MemberRole.Viewer, Now); a.Members.Single().Role.ShouldBe(MemberRole.Editor); }
}
```

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.UnitTests.Wallets.WalletMembershipTests`: FAIL (build).

- [ ] **Step 3: Implement entities and EF configuration**

Konfiguracja: `Name` `HasMaxLength(Limits.…)`; waluty `HasMaxLength(3).IsUnicode(false).IsFixedLength()`;
`WalletMember` PK (`WalletId`, `UserId`), FK do `Wallet` Cascade, FK do `ApplicationUser` (`HasOne<ApplicationUser>().WithMany()`)
Cascade; `Account` FK do `Wallet` **NoAction**, unikalny indeks (`WalletId`, `Name`); `AccountMember` PK (`AccountId`,
`UserId`), FK do `Account` i `ApplicationUser` Cascade. Kolekcje `Members` przez pole (`HasMany(...).WithOne()`,
`Navigation(...).UsePropertyAccessMode(PropertyAccessMode.Field)`).

- [ ] **Step 4: Run tests to verify they pass** – oba filtry: PASS.

- [ ] **Step 5: Add migration and check it**

Run: `dotnet ef migrations add AddWalletsAndAccounts --project src/BM2.Server --output-dir Data/Migrations`
Expected: migracja tworzy `Wallets`, `WalletMembers`, `Accounts`, `AccountMembers`; brak `ON DELETE CASCADE` między
`Accounts` a `Wallets`. Run: `dotnet test --project tests/BM2.IntegrationTests -- --filter-class BM2.IntegrationTests.DatabaseTests`
→ PASS (migracja stosuje się w trybie 110).

- [ ] **Step 6: Commit** – `git commit -m "Add wallet and account entities with membership rules"`

### Task 3: Infrastruktura API i endpointy portfeli

**Files:**
- Modify: `Directory.Packages.props` (FluentValidation, FluentValidation.DependencyInjectionExtensions – najnowsze
  stabilne 12.x, Apache-2.0; sprawdź `dotnet package search FluentValidation --exact-match`),
  `src/BM2.Contracts/BM2.Contracts.csproj`, `src/BM2.Server/BM2.Server.csproj`, `src/BM2.Server/Program.cs`,
  `src/BM2.Server/Features/ApiEndpoints.cs`
- Create: `src/BM2.Contracts/Wallets/{WalletRequests,WalletResponses,WalletValidators}.cs`,
  `src/BM2.Contracts/Accounts/AccountItem.cs`
- Create: `src/BM2.Server/Infrastructure/{ProblemResults,ValidationFilter}.cs`, `src/BM2.Server/Features/FeatureServices.cs`,
  `src/BM2.Server/Features/Wallets/*.cs` (lista w mapie plików)
- Create: `tests/BM2.IntegrationTests/Infrastructure/TestData.cs`, `tests/BM2.IntegrationTests/Features/WalletEndpointAccessTests.cs`,
  `tests/BM2.IntegrationTests/Features/Wallets/WalletTests.cs`, `tests/BM2.UnitTests/Contracts/WalletValidatorTests.cs`
- Modify: `tests/BM2.IntegrationTests/Infrastructure/IntegrationTest.cs` (`protected TestData Data`)

**Interfaces:**
- Consumes: Task 1–2.
- Produces:
  - Contracts: `CreateWalletRequest(string Name, string BaseCurrency)`, `UpdateWalletRequest(string Name, string BaseCurrency)`
    + walidatory `CreateWalletRequestValidator`, `UpdateWalletRequestValidator` (nazwa niepusta po trim, ≤ `Limits.WalletName`;
    waluta `Currencies.IsSupported`); `WalletListItem(Guid Id, string Name, string BaseCurrency, MemberRole? Role)`;
    `WalletPermissions(bool ManageWallet, bool ManageAccounts, bool ManageCategories, bool ViewMembers, bool ManageMembers)`;
    `WalletSnapshot(Guid Id, string Name, string BaseCurrency, MemberRole? Role, WalletPermissions Permissions, IReadOnlyList<AccountItem> Accounts)`
    (Task 8 dopisze `Categories`, `Tags`); `AccountItem(Guid Id, string Name, string Currency, decimal OpeningBalance, decimal Balance, bool IsArchived, bool CanWrite)`.
  - `ProblemResults.ToProblem(this Error error) : ProblemHttpResult` – Validation → `HttpValidationProblemDetails`
    (400, błąd pod `Field`), NotFound 404, Forbidden 403, Conflict 409 (z `errors[Field]`, gdy jest), Gone 410.
  - `ValidationFilter<T> : IEndpointFilter` + `RouteHandlerBuilder Validate<T>(this RouteHandlerBuilder)`;
    brak walidatora w DI = błąd konfiguracji (wyjątek przy starcie żądania).
  - `CurrentWallet` (scoped): `Guid Id`, `WalletAccess Access` (ustawiane przez filtr).
  - `WalletAccessFilter : IEndpointFilter` – czyta `walletId` z trasy, jednym zapytaniem (projekcja `Wallets` z rolą
    członka i kontami z rolą udostępnienia) buduje `WalletAccess`; brak portfela lub `!HasAnyAccess` → 404 Problem.
  - `FeatureServices.AddBm2Features(this IServiceCollection)` – `CurrentWallet`, walidatory
    (`AddValidatorsFromAssemblyContaining<CreateWalletRequestValidator>()`), `TimeProvider.System`.
  - `ApiEndpoints`: `api.MapWalletEndpoints()` mapuje `/wallets` (lista, tworzenie) oraz grupę
    `api.MapGroup("/wallets/{walletId:guid}").AddEndpointFilter<WalletAccessFilter>()` zwracaną jako `RouteGroupBuilder wallet`,
    do której kolejne taski dodają `wallet.Map<Obszar>Endpoints()`.
  - Endpointy: `GET /api/wallets`, `POST /api/wallets` (201 + `WalletListItem`), `GET /api/wallets/{w}` (`WalletSnapshot`),
    `PUT /api/wallets/{w}` (204, `ManageWallet`), `DELETE /api/wallets/{w}` (204, `ManageWallet`; w transakcji
    `ExecuteDeleteAsync` w kolejności ze speca – w tym tasku konta i portfel, kolejne taski dopisują swoje tabele na początku).
  - `TestData` (testy integracyjne): `Task<Wallet> WalletAsync(Guid ownerId, string name = "Home", string baseCurrency = "PLN")`,
    `Task MemberAsync(Guid walletId, Guid userId, MemberRole role)`,
    `Task<Account> AccountAsync(Guid walletId, string name = "Cash", string currency = "PLN", decimal openingBalance = 0)`,
    `Task ShareAsync(Guid accountId, Guid userId, MemberRole role)`.
  - `WalletEndpointAccessTests`: lista przypadków `EndpointCase(string Method, string Path, object? Body, MemberRole Minimum)`
    z placeholderami `{w}`, `{a}` (konto), `{c}`, `{t}`, `{r}`, `{u}` (inny członek), `{i}` (zaproszenie); dwa testy-teorie
    opisane niżej. Każdy kolejny task API dopisuje swoje przypadki.

- [ ] **Step 1: Write the failing tests**

```csharp
public class WalletTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact] public async Task Created_wallet_is_listed_with_owner_role() { /* POST {Name:"Home",BaseCurrency:"PLN"} → 201; GET /api/wallets */ list.ShouldHaveSingleItem().Role.ShouldBe(MemberRole.Owner); }
    [Fact] public async Task Wallet_list_contains_only_my_wallets() { /* Data.WalletAsync(other.Id) */ list.ShouldBeEmpty(); }
    [Fact] public async Task Shared_account_wallet_is_listed_without_role() { /* Data.ShareAsync(account, me, Viewer) */ list.Single().Role.ShouldBeNull(); }
    [Fact] public async Task Snapshot_of_limited_user_contains_only_shared_account() { snapshot.Accounts.Select(a => a.Id).ShouldBe([shared.Id]); snapshot.Permissions.ShouldBe(new WalletPermissions(false, false, false, false, false)); }
    [Fact] public async Task Invalid_wallet_returns_validation_problem() { /* POST {Name:"",BaseCurrency:"XYZ"} */ response.StatusCode.ShouldBe(HttpStatusCode.BadRequest); problem!.Errors.Keys.ShouldBe(["Name", "BaseCurrency"], ignoreOrder: true); }
    [Fact] public async Task Owner_updates_wallet() { /* PUT → 204, GET snapshot */ snapshot.Name.ShouldBe("Family"); }
    [Fact] public async Task Deleting_wallet_removes_accounts_and_members() { /* DELETE → 204 */ (await db.Accounts.CountAsync()).ShouldBe(0); (await db.WalletMembers.CountAsync()).ShouldBe(0); }
}

public class WalletEndpointAccessTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    public static TheoryData<EndpointCase> Cases => [
        new("GET", "/api/wallets/{w}", null, MemberRole.Viewer),
        new("PUT", "/api/wallets/{w}", new UpdateWalletRequest("Renamed", "PLN"), MemberRole.Owner),
        new("DELETE", "/api/wallets/{w}", null, MemberRole.Owner),
    ];

    [Theory, MemberData(nameof(Cases))]
    public async Task Outsider_gets_404(EndpointCase c) => (await SendAsOutsiderAsync(c)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

    [Theory, MemberData(nameof(Cases))]
    public async Task Role_below_minimum_gets_403(EndpointCase c)
    {
        Assert.SkipWhen(c.Minimum == MemberRole.Viewer, "Every member may call it.");
        (await SendAsMemberAsync(c, (MemberRole)((int)c.Minimum - 1))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
```

Fixture przypadków tworzy: portfel ownera z kontem, (od Task 8) kategorią i tagiem, (od Task 11) rekordem, drugim
członkiem (Editor) i (od Task 16) zaproszeniem; outsider ma własny portfel. `EndpointCase` implementuje
`IXunitSerializable` albo jest rekordem z `ToString()` – tak, żeby teoria pokazywała nazwę trasy.

```csharp
public class WalletValidatorTests
{
    [Fact] public void Name_is_required_after_trim() => new CreateWalletRequestValidator().Validate(new CreateWalletRequest("   ", "PLN")).Errors.ShouldContain(e => e.PropertyName == "Name");
    [Fact] public void Name_max_50() => new CreateWalletRequestValidator().Validate(new CreateWalletRequest(new string('x', 51), "PLN")).IsValid.ShouldBeFalse();
    [Fact] public void Currency_must_be_supported() => new CreateWalletRequestValidator().Validate(new CreateWalletRequest("Home", "XYZ")).Errors.ShouldContain(e => e.PropertyName == "BaseCurrency");
}
```

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.IntegrationTests.Features.Wallets.WalletTests`: FAIL.

- [ ] **Step 3: Implement packages, infrastructure, filter and wallet endpoints**

Saldo w migawce = `OpeningBalance` (rekordy dochodzą w Task 11). Konta w migawce: tylko `Access.ReadableAccountIds`,
sortowane: aktywne przed zarchiwizowanymi, potem po nazwie. Odpowiedzi `Results<…, ProblemHttpResult>`.

- [ ] **Step 4: Run tests to verify they pass** – filtry `…Wallets.WalletTests`, `…Features.WalletEndpointAccessTests`,
  `BM2.UnitTests.Contracts.WalletValidatorTests`, `BM2.UnitTests.ArchitectureTests`: PASS.

- [ ] **Step 5: Commit** – `git commit -m "Add wallet endpoints with access filter and validation"`

### Task 4: Fundamenty klienta i komponenty bazowe

**Files:**
- Create: `src/BM2.Client/Services/{ApiClient,ApiResult,BrowserStorage,ToastService}.cs`, `src/BM2.Client/State/WalletState.cs`
- Create: `src/BM2.Client/Components/{Sheet.razor,Sheet.razor.js,ConfirmDialog.razor,ToastHost.razor,ListRow.razor}`
- Create: `src/BM2.Client/Components/Forms/{FluentValidator.cs,TextField.razor,SelectField.razor}`
- Modify: `src/BM2.Client/Program.cs`, `src/BM2.Client/_Imports.razor`, `src/BM2.Client/Components/Icon.razor`
  (ikony `close`, `check`, `trash`, `edit`, `chevron-down`), `src/BM2.Server/wwwroot/css/{components,motion}.css`
- Test: `tests/BM2.ClientTests/Infrastructure/StubApi.cs` (wydzielony z `MeGreetingTests`), `tests/BM2.ClientTests/Services/ApiClientTests.cs`,
  `tests/BM2.ClientTests/Components/FluentValidatorTests.cs`

**Interfaces:**
- Produces:
  - `sealed record ApiProblem(int Status, string? Detail, IReadOnlyDictionary<string, string[]> Errors)` (Status 0 = brak sieci);
    `ApiResult` (`bool Ok`, `ApiProblem? Problem`) i `ApiResult<T>` (+ `T? Value`).
  - `ApiClient(HttpClient http, NavigationManager navigation)`: `Task<ApiResult<T>> GetAsync<T>(string url)`,
    `Task<ApiResult<T>> PostAsync<T>(string url, object body)`, `Task<ApiResult> PostAsync(string url, object? body = null)`,
    `Task<ApiResult> PutAsync(string url, object body)`, `Task<ApiResult> DeleteAsync(string url)`. 401 → `NavigateTo("Account/Login?returnUrl=…", forceLoad: true)`;
    ProblemDetails/ValidationProblem → `ApiProblem` z błędami; `HttpRequestException` → Status 0.
    Konwencja stron: 403/404 → `EmptyState` z komunikatem; 400/409 → `FluentValidator.ShowServerErrors`;
    inne (w tym 0) → toast „Something went wrong. Try again.”
  - `BrowserStorage(IJSRuntime js)`: `ValueTask<string?> GetAsync(string key)`, `ValueTask SetAsync(string key, string value)`,
    `ValueTask RemoveAsync(string key)` – przez `localStorage.getItem/setItem/removeItem`, wyjątki JS → `null`/no-op.
  - `ToastService`: `void Show(string message, ToastKind kind = ToastKind.Info)`, `event Action? Changed`,
    `IReadOnlyList<Toast> Items`; toast znika po 4 s. `ToastHost` w `MainLayout` (`aria-live="polite"`).
  - `WalletState(ApiClient api, BrowserStorage storage)`: `IReadOnlyList<WalletListItem> Wallets`, `WalletSnapshot? Current`,
    `bool IsLoaded`, `event Action? Changed`, `Task InitializeAsync()` (lista + wybór z `bm2-wallet`, w innym razie pierwszy),
    `Task SelectAsync(Guid walletId)`, `Task RefreshAsync()` (lista i migawka), `ValueTask<Guid?> GetLastAccountAsync()`,
    `ValueTask SetLastAccountAsync(Guid accountId)` (klucz `bm2-last-account:{walletId}`).
  - `Sheet`: parametry `bool Open`, `EventCallback<bool> OpenChanged`, `string Title`, `RenderFragment ChildContent`,
    `RenderFragment? Footer`; natywny `<dialog>` otwierany `showModal()` z modułu `Sheet.razor.js`
    (`import("./_content/BM2.Client/Components/Sheet.razor.js")` – zweryfikuj ścieżkę w przeglądarce), zamknięcie
    Escape/tłem → `OpenChanged(false)`; na telefonie od dołu, od 768 px wyśrodkowany.
  - `ConfirmDialog`: `bool Open`, `EventCallback<bool> OpenChanged`, `string Title`, `string ConfirmLabel`,
    `bool Danger`, `string? RequireText` (przycisk aktywny po wpisaniu dokładnie tego tekstu), `EventCallback OnConfirm`.
  - `FluentValidator` (komponent w `EditForm`): uruchamia walidator FluentValidation dla modelu (z `IValidator<T>`
    przekazanego parametrem `Validator`), mapuje `PropertyName` na pola; `void ShowServerErrors(ApiProblem problem)`.
  - `TextField`, `SelectField<TValue>`: `Label`, `Value`/`ValueChanged`/`ValueExpression`, `Hint`; etykieta `for`,
    błąd pod polem z `aria-describedby`, `aria-invalid`.
  - `ListRow`: `Title`, `Subtitle`, `RenderFragment? Leading`, `RenderFragment? Trailing`, `string? Href`, `EventCallback OnClick`.
  - CSS: `.sheet`, `.sheet__header`, `.sheet__body`, `.sheet__footer`, `.field`, `.field__label`, `.field__control`,
    `.field__hint`, `.field__error`, `.list-row`, `.toast-host`, `.toast`; animacje `sheet-in` (translateY + opacity),
    `toast-in` w `motion.css` pod `prefers-reduced-motion: no-preference`.

- [ ] **Step 1: Write the failing tests**

```csharp
public class ApiClientTests
{
    [Fact] public async Task Maps_validation_problem_to_field_errors() { /* stub 400 application/problem+json {"errors":{"Name":["Required"]}} */ result.Problem!.Errors["Name"].ShouldBe(["Required"]); }
    [Fact] public async Task Redirects_to_login_on_401() { /* stub 401 */ navigation.History.ShouldContain(e => e.Uri.Contains("Account/Login") && e.Options.ForceLoad); }
    [Fact] public async Task Network_failure_is_status_zero() { /* stub throws HttpRequestException */ result.Problem!.Status.ShouldBe(0); }
}

public class FluentValidatorTests
{
    [Fact] public void Shows_validator_message_under_field() { /* render EditForm with TextField(Name) + FluentValidator(CreateWalletRequestValidator), submit empty */ cut.Find(".field__error").TextContent.ShouldNotBeEmpty(); cut.Find("input").GetAttribute("aria-invalid").ShouldBe("true"); }
    [Fact] public void Shows_server_errors() { /* validator.ShowServerErrors(new ApiProblem(409, null, {"Name": ["Taken"]})) */ cut.Find(".field__error").TextContent.ShouldBe("Taken"); }
}
```

Model formularza w testach: klasa z właściwościami `Name`, `BaseCurrency` mapowana na `CreateWalletRequest` – walidator
działa na rekordzie, więc `FluentValidator` przyjmuje `Func<TModel, object> ToRequest` (domyślnie model) i mapuje nazwy
właściwości 1:1.

- [ ] **Step 2: Run tests to verify they fail** – `dotnet test --project tests/BM2.ClientTests -- --filter-class BM2.ClientTests.Services.ApiClientTests`: FAIL.

- [ ] **Step 3: Implement services, components and CSS**

Rejestracje w `Program.cs`: `ApiClient`, `BrowserStorage`, `ToastService`, `WalletState` jako scoped; walidatory
z `BM2.Contracts` (`AddValidatorsFromAssemblyContaining<CreateWalletRequestValidator>()` – wymaga
FluentValidation.DependencyInjectionExtensions także w kliencie; jeśli to zbędny narzut, rejestruj walidatory ręcznie
jako singletony).

- [ ] **Step 4: Run tests to verify they pass** – `ApiClientTests`, `FluentValidatorTests`, `MeGreetingTests`: PASS.

- [ ] **Step 5: Commit** – `git commit -m "Add API client, wallet state and base form components"`

### Task 5: UI portfeli – przełącznik, pierwsze uruchomienie, ekran Wallets

**Files:**
- Create: `src/BM2.Client/Components/WalletSwitcher.razor`, `src/BM2.Client/Components/Wallets/CreateWalletForm.razor`,
  `src/BM2.Client/Pages/Wallets.razor`
- Modify: `src/BM2.Client/Layout/MainLayout.razor`, `src/BM2.Client/Pages/{Home,More}.razor`, `components.css`
- Test: `tests/BM2.ClientTests/Components/WalletSwitcherTests.cs`

**Interfaces:**
- Consumes: `WalletState`, `Sheet`, `TextField`, `SelectField`, `FluentValidator`, `ConfirmDialog`, `ToastService` (Task 4).
- Produces: `CreateWalletForm` (`EventCallback<WalletListItem> OnCreated`) – nazwa (domyślnie „Personal”) i waluta
  (domyślnie `PLN`, lista `Currencies.All`); `WalletSwitcher` w nagłówku dla zalogowanych (przycisk z nazwą bieżącego
  portfela i `chevron-down` → arkusz z listą, zaznaczenie bieżącego, „New wallet”); Home: bez portfeli → pusty stan
  z `CreateWalletForm`; `/wallets`: lista z rolą, zmiana nazwy/waluty (Owner), usunięcie z `RequireText` = nazwa portfela.

- [ ] **Step 1: Write the failing tests**

```csharp
public class WalletSwitcherTests : BunitContext
{
    [Fact] public void Shows_current_wallet_name() => cut.Find("button.wallet-switcher").TextContent.ShouldContain("Home");
    [Fact] public void Selecting_wallet_loads_its_snapshot_and_remembers_it() { /* open sheet, click "Family" */ api.Requests.ShouldContain("GET api/wallets/{familyId}"); storage["bm2-wallet"].ShouldBe(familyId.ToString()); }
    [Fact] public void Marks_current_wallet_with_aria_current() => cut.Find("[aria-current=true]").TextContent.ShouldContain("Home");
}
```

`StubApi` z Task 4 odpowiada według metody i ścieżki oraz zapisuje żądania; `localStorage` przez `JSInterop.Setup`.

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.ClientTests.Components.WalletSwitcherTests`: FAIL.

- [ ] **Step 3: Implement components and pages**

`WalletState.InitializeAsync()` wołany w `MainLayout` po przejściu w tryb interaktywny (`RendererInfo.IsInteractive`)
dla zalogowanych. Wiersz w `More`: „Wallets” (ikona `wallet`).

- [ ] **Step 4: Run tests to verify they pass** – `WalletSwitcherTests`: PASS.

- [ ] **Step 5: Slice checkpoint**

Run: `dotnet test` (całość) i `$env:CI='true'; dotnet build BM2.slnx -c Release` → zielono, 0 ostrzeżeń. Demo: logowanie,
utworzenie pierwszego portfela, drugi portfel, przełączanie, zmiana nazwy, usunięcie; telefon 375 px i desktop,
jasny i ciemny motyw.

- [ ] **Step 6: Commit** – `git commit -m "Add wallet switcher, onboarding and wallets screen"`

## Plasterek 2 – konta

### Task 6: API kont i salda

**Files:**
- Create: `src/BM2.Contracts/Accounts/{AccountRequests,AccountValidators}.cs`, `src/BM2.Server/Features/Accounts/*.cs`
- Modify: `src/BM2.Server/Features/Wallets/{GetWallet,DeleteWallet}.cs`, `ApiEndpoints.cs`, `WalletEndpointAccessTests.cs`
- Test: `tests/BM2.IntegrationTests/Features/Accounts/AccountTests.cs`, `tests/BM2.UnitTests/Contracts/AccountValidatorTests.cs`

**Interfaces:**
- Consumes: `Account` (Task 2), `CurrentWallet`, `Validate<T>`, `ToProblem` (Task 3).
- Produces:
  - `CreateAccountRequest(string Name, string Currency, decimal OpeningBalance)`,
    `UpdateAccountRequest(string Name, string Currency, decimal OpeningBalance, bool IsArchived)` + walidatory
    (nazwa ≤ `Limits.AccountName`, waluta wspierana, saldo otwarcia: |x| ≤ `MaxAmount`, maks. 2 miejsca, może być ujemne).
  - `POST /api/wallets/{w}/accounts` (201 + `AccountItem`), `PUT …/accounts/{id}` (204), `DELETE …/accounts/{id}` (204;
    usuwa rekordy, w których konto jest po którejkolwiek stronie – od Task 11); wszystkie `ManageAccounts`.
    Konto spoza portfela z trasy → 404. Duplikat nazwy → 409 z `Field = "Name"`.
  - `AccountBalances.GetAsync(AppDbContext db, IReadOnlyCollection<Guid> accountIds, CancellationToken ct) : Task<Dictionary<Guid, decimal>>`
    – teraz `OpeningBalance`; Task 11 dodaje sumy rekordów.

- [ ] **Step 1: Write the failing tests**

```csharp
public class AccountTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact] public async Task Owner_creates_account_visible_in_snapshot() => snapshot.Accounts.Single().ShouldBe(new AccountItem(id, "Bank", "EUR", 100m, 100m, false, true));
    [Fact] public async Task Duplicate_name_in_wallet_returns_409() => response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    [Fact] public async Task Same_name_in_other_wallet_is_allowed() => response.StatusCode.ShouldBe(HttpStatusCode.Created);
    [Fact] public async Task Account_from_other_wallet_returns_404() { /* PUT /api/wallets/{mine}/accounts/{foreignAccount} */ response.StatusCode.ShouldBe(HttpStatusCode.NotFound); }
    [Fact] public async Task Archived_account_stays_in_snapshot_marked_archived() => snapshot.Accounts.Single().IsArchived.ShouldBeTrue();
    [Fact] public async Task Deleting_account_removes_its_shares() => (await db.AccountMembers.CountAsync()).ShouldBe(0);
}
```

Dopisz do `WalletEndpointAccessTests.Cases`: `POST /api/wallets/{w}/accounts` (Owner), `PUT …/accounts/{a}` (Owner),
`DELETE …/accounts/{a}` (Owner).

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.IntegrationTests.Features.Accounts.AccountTests`: FAIL.
- [ ] **Step 3: Implement requests, validators, endpoints and `AccountBalances`**
- [ ] **Step 4: Run tests to verify they pass** – `AccountTests`, `WalletEndpointAccessTests`, `AccountValidatorTests`: PASS.
- [ ] **Step 5: Commit** – `git commit -m "Add account endpoints and balances"`

### Task 7: UI kont – ekran Accounts, `Amount`, `MoneyInput`, karty na Home

**Files:**
- Create: `src/BM2.Client/Components/Amount.razor`, `src/BM2.Client/Components/Forms/MoneyInput.razor`,
  `src/BM2.Client/Pages/Accounts.razor`
- Modify: `src/BM2.Client/Pages/{Home,More}.razor`, `Icon.razor` (`bank`, `archive`), `components.css`, `motion.css`
- Test: `tests/BM2.ClientTests/Components/{AmountTests,MoneyInputTests}.cs`

**Interfaces:**
- Produces:
  - `Amount`: `decimal Value`, `string Currency`, `AmountSign Sign` (`Auto` – znak z wartości, `Plus`, `Minus`, `None`),
    `bool Large`; renderuje `<span class="amount amount--positive|negative">` z tekstem `+12.50 PLN` / `−12.50 PLN`
    (format `N2` w `CultureInfo.CurrentCulture`, minus U+2212), klasa `.num`.
  - `MoneyInput`: `decimal? Value`, `EventCallback<decimal?> ValueChanged`, `Expression<Func<decimal?>>? ValueExpression`,
    `string Label`, `string? Currency`, `bool Large`, `bool Autofocus`; `<input inputmode="decimal">`; parsuje kropkę
    i przecinek niezależnie od kultury (ostatni separator = dziesiętny, spacje i separatory tysięcy ignorowane), błąd
    przy > 2 miejscach.
  - `/accounts`: lista kont z saldem, „New account” w arkuszu (nazwa, waluta, saldo otwarcia), edycja, archiwizacja,
    usunięcie z potwierdzeniem; akcje tylko przy `Permissions.ManageAccounts`.
  - Home: karty kont (nazwa, `Amount` salda), suma per waluta nad kartami; liczby salda animowane przejściem
    (`.amount--animate`, CSS `@property`/`transition`, wyłączone przy reduced motion).

- [ ] **Step 1: Write the failing tests**

```csharp
public class AmountTests : BunitContext
{
    [Fact] public void Negative_value_uses_unicode_minus_and_negative_class() { CultureInfo.CurrentCulture = new("en-US"); var cut = Render<Amount>(p => p.Add(x => x.Value, -12.5m).Add(x => x.Currency, "PLN")); cut.Markup.ShouldContain("−12.50 PLN"); cut.Find("span").ClassList.ShouldContain("amount--negative"); }
    [Fact] public void Positive_value_has_plus() => cut.Markup.ShouldContain("+1,000.00 EUR");
}

public class MoneyInputTests : BunitContext
{
    [Theory]
    [InlineData("pl-PL", "12,50", "12.50")] [InlineData("pl-PL", "1 234,5", "1234.5")]
    [InlineData("en-US", "12.50", "12.50")] [InlineData("en-US", "1,234.50", "1234.50")]
    public void Parses_amount_in_culture(string culture, string typed, string expected) { CultureInfo.CurrentCulture = new(culture); /* type and change */ value.ShouldBe(decimal.Parse(expected, CultureInfo.InvariantCulture)); }

    [Fact] public void Rejects_more_than_two_decimals() => cut.Find(".field__error").TextContent.ShouldNotBeEmpty();
}
```

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.ClientTests.Components.MoneyInputTests`: FAIL.
- [ ] **Step 3: Implement components, Accounts page and Home cards**
- [ ] **Step 4: Run tests to verify they pass** – `AmountTests`, `MoneyInputTests`: PASS.
- [ ] **Step 5: Slice checkpoint** – pełna suita + build `CI=true`; demo: konta w PLN i EUR, saldo otwarcia, archiwizacja,
  usunięcie; Home pokazuje karty i sumy per waluta.
- [ ] **Step 6: Commit** – `git commit -m "Add accounts screen and amount components"`

## Plasterek 3 – kategorie i tagi

### Task 8: Kategorie i tagi – encje, API, domyślny zestaw

**Files:**
- Create: `src/BM2.Contracts/Categories/{CategoryType,CategoryContracts,CategoryValidators}.cs`,
  `src/BM2.Contracts/Tags/{TagContracts,TagValidators}.cs`
- Create: `src/BM2.Domain/Categories/Category.cs`, `src/BM2.Domain/Tags/Tag.cs`, `src/BM2.Domain/Wallets/DefaultCategories.cs`
- Create: `src/BM2.Server/Data/Configurations/{Category,Tag}Configuration.cs`, `src/BM2.Server/Features/{Categories,Tags}/*.cs`;
  migracja `AddCategoriesAndTags`
- Modify: `CreateWallet.cs` (domyślne kategorie), `GetWallet.cs`, `DeleteWallet.cs`, `WalletResponses.cs`, `AppDbContext.cs`,
  `TestData.cs`, `WalletEndpointAccessTests.cs`
- Test: `tests/BM2.IntegrationTests/Features/Categories/CategoryTests.cs`, `tests/BM2.IntegrationTests/Features/Tags/TagTests.cs`

**Interfaces:**
- Produces:
  - `enum CategoryType { Expense = 1, Income = 2 }`; `CategoryItem(Guid Id, string Name, CategoryType Type, bool IsArchived)`;
    `CreateCategoryRequest(string Name, CategoryType Type)`; `UpdateCategoryRequest(string Name, bool IsArchived)` (typ niezmienny);
    `TagItem(Guid Id, string Name, bool IsArchived)`; `CreateTagRequest(string Name)`; `UpdateTagRequest(string Name, bool IsArchived)`; walidatory.
  - `WalletSnapshot` + `IReadOnlyList<CategoryItem> Categories`, `IReadOnlyList<TagItem> Tags` (sortowane: aktywne, potem nazwa).
  - `Category.Create(Guid walletId, string name, CategoryType type)`, `void Update(string name, bool isArchived)`;
    `Tag.Create(Guid walletId, string name)`, `void Update(string name, bool isArchived)`.
  - `DefaultCategories.For(Guid walletId) : IEnumerable<Category>` – nazwy ze speca.
  - Endpointy (`ManageCategories`): `POST/PUT/DELETE /api/wallets/{w}/categories[/{id}]`, `POST/PUT/DELETE /api/wallets/{w}/tags[/{id}]`.
    Duplikat (kategoria: portfel + typ + nazwa; tag: portfel + nazwa) → 409 `Name`. `DELETE` kategorii z rekordami → 409
    (od Task 11; teraz zawsze można).
  - EF: `Category`/`Tag` → `Wallet` **NoAction**; indeksy unikalne jak wyżej.
  - `TestData`: `Task<Category> CategoryAsync(Guid walletId, string name = "Groceries", CategoryType type = CategoryType.Expense)`,
    `Task<Tag> TagAsync(Guid walletId, string name = "House")`.

- [ ] **Step 1: Write the failing tests**

```csharp
public class CategoryTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact] public async Task New_wallet_has_default_categories() { snapshot.Categories.Count(c => c.Type == CategoryType.Expense).ShouldBe(9); snapshot.Categories.Where(c => c.Type == CategoryType.Income).Select(c => c.Name).ShouldBe(["Other income", "Salary"]); }
    [Fact] public async Task Editor_creates_category() => response.StatusCode.ShouldBe(HttpStatusCode.Created);
    [Fact] public async Task Same_name_with_other_type_is_allowed() => response.StatusCode.ShouldBe(HttpStatusCode.Created);
    [Fact] public async Task Duplicate_name_and_type_returns_409() => response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    [Fact] public async Task Category_from_other_wallet_returns_404() => response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    [Fact] public async Task Limited_user_sees_categories_read_only() { snapshot.Categories.ShouldNotBeEmpty(); snapshot.Permissions.ManageCategories.ShouldBeFalse(); }
}

public class TagTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact] public async Task Editor_creates_and_archives_tag() => snapshot.Tags.Single().IsArchived.ShouldBeTrue();
    [Fact] public async Task Duplicate_tag_returns_409() => response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    [Fact] public async Task Tag_from_other_wallet_returns_404() => response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
}
```

Dopisz przypadki dostępu: kategorie i tagi `POST`/`PUT`/`DELETE` z minimum `Editor`.

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.IntegrationTests.Features.Categories.CategoryTests`: FAIL.
- [ ] **Step 3: Implement entities, migration and endpoints** (`dotnet ef migrations add AddCategoriesAndTags …`)
- [ ] **Step 4: Run tests to verify they pass** – `CategoryTests`, `TagTests`, `WalletEndpointAccessTests`, `WalletTests`: PASS.
- [ ] **Step 5: Commit** – `git commit -m "Add categories and tags with default category set"`

### Task 9: UI kategorii i tagów

**Files:**
- Create: `src/BM2.Client/Pages/{Categories,Tags}.razor`, `src/BM2.Client/Components/SegmentedControl.razor`
- Modify: `More.razor`, `Icon.razor` (`category`, `tag`), `components.css`
- Test: `tests/BM2.ClientTests/Components/SegmentedControlTests.cs`

**Interfaces:**
- Produces: `SegmentedControl<TValue>`: `IReadOnlyList<(TValue Value, string Label)> Options`, `TValue Value`,
  `EventCallback<TValue> ValueChanged`, `string Label` (dla `role="radiogroup"` + `aria-label`); przyciski `role="radio"`
  z `aria-checked`, strzałki zmieniają wybór. `/categories`: przełącznik Expenses/Income, lista, dodanie/edycja w arkuszu,
  archiwizacja (sekcja „Archived” zwinięta), usunięcie (409 → toast „This category has records – archive it instead.”).
  `/tags`: lista, dodanie/edycja, archiwizacja, usunięcie; podpowiedź pod listą: „Use tags for projects like a house
  build or a car.”

- [ ] **Step 1: Write the failing tests**

```csharp
public class SegmentedControlTests : BunitContext
{
    [Fact] public void Marks_selected_option_checked() => cut.Find("[aria-checked=true]").TextContent.ShouldBe("Expenses");
    [Fact] public void Arrow_right_selects_next_option() { cut.Find("[aria-checked=true]").KeyDown(Key.Right); selected.ShouldBe(CategoryType.Income); }
}
```

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.ClientTests.Components.SegmentedControlTests`: FAIL.
- [ ] **Step 3: Implement component and pages**
- [ ] **Step 4: Run tests to verify they pass**
- [ ] **Step 5: Slice checkpoint** – pełna suita + build `CI=true`; demo: domyślne kategorie, dodanie, duplikat (komunikat
  przy polu), archiwizacja, tagi.
- [ ] **Step 6: Commit** – `git commit -m "Add categories and tags screens"`

## Plasterek 4 – rekordy

### Task 10: Rekord w domenie i przeliczenie kursu

**Files:**
- Create: `src/BM2.Contracts/Records/{RecordType,RecordStatus,ExchangeMath}.cs`
- Create: `src/BM2.Domain/Records/{Record,RecordTag,RecordDraft,RecordContext}.cs`
- Test: `tests/BM2.UnitTests/Records/RecordRulesTests.cs`, `tests/BM2.UnitTests/Contracts/ExchangeMathTests.cs`

**Interfaces:**
- Produces:
  - `enum RecordType { Expense = 1, Income = 2, Transfer = 3 }`, `enum RecordStatus { Planned = 1, Pending = 2, Completed = 3 }`.
  - `static class ExchangeMath { decimal ToAccountAmount(decimal amount, decimal rate); decimal RateOf(decimal amount, decimal accountAmount); }`
    – `ToAccountAmount` = `Math.Round(amount * rate, 2, MidpointRounding.AwayFromZero)`; `RateOf` = zaokrąglenie do 6 miejsc.
  - `sealed record RecordDraft(RecordType Type, RecordStatus Status, DateOnly OccurredOn, Guid AccountId, decimal Amount, string Currency, decimal AccountAmount, Guid? CategoryId, Guid? TransferAccountId, decimal? TransferAmount, string? Name, IReadOnlyList<Guid> TagIds)`.
  - `sealed record AccountRef(Guid Id, Guid WalletId, string Currency, bool IsArchived)`,
    `sealed record CategoryRef(Guid Id, Guid WalletId, CategoryType Type, bool IsArchived)`, `sealed record TagRef(Guid Id, Guid WalletId)`,
    `sealed record RecordContext(Guid WalletId, AccountRef? Account, AccountRef? TransferAccount, CategoryRef? Category, IReadOnlyList<TagRef> Tags)`
    (null = nie znaleziono).
  - `Record`: właściwości jak w specu + nawigacje `Account Account`, `Account? TransferAccount`, `Category? Category`,
    `IReadOnlyCollection<RecordTag> Tags`; `static Result<Record> Create(RecordDraft d, RecordContext ctx, Guid userId, DateTimeOffset now)`,
    `Result Update(RecordDraft d, RecordContext ctx, DateTimeOffset now)`.
  - `RecordTag(Guid RecordId, Guid TagId)`.
- Reguły (kolejność błędów dowolna, `Field` = nazwa właściwości draftu): konto nie znalezione lub z innego portfela →
  `NotFound`; konto zarchiwizowane → `Validation("AccountId")` tylko przy tworzeniu albo zmianie konta; transfer: brak/obce
  konto docelowe → `NotFound`, to samo konto → `Validation("TransferAccountId")`, `Currency` ≠ waluta konta źródłowego →
  `Validation("Currency")`, kategoria podana → `Validation("CategoryId")`; `TransferAmount` normalizowane do `Amount`, gdy
  waluty kont równe, w innym razie wymagane (`Validation("TransferAmount")`); wydatek/przychód: kategoria wymagana,
  z tego portfela (`NotFound` w innym razie), typ zgodny (`Validation("CategoryId")`), zarchiwizowana → błąd tylko przy
  tworzeniu albo zmianie kategorii; `AccountAmount` normalizowane do `Amount`, gdy `Currency` = waluta konta; tagi spoza
  portfela → `NotFound`; nazwa przycinana, pusta → `null`.

- [ ] **Step 1: Write the failing tests**

```csharp
public class RecordRulesTests
{
    // Helpers build RecordContext with wallet W, PLN account A, EUR account B, expense category C.
    [Fact] public void Expense_requires_category() => Create(Expense() with { CategoryId = null }).Error!.Field.ShouldBe("CategoryId");
    [Fact] public void Category_type_must_match_record_type() => Create(Income() with { CategoryId = ExpenseCategory.Id }).Error!.Field.ShouldBe("CategoryId");
    [Fact] public void Category_from_other_wallet_is_not_found() => Create(Expense(), category: ForeignCategory).Error!.Kind.ShouldBe(ErrorKind.NotFound);
    [Fact] public void Same_currency_normalizes_account_amount() => Create(Expense() with { Amount = 10m, AccountAmount = 99m }).Value.AccountAmount.ShouldBe(10m);
    [Fact] public void Foreign_currency_keeps_account_amount() => Create(Expense() with { Currency = "EUR", Amount = 10m, AccountAmount = 43.1m }).Value.AccountAmount.ShouldBe(43.1m);
    [Fact] public void Transfer_to_same_account_is_invalid() => Create(Transfer(A, A)).Error!.Field.ShouldBe("TransferAccountId");
    [Fact] public void Transfer_must_use_source_account_currency() => Create(Transfer(A, B) with { Currency = "EUR" }).Error!.Field.ShouldBe("Currency");
    [Fact] public void Transfer_between_currencies_requires_target_amount() => Create(Transfer(A, B) with { TransferAmount = null }).Error!.Field.ShouldBe("TransferAmount");
    [Fact] public void Transfer_with_same_currency_normalizes_target_amount() => Create(Transfer(A, A2) with { Amount = 50m, TransferAmount = 1m }).Value.TransferAmount.ShouldBe(50m);
    [Fact] public void Transfer_cannot_have_category() => Create(Transfer(A, B) with { CategoryId = ExpenseCategory.Id }).Error!.Field.ShouldBe("CategoryId");
    [Fact] public void New_record_on_archived_account_is_invalid() => Create(Expense(), account: ArchivedAccount).Error!.Field.ShouldBe("AccountId");
    [Fact] public void Editing_record_on_archived_account_without_moving_it_succeeds() => existingOnArchived.Update(Expense() with { Amount = 5m }, ctxArchivedAccount, Now).IsSuccess.ShouldBeTrue();
    [Fact] public void Editing_record_keeps_archived_category() => existingWithArchivedCategory.Update(Expense() with { Amount = 5m }, ctxArchivedCategory, Now).IsSuccess.ShouldBeTrue();
    [Fact] public void Switching_to_archived_category_is_invalid() => existing.Update(Expense() with { CategoryId = ArchivedCategory.Id }, ctxArchivedCategory, Now).Error!.Field.ShouldBe("CategoryId");
    [Fact] public void Tags_from_other_wallet_are_not_found() => Create(Expense() with { TagIds = [ForeignTag.Id] }, tags: [ForeignTag]).Error!.Kind.ShouldBe(ErrorKind.NotFound);
    [Fact] public void Blank_name_becomes_null() => Create(Expense() with { Name = "  " }).Value.Name.ShouldBeNull();
}

public class ExchangeMathTests
{
    [Fact] public void Converts_with_rounding_away_from_zero() => ExchangeMath.ToAccountAmount(10.01m, 4.3125m).ShouldBe(43.17m);
    [Fact] public void Derives_rate_from_amounts() => ExchangeMath.RateOf(10m, 43.17m).ShouldBe(4.317m);
}
```

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.UnitTests.Records.RecordRulesTests`: FAIL.
- [ ] **Step 3: Implement record domain types**
- [ ] **Step 4: Run tests to verify they pass** – `RecordRulesTests`, `ExchangeMathTests`: PASS.
- [ ] **Step 5: Commit** – `git commit -m "Add record domain rules and exchange math"`

### Task 11: API rekordów (CRUD) i salda z rekordów

**Files:**
- Create: `src/BM2.Contracts/Records/{SaveRecordRequest,SaveRecordRequestValidator,RecordItem}.cs`
- Create: `src/BM2.Server/Data/Configurations/{Record,RecordTag}Configuration.cs`, migracja `AddRecords`
- Create: `src/BM2.Server/Features/Records/{RecordEndpoints,RecordContextLoader,GetRecord,CreateRecord,UpdateRecord,DeleteRecord}.cs`
- Modify: `AccountBalances.cs`, `DeleteAccount.cs`, `DeleteCategory.cs`, `DeleteWallet.cs`, `UpdateAccount.cs` (`hasRecords`),
  `AppDbContext.cs`, `TestData.cs`, `WalletEndpointAccessTests.cs`
- Test: `tests/BM2.IntegrationTests/Features/Records/RecordTests.cs`, `tests/BM2.UnitTests/Contracts/SaveRecordRequestValidatorTests.cs`

**Interfaces:**
- Consumes: Task 10, `CurrentWallet.Access` (`CanReadRecord`, `CanWriteRecord`).
- Produces:
  - `SaveRecordRequest` – te same pola co `RecordDraft` (create i update); walidator: kwoty `MoneyRules.IsValidAmount`
    (`TransferAmount`, gdy podana), waluta wspierana, nazwa ≤ `Limits.RecordName`, `TagIds` bez duplikatów, enumy zdefiniowane.
  - `RecordItem(Guid Id, RecordType Type, RecordStatus Status, DateOnly OccurredOn, Guid AccountId, decimal Amount, string Currency, decimal AccountAmount, Guid? CategoryId, Guid? TransferAccountId, string? TransferAccountName, decimal? TransferAmount, string? Name, IReadOnlyList<Guid> TagIds)`.
  - `RecordContextLoader.LoadAsync(AppDbContext db, Guid walletId, RecordDraft draft, CancellationToken ct) : Task<RecordContext>`.
  - `GET/PUT/DELETE /api/wallets/{w}/records/{id}`, `POST /api/wallets/{w}/records` (201 + `RecordItem`).
    Odczyt: rekord nieczytelny dla użytkownika → 404. Zapis: `CanWriteRecord` dla nowego stanu **i** (przy `PUT`/`DELETE`)
    dla stanu przed zmianą; czytelny, ale bez zapisu → 403.
  - EF: `Record` → `Account` i `TransferAccount` NoAction, → `Category` NoAction; `RecordTag` PK (`RecordId`, `TagId`),
    Cascade do obu; `CreatedByUserId` bez FK; indeksy ze speca (filtrowany `TransferAccountId IS NOT NULL`).
  - `AccountBalances.GetAsync`: `OpeningBalance` + (zrealizowane, `AccountId ∈ ids`) Σ(`Income` ? `+AccountAmount` : `−AccountAmount`)
    + (zrealizowane transfery, `TransferAccountId ∈ ids`) Σ`TransferAmount` – dwa zapytania z `GroupBy`.
  - `DeleteAccount` usuwa rekordy `AccountId == id || TransferAccountId == id`; `DeleteCategory` z rekordami → 409;
    `DeleteWallet` zaczyna od rekordów portfela.
  - `TestData.RecordAsync(Guid accountId, decimal amount, RecordType type = RecordType.Expense, Guid? categoryId = null, DateOnly? occurredOn = null, RecordStatus status = RecordStatus.Completed, Guid? transferAccountId = null, decimal? transferAmount = null, Guid[]? tagIds = null) : Task<Record>`.

- [ ] **Step 1: Write the failing tests**

```csharp
public class RecordTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact] public async Task Editor_adds_expense_and_balance_drops() => snapshot.Accounts.Single().Balance.ShouldBe(100m - 12.5m);
    [Fact] public async Task Planned_record_does_not_change_balance() => snapshot.Accounts.Single().Balance.ShouldBe(100m);
    [Fact] public async Task Transfer_moves_money_between_currencies() { /* PLN A → EUR B, Amount 430, TransferAmount 100 */ balances[A].ShouldBe(-430m); balances[B].ShouldBe(100m); }
    [Fact] public async Task Foreign_currency_expense_uses_account_amount_for_balance() => balance.ShouldBe(-43.17m);
    [Fact] public async Task Viewer_cannot_add_record() => response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    [Fact] public async Task Shared_account_editor_adds_record_on_shared_account() => response.StatusCode.ShouldBe(HttpStatusCode.Created);
    [Fact] public async Task Shared_account_editor_cannot_read_record_of_other_account() => response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    [Fact] public async Task Shared_account_editor_cannot_move_record_to_other_account() => response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    [Fact] public async Task Shared_account_editor_cannot_create_transfer_to_unshared_account() => response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    [Fact] public async Task Shared_account_viewer_sees_transfer_counterpart_name() => item.TransferAccountName.ShouldBe("Savings");
    [Fact] public async Task Category_from_other_wallet_returns_404() => response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    [Fact] public async Task Wrong_category_type_returns_validation_problem() => problem!.Errors.ShouldContainKey("CategoryId");
    [Fact] public async Task Deleting_category_with_records_returns_409() => response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    [Fact] public async Task Deleting_account_removes_transfers_and_updates_other_balance() => balances[B].ShouldBe(0m);
    [Fact] public async Task Account_currency_cannot_change_once_it_has_records() => problem!.Errors.ShouldContainKey("Currency");
    [Fact] public async Task Deleting_tag_keeps_record() { (await db.Records.CountAsync()).ShouldBe(1); (await db.Set<RecordTag>().CountAsync()).ShouldBe(0); }
    [Fact] public async Task Created_record_stores_author_and_utc_timestamp() { record.CreatedByUserId.ShouldBe(me.Id); record.CreatedAt.Offset.ShouldBe(TimeSpan.Zero); }
}
```

Dopisz przypadki dostępu: `GET …/records/{r}` (Viewer), `POST …/records` (Editor), `PUT …/records/{r}` (Editor),
`DELETE …/records/{r}` (Editor).

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.IntegrationTests.Features.Records.RecordTests`: FAIL.
- [ ] **Step 3: Implement contracts, EF configuration, migration and endpoints** (`dotnet ef migrations add AddRecords …`)
- [ ] **Step 4: Run tests to verify they pass** – `RecordTests`, `AccountTests`, `CategoryTests`, `TagTests`, `WalletTests`,
  `WalletEndpointAccessTests`, `SaveRecordRequestValidatorTests`: PASS.
- [ ] **Step 5: Commit** – `git commit -m "Add record endpoints and balances from records"`

### Task 12: Lista rekordów – filtry, keyset, podsumowanie

**Files:**
- Create: `src/BM2.Contracts/Records/{RecordPage,RecordFilter,RecordSummary}.cs`
- Create: `src/BM2.Server/Features/Records/{RecordQueries,RecordCursor,ListRecords,GetRecordSummary}.cs`
- Modify: `RecordEndpoints.cs`, `WalletEndpointAccessTests.cs`
- Test: `tests/BM2.IntegrationTests/Features/Records/RecordListTests.cs`, `tests/BM2.UnitTests/Contracts/RecordFilterTests.cs`

**Interfaces:**
- Produces:
  - `RecordPage(IReadOnlyList<RecordItem> Items, string? NextCursor)`;
    `CurrencyTotals(string Currency, decimal Income, decimal Expense)`; `RecordSummary(IReadOnlyList<CurrencyTotals> Totals)`.
  - `sealed class RecordFilter` (init-only, binding `[AsParameters]`): `DateOnly? From`, `DateOnly? To`, `Guid? AccountId`,
    `Guid? CategoryId`, `Guid? TagId`, `RecordType? Type`, `RecordStatus? Status`, `string? Search`, `string? Cursor`;
    `string ToQueryString()` (tylko ustawione pola, daty `yyyy-MM-dd`, wartości escapowane, kolejność jak właściwości).
  - `RecordQueries.Visible(this IQueryable<Record> q, IReadOnlyCollection<Guid> readable)` (`AccountId ∈ readable || TransferAccountId ∈ readable`),
    `RecordQueries.Apply(this IQueryable<Record> q, RecordFilter f)` (`OccurredOn >= From`, `< To`; konto po obu stronach;
    `Search` → `Name.Contains`), `RecordQueries.ToItem` (projekcja `Expression<Func<Record, RecordItem>>`).
  - `RecordCursor.Encode(DateOnly occurredOn, DateTimeOffset createdAt, Guid id) : string` (base64url),
    `RecordCursor.TryDecode(string? value, out (DateOnly, DateTimeOffset, Guid) cursor) : bool`.
  - `GET /api/wallets/{w}/records` → `RecordPage`, 50 pozycji, porządek `OccurredOn` desc, `CreatedAt` desc, `Id` desc;
    zły kursor → 400 (`Field = "Cursor"`); `AccountId` nieczytelne → 404.
    `GET /api/wallets/{w}/records/summary` → `RecordSummary` (bez transferów, grupowanie po walucie konta, sortowane po walucie).

Warunek keyset (dla kursora `c`):

```csharp
q.Where(r => r.OccurredOn < c.OccurredOn
    || (r.OccurredOn == c.OccurredOn && (r.CreatedAt < c.CreatedAt
        || (r.CreatedAt == c.CreatedAt && r.Id.CompareTo(c.Id) < 0))))
```

- [ ] **Step 1: Write the failing tests**

```csharp
public class RecordListTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact] public async Task Month_range_includes_last_day_and_excludes_next_month() => page.Items.Select(i => i.OccurredOn).ShouldBe([new DateOnly(2026, 10, 31), new DateOnly(2026, 10, 1)]);
    [Fact] public async Task Paging_returns_every_record_once_in_order() { /* 120 records, some on the same day */ all.Count.ShouldBe(120); all.ShouldBe(all.OrderByDescending(i => i.OccurredOn).ToList()); all.Select(i => i.Id).Distinct().Count().ShouldBe(120); }
    [Fact] public async Task Tag_filter_without_dates_spans_all_time() => page.Items.Count.ShouldBe(3);
    [Fact] public async Task Account_filter_includes_incoming_transfers() => page.Items.ShouldContain(i => i.TransferAccountId == accountB.Id);
    [Fact] public async Task Search_matches_name_fragment() => page.Items.Single().Name.ShouldBe("Bakery Lidl");
    [Fact] public async Task Shared_account_user_lists_only_its_records() => page.Items.ShouldAllBe(i => i.AccountId == shared.Id || i.TransferAccountId == shared.Id);
    [Fact] public async Task Unreadable_account_filter_returns_404() => response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    [Fact] public async Task Invalid_cursor_returns_400() => response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    [Fact] public async Task Summary_sums_per_account_currency_without_transfers() => summary.Totals.ShouldBe([new CurrencyTotals("EUR", 0m, 20m), new CurrencyTotals("PLN", 5000m, 312.5m)]);
    [Fact] public async Task Summary_of_shared_account_user_ignores_other_accounts() => summary.Totals.Single().Expense.ShouldBe(12.5m);
}

public class RecordFilterTests
{
    [Fact] public void Query_string_contains_only_set_fields() => new RecordFilter { From = new(2026, 10, 1), Search = "a&b" }.ToQueryString().ShouldBe("?from=2026-10-01&search=a%26b");
}
```

Dopisz przypadki dostępu: `GET …/records` i `GET …/records/summary` (Viewer).

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.IntegrationTests.Features.Records.RecordListTests`: FAIL.
- [ ] **Step 3: Implement filter, cursor, queries and endpoints**
- [ ] **Step 4: Run tests to verify they pass** – `RecordListTests`, `RecordFilterTests`, `WalletEndpointAccessTests`: PASS.
- [ ] **Step 5: Commit** – `git commit -m "Add record list with filters, keyset paging and summary"`

### Task 13: Formularz rekordu – szybkie dodawanie, edycja, kopiowanie

**Files:**
- Create: `src/BM2.Client/Components/ChipGroup.razor`, `src/BM2.Client/Components/Forms/DateField.razor`,
  `src/BM2.Client/Components/Records/RecordForm.razor`, `src/BM2.Client/Pages/RecordEditor.razor`
- Delete: `src/BM2.Client/Pages/QuickAdd.razor`
- Modify: `Icon.razor` (`transfer`, `copy`, `calendar`), `components.css`, `motion.css`
- Test: `tests/BM2.ClientTests/Components/RecordFormTests.cs`

**Interfaces:**
- Consumes: `WalletState` (migawka: konta, kategorie, tagi, uprawnienia), `MoneyInput`, `SegmentedControl`, `ApiClient`,
  `ToastService`, `ExchangeMath`, `SaveRecordRequestValidator`.
- Produces:
  - `ChipGroup<TValue>`: `IReadOnlyList<(TValue Value, string Label)> Options`, `bool Multiple`,
    `IReadOnlyCollection<TValue> Selected`, `EventCallback<IReadOnlyCollection<TValue>> SelectedChanged`, `string Label`;
    przyciski z `aria-pressed`, przewijane poziomo na telefonie.
  - `DateField`: `DateOnly Value`/`ValueChanged`, `Label`; `<input type="date">` + skróty „Today”, „Yesterday” liczone
    z `DateTime.Now` klienta.
  - `RecordForm`: `RecordItem? Initial`, `bool IsCopy`, `EventCallback<SaveRecordRequest> OnSubmit`, `ApiProblem? ServerProblem`.
    Kolejność: typ (Expense/Income/Transfer) → kwota (`MoneyInput Large Autofocus`) → kategorie danego typu (chipy,
    tylko niezarchiwizowane i bieżąca kategorii edytowanego rekordu) albo konto docelowe (transfer) → konto (tylko z
    `CanWrite`, niezarchiwizowane, domyślnie ostatnio używane, potem pierwsze) → data (dzisiaj) → „More”: nazwa, tagi,
    status, waluta. Zmiana typu czyści kategorię. Waluta ≠ waluta konta → pola „Rate” i „Amount in {account currency}”
    przeliczane wzajemnie (`ExchangeMath`); transfer między walutami → „Amount received in {currency}”. Kopia: wszystko
    z `Initial` poza datą (dzisiaj) i statusem (`Completed`); kwota zaznaczona (`select()` przez JS
    `HTMLInputElement.select`).
  - `RecordEditor` – trasy `records/new` (opcjonalne `?copy={id:guid}`) i `records/{id:guid}`: ładuje rekord
    (`GET …/records/{id}`), zapis `POST`/`PUT`, po sukcesie `SetLastAccountAsync`, `WalletState.RefreshAsync()`, toast
    „Saved” i powrót (`history.back` przez `NavigationManager` do `records`); w edycji przyciski „Copy” (→
    `records/new?copy={id}`) i „Delete” (`ConfirmDialog`). Brak prawa zapisu → pusty stan „You can't add records in this
    wallet.” Wygląd: pełnoekranowy arkusz z animacją `sheet-in`.

- [ ] **Step 1: Write the failing tests**

```csharp
public class RecordFormTests : BunitContext
{
    [Fact] public void Expense_shows_only_active_expense_categories() => chips.Select(c => c.TextContent).ShouldBe(["Groceries", "Transport"]);
    [Fact] public void Switching_type_clears_selected_category() { SelectChip("Groceries"); SelectType("Income"); cut.FindAll("[aria-pressed=true]").ShouldBeEmpty(); }
    [Fact] public void Defaults_to_last_used_account_and_today() { SelectedAccount().ShouldBe(lastUsed.Id); DateValue().ShouldBe(DateOnly.FromDateTime(DateTime.Now)); }
    [Fact] public void Foreign_currency_reveals_rate_and_converts() { SetCurrency("EUR"); SetAmount("10"); SetRate("4.3125"); AccountAmountInput().GetAttribute("value").ShouldBe(43.13m.ToString("N2", CultureInfo.CurrentCulture)); }
    [Fact] public void Transfer_hides_categories_and_lists_other_accounts_as_target() { SelectType("Transfer"); cut.FindAll(".chip-group--categories").ShouldBeEmpty(); TargetOptions().ShouldNotContain(SelectedAccount()); }
    [Fact] public void Submits_request_with_selected_values() => submitted.ShouldBe(new SaveRecordRequest(RecordType.Expense, RecordStatus.Completed, today, cash.Id, 12.5m, "PLN", 12.5m, groceries.Id, null, null, null, []), new RequestComparer());
    [Fact] public void Copy_keeps_fields_but_uses_today_and_completed() { submitted.OccurredOn.ShouldBe(today); submitted.Status.ShouldBe(RecordStatus.Completed); submitted.CategoryId.ShouldBe(source.CategoryId); }
    [Fact] public void Missing_amount_shows_error_and_does_not_submit() { cut.Find(".field__error").TextContent.ShouldNotBeEmpty(); submitted.ShouldBeNull(); }
}
```

(`RequestComparer` porównuje też zawartość `TagIds`.)

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.ClientTests.Components.RecordFormTests`: FAIL.
- [ ] **Step 3: Implement components, form and editor page**
- [ ] **Step 4: Run tests to verify they pass** – `RecordFormTests`, `ShellComponentTests` (FAB nadal prowadzi do `records/new`): PASS.
- [ ] **Step 5: Commit** – `git commit -m "Add record form for quick add, edit and copy"`

### Task 14: Ekran Records i Home

**Files:**
- Create: `src/BM2.Client/Components/Records/{RecordGrouping.cs,RecordList.razor,RecordFilterSheet.razor}`
- Modify: `src/BM2.Client/Pages/{Records,Home}.razor`, `Icon.razor` (`arrow-left`, `arrow-right`, `filter`, `search`),
  `components.css`, `motion.css`
- Test: `tests/BM2.ClientTests/Components/{RecordGroupingTests,RecordListTests}.cs`

**Interfaces:**
- Produces:
  - `sealed record RecordDay(DateOnly Date, IReadOnlyList<RecordItem> Items, IReadOnlyList<(string Currency, decimal Net)> Totals)`;
    `static class RecordGrouping { IReadOnlyList<RecordDay> ByDay(IEnumerable<RecordItem> items, Func<Guid, string?> accountCurrency); }`
    – dni malejąco, kolejność rekordów zachowana, `Net` = Σ przychodów − Σ wydatków w walucie konta (`AccountAmount`),
    transfery pominięte; konto nieznane (niedostępne) – pominięte w sumie.
  - `RecordList`: `IReadOnlyList<RecordItem> Items`, `WalletSnapshot Wallet`; sticky nagłówki dni (`<h2>` z datą
    `dddd, d MMMM` i sumą dnia), wiersz: ikona typu, nazwa (albo nazwa kategorii / „Transfer to {konto}”), konto
    i tagi pod spodem, `Amount` (`Minus` dla wydatku i transferu wychodzącego, `Plus` dla przychodu i przychodzącego),
    etykieta „Planned”/„Pending”; link do `records/{id}`; kaskada wejścia wierszy (`--i` jako opóźnienie, maks. 10).
  - `RecordFilterSheet`: `RecordFilter Value`, `EventCallback<RecordFilter> ValueChanged`, `WalletSnapshot Wallet`.
  - `/records`: nagłówek z miesiącem (`MMMM yyyy`) i strzałkami, przełącznik „All time”; wyszukiwarka (debounce 300 ms);
    przycisk filtrów z licznikiem; aktywne filtry jako chipy z „×”; pasek podsumowania (`GET …/records/summary`);
    doładowanie przez `Virtualize`/obserwację końca listy z `NextCursor`; filtry i miesiąc w query stringu (odświeżenie
    zachowuje widok).
  - Home: sumy per waluta + karty kont (Task 7), „This month” (przychody/wydatki z podsumowania z `Status = Completed`
    i zakresem bieżącego miesiąca), 5 ostatnich rekordów (`RecordList`), link „All records”.

- [ ] **Step 1: Write the failing tests**

```csharp
public class RecordGroupingTests
{
    [Fact] public void Groups_by_day_descending_with_net_per_account_currency() => days.Select(d => (d.Date, d.Totals.Single())).ShouldBe([(Oct31, ("PLN", -12.5m)), (Oct30, ("PLN", 4987.5m))]);
    [Fact] public void Transfers_do_not_change_day_total() => days.Single().Totals.ShouldBeEmpty();
    [Fact] public void Separate_totals_for_each_currency() => days.Single().Totals.ShouldBe([("EUR", -20m), ("PLN", -10m)], ignoreOrder: true);
}

public class RecordListTests : BunitContext
{
    [Fact] public void Expense_shows_minus_and_category_name_when_unnamed() { row.TextContent.ShouldContain("Groceries"); row.TextContent.ShouldContain("−12.50 PLN"); }
    [Fact] public void Planned_record_has_text_label() => row.TextContent.ShouldContain("Planned");
    [Fact] public void Incoming_transfer_for_shared_account_shows_plus_and_counterpart() { row.TextContent.ShouldContain("+100.00 EUR"); row.TextContent.ShouldContain("Savings"); }
}
```

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.ClientTests.Components.RecordGroupingTests`: FAIL.
- [ ] **Step 3: Implement grouping, list, filter sheet, Records page and Home**
- [ ] **Step 4: Run tests to verify they pass** – `RecordGroupingTests`, `RecordListTests`: PASS.
- [ ] **Step 5: Slice checkpoint** – pełna suita + build `CI=true`; demo: wydatek w < 5 s z FAB, przychód, transfer PLN → EUR,
  wydatek w EUR na koncie PLN (kurs), lista miesiąca z sumami dni, filtr tagu „All time” z podsumowaniem, edycja, kopia,
  usunięcie, salda na Home zgodne z rekordami; telefon i desktop, oba motywy.
- [ ] **Step 6: Commit** – `git commit -m "Add records screen and home overview"`

## Plasterek 5 – współdzielenie

### Task 15: API członków i udostępnień kont

**Files:**
- Create: `src/BM2.Contracts/Members/MemberContracts.cs`, `src/BM2.Server/Features/Members/*.cs`
- Modify: `ApiEndpoints.cs`, `WalletEndpointAccessTests.cs`
- Test: `tests/BM2.IntegrationTests/Features/Members/MemberTests.cs`

**Interfaces:**
- Consumes: `Wallet.ChangeMemberRole/RemoveMember`, `Account.ChangeShareRole/RemoveShare` (Task 2).
- Produces:
  - `MemberItem(Guid UserId, string Email, MemberRole Role, bool IsMe)`,
    `AccountShareItem(Guid AccountId, string AccountName, Guid UserId, string Email, MemberRole Role, bool IsMe)`,
    `MembersResponse(IReadOnlyList<MemberItem> Members, IReadOnlyList<AccountShareItem> AccountShares, IReadOnlyList<InvitationItem> Invitations)`
    (`InvitationItem` definiuje Task 16 – do tego czasu lista zawsze pusta, typ dodaj teraz w `Invitations/InvitationContracts.cs`:
    `InvitationItem(Guid Id, MemberRole Role, Guid? AccountId, string? AccountName, DateTimeOffset ExpiresAt)`),
    `UpdateRoleRequest(MemberRole Role)` + walidator (enum zdefiniowany).
  - `GET /api/wallets/{w}/members` (`ViewMembers`; zaproszenia tylko przy `ManageMembers`),
    `PUT /api/wallets/{w}/members/{userId}` (`ManageMembers`), `DELETE /api/wallets/{w}/members/{userId}` (`ManageMembers`
    albo `userId` = ja), `PUT /api/wallets/{w}/accounts/{a}/members/{userId}` (`ManageMembers`; rola Owner → 400),
    `DELETE /api/wallets/{w}/accounts/{a}/members/{userId}` (`ManageMembers` albo ja). Ostatni Owner → 409.

- [ ] **Step 1: Write the failing tests**

```csharp
public class MemberTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact] public async Task Viewer_sees_members_without_invitations() { members.Members.Count.ShouldBe(2); members.Invitations.ShouldBeEmpty(); }
    [Fact] public async Task Owner_promotes_editor_to_owner() => role.ShouldBe(MemberRole.Owner);
    [Fact] public async Task Last_owner_cannot_demote_self() => response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    [Fact] public async Task Member_can_leave_wallet() { response.StatusCode.ShouldBe(HttpStatusCode.NoContent); (await GetAsMember("/api/wallets/{w}")).StatusCode.ShouldBe(HttpStatusCode.NotFound); }
    [Fact] public async Task Editor_cannot_remove_other_member() => response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    [Fact] public async Task Shared_account_user_can_leave_share() => response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    [Fact] public async Task Shared_account_user_cannot_list_members() => response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    [Fact] public async Task Account_share_cannot_be_owner() => response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    [Fact] public async Task Removing_unknown_member_returns_404() => response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
}
```

Dopisz przypadki dostępu: `GET …/members` (Viewer), `PUT …/members/{u}` (Owner), `PUT …/accounts/{a}/members/{u}` (Owner).
(`DELETE` członka ma wyjątek „ja” – pokrywają go testy powyżej, nie teoria.)

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.IntegrationTests.Features.Members.MemberTests`: FAIL.
- [ ] **Step 3: Implement contracts and endpoints**
- [ ] **Step 4: Run tests to verify they pass** – `MemberTests`, `WalletEndpointAccessTests`: PASS.
- [ ] **Step 5: Commit** – `git commit -m "Add member and account share management"`

### Task 16: Zaproszenia – domena i API

**Files:**
- Create: `src/BM2.Domain/Invitations/{Invitation,InvitationToken}.cs`,
  `src/BM2.Contracts/Invitations/CreateInvitationRequestValidator.cs` (+ typy w `InvitationContracts.cs`)
- Create: `src/BM2.Server/Data/Configurations/InvitationConfiguration.cs`, migracja `AddInvitations`,
  `src/BM2.Server/Features/Invitations/*.cs`
- Modify: `ListMembers.cs` (oczekujące zaproszenia), `DeleteWallet.cs`, `AppDbContext.cs`, `ApiEndpoints.cs`, `WalletEndpointAccessTests.cs`
- Test: `tests/BM2.UnitTests/Invitations/InvitationTests.cs`, `tests/BM2.IntegrationTests/Features/Invitations/InvitationTests.cs`

**Interfaces:**
- Produces:
  - `static class InvitationToken { (string Token, byte[] Hash) Generate(); bool TryHash(string? token, out byte[] hash); }`
    – 32 bajty z `RandomNumberGenerator`, token `Base64Url.EncodeToString`, hash `SHA256.HashData(bajty tokenu)`;
    `TryHash` = `false` dla tokenu niebędącego poprawnym base64url 32 bajtów.
  - `Invitation`: `Guid Id, WalletId; Guid? AccountId; MemberRole Role; byte[] TokenHash; Guid CreatedByUserId;
    DateTimeOffset CreatedAt, ExpiresAt; Guid? AcceptedByUserId; DateTimeOffset? AcceptedAt, RevokedAt`;
    `static readonly TimeSpan Lifetime = TimeSpan.FromDays(7)`;
    `static (Invitation Invitation, string Token) Create(Guid walletId, Guid? accountId, MemberRole role, Guid createdBy, DateTimeOffset now)`;
    `Result CheckUsable(DateTimeOffset now)` (wygasłe/odwołane/użyte → `Gone`); `Result Accept(Guid userId, DateTimeOffset now)`;
    `void Revoke(DateTimeOffset now)`.
  - `CreateInvitationRequest(MemberRole Role, Guid? AccountId)` + walidator (`AccountId` podane → rola Viewer/Editor);
    `CreatedInvitation(Guid Id, string Token, DateTimeOffset ExpiresAt)`;
    `InvitationPreview(Guid WalletId, string WalletName, string InvitedBy, MemberRole Role, string? AccountName)`;
    `AcceptedInvitation(Guid WalletId)`.
  - `InvitationLookup.FindAsync(AppDbContext db, string token, CancellationToken ct) : Task<Invitation?>` (po `TokenHash`).
  - `POST /api/wallets/{w}/invitations` (201, `ManageMembers`; konto z innego portfela → 404),
    `DELETE /api/wallets/{w}/invitations/{id}` (204, `ManageMembers`),
    `GET /api/invitations/{token}` (nieznany/niepoprawny → 404, nieużywalny → 410),
    `POST /api/invitations/{token}/accept` (200 + `AcceptedInvitation`; `Wallet.Grant` albo `Account.Grant` – nigdy
    nie obniża roli; oznacza zaproszenie jako użyte).
  - EF: `TokenHash` `binary(32)` z unikalnym indeksem; FK do `Wallet` i `Account` Cascade; `CreatedByUserId`,
    `AcceptedByUserId` bez FK.

- [ ] **Step 1: Write the failing tests**

```csharp
public class InvitationTests // unit
{
    [Fact] public void Token_is_not_stored_in_plain_text() { var (inv, token) = Create(); InvitationToken.TryHash(token, out var hash).ShouldBeTrue(); inv.TokenHash.ShouldBe(hash); }
    [Fact] public void Expires_after_seven_days() => inv.CheckUsable(Now.AddDays(7).AddSeconds(1)).Error!.Kind.ShouldBe(ErrorKind.Gone);
    [Fact] public void Can_be_accepted_only_once() { inv.Accept(UserA, Now); inv.Accept(UserB, Now).Error!.Kind.ShouldBe(ErrorKind.Gone); }
    [Fact] public void Revoked_invitation_is_gone() { inv.Revoke(Now); inv.CheckUsable(Now).Error!.Kind.ShouldBe(ErrorKind.Gone); }
    [Theory] [InlineData("")] [InlineData("not-base64!")] [InlineData("AAAA")] public void Malformed_token_cannot_be_hashed(string t) => InvitationToken.TryHash(t, out _).ShouldBeFalse();
}

public class InvitationTests(IntegrationFixture fixture) : IntegrationTest(fixture) // namespace Features.Invitations
{
    [Fact] public async Task Accepted_wallet_invitation_grants_role() { snapshot.Role.ShouldBe(MemberRole.Editor); }
    [Fact] public async Task Accepted_account_invitation_shares_only_that_account() => snapshot.Accounts.Select(a => a.Id).ShouldBe([account.Id]);
    [Fact] public async Task Owner_accepting_editor_invitation_stays_owner() => snapshot.Role.ShouldBe(MemberRole.Owner);
    [Fact] public async Task Second_accept_returns_410() => response.StatusCode.ShouldBe(HttpStatusCode.Gone);
    [Fact] public async Task Expired_invitation_returns_410() { /* TimeProvider przesunięty o 8 dni (FakeTimeProvider – własny w testach, bez paczki) */ response.StatusCode.ShouldBe(HttpStatusCode.Gone); }
    [Fact] public async Task Unknown_token_returns_404() => response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    [Fact] public async Task Preview_shows_wallet_and_inviter() => preview.ShouldBe(new InvitationPreview(wallet.Id, "Home", "owner@example.com", MemberRole.Viewer, null));
    [Fact] public async Task Pending_invitations_are_listed_for_owner_only() { ownerView.Invitations.Count.ShouldBe(1); viewerView.Invitations.ShouldBeEmpty(); }
    [Fact] public async Task Account_from_other_wallet_returns_404() => response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    [Fact] public async Task Anonymous_preview_returns_401() => response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
}
```

Czas w testach integracyjnych: `BM2AppFactory` rejestruje `TestTimeProvider : TimeProvider` (singleton z `Advance(TimeSpan)`)
w miejsce `TimeProvider.System`. Dopisz przypadki dostępu: `POST …/invitations` (Owner), `DELETE …/invitations/{i}` (Owner).

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.UnitTests.Invitations.InvitationTests`: FAIL.
- [ ] **Step 3: Implement domain, migration and endpoints** (`dotnet ef migrations add AddInvitations …`)
- [ ] **Step 4: Run tests to verify they pass** – oba `InvitationTests`, `MemberTests`, `WalletEndpointAccessTests`, `WalletTests`: PASS.
- [ ] **Step 5: Commit** – `git commit -m "Add wallet and account invitations"`

### Task 17: Rejestracja z zaproszeniem

**Files:**
- Modify: `src/BM2.Server/Infrastructure/RegistrationGate.cs`, `src/BM2.Server/Components/Account/Pages/Register.razor`
- Test: `tests/BM2.IntegrationTests/Features/Invitations/InviteRegistrationTests.cs`

**Interfaces:**
- Consumes: `InvitationLookup.FindAsync`, `Invitation.CheckUsable`, `TimeProvider`.
- Produces: poza Development `/Account/Register` przepuszczane tylko z `?invite=<token>` wskazującym używalne zaproszenie
  (inaczej 404, jak dotąd); `/Account/RegisterConfirmation` nadal 404. `Register.razor`: `[SupplyParameterFromQuery] string? Invite`;
  z ważnym zaproszeniem po `CreateAsync` → potwierdzenie e-maila (`GenerateEmailConfirmationTokenAsync` + `ConfirmEmailAsync`),
  `SignInManager.SignInAsync(user, isPersistent: true)`, przekierowanie na `/invite/{token}` (bez strony potwierdzenia);
  bez zaproszenia (Development) – dotychczasowy przepływ. Ukryte pole formularza przenosi `invite` przy POST.

- [ ] **Step 1: Write the failing tests**

```csharp
public class InviteRegistrationTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact] public async Task Register_page_without_invite_is_404_outside_development() => response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    [Fact] public async Task Register_page_with_expired_invite_is_404() => response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    [Fact] public async Task Registering_with_invite_creates_confirmed_user_and_returns_to_invite()
    {
        // GET /Account/Register?invite={token} → antiforgery token; POST form with Input.Email/Password/ConfirmPassword and invite
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.ShouldEndWith($"/invite/{token}");
        (await db.Users.SingleAsync(u => u.Email == "new@example.com")).EmailConfirmed.ShouldBeTrue();
    }
}
```

Pomocnik formularza: wydziel z `AuthenticationTests` (`AntiforgeryToken()` regex i wysyłka formularza) do
`Infrastructure/FormPost.cs` i użyj w obu klasach.

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.IntegrationTests.Features.Invitations.InviteRegistrationTests`: FAIL.
- [ ] **Step 3: Implement gate and register changes**
- [ ] **Step 4: Run tests to verify they pass** – `InviteRegistrationTests`, `AuthenticationTests`: PASS.
- [ ] **Step 5: Commit** – `git commit -m "Allow registration with a valid invitation"`

### Task 18: UI współdzielenia – Members i strona zaproszenia

**Files:**
- Create: `src/BM2.Client/Pages/{Members,Invite}.razor`
- Modify: `More.razor`, `Icon.razor` (`users`, `share`, `link`), `components.css`
- Test: `tests/BM2.ClientTests/Pages/{MembersTests,InviteTests}.cs`

**Interfaces:**
- Produces:
  - `/members`: sekcje Members (e-mail, rola, „(you)”), Shared accounts, Pending invitations (data ważności, „Revoke”);
    Owner: zmiana roli (`SelectField`), usuwanie (potwierdzenie); każdy: „Leave” przy sobie; „Invite” → arkusz: zakres
    („Whole wallet” / konto), rola → po utworzeniu link `NavigationManager.BaseUri + "invite/" + token`, przyciski
    „Copy link” (`navigator.clipboard.writeText`) i „Share” (`navigator.share`, gdy dostępne), komunikat „The link works
    once and expires in 7 days.” Po „Leave” → `WalletState.RefreshAsync()` i powrót na Home.
  - `/invite/{token}` (bez `[Authorize]`): niezalogowany – „You've been invited to a shared wallet.”, przyciski „Log in”
    (`Account/Login?ReturnUrl=%2Finvite%2F{token}`) i „Create account” (`Account/Register?invite={token}`); zalogowany –
    podgląd (portfel, kto zaprasza, rola, konto), „Join” → `POST …/accept` → `WalletState.SelectAsync(walletId)` → Home;
    404 → „This invitation doesn't exist.”, 410 → „This invitation has expired or was already used.”

- [ ] **Step 1: Write the failing tests**

```csharp
public class InviteTests : BunitContext
{
    [Fact] public void Anonymous_user_gets_login_and_register_links_with_token() { cut.Find("a[href^='Account/Register']").GetAttribute("href").ShouldBe($"Account/Register?invite={Token}"); }
    [Fact] public void Joining_selects_wallet_and_goes_home() { cut.Find("button.btn-primary").Click(); navigation.Uri.ShouldBe("http://localhost/"); storage["bm2-wallet"].ShouldBe(walletId.ToString()); }
    [Fact] public void Used_invitation_shows_gone_message() => cut.Markup.ShouldContain("expired or was already used");
}

public class MembersTests : BunitContext
{
    [Fact] public void Created_invitation_shows_link_with_token() => cut.Find("input[readonly]").GetAttribute("value").ShouldBe($"http://localhost/invite/{Token}");
    [Fact] public void Non_owner_does_not_see_invite_button() => cut.FindAll("button").ShouldNotContain(b => b.TextContent.Contains("Invite"));
}
```

Stan uwierzytelnienia w bUnit: `AddAuthorization()` i `SetAuthorized("me@example.com")` / brak.

- [ ] **Step 2: Run tests to verify they fail** – `--filter-class BM2.ClientTests.Pages.InviteTests`: FAIL.
- [ ] **Step 3: Implement pages**
- [ ] **Step 4: Run tests to verify they pass** – `InviteTests`, `MembersTests`: PASS.
- [ ] **Step 5: Slice checkpoint** – pełna suita + build `CI=true`; demo w dwóch sesjach (zwykła + prywatna): zaproszenie
  do portfela jako Editor, rejestracja nowego konta z linku, dodanie rekordu przez drugą osobę; udostępnienie jednego
  konta jako Viewer – druga osoba widzi tylko to konto, bez przycisku dodawania; „Leave”, odwołanie zaproszenia.
- [ ] **Step 6: Commit** – `git commit -m "Add members screen and invitation page"`

## Plasterek 6 – sprzątanie

### Task 19: Usunięcie `legacy/`, dokumentacja, weryfikacja końcowa

**Files:**
- Delete: `legacy/` (cały katalog)
- Modify: `docs/roadmap.md` (status 1B: ukończony, link do planu; „Stan wyjściowy” – adnotacja, że stary kod usunięty
  w commicie po `<hash>`), `AGENTS.md` (konwencje: `Result`/`ToProblem`, `ValidationFilter`, `CurrentWallet` +
  `WalletAccessFilter`, 404/403, `WalletEndpointAccessTests` – każdy nowy endpoint portfela dopisuje przypadek; `legacy/`
  usunięty), `docs/specs/2026-10-24-podprojekt-1b-funkcje-i-dostep.md` (status), `docs/dev-setup.md` (zaproszenia lokalnie)

- [ ] **Step 1: Check nothing references legacy** – Run: `git grep -n "legacy/" -- . ":!docs"` → brak wyników poza
  usuwanym katalogiem (CI i `BM2.slnx` go nie zawierają).
- [ ] **Step 2: Delete `legacy/` and update docs** (`git rm -r legacy`).
- [ ] **Step 3: Full verification** – Run: `dotnet test` → wszystkie zielone; `$env:CI='true'; dotnet build BM2.slnx -c Release`
  → 0 ostrzeżeń; `dotnet list package --vulnerable --include-transitive` → brak znalezisk.
- [ ] **Step 4: Final demo** – kryteria akceptacji 1–2 ze speca na https://localhost:7237 (telefon 375 px i desktop).
- [ ] **Step 5: Commit** – `git commit -m "Remove legacy code and document sub-project 1B"`
