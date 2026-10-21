# Flujo de especificaciones

Este es un flujo documental inspirado en [GitHub Spec Kit](https://github.com/github/spec-kit), no una instalación de su CLI ni una réplica de sus rutas internas. La constitución se conserva en la ruta solicitada `specs/constitution.md`.

## Orden de trabajo
1. Constitución: principios y restricciones comunes.
2. Spec: qué necesita el usuario, escenarios, alcance y aceptación; sin imponer detalles técnicos.
3. Aclaración: resolver preguntas que afecten arquitectura o aceptación.
4. Research y plan: alternativas, decisiones, contratos y riesgos.
5. Tests y checklist: verificar que los requisitos son completos y comprobables.
6. Tasks: trabajo ordenado, con dependencias y trazabilidad.
7. Revisión cruzada: detectar requisitos sin tarea o prueba, contradicciones y decisiones abiertas.
8. Implementación: únicamente cuando el usuario la autorice; después validar y documentar resultados.

Estados: borrador → listo para revisión → aprobado para implementar → en implementación → validado. El agente puede preparar borradores autónomamente; la autorización de implementar debe constar expresamente.

## Archivos por especificación
- `spec.md`: requisitos y alcance.
- `clarifications.md`: preguntas, respuestas y supuestos.
- `research.md`: investigación y fuentes.
- `plan.md`: arquitectura y estrategia técnica.
- `data-model.md`: entidades e invariantes cuando proceda.
- `contracts.md`: límites entre componentes; no implica una API web.
- `tasks.md`: lista ejecutable y dependencias.
- `tests/acceptance.md`: escenarios y matriz de trazabilidad.
- `checklists/readiness.md`: puerta de preparación.
- `validation.md`: resultados reales y pendientes.

Para una nueva especificación copiar la estructura de la inicial, vaciar resultados, asignar un número correlativo y citar las especificaciones afectadas. IDs de requisito: `SH00-Fxx` y `SH00-Nxx`; nuevas carpetas usan su número para evitar colisiones.

## Cómo dirigir a Codex
Ahora: «Revisa AGENTS.md y la spec 00; aclara las preguntas y actualiza los documentos, sin código».
Cuando esté revisada: «Apruebo la spec 00 y autorizo su implementación siguiendo tasks.md».
Más adelante: «Crea la spec 01 para [funcionalidad], analiza el impacto y no implementes todavía».

## Ampliacion 01
`01_bocadillo_sombra/`: cambio solicitado el 20/10/2026; sustituye punta de SH00-F03 y amplia estilo de SH00-F05. Implementacion 0.1.2.
