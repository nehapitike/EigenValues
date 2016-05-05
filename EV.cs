using System;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System.IO;

namespace EV
{
    public class EV : Hub
    {
        public double[,] QR(double[,] mat1, int mat2)
        {
            double[,] A = mat1;
            int M = mat1.GetLength(0);
            int N = mat1.GetLength(1);// number of rows in matrix A
            double[,] Q = new double[M, N]; 
            double[,] R = new double[N, N];
            double[] T = new double[M * N];
            string[] D = new string[N];
            double sum = 0;
            // Solving for each row of R and each column of Q in a iteration.
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    sum = sum + A[j, i] * A[j, i];
                }
                R[i, i] = Math.Sqrt(sum);
                for (int j = 0; j < M; j++)
                {
                    Q[j, i] = A[j, i] / R[i, i];

                }
                sum = 0;
                if (i < N - 1)
                {
                    for (int d = i; d < N - 1; d++)
                    {
                        sum = 0;
                        for (int j = 0; j < M; j++)
                        {
                            sum = sum + Q[j, i] * A[j, d + 1];
                        }
                        R[i, d + 1] = sum;
                        for (int j = 0; j < M; j++)
                        {
                            A[j, d + 1] = A[j, d + 1] - (Q[j, i] * R[i, d + 1]);
                        }
                    }
                }
                sum = 0;

            }           

            if (mat2 == 2)
            {
                return Q;
            }
            else
            {
                return R;
            }
            
        }

        public string[] eigenValue(String mat1, String connID)
        {
            
            var A = JsonConvert.DeserializeObject<Matrix>(mat1);
            // see below for MatObject class        
            int N = A.size[1]; // the number of columns in matrix A 
            int M = A.size[0]; // number of rows in matrix A
            string[] D = new string[M]; 
            double[,] Q = new double[M, M]; 
            double[,] R = new double[M, M];
            double[,] U = new double[M, M]; 
            if (N != M)
            {
                Clients.Client(connID).displayError1();
                return D;
            }
            
            for (int i = 0; i < M; i++ )
            {
                for (int j =0; j<N; j++)
                {
                    U[i, j] = A.data[i, j];
                }
            }
            //Computing QR and calculatring A=RQ for 300 iterations to converge
            R = QR(U, 1);
            Q = QR(U, 2);
            U = Blas3(R, Q);

            for (int i = 0; i < 300; i++)
            {
                R = QR(U, 1);
                Q = QR(U, 2);
                U = Blas3(R, Q);
            }
            
            string strC = "";
            for (int i = 0; i < M; i++)
            {
                if (Math.Round(U[i, i]) == 0)
                {
                    U[i, i] = Math.Round(U[i, i]);
                }
                    strC = U[i, i].ToString();
                    Clients.Client(connID).store(strC);
            }
            
            D[1] = M.ToString();
            Clients.Client(connID).displayOutput(M.ToString());
            Console.Out.WriteLine(R);
            return D;
        }
        public double[,] Blas3(double[,] var1, double[,] var2)
        {
            double[,] R = var1; // dot product D
            double[,] Q = var2;
            int M = Q.GetLength(0);
            double[,] U= new double[M, M];
            double C = 0;
            string[] D = new string[M]; // dot product D
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    for (int z = 0; z < M; z++)
                    {
                        C = C + R[i, z] * Q[z, j];
                    }
                    U[i, j] = C;
                    C = 0;
                       
                }
            }

            return U;
        }
        // This class can be generated using http://json2csharp.com/
        // Enter {"matrixType":"DenseMatrix", "data": [[1,2], [3,4], [5,6]], "size": [3, 2]}
        public class Matrix
        {
            public string matrixType { get; set; }
            public double[,] data { get; set; }
            public int[] size { get; set; }
        }
    }
}
