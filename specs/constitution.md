# Constitución de ScreenHunter

Versión: 1.0.0. Fecha: 2026-10-15. Base para spec 00, autorizada para implementar por petición expresa del usuario.

1. **Especificación antes de implementación.** El comportamiento observable debe estar definido y vinculado a una validación. Las dudas se registran; las decisiones propuestas no equivalen a consentimiento.
2. **Edición sencilla y reversible.** La captura original y las anotaciones se mantienen separadas durante la edición. Los objetos permanecen seleccionables y modificables; exportar aplana una copia, sin destruir el documento de trabajo.
3. **Arquitectura limpia.** Presentación → aplicación → dominio. Infraestructura implementa puertos definidos en el núcleo. El ensamblado de dependencias ocurre en el arranque. Dominio sin dependencias Windows/UI.
4. **DRY con criterio.** Geometría, estilos, validación y reglas se definen una sola vez. Vista previa y exportación comparten reglas de composición. No crear marcos genéricos, repositorios o servicios sin necesidad concreta.
5. **Integración Windows aislada.** Captura, atajos globales, DPI, portapapeles y persistencia son adaptadores. Deben manejar conflictos y errores sin perder el documento.
6. **Portabilidad real.** ZIP autocontenido por arquitectura, descomprimir y ejecutar sin instalar runtimes ni requerir administrador. No confundir portabilidad con un único EXE. Configuración local al paquete como propuesta inicial, con aviso si la carpeta no permite escritura.
7. **Privacidad local.** Sin nube, cuentas, telemetría ni subida automática de capturas. Sin guardado automático de imágenes salvo decisión explícita. Los registros no contienen imágenes, textos de anotaciones ni información sensible de ventanas.
8. **Calidad demostrable.** Pruebas de reglas y pruebas Windows reales para integración. Matriz multimonitor/DPI y validación del ZIP en equipo limpio. Los objetivos de rendimiento deben medirse, no darse por cumplidos.
9. **Alcance controlado.** Sin video, OCR, colaboracion o complementos en la primera version salvo ampliacion aprobada. No prometer captura de contenido protegido ni escritorio seguro.

## Cambios
Interfaz confirmada: WinForms en español e inglés, seleccionable en Ajustes; ajustes de atajo y modo de captura predeterminado. Cambiar idioma no modifica las anotaciones. Plataforma inicial confirmada: Windows 11 x64. Configuración junto al ejecutable, con aviso y sesión temporal si no se puede escribir. Autorización posterior recibida: implementar la especificación de creación.

Modificar mediante propuesta que indique motivo, impacto y documentos afectados. Incrementar versión mayor si cambia un principio incompatible; menor si se añade uno; parche para aclaraciones. Registrar fecha y aprobación real. Ningún documento subordinado puede contradecir esta constitución sin modificarla explícitamente.
