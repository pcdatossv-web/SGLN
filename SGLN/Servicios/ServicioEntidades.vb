Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Reglas de negocio de entidades legales y sus representantes.
''' </summary>
Public Class ServicioEntidades

    Private ReadOnly _repositorio As New EntidadRepositorio()

    Public Function Buscar(filtro As String) As DataTable
        Return _repositorio.Buscar(filtro)
    End Function

    Public Function Obtener(id As Integer) As DataTable
        Return _repositorio.Obtener(id)
    End Function

    Public Function Guardar(entidad As EntidadLegal, perfil As PerfilLdaft) As Respuesta
        If String.IsNullOrWhiteSpace(entidad.RazonSocial) Then
            Return Respuesta.Fallo("La razón social es obligatoria.")
        End If

        If Not String.IsNullOrWhiteSpace(entidad.NIT) AndAlso Not ValidadorIdentidad.EsFormatoNitValido(entidad.NIT) Then
            Return Respuesta.Fallo("El NIT debe tener 14 dígitos.")
        End If

        If perfil IsNot Nothing AndAlso perfil.NivelRiesgo <> "Bajo" AndAlso perfil.NivelRiesgo <> "Medio" AndAlso perfil.NivelRiesgo <> "Alto" Then
            Return Respuesta.Fallo("El nivel de riesgo debe ser Bajo, Medio o Alto.")
        End If

        entidad.RazonSocial = entidad.RazonSocial.Trim()
        If Not String.IsNullOrWhiteSpace(entidad.NIT) Then
            entidad.NIT = ValidadorIdentidad.FormatearNit(entidad.NIT)
        End If

        Try
            entidad.ID = _repositorio.GuardarCompleto(entidad, perfil)
            Return New Respuesta With {
                .Exito = True,
                .Mensaje = "Entidad y perfil guardados correctamente.",
                .ID = entidad.ID
            }
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("La entidad o su perfil LDA/FT ya está registrado.")
        Catch ex As Exception
            RegistroErrores.Escribir("Entidades - guardar", ex)
            Return Respuesta.Fallo("No se pudo guardar la entidad y su perfil: " & ex.Message)
        End Try
    End Function

    Public Function Desactivar(id As Integer) As Respuesta
        If id <= 0 Then Return Respuesta.Fallo("Seleccione una entidad.")
        Try
            _repositorio.Desactivar(id)
            Return Respuesta.Ok("Entidad desactivada.")
        Catch ex As Exception
            RegistroErrores.Escribir("Entidades - desactivar", ex)
            Return Respuesta.Fallo("No se pudo desactivar la entidad: " & ex.Message)
        End Try
    End Function

    Public Function Representantes(idEntidad As Integer) As DataTable
        Return _repositorio.Representantes(idEntidad)
    End Function

    Public Function ClientesCombo() As DataTable
        Return _repositorio.ClientesCombo()
    End Function

    Public Function AgregarRepresentante(idEntidad As Integer, idCliente As Integer, cargo As String, tipo As String) As Respuesta
        If idEntidad <= 0 Then Return Respuesta.Fallo("Guarde primero la entidad.")
        If idCliente <= 0 Then Return Respuesta.Fallo("Seleccione un cliente.")
        Return _repositorio.AgregarRepresentante(idEntidad, idCliente, cargo, tipo)
    End Function

    Public Sub QuitarRepresentante(id As Integer)
        _repositorio.QuitarRepresentante(id)
    End Sub

End Class
