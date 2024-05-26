using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))"; // Define delimiters, regular expression craziness
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r"; // Define line delimiters, regular experession craziness
    static char[] TRIM_CHARS = { '\"' };

    /// <summary>
    /// This methods will read from the string uploaded, handles a minimal amount of exception handling. 
    /// </summary>
    /// <param name="csvContent"></param>
    /// <returns></returns>
    public static List<Dictionary<string, object>> ReadFromString(string csvContent)
    {
        var list = new List<Dictionary<string, object>>();
        var lines = Regex.Split(csvContent, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);

        for (var i = 1; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", ""); // Trim characters
                object finalvalue = value;

                int n; // Create int, to hold value if int

                float f; // Create float, to hold value if float

                // If-else to attempt to parse value into int or float
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        return list;
    }
}