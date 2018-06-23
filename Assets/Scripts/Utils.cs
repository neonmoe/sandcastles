using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    public static string ConvertToString(List<List<float>> list) {
        string s = "[\n";
        foreach (var subList in list) {
            s += $"  {ConvertToString(subList)},\n";
        }
        return s + "]";
    }
    public static string ConvertToString(List<float> list) {
        List<string> formattedList = new List<string>();
        foreach (var f in list) {
            formattedList.Add(f.ToString("0.0"));
        }
        return "[" + string.Join(", ", formattedList) + "]";
    }
}
