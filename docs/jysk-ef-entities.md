# 📦 Entity Framework — Сущности базы данных

> Проект: Інтернет-магазин меблів та товарів для дому (аналог JYSK)
> ORM: ASP.NET Entity Framework Core
> База даних: PostgreSQL / SQL Server

---

## Схема зв'язків

```
Category (1) ──< (N) Product (N) >──< (N) OrderItem >──(N) Order (N) >── (1) Customer
                        │
                   ProductImage (N)
                   ProductReview (N) ──── Customer (1)
                   ProductVariant (N)     (колір, розмір — напр. ліжко 160/180/200)

Room (1) ──< (N) RoomProduct >──(N) Product
Collection (1) ──< (N) CollectionProduct >──(N) Product
```

---

## 1. Customer — Покупець

```csharp
public class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; }

    [Required, MaxLength(100)]
    public string LastName { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }   // bcrypt, не зберігати plain-text!

    [Phone, MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Role { get; set; } = "Customer";  // "Customer" | "Admin"

    // Navigation
    public ICollection<Order> Orders { get; set; }
    public ICollection<ProductReview> Reviews { get; set; }
}
```

---

## 2. Category — Категорія товарів

> У JYSK категорії — це: Ліжка, Матраци, Дивани, Столи, Стільці, Текстиль, Освітлення тощо.

```csharp
public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; }             // "Ліжка", "Матраци"

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    // Підкатегорія: "Ліжка" → "Двоспальні ліжка", "Односпальні ліжка"
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    // Navigation
    public ICollection<Product> Products { get; set; }
    public ICollection<Category> SubCategories { get; set; }
}
```

---

## 3. Product — Товар

> Специфіка меблевого магазину: у товару є матеріал, колір, розміри.

```csharp
public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; }             // "Ліжко HAUGE 160x200"

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    // Ціна до знижки (якщо null — знижки немає)
    [Column(TypeName = "decimal(18,2)")]
    public decimal? OldPrice { get; set; }

    public int Stock { get; set; } = 0;

    [MaxLength(100)]
    public string? Brand { get; set; }           // "JYSK", "Actona" тощо

    // Специфіка меблів
    [MaxLength(100)]
    public string? Material { get; set; }        // "Дуб", "МДФ", "Тканина"

    [MaxLength(50)]
    public string? Color { get; set; }           // "Білий", "Натуральний дуб"

    [MaxLength(100)]
    public string? Dimensions { get; set; }      // "160x200x90 см"

    [MaxLength(50)]
    public string? ArticleNumber { get; set; }   // артикул, як у JYSK

    public bool IsActive { get; set; } = true;
    public bool IsNew { get; set; } = false;     // позначка "Новинка"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FK
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    // Navigation
    public ICollection<ProductImage> Images { get; set; }
    public ICollection<ProductReview> Reviews { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<ProductVariant> Variants { get; set; }
    public ICollection<RoomProduct> RoomProducts { get; set; }
    public ICollection<CollectionProduct> CollectionProducts { get; set; }
}
```

---

## 4. ProductVariant — Варіант товару

> Наприклад, ліжко може бути 140/160/180 см. Диван — у сірому, бежевому, синьому кольорі.
> Кожен варіант має свою ціну та залишок.

```csharp
public class ProductVariant
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }           // "Сірий"

    [MaxLength(50)]
    public string? Size { get; set; }            // "160x200"

    [MaxLength(100)]
    public string? Label { get; set; }           // відображається юзеру: "160×200, Сірий"

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Stock { get; set; } = 0;

    // FK
    public int ProductId { get; set; }
    public Product Product { get; set; }
}
```

---

## 5. ProductImage — Фото товару

```csharp
public class ProductImage
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string ImageUrl { get; set; }

    public bool IsMain { get; set; } = false;

    public int SortOrder { get; set; } = 0;

    // FK
    public int ProductId { get; set; }
    public Product Product { get; set; }
}
```

---

## 6. Room — Кімната / Інтер'єрне рішення

> Аналог розділу "Спальня", "Вітальня", "Дитяча" на сайті JYSK.
> Це не категорія товару, а інспіраційна добірка: "ось як виглядає спальня у скандинавському стилі".

```csharp
public class Room
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; }             // "Спальня", "Вітальня", "Балкон"

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }   // велике фото інтер'єру

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<RoomProduct> RoomProducts { get; set; }
}
```

---

## 7. RoomProduct — Зв'язок кімнати і товарів

```csharp
public class RoomProduct
{
    public int RoomId { get; set; }
    public Room Room { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
}
```

---

## 8. Collection — Колекція / Стиль

> Аналог добірок типу "Скандинавський стиль", "Лофт", "Мінімалізм".
> Один товар може входити до кількох колекцій.

```csharp
public class Collection
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; }             // "Скандинавський стиль", "Лофт"

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<CollectionProduct> CollectionProducts { get; set; }
}
```

---

## 9. CollectionProduct — Зв'язок колекції і товарів

```csharp
public class CollectionProduct
{
    public int CollectionId { get; set; }
    public Collection Collection { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
}
```

---

## 10. Order — Замовлення

```csharp
public class Order
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required, MaxLength(500)]
    public string DeliveryAddress { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    // FK
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }

    // Navigation
    public ICollection<OrderItem> Items { get; set; }
}

public enum OrderStatus
{
    Pending    = 0,   // Очікує підтвердження
    Confirmed  = 1,   // Підтверджено
    Shipped    = 2,   // Відправлено
    Delivered  = 3,   // Доставлено
    Cancelled  = 4    // Скасовано
}
```

---

## 11. OrderItem — Позиція замовлення

```csharp
public class OrderItem
{
    public int Id { get; set; }

    public int Quantity { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }       // ціна зафіксована на момент замовлення

    // Зберігаємо варіант якщо він був обраний (колір/розмір)
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    // FK
    public int OrderId { get; set; }
    public Order Order { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
}
```

---

## 12. ProductReview — Відгук про товар

```csharp
public class ProductReview
{
    public int Id { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FK
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
}
```

---

## 13. DbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer>          Customers          { get; set; }
    public DbSet<Category>          Categories         { get; set; }
    public DbSet<Product>           Products           { get; set; }
    public DbSet<ProductVariant>    ProductVariants    { get; set; }
    public DbSet<ProductImage>      ProductImages      { get; set; }
    public DbSet<Order>             Orders             { get; set; }
    public DbSet<OrderItem>         OrderItems         { get; set; }
    public DbSet<ProductReview>     ProductReviews     { get; set; }
    public DbSet<Room>              Rooms              { get; set; }
    public DbSet<RoomProduct>       RoomProducts       { get; set; }
    public DbSet<Collection>        Collections        { get; set; }
    public DbSet<CollectionProduct> CollectionProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Унікальний email
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        // Складений ключ для зв'язкових таблиць
        modelBuilder.Entity<RoomProduct>()
            .HasKey(rp => new { rp.RoomId, rp.ProductId });

        modelBuilder.Entity<CollectionProduct>()
            .HasKey(cp => new { cp.CollectionId, cp.ProductId });

        // Каскадне видалення позицій при видаленні замовлення
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Заборона каскадного видалення товарів разом з категорією
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

## Швидка шпаргалка по міграціях

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations remove   # відкотити останню міграцію
```
