# MR.Augmenter

[![Build status](https://img.shields.io/appveyor/ci/mrahhal/mr-augmenter/master.svg)](https://ci.appveyor.com/project/mrahhal/mr-augmenter)
[![NuGet version](https://img.shields.io/nuget/v/MR.Augmenter.svg)](https://www.nuget.org/packages/MR.Augmenter)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)

Take control of the data your API returns.

## What and Why
- We have some changes (add props, remove props) we want to apply to our models centrally (and conditionally).
- We want this to play nice with inheritance.
- We want all of this to happen preferably automatically. We should be able to just write:

```cs
return Ok(Service.GetModel());
```

## Example
This is what we'll be able to do after we configure Augmenter:

```cs
class Model
{
    public int Id { get; set; }
    public string Hash { get; set; }
    public string Secret { get; set; }
}
```

```cs
public IActionResult Get()
{
    var model = Service.GetModel();
    return Ok(model);
}
```

Returned json:
```json
{
  "Id": 42,
  "Hash": "80f0aa63b234498a88fe5f9d2522c2a7",
  "Image": "/images/80f0aa63b234498a88fe5f9d2522c2a7.jpg",
  "ImageThumb": "/images/thumbs/80f0aa63b234498a88fe5f9d2522c2a7.jpg"
}
```

## Getting started

- Add Augmenter and configure global options:

```cs
services.AddAugmenter(config => { ... });

// This will add a global filter that will handle augmenting your returned models.
services.AddAugmenterForMvc();
```
