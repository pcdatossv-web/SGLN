Imports System
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

    Private Const ClavesReservadas As String = "Carpeta_Documentos,Carpeta_Respaldos"

    Public Function Listar() As DataTable
        Return _repositorio.Listar()
    End Function

    Public Function Obtener(clave As String) As String
        Return _repositorio.Obtener(clave)
    End Function

    Public Function Guardar(clave As String, valor As String) As Respuesta
        If Not Permisos.TieneAcceso("config.editar") Then Return Respuesta.Fallo("No tiene permiso para modificar la configuración.")

        If String.IsNullOrWhiteSpace(clave) Then Return Respuesta.Fallo("Indique la clave de configuración.")
        If clave.Trim().Length > 60 Then Return Respuesta.Fallo("La clave no puede superar 60 caracteres.")

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
            _repositorio.EliminarFeriado(id)
            Return Respuesta.Ok("Feriado eliminado. Los plazos ya confirmados no se recalculan.")
        Catch ex As Exception
            RegistroErrores.Escribir("Configuración - eliminar feriado", ex)
            Return Respuesta.Fallo("No se pudo eliminar el feriado. Verifique la conexión y vuelva a intentarlo.")
        End Try
    End Function

    Public Function Usuarios() As DataTable
        Return _repositorio.Usuarios()
    End Function

    Public Function CambiarClave(idUsuario As Integer, claveActual As String, claveNueva As String) As Respuesta
        If Not Permisos.TieneAcceso("config.seguridad") Then Return Respuesta.Fallo("No tiene permiso para cambiar la configuración de seguridad.")
        If Sesion.UsuarioActual Is Nothing OrElse Sesion.UsuarioActual.ID <> idUsuario Then Return Respuesta.Fallo("Solo puede cambiar la clave de la cuenta con la que inició sesión.")

        If idUsuario <= 0 Then Return Respuesta.Fallo("Seleccione un usuario.")
        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(claveNueva)
        If errorClave <> "" Then Return Respuesta.Fallo(errorClave)
        If Not _usuarios.VerificarClave(idUsuario, claveActual) Then
            Return Respuesta.Fallo("La clave actual no es correcta.")
        End If

        Return _usuarios.CambiarClave(idUsuario, claveNueva)
    End Function

    ''' <summary>
    ''' Genera el respaldo con nombre único dentro de la carpeta configurada.
    ''' Devuelve la ruta final para mostrarla al usuario.
    ''' </summary>
    Public Function Respaldar() As Respuesta
        If Not Permisos.TieneAcceso("config.respaldos") Then Return Respuesta.Fallo("No tiene permiso para crear respaldos.")

        Try
            Dim carpeta As String = CarpetaRespaldos()
            Directory.CreateDirectory(carpeta)

            Dim nombre As String = "SGLN_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".sql"
            Dim ruta As String = Path.Combine(carpeta, nombre)

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
            Return Respuesta.Fallo("No se pudo crear el respaldo. Revise la configuración, los permisos de la carpeta y el registro local de errores.")
        End Try
    End Function

    Public Function CarpetaRespaldos() As String
        Dim carpeta As String = _repositorio.Obtener("Carpeta_Respaldos")
        If String.IsNullOrWhiteSpace(carpeta) Then carpeta = "Respaldos"
        If Not Path.IsPathRooted(carpeta) Then
            carpeta = Path.Combine(Application.StartupPath, carpeta)
        End If
        Return carpeta
    End Function

    Public Function EsClaveReservada(clave As String) As Boolean
        If String.IsNullOrWhiteSpace(clave) Then Return False
        Return Array.IndexOf(ClavesReservadas.Split(","c), clave.Trim()) >= 0
    End Function

End Class
