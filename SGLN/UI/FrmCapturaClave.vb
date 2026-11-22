Imports System
Imports System.Windows.Forms

''' <summary>Diálogo reutilizable para capturar y confirmar claves.</summary>
Partial Public Class FrmCapturaClave

    Private ReadOnly _modoCambioInicial As Boolean
    Public Property ClaveCapturada As String

    Public Sub New(modoCambioInicial As Boolean, titulo As String, instruccion As String)
        InitializeComponent()
        _modoCambioInicial = modoCambioInicial
        Text = titulo
        lblInstruccion.Text = instruccion
        lblConfirmar.Visible = modoCambioInicial
        txtConfirmar.Visible = modoCambioInicial
        If modoCambioInicial Then
            lblClave.Text = "Nueva clave:"
            btnAceptar.Text = "Guardar clave"
        Else
            lblClave.Text = "Clave actual:"
            btnAceptar.Text = "Verificar y continuar"
        End If
        ActiveControl = txtClave
    End Sub

    Private Sub FrmCapturaClave_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        ' El diálogo puede manejar claves actuales o nuevas; limpiar ambos campos al cerrarlo.
        txtClave.Clear()
        txtConfirmar.Clear()
        ClaveCapturada = Nothing
        chkMostrarClaves.Checked = False
    End Sub

    Private Sub chkMostrarClaves_CheckedChanged(sender As Object, e As EventArgs) Handles chkMostrarClaves.CheckedChanged
        Dim ocultar As Boolean = Not chkMostrarClaves.Checked
        txtClave.UseSystemPasswordChar = ocultar
        txtConfirmar.UseSystemPasswordChar = ocultar
    End Sub

    Private Sub btnAceptar_Click(sender As Object, e As EventArgs) Handles btnAceptar.Click
        If _modoCambioInicial Then
            Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(txtClave.Text)
            If errorClave <> "" Then
                MessageBox.Show(errorClave, "Clave no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtClave.Focus()
                Return
            End If
            If Not String.Equals(txtClave.Text, txtConfirmar.Text, StringComparison.Ordinal) Then
                MessageBox.Show("La confirmación no coincide con la nueva clave.",
                                "Clave no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtConfirmar.Clear()
                txtConfirmar.Focus()
                Return
            End If
        ElseIf String.IsNullOrEmpty(txtClave.Text) Then
            MessageBox.Show("Ingrese su clave actual.", "Dato requerido",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtClave.Focus()
            Return
        End If

        ClaveCapturada = txtClave.Text
        DialogResult = DialogResult.OK
        Close()
    End Sub
End Class
