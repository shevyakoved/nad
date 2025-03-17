#define _CRT_SECURE_NO_WARNINGS
#include <iostream>
#include <stdio.h>
#include <iomanip>
#include <time.h>
#include <cstdlib>
#include <ctime>
#include <cmath>
#include <Windows.h>

using namespace std;

const int NV = 10000;
const int NE = 15000;

int Number_of_breaks = 0;

struct Graph {
    int KAO[NV + 1];
    int FO[2 * NE];
    double PArray[2 * NE];
    int VertNumb;
    int EdgNumb;
};

extern "C" __declspec(dllexport) Graph GraphFromFile(const char* filePath, double* executionTime);
extern "C" __declspec(dllexport) double MCReliability(Graph G, int Number, double* executionTime, int* breaks);

Graph GraphFromFile(const char* filePath, double* executionTime) {
    clock_t start = clock(); // Start time measurement
    char ch;	Graph G;	int i;
    FILE* F;
    F = fopen(filePath, "rt");
    fscanf(F, "%d", &G.VertNumb);
    fscanf(F, "%d", &G.EdgNumb);
    for (i = 0; i < G.VertNumb; i++)
    {
        fscanf(F, "%d", &G.KAO[i]);
        fscanf(F, "%c", &ch);
    }
    fscanf(F, "%d", &G.KAO[G.VertNumb]);
    for (i = 0; i < G.EdgNumb * 2 - 1; i++)
    {
        fscanf(F, "%d", &G.FO[i]);
        fscanf(F, "%c", &ch);
    }
    fscanf(F, "%d", &G.FO[G.EdgNumb * 2 - 1]);
    fclose(F);
    for (int i = 0; i < 2 * G.EdgNumb; i++) G.PArray[i] = 0.999;
    clock_t end = clock(); // End time measurement
    *executionTime = static_cast<double>(end - start) / CLOCKS_PER_SEC;

    return G;
}


bool Generate_Graph_with_Connectivity_Tracing(Graph G) {
    bool Boolka = true;
    float r = rand();
    int A[NV + 1], B[NV + 1], Spot[NV + 1], DegMas[NV + 1];
    int LB, v, i, j, sum = 1, l = 1;

    for (i = 1; i < 1 + G.VertNumb; i++) DegMas[i] = G.KAO[i] - G.KAO[i - 1];

    A[0] = 1;
    for (i = 0; i < 1 + G.VertNumb; i++) Spot[i] = 0;
    Spot[1] = 1;

    while (l > 0) {
        LB = 0;

        for (i = 0; i < l; i++) {
            for (j = G.KAO[A[i] - 1]; j < G.KAO[A[i]]; j++) {
                v = G.FO[j];
                if (Spot[v] == 0) {
                    r = rand();
                    r = r / RAND_MAX;
                    if (G.PArray[j] >= r) // edge exists
                    {
                        LB++;
                        B[LB - 1] = v;
                        Spot[v] = 1;
                        sum++;
                    }
                    else {
                        DegMas[v]--;
                        if (DegMas[v] == 0) { Boolka = false; break; }
                    }
                }
            }
            if (!Boolka) break;
        }

        if (!Boolka) {
            Number_of_breaks++;
            break; // For testing
        }
        else {
            l = LB;
            if (l > 0)
                for (i = 0; i < l; i++) A[i] = B[i];
        }
    }

    return (sum == G.VertNumb);
}

double MCReliability(Graph G, int Number, double* executionTime, int* breaks) {
    clock_t start = clock(); // Start time measurement
    double q, g;
    int Sch = 0;
    bool Boolka;
    for (int i = 0; i < Number; i++) {
        Boolka = Generate_Graph_with_Connectivity_Tracing(G);
        if (Boolka == true) Sch++;
    }
    *breaks = Number_of_breaks;
    q = Sch;
    g = Number;

    clock_t end = clock(); // End time measurement
    *executionTime = static_cast<double>(end - start) / CLOCKS_PER_SEC;

    return q / g;

}
