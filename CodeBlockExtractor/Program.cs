using System;
using System.IO;
using ScriptLib;
using System.Collections.Generic;

namespace CodeBlockExtractor
{
    class Program
    {
        // Usage:
        //   cbe [--ignore-relative] [filepath]
        // If no filepath is provided, input is read from standard input.
        static void Main(string[] args)
        {
            bool ignoreRelative = false;
            string? filePath = null;

            // Process command–line arguments (a very basic parser)
            foreach (var arg in args)
            {
                if (arg.Equals("--ignore-relative", StringComparison.OrdinalIgnoreCase))
                    ignoreRelative = true;
                else
                    filePath = arg;
            }

            string inputText;
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                inputText = File.ReadAllText(filePath);
            }
            else
            {
                // Read from standard input (e.g. piped input)
                inputText = Console.In.ReadToEnd();
            }

            List<CodeBlock> blocks = CodeBlockParser.ParseCodeBlocks(inputText);

            if (blocks.Count == 0)
            {
                Console.WriteLine("No code blocks found.");
                return;
            }

            Console.WriteLine($"Found {blocks.Count} code block(s).");

            int blockIndex = 1;
            foreach (var block in blocks)
            {
                // Determine the destination filename.
                string destFile = FileHelper.GetDestinationPath(block.FileName, ignoreRelative);

                // If the block’s filename was not specified, and if this is the first block,
                // we might want to use a default naming scheme.
                if (string.IsNullOrWhiteSpace(block.FileName))
                {
                    destFile = FileHelper.GetNextUnnamedFilePath();
                }

                // Write the file
                File.WriteAllText(destFile, block.Content);
                Console.WriteLine($"Saved code block #{blockIndex} to: {destFile}");

                // Always copy to the central repository.
                FileHelper.CopyToCentralRepo(destFile);
                Console.WriteLine($"Copied to central repo: {FileHelper.GetCentralRepoPath()}");

                blockIndex++;
            }
        }
    }
}

