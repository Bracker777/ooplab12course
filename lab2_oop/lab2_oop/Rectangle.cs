namespace lab2_oop;

/// <summary>
/// Завдання 1. Прямокутник: колір заповнення, колір контуру, довжини сторін.
/// IComparable&lt;T&gt; задає природний порядок: за площею, за однакової площі - за периметром.
/// </summary>
public class Rectangle : IComparable<Rectangle>
{
    private double _width;
    private double _height;

    public Rectangle(string fillColor, string borderColor, double width, double height)
    {
        FillColor = fillColor;
        BorderColor = borderColor;
        Width = width;
        Height = height;
    }

    public string FillColor { get; set; }

    public string BorderColor { get; set; }

    public double Width
    {
        get => _width;
        set => _width = ValidateSide(value);
    }

    public double Height
    {
        get => _height;
        set => _height = ValidateSide(value);
    }

    /// <summary>Обчислення площі.</summary>
    public double CalculateArea() => Width * Height;

    /// <summary>Обчислення периметра.</summary>
    public double CalculatePerimeter() => 2 * (Width + Height);

    /// <summary>Виведення інформації про фігуру.</summary>
    public void PrintInfo() => Console.WriteLine(this);

    public int CompareTo(Rectangle? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = CalculateArea().CompareTo(other.CalculateArea());
        return result != 0 ? result : CalculatePerimeter().CompareTo(other.CalculatePerimeter());
    }

    public override string ToString() =>
        $"Прямокутник {Width} x {Height}, заповнення: {FillColor}, контур: {BorderColor}, " +
        $"S = {CalculateArea()}, P = {CalculatePerimeter()}";

    private static double ValidateSide(double value)
    {
        // Умова записана так, щоб значення NaN теж не проходило перевірку.
        if (!(value > 0) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Довжина сторони має бути додатним числом.");
        }

        return value;
    }
}
