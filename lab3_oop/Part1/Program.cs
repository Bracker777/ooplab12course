// Лабораторна робота 3.3, частина 1. Дослідження механізму серіалізації.
// Варіант 5: прямокутник, 5 об'єктів.

using System.Globalization;
using System.Text;
using DAL;

Console.OutputEncoding = Encoding.UTF8;

const string Folder = "part1_output";
Directory.CreateDirectory(Folder);

Rectangle[] array =
[
    new() { FillColor = "червоний", BorderColor = "чорний", Width = 4, Height = 5 },
    new() { FillColor = "синій", BorderColor = "білий", Width = 2, Height = 3 },
    new() { FillColor = "зелений", BorderColor = "сірий", Width = 10, Height = 1.5 },
    new() { FillColor = "жовтий", BorderColor = "чорний", Width = 6, Height = 6 },
    new() { FillColor = "білий", BorderColor = "синій", Width = 3, Height = 3 },
];
var list = new List<Rectangle>(array);

Compare("Бінарна", "bin", new BinaryProvider<Rectangle[]>(), new BinaryProvider<List<Rectangle>>());
Compare("XML", "xml", new XmlProvider<Rectangle[]>(), new XmlProvider<List<Rectangle>>());
Compare("JSON", "json", new JsonProvider<Rectangle[]>(), new JsonProvider<List<Rectangle>>());

// Користувацький формат записує об'єкти по рядках, тож масив і колекція дають той самий файл.
Console.WriteLine();
Console.WriteLine("===== Користувацька серіалізація =====");
var custom = new CustomProvider<Rectangle>();
string customArrayPath = Path.Combine(Folder, "array.txt");
string customListPath = Path.Combine(Folder, "list.txt");
custom.Save(customArrayPath, [.. array]);
custom.Save(customListPath, list);
Rectangle[] restoredFromCustom = custom.Load(customArrayPath).ToArray();
PrintRestored(restoredFromCustom);
PrintFileComparison(customArrayPath, customListPath);

// Головна відмінність після відновлення: колекція може рости, а масив - ні.
Console.WriteLine();
Console.WriteLine("===== Масив і колекція після відновлення =====");
List<Rectangle> restoredList = custom.Load(customListPath);
restoredList.Add(new Rectangle { FillColor = "рожевий", BorderColor = "чорний", Width = 1, Height = 1 });
Console.WriteLine($"Колекція: елемент додано методом Add, тепер їх {restoredList.Count}.");
Console.WriteLine($"Масив: розмір фіксований, елементів {restoredFromCustom.Length}, для додавання потрібен новий масив.");

// Масив і колекцію серіалізує той самий узагальнений клас провайдера.
void Compare(string title, string extension, DataProvider<Rectangle[]> arrayProvider, DataProvider<List<Rectangle>> listProvider)
{
    Console.WriteLine();
    Console.WriteLine($"===== {title} серіалізація =====");

    string arrayPath = Path.Combine(Folder, $"array.{extension}");
    string listPath = Path.Combine(Folder, $"list.{extension}");

    arrayProvider.Save(arrayPath, array);
    listProvider.Save(listPath, list);

    // Новий масив, у якому відновлено значення серіалізованих об'єктів.
    Rectangle[] restoredArray = arrayProvider.Load(arrayPath);
    PrintRestored(restoredArray);

    List<Rectangle> restoredCollection = listProvider.Load(listPath);
    Console.WriteLine($"У колекцію відновлено об'єктів: {restoredCollection.Count}");

    PrintFileComparison(arrayPath, listPath);
}

void PrintRestored(Rectangle[] restored)
{
    Console.WriteLine("Новий масив після десеріалізації:");
    foreach (Rectangle rectangle in restored)
    {
        rectangle.PrintInfo();
    }

    bool same = restored.Select(r => r.ToString()).SequenceEqual(array.Select(r => r.ToString()));
    Console.WriteLine($"Збігається з початковим масивом: {(same ? "так" : "ні")}");
}

void PrintFileComparison(string arrayPath, string listPath)
{
    byte[] arrayFile = File.ReadAllBytes(arrayPath);
    byte[] listFile = File.ReadAllBytes(listPath);
    Console.WriteLine($"Розмір файлу: масив {arrayFile.Length} байт, колекція {listFile.Length} байт");
    Console.WriteLine($"Файли однакові: {(arrayFile.SequenceEqual(listFile) ? "так" : "ні")}");
}

// Для серіалізації потрібні відкритий конструктор без параметрів і відкриті властивості.
public class Rectangle : ICustomSerializable
{
    public string FillColor { get; set; }
    public string BorderColor { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public double CalculateArea() => Width * Height;

    public double CalculatePerimeter() => 2 * (Width + Height);

    public void PrintInfo() => Console.WriteLine(this);

    public string[] ToFields() =>
    [
        FillColor,
        BorderColor,
        Width.ToString(CultureInfo.InvariantCulture),
        Height.ToString(CultureInfo.InvariantCulture),
    ];

    public void FromFields(string[] fields)
    {
        FillColor = fields[0];
        BorderColor = fields[1];
        Width = double.Parse(fields[2], CultureInfo.InvariantCulture);
        Height = double.Parse(fields[3], CultureInfo.InvariantCulture);
    }

    public override string ToString() =>
        $"Прямокутник {Width} x {Height}, заповнення: {FillColor}, контур: {BorderColor}, " +
        $"S = {CalculateArea()}, P = {CalculatePerimeter()}";
}
