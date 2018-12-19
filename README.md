# Profiler.Net
Minimalistic profiling library for .NET

## Usage

### Sequential

```csharp
using (provider.Section("section.one"))
{
}

using (provider.Section("section.two"))
{
}

using (provider.Section("section.three"))
{
}

// log:
//  section.one
//  section.two
//  section.three

// metrics:
//  section.one     : 1
//  section.two     : 1
//  section.three   : 1
```

### Repeating

```csharp
for (int i = 0; i < 3; i++)
{
    using (provider.Section("section.{0}", i))
    {
    }
}

// log:
//  section.0
//  section.1
//  section.2

// metrics:
//  section.{0}     : 3
```

### Childs

```csharp
using (var section = provider.Section("section.one"))
{
    using (section.Section("child.one"))
    {
    }
}

// log:
//  section.one -> child.one
//  section.one

// metrics:
//  section.one                 : 1
//  section.one -> child.one    : 1
```

### Passing Childs

```csharp
void Inner(ISection section, int i)
{
    using (section.Section("child.{0}", i))
    {
    }
}

using (var section = provider.Section("section.one"))
{
    Inner(section, 0);
    Inner(section, 1);
    Inner(section, 2);
}

// log:
//  section.one -> child.0
//  section.one -> child.1
//  section.one -> child.2
//  section.one

// metrics:
//  section.one                 : 1
//  section.one -> child.{0}    : 3
```