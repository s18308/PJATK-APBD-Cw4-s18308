# APBD-PJATK-Kolokwium-s18308

Notatka praktyczna: jak zaimplementowac aplikacje REST API w ASP.NET Core
z Entity Framework Core w podejsciu Code First.

Schemat pracy: zadanie daje Ci diagram encji (tabele + relacje) oraz liste
endpointow. Najpierw odtwarzasz model w kodzie i generujesz migracje (Czesc 1),
potem dopisujesz warstwe API (Czesc 2).

W przykladach uzywam ogolnych nazw: TableA, TableB, TableAB (tabela laczaca),
ParentTable, ChildTable, TabRequest, TabResponse, ITabService, TabService,
TabsController. Zapis TabelaB[Kolumna1] oznacza kolumne Kolumna1 w tabeli TabelaB.

---

## 0. Przygotowanie projektu

Foldery, ktore tworzysz w projekcie (kazdy ma jedna odpowiedzialnosc):

    Entities/        -> klasy encji (1 klasa = 1 tabela z diagramu)
    Infrastructure/  -> DatabaseContext (konfiguracja modelu)
    Migrations/      -> generowane automatycznie przez dotnet ef
    DTOs/            -> obiekty wejscia (Request) i wyjscia (Response)
    Exceptions/      -> wlasne wyjatki (np. NotFoundException)
    Services/        -> logika dostepu do danych (interfejs + implementacja)
    Controllers/     -> endpointy HTTP

Pakiety NuGet (plik .csproj):

    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.8">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>

Instalacja narzedzia dotnet-ef (raz na maszyne lub lokalnie w repo):

    dotnet tool install --global dotnet-ef
    # lub lokalnie (dotnet-tools.json): dotnet tool install dotnet-ef

Connection string i schema w appsettings.json:

    "ConnectionStrings": {
      "Default": "Data Source=localhost,1433; User=SA; Password=<haslo>; Initial Catalog=master; Integrated Security=False; Connect Timeout=30; Encrypt=False; Trust Server Certificate=False"
    },
    "DB": {
      "DefaultSchema": "test-example"
    }

---

# CZESC 1 — Odtworzenie diagramu encji w kodzie

## 1.1. Zasada: 1 tabela na diagramie = 1 klasa w folderze Entities/

Dla kazdego prostokata (tabeli) z diagramu tworzysz jedna klase POCO
(zwykla klasa z wlasciwosciami, bez logiki). Kolumny tabeli = wlasciwosci klasy.

Klasa z prostym kluczem glownym (int, auto-increment):

    namespace Projekt.Entities;

    public class ParentTable
    {
        public int Id { get; set; }              // klucz glowny (PK)
        public string Name { get; set; } = null!; // kolumna NOT NULL
        public string? Description { get; set; }  // kolumna NULL (znak ? = nullable)
    }

Konwencje:
- wlasciwosc o nazwie Id (lub <NazwaKlasy>Id) EF rozpozna automatycznie jako PK
- typ int jako PK domyslnie staje sie kolumna IDENTITY (auto-increment)
- = null! mowi kompilatorowi "to nie bedzie null" (wartosc ustawi EF)
- typ z ? (string?, int?) oznacza kolumne dopuszczajaca NULL

## 1.2. Relacja jeden-do-wielu (1:N)

Najczestsza relacja. Przyklad: jeden ParentTable ma wiele ChildTable
(np. jeden Producent ma wiele Komponentow).

Po stronie "wielu" (ChildTable) dodajesz klucz obcy + wlasciwosc nawigacyjna
do rodzica. Po stronie "jeden" (ParentTable) dodajesz kolekcje dzieci.

ChildTable.cs (strona N — tu trzymamy klucz obcy):

    public class ChildTable
    {
        public int Id { get; set; }

        public int ParentTableId { get; set; }              // klucz obcy (FK)
        public virtual ParentTable ParentTable { get; set; } = null!; // nawigacja do rodzica
    }

ParentTable.cs (strona 1 — kolekcja dzieci):

    public class ParentTable
    {
        public int Id { get; set; }
        // ...pozostale kolumny...

        public virtual ICollection<ChildTable> ChildTables { get; set; } = new List<ChildTable>();
    }

Zasada zapamietania:
- "wiele" = tam jest <Rodzic>Id (FK) oraz pojedyncza nawigacja do rodzica
- "jeden" = tam jest ICollection<Dziecko>
- slowo virtual umozliwia lazy loading / proxy (trzymamy je dla spojnosci)

## 1.3. Relacja wiele-do-wielu (M:N) z dodatkowym polem

Gdy w polaczeniu miedzy dwiema tabelami jest dodatkowa informacja
(np. ilosc, data przypisania) — robisz OSOBNA encje laczaca.
Przyklad: TableA (np. zestaw) i TableB (np. czesc), a w polaczeniu jest
TableAB[Amount] (ile sztuk danej czesci w zestawie).

Encja laczaca ma KLUCZ ZLOZONY z dwoch kluczy obcych + pola dodatkowe.

TableAB.cs (tabela laczaca):

    public class TableAB
    {
        public int TableAId { get; set; }                 // FK do TableA (czesc klucza)
        public virtual TableA TableA { get; set; } = null!;

        public string TableBCode { get; set; } = null!;   // FK do TableB (czesc klucza)
        public virtual TableB TableB { get; set; } = null!;

        public int Amount { get; set; }                   // pole dodatkowe na relacji
    }

TableA.cs i TableB.cs — kazda dostaje kolekcje encji laczacych:

    public class TableA
    {
        public int Id { get; set; }
        public virtual ICollection<TableAB> TableABs { get; set; } = new List<TableAB>();
    }

    public class TableB
    {
        public string Code { get; set; } = null!;
        public virtual ICollection<TableAB> TableABs { get; set; } = new List<TableAB>();
    }

Klucz zlozony konfigurujesz w DatabaseContext (patrz 1.5):

    opt.HasKey(x => new { x.TableAId, x.TableBCode });

Uwaga: jesli na relacji M:N NIE ma zadnego dodatkowego pola, mozesz pominac
encje laczaca i uzyc skin-many: modelBuilder.Entity<TableA>().HasMany(...).WithMany(...).
W zadaniach prawie zawsze jest pole dodatkowe, wiec stosuj encje laczaca.

## 1.4. DatabaseContext — DbSet-y

W folderze Infrastructure/ tworzysz klase dziedziczaca po DbContext.
Kazda encja, ktora ma byc tabela, dostaje wlasciwosc DbSet.

    using Microsoft.EntityFrameworkCore;

    namespace Projekt.Infrastructure;

    public class DatabaseContext(DbContextOptions opt, IConfiguration configuration)
        : DbContext(opt)
    {
        public virtual DbSet<ParentTable> ParentTables { get; set; }
        public virtual DbSet<ChildTable> ChildTables { get; set; }
        public virtual DbSet<TableA> TableAs { get; set; }
        public virtual DbSet<TableB> TableBs { get; set; }
        public virtual DbSet<TableAB> TableABs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // konfiguracja w punkcie 1.5
        }
    }

IConfiguration w konstruktorze jest po to, by odczytac nazwe schematu
z appsettings.json.

## 1.5. OnModelCreating — Fluent API (klucze, kolumny, relacje, typy)

Tu opisujesz model "plynnie". Kazda encje konfigurujesz w osobnym bloku.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ustawia schemat dla wszystkich tabel (czyta z appsettings.json)
        modelBuilder.HasDefaultSchema(configuration["DB:DefaultSchema"]);

        // --- encja z prostym kluczem ---
        modelBuilder.Entity<ParentTable>(opt =>
        {
            opt.HasKey(x => x.Id);

            opt.Property(x => x.Name)
                .HasMaxLength(50)        // VARCHAR/NVARCHAR o dlugosci 50
                .IsRequired();           // NOT NULL
        });

        // --- relacja 1:N (konfiguracja po stronie dziecka) ---
        modelBuilder.Entity<ChildTable>(opt =>
        {
            opt.HasKey(x => x.Id);

            opt.HasOne(x => x.ParentTable)        // dziecko ma jednego rodzica
                .WithMany(x => x.ChildTables)     // rodzic ma wiele dzieci
                .HasForeignKey(x => x.ParentTableId); // kolumna FK
        });

        // --- encja laczaca M:N z kluczem zlozonym ---
        modelBuilder.Entity<TableAB>(opt =>
        {
            opt.HasKey(x => new { x.TableAId, x.TableBCode }); // klucz zlozony

            opt.HasOne(x => x.TableA)
                .WithMany(x => x.TableABs)
                .HasForeignKey(x => x.TableAId);

            opt.HasOne(x => x.TableB)
                .WithMany(x => x.TableABs)
                .HasForeignKey(x => x.TableBCode);
        });
    }

## 1.6. Typy danych — gdzie i jak je ustawiac

Domyslnie EF dobiera typ na podstawie typu C# (string -> nvarchar(max),
int -> int, double -> float, DateTime -> datetime2). Jesli zadanie wymaga
KONKRETNEGO typu kolumny, wymuszasz go w OnModelCreating jedna z metod:

Dlugosc tekstu (mapuje sie na nvarchar(n)):

    opt.Property(x => x.Name).HasMaxLength(150).IsRequired();   // nvarchar(150) NOT NULL

Dokladny typ bazodanowy przez HasColumnType — TU wpisujesz dowolna nazwe typu
z bazy (char, varchar, decimal, datetime, float, nvarchar(max) itd.):

    opt.Property(x => x.Code).HasColumnType("char(10)");        // CHAR(10) - staly rozmiar
    opt.Property(x => x.Description).HasColumnType("nvarchar(max)"); // tekst bez limitu
    opt.Property(x => x.Weight).HasColumnType("float(5)");      // liczba zmiennoprzecinkowa
    opt.Property(x => x.CreatedAt).HasColumnType("datetime");   // konkretny typ daty

Jesli zadanie mowi wprost np. "uzyj varchar(30)" albo (Oracle) "varchar2(30)":

    opt.Property(x => x.Abbreviation).HasColumnType("varchar(30)").IsRequired();
    // typ wpisujesz doslownie jako string w HasColumnType — EF go nie tlumaczy

Liczby z miejscami po przecinku (kwoty):

    opt.Property(x => x.Price).HasColumnType("decimal(18,2)");
    // lub: opt.Property(x => x.Price).HasPrecision(18, 2);

Zasada: HasMaxLength dla dlugosci tekstu, HasColumnType gdy potrzebujesz
dokladnej nazwy typu z bazy. Te ustawienia zawsze trafiaja do
OnModelCreating w DatabaseContext, nie do klas encji.

## 1.7. Seedowanie danych poczatkowych (HasData)

Jesli zadanie wymaga danych startowych, dodajesz je na koncu OnModelCreating.
Musisz podac wartosci kluczy glownych recznie (takze dla relacji).

    modelBuilder.Entity<ParentTable>().HasData(
        new ParentTable { Id = 1, Name = "Pierwszy" },
        new ParentTable { Id = 2, Name = "Drugi" }
    );

    // tabela laczaca — podajesz oba klucze obce i pole dodatkowe
    modelBuilder.Entity<TableAB>().HasData(
        new TableAB { TableAId = 1, TableBCode = "AAA0000001", Amount = 2 }
    );

## 1.8. Rejestracja kontekstu w Program.cs

    using Microsoft.EntityFrameworkCore;

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddDbContext<DatabaseContext>(opt =>
    {
        opt.UseSqlServer(
            builder.Configuration.GetConnectionString("Default"),
            // opcjonalnie: wlasna tabela historii migracji w wybranym schemacie
            x => x.MigrationsHistoryTable("EFCore_Migrations", builder.Configuration["DB:DefaultSchema"])
        );
    });

## 1.9. Generowanie i aplikowanie migracji

Migracja to wygenerowany z modelu kod SQL tworzacy/zmieniajacy tabele.
Uruchamiasz z katalogu projektu (tam gdzie jest plik .csproj):

    # 1) utworzenie migracji na podstawie aktualnego modelu
    dotnet ef migrations add NazwaMigracji

    # 2) zastosowanie migracji do bazy (tworzy tabele)
    dotnet ef database update

Przydatne:

    dotnet ef migrations remove     # usuwa ostatnia (jeszcze nie zaaplikowana) migracje
    dotnet ef migrations list       # lista migracji
    dotnet ef database update 0     # cofa wszystkie migracje

Kolejnosc pracy w Czesci 1: napisz encje -> skonfiguruj OnModelCreating ->
dodaj DbSet-y -> migrations add -> database update. Po kazdej zmianie modelu
generujesz nowa migracje i robisz update.

---

# CZESC 2 — Endpointy (warstwa API)

Przeplyw zadania: Controller (HTTP) -> Service (logika + EF) -> DTO (dane).
Kontroler nie dotyka bazy bezposrednio; robi to serwis. Encje nie wychodza
na zewnatrz — zawsze mapujesz je na DTO.

## 2.1. DTO wejsciowe (Request) z walidacja

W folderze DTOs/. Opisuje dane, ktore klient wysyla (POST/PUT). Walidacje
robisz atrybutami DataAnnotations — sprawdza je [ApiController] automatycznie.

    using System.ComponentModel.DataAnnotations;

    namespace Projekt.DTOs;

    public class TabRequest
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        public int Stock { get; set; }

        [Range(0, int.MaxValue)]
        public double Weight { get; set; }
    }

## 2.2. DTO wyjsciowe (Response), w tym dane z relacji

Opisuje dane zwracane klientowi. Proste (jedna tabela):

    namespace Projekt.DTOs;

    public class TabResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Stock { get; set; }
    }

Gdy zwracasz dane z relacji (np. zestaw razem z czesciami), budujesz
zagniezdzone DTO — jedno glowne i mniejsze dla powiazanych tabel:

    public class TabWithChildrenResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<ChildResponse> Children { get; set; } = new();
    }

    public class ChildResponse
    {
        public int Amount { get; set; }                 // pole z tabeli laczacej
        public string Code { get; set; } = null!;       // dane z tabeli powiazanej
        public string ChildName { get; set; } = null!;
    }

## 2.3. Wlasny wyjatek NotFoundException

W folderze Exceptions/. Rzucasz go w serwisie, gdy rekord nie istnieje,
a w kontrolerze zamieniasz na kod 404.

    namespace Projekt.Exceptions;

    public class NotFoundException(string msg) : Exception(msg);

## 2.4. Interfejs serwisu

W folderze Services/. Definiuje operacje, ktorych potrzebuja endpointy.
Kazda metoda jest asynchroniczna i przyjmuje CancellationToken.

    using Projekt.DTOs;

    namespace Projekt.Services;

    public interface ITabService
    {
        Task<ICollection<TabResponse>> GetAllTabsAsync(CancellationToken cancellationToken);
        Task<TabWithChildrenResponse> GetTabWithChildrenAsync(int id, CancellationToken cancellationToken);
        Task<TabResponse> AddTabAsync(TabRequest request, CancellationToken cancellationToken);
        Task UpdateTabAsync(int id, TabRequest request, CancellationToken cancellationToken);
        Task DeleteTabAsync(int id, CancellationToken cancellationToken);
    }

## 2.5. Implementacja serwisu (CRUD na EF)

Wstrzykujesz DatabaseContext. Wynik zapytania od razu rzutujesz na DTO
przez .Select(...) (projekcja) — nie zwracasz encji.

    using Microsoft.EntityFrameworkCore;
    using Projekt.DTOs;
    using Projekt.Entities;
    using Projekt.Exceptions;
    using Projekt.Infrastructure;

    namespace Projekt.Services;

    public class TabService(DatabaseContext ctx) : ITabService
    {
        // GET lista — projekcja kazdej encji na DTO
        public async Task<ICollection<TabResponse>> GetAllTabsAsync(CancellationToken cancellationToken)
        {
            return await ctx.TableAs
                .Select(e => new TabResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    Stock = e.Stock
                })
                .ToListAsync(cancellationToken);
        }

        // GET szczegoly z danymi z relacji — wchodzisz w nawigacje przez .Select
        public async Task<TabWithChildrenResponse> GetTabWithChildrenAsync(int id, CancellationToken cancellationToken)
        {
            return await ctx.TableAs
                .Where(e => e.Id == id)
                .Select(e => new TabWithChildrenResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    Children = e.TableABs.Select(ab => new ChildResponse
                    {
                        Amount = ab.Amount,
                        Code = ab.TableB.Code,
                        ChildName = ab.TableB.Name
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Tab with id {id} not found");
        }

        // POST — tworzysz encje, zapisujesz, zwracasz DTO z nadanym Id
        public async Task<TabResponse> AddTabAsync(TabRequest request, CancellationToken cancellationToken)
        {
            var entity = new TableA
            {
                Name = request.Name,
                Stock = request.Stock
            };

            await ctx.TableAs.AddAsync(entity, cancellationToken);
            await ctx.SaveChangesAsync(cancellationToken);

            return new TabResponse { Id = entity.Id, Name = entity.Name, Stock = entity.Stock };
        }

        // PUT — pobierasz, modyfikujesz pola, zapisujesz
        public async Task UpdateTabAsync(int id, TabRequest request, CancellationToken cancellationToken)
        {
            var entity = await ctx.TableAs.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
                ?? throw new NotFoundException($"Tab with id {id} not found");

            entity.Name = request.Name;
            entity.Stock = request.Stock;

            await ctx.SaveChangesAsync(cancellationToken);
        }

        // DELETE — gdy trzeba najpierw usunac rekordy zalezne, uzyj transakcji
        public async Task DeleteTabAsync(int id, CancellationToken cancellationToken)
        {
            var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // najpierw rekordy z tabeli laczacej (FK), potem glowny
                await ctx.TableABs.Where(e => e.TableAId == id).ExecuteDeleteAsync(cancellationToken);
                var removed = await ctx.TableAs.Where(e => e.Id == id).ExecuteDeleteAsync(cancellationToken);

                if (removed == 0)
                    throw new NotFoundException($"Tab with id {id} not found");

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

Wzorce do zapamietania:
- odczyt: .Select(...) buduje DTO bezposrednio w zapytaniu (szybkie, bez Include)
- "nie znaleziono": FirstOrDefaultAsync(...) ?? throw new NotFoundException(...)
- zapis nowego: AddAsync + SaveChangesAsync, Id dostepne po zapisie
- usuwanie zaleznosci: ExecuteDeleteAsync w transakcji, by zachowac spojnosc

## 2.6. Rejestracja serwisu w Program.cs

    builder.Services.AddScoped<ITabService, TabService>();

Pelna konfiguracja uslug (kontrolery + OpenAPI + serwis + DbContext):

    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(opt => opt.SuppressMapClientErrors = true);
    builder.Services.AddOpenApi();
    builder.Services.AddScoped<ITabService, TabService>();
    builder.Services.AddDbContext<DatabaseContext>(opt => { /* UseSqlServer jak w 1.8 */ });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();

## 2.7. Kontroler

W folderze Controllers/. Cienka warstwa: odbiera zadanie, wola serwis,
mapuje wyjatki na kody HTTP. Serwis wstrzykujesz przez konstruktor.

    using Microsoft.AspNetCore.Mvc;
    using Projekt.DTOs;
    using Projekt.Exceptions;
    using Projekt.Services;

    namespace Projekt.Controllers;

    [ApiController]
    [Route("api/tabs")]
    public class TabsController(ITabService service) : ControllerBase
    {
        // GET api/tabs
        [HttpGet]
        public async Task<IActionResult> GetAllTabs(CancellationToken cancellationToken)
        {
            var result = await service.GetAllTabsAsync(cancellationToken);
            return Ok(result); // 200
        }

        // GET api/tabs/5/children
        [HttpGet("{id:int}/children")]
        public async Task<IActionResult> GetTabWithChildren([FromRoute] int id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await service.GetTabWithChildrenAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (NotFoundException)
            {
                return NotFound(); // 404
            }
        }

        // POST api/tabs
        [HttpPost]
        public async Task<IActionResult> AddTab([FromBody] TabRequest request, CancellationToken cancellationToken)
        {
            var result = await service.AddTabAsync(request, cancellationToken);
            // 201 + naglowek Location wskazujacy na nowy zasob
            return CreatedAtAction(nameof(GetTabWithChildren), new { id = result.Id }, result);
        }

        // PUT api/tabs/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTab([FromRoute] int id, [FromBody] TabRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await service.UpdateTabAsync(id, request, cancellationToken);
                return Ok(); // 200
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE api/tabs/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTab([FromRoute] int id, CancellationToken cancellationToken)
        {
            try
            {
                await service.DeleteTabAsync(id, cancellationToken);
                return NoContent(); // 204
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }

Co tu jest wazne:
- [ApiController] wlacza automatyczna walidacje Request (400 gdy dane zle)
- [Route("api/tabs")] to prefiks; [HttpGet("{id:int}/children")] dokleja segment
- [FromRoute] = z URL, [FromBody] = z JSON-a w ciele zadania
- kody odpowiedzi: 200 Ok, 201 CreatedAtAction, 204 NoContent, 404 NotFound

## 2.8. Testowanie endpointow (plik .http)

Stworz plik Projekt.http i wysylaj zadania bezposrednio z edytora:

    @host = http://localhost:5000

    ### lista
    GET {{host}}/api/tabs

    ### szczegoly z relacjami
    GET {{host}}/api/tabs/1/children

    ### dodanie
    POST {{host}}/api/tabs
    Content-Type: application/json

    {
      "name": "Nowy",
      "stock": 5,
      "weight": 1.2
    }

    ### edycja
    PUT {{host}}/api/tabs/1
    Content-Type: application/json

    {
      "name": "Zmieniony",
      "stock": 9,
      "weight": 2.0
    }

    ### usuniecie
    DELETE {{host}}/api/tabs/1

---

## Szybka checklista (kolejnosc dzialania)

Czesc 1 (model):
1. Utworz klasy w Entities/ (1 tabela = 1 klasa).
2. Dodaj klucze glowne (prosty Id; zlozony dla tabeli laczacej).
3. Dodaj relacje: 1:N (FK + nawigacja + kolekcja), M:N (encja laczaca z polem).
4. W DatabaseContext dodaj DbSet-y i skonfiguruj OnModelCreating (klucze, typy, relacje).
5. Ustaw typy danych: HasMaxLength / HasColumnType("...") gdzie wymagane.
6. (opcjonalnie) Dodaj dane przez HasData.
7. Zarejestruj DbContext w Program.cs, ustaw connection string i schema.
8. dotnet ef migrations add Init  ->  dotnet ef database update.

Czesc 2 (API):
9. Stworz DTO Request (z walidacja) i Response (w tym zagniezdzone dla relacji).
10. Dodaj NotFoundException.
11. Zdefiniuj interfejs ITabService i napisz TabService (CRUD + projekcja do DTO).
12. Zarejestruj serwis: AddScoped<ITabService, TabService>().
13. Napisz TabsController (atrybuty, routing, mapowanie wyjatkow na kody HTTP).
14. Przetestuj plik .http / OpenAPI.