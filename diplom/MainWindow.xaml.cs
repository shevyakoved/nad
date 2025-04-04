using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace diplom
{
    //[StructLayout(LayoutKind.Sequential)]
    //struct Graph
    //{
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10001)]
    //    public int[] KAO;

    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30000)]
    //    public int[] FO;

    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30000)]
    //    public double[] PArray;

    //    public int VertNumb;
    //    public int EdgNumb;
    //}


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
        [DllImport("KTerminalDll.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern double CalculateReliability(ref KGraph G, ref double timeMS, ref int recursionCount);

        [DllImport("KTerminalDll.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void KGraphFromStrings(string kao, string fo, string targets, ref KGraph graph, double p);



        [DllImport("wirelessDll.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void WGraphFromStrings(string kao, string fo, string targets, ref WGraph graph, double p);
        [DllImport("wirelessDll.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern double WReliabilityWithTime(ref WGraph G, out double outTimeInSeconds);

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

            if (openFileDialog.ShowDialog() == true)
            {
                var stopwatch = Stopwatch.StartNew();

                string[] lines = File.ReadAllLines(openFileDialog.FileName);
                KAOText.Text = lines[2];
                FOText.Text = lines[3];
                TargetsText.Text = lines[4];
                stopwatch.Stop();

                Graphload.Text = stopwatch.ToString();
            }
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(KAOText.Text) || string.IsNullOrWhiteSpace(FOText.Text))
            {
                MessageBox.Show("Введите данные!");
                return;
            }
            double timeMs = 0;
            int recursionCount = 0;

            if (ATRButton.IsChecked == true)
            {

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

                    double p = double.Parse(PText.Text, CultureInfo.InvariantCulture);

                    KGraphFromStrings(KAOText.Text, FOText.Text, TargetsText.Text, ref graph, p);

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
            else if(MENCButton.IsChecked == true)
            {
                try
                {
                    WGraph graph = new WGraph
                    {
                        KAO = new int[101],
                        FO = new int[200],
                        PArray = new double[200],
                        Targets = new int[101],
                        PVert = new double[100]
                    };

                    double p = double.Parse(PText.Text, CultureInfo.InvariantCulture);
                    WGraphFromStrings(KAOText.Text, FOText.Text, TargetsText.Text, ref graph, p);
                    double reliability = WReliabilityWithTime(ref graph, out timeMs);

                    Reliability.Text = timeMs.ToString("F2");
                    Breaks.Text = recursionCount.ToString();
                    ReliabilityResult.Text = reliability.ToString("F6");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка расчета надежности: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Выберите метод");
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
                                         $"Recursion: {Breaks.Text}\n" +
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
