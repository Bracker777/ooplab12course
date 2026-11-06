// Рівень представлення (PL): консольний застосунок.
// Лабораторна робота 3.5. Варіант 5: студенти 3-го курсу з України, відрахування за
// неуспішність, McdonaldsWorker і Manager готують бургери, вміння грати в шахи.

using System.Globalization;
using System.Text;
using BLL;
using DAL;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Console.IsInputRedirected ? Encoding.UTF8 : Encoding.Unicode;

// Тут задається конкретне сховище, яке передається в сервіс (ін'єкція залежності).
var service = new StudentService(new JsonRepository<Student>("students.json"));

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. Додати студента");
    Console.WriteLine("2. Показати всіх студентів");
    Console.WriteLine("3. Студенти 3-го курсу, які проживають в Україні");
    Console.WriteLine("4. Відрахувати студента за неуспішність");
    Console.WriteLine("5. Студент грає в шахи");
    Console.WriteLine("6. Працівник McDonald's готує бургер");
    Console.WriteLine("7. Менеджер готує бургер");
    Console.WriteLine("0. Вихід");
    Console.Write("Ваш вибір: ");

    string choice = (Console.ReadLine() ?? "0").Trim();
    if (choice == "0")
    {
        return;
    }

    try
    {
        Execute(choice);
    }
    catch (Exception e) when (e is BusinessException or FormatException or IOException
        or InvalidDataException or UnauthorizedAccessException)
    {
        Console.WriteLine($"Помилка: {e.Message}");
    }
}

void Execute(string choice)
{
    switch (choice)
    {
        case "1":
            service.Add(ReadStudent());
            Console.WriteLine("Студента додано.");
            break;
        case "2":
            Print(service.GetAll());
            break;
        case "3":
            Console.WriteLine($"Кількість: {service.CountThirdYearFromUkraine()}");
            Print(service.GetThirdYearFromUkraine());
            break;
        case "4":
            service.Expel(ReadText("Студентський квиток"));
            Console.WriteLine("Студента відраховано.");
            break;
        case "5":
            Console.WriteLine(service.PlayChess(ReadText("Студентський квиток")));
            break;
        case "6":
            Console.WriteLine(new McdonaldsWorker(ReadText("Прізвище")).CookBurger(ReadIngredients()));
            break;
        case "7":
            Console.WriteLine(new Manager(ReadText("Прізвище")).CookBurger(ReadIngredients()));
            break;
        default:
            Console.WriteLine("Такого пункту немає.");
            break;
    }
}

Student ReadStudent() => new()
{
    Surname = ReadText("Прізвище"),
    Course = ReadInt("Курс"),
    StudentCard = ReadText("Студентський квиток"),
    AverageGrade = ReadDouble("Середній бал"),
    Country = ReadText("Країна"),
    RecordBook = ReadText("Номер залікової книжки"),
};

List<string> ReadIngredients() =>
    ReadText("Інгредієнти через кому")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList();

void Print(List<Student> students)
{
    if (students.Count == 0)
    {
        Console.WriteLine("Студентів немає.");
    }

    foreach (Student s in students)
    {
        string status = s.IsExpelled ? ", відраховано" : "";
        Console.WriteLine(
            $"{s.Surname}, курс: {s.Course}, квиток: {s.StudentCard}, бал: {s.AverageGrade}, " +
            $"країна: {s.Country}, залікова: {s.RecordBook}{status}");
    }
}

string ReadText(string prompt)
{
    Console.Write($"{prompt}: ");
    return (Console.ReadLine() ?? "").Trim();
}

int ReadInt(string prompt) =>
    int.TryParse(ReadText(prompt), out int value)
        ? value
        : throw new FormatException("Потрібно ввести ціле число.");

double ReadDouble(string prompt)
{
    string text = ReadText(prompt).Replace(',', '.');
    return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && double.IsFinite(value)
        ? value
        : throw new FormatException("Потрібно ввести число.");
}
