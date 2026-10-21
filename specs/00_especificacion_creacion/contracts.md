# Contratos conceptuales

Puertos internos; nombres orientativos, sin firmas de código.

| Puerto | Entrada / salida | Fallos y obligaciones |
|---|---|---|
| Captura | Modo y destino → imagen, tamaño físico, origen y metadatos de escala | Cancelación distinta de fallo; no sustituir documento previo hasta éxito; liberar recursos |
| Opciones de captura | Destino, retardo e inclusión de cursor → captura al vencer espera | Espera cancelable; cursor del instante capturado sin duplicados; validar destino todavía disponible |
| Conversión de color | Superficie SDR/HDR y metadatos por monitor → imagen de trabajo sRGB | No truncar HDR antes de conversión tonal; mismo resultado en editor y salida |
| Importación | Archivo PNG/JPEG → imagen validada y documento nuevo | Carga fallida conserva documento; respetar cambios pendientes y límites de memoria |
| Atajo global | Combinación → registro o conflicto | Desregistrar al salir; no capturas concurrentes; botón disponible si falla |
| Composición | Documento y opciones → imagen rasterizada | Misma geometría/estilos que vista; sin manejadores; tamaño independiente del zoom |
| Portapapeles | Imagen compuesta → éxito/error | Imagen usable después de cerrar ScreenHunter; error recuperable |
| Exportación | Imagen, formato, destino, calidad → resultado | Confirmación de sobrescritura en UX; no marcar proyecto guardado |
| Proyecto | Documento ↔ archivo versionado | Validar contenido; guardar sin corrupción parcial; carga fallida no destruye documento abierto |
| Preferencias | Idioma, atajo y modo predeterminado ↔ almacenamiento local versionado | Cancelar no cambia estado; validar antes de aplicar; atajo ocupado conserva el anterior; fallo de escritura ofrece sesión temporal |
| Historial | Operación de documento ↔ estado anterior/posterior | Una interacción confirmada por paso; edición tras undo invalida redo |
| Agrupación | Selección de unidades ↔ grupo plano y miembros | Operación atómica; conserva identidades y geometría; orden por bloque explícito; undo restaura pertenencia y orden anterior; proyecto conserva grupos |

Cancelación, conflicto, acceso denegado y fallo técnico son resultados distinguibles. Mensajes localizados en presentación; no mostrar excepciones crudas. Operaciones largas permiten mantener la UI responsiva y no aceptar mutaciones concurrentes del documento durante una instantánea inconsistente.
