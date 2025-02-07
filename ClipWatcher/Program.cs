using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ScriptLib;
using TextCopy;

namespace ClipWatcher
{
    class Program
    {
        // In this simple example, we poll the clipboard every second.
        // If the clipboard content changes, we process it.
        // If the content looks like a single line (and possibly a filename), we store it as the target file.
        // Otherwise, we treat it as file content.
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting clipboard watcher. Press Ctrl+C to exit.");
            string? lastClipboardText = null;
            // This variable holds a pending filename if one was copied first.
            string? pendingFileName = null;

            // Create an instance of Clipboard (TextCopy's API in your version is instance-based).
            var clipboard = new Clipboard();

            while (true)
            {
                try
                {
                    // Use the clipboard instance to get the text asynchronously.
                    string clipboardText = await clipboard.GetTextAsync();

                    if (clipboardText != lastClipboardText)
                    {
                        lastClipboardText = clipboardText;
                        // Simple heuristic: if the clipboard text is a short line without whitespace, consider it a filename.
                        if (clipboardText.Trim().IndexOfAny(new char[] { ' ', '\t', '\r', '\n' }) == -1)
                        {
                            // Assume it is a filename.
                            pendingFileName = clipboardText.Trim();
                            // “Touch” the file (create it or update its last write time).
                            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), pendingFileName);
                            if (!File.Exists(fullPath))
                            {
                                File.WriteAllText(fullPath, "");
                                Console.WriteLine($"Created file: {fullPath}");
                            }
                            else
                            {
                                File.SetLastWriteTime(fullPath, DateTime.Now);
                                Console.WriteLine($"Updated timestamp of: {fullPath}");
                            }
                        }
                        else
                        {
                            // Clipboard text contains multiple words or newlines; treat it as file content.
                            if (!string.IsNullOrWhiteSpace(pendingFileName))
                            {
                                // Use the pending file name.
                                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), pendingFileName);
                                File.WriteAllText(fullPath, clipboardText);
                                Console.WriteLine($"Wrote clipboard content to: {fullPath}");
                                // Also copy to the central repository.
                                FileHelper.CopyToCentralRepo(fullPath);
                                pendingFileName = null; // clear the pending filename.
                            }
                            else
                            {
                                // No filename was provided; create a new file with an auto–generated name.
                                string fullPath = FileHelper.GetNextUnnamedFilePath();
                                File.WriteAllText(fullPath, clipboardText);
                                Console.WriteLine($"Wrote clipboard content to new file: {fullPath}");
                                FileHelper.CopyToCentralRepo(fullPath);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading clipboard: {ex.Message}");
                }
                // Poll every 1 second.
                Thread.Sleep(1000);
            }
        }
    }
}

