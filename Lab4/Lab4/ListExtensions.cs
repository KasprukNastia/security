using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Lab4
{
    public static class ListExtensions
    {
        public static List<T> Shuffle<T>(this List<T> list)
        {
            Random random = new Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static void WriteListToCsvFile<T>(this IEnumerable<T> records, string filePath)
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            
            csvWriter.Configuration.Delimiter = ";";
            csvWriter.Configuration.HasHeaderRecord = true;
            csvWriter.Configuration.AutoMap<T>();

            csvWriter.WriteHeader<T>();
            csvWriter.WriteRecords(records);

            streamWriter.Flush();

            var result = Encoding.UTF8.GetString(memoryStream.ToArray());

            File.WriteAllText(filePath, result);
        }

        public static void AppendRecordToCsvFile<T>(this T record, string filePath)
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

            csvWriter.Configuration.Delimiter = ";";
            csvWriter.Configuration.HasHeaderRecord = false;
            csvWriter.Configuration.AutoMap<T>();

            csvWriter.WriteRecord(record);

            streamWriter.Flush();

            var result = Encoding.UTF8.GetString(memoryStream.ToArray());

            File.AppendAllText(filePath, result);
        }

        public static void FillListFromCsvFile<T>(this IEnumerable<T> records, string filePath)
        {
            using TextReader reader = new StreamReader(filePath);
            var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            records = csvReader.GetRecords<T>().ToList();
        }
    }
}
