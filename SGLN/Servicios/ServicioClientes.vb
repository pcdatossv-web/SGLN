Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Reglas de negocio del módulo de clientes y debida diligencia (LDA/FT).
''' </summary>
Public Class ServicioClientes

    Private ReadOnly _repositorio As New ClienteRepositorio()

    Public Function Buscar(filtro As String) As DataTable
        If Not Permisos.TieneAcceso("clientes.ver") Then Return New DataTable()
        Return _repositorio.Buscar(filtro)
    End Function

    Public Function Obtener(id As Integer) As DataTable
        If Not Permisos.TieneAcceso("clientes.ver") OrElse id <= 0 Then Return New DataTable()
        Return _repositorio.Obtener(id)
    End Function

    Public Function Guardar(cliente As Cliente, perfil As PerfilLdaft) As Respuesta
        If Not Permisos.TieneAcceso("clientes.gestionar") Then Return Respuesta.Fallo("No tiene permiso para gestionar clientes.")
        If cliente Is Nothing Then Return Respuesta.Fallo("No se recibieron los datos del cliente.")

        Dim errorValidacion As String = Validar(cliente, perfil)
        If errorValidacion <> "" Then Return Respuesta.Fallo(errorValidacion)

        Normalizar(cliente, perfil)

        Try
            cliente.ID = _repositorio.GuardarCompleto(cliente, perfil)
            Return New Respuesta With {
                .Exito = True,
                .Mensaje = "Cliente y perfil guardados correctamente.",
                .ID = cliente.ID
            }
        Catch ex As MySqlException When ex.Number = 1062
            ' La restricción única del DUI y la del perfil se aplican en el servidor.
            Return Respuesta.Fallo("El DUI ya está registrado o existe un perfil LDA/FT duplicado.")
        Catch ex As InvalidOperationException
            Return Respuesta.Fallo(ex.Message)
        Catch ex As Exception
            RegistroErrores.Escribir("Clientes - guardar", ex)
            Return Respuesta.Fallo("No se pudo guardar el cliente y su perfil. Verifique los datos y la conexión con la base de datos.")
        End Try
    End Function

    Public Function Desactivar(id As Integer) As Respuesta
        If Not Permisos.TieneAcceso("clientes.desactivar") Then Return Respuesta.Fallo("No tiene permiso para desactivar clientes.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione un cliente.")
        Try
            Dim filas As Integer = _repositorio.Desactivar(id)
            If filas = 0 Then Return Respuesta.Fallo("El cliente no existe o ya estaba desactivado.")
            Return Respuesta.Ok("Cliente desactivado.")
        Catch ex As Exception
            RegistroErrores.Escribir("Clientes - desactivar", ex)
            Return Respuesta.Fallo("No se pudo desactivar el cliente. Verifique la conexión con la base de datos.")
        End Try
    End Function

    Private Function Validar(cliente As Cliente, perfil As PerfilLdaft) As String
        If String.IsNullOrWhiteSpace(cliente.Nombres) Then Return "Los nombres son obligatorios."
        If String.IsNullOrWhiteSpace(cliente.Apellidos) Then Return "Los apellidos son obligatorios."
        If cliente.Nombres.Trim().Length > 120 Then Return "Los nombres no pueden superar 120 caracteres."
        If cliente.Apellidos.Trim().Length > 120 Then Return "Los apellidos no pueden superar 120 caracteres."
        If If(cliente.Nacionalidad, "").Length > 60 Then Return "La nacionalidad no puede superar 60 caracteres."
        If If(cliente.Profesion, "").Length > 120 Then Return "La profesión no puede superar 120 caracteres."
        If If(cliente.Direccion, "").Length > 300 Then Return "La dirección no puede superar 300 caracteres."
        If If(cliente.Telefono, "").Length > 40 Then Return "El teléfono no puede superar 40 caracteres."
        If If(cliente.Email, "").Length > 120 Then Return "El correo electrónico no puede superar 120 caracteres."
        If cliente.FechaNacimiento.HasValue AndAlso cliente.FechaNacimiento.Value.Date > Date.Today Then
            Return "La fecha de nacimiento no puede ser futura."
        End If

        If Not String.IsNullOrWhiteSpace(cliente.DUI) AndAlso Not ValidadorIdentidad.EsFormatoDuiValido(cliente.DUI) Then
            Return "El DUI debe tener 9 dígitos (formato 00000000-0)."
        End If

        If Not String.IsNullOrWhiteSpace(cliente.NIT) AndAlso Not ValidadorIdentidad.EsFormatoNitValido(cliente.NIT) Then
            Return "El NIT debe tener 14 dígitos."
        End If

        If Not String.IsNullOrWhiteSpace(cliente.Email) AndAlso Not cliente.Email.Contains("@") Then
            Return "El correo electrónico no es válido."
        End If

        If perfil IsNot Nothing Then
            If perfil.NivelRiesgo <> "Bajo" AndAlso perfil.NivelRiesgo <> "Medio" AndAlso perfil.NivelRiesgo <> "Alto" Then
                Return "El nivel de riesgo debe ser Bajo, Medio o Alto."
            End If
            If If(perfil.DetalleOrigenFondos, "").Length > 500 OrElse
               If(perfil.Observaciones, "").Length > 500 Then
                Return "El detalle de origen de fondos y las observaciones no pueden superar 500 caracteres."
            End If
            If perfil.FechaVerificacionListas.HasValue AndAlso
               perfil.FechaVerificacionListas.Value.Date > Date.Today Then
                Return "La fecha de verificación de listas no puede ser futura."
            End If
        End If

        Return ""
    End Function

    Private Sub Normalizar(cliente As Cliente, perfil As PerfilLdaft)
        If Not String.IsNullOrWhiteSpace(cliente.DUI) Then
            cliente.DUI = ValidadorIdentidad.FormatearDui(cliente.DUI)
        End If
        If Not String.IsNullOrWhiteSpace(cliente.NIT) Then
            cliente.NIT = ValidadorIdentidad.FormatearNit(cliente.NIT)
        End If
        cliente.Nombres = If(cliente.Nombres, "").Trim()
        cliente.Apellidos = If(cliente.Apellidos, "").Trim()
        cliente.Nacionalidad = If(cliente.Nacionalidad, "").Trim()
        cliente.Profesion = If(cliente.Profesion, "").Trim()
        cliente.Direccion = If(cliente.Direccion, "").Trim()
        cliente.Telefono = If(cliente.Telefono, "").Trim()
        cliente.Email = If(cliente.Email, "").Trim()
        If perfil IsNot Nothing Then
            perfil.DetalleOrigenFondos = If(perfil.DetalleOrigenFondos, "").Trim()
            perfil.Observaciones = If(perfil.Observaciones, "").Trim()
        End If
    End Sub

End Class
