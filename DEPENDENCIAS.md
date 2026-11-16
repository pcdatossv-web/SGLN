# Inventario de dependencias de SGLN

El proyecto apunta a .NET Framework 4.8 y referencia cuatro ensamblados externos
almacenados directamente en `lib/`:

| Archivo | Uso |
|---|---|
| `MySql.Data.dll` | Conector ADO.NET para MySQL |
| `System.Buffers.dll` | Soporte de buffers |
| `System.Memory.dll` | Tipos de memoria y spans |
| `System.Numerics.Vectors.dll` | Tipos vectoriales |

El proyecto no declara actualmente un `packages.config` ni referencias
`PackageReference`; por eso las versiones deben obtenerse de los metadatos
reales de cada DLL y no inferirse del nombre del archivo.

El workflow de Windows ejecuta `Pruebas/Inventario-Dependencias.ps1`, que
registra para cada ensamblado:

- AssemblyVersion, FileVersion y ProductVersion.
- Empresa declarada en los metadatos.
- Tamaño y SHA-256 del archivo.

Estos datos permiten identificar exactamente los binarios que se compilan.
El inventario no sustituye el análisis de vulnerabilidades: después de conocer
las versiones efectivas hay que contrastarlas con los avisos de seguridad del
proveedor y planificar actualizaciones compatibles.

## Versiones fijadas para esta etapa

Se conserva **MySql.Data.dll 8.0.30.0** y la versión de servidor MySQL que
actualmente utiliza la instalación. No se actualizarán el conector, sus DLL
auxiliares ni el servidor durante esta etapa de correcciones, para evitar
introducir incompatibilidades en una aplicación que ya trabaja con esa
combinación.

Las correcciones deben realizarse en el código VB.NET, consultas y scripts SQL
compatibles con la versión MySQL instalada. Cualquier cambio futuro de
versiones será una migración independiente, con respaldo, entorno de pruebas
y validación completa, y no forma parte de esta ronda.

Las referencias a `System.*` proporcionadas por .NET Framework no se incluyen
en esta lista porque forman parte de la plataforma de destino.
