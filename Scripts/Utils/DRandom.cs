using System;

public class DRandom  {

    public int seed = 0;
    public Random random;

    public DRandom(int _seed = 0)
    {
        seed = _seed;
        random = new Random(seed);
    }

    public float value() {
        return (float)random.NextDouble();
    }

    /// <summary>
    /// returns a random int between min and max
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public int range(int min, int max) {
        return (int)((max - min + 1)*value())+min;
    }
}


