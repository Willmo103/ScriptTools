# ScriptTools

This repository contains multiple command–line tools:

- **CodeBlockExtractor**: Extracts code blocks from input files.
  - **Usage:**
    cat file.md | dotnet run --project CodeBlockExtractor
    or
    dotnet run --project CodeBlockExtractor file.md

- **ThinkExtractor**: Extracts text between <think>...</think> tags.
  - **Usage:**
    cat file.txt | dotnet run --project ThinkExtractor
    or
    dotnet run --project ThinkExtractor file.txt

- **ClipWatcher**: Monitors the clipboard and creates/updates files.
  - **Usage:**
    dotnet run --project ClipWatcher

## Workflow

- Automatic tagging and releases are set up.
- A GitHub Actions workflow is configured to build, test, and (in future) release on pushes to the **main** branch.
