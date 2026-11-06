// Лабораторна робота 3.2. Дослідження колекцій та узагальнених колекцій C#.
// Варіант 5: прямокутник, 5 об'єктів, зворотній порядок обходу дерева (postorder).

using System.Collections;
using System.Text;
using lab2_oop;

Console.OutputEncoding = Encoding.UTF8;

DemonstrateArrayList();
DemonstrateGenericList();
DemonstrateArray();
PrintComparison();
DemonstrateTree();
DemonstrateEnumerator();
DemonstrateGenericTree();
DemonstrateSorting();

// Кожна демонстрація отримує власні 5 об'єктів, щоб зміни в одній не впливали на іншу.
static Rectangle[] CreateRectangles() =>
[
    new("червоний", "чорний", 4, 5),     // S = 20, P = 18
    new("синій", "білий", 2, 3),         // S = 6,  P = 10
    new("зелений", "сірий", 10, 1.5),    // S = 15, P = 23
    new("жовтий", "чорний", 6, 6),       // S = 36, P = 24
    new("білий", "синій", 3, 3),         // S = 9,  P = 12
];

static void Title(string text)
{
    Console.WriteLine();
    Console.WriteLine($"===== {text} =====");
}

static void Print(IEnumerable<Rectangle> items)
{
    foreach (Rectangle item in items)
    {
        item.PrintInfo();
    }
}

static void DemonstrateArrayList()
{
    Title("Завдання 2. ArrayList (non-generic)");
    Rectangle[] rectangles = CreateRectangles();

    // Додавання: зберігається object, тож приймається значення будь-якого типу.
    var list = new ArrayList();
    list.AddRange(rectangles);
    list.Add(42);        // упаковка (boxing): int -> object
    list.Add("рядок");

    Console.WriteLine("Прохід (елементи мають тип object, тип перевіряємо вручну):");
    foreach (object? item in list)
    {
        Console.WriteLine(item is Rectangle rectangle ? rectangle.ToString() : $"не прямокутник: {item}");
    }

    int unboxed = (int)list[5]!;    // розпаковка (unboxing)
    Console.WriteLine($"Розпаковане значення: {unboxed}");

    // Помилка типу з'являється лише під час виконання.
    try
    {
        foreach (Rectangle rectangle in list)
        {
            _ = rectangle.CalculateArea();
        }
    }
    catch (InvalidCastException)
    {
        Console.WriteLine("foreach (Rectangle ...) по ArrayList: InvalidCastException на елементі 42");
    }

    // Видалення.
    list.Remove(42);
    list.RemoveAt(list.Count - 1);
    list.Remove(rectangles[1]);
    Console.WriteLine($"Після видалення: Count = {list.Count}");

    // Оновлення: потрібне приведення типу.
    ((Rectangle)list[0]!).FillColor = "фіолетовий";
    Console.WriteLine($"Після оновлення [0]: {list[0]}");

    // Пошук: порівняння посилань, бо Equals у Rectangle не перевизначено.
    Console.WriteLine($"Індекс зеленого: {list.IndexOf(rectangles[2])}");
    Console.WriteLine($"Містить синій (видалений): {list.Contains(rectangles[1])}");
}

static void DemonstrateGenericList()
{
    Title("Завдання 2. List<Rectangle> (generic)");

    // Додавання: list.Add(42) не скомпілюється, тому помилка типу неможлива під час виконання.
    var list = new List<Rectangle>(CreateRectangles());
    list.Insert(1, new Rectangle("рожевий", "чорний", 1, 1));

    Console.WriteLine("Прохід (приведення типів не потрібне):");
    Print(list);

    // Оновлення.
    list[1].Width = 7;
    Console.WriteLine($"Після оновлення [1]: {list[1]}");

    // Пошук за умовою.
    Console.WriteLine($"Перший із S > 30: {list.Find(r => r.CalculateArea() > 30)}");
    Console.WriteLine($"Із чорним контуром: {list.FindAll(r => r.BorderColor == "чорний").Count}");
    Console.WriteLine($"Індекс зеленого: {list.FindIndex(r => r.FillColor == "зелений")}");

    // Видалення.
    list.RemoveAt(0);
    int removed = list.RemoveAll(r => r.CalculateArea() < 10);
    Console.WriteLine($"Після видалення: Count = {list.Count} (за умовою S < 10 видалено {removed})");
}

static void DemonstrateArray()
{
    Title("Завдання 2. Rectangle[] (масив)");
    Rectangle[] array = CreateRectangles();

    Console.WriteLine("Прохід (for за індексом):");
    for (int i = 0; i < array.Length; i++)
    {
        array[i].PrintInfo();
    }

    // Розмір масиву фіксований.
    try
    {
        array[array.Length] = new Rectangle("сірий", "червоний", 8, 2);
    }
    catch (IndexOutOfRangeException)
    {
        Console.WriteLine("Запис за межі масиву: IndexOutOfRangeException");
    }

    // Додавання: Array.Resize створює новий масив і копіює елементи.
    Array.Resize(ref array, array.Length + 1);
    array[^1] = new Rectangle("сірий", "червоний", 8, 2);
    Console.WriteLine($"Після додавання: Length = {array.Length}");

    // Оновлення.
    array[0].FillColor = "помаранчевий";
    Console.WriteLine($"Після оновлення [0]: {array[0]}");

    // Пошук.
    Console.WriteLine($"Знайдено: {Array.Find(array, r => r.FillColor == "зелений")}");
    int index = Array.FindIndex(array, r => r.FillColor == "синій");
    Console.WriteLine($"Індекс синього: {index}");

    // Видалення: зсув елементів вліво і зменшення масиву.
    Array.Copy(array, index + 1, array, index, array.Length - index - 1);
    Array.Resize(ref array, array.Length - 1);
    Console.WriteLine($"Після видалення: Length = {array.Length}");
}

static void PrintComparison()
{
    Title("Завдання 2. Порівняння");
    Console.WriteLine(
        """
        ArrayList: розмір змінюється; елементи мають тип object, тому потрібні приведення,
                   типи-значення упаковуються, а помилка типу виникає лише під час виконання.
        List<T>:   розмір змінюється; тип перевіряє компілятор, упаковки та приведень немає,
                   є методи пошуку й видалення за умовою (Find, FindAll, RemoveAll).
        Масив:     розмір фіксований, додавання й видалення означають створення нового масиву
                   і копіювання; типізований, доступ за індексом найпростіший і найшвидший.
        """);
}

static void DemonstrateTree()
{
    Title("Завдання 3-4. Бінарне дерево (IComparable<T>: за площею)");
    Rectangle[] rectangles = CreateRectangles();

    var tree = new BinaryTree<Rectangle>();
    foreach (Rectangle rectangle in rectangles)
    {
        tree.Add(rectangle);
    }

    //         20
    //        /  \
    //       6    36
    //        \
    //         15
    //        /
    //       9
    Console.WriteLine($"Кількість елементів: {tree.Count}");
    Console.WriteLine("Postorder (власний ітератор, очікувані площі 9, 15, 6, 36, 20):");
    Print(tree);

    Console.WriteLine("Inorder (за зростанням площі):");
    Print(tree.InOrder());

    // Пошук за порівнювачем: це інший об'єкт із тією самою площею та периметром.
    Console.WriteLine($"Пошук фігури 5 x 4: {tree.Find(new Rectangle("-", "-", 5, 4))}");
    Console.WriteLine($"Містить фігуру 1 x 1: {tree.Contains(new Rectangle("-", "-", 1, 1))}");

    tree.Remove(rectangles[0]);     // корінь із двома нащадками
    Console.WriteLine($"Postorder після видалення кореня (S = 20): {string.Join(", ", tree.Select(r => r.CalculateArea()))}");

    Title("Завдання 4. Бінарне дерево (IComparer<T>: за периметром)");
    var byPerimeter = new BinaryTree<Rectangle>(new PerimeterComparer());
    foreach (Rectangle rectangle in CreateRectangles())
    {
        byPerimeter.Add(rectangle);
    }

    Console.WriteLine($"Postorder, периметри: {string.Join(", ", byPerimeter.Select(r => r.CalculatePerimeter()))}");
    Console.WriteLine($"Inorder, периметри:   {string.Join(", ", byPerimeter.InOrder().Select(r => r.CalculatePerimeter()))}");
}

static void DemonstrateEnumerator()
{
    Title("Завдання 4. Власний ітератор без foreach");
    var tree = new BinaryTree<Rectangle>();
    foreach (Rectangle rectangle in CreateRectangles())
    {
        tree.Add(rectangle);
    }

    using IEnumerator<Rectangle> enumerator = tree.GetEnumerator();
    while (enumerator.MoveNext())
    {
        Console.WriteLine($"MoveNext -> S = {enumerator.Current.CalculateArea()}");
    }

    enumerator.Reset();
    enumerator.MoveNext();
    Console.WriteLine($"Після Reset перший елемент: S = {enumerator.Current.CalculateArea()}");

    // Як і колекції .NET, ітератор не дозволяє змінювати дерево під час обходу.
    try
    {
        foreach (Rectangle rectangle in tree)
        {
            tree.Add(new Rectangle("сірий", "сірий", 1, 1));
        }
    }
    catch (InvalidOperationException exception)
    {
        Console.WriteLine($"Зміна під час обходу: {exception.Message}");
    }
}

static void DemonstrateGenericTree()
{
    Title("Завдання 3. Узагальненість: дерево рядків");
    var words = new BinaryTree<string>();
    foreach (string word in new[] { "груша", "вишня", "слива", "абрикос", "яблуко" })
    {
        words.Add(word);
    }

    Console.WriteLine($"Postorder: {string.Join(", ", words)}");
    Console.WriteLine($"Inorder:   {string.Join(", ", words.InOrder())}");

    // new BinaryTree<int>() не скомпілюється: int - структура (обмеження where T : class).
}

static void DemonstrateSorting()
{
    Title("Завдання 4. Методи сортування");
    Rectangle[] array = CreateRectangles();
    var list = new List<Rectangle>(array);

    list.Sort();    // IComparable<T>
    Console.WriteLine("List.Sort() - IComparable<T>, за площею:");
    Print(list);

    list.Sort(new PerimeterComparer());     // IComparer<T>
    Console.WriteLine("List.Sort(IComparer<T>) - за периметром:");
    Print(list);

    Array.Sort(array, new AreaDescendingComparer());    // IComparer<T>
    Console.WriteLine("Array.Sort(array, IComparer<T>) - за спаданням площі:");
    Print(array);
}
