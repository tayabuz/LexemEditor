using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace LexemEditor
{
    public class LexemesFilesHandler
    {
        public Dictionary<string, List<LexemValue>> Lexemes { get; set; }

        public readonly List<string> Languages;

        public LexemesFilesHandler() { }
        private string[] GetPathesToFiles(string pathOfFolder)
        {
            string[] result = Directory.GetFiles(pathOfFolder, "*.lst");
            Array.Sort(result);
            return result;
        }

        private List<string> GetLanguages(params string[] namesOfFiles)
        {
            List<string> result = new List<string>();
            const string filename = "Messages";
            foreach (var name in namesOfFiles)
            {
                if (Regex.IsMatch(name, filename + @"-[A-Za-z]{2}.lst$"))
                {
                    string language = name.Remove(name.LastIndexOf(".lst"), name.Length - name.LastIndexOf(".lst"));
                    language = language.Remove(0, language.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    language = language.Remove(0, language.LastIndexOf('-') + 1);
                    result.Add(language);
                }
                else
                {
                    result.Add("en");
                }
            }
            return result;
        }

        private List<string[]> GetContentOfFiles(params string[] pathsToFiles)
        {
            List<string[]> contentOfFiles = new List<string[]>();
            foreach (var path in pathsToFiles)
            {
                string[] textInFile = File.ReadAllLines(path);
                contentOfFiles.Add(textInFile);
            }
            return contentOfFiles;
        }

        private Dictionary<string, List<LexemValue>> GetLexemesFromFiles(params string[] pathsToFiles)
        {
            return GetLexemesFromListOfStrings(GetContentOfFiles(pathsToFiles));
        }

        private Dictionary<string, List<LexemValue>> GetLexemesFromListOfStrings(List<string[]> list)
        {
            int index = 0;
            Dictionary<string, List<LexemValue>> result = new Dictionary<string, List<LexemValue>>();
            foreach (var contentInFile in list)
            {
                foreach (var stringInFile in contentInFile)
                {
                    if (!stringInFile.StartsWith("#") && stringInFile.Contains("="))
                    {
                        string key = stringInFile.Remove(stringInFile.IndexOf('=')).Trim();
                        string value = stringInFile.Remove(0, stringInFile.LastIndexOf('=') + 1).Trim();
                        if (!result.ContainsKey(key))
                        {
                            LexemValue lexemValue = new LexemValue {Value = value, Language = Languages[index]};
                            List<LexemValue> listOfLexemValues = new List<LexemValue> { lexemValue };
                            result.Add(key, listOfLexemValues);
                        }
                        else
                        {
                            List<LexemValue> valuesInResultByKey = result[key];
                            LexemValue lexemValue = new LexemValue {Value = value, Language = Languages[index]};
                            new List<LexemValue> { lexemValue };
                            if (!valuesInResultByKey.Contains(lexemValue))
                            {
                                valuesInResultByKey.Add(lexemValue);
                                result[key] = valuesInResultByKey;
                            }
                        }
                    }
                }
                index++;
            }
            return result;
        }

        private readonly string pathOfFolder;

        public LexemesFilesHandler(string folderPath)
        {
            pathOfFolder = folderPath;
            Languages = GetLanguages(GetPathesToFiles(pathOfFolder));
            Lexemes = GetLexemesFromFiles(GetPathesToFiles(pathOfFolder));
        }

        public void SaveLexemesToFiles()
        {
            var changes = DictionaryOfChangedAndOldValuesInLexemes();
            if (changes.Count != 0)
            {
                string text;
                var pathesForSave = GetPathesToFiles(pathOfFolder);
                foreach (var change in changes)
                {
                    string language = change.Key;
                    text = File.ReadAllText(pathesForSave[Languages.IndexOf(language)]);
                    foreach (var item in change.Value)
                    {
                        text = text.Replace(item.Item1 + '=' + item.Item2, item.Item1 + '=' + item.Item3);
                    }
                    File.WriteAllText(pathesForSave[Languages.IndexOf(language)], text);
                }
            }
        }

        private Dictionary<string, List<Tuple<string, string, string>>> DictionaryOfChangedAndOldValuesInLexemes() //key - language, value - Tuple<lexem_name, oldValue, newValue> 
        {
            var LexemesFromFiles = GetLexemesFromFiles(GetPathesToFiles(pathOfFolder));
            Dictionary<string, List<Tuple<string, string, string>>> result = new Dictionary<string, List<Tuple<string, string, string>>>();
            foreach (var key in LexemesFromFiles.Keys)
            {
                foreach (var value in LexemesFromFiles[key])
                {
                    int index = LexemesFromFiles[key].IndexOf(value);
                    if (value.Value != Lexemes[key][index].Value)
                    {
                        string language = value.Language;
                        Tuple<string, string, string> changedAndOldValue = new Tuple<string, string, string>(key, value.Value, Lexemes[key][index].Value);
                        if (!result.ContainsKey(language))
                        {
                            List<Tuple<string, string, string>> changedAndOldValues = new List<Tuple<string, string, string>> {changedAndOldValue};
                            result.Add(language, changedAndOldValues);
                        }
                        else
                        {
                            List<Tuple<string, string, string>> valuesInResultByLanguage = result[language];
                            valuesInResultByLanguage.Add(changedAndOldValue);
                            result[language] = valuesInResultByLanguage;
                        }
                    }
                }
            }
            return result;
        }

        public bool IsStorageOfLexemesNullOrEmpty()
        {
            if (Lexemes == null) { return true; }
            if (Lexemes.Count == 0) { return true; }
            return false;
        }
    }
}
