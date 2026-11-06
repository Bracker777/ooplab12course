// Модульні тести бізнес-логіки. Кожен тест побудовано за принципом Triple A:
// Arrange (підготовка), Act (дія), Assert (перевірка).

using BLL;
using DAL;
using Xunit;

namespace lab5_oop.Tests;

// Заглушка замість файлу: дані лежать у пам'яті, а тести залишаються швидкими й незалежними.
public class StubRepository : IRepository<Student>
{
    public List<Student> Items { get; } = new();

    public int SaveCount { get; private set; }

    public List<Student> Load() => Items;

    public void Save(List<Student> items) => SaveCount++;
}

public class StudentServiceTests
{
    private readonly StubRepository _repository = new();
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _service = new StudentService(_repository);
    }

    private static Student CreateStudent(
        string card = "S1", int course = 3, double grade = 75, string country = "Україна", bool isExpelled = false) =>
        new()
        {
            Surname = "Шевченко",
            Course = course,
            StudentCard = card,
            AverageGrade = grade,
            Country = country,
            RecordBook = "Z1",
            IsExpelled = isExpelled,
        };

    [Fact]
    public void GetAll_StudentsInRepository_ReturnsAllStudents()
    {
        // Arrange
        _repository.Items.Add(CreateStudent("S1"));
        _repository.Items.Add(CreateStudent("S2"));

        // Act
        List<Student> result = _service.GetAll();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(6, 100)]
    public void Add_BoundaryValues_SavesStudent(int course, double grade)
    {
        // Arrange
        Student student = CreateStudent(course: course, grade: grade);

        // Act
        _service.Add(student);

        // Assert
        Assert.Contains(student, _repository.Items);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public void Add_DuplicateCard_ThrowsAndDoesNotSave()
    {
        // Arrange
        _repository.Items.Add(CreateStudent("S1"));

        // Act
        void Act() => _service.Add(CreateStudent("S1"));

        // Assert
        Assert.Throws<BusinessException>(Act);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Theory]
    [InlineData(0, 75)]
    [InlineData(7, 75)]
    [InlineData(3, -1)]
    [InlineData(3, 101)]
    public void Add_InvalidCourseOrGrade_Throws(int course, double grade)
    {
        // Arrange
        Student student = CreateStudent(course: course, grade: grade);

        // Act
        void Act() => _service.Add(student);

        // Assert
        Assert.Throws<BusinessException>(Act);
    }

    [Theory]
    [InlineData("", "S1", "Україна", "Z1")]
    [InlineData("Шевченко", " ", "Україна", "Z1")]
    [InlineData("Шевченко", "S1", "", "Z1")]
    [InlineData("Шевченко", "S1", "Україна", null)]
    public void Add_EmptyTextField_Throws(string surname, string card, string country, string recordBook)
    {
        // Arrange
        var student = new Student
        {
            Surname = surname,
            Course = 3,
            StudentCard = card,
            AverageGrade = 75,
            Country = country,
            RecordBook = recordBook,
        };

        // Act
        void Act() => _service.Add(student);

        // Assert
        Assert.Throws<BusinessException>(Act);
    }

    [Fact]
    public void GetThirdYearFromUkraine_MixedStudents_ReturnsOnlyMatching()
    {
        // Arrange
        _repository.Items.Add(CreateStudent("MATCH"));
        _repository.Items.Add(CreateStudent("COURSE", course: 2));
        _repository.Items.Add(CreateStudent("COUNTRY", country: "Польща"));
        _repository.Items.Add(CreateStudent("EXPELLED", isExpelled: true));

        // Act
        List<Student> result = _service.GetThirdYearFromUkraine();

        // Assert
        Student student = Assert.Single(result);
        Assert.Equal("MATCH", student.StudentCard);
    }

    [Theory]
    [InlineData("Україна")]
    [InlineData("україна")]
    [InlineData("Ukraine")]
    [InlineData("UKRAINE")]
    public void CountThirdYearFromUkraine_CountryNameVariants_CountsStudent(string country)
    {
        // Arrange
        _repository.Items.Add(CreateStudent(country: country));

        // Act
        int result = _service.CountThirdYearFromUkraine();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void CountThirdYearFromUkraine_NoStudents_ReturnsZero()
    {
        // Arrange: сховище порожнє.

        // Act
        int result = _service.CountThirdYearFromUkraine();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Expel_LowGrade_MarksExpelledAndSaves()
    {
        // Arrange
        Student student = CreateStudent(grade: 59.9);
        _repository.Items.Add(student);

        // Act
        _service.Expel("S1");

        // Assert
        Assert.True(student.IsExpelled);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public void Expel_PassingGrade_ThrowsAndDoesNotExpel()
    {
        // Arrange
        Student student = CreateStudent(grade: StudentService.PassingGrade);
        _repository.Items.Add(student);

        // Act
        void Act() => _service.Expel("S1");

        // Assert
        Assert.Throws<BusinessException>(Act);
        Assert.False(student.IsExpelled);
    }

    [Fact]
    public void Expel_AlreadyExpelled_Throws()
    {
        // Arrange
        _repository.Items.Add(CreateStudent(grade: 10, isExpelled: true));

        // Act
        void Act() => _service.Expel("S1");

        // Assert
        Assert.Throws<BusinessException>(Act);
    }

    [Fact]
    public void Expel_UnknownCard_Throws()
    {
        // Arrange: сховище порожнє.

        // Act
        void Act() => _service.Expel("NOPE");

        // Assert
        Assert.Throws<BusinessException>(Act);
    }

    [Fact]
    public void PlayChess_ExistingStudent_ReturnsMessageWithSurname()
    {
        // Arrange
        _repository.Items.Add(CreateStudent());

        // Act
        string result = _service.PlayChess("S1");

        // Assert
        Assert.Equal("Студент Шевченко грає в шахи.", result);
    }

    [Fact]
    public void PlayChess_UnknownCard_Throws()
    {
        // Arrange: сховище порожнє.

        // Act
        void Act() => _service.PlayChess("NOPE");

        // Assert
        Assert.Throws<BusinessException>(Act);
    }
}

public class EmployeeTests
{
    [Fact]
    public void CookBurger_WorkerWithDifferentIngredients_ReturnsBurger()
    {
        // Arrange
        var worker = new McdonaldsWorker("Іваненко");

        // Act
        string result = worker.CookBurger(new List<string> { "булка", "котлета" });

        // Assert
        Assert.Equal("Працівник McDonald's Іваненко приготував бургер: булка, котлета.", result);
    }

    [Fact]
    public void CookBurger_ManagerWithDifferentIngredients_ReturnsBurger()
    {
        // Arrange
        var manager = new Manager("Петренко");

        // Act
        string result = manager.CookBurger(new List<string> { "булка", "сир", "салат" });

        // Assert
        Assert.Equal("Менеджер Петренко приготував бургер: булка, сир, салат.", result);
    }

    [Fact]
    public void CookBurger_OneIngredient_Throws()
    {
        // Arrange
        var worker = new McdonaldsWorker("Іваненко");

        // Act
        void Act() => worker.CookBurger(new List<string> { "булка" });

        // Assert
        Assert.Throws<BusinessException>(Act);
    }

    [Fact]
    public void CookBurger_RepeatedIngredientInOtherCase_Throws()
    {
        // Arrange
        var manager = new Manager("Петренко");

        // Act
        void Act() => manager.CookBurger(new List<string> { "сир", "СИР" });

        // Assert
        Assert.Throws<BusinessException>(Act);
    }
}
