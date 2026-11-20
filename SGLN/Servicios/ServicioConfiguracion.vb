Imports System
Imports System.Linq
Imports System.Data
Imports System.IO
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

''' <summary>
''' Reglas de configuración, feriados y respaldos. El respaldo se guarda en
''' la carpeta configurada y se nombra con fecha y hora para no sobrescribir.
''' </summary>
Public Class ServicioConfiguracion

    Private ReadOnly _repositorio As New ConfiguracionRepositorio()
    Private ReadOnly _usuarios As New UsuarioRepositorio()

    ''' <summary>
    ''' Revalida la sesión una sola vez cuando una operación admite permisos alternativos.
    ''' </summary>
    Private Shared Function TieneAccesoAlguno(ParamArray codigos As String()) As Boolean
        If Not Permisos.RevalidarSesion() OrElse codigos Is Nothing Then Return False
        For Each codigo As String In codigos
            If Permisos.Puede(codigo) Then Return True
        Next
        Return False
    End Function

    Private Const ClavesReservadas As String = "Carpeta_Documentos,Carpeta_Respaldos"

    Public Function Listar() As DataTable
        If Not Permisos.TieneAcceso("config.ver") Then Return New DataTable()
        Return _repositorio.Listar()
    End Function

    Public Function Obtener(clave As String) As String
        If Not Permisos.TieneAcceso("config.ver") Then Return ""
        Return _repositorio.Obtener(clave)
    End Function

    Public Function Guardar(clave As String, valor As String) As Respuesta
        If Not Permisos.TieneAcceso("config.editar") Then Return Respuesta.Fallo("No tiene permiso para modificar la configuración.")

        If String.IsNullOrWhiteSpace(clave) Then Return Respuesta.Fallo("Indique la clave de configuración.")
        If clave.Trim().Length > 60 Then Return Respuesta.Fallo("La clave no puede superar 60 caracteres.")
        If EsClaveReservada(clave) Then
            Return Respuesta.Fallo("Las rutas de documentos y respaldos no se pueden modificar desde la configuración general.")
        End If

        Dim valorNormalizado As String = If(valor, "").Trim()
        If valorNormalizado.Length > 500 Then
            Return Respuesta.Fallo("El valor de configuración no puede superar 500 caracteres.")
        End If

        Try
            _repositorio.Guardar(clave.Trim(), valorNormalizado)
            Return Respuesta.Ok("Configuración guardada.")
        Catch ex As Exception
            RegistroErrores.Escribir("Configuración - guardar", ex)
            Return Respuesta.Fallo("No se pudo guardar la configuración. Verifique los datos y la conexión con la base de datos.")
        End Try
    End Function

    Public Function Feriados() As DataTable
        If Not TieneAccesoAlguno("config.feriados", "config.ver") Then Return New DataTable()
        Return _repositorio.Feriados()
    End Function

    Public Function AgregarFeriado(fecha As Date, descripcion As String) As Respuesta
        If Not Permisos.TieneAcceso("config.feriados") Then Return Respuesta.Fallo("No tiene permiso para administrar feriados.")

        If String.IsNullOrWhiteSpace(descripcion) Then
            Return Respuesta.Fallo("Indique la descripción del feriado.")
        End If
        If fecha.Date.Year < 2000 OrElse fecha.Date.Year > 2100 Then
            Return Respuesta.Fallo("El año del feriado no es válido.")
        End If

        Dim descripcionNormalizada As String = descripcion.Trim()
        If descripcionNormalizada.Length > 120 Then
            Return Respuesta.Fallo("La descripción del feriado no puede superar 120 caracteres.")
        End If

        Try
            Dim existentes As DataTable = _repositorio.Feriados()
            For Each fila As DataRow In existentes.Rows
                If Convert.ToDateTime(fila("Fecha")).Date = fecha.Date Then
                    Return Respuesta.Fallo("Ese día ya está registrado como feriado.")
                End If
            Next

            ' La restricción UNIQUE de la base de datos protege también frente
            ' a dos usuarios que intenten registrar la misma fecha a la vez.
            _repositorio.InsertarFeriado(fecha.Date, descripcionNormalizada)
            Return Respuesta.Ok("Feriado agregado.")
        Catch ex As MySqlException
            RegistroErrores.Escribir("Configuración - agregar feriado", ex)
            If ex.Number = 1062 Then
                Return Respuesta.Fallo("Ese día ya está registrado como feriado.")
            End If
            Return Respuesta.Fallo("No se pudo agregar el feriado. Verifique la conexión y los datos.")
        Catch ex As Exception
            RegistroErrores.Escribir("Configuración - agregar feriado", ex)
            Return Respuesta.Fallo("No se pudo agregar el feriado. Verifique la conexión y los datos.")
        End Try
    End Function

    Public Function EliminarFeriado(id As Integer) As Respuesta
        If Not Permisos.TieneAcceso("config.feriados") Then Return Respuesta.Fallo("No tiene permiso para administrar feriados.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione un feriado.")

        Try
            Dim filas As Integer = _repositorio.EliminarFeriado(id)
            If filas = 0 Then Return Respuesta.Fallo("El feriado ya no existe.")
            Return Respuesta.Ok("Feriado eliminado. Los plazos ya confirmados no se recalculan.")
        Catch ex As Exception
            RegistroErrores.Escribir("Configuración - eliminar feriado", ex)
            Return Respuesta.Fallo("No se pudo eliminar el feriado. Verifique la conexión y vuelva a intentarlo.")
        End Try
    End Function

    Public Function Usuarios() As DataTable
        If Not Permisos.TieneAcceso("config.seguridad") Then Return New DataTable()
        Return _repositorio.Usuarios()
    End Function

    Public Function CambiarClave(idUsuario As Integer, claveActual As String, claveNueva As String) As Respuesta
        If Not Permisos.TieneAcceso("config.seguridad") Then Return Respuesta.Fallo("No tiene permiso para cambiar la configuración de seguridad.")
        If Sesion.UsuarioActual Is Nothing OrElse Sesion.UsuarioActual.ID <> idUsuario Then Return Respuesta.Fallo("Solo puede cambiar la clave de la cuenta con la que inició sesión.")

        If idUsuario <= 0 Then Return Respuesta.Fallo("Seleccione un usuario.")
        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(claveNueva)
        If errorClave <> "" Then Return Respuesta.Fallo(errorClave)
        Return New ServicioUsuarios().CambiarClavePropia(idUsuario, claveActual, claveNueva)
    End Function

    ''' <summary>
    ''' Genera el respaldo con nombre único dentro de la carpeta configurada.
    ''' Devuelve la ruta final para mostrarla al usuario.
    ''' </summary>
    Public Function Respaldar() As Respuesta
        If Not Permisos.TieneAcceso("config.respaldos") Then Return Respuesta.Fallo("No tiene permiso para crear respaldos.")

        Dim ruta As String = ""
        Try
            Dim carpeta As String = CarpetaRespaldos()
            Directory.CreateDirectory(carpeta)
            ' La carpeta puede haber sido sustituida por un junction/symlink
            ' después de leer la configuración. Revalidar antes de escribir.
            carpeta = CarpetaRespaldos()

            ' Milisegundos más sufijo aleatorio: dos respaldos en el mismo
            ' segundo nunca deben reemplazarse entre sí.
            Dim nombre As String = "SGLN_" & DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") & "_" &
                Guid.NewGuid().ToString("N").Substring(0, 8) & ".sql"
            ruta = Path.Combine(carpeta, nombre)

            _repositorio.RespaldarBaseDatos(ruta)

            If Not File.Exists(ruta) Then
                Return Respuesta.Fallo(
                    "mysqldump terminó pero el archivo no quedó en la ruta indicada. " &
                    "Verifique que la carpeta exista y tenga permisos de escritura.")
            End If

            Dim informacion As New FileInfo(ruta)
            Return Respuesta.Ok(
                "Respaldo creado: " & ruta & " (" & (informacion.Length \ 1024).ToString("N0") & " KB)")
        Catch ex As Exception
            RegistroErrores.Escribir("Configuración - respaldo", ex)
            If Not String.IsNullOrWhiteSpace(ruta) AndAlso File.Exists(ruta) Then
                Return Respuesta.Fallo(
                    "El archivo de respaldo se generó y se conserva en: " & ruta &
                    ". Sin embargo, no se pudo completar la confirmación/auditoría de la operación. " &
                    "Informe al administrador antes de repetir el respaldo.")
            End If
            Return Respuesta.Fallo("No se pudo crear el respaldo. Revise la configuración, los permisos de la carpeta y el registro local de errores.")
        End Try
    End Function

    Public Function CarpetaRespaldos() As String
        If Not Permisos.TieneAcceso("config.respaldos") Then Return ""
        Dim carpeta As String = _repositorio.Obtener("Carpeta_Respaldos")
        If String.IsNullOrWhiteSpace(carpeta) Then carpeta = "Respaldos"
        If Not Path.IsPathRooted(carpeta) Then
            carpeta = Path.Combine(Application.StartupPath, carpeta)
        End If

        Dim rutaCompleta As String = Path.GetFullPath(carpeta)
        Dim raiz As String = Path.GetPathRoot(rutaCompleta)
        If String.Equals(
            rutaCompleta.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            raiz.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException(
                "La carpeta de respaldos debe ser una carpeta dedicada; no se permite usar la raíz de una unidad o recurso compartido.")
        End If

        ValidarCarpetaRespaldosSinEnlaces(rutaCompleta)
        Return rutaCompleta
    End Function

    ''' <summary>
    ''' Impide que una carpeta de respaldos aparentemente local redirija la
    ''' escritura de copias sensibles fuera de la ubicación autorizada.
    ''' </summary>
    Private Shared Sub ValidarCarpetaRespaldosSinEnlaces(rutaCompleta As String)
        ' Recorrer todos los segmentos desde la raíz para detectar enlaces
        ' también en directorios padres de la carpeta configurada.
        Dim rutaAbsoluta As String = Path.GetFullPath(rutaCompleta)
        Dim raiz As String = Path.GetPathRoot(rutaAbsoluta)
        If String.IsNullOrWhiteSpace(raiz) Then
            Throw New InvalidOperationException("La ruta de respaldos no tiene una raíz válida.")
        End If

        Dim relativo As String = rutaAbsoluta.Substring(raiz.Length)
        Dim segmentos As String() = relativo.Split(
            New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar},
            StringSplitOptions.RemoveEmptyEntries)
        Dim actual As String = raiz

        For Each segmento As String In segmentos
            actual = Path.Combine(actual, segmento)
            Try
                Dim atributos As FileAttributes = File.GetAttributes(actual)
                If (atributos And FileAttributes.ReparsePoint) = FileAttributes.ReparsePoint Then
                    Throw New InvalidOperationException(
                        "La ruta de respaldos contiene un enlace simbólico o punto de reanálisis no permitido.")
                End If
            Catch ex As FileNotFoundException
                ' El archivo o directorio final puede no existir todavía.
            Catch ex As DirectoryNotFoundException
                ' Se permiten segmentos que aún no se han creado.
            End Try
        Next
    End Sub

    Public Function EsClaveReservada(clave As String) As Boolean
        If String.IsNullOrWhiteSpace(clave) Then Return False
        Return ClavesReservadas.Split(","c).Any(
            Function(reservada As String) String.Equals(reservada, clave.Trim(), StringComparison.OrdinalIgnoreCase))
    End Function

End Class
