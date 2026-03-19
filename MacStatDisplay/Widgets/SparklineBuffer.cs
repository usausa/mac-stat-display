namespace MacStatDisplay.Widgets;

// Allocation-free fixed-capacity ring buffer for sparkline history.
// Pre-filled with zeros so the graph always renders at full width from the first sample.
// Logical index 0 is the oldest sample; Capacity-1 is the newest.
internal sealed class SparklineBuffer
{
    private readonly float[] data;

    // Points to the slot where the NEXT write will go (also the oldest sample's slot).
    private int head;

    internal int Capacity => data.Length;

    internal SparklineBuffer(int capacity)
    {
        data = new float[capacity]; // zero-initialised — graph starts flat
        head = 0;
    }

    // Pushes a new sample, overwriting the oldest one. Never allocates.
    internal void Push(float value)
    {
        data[head] = value;
        head = (head + 1) % data.Length;
    }

    // Returns the sample at logical index i (0 = oldest, Capacity-1 = newest).
    internal float this[int i] => data[(head + i) % data.Length];

    // Returns the maximum value currently in the buffer (O(n), no allocation).
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
