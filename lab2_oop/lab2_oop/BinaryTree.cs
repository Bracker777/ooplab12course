using System.Collections;

namespace lab2_oop;

/// <summary>
/// Завдання 3-4. Узагальнене бінарне дерево пошуку.
/// Обмеження "where T : class" не дозволяє зберігати структури.
/// Порядок задає IComparer&lt;T&gt;, а якщо його не передано - IComparable&lt;T&gt; самого типу.
/// foreach обходить дерево у зворотному порядку (postorder): ліве піддерево, праве піддерево, корінь.
/// </summary>
public class BinaryTree<T> : IEnumerable<T> where T : class
{
    private readonly IComparer<T> _comparer;
    private Node? _root;
    private int _version;

    public BinaryTree(IComparer<T>? comparer = null)
    {
        _comparer = comparer ?? Comparer<T>.Default;
    }

    public int Count { get; private set; }

    /// <summary>Додає елемент. Елемент, рівний наявному, потрапляє у праве піддерево.</summary>
    public void Add(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var node = new Node(value);
        if (_root is null)
        {
            _root = node;
        }
        else
        {
            Node current = _root;
            while (true)
            {
                if (_comparer.Compare(value, current.Value) < 0)
                {
                    if (current.Left is null)
                    {
                        current.Left = node;
                        break;
                    }

                    current = current.Left;
                }
                else
                {
                    if (current.Right is null)
                    {
                        current.Right = node;
                        break;
                    }

                    current = current.Right;
                }
            }
        }

        Count++;
        _version++;
    }

    /// <summary>Шукає елемент, рівний заданому з погляду порівнювача дерева.</summary>
    public T? Find(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Node? current = _root;
        while (current is not null)
        {
            int result = _comparer.Compare(value, current.Value);
            if (result == 0)
            {
                return current.Value;
            }

            current = result < 0 ? current.Left : current.Right;
        }

        return null;
    }

    public bool Contains(T value) => Find(value) is not null;

    /// <summary>Видаляє один елемент, рівний заданому з погляду порівнювача дерева.</summary>
    public bool Remove(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Node? parent = null;
        Node? current = _root;
        while (current is not null)
        {
            int result = _comparer.Compare(value, current.Value);
            if (result == 0)
            {
                break;
            }

            parent = current;
            current = result < 0 ? current.Left : current.Right;
        }

        if (current is null)
        {
            return false;
        }

        // Вузол із двома нащадками: беремо значення наступника, а видаляємо вузол наступника.
        if (current.Left is not null && current.Right is not null)
        {
            Node successorParent = current;
            Node successor = current.Right;
            while (successor.Left is not null)
            {
                successorParent = successor;
                successor = successor.Left;
            }

            current.Value = successor.Value;
            parent = successorParent;
            current = successor;
        }

        Node? child = current.Left ?? current.Right;
        if (parent is null)
        {
            _root = child;
        }
        else if (parent.Left == current)
        {
            parent.Left = child;
        }
        else
        {
            parent.Right = child;
        }

        Count--;
        _version++;
        return true;
    }

    /// <summary>Центрований обхід: елементи у порядку порівнювача (сортування деревом).</summary>
    public IEnumerable<T> InOrder()
    {
        var stack = new Stack<Node>();
        Node? current = _root;
        while (current is not null || stack.Count > 0)
        {
            while (current is not null)
            {
                stack.Push(current);
                current = current.Left;
            }

            current = stack.Pop();
            yield return current.Value;
            current = current.Right;
        }
    }

    public IEnumerator<T> GetEnumerator() => new PostOrderEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Node(T value)
    {
        public T Value { get; set; } = value;

        public Node? Left { get; set; }

        public Node? Right { get; set; }
    }

    /// <summary>Власний ітератор postorder на явному стеку, без рекурсії та без yield.</summary>
    private sealed class PostOrderEnumerator : IEnumerator<T>
    {
        private readonly BinaryTree<T> _tree;
        private readonly Stack<Node> _stack = new();
        private readonly int _version;
        private Node? _next;
        private Node? _lastVisited;
        private T? _current;

        public PostOrderEnumerator(BinaryTree<T> tree)
        {
            _tree = tree;
            _version = tree._version;
            _next = tree._root;
        }

        public T Current => _current ?? throw new InvalidOperationException(
            "Ітератор не вказує на елемент: викличте MoveNext().");

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            CheckVersion();

            while (_next is not null || _stack.Count > 0)
            {
                if (_next is not null)
                {
                    _stack.Push(_next);
                    _next = _next.Left;
                    continue;
                }

                Node top = _stack.Peek();
                if (top.Right is not null && top.Right != _lastVisited)
                {
                    _next = top.Right;
                    continue;
                }

                _stack.Pop();
                _lastVisited = top;
                _current = top.Value;
                return true;
            }

            _current = null;
            return false;
        }

        public void Reset()
        {
            CheckVersion();

            _stack.Clear();
            _next = _tree._root;
            _lastVisited = null;
            _current = null;
        }

        public void Dispose()
        {
        }

        private void CheckVersion()
        {
            if (_version != _tree._version)
            {
                throw new InvalidOperationException("Дерево змінено під час обходу.");
            }
        }
    }
}
