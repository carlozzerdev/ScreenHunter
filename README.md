# ScreenHunter

Aplicación portable para Windows 11 x64 con edición de capturas: nueve herramientas, propiedades, selección múltiple, grupos, rotación, historial y proyectos editables. Versión 0.1.2: corrección de captura negra; validación parcial documentada.

Tecnología: **C# con WinForms**, sin WPF, .NET 10 LTS. Arquitectura en cuatro proyectos, con dominio y casos de uso independientes de Windows.

- [Guía de uso](docs/guia_de_uso.md)
- [English quick guide](docs/user_guide.md)
- [Estado, pruebas y limitaciones](docs/estado_y_validacion.md)
- [Instrucciones para Codex](AGENTS.md)
- [Flujo de especificaciones](specs/README.md)
- [Constitución](specs/constitution.md)
- [Especificación inicial](specs/00_especificacion_creacion/spec.md)
- [Preguntas pendientes](specs/00_especificacion_creacion/clarifications.md)

## Compilar y verificar

Requisitos de desarrollo: Visual Studio 2026 con escritorio .NET o SDK .NET 10.0.401 y pack Windows Desktop. No hace falta C/C++.

```powershell
dotnet build ScreenHunter.sln -c Release
dotnet run --project tests/ScreenHunter.Tests -c Release
& .\src\ScreenHunter.Presentation.WinForms\bin\Release\net10.0-windows\ScreenHunter.exe --self-test --output artifacts\verification
python tools/package_portable.py
```

El empaquetador offline usa los runtimes Microsoft 10.0.12 instalados en `C:\Program Files\dotnet`, copia licencias y combina sus manifiestos de dependencias con el de la aplicación. Produce carpeta y ZIP bajo artifacts/. La alternativa habitual, cuando NuGet esté accesible, es `dotnet publish src/ScreenHunter.Presentation.WinForms -c Release -r win-x64 --self-contained true -o artifacts/publish` y copiar documentación y licencias.

La prueba real de captura está disponible añadiendo `--capture-test` a `--self-test`; solo intenta capturar la zona interior de una ventana sintética propia. Recibió acceso denegado en este entorno y sigue pendiente. No representa un fallo de las pruebas del editor ni una captura verificada. Consultar el registro antes de marcar casos de aceptación.
