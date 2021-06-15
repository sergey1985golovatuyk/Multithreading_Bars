using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace TestTask_MultiThreading
{
    class Program
    {
        // Импортирование WIn32 API для разворачивания окна консоли на весь экран при запуске программы
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static readonly IntPtr ThisConsole = GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int MAXIMIZE = 3;


        static object locker = new object(); // Создание объекта locker для синхронизации потоков при работе курсора консоли в потоке
        public static int threadsNumber = 10; // Задание количества потоков
        public static Queue<Thread> queue = new Queue<Thread>(); // Создание очереди потоков

        static void Main(string[] args)
        {

            ShowWindow(ThisConsole, MAXIMIZE);

            for (int i = 1; i <= threadsNumber; i++)    // Создаем потоки
            {
                //ThreadClass tClass = new ThreadClass(); // Создание объекта класса ThreadClass
                Thread t = new Thread(new ThreadStart(Delay)); // Создание нового потока, в которым выполняется метод Delay объекта класса ThreadClass 
                t.Name = i.ToString(); // Присваивание имени потоку
                t.Start(); // Старт выполнения потока
                queue.Enqueue(t); // Добавление потока в очередь 
            }

            while (queue.Any(x => x.IsAlive)) // Блокировка основного потока, если еще выполняются вызываемые потоки
            {
                Thread.CurrentThread.Join(2500);
            }

            Console.CursorTop = 20; // Задание высоты позиции курсора в окне консоли
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n Job Done! Press any key...");
            Console.ReadKey(true); // Ожидание нажатия клавиши для выхода из программы

        }

        public static void Delay() // Метод Delay, в котором производятся вычисления и выполняется визуализация
        {
            int calcNumbers = 50; // Количество итераций вычислений
            int cursorPositionLeft = 0; // Установка позиции курсора по горизонтали
            int cursorPositionTop = 1 + (Int16.Parse(Thread.CurrentThread.Name) - 1) * 2; // Установка позиции курсора по вертикали для визуализации процесса потока
            Console.CursorVisible = false; // Скрытие курсора консоли


            ProgressBar bar = new ProgressBar(); // Создание объекта Progress Bar для визуализации процесса работы потока

            Stopwatch sw = new Stopwatch(); // Контроль времени работы потока
            sw.Start();

            lock (locker) // Блок кода блокируется внутри блока lock, курсор захватывается текущим потоком
            {
                bar.DrawHeaderOfBar(ref cursorPositionLeft, cursorPositionTop); // Вывод на консоль номер потока и ManagedThreadId
            }

            for (int i = 0; i <= calcNumbers; i++) // Выполнение заданного количества вычислений
            {
                var random = new Random();
                int timeToDelay = random.Next(100, 500); // Время вычисления в цикле
                Thread.Sleep(timeToDelay); // Приостановка потока, эмуляция процесса вычесления

                lock (locker) // Блок кода блокируется внутри блока lock, курсор захватывается текущим потоком
                {
                    bar.DrawIterationsBar(i, calcNumbers, ref cursorPositionLeft, cursorPositionTop); // Вывод на консоль прогресс бара
                }
            }

            lock (locker) // Блок кода блокируется внутри блока lock, курсор захватывается текущим потоком
            {
                Console.Write(" Thread finished!  ");
                sw.Stop();
                TimeSpan timeSpan = sw.Elapsed;
                Console.Write("(Work time: " + String.Format("{0:00sec}:{1:000ms}) ", timeSpan.Seconds, timeSpan.Milliseconds)); // Вывод на консоль времени работы потока
                Console.Write("Index of thread in Queue: " + queue.ToArray().IndexOf(Thread.CurrentThread).ToString()); // Вывод на консоль номер потока в очереди
            }
        }
    }
}

public class ProgressBar
{
    // Метод для отрисовки имени и ManagedThreadId потока
    public void DrawHeaderOfBar(ref int cursorPositionLeft, int cursorPositionTop)
    {
        Console.CursorLeft = cursorPositionLeft;
        Console.CursorTop = cursorPositionTop;
        Console.Write((Thread.CurrentThread.Name) + " " + "(" + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
        cursorPositionLeft = 8;
    }
    // Метод для отрисовки выполненой итерации вычисления в прогресс баре
    public void DrawIterationsBar(int progress, int total, ref int cursorPositionLeft, int cursorPositionTop)
    {
        Console.CursorLeft = cursorPositionLeft;
        Console.CursorTop = cursorPositionTop;

        // Отрисовка одной единицы прогресс бара
        var random = new Random();
        int caseException = random.Next(0, total); // Случайное значение для возникновения исключения

        if (caseException != progress) // Вычисление выполнено без ошибок
        {    
            Console.BackgroundColor = ConsoleColor.Green; // Синий элемент прогресс бара, нет ошибок
            Console.Write(" ");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" ");
        }
        else // в процессе выполнения вычисление возникло исключение 
        {
            try // Обработка исключения
            {
                throw new Exception("Error of calculation exception");
            }
            catch (Exception) { };

            Console.BackgroundColor = ConsoleColor.Red; // Красный элемент прогресс бара, ошибка
            Console.Write(" ");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" ");
        }
        Console.Write(" " + progress.ToString()); // Вывод на консоль номера вычисления (итерации)
        cursorPositionLeft = Console.CursorLeft - (progress.ToString().Length + 1);
    }

}


static class ArrayExtensions // Использую класс ArrayExtensions для преобразования очереди в массив, чтобы получить
                            // значение по индексу.
{
    public static int IndexOf<T>(this T[] array, T value)
    {
        return Array.IndexOf(array, value);
    }
}

