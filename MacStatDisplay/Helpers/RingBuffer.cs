namespace MacStatDisplay.Helpers;

internal sealed class RingBuffer
{
    private readonly float[] data;

    private int head;

    internal int Capacity => data.Length;

    internal RingBuffer(int capacity)
    {
        data = new float[capacity];
        head = 0;
    }

    internal void Push(float value)
    {
        data[head] = value;
        head = (head + 1) % data.Length;
    }

    internal float this[int i] => data[(head + i) % data.Length];

    internal float Max()
    {
        var max = 0f;
        foreach (var v in data)
        {
            if (v > max)
            {
                max = v;
            }
        }

        return max;
    }
}
