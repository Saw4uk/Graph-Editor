using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;


    public static class Serializer
    {
        public static void WriteObject<T>(string fileName, T obj)
        {
            var json = JsonUtility.ToJson(obj);
            using var writer = File.CreateText(fileName);
            writer.Write(json);
        }

        public static T ReadObject<T>(string fileName)
        {
            var json = File.ReadAllText(fileName);
            return JsonUtility.FromJson<T>(json);
        }
    }
