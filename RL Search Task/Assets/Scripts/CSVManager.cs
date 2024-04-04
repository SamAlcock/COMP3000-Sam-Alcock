using OpenCover.Framework.Model;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Analytics;
using File = UnityEngine.Windows.File;

public class CSVManager : MonoBehaviour
{
    public void CSVWrite(string[] data, string csvName)
    {
        string filePath = Path.Combine(Application.dataPath, csvName);
        if (!File.Exists(csvName))
        {
            string header = "Algorithm, Generation, Reward";
            using (StreamWriter sw = new (csvName)) 
            {
                sw.WriteLine(string.Join(", ", header));
                sw.WriteLine(string.Join(", ", data));
            }
        }
        else
        {
            using (StreamWriter sw = new(csvName, true))
            {
                sw.WriteLine(string.Join(", ", data));
            }
        }
    }
}
