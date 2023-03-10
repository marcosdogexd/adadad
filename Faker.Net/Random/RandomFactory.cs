using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Faker.Random
{
    internal class RandomFactory
    {
        private Object obj;
        private LocaleType localeType;
        private const string propertyName = "strPropertyName";
        private const string methodName = "methodName";

        // Might have some other better way to do it
        // For better performance and memory usage
        private Dictionary<string, FakerBase> fakerDictionary;
        
        internal RandomFactory(Object obj, LocaleType type)
        {
            this.obj = obj;
            this.localeType = type;
            this.fakerDictionary = new Dictionary<string, FakerBase>();
        }

        internal string Next<T>(string format, FormatType type = FormatType.Other)
        {
            return FillInRandomData<T>(format, this.obj, type);
        }

        internal string FillInRandomData<T>(string format,  Object obj, FormatType type = FormatType.Other)
        {
            if (type == FormatType.Other)
            {
                string result = format;
                string pattern = @"\#\{(?<" + propertyName + @">\w+)\}";
                var withProperty =  FillInRandomDataFromProperty<T>(format, obj, ref result, pattern);
                return this.FillInRandomDataFromMethod(withProperty);
            }
            return this.FillInRandomDataFromNumber(format);
        }

        private string FillInRandomDataFromNumber(string format, char symbol = '#')
        {
            var result = format.ToCharArray();
            for(int i = 0; i < result.Length; i++)
            {
                if (result[i] == symbol)
                    result[i] = (char)(RandomProxy.Next(10) + 48);
            }
            return new string(result);
        }

        private string FillInRandomDataFromProperty<T>(string format, Object obj, ref string result, string pattern)
        {
            var matches = Regex.Matches(format, pattern);
            while (matches.Count > 0)
            {
                Match match = matches[0];
                string name = match.Groups[propertyName].Value;
                result = result.Remove(match.Index, match.Length).Insert(match.Index, GetRandomItemFromProperty<T>(name, obj).ToString());
                matches = Regex.Matches(result, pattern);
            }
            return FillInRandomDataFromNumber(result); // replace the special # symbol wih random number
        }

        internal string FillInRandomDataFromMethod(string format)
        {
            string result = format;
            string pattern = @"\@\{(?<" + methodName + @">[A-Za-z.]+)\}";
            var matches = Regex.Matches(format, pattern);
            while (matches.Count > 0)
            {
                Match match = matches[0];
                string fullNameSpaces = match.Groups[methodName].Value;
                string[] names = fullNameSpaces.Split('.');
                string[] namespaceArray = new string[names.Length - 1];
                Array.Copy(names, namespaceArray, names.Length - 1);
                string nameSpace = string.Join(".", namespaceArray);
                string methodname = names[names.Length - 1];
                FakerBase faker = this.GetFakerObjectFromName(nameSpace);
                result = result.Remove(match.Index, match.Length).Insert(match.Index, GetRandomItemFromMethod<string>(methodname, faker));
                matches = Regex.Matches(result, pattern);
            }
            return result;
        }

        internal T GetRandomItemFromProperty<T>(string strPropertyName, Object obj, int index = -1)
        {
            T result = default(T);
            Type type = obj.GetType();
            var property = type.GetProperties().FirstOrDefault(entry => entry.Name == strPropertyName);
            if(property != null)
            {
                var values = property.GetValue(obj, null) as IList<T>;
                if (values != null)
                    return index == -1? Selector.GetRandomItemFromList(values) : values[index];
            }
            return result;
        }

        internal FakerBase GetFakerObjectFromName(string name)
        {
            if (this.fakerDictionary.ContainsKey(name))
            {
                return this.fakerDictionary[name];
            }
            // If it is the first time, create the FakerBase and add it to the dictionary
            Type t = Assembly.GetExecutingAssembly().GetType("Faker." + name);
            FakerBase faker = Activator.CreateInstance(t, this.localeType) as FakerBase;
            this.fakerDictionary.Add(name, faker);
            return faker;
        }

        private T GetRandomItemFromMethod<T>(string methodName, FakerBase obj)
        {
            Type type = obj.GetType();
            var method = type.GetMethod(methodName);
            return (T)method.Invoke(obj, null);
        }

        internal bool GetProbablyFromFormat(ref string result)
        {
            const string pattern = @"{0.[0-9]+}";
            var match = Regex.Match(result, pattern);
            if (!match.Success) return true;
            result = result.Remove(match.Index, match.Length);
            float chance = 0f;
            float.TryParse(match.Value.Substring(1, match.Value.Length - 2), out chance);
            return RandomProxy.NextBool(chance);
        }

        
    }

    public enum FormatType
    {
        Number,
        Other
    }
}
