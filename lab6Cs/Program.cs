using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace lab6Cs
{
    class Program
    {
        public delegate void Delelka();

        static void Main(string[] args)
        {
            Console.WriteLine("Введите размерность матрицы:");
            Console.Write("Столбцы: ");
            int cols = Int32.Parse(Console.ReadLine());
            Console.Write("Ряды: ");
            int rows = Int32.Parse(Console.ReadLine());

            MSystem ms = new MSystem(cols, rows);

            Console.WriteLine("\nСгенерированная матрица:");
            ms.ShowMatr();

            Console.WriteLine("\nПоследовательный просчет:");
            ms.PosledShowBiggestValue();
            Console.WriteLine("--------------------------------");

            Console.WriteLine("\nБольшее число каждой строки:");
            Delelka dl = new Delelka(ms.ShowBiggestValues);
            dl.Invoke();
        }
    }

    class MSystem
    {
        private int cols, rows;
        private int[,] matr;
        private static Semaphore _pool;
        private bool[] check;
        private double[] time_mas;
        private bool[] time_check;

        public MSystem(int _cols, int _rows)
        {
            cols = _cols;
            rows = _rows;
            matr = new int[rows, cols];
            check = new bool[rows];
            time_mas = new double[4];
            time_check = new bool[4];
            _pool = new Semaphore(0, 3);
            FillMatr();
        }

        public void ShowMatr()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                    Console.Write("{0}\t", matr[i, j]);
                Console.WriteLine();
            }
        }

        private void FillMatr()
        {
            Random rd = new Random();

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    matr[i, j] = rd.Next(0, 55);

            Console.WriteLine("Автозаполнение выполнено.");
            Thread.Sleep(500);
        }

        public void ShowBiggestValues()
        {
            for (int i = 0; i < 3; i++)
            {
                Thread tr = new Thread(new ParameterizedThreadStart(Count));
                tr.Start(i + 1);
            }
            _pool.Release(3);
        }

        private void Count(object number)
        {
            // счетчик времени работы
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //int result = 0;
            int index = 0;
            int max = -1;

            _pool.WaitOne();
            while (index != -1)
            {
                // получения индекса ещё не проверенной строки
                for (int i = 0; i < rows; i++)
                {
                    if (!check[i])
                    {
                        index = i;
                        max = matr[i, 0];
                        check[i] = true;
                        break;
                    }
                    else
                        index = -1;
                }

                if (index != -1)
                {
                    // работа со строкой (если есть)
                    for (int j = 0; j < cols; j++)
                        max = Math.Max(matr[index, j], max);

                    // вывод результата
                    Console.WriteLine("Поток {0}: больший элемент в строке {1} - {2}", number, index + 1, max);

                    // задержка
                    Thread.Sleep(1000);
                }
            }

            // Подсчет времени работы потока
            sw.Stop();
            Console.WriteLine("Поток {0} закончил работу. Время работы: {1}мс.", number, sw.Elapsed.TotalSeconds);
            // Занесение данных о конце работы потока и его времени выполнения в массивы проверки
            time_mas[Int32.Parse(number.ToString())] = sw.Elapsed.TotalSeconds;
            time_check[Int32.Parse(number.ToString())] = true;
            // задержка
            Thread.Sleep(500);
            // Подсчет окончательного времени работы потоков (если в массиве проверок bool все true)
            CheckTime();
        }

        public void PosledShowBiggestValue()
        {
            // счетчик времени работы
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int max;
            for (int i = 0; i < rows; i++)
            {
                max = matr[i, 0];
                for (int j = 0; j < cols; j++)
                    if (matr[i, j] > max)
                        max = matr[i, j];
                Console.WriteLine("Ряд {0}: максимальный элемент - {1}.", i + 1, max);
            }

            sw.Stop();
            Console.WriteLine("Время работы: {0}мс.", sw.Elapsed.TotalSeconds);
            time_mas[0] = sw.Elapsed.TotalSeconds;
            time_check[0] = true;
        }

        private void CheckTime()
        {
            if (time_check[0] && time_check[1] && time_check[2] && time_check[3])
            {
                Console.WriteLine("\n--------------------------------");
                double tr_sum = time_mas[1] + time_mas[2] + time_mas[3];
                Console.WriteLine("Время работы последовательного поиска: {0}мс.\nВремя работы параллельного поиска: {1}мс.\n", time_mas[0], tr_sum);
            }
        }
    }
}
