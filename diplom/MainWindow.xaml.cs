using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace diplom
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Graph
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
        public MainWindow()
        {
            InitializeComponent();
            string filePath;
        }

        private void ImportKAOFOButton_Click(object sender, RoutedEventArgs e)
        {
            [DllImport("C:\\Users\\admin\\source\\repos\\ConsoleApp2\\x64\\Debug\\GraphLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
            static extern Graph GraphFromFile(string filePath, ref double executionTime);

            double loadTime = 0;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"; 

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                Graph graph = GraphFromFile(filePath, ref loadTime);

                string kaoContent = string.Join(", ", graph.KAO.Take(graph.VertNumb + 1));
                KAOText.Text = kaoContent;

                string foContent = string.Join(", ", graph.FO.Take(graph.EdgNumb * 2));
                FOText.Text = foContent;
            }
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            [DllImport("C:\\Users\\admin\\source\\repos\\ConsoleApp2\\x64\\Debug\\GraphLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
            static extern double MCReliability(Graph graph, int number, ref double executionTime, ref int breaks);

            double reliabilityTime = 0;
            int breaks = 0;
            int numberOfIterations = 100;
            double reliability = MCReliability(graph, numberOfIterations, ref reliabilityTime, ref breaks);
            Console.WriteLine($"Reliability calculated in {reliabilityTime:F5} seconds");
            Console.WriteLine($"Number of breaks = {breaks}");

            Console.WriteLine($"Reliability: {reliability:F25}");
        }
    }
}
