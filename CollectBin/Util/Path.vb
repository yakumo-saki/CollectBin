''' <summary>
''' パスに関するユーティリティクラス
''' </summary>
''' <remarks></remarks>
Public Class Path

    ''' <summary>
    ''' インスタンス化不可
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub New()
    End Sub

    ''' <summary>
    ''' ディレクトリが存在しなければ作成する
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Sub CreateDirectoryIfNone(path As String)

        If My.Computer.FileSystem.DirectoryExists(path) = False Then
            My.Computer.FileSystem.CreateDirectory(path)
        End If

    End Sub

    ''' <summary>
    ''' プログラムのEXEが格納されているディレクトリ名を返します
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Function GetAppPath() As String
        Dim exePath = System.Reflection.Assembly.GetExecutingAssembly().Location
        Dim file = New IO.FileInfo(exePath)

        Return file.DirectoryName

    End Function

End Class
