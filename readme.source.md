# <img src="/src/icon.png" height="40px"> EfResultsFilter

Sometimes, in the context of constructing an EF query, it is not possible to know if any given item should be returned in the results. For example when performing authorization where the rules rules are pulled from a different system, and that information does not exist in the database.

This project allows a custom function to be executed after the EF query execution and determine if any given node should be included in the result.


## NuGet [![NuGet Status](http://img.shields.io/nuget/v/EfResultsFilter.svg?longCache=true&style=flat)](https://www.nuget.org/packages/EfResultsFilter/)

https://nuget.org/packages/EfResultsFilter/


## Notes:

 * When evaluated on nodes of a collection, excluded nodes will be removed from collection.
 * When evaluated on a property node, the value will be replaced with null.
 * A [Type.IsAssignableFrom](https://docs.microsoft.com/en-us/dotnet/api/system.type.isassignablefrom) check will be performed to determine if an item instance should be filtered based on the `<TItem>`.
 * Filters are static and hence shared for the current [AppDomain](https://docs.microsoft.com/en-us/dotnet/api/system.appdomain).


### Signature:

snippet: GlobalFiltersSignature


### Usage:

snippet: add-filter


## Icon

[Memory](https://thenounproject.com/term/database/1631008/) designed by H Alberto Gongora from [The Noun Project](https://thenounproject.com)