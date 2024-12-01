# DllDepAnalyzer

A tool that automatically copies required DLL files to the executable's directory, ensuring the application can run independently of the development environment.

## Features

- Automatically identifies and copies required DLLs for an EXE.
- Ensures the application can run without needing the development environment.
- Simple to use and integrate into your development workflow.

## Requirements

- **dumpbin**: This tool requires [dumpbin](https://github.com/Delphier/dumpbin) to analyze dependencies.  
  Please download and add `dumpbin` to your system's `PATH` environment variable.
- **.NET runtime**
