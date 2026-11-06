// Лабораторна робота 3.4. Створення та використання делегатів, подій та лямбда-виразів.
// Варіант 5: кількість цифр у рядку, клас "Стек", подія "Очищення стеку".

using System.Text;

Console.OutputEncoding = Encoding.UTF8;

DemonstrateDelegates();
DemonstrateEvent();

// Завдання 1. Анонімний метод і лямбда-вираз викликаються через делегат.
static void DemonstrateDelegates()
{
    Console.WriteLine("===== Завдання 1. Кількість цифр у рядку =====");

    // Анонімний метод.
    DigitCounter anonymousMethod = delegate (string text)
    {
        int count = 0;
        foreach (char symbol in text)
        {
            if (char.IsDigit(symbol))
            {
                count++;
            }
        }

        return count;
    };

    // Лямбда-вираз.
    DigitCounter lambda = text => text.Count(char.IsDigit);

    foreach (string text in new[] { "Група ІПЗ-21, кімната 305", "без цифр", "" })
    {
        Console.WriteLine($"\"{text}\": анонімний метод = {anonymousMethod(text)}, лямбда-вираз = {lambda(text)}");
    }
}

// Завдання 2-3. Стек генерує подію, а StackMonitor реагує на неї.
static void DemonstrateEvent()
{
    Console.WriteLine();
    Console.WriteLine("===== Завдання 2-3. Подія очищення стеку =====");

    var stack = new MyStack<int>("Стек чисел");
    var first = new StackMonitor("Перший спостерігач");
    var second = new StackMonitor("Другий спостерігач");

    // Підписка на подію: один делегат викликає обидва обробники.
    stack.Cleared += first.OnStackCleared;
    stack.Cleared += second.OnStackCleared;

    stack.Push(10);
    stack.Push(20);
    stack.Push(30);
    Console.WriteLine($"Після додавання: {stack}");
    Console.WriteLine($"Видалено елемент: {stack.Pop()}");
    Console.WriteLine($"Після видалення: {stack}");

    Console.WriteLine("Очищення, підписано двох спостерігачів:");
    stack.Clear();

    Console.WriteLine("Очищення порожнього стеку, видаляти нічого, тож події немає:");
    stack.Clear();

    // Скасування підписки: другий спостерігач більше не отримує подію.
    stack.Cleared -= second.OnStackCleared;
    stack.Push(40);
    Console.WriteLine("Очищення після відписки другого спостерігача:");
    stack.Clear();

    // Той самий клас працює з іншим типом даних.
    var words = new MyStack<string>("Стек рядків");
    words.Cleared += first.OnStackCleared;
    words.Push("один");
    words.Push("два");
    Console.WriteLine("Очищення стеку рядків:");
    words.Clear();

    try
    {
        words.Pop();
    }
    catch (InvalidOperationException exception)
    {
        Console.WriteLine($"Видалення з порожнього стеку: {exception.Message}");
    }
}

// Делегат для методу, який рахує цифри в рядку.
public delegate int DigitCounter(string text);

// Клас-аргумент події: дані, які подія передає обробникам.
public class StackClearedEventArgs : EventArgs
{
    public StackClearedEventArgs(int removedCount)
    {
        RemovedCount = removedCount;
    }

    public int RemovedCount { get; }
}

// Компонент багаторазового використання: узагальнений стек, у якому генерується подія.
public class MyStack<T>
{
    private readonly List<T> _items = new();

    // Ініціалізація стеку.
    public MyStack(string name)
    {
        Name = name;
    }

    // Подія очищення стеку зі стандартним делегатом EventHandler<T>.
    public event EventHandler<StackClearedEventArgs> Cleared;

    public string Name { get; }

    public int Count => _items.Count;

    public void Push(T item) => _items.Add(item);

    public T Pop()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Стек порожній.");
        }

        T item = _items[^1];
        _items.RemoveAt(_items.Count - 1);
        return item;
    }

    public void Clear()
    {
        int removedCount = _items.Count;
        if (removedCount > 0)
        {
            _items.Clear();
            OnCleared(new StackClearedEventArgs(removedCount));
        }
    }

    public override string ToString() => $"{Name}, елементів: {Count}";

    // Метод, який викликає подію. Оператор ?. перевіряє, що є хоча б один підписник.
    protected virtual void OnCleared(StackClearedEventArgs e) => Cleared?.Invoke(this, e);
}

// Клас із методом-обробником події.
public class StackMonitor
{
    private readonly string _name;

    public StackMonitor(string name)
    {
        _name = name;
    }

    // Сигнатура як у стандартних подій .NET: ініціатор події та її аргумент.
    public void OnStackCleared(object sender, StackClearedEventArgs e)
    {
        Console.WriteLine($"{_name}: ініціатор \"{sender}\", видалено елементів: {e.RemovedCount}");
    }
}
