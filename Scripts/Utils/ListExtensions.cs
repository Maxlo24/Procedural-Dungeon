using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class ListExtensions  {
    public static void Shuffle<T>(this IList<T> list, System.Random rnd) {
        for (var i = 0; i < list.Count; i++)
            list.Swap(i, rnd.Next(i, list.Count));
    }

    public static void Swap<T>(this IList<T> list, int i, int j) {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }

    public static bool WithinRange<T>(this IList<T> list, int index){
        if (index >= 0 && index < list.Count) return true;
        else return false;
    }
}

public static class DictionaryExtension
{

    public static List<T> WeightedShuffle<T>(this Dictionary<T, int> _dictionary, System.Random _rnd)
    {

        List<T> _shuffledList = new List<T>();

        int totalWeight = 0;

        Dictionary<T, int> _workingDic = new Dictionary<T, int>(_dictionary);

        foreach (var kvp in _dictionary)
        {
            if (kvp.Value == 0)
                _workingDic.Remove(kvp.Key);
        }

        while (_workingDic.Count > 0) {

            foreach (var kvp in _workingDic)
            {
                totalWeight += kvp.Value;
            }

            int randomNumber = _rnd.Next(0, totalWeight);
            foreach (var kvp in _workingDic)
            {
                if (randomNumber < kvp.Value)
                {
                    _shuffledList.Add(kvp.Key);
                    _workingDic.Remove(kvp.Key);
                    break;
                }
                randomNumber = randomNumber - kvp.Value;
            }
        }

        return _shuffledList;
    }


}
