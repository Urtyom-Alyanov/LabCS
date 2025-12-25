using System.Net;

namespace Lab04
{
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
      catch (Exception e)
      {
        switch (e)
        {
          case FormatException:
            throw new FormatException("Ошибка: введено не число.");
          case OverflowException:
            throw new OverflowException($"Ошибка: число слишком большое или слишком маленькое, вне диапозона [{min} ... {max}].");
          default:
            throw e;
        }
      }
    }

    static public bool GetAndValidateNumberAsBool(string? prompt)
    {
      try
      {
        if (prompt != null)
          Console.Write(prompt);
        int numb = int.Parse(Console.ReadLine() ?? ""); // Может выбросить FormatException или OverflowException, ?? "" для того, чтобы не было null (мозолит глаза)
        if (numb != 0 && numb != 1)
          throw new ArgumentOutOfRangeException(null, null, "Ошибка: введено не 0 и не 1.");
        return numb == 1; // Возвращаем валидный коэффициент
      }
      catch (Exception e)
      {
        switch (e)
        {
          case FormatException:
            throw new FormatException("Ошибка: введено не число.");
          case OverflowException:
            throw new OverflowException("Ошибка: число слишком большое или слишком маленькое.");
          default:
            throw e;
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

    static public IEnumerable<int[]> BubbleSort(int[] array, bool ascending = true) // Пузырьковая сортировка, генератор
    {
      int[] currentArray = (int[])array.Clone();
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
            yield return (int[])currentArray.Clone();
          }
        }
      }
    }

    static public IEnumerable<int[]> SelectionSort(int[] array, bool ascending = true) // Сортировка выбором, генератор
    {
      int[] currentArray = (int[])array.Clone();
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
        yield return (int[])currentArray.Clone();
      }
    }

    static public IEnumerable<int[]> InsertionSort(int[] array, bool ascending = true) // Сортировка вставками, генератор
    {
      int[] currentArray = (int[])array.Clone();
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
          yield return (int[])currentArray.Clone();
        }
        currentArray[j + 1] = key;
        yield return (int[])currentArray.Clone();
      }
    }

    static public IEnumerable<int[]> CountingSort(int[] array, bool ascending = true) // Сортировка подсчётом, генератор
    {
      int[] currentArray = (int[])array.Clone();
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
            yield return (int[])currentArray.Clone();
          }
        }
      else
        for (int i = range - 1; i >= 0; i--) // Проходим по массиву подсчёта в обратном порядке
        {
          while (count[i] > 0) // Пока есть вхождения элемента
          {
            currentArray[index++] = i + min; // Восстанавливаем значение элемента
            count[i]--;
            yield return (int[])currentArray.Clone();
          }
        }
    }

    static public IEnumerable<int[]> QuickSort(int[] array, int first, int last, bool ascending = true) // Быстрая сортировка, генератор
    {
      int[] currentArray = (int[])array.Clone();
      if (first < last)
      {
        int pivotIndex = Partition(currentArray, first, last);
        yield return currentArray;
        foreach (var step in QuickSort(currentArray, first, pivotIndex - 1))
          yield return (int[])step.Clone();
        foreach (var step in QuickSort(currentArray, pivotIndex + 1, last))
          yield return (int[])step.Clone();
      }
    }

    static private int Partition(int[] array, int first, int last) // Вспомогательный метод для быстрой сортировки, выбор опорного элемента
    {
      int pivot = array[last]; // Последний элемент (опорный)
      int i = first - 1; // Индекс меньшего элемента

      for (int j = first; j < last; j++) // Проходим по всем элементам
      {
        if (array[j] <= pivot) // Если текущий элемент меньше или равен опорному
        {
          i++;
          (array[i], array[j]) = (array[j], array[i]); // Меняем элементы местами
        }
      }
      (array[i + 1], array[last]) = (array[last], array[i + 1]); // Меняем элементы местами
      return i + 1; // Возвращаем индекс опорного элемента
    }

    static public IEnumerable<int[]> MergeSort(int[] array, int first, int last, bool ascending = true) // Сортировка слиянием, генератор
    {
      int[] currentArray = (int[])array.Clone();
      if (first < last)
      {
        int mid = first + (last - first) / 2;

        foreach (var step in MergeSort(currentArray, first, mid))
          yield return (int[])step.Clone();
        foreach (var step in MergeSort(currentArray, mid + 1, last))
          yield return (int[])step.Clone();

        foreach (var step in Merge(currentArray, first, mid, last))
          yield return (int[])step.Clone();
      }
    }

    static private IEnumerable<int[]> Merge(int[] array, int first, int mid, int last) // Вспомогательный метод для сортировки слиянием
    {
      int n1 = mid - first + 1;
      int n2 = last - mid;
      int[] L = new int[n1];
      int[] R = new int[n2];

      for (int i = 0; i < n1; i++)
        L[i] = array[first + i];
      for (int j = 0; j < n2; j++)
        R[j] = array[mid + 1 + j];

      int k = first;
      int iIndex = 0, jIndex = 0;

      while (iIndex < n1 && jIndex < n2)
      {
        if (L[iIndex] <= R[jIndex])
        {
          array[k] = L[iIndex];
          iIndex++;
        }
        else
        {
          array[k] = R[jIndex];
          jIndex++;
        }
        k++;
        yield return array;
      }

      while (iIndex < n1)
      {
        array[k] = L[iIndex];
        iIndex++;
        k++;
        yield return array;
      }

      while (jIndex < n2)
      {
        array[k] = R[jIndex];
        jIndex++;
        k++;
        yield return array;
      }
    }

    static public IEnumerable<int[]> HeapSort(int[] array) // Пирамидальная сортировка, генератор
    {
      int[] currentArray = (int[])array.Clone();
      int n = currentArray.Length;

      for (int i = n / 2 - 1; i >= 0; i--)
      {
        foreach (var step in Heapify(currentArray, n, i))
          yield return step;
      }

      for (int i = n - 1; i > 0; i--)
      {
        (currentArray[0], currentArray[i]) = (currentArray[i], currentArray[0]);
        yield return currentArray;

        foreach (var step in Heapify(currentArray, i, 0))
          yield return step;
      }
    }

    static private IEnumerable<int[]> Heapify(int[] array, int n, int i) // Вспомогательный метод для пирамидальной сортировки
    {
      int largest = i;
      int left = 2 * i + 1;
      int right = 2 * i + 2;

      if (left < n && array[left] > array[largest])
        largest = left;

      if (right < n && array[right] > array[largest])
        largest = right;

      if (largest != i)
      {
        (array[i], array[largest]) = (array[largest], array[i]);
        yield return array;

        foreach (var step in Heapify(array, n, largest))
          yield return step;
      }
    }

    static public void PrintArray(int[] array) // Метод для печати массива
    {
      Console.WriteLine(string.Join(" ", array));
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

      int left = 0;
      int right = array.Length - 1;

      while (left <= right)
      {
        int mid = left + (right - left) / 2;

        if (array[mid] == target)
          return mid; // Элемент найден, возвращаем его индекс, в методе ввода/вывода прибавим 1
        if (array[mid] < target)
          left = mid + 1;
        else
          right = mid - 1;
      }

      throw new ArgumentException("Элемент не найден в массиве.");
    }
  };

  // Класс для ввода и вывода с состоянием, здесь хранится текущий массив, состояние его отсортированности и т.д.
  public class MenuWithState
  {
    private int[] currentArray = new int[0]; // Если массив не заполнен, его длина равна 0


    static public int[] PrintArrayStepByStep(IEnumerable<int[]> sortingSteps) // Метод для поэтапного вывода сортировки
    {
      foreach (var step in sortingSteps)
      {
        ArrayOperations.PrintArray(step);
      }
      return sortingSteps.Last(); // Возвращаем отсортированный массив
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
      return CommonUtilities.GetAndValidateNumberAsBool("Введите направление сортировки (1 - возрастание, 0 - убывание): ");
    }


    public void FillArray()
    {
      int length = CommonUtilities.GetAndValidateNumber("Введите длину массива [1 ... 50]", 1, 50); // Ввод
      int min = CommonUtilities.GetAndValidateNumber("Введите минимальное значение элемента массива [-100 ... 100]", -100, 100);
      int max = CommonUtilities.GetAndValidateNumber($"Введите максимальное значение элемента массива [{min} ... 100]", min, 100);


      currentArray = ArrayOperations.GenerateArray(length, min, max); // Действие с массивом

      Console.WriteLine("Массив заполнен случайными числами:");
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
      currentArray = PrintArrayStepByStep(ArrayOperations.QuickSort(currentArray, 0, currentArray.Length - 1, ascending));
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
      currentArray = PrintArrayStepByStep(ArrayOperations.MergeSort(currentArray, 0, currentArray.Length - 1, ascending));
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
      currentArray = PrintArrayStepByStep(ArrayOperations.HeapSort(currentArray));
      PrintCurrentArray();
    }

    public void BinarySearchCurrentArray()
    {
      if (currentArray.Length == 0)
      {
        Console.WriteLine("Массив не заполнен.");
        return;
      }
      int target = CommonUtilities.GetAndValidateNumber("Введите искомое значение: ");
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
        Console.WriteLine("10 - Выход.");
        Console.Write("Введите номер операции (1 ... 10): ");
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
              Console.WriteLine("Выход из программы.");
              break;
            default:
              Console.WriteLine("Ошибка: некорректный номер операции.");
              break;
          }
          ;
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
          continue;
        }
      } while (input != "10");
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