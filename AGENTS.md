# Instrucciones para agentes — ScreenHunter

## Estado y autoridad
- El usuario ha autorizado implementar la spec 00 («implementa la aplicación con la especificación de creación»). Se permite código y pruebas dentro de ese alcance. Nuevas especificaciones requieren su propia autorización de implementación.
- Leer este archivo, `specs/constitution.md`, `specs/README.md` y la especificación activa antes de trabajar.
- Las instrucciones del usuario prevalecen. No considerar aprobada una propuesta por silencio o por estar escrita en estos documentos.
- Especificación activa: `specs/00_especificacion_creacion/`. Estado: implementación 0.1.1 y validación; consultar validation.md para resultados y límites reales.
- Comunicar en español. Mantener identificadores técnicos estables.

## Flujo obligatorio
1. Definir el comportamiento en spec.md con requisitos identificables y criterios verificables.
2. Registrar preguntas, respuestas y supuestos en clarifications.md. No inventar respuestas del usuario.
3. Investigar opciones en research.md y justificar decisiones técnicas en plan.md.
4. Desglosar en tasks.md y vincular requisitos con tests/acceptance.md.
5. Revisar checklists/readiness.md y resolver contradicciones antes de implementar.
6. Tras autorización de implementación, ejecutar tareas y registrar pruebas reales y limitaciones. Una casilla no se marca por intención.
- Para nuevas funcionalidades crear `specs/01_nombre/`, `02_nombre/`, etc. No sobrescribir el alcance histórico: registrar cambios y sustituciones explícitos.
- No ejecutar comandos `/speckit.*` como si estuvieran instalados: este repositorio usa un flujo documental propio inspirado en Spec Kit.

## Arquitectura y calidad
- Tecnología de presentación decidida por el usuario: C# con WinForms. No usar WPF ni controles WPF alojados. Emplear presentadores para separar los formularios de los casos de uso.
- Interfaz en español e inglés; Ajustes permite elegir idioma, atajo y modo de captura predeterminado. Recursos localizados separados; no traducir las anotaciones del usuario.
- Cumplir constitution.md: separación de presentación, casos de uso, dominio e infraestructura; DRY sin abstracciones prematuras.
- El dominio y los casos de uso no dependen de WinForms, Win32, archivos ni portapapeles.
- Modelo de anotaciones como fuente de verdad; no usar controles visuales serializados como documento.
- Evitar lógica de negocio en code-behind. Adaptadores Windows aislados y sustituibles.
- Cambios pequeños y coherentes; pruebas proporcionales al riesgo, especialmente coordenadas, historial, persistencia y composición.
- No capturar ni guardar pantallas del usuario como evidencia sin autorización específica. Usar contenido sintético en validación.
- No subir imágenes, proyectos ni telemetría a servicios externos. No instalar, publicar o modificar el equipo sin necesidad autorizada.
- No declarar compatibilidad, rendimiento o pruebas superadas sin evidencia. No delegar a otros agentes salvo autorización explícita aplicable.

## Entrega futura
- Windows 11, distribución ZIP autocontenida, sin instalador ni privilegios administrativos para uso normal.
- Incluir instrucciones de uso, versión, licencias de terceros y limitaciones verificadas.
- Mantener `docs/guia_de_uso.md` alineada con el comportamiento implementado; distinguir propuestas y funciones disponibles. Incluir la guía en el ZIP final.
- Preservar cambios ajenos y actualizar documentación cuando cambien contratos o comportamiento.

## Ampliacion 01
Peticion del usuario del 20/10/2026 autoriza quitar la punta del bocadillo y ofrecer sombra para todos los objetos. Especificacion: `specs/01_bocadillo_sombra/`; version 0.1.2. Consultar su validation.md.
