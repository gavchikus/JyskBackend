# 🌐 REST API — Ендпоінти магазину меблів

> Base URL: `https://localhost:5001/api`
> Формат: JSON
> Авторизація: `Authorization: Bearer <token>`
> 🔓 — публічний | 🔐 — потрібен JWT | 👑 — тільки Admin

---

## Легенда статус-кодів

| Код | Значення |
|-----|----------|
| 200 | Успішно |
| 201 | Створено |
| 400 | Помилка валідації |
| 401 | Не авторизований |
| 403 | Немає прав |
| 404 | Не знайдено |
| 409 | Конфлікт (напр. email вже зайнятий) |

---

## 🔑 AUTH — Авторизація

### POST `/auth/register` 🔓

**Request:**
```json
{
  "firstName":   "Олена",
  "lastName":    "Ковальчук",
  "email":       "olena@example.com",
  "password":    "StrongPass123!",
  "phoneNumber": "+380501234567"
}
```

**Response `201`:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "customer": {
    "id":        1,
    "firstName": "Олена",
    "lastName":  "Ковальчук",
    "email":     "olena@example.com",
    "role":      "Customer"
  }
}
```

---

### POST `/auth/login` 🔓

**Request:**
```json
{
  "email":    "olena@example.com",
  "password": "StrongPass123!"
}
```

**Response `200`:** _(такий самий формат як у register)_

---

## 👤 CUSTOMERS — Користувачі

### GET `/customers/me` 🔐
Профіль поточного користувача.

**Response `200`:**
```json
{
  "id":          1,
  "firstName":   "Олена",
  "lastName":    "Ковальчук",
  "email":       "olena@example.com",
  "phoneNumber": "+380501234567",
  "address":     "Київ, вул. Хрещатик, 1",
  "createdAt":   "2024-03-01T10:00:00Z"
}
```

---

### PUT `/customers/me` 🔐
Оновити профіль.

**Request:**
```json
{
  "firstName":   "Олена",
  "lastName":    "Ковальчук",
  "phoneNumber": "+380501234567",
  "address":     "Київ, вул. Хрещатик, 1"
}
```

**Response `200`:** _(оновлений профіль)_

---

### GET `/customers` 👑
Всі користувачі (тільки Admin).

**Response `200`:**
```json
[
  {
    "id":        1,
    "firstName": "Олена",
    "lastName":  "Ковальчук",
    "email":     "olena@example.com",
    "role":      "Customer",
    "createdAt": "2024-03-01T10:00:00Z"
  }
]
```

---

## 📂 CATEGORIES — Категорії

### GET `/categories` 🔓
Всі категорії (включно з підкатегоріями).

**Response `200`:**
```json
[
  {
    "id":               1,
    "name":             "Ліжка",
    "description":      "Односпальні, двоспальні та розкладні ліжка",
    "imageUrl":         "https://cdn.example.com/cats/beds.jpg",
    "parentCategoryId": null
  },
  {
    "id":               4,
    "name":             "Двоспальні ліжка",
    "imageUrl":         null,
    "parentCategoryId": 1
  }
]
```

---

### GET `/categories/{id}` 🔓

**Response `200`:**
```json
{
  "id":               1,
  "name":             "Ліжка",
  "description":      "Односпальні, двоспальні та розкладні ліжка",
  "imageUrl":         "https://cdn.example.com/cats/beds.jpg",
  "parentCategoryId": null,
  "subCategories": [
    { "id": 4, "name": "Двоспальні ліжка" },
    { "id": 5, "name": "Односпальні ліжка" }
  ]
}
```

---

### POST `/categories` 👑

**Request:**
```json
{
  "name":             "Матраци",
  "description":      "Пружинні та безпружинні матраци",
  "imageUrl":         "https://cdn.example.com/cats/mattresses.jpg",
  "parentCategoryId": null
}
```

**Response `201`:** _(створена категорія)_

---

### PUT `/categories/{id}` 👑
**Request:** _(ті самі поля що в POST)_
**Response `200`:** _(оновлена категорія)_

---

### DELETE `/categories/{id}` 👑
**Response `204`**

---

## 🛋️ PRODUCTS — Товари

### GET `/products` 🔓
Список товарів з фільтрацією та пагінацією.

**Query параметри:**

| Параметр | Тип | Опис | Приклад |
|----------|-----|------|---------|
| `categoryId` | int | Фільтр за категорією | `?categoryId=1` |
| `roomId` | int | Товари з певної кімнати | `?roomId=2` |
| `collectionId` | int | Товари з колекції | `?collectionId=3` |
| `search` | string | Пошук за назвою | `?search=диван` |
| `minPrice` | decimal | Ціна від | `?minPrice=2000` |
| `maxPrice` | decimal | Ціна до | `?maxPrice=30000` |
| `color` | string | Колір | `?color=Білий` |
| `material` | string | Матеріал | `?material=Дуб` |
| `isNew` | bool | Тільки новинки | `?isNew=true` |
| `onSale` | bool | Тільки зі знижкою | `?onSale=true` |
| `page` | int | Сторінка | `?page=1` |
| `pageSize` | int | К-ть на сторінці | `?pageSize=20` |
| `sortBy` | string | Сортування | `?sortBy=price_asc` |

**Значення `sortBy`:** `price_asc`, `price_desc`, `newest`, `rating`

**Response `200`:**
```json
{
  "items": [
    {
      "id":           1,
      "name":         "Ліжко HAUGE 160x200",
      "price":        12999.00,
      "oldPrice":     15999.00,
      "articleNumber":"501.234.56",
      "brand":        "JYSK",
      "color":        "Білий",
      "material":     "МДФ",
      "dimensions":   "160x200x90 см",
      "isNew":        false,
      "categoryId":   4,
      "categoryName": "Двоспальні ліжка",
      "mainImageUrl": "https://cdn.example.com/products/1/main.jpg",
      "avgRating":    4.6,
      "reviewCount":  34
    }
  ],
  "totalCount": 87,
  "page":       1,
  "pageSize":   20,
  "totalPages": 5
}
```

---

### GET `/products/{id}` 🔓
Деталі товару.

**Response `200`:**
```json
{
  "id":           1,
  "name":         "Ліжко HAUGE 160x200",
  "description":  "Класичне скандинавське ліжко з узголів'ям...",
  "price":        12999.00,
  "oldPrice":     15999.00,
  "stock":        8,
  "articleNumber":"501.234.56",
  "brand":        "JYSK",
  "color":        "Білий",
  "material":     "МДФ",
  "dimensions":   "160x200x90 см",
  "isNew":        false,
  "isActive":     true,
  "categoryId":   4,
  "categoryName": "Двоспальні ліжка",
  "images": [
    { "id": 1, "imageUrl": "https://cdn.example.com/products/1/main.jpg",  "isMain": true },
    { "id": 2, "imageUrl": "https://cdn.example.com/products/1/side.jpg",  "isMain": false }
  ],
  "variants": [
    { "id": 1, "label": "140×200, Білий", "color": "Білий", "size": "140x200", "price": 10999.00, "stock": 5 },
    { "id": 2, "label": "160×200, Білий", "color": "Білий", "size": "160x200", "price": 12999.00, "stock": 8 },
    { "id": 3, "label": "180×200, Білий", "color": "Білий", "size": "180x200", "price": 14999.00, "stock": 3 }
  ],
  "avgRating":   4.6,
  "reviewCount": 34
}
```

---

### POST `/products` 👑

**Request:**
```json
{
  "name":          "Диван LEJRE 3-місний",
  "description":   "М'який диван у скандинавському стилі",
  "price":         24999.00,
  "oldPrice":      null,
  "stock":         10,
  "brand":         "JYSK",
  "color":         "Сірий",
  "material":      "Тканина",
  "dimensions":    "220x90x80 см",
  "articleNumber": "701.456.78",
  "isNew":         true,
  "categoryId":    2
}
```

**Response `201`:** _(створений товар)_

---

### PUT `/products/{id}` 👑
**Request:** _(ті самі поля що в POST)_
**Response `200`:**

---

### DELETE `/products/{id}` 👑
**Response `204`**

---

## 🎨 VARIANTS — Варіанти товару

### GET `/products/{productId}/variants` 🔓

**Response `200`:**
```json
[
  { "id": 1, "label": "140×200, Білий", "color": "Білий", "size": "140x200", "price": 10999.00, "stock": 5 },
  { "id": 2, "label": "160×200, Білий", "color": "Білий", "size": "160x200", "price": 12999.00, "stock": 8 }
]
```

---

### POST `/products/{productId}/variants` 👑

**Request:**
```json
{
  "color": "Бежевий",
  "size":  "160x200",
  "label": "160×200, Бежевий",
  "price": 12999.00,
  "stock": 4
}
```

**Response `201`:** _(створений варіант)_

---

### DELETE `/products/{productId}/variants/{variantId}` 👑
**Response `204`**

---

## ⭐ REVIEWS — Відгуки

### GET `/products/{productId}/reviews` 🔓

**Response `200`:**
```json
[
  {
    "id":           1,
    "rating":       5,
    "comment":      "Чудове ліжко, збирається легко, виглядає дорого!",
    "createdAt":    "2024-04-01T12:00:00Z",
    "customerName": "Олена К."
  }
]
```

---

### POST `/products/{productId}/reviews` 🔐

**Request:**
```json
{
  "rating":  5,
  "comment": "Якість відмінна, доставка швидка!"
}
```

**Response `201`:** _(створений відгук)_

---

## 🛏️ ROOMS — Кімнати

> Інспіраційні розділи: "Спальня", "Вітальня", "Балкон", "Дитяча"

### GET `/rooms` 🔓

**Response `200`:**
```json
[
  {
    "id":            1,
    "name":          "Спальня",
    "description":   "Меблі та текстиль для затишної спальні",
    "coverImageUrl": "https://cdn.example.com/rooms/bedroom.jpg"
  },
  {
    "id":            2,
    "name":          "Вітальня",
    "description":   "Дивани, столики та освітлення для вітальні",
    "coverImageUrl": "https://cdn.example.com/rooms/living.jpg"
  }
]
```

---

### GET `/rooms/{id}` 🔓
Кімната + список товарів у ній.

**Response `200`:**
```json
{
  "id":            1,
  "name":          "Спальня",
  "description":   "Меблі та текстиль для затишної спальні",
  "coverImageUrl": "https://cdn.example.com/rooms/bedroom.jpg",
  "products": [
    {
      "id":           1,
      "name":         "Ліжко HAUGE 160x200",
      "price":        12999.00,
      "mainImageUrl": "https://cdn.example.com/products/1/main.jpg"
    }
  ]
}
```

---

### POST `/rooms` 👑

**Request:**
```json
{
  "name":          "Балкон",
  "description":   "Меблі для балкону та тераси",
  "coverImageUrl": "https://cdn.example.com/rooms/balcony.jpg"
}
```

**Response `201`:**

---

### POST `/rooms/{roomId}/products` 👑
Додати товар до кімнати.

**Request:**
```json
{ "productId": 5 }
```

**Response `200`**

---

### DELETE `/rooms/{roomId}/products/{productId}` 👑
Прибрати товар з кімнати.

**Response `204`**

---

## 🎭 COLLECTIONS — Колекції / Стилі

> Добірки за стилем: "Скандинавський", "Лофт", "Мінімалізм", "Провансаль"

### GET `/collections` 🔓

**Response `200`:**
```json
[
  {
    "id":            1,
    "name":          "Скандинавський стиль",
    "description":   "Чисті лінії, натуральні матеріали, функціональність",
    "coverImageUrl": "https://cdn.example.com/collections/scandi.jpg"
  }
]
```

---

### GET `/collections/{id}` 🔓
Колекція + список товарів.

**Response `200`:**
```json
{
  "id":            1,
  "name":          "Скандинавський стиль",
  "description":   "Чисті лінії, натуральні матеріали, функціональність",
  "coverImageUrl": "https://cdn.example.com/collections/scandi.jpg",
  "products": [
    {
      "id":           1,
      "name":         "Ліжко HAUGE 160x200",
      "price":        12999.00,
      "mainImageUrl": "https://cdn.example.com/products/1/main.jpg"
    }
  ]
}
```

---

### POST `/collections` 👑

**Request:**
```json
{
  "name":          "Лофт",
  "description":   "Індустріальний стиль з металом та деревом",
  "coverImageUrl": "https://cdn.example.com/collections/loft.jpg"
}
```

**Response `201`:**

---

### POST `/collections/{collectionId}/products` 👑
Додати товар до колекції.

**Request:**
```json
{ "productId": 7 }
```

**Response `200`**

---

### DELETE `/collections/{collectionId}/products/{productId}` 👑
**Response `204`**

---

## 🛒 ORDERS — Замовлення

### GET `/orders` 🔐
Мої замовлення.

**Response `200`:**
```json
[
  {
    "id":              10,
    "status":          "Shipped",
    "totalAmount":     25998.00,
    "deliveryAddress": "Київ, вул. Хрещатик, 1",
    "createdAt":       "2024-04-10T09:00:00Z",
    "itemCount":       2
  }
]
```

---

### GET `/orders/{id}` 🔐
Деталі замовлення.

**Response `200`:**
```json
{
  "id":              10,
  "status":          "Shipped",
  "totalAmount":     25998.00,
  "deliveryAddress": "Київ, вул. Хрещатик, 1",
  "comment":         "Зателефонуйте за годину до доставки",
  "createdAt":       "2024-04-10T09:00:00Z",
  "items": [
    {
      "productId":    1,
      "productName":  "Ліжко HAUGE 160x200",
      "variantLabel": "160×200, Білий",
      "imageUrl":     "https://cdn.example.com/products/1/main.jpg",
      "quantity":     1,
      "unitPrice":    12999.00
    }
  ]
}
```

---

### POST `/orders` 🔐
Оформити замовлення.

**Request:**
```json
{
  "deliveryAddress": "Київ, вул. Хрещатик, 1",
  "comment":         "Зателефонуйте за годину до доставки",
  "items": [
    { "productId": 1, "variantId": 2, "quantity": 1 },
    { "productId": 8, "variantId": null, "quantity": 2 }
  ]
}
```

> ⚠️ `variantId` — передається тільки якщо юзер обрав конкретний варіант (розмір/колір). Бекенд фіксує ціну з варіанта або з товару — не з запиту фронтенда.

**Response `201`:** _(повна інформація про замовлення)_

---

### PATCH `/orders/{id}/status` 👑

**Request:**
```json
{ "status": "Confirmed" }
```

**Допустимі значення:** `Pending`, `Confirmed`, `Shipped`, `Delivered`, `Cancelled`

**Response `200`:**

---

### GET `/orders/all` 👑
Всі замовлення з фільтром.

**Query:** `?status=Pending&page=1&pageSize=20`

**Response `200`:** _(список замовлень + ім'я покупця)_

---

## Зведена таблиця ендпоінтів

| Метод | URL | Доступ | Опис |
|-------|-----|--------|------|
| POST | `/auth/register` | 🔓 | Реєстрація |
| POST | `/auth/login` | 🔓 | Вхід |
| GET | `/customers/me` | 🔐 | Свій профіль |
| PUT | `/customers/me` | 🔐 | Редагувати профіль |
| GET | `/customers` | 👑 | Всі користувачі |
| GET | `/categories` | 🔓 | Всі категорії |
| GET | `/categories/{id}` | 🔓 | Категорія |
| POST | `/categories` | 👑 | Створити категорію |
| PUT | `/categories/{id}` | 👑 | Оновити категорію |
| DELETE | `/categories/{id}` | 👑 | Видалити категорію |
| GET | `/products` | 🔓 | Список товарів (фільтри) |
| GET | `/products/{id}` | 🔓 | Деталі товару |
| POST | `/products` | 👑 | Створити товар |
| PUT | `/products/{id}` | 👑 | Оновити товар |
| DELETE | `/products/{id}` | 👑 | Видалити товар |
| GET | `/products/{id}/variants` | 🔓 | Варіанти товару |
| POST | `/products/{id}/variants` | 👑 | Додати варіант |
| DELETE | `/products/{id}/variants/{vid}` | 👑 | Видалити варіант |
| GET | `/products/{id}/reviews` | 🔓 | Відгуки |
| POST | `/products/{id}/reviews` | 🔐 | Залишити відгук |
| GET | `/rooms` | 🔓 | Всі кімнати |
| GET | `/rooms/{id}` | 🔓 | Кімната + товари |
| POST | `/rooms` | 👑 | Створити кімнату |
| POST | `/rooms/{id}/products` | 👑 | Додати товар до кімнати |
| DELETE | `/rooms/{id}/products/{pid}` | 👑 | Прибрати товар з кімнати |
| GET | `/collections` | 🔓 | Всі колекції |
| GET | `/collections/{id}` | 🔓 | Колекція + товари |
| POST | `/collections` | 👑 | Створити колекцію |
| POST | `/collections/{id}/products` | 👑 | Додати товар до колекції |
| DELETE | `/collections/{id}/products/{pid}` | 👑 | Прибрати товар з колекції |
| GET | `/orders` | 🔐 | Мої замовлення |
| GET | `/orders/{id}` | 🔐 | Деталі замовлення |
| POST | `/orders` | 🔐 | Оформити замовлення |
| PATCH | `/orders/{id}/status` | 👑 | Змінити статус |
| GET | `/orders/all` | 👑 | Всі замовлення |
