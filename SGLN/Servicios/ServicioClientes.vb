Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Reglas de negocio del módulo de clientes y debida diligencia (LDA/FT).
''' </summary>
Public Class ServicioClientes

    Private ReadOnly _repositorio As New ClienteRepositorio()

    Public Function Buscar(filtro As String) As DataTable
        Return _repositorio.Buscar(filtro)
    End Function

    Public Function Obtener(id As Integer) As DataTable
        Return _repositorio.Obtener(id)
    End Function

    Public Function Guardar(cliente As Cliente, perfil As PerfilLdaft) As Respuesta
        If Not Permisos.TieneAcceso("clientes.gestionar") Then Return Respuesta.Fallo("No tiene permiso para gestionar clientes.")

        Dim errorValidacion As String = Validar(cliente, perfil)
        If errorValidacion <> "" Then Return Respuesta.Fallo(errorValidacion)

        Normalizar(cliente)

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
        Catch ex As Exception
            RegistroErrores.Escribir("Clientes - guardar", ex)
            Return Respuesta.Fallo("No se pudo guardar el cliente y su perfil. Verifique los datos y la conexión con la base de datos.")
        End Try
    End Function

    Public Function Desactivar(id As Integer) As Respuesta
        If Not Permisos.TieneAcceso("clientes.desactivar") Then Return Respuesta.Fallo("No tiene permiso para desactivar clientes.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione un cliente.")
        Try
            _repositorio.Desactivar(id)
            Return Respuesta.Ok("Cliente desactivado.")
        Catch ex As Exception
            RegistroErrores.Escribir("Clientes - desactivar", ex)
            Return Respuesta.Fallo("No se pudo desactivar el cliente. Verifique la conexión con la base de datos.")
        End Try
    End Function

    Private Function Validar(cliente As Cliente, perfil As PerfilLdaft) As String
        If String.IsNullOrWhiteSpace(cliente.Nombres) Then Return "Los nombres son obligatorios."
        If String.IsNullOrWhiteSpace(cliente.Apellidos) Then Return "Los apellidos son obligatorios."

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
        End If

        Return ""
    End Function

    Private Sub Normalizar(cliente As Cliente)
        If Not String.IsNullOrWhiteSpace(cliente.DUI) Then
            cliente.DUI = ValidadorIdentidad.FormatearDui(cliente.DUI)
        End If
        If Not String.IsNullOrWhiteSpace(cliente.NIT) Then
            cliente.NIT = ValidadorIdentidad.FormatearNit(cliente.NIT)
        End If
        cliente.Nombres = If(cliente.Nombres, "").Trim()
        cliente.Apellidos = If(cliente.Apellidos, "").Trim()
        cliente.Email = If(cliente.Email, "").Trim()
    End Sub

End Class
