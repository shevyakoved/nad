using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.IO;

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


    [StructLayout(LayoutKind.Sequential)]
    struct KGraph
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 51)]
        public int[] KAO;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public int[] FO;

        public int VertexCount;
        public int EdgeCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public double[] PArray;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 51)]
        public int[] Targets;

    }

    [StructLayout(LayoutKind.Sequential)]
    struct WGraph
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 101)]
        public int[] KAO;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public int[] FO;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public double[] PArray;

        public int VertexCount;
        public int EdgeCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 101)]
        public int[] Targets;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public double[] PVert;

    }

    public partial class MainWindow : Window
    {
        [DllImport("GraphLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern Graph GraphFromFile(string filePath, ref double executionTime);

        [DllImport("GraphLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern double MCReliability(ref Graph graph, int number, ref double executionTime, ref int breaks);

        [DllImport("GraphLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void GraphFromStrings(string KAO, string FO, IntPtr graphPtr);



        [DllImport("KTerminalDll.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern double CalculateReliability(ref KGraph G, ref double timeMS, ref int recursionCount);

        [DllImport("KTerminalDll.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern KGraph ReadingGraphFromFile(string filePath, ref double executionTime);

        [DllImport("KTerminalDll.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void KGraphFromStrings(string kao, string fo, string targets, ref KGraph graph);


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
            if (ATRButton.IsChecked == true)
            {
                TargetsText.IsReadOnly = true;
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
            else if (KTRButton.IsChecked == true)
            {
                TargetsText.IsReadOnly = false;
                try
                {
                    if (openFileDialog.ShowDialog() == true)
                    {
                        string filePath = openFileDialog.FileName;
                        KGraph graph = new KGraph();

                        graph = ReadingGraphFromFile(filePath, ref loadTime);
                        if (graph.FO[graph.FO.Length - 1] != -858993460 || graph.KAO[graph.KAO.Length - 1] != -858993460)
                        {
                            KAOText.Text = string.Join(", ", graph.KAO.Take(graph.VertexCount + 1));
                            FOText.Text = string.Join(", ", graph.FO.Take(graph.EdgeCount * 2));
                            if (graph.Targets[1] >= 0)
                            {
                                TargetsText.Text = string.Join(", ", graph.Targets.Take(graph.VertexCount + 1));
                            }
                            Graphload.Text = loadTime.ToString("R");
                        }
                        else
                        {
                            MessageBox.Show("Неверный формат файла");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки графа: {ex.Message}");
                }
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
            double timeMs = 0;
            int recursionCount = 0;

            if (ATRButton.IsChecked == true)
            {
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

                    GraphFromStrings(KAOText.Text, FOText.Text, graphPtr);

                    graph = Marshal.PtrToStructure<Graph>(graphPtr);

                    double reliability = 0;

                    reliability = MCReliability(ref graph, numberOfIterations, ref reliabilityTime, ref breaks);

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
            else if (KTRButton.IsChecked == true)
            {
                try
                {
                    KGraph graph = new KGraph
                    {
                        KAO = new int[51],
                        FO = new int[100],
                        PArray = new double[100],
                        Targets = new int[51]
                    };
                    KGraphFromStrings(KAOText.Text, FOText.Text, TargetsText.Text, ref graph);

                    
                    double reliability = CalculateReliability(ref graph, ref timeMs, ref recursionCount);

                    Reliability.Text = timeMs.ToString("F2");
                    Breaks.Text = recursionCount.ToString();
                    ReliabilityResult.Text = reliability.ToString("F6");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка расчета надежности: {ex.Message}");
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
