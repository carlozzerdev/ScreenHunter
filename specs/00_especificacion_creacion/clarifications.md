# Aclaraciones y decisiones pendientes

## Autorización de implementación
El usuario solicita «implementa la aplicación con la especificación de creación». Autoriza implementar el alcance documentado. Versión 0.1.0 creada; validación parcial y límites reales en validation.md. Las notas anteriores que indiquen «sin autorización» son historial previo, no el estado actual.

## Revisión actual: catálogo de objetos
Ver [objects.md](objects.md). Confirmados línea, resaltador, marcador numerado, selección múltiple, rotación y agrupación. Imagen insertada como objeto sigue sin aprobarse. Los gestos y transformaciones se detallan como diseño revisable.

2026-10-15: el usuario confirma «que se pueda agrupar». Añadido F25, persistencia de grupos y acciones Agrupar/Desagrupar. Diseño inicial revisable: sin anidamiento ni edición interna; desagrupar para editar un miembro. Los grupos ocupan bloques contiguos de superposición; documentado efecto de agrupar objetos intercalados. No autoriza implementación.

2026-10-15: el usuario confirma rotación y pregunta por agrupación. Se explica: selección múltiple temporal frente a grupo persistente seleccionable como unidad. No interpretar la pregunta como aprobación de agrupar. Añadido F24; giro individual y conjunto, con pivotes y gestos como decisiones iniciales revisables.

2026-10-15: el usuario solicita selección múltiple. Se incluye Shift+clic, marco y acciones conjuntas; redimensionado proporcional y reglas de propiedades mixtas como diseño inicial. No implica aprobar agrupación o rotación ni autorizar código.

2026-10-15: el usuario confirma «línea, resaltador y marcador numerado ok». Añadidos F21–F23 y aceptación A29–A31. Las reglas de numeración, valores iniciales y gestos detallados son propuestas de diseño revisables, no respuestas literales del usuario. Sin autorización para código.

El alcance principal está confirmado. La implementación sigue sin autorizarse. La tabla siguiente sustituye las propuestas iniciales por las decisiones recibidas.

| ID | Pregunta | Propuesta de partida | Impacto |
|---|---|---|---|
| Q02 | Primera versión | Confirmado: completa con deshacer/rehacer, zoom y recorte | Alcance |
| Q03 | Persistencia | Confirmado: proyectos editables y PNG/JPEG | Guardado y avisos |
| Q04 | Captura | Confirmado: región, ventana, monitor y multimonitor | Selector |
| Q05 | Ciclo de vida | Confirmado: bandeja y Ctrl+Alt+S configurable en Ajustes | Integración |
| Q06 | Plataforma | Confirmado: Windows 11 x64; ARM64 posterior | Distribución |
| Q07 | Preferencias | Confirmado: junto al EXE; aviso y sesión temporal si no hay escritura | Portabilidad |
| Q08 | Idioma y nombre | Confirmado: ScreenHunter, español e inglés elegibles en Ajustes; también atajo y modo predeterminado | Localización |
| Q09 | Extras | Confirmado: abrir imágenes existentes, captura retardada, cursor opcional y HDR en primera versión | F17–F20 |

## Respuestas
2026-10-15: el usuario indica «evita usar WPF, prefiero Winforms» y solicita documentación de uso. Decisión: WinForms; actualizar arquitectura a MVP y crear `docs/guia_de_uso.md`. No implica autorización para implementar ni confirma las restantes propuestas.

2026-10-15: confirmados los puntos 2–7 y ampliado el 8 a interfaz bilingüe y ajustes de idioma, atajo y modo predeterminado. Requisitos afectados: F07, F12–F16, N01 y N03. Q09 permanece pendiente.

Decisiones de diseño derivadas, revisables: idioma inicial español; modo inicial región; cambio de idioma al aceptar sin reiniciar ni modificar anotaciones; Cancelar Ajustes descarta cambios. La versión mínima concreta de Windows 11 y límites de memoria/historial se documentarán durante la validación técnica.

2026-10-15: el usuario responde «9 todo si». Se confirman los cuatro extras, incluido soporte HDR ahora; queda sustituida la propuesta de aplazarlo. No quedan preguntas funcionales previas sin responder. No implica autorización de implementación.

Diseño técnico inicial revisable para HDR: captura HDR con conversión tonal a SDR sRGB en editor y salidas existentes. El usuario ha aprobado HDR sin precisar formato de salida: esta interpretación no equivale a confirmación de excluir conservación/exportación HDR nativa. Se hace explícita en F20 para revisión. Importación inicial PNG/JPEG; retardo 0/3/5/10 s y cursor desactivado por defecto son decisiones de diseño, no respuestas literales del usuario.
