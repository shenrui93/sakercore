/***************************************************************************
 * 
 * 创建时间：   2016/12/24 11:00:21
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   服务器随机数提供类，为服务器提供随机数生成服务
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SakerCore.Tools
{
    /// <summary>
    /// 服务器随机数提供类，为服务器提供随机数生成服务
    /// </summary>
    public static class RandomService
    {
        //服务器随机数对象
        static IRandom _random;
        /// <summary>
        /// 获取基础随机数提供对象
        /// </summary>
        public static IRandom RndInstance
        {
            get
            {
                //利用加密随机数生成的强随机数序列
                return _random ?? (_random = new private_Random());
            }
        }
        /// <summary>
        /// 提供一种机制，随机数的生成概率从高到低依次递增，非等概率的随机数生成
        /// </summary>
        /// <param name="min">最小数</param>
        /// <param name="max">最大数</param>
        /// <returns></returns>
        public static int GetNewRandomNum(int min, int max)
        {
            return GetNewRandomNum(min, max, 2);
        }
        /// <summary>
        /// 提供一种机制，随机数的生成概率从高到低依次递增，非等概率的随机数生成
        /// </summary>
        /// <param name="min">最小数</param>
        /// <param name="max">最大数</param>
        /// <param name="count">计算基数</param>
        /// <returns></returns>
        public static int GetNewRandomNum(int min, int max, int count)
        {
            int num = max;
            while (--count >= 0)
            {
                num = RndInstance.Next(min, num);
            }
            return num;
        }
        /// <summary>
        /// 返回两个服从正态分布N(0,1)的随机数z0 和 z1
        /// </summary>
        /// <returns></returns>
        internal static double[] NormalDistribution()
        {
            Random rand = new Random();
            double[] y;
            double u1, u2, v1 = 0, v2 = 0, s = 0, z1 = 0, z2 = 0;
            while (s > 1 || s == 0)
            {
                u1 = rand.NextDouble();
                u2 = rand.NextDouble();
                v1 = 2 * u1 - 1;
                v2 = 2 * u2 - 1;
                s = v1 * v1 + v2 * v2;
            }
            z1 = Math.Sqrt(-2 * Math.Log(s) / s) * v1;
            z2 = Math.Sqrt(-2 * Math.Log(s) / s) * v2;
            y = new double[] { z1, z2 };
            return y; //返回两个服从正态分布N(0,1)的随机数z0 和 z1
        }

        /// <summary>
        /// 随机数生成服务对象
        /// </summary>
        class private_Random : IRandom
        {
            static int currentStep = 0;
            static int GetRandomCode()
            {
                int step = (int)DateTime.Now.Ticks;
                step += Interlocked.Increment(ref currentStep);
                return Math.Abs(step);
            }
            static Random systemRandom { get { return new Random(GetRandomCode()); } }

            public int Next(int min, int max)
            {
                return systemRandom.Next(min, max);
            }
            public double NextDouble()
            {
                return systemRandom.NextDouble();
            }

            public void NextBytes(byte[] data)
            {
                if (data == null || data.Length ==0) return;
                systemRandom.NextBytes(data);
            }
        }

    }

    /// <summary>
    /// 定义一个接口，该接口定义提供随机数生成必须实现的方法
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// 随机生成一个大于等于0.0且小于等于1.0的一个双精度的浮点型数值
        /// </summary>
        /// <returns></returns>
        double NextDouble();
        /// <summary>
        /// 生成一个大于等于最小值且小于最大值的随机整数
        /// </summary>
        /// <param name="min">随机数的最小值，随机数可以取该最小值</param>
        /// <param name="max">随机数最大值，随机数不可以取得该最大值</param>
        /// <returns></returns>
        int Next(int min, int max);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        void NextBytes(byte[] data);
    }
}
