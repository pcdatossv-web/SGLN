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
        Dim errorValidacion As String = Validar(cliente, perfil)
        If errorValidacion <> "" Then Return Respuesta.Fallo(errorValidacion)

        Normalizar(cliente)

        Try
            If cliente.ID = 0 Then
                cliente.ID = _repositorio.Insertar(cliente)
            Else
                _repositorio.Actualizar(cliente)
            End If
        Catch ex As MySqlException When ex.Number = 1062
            ' 1062 = entrada duplicada en un índice único (DUI repetido).
            Return Respuesta.Fallo("Ya existe un cliente registrado con ese DUI.")
        End Try

        If perfil IsNot Nothing Then
            perfil.IDCliente = cliente.ID
            _repositorio.GuardarPerfil(perfil)
        End If

        Return New Respuesta With {
            .Exito = True,
            .Mensaje = "Cliente guardado correctamente.",
            .ID = cliente.ID
        }
    End Function

    Public Function Desactivar(id As Integer) As Respuesta
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un cliente.")
        _repositorio.Desactivar(id)
        Return Respuesta.Ok("Cliente desactivado.")
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
        cliente.Nombres = cliente.Nombres.Trim()
        cliente.Apellidos = cliente.Apellidos.Trim()
        cliente.Email = cliente.Email.Trim()
    End Sub

End Class
