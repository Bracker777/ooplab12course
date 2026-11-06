// Рівень доступу до даних (DAL): сутність, провайдери серіалізації і робота з файлом.

using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace DAL;

public enum DataFormat
{
    Binary,
    Xml,
    Json,
    Custom,
}

// Користувацька серіалізація: об'єкт сам вирішує, які дані зберігати і як їх відновити.
public interface ICustomSerializable
{
    string[] ToFields();

    void FromFields(string[] fields);
}

// Сутність рівня DAL: саме цей клас записується у файли.
public class RectangleEntity : ICustomSerializable
{
    public string FillColor { get; set; }
    public string BorderColor { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

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
}

// Абстракція DataProvider: спільна робота з файлом тут, а спосіб серіалізації - у нащадках.
public abstract class DataProvider<T>
{
    public void Save(string path, T data)
    {
        using var stream = new FileStream(path, FileMode.Create);
        Write(stream, data);
    }

    public T Load(string path)
    {
        using var stream = new FileStream(path, FileMode.Open);
        return Read(stream);
    }

    protected abstract void Write(Stream stream, T data);

    protected abstract T Read(Stream stream);
}

// Бінарна серіалізація. BinaryFormatter у .NET 9 і новіших вилучено через небезпеку,
// тому використано DataContractSerializer зі стандартним бінарним форматом .NET.
public class BinaryProvider<T> : DataProvider<T>
{
    protected override void Write(Stream stream, T data)
    {
        using XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream);
        new DataContractSerializer(typeof(T)).WriteObject(writer, data);
    }

    protected override T Read(Stream stream)
    {
        using XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
        return (T)new DataContractSerializer(typeof(T)).ReadObject(reader);
    }
}

public class XmlProvider<T> : DataProvider<T>
{
    protected override void Write(Stream stream, T data) =>
        new XmlSerializer(typeof(T)).Serialize(stream, data);

    protected override T Read(Stream stream) =>
        (T)new XmlSerializer(typeof(T)).Deserialize(stream);
}

public class JsonProvider<T> : DataProvider<T>
{
    // Кирилиця залишається у файлі читабельною, а не у вигляді кодів \uXXXX.
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    protected override void Write(Stream stream, T data) =>
        JsonSerializer.Serialize(stream, data, Options);

    protected override T Read(Stream stream) =>
        JsonSerializer.Deserialize<T>(stream, Options) ?? throw new InvalidDataException("Файл не містить даних.");
}

// Користувацький формат: один об'єкт у рядку, поля розділені символом ";".
public class CustomProvider<T> : DataProvider<List<T>> where T : ICustomSerializable, new()
{
    protected override void Write(Stream stream, List<T> data)
    {
        using var writer = new StreamWriter(stream);
        foreach (T item in data)
        {
            writer.WriteLine(string.Join(';', item.ToFields().Select(Escape)));
        }
    }

    protected override List<T> Read(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var result = new List<T>();
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            var item = new T();
            item.FromFields(line.Split(';').Select(Uri.UnescapeDataString).ToArray());
            result.Add(item);
        }

        return result;
    }

    // Кодуються лише службові символи, тому текст у файлі залишається читабельним.
    private static string Escape(string field) =>
        (field ?? "").Replace("%", "%25").Replace(";", "%3B").Replace("\r", "%0D").Replace("\n", "%0A");
}

// Вищі рівні отримують єдиний тип помилки і не залежать від деталей роботи з файлами.
public class DataAccessException : Exception
{
    public DataAccessException(string message, Exception inner = null) : base(message, inner)
    {
    }
}

// Створення, відкриття, оновлення, закриття і видалення файлу даних.
public class EntityContext<T> where T : ICustomSerializable, new()
{
    private string _path;
    private DataProvider<List<T>> _provider;

    public List<T> Items { get; private set; }

    public bool IsOpen => _path != null;

    public void Create(string path, DataFormat format)
    {
        if (File.Exists(path))
        {
            throw new DataAccessException($"Файл уже існує: {path}");
        }

        DataProvider<List<T>> provider = CreateProvider(format);
        Guard(() => provider.Save(path, new List<T>()));
        Remember(path, provider, new List<T>());
    }

    public void Open(string path, DataFormat format)
    {
        DataProvider<List<T>> provider = CreateProvider(format);
        List<T> items = null;
        Guard(() => items = provider.Load(path));
        Remember(path, provider, items);
    }

    public void Update() => Guard(() => _provider.Save(_path, Items));

    // Зберігає дані в інший файл або формат і далі працює вже з ним.
    public void SaveAs(string path, DataFormat format)
    {
        DataProvider<List<T>> provider = CreateProvider(format);
        Guard(() => provider.Save(path, Items));
        Remember(path, provider, Items);
    }

    public void Close() => Remember(null, null, null);

    public void Delete()
    {
        Guard(() => File.Delete(_path));
        Close();
    }

    // Кожному типу серіалізації відповідає свій провайдер.
    private static DataProvider<List<T>> CreateProvider(DataFormat format) =>
        format switch
        {
            DataFormat.Binary => new BinaryProvider<List<T>>(),
            DataFormat.Xml => new XmlProvider<List<T>>(),
            DataFormat.Json => new JsonProvider<List<T>>(),
            DataFormat.Custom => new CustomProvider<T>(),
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };

    private void Remember(string path, DataProvider<List<T>> provider, List<T> items)
    {
        _path = path;
        _provider = provider;
        Items = items;
    }

    private static void Guard(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e) when (IsFileError(e))
        {
            throw new DataAccessException($"Не вдалося виконати операцію з файлом. {e.Message}", e);
        }
    }

    // Помилки доступу до файлу та помилки читання файлу в неправильному форматі.
    private static bool IsFileError(Exception e) =>
        e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException
            or InvalidDataException or InvalidOperationException or SerializationException
            or XmlException or JsonException or FormatException or OverflowException or IndexOutOfRangeException;
}
