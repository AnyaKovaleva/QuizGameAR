using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class JSON_Reader
{
    
    public static QuestionCollection ReadQuestionCollection(string jsonFileName)
    {
        QuestionCollection questionCollection;

        string jsonFilePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        string jsonString;
        
        WWW reader = new WWW(jsonFilePath);
        while (!reader.isDone) { }

        jsonString = Encoding.UTF8.GetString(reader.bytes, 3, reader.bytes.Length - 3);
        Debug.Log(jsonString);

        questionCollection = JsonUtility.FromJson<QuestionCollection>(jsonString);

        return questionCollection;
    }
}
