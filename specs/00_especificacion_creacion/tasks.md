# Tareas propuestas

Implementación autorizada por el usuario. Versión 0.1.0 disponible; validación parcial. Casillas I marcadas indican código creado y compilado, no que toda su aceptación de hardware esté superada. Ver validation.md. D = documentación; I = implementación; V = validación final.

- [x] D01 Confirmar primera versión completa y proyectos editables; ajustar alcance.
- [x] D02 Confirmar extras Q09 y actualizar alcance, plan, modelo y contratos. Límites técnicos por medir antes de entrega.
- [x] D03 Revisar trazabilidad y readiness; registrar autorización explícita de implementar. Depende de D02.
- [ ] I01 Verificar herramientas y prueba acotada de captura, DPI, atajo y ZIP. Registrar elección de motor y restricciones. Depende de D03. SH00-F01/F07/F14, N01/N04.
- [x] I02 Crear estructura con dirección de dependencias y pruebas del núcleo. SH00-N02. I01 solo parcialmente verificable; se continuó con núcleo/editor independientes de captura para avanzar con evidencia.
- [x] I02b Crear presentación WinForms con MVP y lienzo; verificar separación de formularios y funcionalidad, sin dependencias WPF. Depende de I02. SH00-N06.
- [x] I03 Documento, geometría, estilos y transformaciones. Depende de I02. SH00-F02–F05, N04.
- [x] I04 Historial y operaciones reversibles. Depende de I03. SH00-F08–F10.
- [x] I05 Selector y captura por botón/atajo con cancelación y errores. Depende de I01/I02. SH00-F01/F07/F14. Código creado; captura real bloqueada por E_ACCESSDENIED.
- [x] I06 Editor de texto, bocadillo, flecha, formas y trazos; selección y propiedades. Depende de I03/I04. SH00-F02–F05/F08, N03.
- [x] I07 Zoom y recorte reversible. Depende de I06. SH00-F10, N04.
- [x] I08 Composición compartida, portapapeles y exportación. Depende de I06. SH00-F06/F11. Pegado externo pendiente.
- [x] I09 Proyecto versionado y avisos de cambios. Depende de I03/I06. SH00-F12/F15.
- [x] I10 Bandeja, preferencias y ciclo de vida. Depende de I05/I09. SH00-F13/F14/F15, N05.
- [x] I11 Ajustes de idioma es/en, atajo y modo predeterminado; persistencia y localización completa de interfaz propia. Depende de I05/I10. SH00-F07/F14/F16, N03.
- [x] I12 Importar PNG/JPEG con validación y avisos de cambios pendientes. Depende de I03/I09. SH00-F17.
- [x] I13 Retardo cancelable y cursor opcional, con preferencias persistidas y captura al finalizar la espera. Depende de I05/I11. SH00-F18/F19. Aceptación de captura pendiente.
- [ ] I14 Validar captura HDR, conversión tonal y monitores mixtos; documentar algoritmo y contrato de salida. Depende de I01/I03/I08. SH00-F20. Incluir el experimento HDR en I01 antes de consolidar el motor.
- [x] I15 Línea, resaltador y marcador numerado: creación, selección, propiedades, historial, numeración y persistencia; reutilizar geometría y estilos existentes. Depende de I03/I04/I06/I08/I09. SH00-F21–F23.
- [x] I16 Selección múltiple Shift+clic y marco, propiedades mixtas, movimiento/tamaño/duplicado/borrado/orden conjuntos e historial atómico. Depende de I03/I04/I06/I15. SH00-F08.
- [x] I17 Rotación individual y múltiple, manejador y grados numéricos, pivotes, selección transformada, historial, persistencia y exportación. Depende de I03/I04/I08/I09/I16. SH00-F24.
- [x] I18 Agrupar/desagrupar, selección por unidad, propiedades compatibles, orden por bloque, duplicación y persistencia validada; reutilizar transformaciones e historial. Depende de I09/I15/I16/I17. SH00-F25.
- [ ] V01 Pruebas de aceptación, errores, privacidad, DPI, rendimiento, fidelidad, ambos idiomas y extras; registrar resultados. Depende de I07–I18. Todos los requisitos aplicables.
- [ ] V02 Publicar ZIP, validar en equipo limpio y documentar uso/licencias/limitaciones. Depende de V01. SH00-N01/N03.
- [ ] V03 Recorrer la guía `docs/guia_de_uso.md` con la aplicación entregable; corregir nombres, atajos y funciones, e incluirla en el ZIP. Depende de V02. SH00-N07.
- [x] FIX01 Captura negra: ruta SDR GDI RGB verificada por retorno, espera de fotograma válido HDR y regresión de selector. A40. Código compilado; captura real bloqueada por acceso denegado y pendiente en equipo del usuario.

Si se descarta un requisito propuesto, eliminar o reformular las tareas y pruebas relacionadas antes de implementar. No marcar pruebas aprobadas por inspección documental.
