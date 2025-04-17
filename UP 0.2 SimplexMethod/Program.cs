using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UP_0._2_SimplexMethod
{
    internal class Program
    {
        public static string Solve(double[,] A, double[] b, double[] c)
        {
            StringBuilder result = new StringBuilder();
            int m = A.GetLength(0); // количество ограничений
            int n = A.GetLength(1); // количество переменных

            // Шаг 1: Создаем симплексную таблицу.
            double[,] table = new double[m + 1, n + m + 1];

            // Заполняем симплексную таблицу.
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                    table[i, j] = A[i, j];

                table[i, n + i] = 1; // единичная матрица для базисных переменных
                table[i, n + m] = b[i];
            }

            // Заполняем строку целевой функции.
            for (int j = 0; j < n; j++)
                table[m, j] = -c[j]; // коэффициенты функции цели (с минусом для максимизации)

            // Шаг 2: Проводим симплексные итерации.
            while (true)
            {
                // Поиск колонки для ввода переменной.
                int col = -1;
                for (int j = 0; j < n + m; j++)
                {
                    if (table[m, j] < 0)
                    {
                        col = j;
                        break;
                    }
                }

                // Если нет таких колонок, то решение найдено.
                if (col == -1)
                    break;

                // Поиск строки для удаления переменной.
                int row = -1;
                double minRatio = double.MaxValue;

                for (int i = 0; i < m; i++)
                {
                    if (table[i, col] > 0)
                    {
                        double ratio = table[i, n + m] / table[i, col];
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            row = i;
                        }
                    }
                }

                // Если строки не найдено, то задача не ограничена.
                if (row == -1)
                {
                    result.AppendLine("Задача не ограничена.");
                    return result.ToString();
                }

                // Проводим элементарные преобразования.
                double pivot = table[row, col];
                for (int j = 0; j < n + m + 1; j++)
                    table[row, j] /= pivot;

                for (int i = 0; i <= m; i++)
                {
                    if (i != row)
                    {
                        double factor = table[i, col];
                        for (int j = 0; j < n + m + 1; j++)
                            table[i, j] -= factor * table[row, j];
                    }
                }
            }

            result.AppendLine("Оптимальное решение:");
            for (int i = 0; i < n; i++)
            {
                bool isBasic = false;
                for (int j = 0; j < m; j++)
                {
                    if (table[j, i] == 1)
                    {
                        isBasic = true;
                        result.AppendLine($"x{i + 1} = {table[j, n + m]}");
                        break;
                    }
                }
                if (!isBasic)
                {
                    result.AppendLine($"x{i + 1} = 0");
                }
            }

            result.AppendLine($"Оптимальное значение Z = {table[m, n + m]}");
            return result.ToString();
        }

        static void Main()
        {
            Console.WriteLine("Симплекс-метод для решения задач ЛП");
            Console.WriteLine("====================================");

            while (true)
            {
                try
                {
                    ProcessCalculationCycle();

                    Console.Write("\nХотите решить еще одну задачу? (y/n): ");
                    if (Console.ReadLine().ToLower() != "y")
                    {
                        Console.WriteLine("Работа программы завершена.");
                        break;
                    }

                    Console.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nОшибка: {ex.Message}");
                    Console.WriteLine("Попробуйте ввести данные еще раз.");
                    Console.ReadLine();
                }
            }
        }

        static void ProcessCalculationCycle()
        {
            Console.WriteLine("\n*** Новая задача ***");

            Console.WriteLine("\nВыберите способ ввода данных:");
            Console.WriteLine("1 - Ввод через консоль");
            Console.WriteLine("2 - Загрузка из файла");
            Console.Write("Ваш выбор: ");

            var choice = Console.ReadLine();
            string solution;

            if (choice == "1")
            {
                solution = ConsoleInput();
            }
            else if (choice == "2")
            {
                solution = FileInput();
            }
            else
            {
                throw new ArgumentException("Неверный выбор способа ввода");
            }

            Console.WriteLine("\nРезультат:");
            Console.WriteLine(solution);

            Console.Write("\nСохранить результат в файл? (y/n): ");
            if (Console.ReadLine().ToLower() == "y")
            {
                Console.Write("Введите путь для сохранения: ");
                var path = Console.ReadLine();
                File.WriteAllText(path, solution);
                Console.WriteLine("Файл успешно сохранен!");
            }
        }


        static string ConsoleInput()
        {
            Console.Write("Введите количество переменных: ");
            int n = int.Parse(Console.ReadLine());
            Console.Write("Введите количество ограничений: ");
            int m = int.Parse(Console.ReadLine());

            double[,] A = new double[m, n];
            Console.WriteLine("\nВведите коэффициенты матрицы ограничений (A):");
            for (int i = 0; i < m; i++)
            {
                Console.Write($"Строка {i + 1} (через пробел): ");
                var input = GetValidatedInput(n);
                for (int j = 0; j < n; j++)
                    A[i, j] = double.Parse(input[j]);
            }

            Console.Write("\nВведите правые части ограничений (b) через пробел: ");
            var bInput = GetValidatedInput(m);
            double[] b = bInput.Select(double.Parse).ToArray();

            Console.Write("\nВведите коэффициенты целевой функции (c) через пробел: ");
            var cInput = GetValidatedInput(n);
            double[] c = cInput.Select(double.Parse).ToArray();

            return Solve(A, b, c);
        }

        static string FileInput()
        {
            try
            {
                Console.Write("\nВведите путь к файлу: ");
                string path = Console.ReadLine();
                string[] lines = File.ReadAllLines(path);

                // Парсим первую строку (n и m)
                var firstLine = ParseLine(lines[0], 2);
                int n = int.Parse(firstLine[0]);
                int m = int.Parse(firstLine[1]);

                // Парсим матрицу A
                double[,] A = new double[m, n];
                for (int i = 0; i < m; i++)
                {
                    var row = ParseLine(lines[i + 1], n);
                    for (int j = 0; j < n; j++)
                        A[i, j] = double.Parse(row[j]);
                }

                // Парсим вектор b
                var bLine = ParseLine(lines[m + 1], m);
                double[] b = bLine.Select(double.Parse).ToArray();

                // Парсим вектор c
                var cLine = ParseLine(lines[m + 2], n);
                double[] c = cLine.Select(double.Parse).ToArray();

                Console.WriteLine("\nДанные успешно загружены из файла!");
                return Solve(A, b, c);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        static string[] ParseLine(string line, int expectedCount)
        {
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != expectedCount)
                throw new ArgumentException($"Ожидается {expectedCount} значений, получено {parts.Length}");
            return parts;
        }

        static string[] GetValidatedInput(int requiredCount)
        {
            while (true)
            {
                var input = Console.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (input.Length == requiredCount) return input;
                Console.Write($"Нужно {requiredCount} значений. Повторите ввод: ");
            }
        }
    }
}