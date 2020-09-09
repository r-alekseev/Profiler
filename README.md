# Profiler
Minimalistic and fast profiling library for .NET

[![Build Status](https://api.travis-ci.com/r-alekseev/Profiler.svg?token=6vyZfrof99dSqe746sJ2&branch=master)](https://travis-ci.com/r-alekseev/Profiler)
[![NuGet version (Profiler)](https://img.shields.io/nuget/v/Profiler.svg?style=flat)](https://www.nuget.org/packages/Profiler/)

## Configuration

### Default

Example writing traces and reports to output window like ```Debug.WriteLine```:

```csharp
var profiler = new ProfilerConfiguration()
    .CreateProfiler()
```

### Serilog

see [Profiler.Serilog](https://github.com/r-alekseev/Profiler.Serilog) 

Example writing profiling traces and reports to structured events in Serilog:

```csharp

var profiler = new ProfilerConfiguration()
    .UseSerilogTraceWriter(settings => settings
        .UseLogEventLevel(LogEventLevel.Verbose)
        .UseLogger(logger))
    .UseSerilogReportWriter(settings => settings
        .UseLogEventLevel(LogEventLevel.Information)
        .UseLogger(logger))
    .CreateProfiler();
```

## Usage

### Sequential

```csharp
using (profiler.Section("section.one"))
{
    // delay 1 ms
}

using (profiler.Section("section.two"))
{
    // delay 1 ms
}

using (profiler.Section("section.three"))
{
    // delay 1 ms
}

// trace:
//  section.one     : 1 ms
//  section.two     : 1 ms
//  section.three   : 1 ms

// metrics:
//  section.one     : 1 ms
//  section.two     : 1 ms
//  section.three   : 1 ms
```

### Repeating

```csharp
for (int i = 0; i < 3; i++)
{
    using (profiler.Section("section.{number}", i))
    {
        // delay 1 ms
    }
}

// trace:
//  section.0           : 1 ms
//  section.1           : 1 ms
//  section.2           : 1 ms

// metrics:
//  section.{number}    : 3 ms
```

### Childs

```csharp
using (var section = profiler.Section("section"))
{
    using (section.Section("child"))
    {
        // delay 1 ms
    }

    // delay 1 ms
}

// trace:
//  section -> child        : 1 ms
//  section                 : 2 ms

// metrics:
//  section                 : 2 ms
//  section -> child        : 1 ms
```

### Passing Childs

```csharp
void Inner(ISection section, int i)
{
    using (section.Section("child.{number}", i))
    {
        // delay 1 ms
    }
}

using (var section = profiler.Section("section"))
{
    Inner(section, 0);
    Inner(section, 1);
    Inner(section, 2);

    // delay 1 ms
}

// trace:
//  section -> child.0          : 1 ms
//  section -> child.1          : 1 ms
//  section -> child.2          : 1 ms
//  section                     : 4 ms

// metrics:
//  section                     : 4 ms
//  section -> child.{number}   : 3 ms
```
