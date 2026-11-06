// Рівень доступу до даних (DAL): читання і запис файлу через JSON-серіалізацію.

using System.Text.Encodings.Web;
using System.Text.Json;

namespace DAL;

// Абстракція сховища. BLL залежить від неї, тому в тестах файл замінюється заглушкою.
public interface IRepository<T>
{
    List<T> Load();

    void Save(List<T> items);
}

public class JsonRepository<T> : IRepository<T>
{
    // Кирилиця залишається у файлі читабельною, а не у вигляді кодів \uXXXX.
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly string _path;

    public JsonRepository(string path)
    {
        _path = path;
    }

    public List<T> Load()
    {
        if (!File.Exists(_path))
        {
            return new List<T>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(_path), Options) ?? new List<T>();
        }
        catch (JsonException e)
        {
            throw new InvalidDataException($"Файл {_path} пошкоджено.", e);
        }
    }

    public void Save(List<T> items) => File.WriteAllText(_path, JsonSerializer.Serialize(items, Options));
}
