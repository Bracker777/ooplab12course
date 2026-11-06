// Рівень бізнес-логіки (BLL): модель, власні винятки і сервіс.
// Консоль і потоки файлів тут не використовуються.

using DAL;

namespace BLL;

// Власне перелічення BLL, щоб рівень представлення не залежав від DAL.
public enum StorageFormat
{
    Binary,
    Xml,
    Json,
    Custom,
}

// Модель рівня BLL: містить обчислення, потрібні за варіантом.
public class Rectangle
{
    public string FillColor { get; set; }
    public string BorderColor { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public double CalculateArea() => Width * Height;

    public double CalculatePerimeter() => 2 * (Width + Height);
}

// Власні винятки. Генеруються на рівні BLL, а обробляються в меню (PL).
public class RectangleAppException : Exception
{
    public RectangleAppException(string message, Exception inner = null) : base(message, inner)
    {
    }
}

public class StorageException(string message, Exception inner)
    : RectangleAppException(message, inner);

public class FileNotOpenException()
    : RectangleAppException("Спочатку створіть або відкрийте файл.");

public class InvalidRectangleException(string message)
    : RectangleAppException(message);

public class RectangleNotFoundException(string message)
    : RectangleAppException(message);

public class EntityService
{
    private readonly EntityContext<RectangleEntity> _context = new();

    public void CreateFile(string path, StorageFormat format) =>
        Storage(() => _context.Create(path, ToDataFormat(format)));

    public void OpenFile(string path, StorageFormat format) =>
        Storage(() => _context.Open(path, ToDataFormat(format)));

    public void SaveAs(string path, StorageFormat format)
    {
        CheckOpen();
        Storage(() => _context.SaveAs(path, ToDataFormat(format)));
    }

    public void CloseFile() => _context.Close();

    public void DeleteFile()
    {
        CheckOpen();
        Storage(_context.Delete);
    }

    public void Add(Rectangle rectangle)
    {
        CheckOpen();
        Validate(rectangle);
        _context.Items.Add(ToEntity(rectangle));
        Storage(_context.Update);
    }

    // Номер запису починається з 1, як у списку на екрані.
    public void Delete(int number)
    {
        CheckOpen();
        if (number < 1 || number > _context.Items.Count)
        {
            throw new RectangleNotFoundException($"Запису з номером {number} немає.");
        }

        _context.Items.RemoveAt(number - 1);
        Storage(_context.Update);
    }

    public List<Rectangle> FindByFillColor(string color)
    {
        List<Rectangle> found = GetAll()
            .Where(rectangle => string.Equals(rectangle.FillColor, color, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return found.Count > 0
            ? found
            : throw new RectangleNotFoundException($"Прямокутників із заповненням \"{color}\" немає.");
    }

    public List<Rectangle> GetAll()
    {
        CheckOpen();
        return _context.Items.Select(ToModel).ToList();
    }

    private static void Validate(Rectangle rectangle)
    {
        if (string.IsNullOrWhiteSpace(rectangle.FillColor) || string.IsNullOrWhiteSpace(rectangle.BorderColor))
        {
            throw new InvalidRectangleException("Кольори не можуть бути порожніми.");
        }

        if (rectangle.Width <= 0 || rectangle.Height <= 0)
        {
            throw new InvalidRectangleException("Довжини сторін мають бути більшими за нуль.");
        }
    }

    private static RectangleEntity ToEntity(Rectangle rectangle) => new()
    {
        FillColor = rectangle.FillColor,
        BorderColor = rectangle.BorderColor,
        Width = rectangle.Width,
        Height = rectangle.Height,
    };

    private static Rectangle ToModel(RectangleEntity entity) => new()
    {
        FillColor = entity.FillColor,
        BorderColor = entity.BorderColor,
        Width = entity.Width,
        Height = entity.Height,
    };

    private void CheckOpen()
    {
        if (!_context.IsOpen)
        {
            throw new FileNotOpenException();
        }
    }

    private static void Storage(Action action)
    {
        try
        {
            action();
        }
        catch (DataAccessException e)
        {
            throw new StorageException(e.Message, e);
        }
    }

    private static DataFormat ToDataFormat(StorageFormat format) =>
        format switch
        {
            StorageFormat.Binary => DataFormat.Binary,
            StorageFormat.Xml => DataFormat.Xml,
            StorageFormat.Json => DataFormat.Json,
            StorageFormat.Custom => DataFormat.Custom,
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };
}
