using System;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System.IO;

namespace EV
{
    public class BLAS : Hub
    {
        public double[,] QR(double[,] mat1, int mat2)
        {
            double[,] A = mat1;
            int M = mat1.GetLength(0); // number of rows in matrix A
            double[,] Q = new double[M, M]; // dot product D
            double[,] R = new double[M, M];
            double[] T = new double[M * M];
            string[] D = new string[M];
            double sum = 0;
            // Solving for each row of R and each column of Q in a iteration.
            for (int i = 0; i < M; i++)
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
                if (i < M - 1)
                {
                    for (int d = i; d < M - 1; d++)
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
            if (mat2 == 1)
            {
                return Q;
            }
            else
            {
                return R;
            }
            
        }

        public string[] Blas1(String mat1, String connID)
        {
            
            var A = JsonConvert.DeserializeObject<Matrix>(mat1);
            // see below for MatObject class        
            int N = A.size[1]; // the number of columns in matrix A 
            int M = A.size[0]; // number of rows in matrix A
            string[] D = new string[M]; 
            double[,] Q = new double[M, M]; // dot product D
            double[,] R = new double[M, M];
            double[,] U = new double[M, M]; 
            //if (N != M)
            //{
              //  Clients.Client(connID).displayError1();
                //return D;
           // }
            
            for (int i = 0; i < M; i++ )
            {
                for (int j =0; j<M ; j++)
                {
                    U[i, j] = A.data[i, j];
                }
            }
            Q = QR(U, 1);
            R = QR(U, 2);
            U = Blas3(R, Q);
            
            for (int i = 0; i < 0; i++ )
            {
                Q = QR(U,1);
                R = QR(U,2);
                U = Blas3(R, Q);
            }
            
            string strC = "";
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < M; j++)
                {                    
                    strC =R[i, j].ToString() ;
                    Clients.Client(connID).store(strC);                  
                }
                
                
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
