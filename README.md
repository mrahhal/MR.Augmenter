# MR.Augmenter

AppVeyor | Travis
---------|-------
[![Build status](https://img.shields.io/appveyor/ci/mrahhal/mr-augmenter/master.svg)](https://ci.appveyor.com/project/mrahhal/mr-augmenter) | [![Travis](https://img.shields.io/travis/mrahhal/MR.Augmenter.svg)](https://travis-ci.org/mrahhal/MR.Augmenter)

[![NuGet version](https://img.shields.io/nuget/v/MR.Augmenter.svg)](https://www.nuget.org/packages/MR.Augmenter)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)

Take control of the data your API returns.

## What and Why
- We have some changes (add props, remove props) we want to apply to our models centrally (and conditionally).
- We want this to play nice with inheritance, nesting, enumerables, ...
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

    // Suppose we need this in our action, but we want to hide it in our response.
    public string Secret { get; set; }

    // Also, we want to add computed "Image" and "ImageThumb" properties.
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

You'll want to add the `MR.Augmenter.AspNetCore` package to your dependencies (which depends on `MR.Augmenter`).

Add Augmenter and configure global options:

```cs
services.AddAugmenter(config => { ... });

// This will add a global filter that will handle augmenting models you return from actions.
services.AddAugmenterForMvc();
```

From here on out, simply do what you always do. Augmenter will start working automatically with the models you return.

Inheritance, nested types, anonymous objects containing configured models, lists and arrays... Those are all accounted for.

## Configuration

```cs
services.AddAugmenter(config =>
{
    // Start configuring the type "Model1".
    config.Configure<Model1>(c =>
    {
        // Use ConfigureRemove to configure a "Remove" agumentation.
        // From now on, the "Secret" property will always be removed from the response.
        c.ConfigureRemove(nameof(Model1.Secret));

        // Use ConfigureAdd to configure an "Add" augmentation.
        // From now on, the "Image" property will always be added to the response.
        c.ConfigureAdd("Image", (x, state) => $"/{x.Hash}/some/path");
    });
});
```

For a lot more options checkout the samples.

## Advanced

[TODO]

# Samples
Check out the [`samples`](samples) under "samples/" for more practical use cases.

#### [`Basic`](samples/Basic)
Shows how to configure Augmenter in an Asp.Net core app with some practical examples.
