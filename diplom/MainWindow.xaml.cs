using Microsoft.Win32;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace diplom
{
    [StructLayout(LayoutKind.Sequential)]
    struct Graph
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10001)]
        public int[] KAO;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30000)]
        public int[] FO;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30000)]
        public double[] PArray;

        public int VertNumb;
        public int EdgNumb;
    }

    public partial class MainWindow : Window
    {
        [DllImport("C:\\Users\\admin\\source\\repos\\ConsoleApp2\\x64\\Debug\\GraphLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern Graph GraphFromFile(string filePath, ref double executionTime);

        [DllImport("C:\\Users\\admin\\source\\repos\\ConsoleApp2\\x64\\Debug\\GraphLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern double MCReliability(ref Graph graph, int number, ref double executionTime, ref int breaks);

        [DllImport("C:\\Users\\admin\\source\\repos\\ConsoleApp2\\x64\\Debug\\GraphLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void GraphFromStrings(string KAO, string FO, IntPtr graphPtr);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ImportKAOFOButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
            };

            double loadTime = 0;

            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    Graph graph;

                    try
                    {
                        graph = GraphFromFile(filePath, ref loadTime);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке графа: {ex.Message}");
                        return;
                    }

                    KAOText.Text = string.Join(", ", graph.KAO.Take(graph.VertNumb + 1));
                    FOText.Text = string.Join(", ", graph.FO.Take(graph.EdgNumb * 2));
                    Graphload.Text = loadTime.ToString("R");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия файла: {ex.Message}");
            }
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(KAOText.Text) || string.IsNullOrWhiteSpace(FOText.Text))
            {
                MessageBox.Show("Введите данные!");
                return;
            }

            int numberOfIterations = 100;
            double reliabilityTime = 0;
            int breaks = 0;

            Graph graph = new Graph
            {
                KAO = new int[10001],
                FO = new int[30000],
                PArray = new double[30000]
            };

            IntPtr graphPtr = IntPtr.Zero;

            try
            {
                graphPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Graph)));

                if (graphPtr == IntPtr.Zero)
                {
                    MessageBox.Show("Ошибка выделения памяти!");
                    return;
                }

                Marshal.StructureToPtr(graph, graphPtr, false);

                try
                {
                    GraphFromStrings(KAOText.Text, FOText.Text, graphPtr);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка вызова GraphFromStrings: {ex.Message}");
                    return;
                }

                graph = Marshal.PtrToStructure<Graph>(graphPtr);

                double reliability = 0;

                try
                {
                    reliability = MCReliability(ref graph, numberOfIterations, ref reliabilityTime, ref breaks);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка вычисления надежности: {ex.Message}");
                    return;
                }

                Breaks.Text = breaks.ToString();
                Reliability.Text = reliabilityTime.ToString("R");
                ReliabilityResult.Text = reliability.ToString("R");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Общая ошибка: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                if (graphPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(graphPtr);
                }
            }
        }

        private void SaveResultButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Сохранить отчет",
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                FileName = $"ReliabilityReport_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileContent = $"Дата: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                         $"Breaks: {Breaks.Text}\n" +
                                         $"Reliability Time: {Reliability.Text}\n" +
                                         $"Reliability Result: {ReliabilityResult.Text}\n";

                    System.IO.File.WriteAllText(saveFileDialog.FileName, fileContent);
                    MessageBox.Show($"Файл успешно сохранен:\n{saveFileDialog.FileName}", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
