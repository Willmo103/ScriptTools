using System;
using System.IO;
using ScriptLib;
using System.Collections.Generic;

namespace ThinkExtractor
{
    class Program
    {
        // Usage:
        //   thinker [inputFile]
        // Reads input text from file or standard input, extracts <think>...</think> sections,
        // and saves each extracted section to a file in the current directory.
        static void Main(string[] args)
        {
            string inputText;
            if (args.Length > 0 && File.Exists(args[0]))
            {
                inputText = File.ReadAllText(args[0]);
            }
            else
            {
                Console.WriteLine("Reading input from standard input. (End with Ctrl+Z [Windows] or Ctrl+D [Unix])");
                inputText = Console.In.ReadToEnd();
            }

            var (modifiedText, thinks) = ThinkExtractorHelper.ExtractThinkTags(inputText);

            if (thinks.Count == 0)
            {
                Console.WriteLine("No <think> sections found.");
                return;
            }

            Console.WriteLine($"Extracted {thinks.Count} <think> section(s).");
            int count = 1;
            foreach (var thinkText in thinks)
            {
                string fileName = Path.Combine(Directory.GetCurrentDirectory(), $"think-{count:D2}.txt");
                File.WriteAllText(fileName, thinkText);
                Console.WriteLine($"Saved <think> section #{count} to: {fileName}");
                count++;
            }
        }
    }
}

