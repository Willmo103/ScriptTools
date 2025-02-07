using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ScriptLib
{
    // Represents an extracted code block.
    public class CodeBlock
    {
        // May be null if no filename was specified
        public string? FileName { get; set; }
        public string Content { get; set; } = "";
    }

    public static class CodeBlockParser
    {
        /// <summary>
        /// Parses the given input text and returns a list of code blocks.
        /// Looks for code blocks delimited by lines that start with "```".
        /// The filename is determined either from the line immediately preceding the delimiter
        /// or from a comment (e.g. "// filename: example.cs") at the start of the code block.
        /// </summary>
        public static List<CodeBlock> ParseCodeBlocks(string input)
        {
            var blocks = new List<CodeBlock>();
            var lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            CodeBlock? currentBlock = null;
            bool inBlock = false;
            string? lastNonEmptyLine = null;
            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("```"))
                {
                    if (!inBlock)
                    {
                        // Starting a code block
                        inBlock = true;
                        currentBlock = new CodeBlock();

                        // Try to pick up a filename from the previous nonempty line.
                        if (!string.IsNullOrWhiteSpace(lastNonEmptyLine))
                        {
                            // For example, if the line is: "filename: path/to/file.cs" or "# filename: file.cs"
                            var m = Regex.Match(lastNonEmptyLine, @"filename\s*:\s*(.+)", RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                currentBlock.FileName = m.Groups[1].Value.Trim();
                            }
                        }
                    }
                    else
                    {
                        // Ending a code block
                        inBlock = false;
                        // If the code block’s first line is a comment with filename, override it.
                        var contentLines = currentBlock.Content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                        if (contentLines.Length > 0)
                        {
                            var firstLine = contentLines[0].Trim();
                            var m = Regex.Match(firstLine, @"^(//|#)\s*filename\s*:\s*(.+)$", RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                currentBlock.FileName = m.Groups[2].Value.Trim();
                                // Remove that comment line from content.
                                currentBlock.Content = string.Join(Environment.NewLine, contentLines, 1, contentLines.Length - 1);
                            }
                        }
                        blocks.Add(currentBlock);
                        currentBlock = null;
                    }
                    continue;
                }

                if (inBlock && currentBlock != null)
                {
                    currentBlock.Content += line + Environment.NewLine;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        lastNonEmptyLine = line;
                }
            }
            return blocks;
        }
    }

    public static class FileHelper
    {
        // Determines the “central repository” folder.
        // On Windows, this would be "C:\Users\<User>\.script_kb\llm_code"
        // On Linux/macOS, we use $HOME/.script_kb/llm_code.
        public static string GetCentralRepoPath()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string repo = Path.Combine(home, ".script_kb", "llm_code");
            if (!Directory.Exists(repo))
                Directory.CreateDirectory(repo);
            return repo;
        }

        /// <summary>
        /// Given an optional relative or absolute filename, returns the full path to use.
        /// If ignoreRelative is true, then any relative path is interpreted relative to the current execution folder.
        /// If filename is null or empty, a default name is generated using the current date/time and a running index.
        /// </summary>
        public static string GetDestinationPath(string? fileName, bool ignoreRelative = false)
        {
            string destDir = Directory.GetCurrentDirectory();

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                // If ignoreRelative is set, then use the execution root for the file
                if (ignoreRelative || !Path.IsPathRooted(fileName))
                {
                    fileName = Path.GetFileName(fileName);
                    return Path.Combine(destDir, fileName);
                }
                else
                {
                    return fileName;
                }
            }
            else
            {
                // Generate a default file name.
                // For example, "2025-02-06-001.txt"
                string datePart = DateTime.Now.ToString("yyyy-MM-dd");
                int index = 1;
                string candidate;
                do
                {
                    candidate = Path.Combine(destDir, $"{datePart}-{index:D3}.txt");
                    index++;
                } while (File.Exists(candidate));
                return candidate;
            }
        }

        /// <summary>
        /// Copies the file at the given path to the central repository.
        /// </summary>
        public static void CopyToCentralRepo(string filePath)
        {
            string centralRepo = GetCentralRepoPath();
            string fileName = Path.GetFileName(filePath);
            string destPath = Path.Combine(centralRepo, fileName);
            File.Copy(filePath, destPath, overwrite: true);
        }

        /// <summary>
        /// Returns the next unnamed file name in the current directory in the pattern "unnamed-file-{i}".
        /// </summary>
        public static string GetNextUnnamedFilePath()
        {
            string currentDir = Directory.GetCurrentDirectory();
            int i = 1;
            string candidate;
            do
            {
                candidate = Path.Combine(currentDir, $"unnamed-file-{i}.txt");
                i++;
            } while (File.Exists(candidate));
            return candidate;
        }
    }

    public static class ThinkExtractorHelper
    {
        /// <summary>
        /// Extracts and removes text enclosed within <think>...</think> tags from the given input.
        /// Returns a tuple: (modifiedText, list of extracted think texts).
        /// </summary>
        public static (string modifiedText, List<string> thinks) ExtractThinkTags(string input)
        {
            var thinks = new List<string>();
            // This regex captures text between <think> and </think> (dot matches newline).
            string pattern = @"<think>(.*?)<\/think>";
            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var modifiedText = regex.Replace(input, m =>
            {
                thinks.Add(m.Groups[1].Value.Trim());
                return ""; // remove the think section from the text
            });
            return (modifiedText, thinks);
        }
    }
}

