using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    public static SaveData i;
    void Start()
    {
        if (i == null) i = this;
        
    }

    public void LoadSave()
    {
        string line;
        GameObject character = GameObject.FindGameObjectWithTag("Player");
        try
        {
            using (StreamReader reader = new StreamReader("SaveFile.txt"))
            {

                while ((line = reader.ReadLine()) != null)
                {
                    string[] strlist = line.Split(' ');
                    character.transform.position = new Vector2(float.Parse(strlist[0]), float.Parse(strlist[1]));


                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading file: {ex.Message}");

        }
        
    }

    public void SaveGame(Vector2 pos)
    {
        using (StreamWriter writer = new StreamWriter("SaveFile.txt"))
        {
            writer.WriteLine($"{pos.x} {pos.y}");
        }
    }
}
