using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Program
    {

        static void Main(string[] args)
        {
            // 캐시메모리 공간지역성 테스트
            // [][][][][] [][][][][] [][][][][] [][][][][]

            // 2차원 배열
            int[,] arr = new int[10000, 10000];

            // 메모리 접근 순서
            // [1][2][3][4][5] [][][][][] [][][][][] [][][][][]
            {
                long now = DateTime.Now.Ticks;
                for (int y = 0; y < 10000; y++)
                    for (int x = 0; x < 10000; x++)
                        arr[y, x] = 1;
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(y, x) 순서 걸린 시간 {end - now}");
            }

            // 이 로직이 2배 이상 더 오래걸린다. 
            // 
            // 메모리 접근 순서
            // [1][][][][] [2][][][][] [3][][][][] [4][][][][]
            {
                long now = DateTime.Now.Ticks;
                for (int y = 0; y < 10000; y++)
                    for (int x = 0; x < 10000; x++)
                        arr[x, y] = 1;
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(y, x) 순서 걸린 시간 {end - now}");
            }

            // 단순히 2차원 배열정도에서도 캐시메모리 접근에 따라 속도가 아주많이 차이남. 
            // 멀티쓰레드에서도 캐시메모리 접근문제를 고려해야함.
        }
    }
}
