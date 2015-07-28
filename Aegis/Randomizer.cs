using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public sealed class Randomizer
    {
        private static Random _staticRand = new Random((Int32)DateTime.Now.Ticks);
        private Random _rand;



        public Randomizer()
        {
            _rand = new Random();
        }


        public Randomizer(Int32 seed)
        {
            _rand = new Random(seed);
        }


        public void Seed(Int32 seed)
        {
            _rand = new Random(seed);
        }


        /// <summary>
        /// 지정되지 않은 범위 내에서 임의 값을 반환합니다.
        /// </summary>
        /// <returns>임의 값</returns>
        public static Int32 NextNumber()
        {
            return _staticRand.Next();
        }


        /// <summary>
        /// min 값 이상 부터 max 값 미만 사이의 임의 값을 반환합니다.
        /// </summary>
        /// <param name="min">최소값</param>
        /// <param name="max">최대값</param>
        /// <returns>임의 값</returns>
        public static Int32 NextNumber(Int32 min, Int32 max)
        {
            return _staticRand.Next(min, max);
        }
    }
}
