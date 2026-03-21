namespace MacStatDisplay.Helpers;

internal sealed class RingBuffer
{
    private readonly float[] data;

    private int head;

    public int Capacity => data.Length;

    public RingBuffer(int capacity)
    {
        data = new float[capacity];
        head = 0;
    }

    public void Push(float value)
    {
        data[head] = value;
        head = (head + 1) % data.Length;
    }

    public float this[int i] => data[(head + i) % data.Length];

    public float Max()
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
