using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public sealed class Randomizer : IDisposable
    {
        private struct RandomItem
        {
            internal Int16 Prob;
            internal Object Item;

            public RandomItem(Int16 prob, Object item)
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


        public Randomizer(Int32 seed)
        {
            _rand = new Random(seed);
        }


        public void Dispose()
        {
            Clear();
        }


        public void Clear()
        {
            lock (this)
            {
                _items.Clear();
            }
        }


        public void Seed(Int32 seed)
        {
            _rand = new Random(seed);
        }


        public void AddItem(Int16 prob, Object item)
        {
            lock (this)
            {
                _items.Add(new RandomItem(prob, item));
            }
        }


        /// <summary>
        /// 저장된 Object 중 임의의 하나를 선택하여 반환합니다.
        /// fraction 값이 저장된 Object의 prob 합 보다 작거나 같으면 저장된 Object 중 하나를 반환합니다.
        /// fraction 값이 저장된 Object의 prob 합 보다 크면 저장된 Object 중 하나를 반환하거나 혹은 null을 반환합니다.
        /// </summary>
        /// <param name="fraction">확률값</param>
        /// <param name="eraseItem">true인 경우, 저장된 Object 중 하나를 반환할 때 해당 Object가 다음에 선택되지 않도록 목록에서 삭제합니다.</param>
        /// <returns>임의로 선택된 Object, 선택되지 않았을 경우 null을 반환합니다.</returns>
        public Object NextItem(Int16 fraction, Boolean eraseItem)
        {
            Object ret = null;
            Int32 curProb = 0, sumProb = 0;


            lock (this)
            {
                if (_items.Count == 0)
                    return null;


                //  전체 확률값 계산
                foreach (RandomItem data in _items)
                    sumProb += data.Prob;

                if (sumProb <= fraction)
                    sumProb = fraction;



                //  확률계산 & 아이템 선택
                curProb = NextNumber(0, sumProb);
                sumProb = 0;

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
            }

            return ret;
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
