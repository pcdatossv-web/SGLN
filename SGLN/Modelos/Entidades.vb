''' <summary>
''' Entidades del dominio. Se mantienen como clases simples (sin lógica) para
''' que la UI y los repositorios compartan una estructura clara.
''' </summary>
Public Class Cliente
    Public Property ID As Integer
    Public Property Nombres As String = ""
    Public Property Apellidos As String = ""
    Public Property DUI As String = ""
    Public Property NIT As String = ""
    Public Property Nacionalidad As String = ""
    Public Property Profesion As String = ""
    Public Property FechaNacimiento As Date?
    Public Property Direccion As String = ""
    Public Property Telefono As String = ""
    Public Property Email As String = ""
    Public Property Activo As Boolean = True
End Class

Public Class EntidadLegal
    Public Property ID As Integer
    Public Property RazonSocial As String = ""
    Public Property NIT As String = ""
    Public Property NRC As String = ""
    Public Property Domicilio As String = ""
    Public Property Activo As Boolean = True
End Class

Public Class PerfilLdaft
    Public Property ID As Integer
    Public Property IDCliente As Integer?
    Public Property IDEntidad As Integer?
    Public Property NivelRiesgo As String = "Bajo"
    Public Property OrigenFondosVerificado As Boolean
    Public Property DetalleOrigenFondos As String = ""
    Public Property FechaVerificacionListas As Date?
    Public Property Observaciones As String = ""
End Class

Public Class Caso
    Public Property ID As Integer
    Public Property IDCliente As Integer
    Public Property NumeroExpediente As String = ""
    Public Property Materia As String = ""
    Public Property IDJuzgado As Integer?
    Public Property Descripcion As String = ""
    Public Property Estado As String = "Activo"
    Public Property FechaApertura As Date = Date.Today
    Public Property Responsable As String = ""
End Class

Public Class ActuacionProcesal
    Public Property ID As Integer
    Public Property IDCaso As Integer
    Public Property IDTipoActuacion As Integer
    Public Property FechaNotificacion As Date
    Public Property PlazoLegalDias As Integer
    Public Property FechaVencimiento As Date?
    Public Property Estado As String = "Pendiente"
    Public Property Observaciones As String = ""
End Class

Public Class LibroProtocolo
    Public Property ID As Integer
    Public Property NumeroLibro As Integer
    Public Property AnioVigencia As Integer
    Public Property FechaAutorizacion As Date = Date.Today
    Public Property FechaVencimiento As Date
    Public Property Estado As String = "Vigente"
    Public Property RazonCierre As String = ""
    Public Property FechaCierre As Date?
    Public Property FechaEntregaNotariado As Date?
    Public Property IndiceGenerado As Boolean
End Class

Public Class Instrumento
    Public Property ID As Integer
    Public Property IDLibro As Integer
    Public Property NumeroEscritura As Integer
    Public Property FechaHoraOtorgamiento As Date = Date.Now
    Public Property FolioInicio As Integer
    Public Property FolioFin As Integer
    Public Property ObjetoActo As String = ""
    Public Property Estado As String = "Vigente"
End Class

Public Class Documento
    Public Property ID As Integer
    Public Property TipoDocumento As String = ""
    Public Property RutaArchivo As String = ""
    Public Property Hash As String = ""
    Public Property EstadoProcesamientoIA As String = "No procesado"
    Public Property IDCaso As Integer?
    Public Property IDInstrumento As Integer?
End Class

Public Class OperacionLdaft
    Public Property ID As Integer
    Public Property IDCaso As Integer?
    Public Property IDInstrumento As Integer?
    Public Property TipoOperacion As String = "Regulada"
    Public Property Monto As Decimal
    Public Property Moneda As String = "USD"
    Public Property FechaOperacion As Date = Date.Today
    Public Property Descripcion As String = ""
    Public Property Estado As String = "Registrada"
    Public Property FechaVencimientoAnalisis As Date?
    Public Property FechaVencimientoReporte As Date?
    Public Property FechaReporte As Date?
    Public Property ReportadoUIF As Boolean
    Public Property Observaciones As String = ""
End Class

Public Class SolicitudArco
    Public Property ID As Integer
    Public Property IDCliente As Integer
    Public Property TipoDerecho As String = "Acceso"
    Public Property FechaSolicitud As Date = Date.Today
    Public Property FechaVencimiento As Date
    Public Property FechaVencimientoProrroga As Date?
    Public Property ProrrogaAplicada As Boolean
    Public Property Estado As String = "Pendiente"
    Public Property FechaRespuesta As Date?
    Public Property Detalle As String = ""
End Class

Public Class UsuarioSesion
    Public Property ID As Integer
    Public Property Usuario As String = ""
    Public Property NombreCompleto As String = ""
    Public Property IDRol As Integer
    Public Property Rol As String = ""
    ''' <summary>True para el rol Administrador: obtiene acceso total.</summary>
    Public Property RolProtegido As Boolean
End Class
