using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        // Update the paths with your specified locations
        string inputCsvPath = "";/* file location ; */
        string outputCsvPath = "";/* file location ; */

        if (!File.Exists(inputCsvPath))
        {
            Console.WriteLine($"Error: The file '{inputCsvPath}' does not exist.");
            return;
        }

        var shiftData = new Dictionary<string, List<Shift>>();

        using (var reader = new StreamReader(inputCsvPath))
        {
            // Read and skip the header row
            var header = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                var row = reader.ReadLine().Split(',');

                string helper = row[0];
                string weekCommencing = row[1];
                string onDay = row[2];
                string interval = row[3];
                string startTime = row[4];
                string endTime = row[5];

                // Skip rows with invalid data
                if (startTime == "Required" || endTime == "Required")
                {
                    continue;
                }

                DateTime start, end;
                if (!DateTime.TryParseExact(startTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out start) ||
                    !DateTime.TryParseExact(endTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out end))
                {
                    Console.WriteLine($"Warning: Skipping invalid time data for helper {helper} on {onDay}, {weekCommencing}");
                    continue;
                }

                string key = $"{helper}-{weekCommencing}-{onDay}";

                if (!shiftData.ContainsKey(key))
                {
                    shiftData[key] = new List<Shift>();
                }

                shiftData[key].Add(new Shift
                {
                    StartTime = start,
                    EndTime = end
                });
            }
        }

        var outputData = new List<string> { "Helper,WeekCommencing,Day,StartTime,EndTime" };

        foreach (var entry in shiftData)
        {
            var keyParts = entry.Key.Split('-');
            string helper = keyParts[0];
            string weekCommencing = keyParts[1];
            string day = keyParts[2];

            var shifts = entry.Value;
            var earliestStartTime = shifts.Min(s => s.StartTime);
            var latestEndTime = shifts.Max(s => s.EndTime);

            outputData.Add($"{helper},{weekCommencing},{day},{earliestStartTime:HH:mm},{latestEndTime:HH:mm}");
        }

        File.WriteAllLines(outputCsvPath, outputData);
        Console.WriteLine("Shifts successfully calculated and written to combo_output.csv");
    }

    class Shift
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
