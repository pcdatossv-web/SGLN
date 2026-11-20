Imports System
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Permite a cualquier usuario autenticado cambiar su propia clave,
''' exigiendo la clave actual y confirmación de la nueva.
''' </summary>
Partial Public Class FrmCambioClave

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub FrmCambioClave_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        txtActual.Focus()
    End Sub

    Private Sub GuardarClick(sender As Object, e As EventArgs)
        UtileriasUI.EjecutarUnaVez(btnGuardar, AddressOf Guardar)
    End Sub

    Private Sub Guardar()
        Try
            If Not Permisos.RevalidarSesion() Then
                Sesion.CerrarSesionSolicitado = True
                lblMensaje.ForeColor = Color.Firebrick
                lblMensaje.Text = "La sesión ya no está activa. Inicie sesión nuevamente."
                Me.DialogResult = DialogResult.Cancel
                Me.Close()
                Return
            End If

            If Sesion.UsuarioActual Is Nothing Then
                lblMensaje.ForeColor = Color.Firebrick
                lblMensaje.Text = "La sesión ya no está activa. Inicie sesión nuevamente."
                Return
            End If

            If Not String.Equals(txtNueva.Text, txtConfirmar.Text, StringComparison.Ordinal) Then
                lblMensaje.ForeColor = Color.Firebrick
                lblMensaje.Text = "La confirmación no coincide con la nueva clave."
                txtConfirmar.Clear()
                txtConfirmar.Focus()
                Return
            End If

            Dim respuesta As Respuesta =
                New ServicioUsuarios().CambiarClavePropia(
                    Sesion.UsuarioActual.ID, txtActual.Text, txtNueva.Text)

            If Not respuesta.Exito Then
                lblMensaje.ForeColor = Color.Firebrick
                lblMensaje.Text = respuesta.Mensaje
                txtActual.Clear()
                txtActual.Focus()
                Return
            End If

            MessageBox.Show("La clave se cambió correctamente.",
                            "Seguridad", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            RegistroErrores.Escribir("Cambiar mi clave", ex)
            lblMensaje.Text = "No se pudo cambiar la clave. Verifique la conexión y vuelva a intentarlo."
        End Try
    End Sub
End Class
