using JyskBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace JyskBackend.Database;

/// <summary>
/// Наповнення каталогу демо-даними.
///
/// Асортимент меблевий, як у справжньому JYSK: меблі, матраци, текстиль,
/// килими, освітлення та зберігання. Побутової електроніки, інструментів
/// і посуду тут свідомо немає.
///
/// Сідер доповнювальний, а не «тільки для порожньої бази»: категорії звіряються
/// за назвою, товари — за артикулом. Тому нові позиції з'являються при звичайному
/// перезапуску, без видалення jysk.db, а вже наявні товари не дублюються
/// й не перезаписуються (правки через адмінку залишаються на місці).
/// </summary>
public static class DbInitializer
{
    private const string U = "https://images.unsplash.com/";
    private const string Q = "?auto=format&fit=crop&w=800&q=80";

    private static string Photo(string id) => U + id + Q;

    /// <summary>
    /// Опис товару для сідера. Ключ ідемпотентності — <paramref name="Article"/>.
    /// Характеристики необов'язкові: частина позицій каталогу заведена без них,
    /// і вигадувати матеріал чи габарити заради заповнення полів не варто.
    /// </summary>
    private record Seed(
        string Article, string Category, string Name, string Description,
        decimal Price, decimal? OldPrice, int Stock, bool IsNew,
        string? Brand = null, string? Material = null, string? Color = null,
        string? Dimensions = null, string[]? Photos = null
    );

    public static async Task SeedAsync(JyskDbContext context)
    {
        await RemoveRetiredAsync(context);
        var categories = await EnsureCategoriesAsync(context);
        await EnsureProductsAsync(context, categories);
        // Порожні розділи прибираємо в кінці: товари, що лишились, встигають
        // переїхати у нові категорії, і аж тоді старі виявляються порожніми.
        await RemoveEmptyRetiredCategoriesAsync(context);
    }

    // ──────────────────── Прибирання застарілого ─────────────────────
    // Каталог свого часу набрали немеблевими товарами (розумні розетки,
    // блендер, дриль, гірлянди). Сідер лише додає, тому у вже наповнених
    // базах ці позиції треба прибрати явно — інакше вони житимуть вічно.

    private static readonly string[] RetiredArticles =
    [
        "TALO-001", "TALO-003", "TALO-004", "TALO-005", "TALO-006", "TALO-007",
        "TALO-008", "TALO-009", "TALO-010", "TALO-011", "TALO-012", "TALO-013",
        "TALO-014", "TALO-015", "TALO-016", "TALO-017", "TALO-018", "TALO-019",
        "TALO-020", "TALO-030",
    ];

    private static readonly string[] RetiredCategories =
    [
        "Організація", "Декор та освітлення", "Електротехніка",
        "Для саду", "Інструменти", "Для свята",
    ];

    /// <summary>
    /// Категорії, які лише змінили назву. Їх треба перейменувати, а не видаляти:
    /// інакше товари лишились би у старому розділі, а поряд з'явився б порожній новий.
    /// </summary>
    private static readonly (string From, string To)[] RenamedCategories =
    [
        ("Меблі для спальні", "Спальня"),
    ];

    private static async Task RemoveRetiredAsync(JyskDbContext context)
    {
        foreach (var (from, to) in RenamedCategories)
        {
            var old = await context.Categories.FirstOrDefaultAsync(c => c.Name == from);
            if (old == null) continue;

            var target = await context.Categories.FirstOrDefaultAsync(c => c.Name == to);
            if (target == null)
            {
                old.Name = to;
            }
            else
            {
                // Обидві назви вже існують — переносимо товари у нову й прибираємо стару.
                var moved = await context.Products.Where(p => p.CategoryId == old.Id).ToListAsync();
                foreach (var p in moved) p.CategoryId = target.Id;
                context.Categories.Remove(old);
            }
        }

        await context.SaveChangesAsync();

        var stale = await context.Products
            .Include(p => p.Images)
            .Where(p => p.ArticleNumber != null && RetiredArticles.Contains(p.ArticleNumber))
            .ToListAsync();

        if (stale.Count > 0)
        {
            context.Products.RemoveRange(stale);
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Прибирає старі розділи каталогу. Категорію видаляємо лише тоді, коли в ній
    /// не лишилось товарів: якщо хтось встиг завести туди власну позицію,
    /// вона важливіша за наше прибирання.
    /// </summary>
    private static async Task RemoveEmptyRetiredCategoriesAsync(JyskDbContext context)
    {
        var categories = await context.Categories
            .Where(c => RetiredCategories.Contains(c.Name))
            .ToListAsync();

        foreach (var category in categories)
        {
            var used = await context.Products.AnyAsync(p => p.CategoryId == category.Id);
            if (!used) context.Categories.Remove(category);
        }

        await context.SaveChangesAsync();
    }

    // ─────────────────────────── Категорії ───────────────────────────

    private static readonly (string Name, string Description, string Photo)[] CategorySeed =
    [
        ("Спальня",          "Ліжка, матраци, шафи, комоди та тумби для спальні",     "photo-1522771739844-6a9f6d5f14af"),
        ("Вітальня",         "Дивани, крісла, столики, тумби під ТВ та килими",        "photo-1493663284031-b7e3aefcae8e"),
        ("Їдальня та кухня", "Обідні столи, стільці та серванти",                      "photo-1617806118233-18e1de247200"),
        ("Офіс та кабінет",  "Робочі столи та ергономічні крісла для продуктивності",  "photo-1518455027359-f3f8164ba6bd"),
        ("Сад і тераса",     "Меблі для саду, балкона й тераси",                       "photo-1519046904884-53103b34b206"),
        ("Зберігання",       "Стелажі, шафи та кошики для порядку в домі",             "photo-1497366216548-37526070297c"),
    ];

    private static async Task<Dictionary<string, int>> EnsureCategoriesAsync(JyskDbContext context)
    {
        var existing = await context.Categories.ToDictionaryAsync(c => c.Name, c => c);

        foreach (var (name, description, photo) in CategorySeed)
        {
            if (existing.ContainsKey(name)) continue;

            var category = new Category
            {
                Name = name,
                Description = description,
                ImageUrl = Photo(photo)
            };
            context.Categories.Add(category);
            existing[name] = category;
        }

        await context.SaveChangesAsync();
        return existing.ToDictionary(kv => kv.Key, kv => kv.Value.Id);
    }

    // ──────────────────────────── Товари ─────────────────────────────

    private static readonly Seed[] ProductSeed =
    [
        // ─────────────── Спальня ───────────────
        new("JYSK-001", "Спальня", "Ліжко кутове JYSK Vedde",
            "Комфортне двоспальне ліжко з м'яким узголів'ям та міцним каркасом.",
            11500, 14000, 12, true, "JYSK", "ДСП / тканина", "Сірий", "160×200 см",
            ["photo-1505693416388-ac5ce068fe85", "photo-1616594039964-ae9021a400a0"]),
        new("TALO-040", "Спальня", "Ліжко двоспальне GABEL",
            "Класичне ліжко з натурального дерева для здорового сну.",
            9800, null, 9, false, "GABEL", "Масив дуба", "Дуб", "180×200 см",
            ["photo-1522771739844-6a9f6d5f14af", "photo-1560448204-e02f11c3d0e2"]),
        new("JYSK-005", "Спальня", "Матрац безпружинний GOLD F30",
            "Високоякісний ортопедичний матрац з ефектом пам'яті.",
            8900, 11200, 8, false, Photos: ["photo-1631049307264-da0ec9d70304"]),
        new("JYSK-007", "Спальня", "Шафа для одягу TARP 2-дверна",
            "Містка шафа-купе з дзеркалом та висувними полицями.",
            15400, 18000, 4, false, Photos: ["photo-1595428774223-ef52624120d2"]),
        new("TALO-041", "Спальня", "Шафа розсувна VINTERBRO",
            "Місткa шафа-купе з дзеркалом і трьома секціями.",
            15400, 17900, 6, false, "JYSK", "ЛДСП", "Білий", "200×220 см",
            ["photo-1558997519-83ea9252edf8"]),
        new("JYSK-006", "Спальня", "Тумба приліжкова ABETONE",
            "Компактна тумба з двома шухлядами для спальні.",
            2100, null, 25, true, Photos: ["photo-1532323544230-7191fd51bc1b"]),
        new("TALO-042", "Спальня", "Тумба приліжкова NORESUND",
            "Компактна тумба з двома шухлядами та відкритою нішею.",
            2300, null, 25, true, "JYSK", "ДСП", "Дуб сонома", "45×40 см",
            ["photo-1532372320572-cda25653a26d"]),
        new("JYSK-008", "Спальня", "Комод VEDDE 4 шухляди",
            "Стильний комод у кольорі дуб для білизни та речей.",
            6200, 7500, 10, false, Photos: ["photo-1558882224-dda166733046"]),
        new("TALO-043", "Спальня", "Комод HARLEV",
            "Просторий комод на шість шухляд для спальні чи передпокою.",
            6900, 8200, 10, false, "GABEL", "МДФ", "Графіт", "120×80 см",
            ["photo-1595428774223-ef52624120d2"]),
        new("JYSK-009", "Спальня", "Подушка анатомічна WELLPUR",
            "Ортопедична подушка з наповнювачем Memory Foam.",
            1450, null, 30, true, Photos: ["photo-1584100936595-c0654b55a2e2"]),
        new("TALO-044", "Спальня", "Дзеркало підлогове STUBBERUP",
            "Велике підлогове дзеркало в тонкій металевій рамі.",
            3400, null, 14, true, "JYSK", "Метал / скло", "Чорний", "50×170 см",
            ["photo-1618220179428-22790b461013"]),

        // ─────────────── Вітальня ───────────────
        new("JYSK-002", "Вітальня", "Диван розкладний MARIBO",
            "Стильний сірий диван з механізмом «єврокнижка» та нішею для білизни.",
            18900, null, 5, true, "MARIBO", "Рогожка", "Сірий", "220×95 см",
            ["photo-1555041469-a586c61ea9bc", "photo-1493663284031-b7e3aefcae8e"]),
        new("TALO-045", "Вітальня", "Диван кутовий VEJLBY",
            "Просторий кутовий диван для великої родини та гостей.",
            24500, 28000, 4, false, "MARIBO", "Велюр", "Синій", "280×180 см",
            ["photo-1550254478-ead40cc54513", "photo-1567016432779-094069958ea5"]),
        new("JYSK-011", "Вітальня", "Крісло відпочинкове HAVNDAL",
            "М'яке затишне крісло з ергономічною спинкою.",
            7200, 8500, 7, false, Photos: ["photo-1586023492125-27b2c045efd7"]),
        new("TALO-046", "Вітальня", "Крісло м'яке AABENRAA",
            "Затишне крісло для читання з високою спинкою.",
            7200, null, 11, true, "JYSK", "Букле", "Молочний", "80×85 см",
            ["photo-1567538096630-e0c55bd6374c"]),
        new("JYSK-010", "Вітальня", "Журнальний столик GANDRUP",
            "Лаконічний круглий столик із дерев'яною стільницею.",
            3100, 3800, 18, false, Photos: ["photo-1540574163026-643ea20ade25"]),
        new("TALO-047", "Вітальня", "Журнальний столик FROSTED",
            "Лаконічний журнальний столик зі склом та металевими ніжками.",
            3900, 4800, 18, false, "JYSK", "Скло / метал", "Прозорий", "100×50 см",
            ["photo-1499933374294-4584851497cc"]),
        new("JYSK-012", "Вітальня", "Тумба під ТВ HAGEN",
            "Сучасна тумба під телевізор із закритими полицями.",
            4800, null, 14, true, Photos: ["photo-1607400201515-c2c41c07d307"]),
        new("JYSK-014", "Вітальня", "Килим тканий BARRIT 160×230",
            "Безворсовий прямокутний килим нейтрального відтінку.",
            2600, null, 15, false, Photos: ["photo-1600121848594-d8644e57abab"]),
        new("JYSK-019", "Вітальня", "Килим VISKUM",
            "М'який короткий ворс, приємний на дотик і легкий у догляді.",
            2900, null, 30, true, "JYSK", "Поліпропілен", "Бежевий", "160×230 см",
            ["photo-1600166898405-da9535204843"]),
        new("JYSK-020", "Вітальня", "Торшер AKSEL",
            "Підлоговий світильник з тканинним абажуром і теплим світлом.",
            1850, 2400, 22, false, "JYSK", "Метал / тканина", "Чорний", "40×150 см",
            ["photo-1507473885765-e6ed057f782c"]),
        new("TALO-031", "Вітальня", "Подушки декоративні COZY",
            "Набір декоративних подушок із чохлами, що знімаються, 2 шт.",
            740, 990, 35, true, "TALO", "Льон", "Оливковий", "45×45 см",
            ["photo-1584100936595-c0654b55a2e2"]),

        // ─────────────── Їдальня та кухня ───────────────
        new("TALO-050", "Їдальня та кухня", "Обідній стіл MELVIN",
            "Обідній стіл із суцільною стільницею на шість персон.",
            8400, 9900, 9, true, "JYSK", "Масив дуба", "Дуб", "180×90 см",
            ["photo-1533090161767-e6ffed986c88"]),
        new("JYSK-023", "Їдальня та кухня", "Стілець ергономічний HERNING",
            "Легкий стілець з дерев'яними ніжками для їдальні чи кухні.",
            3200, null, 24, false, "GABEL", "Дерево / тканина", "Гірчичний", "45×80 см",
            ["photo-1503602642458-232111445657"]),
        new("TALO-051", "Їдальня та кухня", "Барний стілець HAMPEN",
            "Високий барний стілець з підніжкою та м'яким сидінням.",
            2450, 2900, 20, true, "JYSK", "Метал / екошкіра", "Чорний", "45×105 см",
            ["photo-1631679706909-1844bbd07221"]),
        new("TALO-052", "Їдальня та кухня", "Сервант NORDBY",
            "Сервант із заскленими дверцятами для посуду й текстилю.",
            7600, null, 7, false, "GABEL", "МДФ", "Білий", "140×85 см",
            ["photo-1616486338812-3dadae4b4ace"]),

        // ─────────────── Офіс та кабінет ───────────────
        new("JYSK-003", "Офіс та кабінет", "Стіл робочий Loft Desk",
            "Ергономічний стіл у стилі лофт з металевим каркасом.",
            4200, 5500, 20, false, "JYSK", "ДСП / метал", "Дуб / чорний", "120×60 см",
            ["photo-1518455027359-f3f8164ba6bd", "photo-1593062096033-9a26b09da705"]),
        new("JYSK-015", "Офіс та кабінет", "Стіл з регулюванням висоти SLANGERUP",
            "Електричний стіл для роботи стоячи та сидячи.",
            13500, 16000, 6, true, Photos: ["photo-1527443224154-c4a3942d3acf"]),
        new("JYSK-021", "Офіс та кабінет", "Стіл письмовий AUSTIN",
            "Компактний письмовий стіл з двома шухлядами для дому.",
            5600, null, 13, true, "GABEL", "МДФ", "Білий", "110×55 см",
            ["photo-1524758631624-e2822e304c36"]),
        new("JYSK-004", "Офіс та кабінет", "Крісло офісне TJELE",
            "Офісне крісло з еко-шкіри з регулюванням висоти та підлокітниками.",
            6700, 8000, 8, false, "JYSK", "Еко-шкіра", "Чорний", "60×60 см",
            ["photo-1592078615290-033ee584e267"]),
        new("JYSK-016", "Офіс та кабінет", "Геймерське крісло AGERSTRUP",
            "Ергономічне геймерське крісло з підтримкою попереку.",
            7900, 9200, 11, true, Photos: ["photo-1598300042247-d088f8ab3a91"]),
        new("JYSK-022", "Офіс та кабінет", "Крісло робоче SVANE",
            "Сітчаста спинка для вентиляції та підтримки спини протягом дня.",
            4900, null, 16, true, "JYSK", "Сітка / пластик", "Сірий", "58×58 см",
            ["photo-1519947486511-46149fa0a254"]),
        new("JYSK-018", "Офіс та кабінет", "Настільна лампа LED FREDERIK",
            "Світлодіодна лампа з регулюванням яскравості та USB-портом.",
            1100, 1450, 35, false, Photos: ["photo-1517991104123-1d56a6e81ed9"]),

        // ─────────────── Сад і тераса ───────────────
        new("TALO-053", "Сад і тераса", "Садовий стіл RANDERS",
            "Стійкий до вологи стіл для тераси та саду.",
            5900, 7200, 8, true, "JYSK", "Акація / метал", "Натуральний", "150×90 см",
            ["photo-1600585154340-be6161a56a0c"]),
        new("TALO-054", "Сад і тераса", "Садове крісло HOLMSLAND",
            "Плетене крісло для тераси з м'якою подушкою.",
            3400, null, 16, true, "JYSK", "Ротанг / метал", "Бежевий", "60×85 см",
            ["photo-1567016376408-0226e4d0c1ea"]),
        new("TALO-055", "Сад і тераса", "Шезлонг LANDSKRONA",
            "Розкладний шезлонг з регульованою спинкою.",
            4200, 5100, 11, false, "JYSK", "Текстилен / алюміній", "Сірий", "190×60 см",
            ["photo-1540541338287-41700207dee6"]),
        new("TALO-056", "Сад і тераса", "Лавка садова ODDER",
            "Дерев'яна лавка для саду на двох осіб.",
            3800, null, 13, false, "GABEL", "Масив сосни", "Натуральний", "120×60 см",
            ["photo-1519710164239-da123dc03ef4"]),

        // ─────────────── Зберігання ───────────────
        new("JYSK-013", "Зберігання", "Стелаж підлоговий BROBY",
            "Легкий відкритий стелаж для книг, рослин та декору.",
            2900, 3500, 22, false, Photos: ["photo-1594620302200-9a762244a156"]),
        new("TALO-048", "Зберігання", "Стелаж BILLUND",
            "Відкритий стелаж на п'ять полиць для книг та декору.",
            5400, null, 15, false, "GABEL", "ЛДСП", "Дуб", "80×180 см",
            ["photo-1497366216548-37526070297c"]),
        new("JYSK-017", "Зберігання", "Шафа для документів MUSDAL",
            "Шафа з замком та регульованими по висоті полицями.",
            5400, null, 9, false, Photos: ["photo-1505330622279-bf7d7fc918f4"]),
        new("TALO-002", "Зберігання", "Кошик плетений WOVEN L",
            "Плетений кошик для білизни, іграшок або пледів.",
            560, 780, 45, false, "TALO", "Джут", "Натуральний", "40×45 см",
            ["photo-1584589167171-541ce45f1eea"]),
    ];

    private static async Task EnsureProductsAsync(JyskDbContext context, Dictionary<string, int> categories)
    {
        var existing = await context.Products
            .Where(p => p.ArticleNumber != null)
            .ToDictionaryAsync(p => p.ArticleNumber!, p => p);

        // Розділи каталогу переїжджали, тому в уже заведених товарів звіряємо
        // категорію з описом у сідері. Ціни, залишки й тексти не чіпаємо —
        // правки через адмінку мають лишитися на місці.
        foreach (var s in ProductSeed)
        {
            if (!existing.TryGetValue(s.Article, out var product)) continue;
            if (!categories.TryGetValue(s.Category, out var wantedCategoryId)) continue;
            if (product.CategoryId != wantedCategoryId) product.CategoryId = wantedCategoryId;
        }
        await context.SaveChangesAsync();

        var known = existing.Keys.ToHashSet();
        var toAdd = ProductSeed.Where(s => !known.Contains(s.Article)).ToList();
        if (toAdd.Count == 0) return;

        // CreatedAt рознесений у часі, щоб блок «Новинки» мав осмислений порядок:
        // перші в списку — найсвіжіші.
        var now = DateTime.UtcNow;

        for (int i = 0; i < toAdd.Count; i++)
        {
            var s = toAdd[i];
            if (!categories.TryGetValue(s.Category, out var categoryId)) continue;

            context.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                OldPrice = s.OldPrice,
                Stock = s.Stock,
                IsNew = s.IsNew,
                ArticleNumber = s.Article,
                Brand = s.Brand,
                Material = s.Material,
                Color = s.Color,
                Dimensions = s.Dimensions,
                CategoryId = categoryId,
                CreatedAt = now.AddMinutes(-i),
                Images = (s.Photos ?? [])
                    .Select((photo, index) => new ProductImage
                    {
                        ImageUrl = Photo(photo),
                        IsMain = index == 0,
                        SortOrder = index
                    })
                    .ToList()
            });
        }

        await context.SaveChangesAsync();
    }
}
