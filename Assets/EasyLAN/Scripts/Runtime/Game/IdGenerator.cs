using System.Threading;

namespace AillieoUtils.EasyLAN
{
    public class IdGenerator
    {
        private int sid;

        public int Get()
        {
            do
            {
                int cur = sid;
                int newId = cur + 1;

                int exchangedId = Interlocked.CompareExchange(ref sid, newId, cur);

                if (exchangedId == cur)
                {
                    return newId;
                }

            } while (true);
        }
    }
}
