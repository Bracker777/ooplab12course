// Рівень представлення (PL): робота з користувачем через консоль.

using System.Globalization;
using System.Text;
using BLL;

namespace PL;

// Клас рівня PL: дані, які бачить користувач.
public class RectangleView
{
    public string FillColor { get; set; }
    public string BorderColor { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Area { get; set; }
    public double Perimeter { get; set; }

    public override string ToString() =>
        $"Прямокутник {Width} x {Height}, заповнення: {FillColor}, контур: {BorderColor}, " +
        $"S = {Area}, P = {Perimeter}";
}

// Назву файлу і формат обирає користувач, і вони передаються звідси на нижчі рівні.
public static class Menu
{
    private static readonly EntityService Service = new();

    public static void MainMenu()
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Кодування для введення кирилиці з вікна консолі або з перенаправленого потоку.
        Console.InputEncoding = Console.IsInputRedirected ? Encoding.UTF8 : Encoding.Unicode;

        while (true)
        {
            PrintOptions();
            string choice = (Console.ReadLine() ?? "0").Trim();
            if (choice == "0")
            {
                return;
            }

            // Винятки виникають на нижчих рівнях, а обробляються тут.
            try
            {
                Execute(choice);
            }
            catch (RectangleAppException e)
            {
                Console.WriteLine($"Помилка: {e.Message}");
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Помилка: {e.Message}");
            }
        }
    }

    private static void PrintOptions()
    {
        Console.WriteLine();
        Console.WriteLine("=== Прямокутники ===");
        Console.WriteLine("1. Створити файл");
        Console.WriteLine("2. Відкрити файл");
        Console.WriteLine("3. Зберегти у файл");
        Console.WriteLine("4. Закрити файл");
        Console.WriteLine("5. Видалити файл");
        Console.WriteLine("6. Додати прямокутник");
        Console.WriteLine("7. Видалити прямокутник");
        Console.WriteLine("8. Знайти за кольором заповнення");
        Console.WriteLine("9. Показати всі");
        Console.WriteLine("0. Вихід");
        Console.Write("Ваш вибір: ");
    }

    private static void Execute(string choice)
    {
        switch (choice)
        {
            case "1":
                StorageFormat createFormat = ReadFormat();
                Service.CreateFile(ReadPath(createFormat), createFormat);
                Console.WriteLine("Файл створено.");
                break;
            case "2":
                StorageFormat openFormat = ReadFormat();
                Service.OpenFile(ReadPath(openFormat), openFormat);
                Console.WriteLine("Файл відкрито.");
                break;
            case "3":
                StorageFormat saveFormat = ReadFormat();
                Service.SaveAs(ReadPath(saveFormat), saveFormat);
                Console.WriteLine("Дані збережено.");
                break;
            case "4":
                Service.CloseFile();
                Console.WriteLine("Файл закрито.");
                break;
            case "5":
                DeleteFile();
                break;
            case "6":
                Service.Add(ReadRectangle());
                Console.WriteLine("Прямокутник додано.");
                break;
            case "7":
                Service.Delete(ReadInt("Номер у списку"));
                Console.WriteLine("Прямокутник видалено.");
                break;
            case "8":
                // Без номерів, бо номер для видалення береться із загального списку.
                foreach (Rectangle rectangle in Service.FindByFillColor(ReadText("Колір заповнення")))
                {
                    Console.WriteLine(ToView(rectangle));
                }

                break;
            case "9":
                PrintAll(Service.GetAll());
                break;
            default:
                Console.WriteLine("Такого пункту немає.");
                break;
        }
    }

    private static void DeleteFile()
    {
        if (ReadText("Видалити поточний файл? (т/н)").ToLower() == "т")
        {
            Service.DeleteFile();
            Console.WriteLine("Файл видалено.");
        }
    }

    private static Rectangle ReadRectangle() => new()
    {
        FillColor = ReadText("Колір заповнення"),
        BorderColor = ReadText("Колір контуру"),
        Width = ReadDouble("Довжина"),
        Height = ReadDouble("Ширина"),
    };

    private static void PrintAll(List<Rectangle> rectangles)
    {
        if (rectangles.Count == 0)
        {
            Console.WriteLine("Записів немає.");
        }

        for (int i = 0; i < rectangles.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {ToView(rectangles[i])}");
        }
    }

    private static RectangleView ToView(Rectangle rectangle) => new()
    {
        FillColor = rectangle.FillColor,
        BorderColor = rectangle.BorderColor,
        Width = rectangle.Width,
        Height = rectangle.Height,
        Area = rectangle.CalculateArea(),
        Perimeter = rectangle.CalculatePerimeter(),
    };

    private static StorageFormat ReadFormat()
    {
        return ReadInt("Формат (1 - бінарний, 2 - XML, 3 - JSON, 4 - користувацький)") switch
        {
            1 => StorageFormat.Binary,
            2 => StorageFormat.Xml,
            3 => StorageFormat.Json,
            4 => StorageFormat.Custom,
            _ => throw new FormatException("Такого формату немає."),
        };
    }

    private static string ReadPath(StorageFormat format)
    {
        string extension = format switch
        {
            StorageFormat.Binary => "bin",
            StorageFormat.Xml => "xml",
            StorageFormat.Json => "json",
            _ => "txt",
        };
        string defaultPath = $"rectangles.{extension}";
        string path = ReadText($"Назва або шлях файлу (Enter - {defaultPath})");
        return path.Length > 0 ? path : defaultPath;
    }

    private static string ReadText(string prompt)
    {
        Console.Write($"{prompt}: ");
        return (Console.ReadLine() ?? "").Trim();
    }

    private static int ReadInt(string prompt)
    {
        return int.TryParse(ReadText(prompt), out int value)
            ? value
            : throw new FormatException("Потрібно ввести ціле число.");
    }

    private static double ReadDouble(string prompt)
    {
        string text = ReadText(prompt).Replace(',', '.');
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && double.IsFinite(value)
            ? value
            : throw new FormatException("Потрібно ввести число.");
    }
}
