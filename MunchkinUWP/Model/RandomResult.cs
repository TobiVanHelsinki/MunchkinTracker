namespace MunchkinUWP.Model
{
    public class RandomResult
    {
        public uint nResult;
        public uint nMax;

        internal RandomResult(uint nResult, uint nMax)
        {
            this.nResult = nResult;
            this.nMax = nMax;
        }
    }
}
