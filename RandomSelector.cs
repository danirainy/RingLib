namespace RingLib;

internal class RandomSelectorItem<T>
{
    public T Value { get; }
    public float Weight { get; }
    public int MaxCount { get; }
    public int CurrentCount = 0;
    public RandomSelectorItem(T value, float weight, int maxCount)
    {
        Value = value;
        if (weight < 0)
        {
            Log.LogError(GetType().Name, "Weight must be non-negative");
        }
        Weight = weight;
        MaxCount = maxCount;
    }
}

internal class RandomSelector<T>
{
    private List<RandomSelectorItem<T>> items;

    public RandomSelector(List<RandomSelectorItem<T>> items)
    {
        this.items = items;
    }

    private int GetRandomIndex(List<int> candidates)
    {
        List<float> newWeights = [];
        float totalWeight = 0;
        foreach (var i in candidates)
        {
            newWeights.Add(items[i].Weight);
            totalWeight += items[i].Weight;
        }
        for (int i = 0; i < candidates.Count; i++)
        {
            if (totalWeight == 0)
            {
                newWeights[i] = 1 / candidates.Count;
            }
            else
            {
                newWeights[i] /= totalWeight;
            }
        }
        float randomValue = UnityEngine.Random.Range(0f, 1);
        float currentWeight = 0;
        for (int i = 0; i < candidates.Count; i++)
        {
            currentWeight += newWeights[i];
            if (randomValue <= currentWeight || i + 1 == candidates.Count)
            {
                return candidates[i];
            }
        }
        Log.LogError(GetType().Name, "Failed to get random key");
        return -1;
    }

    public T Get()
    {
        List<int> candidates = [];
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].CurrentCount < items[i].MaxCount)
            {
                candidates.Add(i);
            }
        }
        if (candidates.Count == 0)
        {
            Log.LogError(GetType().Name, "All keys have reached their max count");
            candidates.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                candidates.Add(i);
            }
        }
        var randomIndex = GetRandomIndex(candidates);
        for (int i = 0; i < items.Count; i++)
        {
            if (i == randomIndex)
            {
                ++items[i].CurrentCount;
            }
            else
            {
                items[i].CurrentCount = 0;
            }
        }
        return items[randomIndex].Value;
    }
}
