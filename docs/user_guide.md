# ScreenHunter 0.1.1 — Quick guide

Windows 11 x64. Extract the entire ZIP and run `ScreenHunter.exe`. The .NET runtime is included; keep all accompanying files. Choose English in **Ajustes → Idioma / Language → English → Aceptar**.

0.1.1 changes SDR capture to checked GDI RGB copying and waits for a real desktop frame on the HDR duplication path. API failures preserve the previous document. Real desktop capture remains blocked by access restrictions in the development environment; synthetic overlay/frame-validity tests pass.

1. **Capture** uses your default mode. Its dropdown chooses Region, Window or Monitor for one capture. The default global shortcut is Ctrl+Alt+S while the application is running. Escape cancels. Window mode captures the visible screen rectangle; overlapping windows can appear.
2. **Settings** changes language, shortcut, default capture mode, delay (0/3/5/10 seconds) and cursor inclusion. Press your new key combination in the shortcut field. Invalid/unavailable shortcuts keep previous settings. If settings cannot be saved next to the executable, a prompt offers session-only changes.
3. **Open image** loads PNG/JPEG as a new document. Use **Open project** for editable `.shproj` files.
4. Select Text, Callout, Arrow, Rectangle, Ellipse, Freehand, Line, Highlighter or Numbered marker on the left. Drag to create shapes; a text dialog opens for text/callouts. Click to place a numbered marker. Shift constrains rectangles/ellipses to squares/circles and lines/arrows to 45° increments.
5. Use **Select** to move objects and their handles to resize. The right panel changes applicable colors, thickness, fill, border, opacity and text properties. Double-click text to edit; ungroup first when it belongs to a group.
6. Shift+click adds/removes objects. Drag a selection frame from an empty area; only completely enclosed objects/whole groups are selected. Shift+frame adds to the selection. Drag the rotation handle or enter degrees; Shift snaps rotation to 15°. A group/multiple selection rotates by an increment around its shared center.
7. **Group / Ungroup** (Ctrl+G / Ctrl+Shift+G) creates/removes persistent groups. Groups can move, scale uniformly and rotate. No nested groups: grouping groups combines their members. Grouping interleaved objects can change overlap with other objects; Undo restores the previous order.
8. Ctrl+mouse wheel zooms. **Fit** and **100%** change the view only. **Crop** limits the exported image without deleting annotations. **Reset crop** restores the whole image.
9. **Copy image** (Ctrl+C with canvas focus) copies the flattened result. **Export image** saves PNG or JPEG; JPEG quality is configurable. **Save project** (Ctrl+S) keeps editable objects, groups and the base image. Copying/exporting does not mark the project as saved.
10. Closing the window keeps ScreenHunter in the tray. **Exit** closes it and offers to save unsaved changes. There is no automatic startup or network upload.

Other shortcuts: Ctrl+Z/Y undo/redo, Ctrl+A select all, Ctrl+D duplicate, Delete remove, arrow keys move 1 pixel (Shift: 10). Text fields retain normal text shortcuts.

## Validation and limits
The editor, project format, import/export and portable startup have synthetic automated checks. DXGI capture testing received E_ACCESSDENIED in the development environment; real capture and multi-monitor/DPI acceptance still need an unrestricted interactive desktop. No secure/protected desktop bypass is provided.

HDR capture code converts FP16 to SDR sRGB with a 203-nit reference white and highlight compression. Native HDR export is not included. Real HDR hardware/color fidelity has not been validated.

Limits: 64 million decoded pixels, 32,768 pixels per side, 2000 objects, 50,000 points per stroke and 500,000 per document. Undo is limited to 80 interactions and an additional geometry/text budget. Fonts missing on another computer use a fallback with a warning.


New objects default to a white background where fill is supported and a 4-pixel stroke, including the highlighter. Fill can be enabled for tools where it starts disabled. Styles remain editable and projects retain saved values.
