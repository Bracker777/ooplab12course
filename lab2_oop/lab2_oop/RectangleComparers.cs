namespace lab2_oop;

/// <summary>Зовнішнє порівняння за периметром (за зростанням).</summary>
public class PerimeterComparer : IComparer<Rectangle>
{
    public int Compare(Rectangle? x, Rectangle? y)
    {
        if (x is null || y is null)
        {
            return x is null ? (y is null ? 0 : -1) : 1;
        }

        return x.CalculatePerimeter().CompareTo(y.CalculatePerimeter());
    }
}

/// <summary>Зовнішнє порівняння за площею (за спаданням).</summary>
public class AreaDescendingComparer : IComparer<Rectangle>
{
    public int Compare(Rectangle? x, Rectangle? y)
    {
        if (x is null || y is null)
        {
            return x is null ? (y is null ? 0 : 1) : -1;
        }

        return y.CalculateArea().CompareTo(x.CalculateArea());
    }
}
