
Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Data.SqlClient
Imports System.Threading

Module funcoes

    Public Function Pegar_Caminho() As String
        Dim NomeFicheiro As String = String.Empty
        Dim filedialog As New OpenFileDialog()
        Dim formatoIMG As String = "All Images|*.BMP;*.DIB;*.RLE;*.JPG;*.JPEG;*.JPE;*.JFIF;*.GIF;*.TIF;*.TIFF;*.PNG|" &
            "BMP Files: (*.BMP;*.DIB;*.RLE)|*.BMP;*.DIB;*.RLE|" &
            "JPEG Files: (*.JPG;*.JPEG;*.JPE;*.JFIF)|*.JPG;*.JPEG;*.JPE;*.JFIF|" &
            "GIF Files: (*.GIF)|*.GIF|" &
            "TIFF Files: (*.TIF;*.TIFF)|*.TIF;*.TIFF|" &
            "PNG Files: (*.PNG)|*.PNG|" &
            "All Files|*.*"
        Dim formatoDEF As String = "Imagens(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*"

        filedialog.InitialDirectory = My.Application.Info.DirectoryPath
        'filedialog.Filter = "jpeg files|*.jpg|All files (*.*)|*.*" ''Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*
        filedialog.Filter = formatoIMG
        filedialog.FilterIndex = 1
        filedialog.RestoreDirectory = True

        If filedialog.ShowDialog() = DialogResult.OK Then
            NomeFicheiro = filedialog.FileName
        End If

        Return NomeFicheiro

    End Function

    Public Function Ficheiro(caminho As String) As [Byte]()
        Dim file As New FileStream(caminho, FileMode.Open, FileAccess.Read)
        Dim BR_Ficheiro As New BinaryReader(file)
        Dim Meu_Fcheiro As Byte() = BR_Ficheiro.ReadBytes(CInt(file.Length))

        BR_Ficheiro.Close()
        file.Close()

        Return Meu_Fcheiro
    End Function

#Region " ResizeImage "
    Public Function ResizeImage(SourceImage As Drawing.Image, TargetWidth As Int32, TargetHeight As Int32) As Drawing.Bitmap
        Dim bmSource = New Drawing.Bitmap(SourceImage)

        Return ResizeImage(bmSource, TargetWidth, TargetHeight)
    End Function

    Public Function ResizeImage(bmSource As Drawing.Bitmap, TargetWidth As Int32, TargetHeight As Int32) As Drawing.Bitmap
        Dim bmDest As New Drawing.Bitmap(TargetWidth, TargetHeight, Drawing.Imaging.PixelFormat.Format32bppArgb)

        Dim nSourceAspectRatio = bmSource.Width / bmSource.Height
        Dim nDestAspectRatio = bmDest.Width / bmDest.Height

        Dim NewX = 0
        Dim NewY = 0
        Dim NewWidth = bmDest.Width
        Dim NewHeight = bmDest.Height

        If nDestAspectRatio = nSourceAspectRatio Then
            'same ratio
        ElseIf nDestAspectRatio > nSourceAspectRatio Then
            'Source is taller
            NewWidth = Convert.ToInt32(Math.Floor(nSourceAspectRatio * NewHeight))
            NewX = Convert.ToInt32(Math.Floor((bmDest.Width - NewWidth) / 2))
        Else
            'Source is wider
            NewHeight = Convert.ToInt32(Math.Floor((1 / nSourceAspectRatio) * NewWidth))
            NewY = Convert.ToInt32(Math.Floor((bmDest.Height - NewHeight) / 2))
        End If

        Using grDest = Drawing.Graphics.FromImage(bmDest)
            With grDest
                .CompositingQuality = Drawing.Drawing2D.CompositingQuality.HighQuality
                .InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
                .PixelOffsetMode = Drawing.Drawing2D.PixelOffsetMode.HighQuality
                .SmoothingMode = Drawing.Drawing2D.SmoothingMode.AntiAlias
                .CompositingMode = Drawing.Drawing2D.CompositingMode.SourceOver

                .DrawImage(bmSource, NewX, NewY, NewWidth, NewHeight)
            End With
        End Using

        Return bmDest
    End Function
#End Region

#Region "Inverter / Clarear / Escurecer"
    ''#####################################################

    Public Function inverterImagem(ByRef imagem As Bitmap)
        For X As Integer = 0 To (imagem.Width) - 1
            For Y As Integer = 0 To (imagem.Height) - 1
                Dim C As Color = imagem.GetPixel(X, Y)
                imagem.SetPixel(X, Y, Color.FromArgb(C.A, 255 - C.R, 255 - C.G, 255 - C.B))
            Next
        Next
        Return imagem
    End Function

    Public Function clarearImagem(ByRef imagem As Bitmap)
        For H As Integer = 0 To imagem.Width - 1
            For V As Integer = 0 To imagem.Height - 1

                Dim C As Color = imagem.GetPixel(H, V)
                Dim R As Integer = C.R + 10
                Dim G As Integer = C.G + 10
                Dim B As Integer = C.B + 10

                If R > 255 Then R = 255
                If G > 255 Then G = 255
                If B > 255 Then B = 255

                imagem.SetPixel(H, V, Color.FromArgb(C.A, R, G, B))

            Next
        Next

        Return imagem
    End Function

    Public Function escurecerImagem(ByRef imagem As Bitmap)
        For H As Integer = 0 To imagem.Width - 1
            For V As Integer = 0 To imagem.Height - 1

                Dim C As Color = imagem.GetPixel(H, V)
                Dim R As Integer = C.R - 10
                Dim G As Integer = C.G - 10
                Dim B As Integer = C.B - 10

                If R < 0 Then R = 0
                If G < 0 Then G = 0
                If B < 0 Then B = 0

                imagem.SetPixel(H, V, Color.FromArgb(C.A, R, G, B))

            Next
        Next

        Return imagem
    End Function

    ''#####################################################
#End Region

#Region "Preto e Branco"
    ''#####################################################

    '' Imagem coloria para tons de cinza: (padrão)
    Public Function imgPB_1(image As Bitmap)
        Dim bm As New Bitmap(image)
        Dim X As Integer
        Dim Y As Integer
        Dim clr As Integer

        For X = 0 To bm.Width - 1
            For Y = 0 To bm.Height - 1
                clr = (CInt(bm.GetPixel(X, Y).R) + bm.GetPixel(X, Y).G + bm.GetPixel(X, Y).B) \ 3
                bm.SetPixel(X, Y, Color.FromArgb(clr, clr, clr))
            Next Y
        Next X

        Return bm

    End Function

    '' Imagem coloria para tons de cinza: (MAIS RAPIDA)
    Public Function imgPB_2(ByRef img As Image)
        Try
            Dim grayscale As New Imaging.ColorMatrix(New Single()() _
        {
            New Single() {0.299, 0.299, 0.299, 0, 0},
            New Single() {0.587, 0.587, 0.587, 0, 0},
            New Single() {0.114, 0.114, 0.114, 0, 0},
            New Single() {0, 0, 0, 1, 0},
            New Single() {0, 0, 0, 0, 1}
        })

            Dim bmp As New Bitmap(img)
            Dim imgattr As New Imaging.ImageAttributes()
            imgattr.SetColorMatrix(grayscale)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.DrawImage(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, imgattr)
            End Using
            ''img = bmp
            Return bmp
        Catch ex As Exception

        End Try

    End Function

    '' Imagem colorida para tons de cinza (LENTA)
    Public Function imgPB_3(ByRef I As Bitmap)
        Dim x As Integer
        Dim y As Integer
        Dim red As Byte
        Dim green As Byte
        Dim blue As Byte

        For x = 0 To I.Width - 1
            For y = 0 To I.Height - 1
                red = I.GetPixel(x, y).R
                green = I.GetPixel(x, y).G
                blue = I.GetPixel(x, y).B
                I.SetPixel(x, y, Color.FromArgb(blue, blue, blue))
            Next
        Next
        Return I

    End Function

    ''###################################################################################
#End Region

    '' DIFERENCA ENTRE DUAS IMAGENS
    Public Function Diferenca(ByRef img1 As Bitmap, ByRef img2 As Bitmap)
        If img1.Width = img2.Width Then

            Dim thisOne As Bitmap = imgPB_2(img1)
            Dim theOtherOne As Bitmap = imgPB_2(img2)
            Dim differences(,) As Byte = New Byte(img1.Width, img1.Height) {}
            Dim t1, t2 As Color
            Dim w, z As Integer

            For y As Integer = 0 To img1.Height - 1
                For x As Integer = 0 To img1.Width - 1
                    t1 = img1.GetPixel(x, y)
                    t2 = img2.GetPixel(x, y)
                    w = (CInt(t1.R) + CInt(t1.G) + CInt(t1.B)) / 3
                    z = (CInt(t2.R) + CInt(t2.G) + CInt(t2.B)) / 3

                    differences(x, y) = Math.Abs(w - z)

                Next
            Next
            Dim pxDif As ULong
            Dim pxTotal As ULong = img1.Height * img1.Width

            For y As Integer = 0 To img1.Height
                For x As Integer = 0 To img1.Width
                    pxDif += differences(x, y)
                Next
            Next
            pxDif /= 255

            Dim pxPorc As Double = (100 * pxDif) / pxTotal

            thisOne.Dispose()
            theOtherOne.Dispose()
            Return pxPorc

        Else : MsgBox("Imagens diferentes")

        End If

        Return vbNull
    End Function

End Module
