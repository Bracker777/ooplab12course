// Рівень бізнес-логіки (BLL): сутності, контроль даних і операції. Консолі та файлів тут немає.

using DAL;

namespace BLL;

// Порушення правил предметної області.
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}

public class Student
{
    public string Surname { get; set; }
    public int Course { get; set; }
    public string StudentCard { get; set; }
    public double AverageGrade { get; set; }
    public string Country { get; set; }
    public string RecordBook { get; set; }
    public bool IsExpelled { get; set; }

    // Додаткове вміння за варіантом.
    public string PlayChess() => $"Студент {Surname} грає в шахи.";
}

// Додаткові сутності мають спільну операцію, тому вона винесена в базовий клас.
public abstract class Employee
{
    protected Employee(string surname)
    {
        Surname = surname;
    }

    public string Surname { get; }

    protected abstract string Title { get; }

    // Бургер готується лише з різних інгредієнтів.
    public string CookBurger(List<string> ingredients)
    {
        if (ingredients.Count < 2)
        {
            throw new BusinessException("Для бургера потрібно щонайменше 2 інгредієнти.");
        }

        if (ingredients.Distinct(StringComparer.OrdinalIgnoreCase).Count() != ingredients.Count)
        {
            throw new BusinessException("Інгредієнти не повинні повторюватися.");
        }

        return $"{Title} {Surname} приготував бургер: {string.Join(", ", ingredients)}.";
    }
}

public class McdonaldsWorker : Employee
{
    public McdonaldsWorker(string surname) : base(surname)
    {
    }

    protected override string Title => "Працівник McDonald's";
}

public class Manager : Employee
{
    public Manager(string surname) : base(surname)
    {
    }

    protected override string Title => "Менеджер";
}

public class StudentService
{
    // Менший середній бал вважається неуспішністю.
    public const double PassingGrade = 60;

    private readonly IRepository<Student> _repository;

    // Ін'єкція залежності: сховище передається ззовні через конструктор.
    public StudentService(IRepository<Student> repository)
    {
        _repository = repository;
    }

    public List<Student> GetAll() => _repository.Load();

    public void Add(Student student)
    {
        Validate(student);

        List<Student> students = _repository.Load();
        if (students.Any(s => s.StudentCard == student.StudentCard))
        {
            throw new BusinessException($"Студент із квитком {student.StudentCard} уже існує.");
        }

        students.Add(student);
        _repository.Save(students);
    }

    // Студенти 3-го курсу, які проживають в Україні. Дані щоразу читаються з файлу.
    public List<Student> GetThirdYearFromUkraine() =>
        _repository.Load().Where(s => s.Course == 3 && !s.IsExpelled && IsUkraine(s.Country)).ToList();

    public int CountThirdYearFromUkraine() => GetThirdYearFromUkraine().Count;

    // Відрахування за неуспішність.
    public void Expel(string studentCard)
    {
        List<Student> students = _repository.Load();
        Student student = Find(students, studentCard);

        if (student.IsExpelled)
        {
            throw new BusinessException($"Студента {student.Surname} уже відраховано.");
        }

        if (student.AverageGrade >= PassingGrade)
        {
            throw new BusinessException($"Студент {student.Surname} встигає, відрахування неможливе.");
        }

        student.IsExpelled = true;
        _repository.Save(students);
    }

    public string PlayChess(string studentCard) => Find(_repository.Load(), studentCard).PlayChess();

    private static Student Find(List<Student> students, string studentCard) =>
        students.FirstOrDefault(s => s.StudentCard == studentCard)
        ?? throw new BusinessException($"Студента з квитком {studentCard} не знайдено.");

    private static bool IsUkraine(string country) =>
        string.Equals(country, "Україна", StringComparison.OrdinalIgnoreCase)
        || string.Equals(country, "Ukraine", StringComparison.OrdinalIgnoreCase);

    // Контроль даних.
    private static void Validate(Student student)
    {
        if (string.IsNullOrWhiteSpace(student.Surname)
            || string.IsNullOrWhiteSpace(student.StudentCard)
            || string.IsNullOrWhiteSpace(student.Country)
            || string.IsNullOrWhiteSpace(student.RecordBook))
        {
            throw new BusinessException("Усі текстові поля мають бути заповнені.");
        }

        if (student.Course < 1 || student.Course > 6)
        {
            throw new BusinessException("Курс має бути від 1 до 6.");
        }

        if (student.AverageGrade < 0 || student.AverageGrade > 100)
        {
            throw new BusinessException("Середній бал має бути від 0 до 100.");
        }
    }
}
