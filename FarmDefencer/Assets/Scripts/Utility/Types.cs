namespace JoonyleGameDevKit
{
    [System.Serializable]
    public class RangeFloat
    {
        private readonly System.Random random = new System.Random();

        public float Start;
        public float End;

        public float Random()
        {
            var offset = End - Start;
            var factor = random.NextDouble();
            return Start + (float)(offset * factor);
        }
    }

    [System.Serializable]
    public class RangeInt
    {
        private readonly System.Random random = new System.Random();

        public int Start;
        public int End;

        public int Random()
        {
            return random.Next(Start, End);
        }
    }
}