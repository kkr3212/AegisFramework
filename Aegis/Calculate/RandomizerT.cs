using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Calculate
{
    public sealed class Randomizer<T>
    {
        private struct RandomItem
        {
            internal int Prob;
            internal T Item;

            public RandomItem(int prob, T item)
            {
                Prob = prob;
                Item = item;
            }
        }


        private static Random _staticRand = new Random((int)DateTime.Now.Ticks);
        private List<RandomItem> _items = new List<RandomItem>();
        private Random _rand;





        public Randomizer()
        {
            _rand = new Random();
        }


        public Randomizer(int seed)
        {
            _rand = new Random(seed);
        }


        public void Clear()
        {
            lock (this)
            {
                _items.Clear();
            }
        }


        public void Seed(int seed)
        {
            _rand = new Random(seed);
        }


        /// <summary>
        /// 지정되지 않은 범위 내에서 임의 값을 반환합니다.
        /// </summary>
        /// <returns>임의 값</returns>
        public int Next()
        {
            return _rand.Next();
        }


        /// <summary>
        /// min 값 이상 부터 max 값 미만 사이의 임의 값을 반환합니다.
        /// </summary>
        /// <param name="min">최소값</param>
        /// <param name="max">최대값</param>
        /// <returns>임의 값</returns>
        public int Next(int min, int max)
        {
            return _rand.Next(min, max);
        }


        /// <summary>
        /// NextItem 함수에서 확률적으로 가져올 수 있는 객체를 추가합니다.
        /// </summary>
        /// <param name="prob">확률값</param>
        /// <param name="item">추가할 객체</param>
        public void AddItem(int prob, T item)
        {
            _items.Add(new RandomItem(prob, item));
        }


        /// <summary>
        /// 저장된 Object 중 임의의 하나를 선택하여 반환합니다.
        /// fraction 값이 저장된 Object의 prob 합 보다 작거나 같으면 저장된 Object 중 하나를 반환합니다.
        /// fraction 값이 저장된 Object의 prob 합 보다 크면 저장된 Object 중 하나를 반환하거나 혹은 default을 반환합니다.
        /// </summary>
        /// <param name="fraction">확률값</param>
        /// <param name="eraseItem">true인 경우, 저장된 Object 중 하나를 반환할 때 해당 Object가 다음에 선택되지 않도록 목록에서 삭제합니다.</param>
        /// <returns>임의로 선택된 객체 혹은 선택되지 않았을 경우 null을 반환합니다.</returns>
        public T NextItem(int fraction, bool eraseItem)
        {
            T ret = default(T);
            int curProb = 0, sumProb = 0;


            if (_items.Count == 0)
                return ret;


            //  전체 확률값 계산
            foreach (RandomItem data in _items)
                sumProb += data.Prob;

            if (fraction < sumProb)
                fraction = sumProb;


            //  확률계산 & 아이템 선택
            curProb = NextNumber(0, fraction);
            foreach (RandomItem data in _items)
            {
                sumProb += data.Prob;
                if (sumProb >= curProb)
                {
                    ret = data.Item;
                    if (eraseItem == true)
                        _items.Remove(data);

                    break;
                }
            }

            return ret;
        }


        /// <summary>
        /// 지정되지 않은 범위 내에서 임의 값을 반환합니다.
        /// </summary>
        /// <returns>임의 값</returns>
        public static int NextNumber()
        {
            return _staticRand.Next();
        }


        /// <summary>
        /// min 값 이상 부터 max 값 미만 사이의 임의 값을 반환합니다.
        /// </summary>
        /// <param name="min">최소값</param>
        /// <param name="max">최대값</param>
        /// <returns>임의 값</returns>
        public static int NextNumber(int min, int max)
        {
            return _staticRand.Next(min, max);
        }
    }
}
