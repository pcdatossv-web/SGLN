<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmAcercaDe
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lblTitulo = New System.Windows.Forms.Label()
        Me.lblVersion = New System.Windows.Forms.Label()
        Me.txtAviso = New System.Windows.Forms.TextBox()
        Me.btnCerrar = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        ' lblTitulo
        '
        Me.lblTitulo.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitulo.Location = New System.Drawing.Point(24, 20)
        Me.lblTitulo.Name = "lblTitulo"
        Me.lblTitulo.Size = New System.Drawing.Size(560, 30)
        Me.lblTitulo.TabIndex = 0
        Me.lblTitulo.Text = "SGLN El Salvador"
        '
        ' lblVersion
        '
        Me.lblVersion.ForeColor = System.Drawing.Color.DimGray
        Me.lblVersion.Location = New System.Drawing.Point(26, 52)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(560, 20)
        Me.lblVersion.TabIndex = 1
        Me.lblVersion.Text = "Sistema de Gestión Legal y Notarial · Versión 1.0"
        '
        ' txtAviso
        '
        Me.txtAviso.BackColor = System.Drawing.Color.White
        Me.txtAviso.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtAviso.Location = New System.Drawing.Point(28, 90)
        Me.txtAviso.Multiline = True
        Me.txtAviso.Name = "txtAviso"
        Me.txtAviso.ReadOnly = True
        Me.txtAviso.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtAviso.Size = New System.Drawing.Size(552, 230)
        Me.txtAviso.TabIndex = 2
        Me.txtAviso.TabStop = False
        Me.txtAviso.Text = "Sistema diseñado para apoyar la gestión de un bufete legal y notarial salvadoreño." & Microsoft.VisualBasic.ControlChars.CrLf & Microsoft.VisualBasic.ControlChars.CrLf &
            "Advertencias legales:" & Microsoft.VisualBasic.ControlChars.CrLf &
            "• Los plazos procesales son calculados por el sistema en días hábiles (Art. 145 CPCM) y deben ser validados y confirmados por el abogado responsable." & Microsoft.VisualBasic.ControlChars.CrLf &
            "• Los borradores y extracciones asistidas por IA requieren revisión y firma del abogado. La IA/OCR se implementará en la fase final." & Microsoft.VisualBasic.ControlChars.CrLf &
            "• Verifique la concurrencia de testigos e intérpretes conforme al Art. 32 de la Ley de Notariado." & Microsoft.VisualBasic.ControlChars.CrLf &
            "• Los reportes a la UIF son confidenciales: el sistema no permite notificar al cliente sobre una operación reportada." & Microsoft.VisualBasic.ControlChars.CrLf & Microsoft.VisualBasic.ControlChars.CrLf &
            "Este software no sustituye el criterio jurídico del profesional."
        '
        ' btnCerrar
        '
        Me.btnCerrar.Location = New System.Drawing.Point(460, 336)
        Me.btnCerrar.Name = "btnCerrar"
        Me.btnCerrar.Size = New System.Drawing.Size(120, 32)
        Me.btnCerrar.TabIndex = 3
        Me.btnCerrar.Text = "Cerrar"
        Me.btnCerrar.UseVisualStyleBackColor = True
        '
        ' FrmAcercaDe
        '
        Me.AcceptButton = Me.btnCerrar
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(610, 390)
        Me.Controls.Add(Me.btnCerrar)
        Me.Controls.Add(Me.txtAviso)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.lblTitulo)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmAcercaDe"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Acerca de SGLN"
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents lblTitulo As System.Windows.Forms.Label
    Friend WithEvents lblVersion As System.Windows.Forms.Label
    Friend WithEvents txtAviso As System.Windows.Forms.TextBox
    Friend WithEvents btnCerrar As System.Windows.Forms.Button
End Class
