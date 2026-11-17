# Workshop

Personal console toolbox for day-to-day development: pick a project folder and run analysis tools on it.

## Run

```
dotnet run --project src/Workshop
```

## Test

```
dotnet test
```

## Add a tool

Create a class in `src/Workshop/Tools/` that implements `ITool`. The menu discovers it automatically.
