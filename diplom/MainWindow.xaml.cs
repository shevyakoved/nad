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

            

            [DllImport("C:\\Users\\admin\\source\\repos\\ConsoleApp2\\x64\\Debug\\GraphLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
            static extern Graph GraphFromFile(string filePath, ref double executionTime);

            [DllImport("C:\\Users\\admin\\source\\repos\\ConsoleApp2\\x64\\Debug\\GraphLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
            static extern double MCReliability(Graph graph, int number, ref double executionTime, ref int breaks);
                string filePath = "D:\\graph.txt";
                double loadTime = 0, reliabilityTime = 0;
                int breaks = 0;

                // Load graph from file
                Graph graph = GraphFromFile(filePath, ref loadTime);
                Console.WriteLine($"Graph loaded in {loadTime:F5} seconds");
                // Calculate reliability
                int numberOfIterations = 100;
                double reliability = MCReliability(graph, numberOfIterations, ref reliabilityTime, ref breaks);
                Console.WriteLine($"Reliability calculated in {reliabilityTime:F5} seconds");
                Console.WriteLine($"Number of breaks = {breaks}");
                // Display results
                Console.WriteLine($"Reliability: {reliability:F25}");

        }
    }
}
