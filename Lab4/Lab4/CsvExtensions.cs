using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Lab4
{
    public static class CsvExtensions
    {
        public static void WriteListToCsvFile<T>(this IEnumerable<T> records, string filePath)
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

            csvWriter.Configuration.Delimiter = ";";
            csvWriter.Configuration.HasHeaderRecord = true;
            csvWriter.Configuration.AutoMap<T>();

            csvWriter.WriteHeader<T>();
            csvWriter.NextRecord();
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
            csvWriter.Configuration.AutoMap<T>();

            if (string.IsNullOrWhiteSpace(File.ReadAllText(filePath)))
            {
                csvWriter.Configuration.HasHeaderRecord = true;
                csvWriter.WriteHeader<T>();
                csvWriter.NextRecord();
            }
            else
            {
                csvWriter.Configuration.HasHeaderRecord = false;
            }

            csvWriter.WriteRecords(new List<T> { record });

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
