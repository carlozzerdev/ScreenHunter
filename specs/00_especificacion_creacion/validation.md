# Registro de validación

## 2026-10-16 — Corrección de captura negra, 0.1.1
El usuario informa pantalla negra al seleccionar y resultado negro. Riesgo identificado: aceptar el primer AcquireNextFrame aunque solo sea actualización de cursor, sin imagen nueva del escritorio. No se pudo reproducir el fallo visual original con captura real por restricciones del entorno; la causa concreta en su equipo no se da por demostrada.

Corrección: SDR por GDI BitBlt con formato RGB de 24 bits y comprobación de errores; HDR por DXGI FP16 con descarte/liberación de fotogramas sin LastPresentTime/AccumulatedFrames y plazo total de 2,5 segundos. Detección HDR por salida, composición y cursor conservados. No se rechazan imágenes negras por su contenido: un escritorio negro legítimo sigue siendo válido. Un error nativo no abre un selector sobre una imagen fallida ni reemplaza el documento anterior.

Pruebas: compilación sin errores/advertencias y 23 aserciones de núcleo aprobadas. Tres regresiones sintéticas adicionales verifican metadatos de fotograma, colores de la región seleccionada y fondo atenuado no negro. Intento de captura de ventana opaca sintética con nueva ruta SDR: BitBlt devuelve Win32 error 5 (acceso denegado), registrado en artifacts/capture-fix-0.1.1/report.json. No se guardaron imágenes del escritorio del usuario. La validación real en su sesión y HDR sigue pendiente.

Referencias técnicas: [metadatos de fotograma DXGI](https://learn.microsoft.com/windows/win32/api/dxgi1_2/ns-dxgi1_2-dxgi_outdupl_frame_info) y [captura con GDI](https://learn.microsoft.com/en-us/windows/win32/gdi/capturing-an-image). La decisión de separar SDR/HDR es de este proyecto, no una garantía de compatibilidad derivada de las fuentes.

Paquete 0.1.1 verificado: 488 hashes correctos y 28 comprobaciones sintéticas aprobadas desde ZIP extraído en ruta con espacios y runtime local al paquete. Informe artifacts/package-verification-0.1.1.json. Se conserva la 0.1.0 sin sobrescribir sus ajustes ni detener su proceso. Para probar el cambio, salir de la versión anterior mediante Salir y ejecutar la nueva carpeta. La prueba sintética no demuestra todavía que el fallo visual se haya resuelto en el equipo del usuario.

## 2026-10-16 — Implementación 0.1.0 y estado actual

El usuario autoriza «implementa la aplicación con la especificación de creación». Implementados los cuatro proyectos C# y el programa de pruebas; no se han instalado dependencias ni modificado componentes globales.

### Resultados comprobados
- ZIP final comprobado con `tools/verify_package.py`: extracción en ruta con espacios, hashes de 488 archivos correctos y las 25 comprobaciones sintéticas del ejecutable extraído aprobadas. Evidencia en artifacts/package-verification.json y artifacts/zip-verification/. Cada reconstrucción actualiza SHA256 y debe volver a verificarse.
- `dotnet build ScreenHunter.sln -c Release`: 0 errores, 0 advertencias.
- Programa `tests/ScreenHunter.Tests`: 23 aserciones aprobadas sobre historial, estado guardado, cancelación, grupos, orden, duplicación, numeración, selección, giro y validación de datos.
- `ScreenHunter.exe --self-test`: 25 comprobaciones aprobadas; nueve tipos por gesto de lienzo, proyecto exacto con grupos/rotación, importación/exportación, recorte, preferencias, recursos español/inglés, cancelación de ajustes y diseño de ventana.
- Evidencia sintética en `artifacts/final-verification`: report.json, performance.json, editor-es/en.png, settings-en.png, objects.png, sample.shproj y export.jpg. Ninguna imagen del escritorio del usuario guardada como evidencia.
- Composición y codificación PNG de imagen SDR 3840×2160 con 100 textos, diez ejecuciones: mediana 197,4 ms; máximo 236,5 ms en esta ejecución. CPU Intel64 Family 6 Model 190 Stepping 0; memoria disponible para GC ~16,9 GB. No incluye disco, captura ni portapapeles. No equivale a aprobar A17 completo.
- Paquete offline autocontenido de .NET 10.0.12: arranque y self-test con DOTNET_ROOT inexistente y búsqueda multinivel desactivada. Traza de host en artifacts/portable-host-trace.txt confirma DLL de host y CoreCLR locales al paquete. No sustituye Windows limpio en otro equipo.

### Fallos y límites preservados
- `--self-test --capture-test` intentó solo la región interior de una ventana opaca sintética propia. `IDXGIOutput5::DuplicateOutput1` devolvió E_ACCESSDENIED (0x80070005). Informe `artifacts/capture-verification/report.json`; no se superó ni se marcó aprobada la captura. No se solicitó elevación ni se intentó sortear restricciones de escritorio.
- La publicación habitual autocontenida mediante `dotnet publish` no pudo descargar paquetes de runtime de NuGet (NU1301, sockets no permitidos). Se creó `tools/package_portable.py` que usa los binarios Microsoft ya instalados, copia sus licencias y combina manifiestos; probado su arranque local sin resolución al runtime instalado.
- Falta aceptación real de captura, retardo, cursor, monitores múltiples y DPI mixtos. HDR tiene ruta FP16 y conversión implementadas, pero solo pruebas numéricas; falta hardware HDR y referencia visual. No declarar A01/A02/A15/A25/A26/A27 superados.
- Captura de ventana usa su rectángulo visible del escritorio; no aísla contenido detrás de otras ventanas. Esta limitación de la versión está documentada en guía y plan.
- Portapapeles implementado con bitmap/PNG y persistencia al salir; pegado externo tras cierre todavía no comprobado.
- V01/V02/V03 siguen pendientes en su alcance de aceptación final/equipo limpio/recorrido completo. El ZIP es una primera versión para evaluación.

### Correcciones durante la verificación
Corregidos avisos de serialización de propiedades WinForms, stackalloc en bucle, resolución de recursos, giro individual de bocadillo respecto a su centro y cambio de posición de selección múltiple sin colapsar los miembros. Dos aserciones iniciales del programa de pruebas asumían marco sin borde y selección persistente tras redo: corregidas para comprobar el contrato real; los informes fallidos anteriores permanecen en artifacts/.

Las entradas siguientes son historial de la fase documental anterior, no el estado actual de autorización o implementación.

## 2026-10-15 — Preparación documental
- Entorno inspeccionado: carpeta inicialmente vacía; Visual Studio Community 2026 18.9.3 y SDK .NET 10.0.401.
- Documentación creada: AGENTS, constitución, flujo y especificación inicial con investigación, plan, modelo, contratos, tareas, aceptación y checklist.
- No se ha creado código ni se han instalado componentes.
- No se han ejecutado pruebas funcionales, compilación, publicación ni mediciones. Todos los casos de aceptación están pendientes.
- Aprobación de requisitos propuestos e implementación: pendiente.

## Registro futuro
### Agrupación — 2026-10-15
Agrupar/desagrupar confirmado. Añadidos F25, I18, A37–A39, pertenencia persistida y reglas de orden por bloque. Documentados límites de diseño inicial (sin anidamiento ni edición interna) y guía actualizada. Sin código ni pruebas ejecutadas.

### Rotación — 2026-10-15
Rotación confirmada; agrupación pendiente. Añadidos F24, I17 y A35–A36, transformaciones en modelo/plan y guía. Sin código ni pruebas ejecutadas.

### Selección múltiple — 2026-10-15
Confirmada por el usuario. Actualizados F08, catálogo, modelo, plan, guía, tarea I16 y aceptación A32–A34. Agrupación y rotación no confirmadas. Cambios exclusivamente documentales, sin pruebas ejecutadas.

### Confirmación de objetos adicionales — 2026-10-15
Línea, resaltador y marcador numerado confirmados. Actualizados catálogo, requisitos F21–F23, modelo, tarea I15, aceptación A29–A31 y guía. Imagen insertada y manipulación avanzada permanecen sin aprobar. No se han ejecutado pruebas ni escrito código.

### Definición de objetos — 2026-10-15
Creado objects.md: seis tipos base, propiedades y gestos propuestos, tipos adicionales opcionales y revisión pendiente de selección múltiple/agrupación/rotación. Sin cambios de alcance implícitos y sin código ni pruebas ejecutadas.

### Confirmación final de extras — 2026-10-15
El usuario confirma «9 todo si»: importación de imágenes, retardo, cursor opcional y HDR incluidos ahora. Añadidos F17–F20, tareas I12–I14 y pruebas A24–A28. El contrato HDR→SDR se documenta como diseño técnico inicial revisable, no como una respuesta literal del usuario sobre formatos HDR. No se ha implementado ni probado ninguna función. Autorización de código pendiente.

Añadir por sesión versión/commit, entorno, casos ejecutados, resultados reales, evidencia, defectos y riesgos restantes. No sustituir resultados fallidos por resultados posteriores: conservar el historial.

## 2026-10-15 — Confirmación del alcance y ajustes
- Confirmados editor completo, proyectos editables, tres modos de captura, multimonitor, bandeja, atajo configurable, x64 y preferencias portables.
- Añadidos español/inglés y pantalla Ajustes para idioma, atajo y modo predeterminado; trazabilidad F16 y casos A21–A23.
- Actualizados guía, modelo, contratos, plan, tareas y aclaraciones. Q09 sigue pendiente; no se ha autorizado ni escrito código.

## 2026-10-15 — Cambio de tecnología y guía de uso
- El usuario elige WinForms en lugar de WPF. Actualizados arquitectura MVP, lienzo dibujado, coordenadas de vista y restricciones de dependencia.
- Añadidos requisitos N06/N07, tareas y aceptación para tecnología y documentación.
- Creados README raíz y guía prevista en `docs/guia_de_uso.md`; pendiente de verificar contra una aplicación implementada.
- Sin código, instalaciones ni pruebas funcionales. El resto de aclaraciones sigue pendiente.
