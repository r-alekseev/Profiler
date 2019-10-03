# Profiler.Net
Minimalistic profiling library for .NET

[![Build Status](https://api.travis-ci.com/r-alekseev/Profiler.Net.svg?token=6vyZfrof99dSqe746sJ2&branch=master)](https://travis-ci.com/r-alekseev/Profiler.Net)

## Usage

### Sequential

```csharp
using (provider.Section("section.one"))
{
    // delay 1 ms
}

using (provider.Section("section.two"))
{
    // delay 1 ms
}

using (provider.Section("section.three"))
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
    using (provider.Section("section.{number}", i))
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
using (var section = provider.Section("section"))
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

using (var section = provider.Section("section"))
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