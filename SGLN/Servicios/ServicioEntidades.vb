Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Reglas de negocio de entidades legales y sus representantes.
''' </summary>
Public Class ServicioEntidades

    Private ReadOnly _repositorio As New EntidadRepositorio()

    Public Function Buscar(filtro As String) As DataTable
        If Not Permisos.TieneAcceso("entidades.ver") Then Return New DataTable()
        Return _repositorio.Buscar(filtro)
    End Function

    Public Function Obtener(id As Integer) As DataTable
        If Not Permisos.TieneAcceso("entidades.ver") OrElse id <= 0 Then Return New DataTable()
        Return _repositorio.Obtener(id)
    End Function

    Public Function Guardar(entidad As EntidadLegal, perfil As PerfilLdaft) As Respuesta
        If Not Permisos.TieneAcceso("entidades.gestionar") Then Return Respuesta.Fallo("No tiene permiso para gestionar entidades.")

        If entidad Is Nothing Then Return Respuesta.Fallo("No se recibieron los datos de la entidad.")
        If String.IsNullOrWhiteSpace(entidad.RazonSocial) Then
            Return Respuesta.Fallo("La razón social es obligatoria.")
        End If
        If entidad.RazonSocial.Trim().Length > 200 Then Return Respuesta.Fallo("La razón social no puede superar 200 caracteres.")
        If If(entidad.NRC, "").Length > 30 Then Return Respuesta.Fallo("El NRC no puede superar 30 caracteres.")
        If If(entidad.Domicilio, "").Length > 300 Then Return Respuesta.Fallo("El domicilio no puede superar 300 caracteres.")
        If If(entidad.NIT, "").Length > 20 Then Return Respuesta.Fallo("El NIT no puede superar 20 caracteres.")

        If Not String.IsNullOrWhiteSpace(entidad.NIT) AndAlso Not ValidadorIdentidad.EsFormatoNitValido(entidad.NIT) Then
            Return Respuesta.Fallo("El NIT debe tener 14 dígitos.")
        End If

        If perfil IsNot Nothing Then
            If perfil.NivelRiesgo <> "Bajo" AndAlso perfil.NivelRiesgo <> "Medio" AndAlso perfil.NivelRiesgo <> "Alto" Then
                Return Respuesta.Fallo("El nivel de riesgo debe ser Bajo, Medio o Alto.")
            End If
            If If(perfil.DetalleOrigenFondos, "").Length > 500 OrElse If(perfil.Observaciones, "").Length > 500 Then
                Return Respuesta.Fallo("El detalle de origen de fondos y las observaciones no pueden superar 500 caracteres.")
            End If
            If perfil.FechaVerificacionListas.HasValue AndAlso perfil.FechaVerificacionListas.Value.Year < 1000 Then
                Return Respuesta.Fallo("La fecha de verificación de listas no es válida.")
            End If
            If perfil.FechaVerificacionListas.HasValue AndAlso perfil.FechaVerificacionListas.Value.Date > Date.Today Then
                Return Respuesta.Fallo("La fecha de verificación de listas no puede ser futura.")
            End If
        End If

        entidad.RazonSocial = entidad.RazonSocial.Trim()
        entidad.NRC = If(entidad.NRC, "").Trim()
        entidad.Domicilio = If(entidad.Domicilio, "").Trim()
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
        Catch ex As InvalidOperationException
            Return Respuesta.Fallo(ex.Message)
        Catch ex As Exception
            RegistroErrores.Escribir("Entidades - guardar", ex)
            Return Respuesta.Fallo("No se pudo guardar la entidad y su perfil. Verifique los datos y la conexión con la base de datos.")
        End Try
    End Function

    Public Function Desactivar(id As Integer) As Respuesta
        If Not Permisos.TieneAcceso("entidades.gestionar") Then Return Respuesta.Fallo("No tiene permiso para gestionar entidades.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione una entidad.")
        Try
            Dim filas As Integer = _repositorio.Desactivar(id)
            If filas = 0 Then Return Respuesta.Fallo("La entidad no existe o ya estaba desactivada.")
            Return Respuesta.Ok("Entidad desactivada.")
        Catch ex As Exception
            RegistroErrores.Escribir("Entidades - desactivar", ex)
            Return Respuesta.Fallo("No se pudo desactivar la entidad. Verifique la conexión y vuelva a intentarlo.")
        End Try
    End Function

    Public Function Representantes(idEntidad As Integer) As DataTable
        If Not Permisos.TieneAcceso("entidades.ver") OrElse idEntidad <= 0 Then Return New DataTable()
        Return _repositorio.Representantes(idEntidad)
    End Function

    Public Function ClientesCombo() As DataTable
        If Not (Permisos.TieneAcceso("entidades.ver") OrElse Permisos.TieneAcceso("entidades.gestionar")) Then Return New DataTable()
        Return _repositorio.ClientesCombo()
    End Function

    Public Function AgregarRepresentante(idEntidad As Integer, idCliente As Integer, cargo As String, tipo As String) As Respuesta
        If Not Permisos.TieneAcceso("entidades.gestionar") Then Return Respuesta.Fallo("No tiene permiso para gestionar representantes.")

        If idEntidad <= 0 Then Return Respuesta.Fallo("Guarde primero la entidad.")
        If idCliente <= 0 Then Return Respuesta.Fallo("Seleccione un cliente.")
        If If(cargo, "").Length > 120 Then Return Respuesta.Fallo("El cargo no puede superar 120 caracteres.")
        If If(tipo, "").Length > 80 Then Return Respuesta.Fallo("El tipo de representación no puede superar 80 caracteres.")
        Try
            Return _repositorio.AgregarRepresentante(idEntidad, idCliente, If(cargo, "").Trim(), If(tipo, "").Trim())
        Catch ex As Exception
            RegistroErrores.Escribir("Entidades - agregar representante", ex)
            Return Respuesta.Fallo("No se pudo agregar el representante. Verifique los datos y la conexión con la base de datos.")
        End Try
    End Function

    Public Function QuitarRepresentante(id As Integer) As Respuesta
        If Not Permisos.TieneAcceso("entidades.gestionar") Then Return Respuesta.Fallo("No tiene permiso para gestionar representantes.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione un representante.")
        Try
            Dim filas As Integer = _repositorio.QuitarRepresentante(id)
            If filas = 0 Then Return Respuesta.Fallo("El representante ya no existe.")
            Return Respuesta.Ok("Representante removido.")
        Catch ex As Exception
            RegistroErrores.Escribir("Entidades - quitar representante", ex)
            Return Respuesta.Fallo("No se pudo quitar el representante. Verifique la conexión y vuelva a intentarlo.")
        End Try
    End Function

End Class
