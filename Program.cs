using System.Net;

namespace Lab05
{
  // Типы шагов для гибкого вывода
  public abstract class SortStep { }

  public sealed class DataStep : SortStep
  {
    public int[] Data { get; }
    public DataStep(int[] data)
    {
      Data = data ?? throw new ArgumentNullException(nameof(data));
    }
  }

  public sealed class RangeStep : SortStep
  {
    public int Start { get; }
    public int End { get; }
    public RangeStep(int start, int end)
    {
      Start = start;
      End = end;
    }

    public override string ToString() => $"{Start}-{End}";
  }

  public sealed class InfoStep : SortStep
  {
    public string Message { get; }
    public InfoStep(string message) => Message = message ?? throw new ArgumentNullException(nameof(message));
  }

  // Узел дерева диапазонов для вложенного вывода
  public sealed class RangeNode
  {
    public int Start { get; }
    public int End { get; }
    public List<RangeNode> Children { get; } = new();

    public RangeNode(int start, int end)
    {
      Start = start;
      End = end;
    }

    public override string ToString()
    {
      if (Children.Count == 0)
        return $"{Start}-{End}";

      var childrenStr = string.Join(" ", Children.Select(c => c.ToString()));
      return $"[{Start}-{End} {childrenStr}]";
    }
  }

  public class CommonUtilities
  {
    static public int GetAndValidateNumber(string? prompt, int min = -10_000, int max = 10_000)
    {
      try
      {
        if (prompt != null)
          Console.Write(prompt + ": ");
        int numb = int.Parse(Console.ReadLine() ?? ""); // Может выбросить FormatException или OverflowException, ?? "" для того, чтобы не было null (мозолит глаза)
        if (numb > max || numb < min)
          throw new ArgumentOutOfRangeException(null, null, $"Ошибка: число вне диапазона [{min} ... {max}].");
        return numb; // Возвращаем валидный коэффициент
      }
      catch (Exception end)
      {
        switch (end)
        {
          case FormatException:
            throw new FormatException("Ошибка: введено не число.");
          case OverflowException:
            throw new OverflowException($"Ошибка: число слишком большое или слишком маленькое, вне диапозона [{min} ... {max}].");
          default:
            throw end;
        }
      }
    }

    static public bool GetAndValidateNumberAsBool(string? prompt)
    {
      try
      {
        if (prompt != null)
          Console.Write(prompt + ": ");
        int numb = int.Parse(Console.ReadLine() ?? ""); // Может выбросить FormatException или OverflowException, ?? "" для того, чтобы не было null (мозолит глаза)
        if (numb != 0 && numb != 1)
          throw new ArgumentOutOfRangeException(null, null, "Ошибка: введено не 0 и не 1.");
        return numb == 1; // Возвращаем валидный коэффициент
      }
      catch (Exception end)
      {
        switch (end)
        {
          case FormatException:
            throw new FormatException("Ошибка: введено не число.");
          case OverflowException:
            throw new OverflowException("Ошибка: число слишком большое или слишком маленькое.");
          default:
            throw end;
        }
      }
    }
  }

  // Класс для операций с массивами (только чистые функции)
  // Так как в вводе есть порядок сортировок, то методы сортировок будут генераторами (yield return синтаксис)
  // Каждая сортировка содает локальный инстанс массива, чтобы не менять исходный массив, так называеемая "чистота функций" и транзакционность
  public class ArrayOperations
  {
    static public int[] GenerateArray(int length, int minimum = -100, int maximum = 100)
    {
      Random rand = new Random();
      int[] array = new int[length];
      for (int i = 0; i < length; i++)
      {
        array[i] = rand.Next(minimum, maximum + 1); // +1 потому что верхняя граница не включается
      }
      return array;
    }

    static public void PrintArray(int[] array, string? prefix = null)
    {
      if (prefix != null)
        Console.Write(prefix);

      // Выравнивание через определение ширины самого большого числа
      int maxWidth = array.Max(x => x.ToString().Length);
      string formatted = string.Join(" ", array.Select(x => x.ToString().PadLeft(maxWidth)));
      Console.WriteLine(formatted);
    }

    static public IEnumerable<SortStep> BubbleSort(int[] array, bool ascending = true) // Пузырьковая сортировка, генератор
    {
      int[] currentArray = (int[])array.Clone();
      yield return new DataStep(currentArray); // Исходный массив

      int n = currentArray.Length;
      for (int i = 0; i < n - 1; i++)
      {
        for (int j = 0; j < n - i - 1; j++)
        {
          bool shouldSwap = ascending ? (currentArray[j] > currentArray[j + 1]) : (currentArray[j] < currentArray[j + 1]);
          if (shouldSwap)
          {
            // Меняем элементы местами
            (currentArray[j], currentArray[j + 1]) = (currentArray[j + 1], currentArray[j]);
            yield return new DataStep((int[])currentArray.Clone());
          }
        }
      }
      yield return new DataStep(currentArray); // Финальный массив (на случай, если не было обменов)
    }

    static public IEnumerable<SortStep> SelectionSort(int[] array, bool ascending = true) // Сортировка выбором, генератор
    {
      int[] currentArray = (int[])array.Clone();
      yield return new DataStep(currentArray);

      int n = currentArray.Length;
      for (int i = 0; i < n - 1; i++)
      {
        int minIndex = i;
        for (int j = i + 1; j < n; j++)
        {
          bool shouldUpdate = ascending ? (currentArray[j] < currentArray[minIndex]) : (currentArray[j] > currentArray[minIndex]);
          if (shouldUpdate)
          {
            minIndex = j;
          }
        }
        // Меняем найденный минимальный элемент с первым элементом
        (currentArray[minIndex], currentArray[i]) = (currentArray[i], currentArray[minIndex]);
        yield return new DataStep((int[])currentArray.Clone());
      }
    }

    static public IEnumerable<SortStep> InsertionSort(int[] array, bool ascending = true) // Сортировка вставками, генератор
    {
      int[] currentArray = (int[])array.Clone();
      yield return new DataStep(currentArray);

      int n = currentArray.Length;
      for (int i = 1; i < n; i++)
      {
        int key = currentArray[i];
        int j = i - 1;

        // Перемещаем элементы array[0..i-1], которые больше key, на одну позицию вперед
        while (j >= 0 && (ascending ? currentArray[j] > key : currentArray[j] < key))
        {
          currentArray[j + 1] = currentArray[j];
          j--;
          yield return new DataStep((int[])currentArray.Clone());
        }
        currentArray[j + 1] = key;
        yield return new DataStep((int[])currentArray.Clone());
      }
    }

    static public IEnumerable<SortStep> CountingSort(int[] array, bool ascending = true) // Сортировка подсчётом, генератор
    {
      int[] currentArray = (int[])array.Clone();
      yield return new DataStep(currentArray);

      int max = currentArray.Max();
      int min = currentArray.Min();
      int range = max - min + 1; // Диапазон значений в массиве
      int[] count = new int[range]; // Массив для подсчёта вхождений каждого элемента
      int n = currentArray.Length;

      // Подсчитываем количество вхождений каждого элемента
      for (int i = 0; i < n; i++)
      {
        count[currentArray[i] - min]++;
      }

      int index = 0;
      if (ascending)
        for (int i = 0; i < range; i++) // Проходим по массиву подсчёта
        {
          while (count[i] > 0) // Пока есть вхождения элемента
          {
            currentArray[index++] = i + min; // Восстанавливаем значение элемента
            count[i]--;
            yield return new DataStep((int[])currentArray.Clone());
          }
        }
      else
        for (int i = range - 1; i >= 0; i--) // Проходим по массиву подсчёта в обратном порядке
        {
          while (count[i] > 0) // Пока есть вхождения элемента
          {
            currentArray[index++] = i + min; // Восстанавливаем значение элемента
            count[i]--;
            yield return new DataStep((int[])currentArray.Clone());
          }
        }
    }

    static public IEnumerable<SortStep> QuickSort(int[] array, bool ascending = true) // Быстрая сортировка, генератор
    {
      int[] currentArray = (int[])array.Clone();
      yield return new DataStep(currentArray); // Исходный массив

      var root = new RangeNode(0, array.Length - 1);
      foreach (var step in QuickSortInternal(currentArray, 0, array.Length - 1, ascending, root))
        yield return step;

      // Выводим структуру разбиения
      yield return new InfoStep(root.ToString());

      yield return new DataStep(currentArray); // Отсортированный массив
    }

    // Вспомогательный рекурсивный метод, возвращающий последовательность шагов
    static private IEnumerable<SortStep> QuickSortInternal(int[] array, int first, int last, bool ascending, RangeNode parentNode)
    {
      if (first < last)
      {
        int pivotIndex = Partition(array, first, last, ascending);

        // Добавляем дочерние узлы
        var startChild = new RangeNode(first, pivotIndex - 1);
        var endChild = new RangeNode(pivotIndex + 1, last);
        parentNode.Children.Add(startChild);
        parentNode.Children.Add(endChild);

        // Рекурсивно сортируем левую часть
        foreach (var step in QuickSortInternal(array, first, pivotIndex - 1, ascending, startChild))
          yield return step;

        // Рекурсивно сортируем правую часть
        foreach (var step in QuickSortInternal(array, pivotIndex + 1, last, ascending, endChild))
          yield return step;
      }
    }

    static private int Partition(int[] array, int first, int last, bool ascending = true) // Вспомогательный метод для быстрой сортировки, выбор опорного элемента
    {
      int pivot = array[last]; // Последний элемент (опорный)
      int i = first - 1; // Индекс меньшего элемента

      for (int j = first; j < last; j++) // Проходим по всем элементам
      {
        bool shouldMovestart = ascending
          ? (array[j] <= pivot)
          : (array[j] >= pivot);

        if (shouldMovestart) // Если текущий элемент меньше или равен опорному
        {
          i++;
          (array[i], array[j]) = (array[j], array[i]); // Меняем элементы местами
        }
      }
      (array[i + 1], array[last]) = (array[last], array[i + 1]); // Меняем элементы местами
      return i + 1; // Возвращаем индекс опорного элемента
    }

    static public IEnumerable<SortStep> MergeSort(int[] array, bool ascending = true)
    {
      int[] currentArray = (int[])array.Clone();
      yield return new DataStep(currentArray); // Исходный

      var root = new RangeNode(0, array.Length - 1);
      foreach (var step in MergeSortInternal(currentArray, 0, array.Length - 1, ascending, root))
        yield return step;

      // Выводим структуру слияния
      yield return new InfoStep(root.ToString());

      yield return new DataStep(currentArray); // Результат
    }

    static private IEnumerable<SortStep> MergeSortInternal(int[] array, int first, int last, bool ascending, RangeNode parentNode)
    {
      if (first < last)
      {
        int mid = first + (last - first) / 2;

        var startChild = new RangeNode(first, mid);
        var endChild = new RangeNode(mid + 1, last);
        parentNode.Children.Add(startChild);
        parentNode.Children.Add(endChild);

        // Рекурсивно сортируем левую часть
        foreach (var step in MergeSortInternal(array, first, mid, ascending, startChild))
          yield return step;

        // Рекурсивно сортируем правую часть
        foreach (var step in MergeSortInternal(array, mid + 1, last, ascending, endChild))
          yield return step;
      }
    }

    static private IEnumerable<SortStep> Merge(int[] array, int first, int mid, int last, bool ascending = true)
    {
      int n1 = mid - first + 1;
      int n2 = last - mid;
      int[] start = new int[n1];
      int[] end = new int[n2];

      for (int i = 0; i < n1; i++)
        start[i] = array[first + i];
      for (int j = 0; j < n2; j++)
        end[j] = array[mid + 1 + j];

      int k = first;
      int iIndex = 0, jIndex = 0;

      while (iIndex < n1 && jIndex < n2)
      {
        bool takeFromstart = ascending
            ? start[iIndex] <= end[jIndex]
            : start[iIndex] >= end[jIndex];

        if (takeFromstart)
        {
          array[k] = start[iIndex];
          iIndex++;
        }
        else
        {
          array[k] = end[jIndex];
          jIndex++;
        }
        k++;
        yield return new DataStep((int[])array.Clone());
      }

      while (iIndex < n1)
      {
        array[k] = start[iIndex];
        iIndex++;
        k++;
        yield return new DataStep((int[])array.Clone());
      }

      while (jIndex < n2)
      {
        array[k] = end[jIndex];
        jIndex++;
        k++;
        yield return new DataStep((int[])array.Clone());
      }
    }

    static public IEnumerable<SortStep> HeapSort(int[] array, bool ascending = true) // Пирамидальная сортировка, генератор
    {
      int[] currentArray = (int[])array.Clone();
      yield return new DataStep(currentArray); // Исходный массив

      int n = currentArray.Length;

      yield return new InfoStep("Построение кучи:");
      for (int i = n / 2 - 1; i >= 0; i--)
      {
        foreach (var step in Heapify(currentArray, n, i, ascending))
          yield return step;
      }
      yield return new InfoStep("Построение кучи завершено.");

      for (int i = n - 1; i > 0; i--)
      {
        (currentArray[0], currentArray[i]) = (currentArray[i], currentArray[0]);
        yield return new DataStep((int[])currentArray.Clone());

        foreach (var step in Heapify(currentArray, i, 0, ascending))
          yield return step;
      }

      yield return new DataStep(currentArray); // Финальный результат
    }

    static private IEnumerable<SortStep> Heapify(int[] array, int n, int i, bool ascending = true) // Вспомогательный метод для пирамидальной сортировки
    {
      int target = i;
      int start = 2 * i + 1;
      int end = 2 * i + 2;

      if (ascending)
      {
        // Max-heap: ищем наибольший
        if (start < n && array[start] > array[target])
          target = start;

        if (end < n && array[end] > array[target])
          target = end;
      }
      else
      {
        // Min-heap: ищем наименьший
        if (start < n && array[start] < array[target])
          target = start;

        if (end < n && array[end] < array[target])
          target = end;
      }

      if (target != i)
      {
        (array[i], array[target]) = (array[target], array[i]);
        yield return new DataStep((int[])array.Clone());

        foreach (var step in Heapify(array, n, target, ascending))
          yield return step;
      }
    }

    static public (bool, bool) SortedCheck(int[] array)
    {
      bool asc = true;
      bool desc = true;
      for (int i = 1; i < array.Length; i++)
      {
        if (array[i] < array[i - 1])
          asc = false; // Если хотя бы один раз текущее значение меньше предыдущего, массив не может быть отсортирован по возрастанию
        if (array[i] > array[i - 1])
          desc = false; // Если хотя бы один раз текущее значение больше предыдущего, массив не может быть отсортирован по убыванию

        if (!asc && !desc)
          break; // Если оба флага ложны, можно прервать цикл раньше времени
      }
      return (asc, desc); // Кортеж из двух булевых значений
    }

    static public int BinarySearch(int[] array, int target) // Двоичный поиск, возможен только на отсортированном массиве по возрастанию
    {
      if (array.Length <= 0)
        throw new ArgumentException("Массив пустой.");
      if (!SortedCheck(array).Item1)
        throw new ArgumentException("Массив не отсортирован по возрастанию.");

      int start = 0;
      int end = array.Length - 1;

      while (start <= end)
      {
        int mid = start + (end - start) / 2;

        if (array[mid] == target)
          return mid; // Элемент найден, возвращаем его индекс, в методе ввода/вывода прибавим 1
        if (array[mid] < target)
          start = mid + 1;
        else
          end = mid - 1;
      }

      throw new ArgumentException("Элемент не найден в массиве.");
    }

    static public int[] Shake(int[] array)
    {
      if (array.Length <= 1)
        return (int[])array.Clone();

      int[] result = (int[])array.Clone();
      Random rand = new();

      for (int i = result.Length - 1; i > 0; i--)
      {
        int j = rand.Next(0, i + 1);
        (result[i], result[j]) = (result[j], result[i]);
      }

      return result;
    }

    static public (int, int[]) QuickSelect(int[] array, int k)
    {
      if (array == null || array.Length == 0)
        throw new ArgumentException("Массив пуст.");
      if (k < 1 || k > array.Length)
        throw new ArgumentOutOfRangeException(nameof(k), $"k должно быть от 1 до {array.Length}.");

      int[] workingArray = (int[])array.Clone();
      return QuickSelectInternal(workingArray, 0, workingArray.Length - 1, k - 1); // переводим k в 0-индексацию
    }

    static private (int, int[]) QuickSelectInternal(int[] array, int start, int end, int k)
    {
      if (start == end)
        return (array[start], array);

      int pivotIndex = Partition(array, start, end);

      if (k == pivotIndex)
        return (array[k], array);
      else if (k < pivotIndex)
        return QuickSelectInternal(array, start, pivotIndex - 1, k);
      else
        return QuickSelectInternal(array, pivotIndex + 1, end, k);
    }
  };

  // Класс для ввода и вывода с состоянием, здесь хранится текущий массив, состояние его отсортированности и т.д.
  public class MenuWithState
  {
    private int[] currentArray = new int[0]; // Если массив не заполнен, его длина равна 0

    static public int[] PrintArrayStepByStep(IEnumerable<SortStep> sortingSteps) // Метод для поэтапного вывода сортировки
    {
      int[] finalArray = null;
      foreach (var step in sortingSteps)
      {
        switch (step)
        {
          case DataStep ds:
            ArrayOperations.PrintArray(ds.Data);
            finalArray = ds.Data;
            break;

          case RangeStep rs:
            Console.WriteLine($" [{rs}]");
            break;

          case InfoStep info:
            Console.WriteLine(info.Message);
            break;

          default:
            throw new NotSupportedException("Неизвестный тип шага сортировки.");
        }
      }
      if (finalArray == null)
        throw new InvalidOperationException("Сортировка не вернула финальный массив.");
      return finalArray; // Возвращаем отсортированный массив
    }

    public void PrintCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      Console.WriteLine("Текущий массив:");
      ArrayOperations.PrintArray(currentArray);
    }

    static public bool IsAscendingInput()
    {
      return CommonUtilities.GetAndValidateNumberAsBool("Введите направление сортировки (1 - возрастание, 0 - убывание)");
    }


    public void FillArray()
    {
      int length = CommonUtilities.GetAndValidateNumber("Введите длину массива [1 ... 50]", 1, 50); // Ввод
      int min = CommonUtilities.GetAndValidateNumber("Введите минимальное значение элемента массива [-100 ... 100]", -100, 100);
      int max = CommonUtilities.GetAndValidateNumber($"Введите максимальное значение элемента массива [{min} ... 100]", min, 100);


      currentArray = ArrayOperations.GenerateArray(length, min, max); // Действие с массивом

      Console.WriteLine("Массив заполнен случайными числами.");
      PrintCurrentArray(); // Вывод
    }

    public void BubbleSortCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      Console.WriteLine("ПУЗЫРЬКОВАЯ СОРТИРОВКА");
      bool ascending = IsAscendingInput();
      currentArray = PrintArrayStepByStep(ArrayOperations.BubbleSort(currentArray, ascending));
      PrintCurrentArray();
    }
    public void SelectionSortCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      Console.WriteLine("СОРТИРОВКА ВЫБОРОМ");
      bool ascending = IsAscendingInput();
      currentArray = PrintArrayStepByStep(ArrayOperations.SelectionSort(currentArray, ascending));
      PrintCurrentArray();
    }


    public void InsertionSortCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      Console.WriteLine("СОРТИРОВКА ВСТАВКАМИ");
      bool ascending = IsAscendingInput();
      currentArray = PrintArrayStepByStep(ArrayOperations.InsertionSort(currentArray, ascending));
      PrintCurrentArray();
    }

    public void CountingSortCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      Console.WriteLine("СОРТИРОВКА ПОДСЧЁТАМИ");
      bool ascending = IsAscendingInput();
      currentArray = PrintArrayStepByStep(ArrayOperations.CountingSort(currentArray, ascending));
      PrintCurrentArray();
    }

    public void QuickSortCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      Console.WriteLine("БЫСТРАЯ СОРТИРОВКА");
      bool ascending = IsAscendingInput();
      currentArray = PrintArrayStepByStep(ArrayOperations.QuickSort(currentArray, ascending));
      PrintCurrentArray();
    }

    public void QuickSelectCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      Console.WriteLine("ПОИСК ПОРЯДКОВОЙ СТАТИСТИКИ");
      PrintCurrentArray();
      int k = CommonUtilities.GetAndValidateNumber($"Введите номер порядковой статистики [1 ... {currentArray.Length}]", 1, currentArray.Length);
      var res = ArrayOperations.QuickSelect(currentArray, k);
      Console.WriteLine($"{k}-я порядковая статистика: {res.Item1}");
      currentArray = res.Item2;
      PrintCurrentArray();
    }

    public void ShakeCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      Console.WriteLine("ПЕРЕМЕШИВАНИЕ МАССИВА");
      currentArray = ArrayOperations.Shake(currentArray);
      Console.WriteLine("Массив перемешан.");
      PrintCurrentArray();
    }

    public void MergeSortCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      Console.WriteLine("СОРТИРОВКА СЛИЯНИЕМ");
      bool ascending = IsAscendingInput();
      currentArray = PrintArrayStepByStep(ArrayOperations.MergeSort(currentArray, ascending));
      PrintCurrentArray();
    }

    public void HeapSortCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      Console.WriteLine("ПИРАМИДАЛЬНАЯ СОРТИРОВКА");
      bool ascending = IsAscendingInput();
      currentArray = PrintArrayStepByStep(ArrayOperations.HeapSort(currentArray, ascending));
      PrintCurrentArray();
    }

    public void BinarySearchCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      if (!ArrayOperations.SortedCheck(currentArray).Item1)
      {
        Console.WriteLine("Массив не отсортирован.");
        return;
      }
      int target = CommonUtilities.GetAndValidateNumber("Введите искомое значение");
      int index = ArrayOperations.BinarySearch(currentArray, target);
      if (index >= 0)
        Console.WriteLine($"Элемент найден на позиции {index + 1}.");

    }


    public void Main()
    {
      string input;
      do
      {
        Console.WriteLine("ОПЕРАЦИИ:");
        Console.WriteLine("1 - Заполнение массива.");
        Console.WriteLine("2 - Пузырьковая сортировка.");
        Console.WriteLine("3 - Сортировка выбором.");
        Console.WriteLine("4 - Сортировка вставками.");
        Console.WriteLine("5 - Сортировка подсчётами.");
        Console.WriteLine("6 - Быстрая сортировка.");
        Console.WriteLine("7 - Сортировка слиянием.");
        Console.WriteLine("8 - Пирамидальная сортировка.");
        Console.WriteLine("9 - Двоичный поиск массива.");
        Console.WriteLine("10 - Поиск порядковой статистики.");
        Console.WriteLine("11 - Перемешивание массива.");
        Console.WriteLine("12 - Выход.");
        Console.Write("Введите номер операции (1 ... 12): ");
        input = Console.ReadLine() ?? "";

        try
        {
          switch (input)
          {
            case "1":
              FillArray();
              break;
            case "2":
              BubbleSortCurrentArray();
              break;
            case "3":
              SelectionSortCurrentArray();
              break;
            case "4":
              InsertionSortCurrentArray();
              break;
            case "5":
              CountingSortCurrentArray();
              break;
            case "6":
              QuickSortCurrentArray();
              break;
            case "7":
              MergeSortCurrentArray();
              break;
            case "8":
              HeapSortCurrentArray();
              break;
            case "9":
              BinarySearchCurrentArray();
              break;
            case "10":
              QuickSelectCurrentArray();
              break;
            case "11":
              ShakeCurrentArray();
              break;
            case "12":
              Console.WriteLine("Выход из программы.");
              break;
            default:
              Console.WriteLine("Ошибка: некорректный номер операции.");
              break;
          }
          ;
        }
        catch (Exception end)
        {
          Console.WriteLine(end.Message);
          continue;
        }
      } while (input != "12");
    }
  }

  internal class Program
  {
    static void Main(string[] args)
    {
      MenuWithState menu = new MenuWithState();
      menu.Main();
    }
  }
}