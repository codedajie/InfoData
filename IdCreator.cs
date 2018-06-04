namespace ChivaVR.Net.Core
{
    public static class ClientIdCreator
    {
        private static int _i = 0;

        public static int NextId
        {
            get
            {
                _i++;
                if (_i > int.MaxValue)
                {
                    _i = int.MinValue;
                }
                return _i;
            }
        }
    }
}
