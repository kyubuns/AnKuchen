namespace AnKuchen
{
    public static class FastHash
    {
        public static uint CalculateHash(string read, int seek)
        {
            var hashedValue = 2654435761;
            for (var i = 0; i < read.Length; ++i)
            {
                if (read[i] == '/')
                {
                    seek--;
                    continue;
                }

                if (seek != 0) continue;

                hashedValue += read[i];
                hashedValue *= 2654435761;
            }
            return hashedValue;
        }

        public static uint CalculateHash(string read)
        {
            var hashedValue = 2654435761;
            foreach (var t in read)
            {
                hashedValue += t;
                hashedValue *= 2654435761;
            }
            return hashedValue;
        }

        public static uint CalculateHash(uint[] read)
        {
            var hashedValue = 2654435761;
            foreach (var t in read)
            {
                hashedValue += t;
                hashedValue *= 2654435761;
            }
            return hashedValue;
        }
    }
}
