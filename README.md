# Workshop

**Español** · [English](README.en.md)

Caja de herramientas personal de consola para el día a día como programador: elegís la carpeta de un proyecto y corrés herramientas sobre ella.

## Requisitos

- [SDK de .NET 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) (solo para la herramienta de código duplicado, que ejecuta `jscpd` con `npx`)
- [Git](https://git-scm.com) (para ver la rama del proyecto, crear tags y calcular la versión al compilar)

## Cómo se ejecuta

```
dotnet run --project src/Workshop
```

La primera vez, elegí **Agregar proyecto** y pegá la carpeta del proyecto a analizar. La app recuerda el último proyecto usado.

## Instalación

Corriendo desde el repositorio en Windows, elegí **Instalar en esta PC**. Workshop queda en `%LOCALAPPDATA%\Programs\Workshop` y se abre escribiendo `workshop` en una terminal nueva. Para actualizarlo, hacé `git pull`, corré la app desde el repositorio y elegí la opción de nuevo, con `workshop` cerrado.

## Herramientas

| Herramienta | Qué hace |
|---|---|
| **Auditoría CSS** | Lista las clases definidas en los CSS globales (no en los `.razor.css`) que no se usan en ningún `.razor`, `.cs`, `.cshtml`, `.html`, `.js` ni `.ts`. Las clases que se arman por interpolación (`badge-{codigo}`) se marcan para revisar a mano en lugar de darse por muertas. También revisa las variables CSS en todos los `.css` (incluidos los `.razor.css`) y los estilos inline: las que se usan con `var(--x)` sin declararse (separando las que tienen valor de respaldo) y las que se declaran y nadie usa. Las de Bootstrap (`--bs-*`) cuentan como declaradas. |
| **Código duplicado** | Ejecuta [jscpd](https://github.com/kucherenko/jscpd) sobre archivos C#, Razor y CSS, muestra los bloques duplicados más grandes y abre el reporte HTML completo. |
| **Crear tag** | Muestra los commits desde el último tag `vX.Y.Z`, propone la versión siguiente y publica un tag anotado en `origin`. Solo funciona sin cambios pendientes y parado en el último commit de la rama principal. |
| **Conversor Clarion** | Convierte fechas (`82455` ↔ `29/09/2026`), horas (`4599263` ↔ `12:46:32.62`) y fecha con hora entre el formato Clarion y el legible. Detecta el sentido según lo que escribas. |

## Versión

La versión sale del último tag de git con [MinVer](https://github.com/adamralph/minver), así que no se edita ningún archivo. Compilá después de crear el tag y desde un clon con historial completo.

## Tests

```
dotnet test
```

Los tests corren sobre [Microsoft.Testing.Platform](https://aka.ms/mtp-overview) (xUnit v3), activado en `global.json`.

## Cómo agregar una herramienta

Creá una clase en `src/Workshop/Tools/` que implemente `ITool` (`Name`, `Icon`, `Description` y `RunAsync`). Se registra sola al arrancar y aparece en el menú. Puede pedir dependencias como `IAnsiConsole` o `ProjectStore` por constructor.

## Datos

Los proyectos guardados y los reportes generados viven en `%APPDATA%\Workshop`, fuera del repositorio.
