# Workshop

**Español** · [English](README.en.md)

Caja de herramientas personal de consola para el día a día como programador: elegís la carpeta de un proyecto y corrés herramientas de análisis sobre ella.

## Requisitos

- [SDK de .NET 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) (solo para la herramienta de código duplicado, que ejecuta `jscpd` con `npx`)

## Cómo se ejecuta

```
dotnet run --project src/Workshop
```

La primera vez, elegí **Agregar proyecto** y pegá la carpeta del proyecto a analizar. La app recuerda el último proyecto usado.

## Herramientas

| Herramienta | Qué hace |
|---|---|
| **CSS muerto** | Lista las clases definidas en los CSS globales (no en los `.razor.css`) que no se usan en ningún `.razor`, `.cs`, `.cshtml`, `.html`, `.js` ni `.ts`. Las clases que se arman por interpolación (`badge-{codigo}`) se marcan para revisar a mano en lugar de darse por muertas. |
| **Código duplicado** | Ejecuta [jscpd](https://github.com/kucherenko/jscpd) sobre archivos C#, Razor y CSS, muestra los bloques duplicados más grandes y abre el reporte HTML completo. |

## Tests

```
dotnet test
```

Los tests corren sobre [Microsoft.Testing.Platform](https://aka.ms/mtp-overview) (xUnit v3), activado en `global.json`.

## Cómo agregar una herramienta

Creá una clase en `src/Workshop/Tools/` que implemente `ITool`. Se registra sola al arrancar y aparece en el menú. Puede pedir dependencias como `IAnsiConsole` o `ProjectStore` por constructor.

## Datos

Los proyectos guardados y los reportes generados viven en `%APPDATA%\Workshop`, fuera del repositorio.
