using System.Collections.Generic;

namespace RingLib.Utils;

internal class RandomSelectorItem<T>
{
    public T Value { get; }
    public float Weight { get; }
    public int MaxCount { get; }
    public int CurrentCount = 0;
    public int MaxMiss { get; }
    public int CurrentMiss = 0;

    public RandomSelectorItem(T value, float weight, int maxCount, int maxMiss)
    {
        Value = value;
        if (weight < 0)
        {
            Log.LogError(GetType().Name, "Weight must be non-negative");
        }
        Weight = weight;
        MaxCount = maxCount;
        MaxMiss = maxMiss;
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
            Log.LogError(GetType().Name, "All items have reached their max count");
            candidates.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                candidates.Add(i);
            }
        }
        List<int> missedCandidates = [];
        for (int i = 0; i < candidates.Count; i++)
        {
            if (items[candidates[i]].CurrentMiss >= items[candidates[i]].MaxMiss)
            {
                missedCandidates.Add(candidates[i]);
            }
        }
        if (missedCandidates.Count > 0)
        {
            if (missedCandidates.Count == 1)
            {
                var value = items[missedCandidates[0]].Value;
                Log.LogInfo(GetType().Name, $"Only one item {value} has missed enough times, selecting it");
            }
            else
            {
                Log.LogInfo(GetType().Name, "Multiple items have missed enough times, selecting one randomly");
            }
            candidates = missedCandidates;
        }
        var randomIndex = GetRandomIndex(candidates);
        for (int i = 0; i < items.Count; i++)
        {
            if (i == randomIndex)
            {
                ++items[i].CurrentCount;
                items[i].CurrentMiss = 0;
            }
            else
            {
                items[i].CurrentCount = 0;
                ++items[i].CurrentMiss;
            }
        }
        return items[randomIndex].Value;
    }
}
