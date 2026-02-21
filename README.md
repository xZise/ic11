# IC11: Compiler for Stationeers Game IC10 Assembly

> This is a fork of the original repository which may contain patches to be
> upstreamed.

A tool that translates (compiles) a high-level language program to an IC10 assembly for the Stationeers game. 

The language features a C-like syntax and supports basic instructions, including if/then/else, while loops, function calls, and return values.

Use the [Wiki](https://github.com/Raibo/ic11/wiki) for the ic11 language reference.

# Usage in VSCode

Follow instructions in the [vscode extension README](./extension/README.md). 

This also works in Cursor. Use the [examples](./examples/) for context. 

# Usage

`ic11.exe <path> [-w]`  
Provide a path to the source code as a first argument.  
If the path is a file, then this file will be compiled.  
If the path is a directory, then all `*.ic11` files in this directory will be compiled.  
The optional second argument `-w` will write compiled code to new `*.ic10` files next to the sources.  
```bash
ic11.exe source.ic11
ic11.exe ./examples
ic11.exe source.ic11 -w
ic11.exe ./examples -w
```

The compiled code is provided in the Stdout.

# Building

Download/update dependencies:
```bash
dotnet restore
```

Build binaries:
```bash
dotnet build src/ic11/ic11.csproj -c Release --no-restore
```

Build a single exe:
```bash
dotnet publish src/ic11/ic11.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

# Differences to original repository

The following changes are included in here compared to 1.41.0:
* Removing unnecessary casts between `IExpression`, `IStatement` and `INode` ([#22](https://github.com/Raibo/ic11/pull/22))
* Support unbalanced block/statement in if-else ([#18](https://github.com/Raibo/ic11/pull/18))
* Remove jumps to labels without code between them
* Run tests for each example ([#19](https://github.com/Raibo/ic11/pull/19))
* Cleaning up some parts of the code ([#26](https://github.com/Raibo/ic11/pull/26))
* Do not crash for unsupported nodes in visualizer
* On assignment write result of previous node directly into the assigned register
* Use messages to report compiler errors and warnings instead of throwing exceptions
* Add suppport for including other files (see [issue #13](https://github.com/Raibo/ic11/issues/13))
* Reuse logic and grammar for addressing devices differently ([#29](https://github.com/Raibo/ic11/pull/29))
* Add warnings for unreachable code and improve detection of it
* Add warnings for unused variables and devices