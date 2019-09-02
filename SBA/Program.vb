Imports System.Console
Imports System.Drawing

Module SunnysBigAdventure
    Property CursorPosition As Point
        Get
            Return New Point(CursorLeft, CursorTop)
        End Get
        Set(value As Point)
            SetCursorPosition(value.X, value.Y)
        End Set
    End Property

    Dim Rectangles As New List(Of Rectangle)
    Dim Sprites As New List(Of (Point As Point, C As Char))

    Sub Redraw()
        For Each rect In Rectangles
            CursorPosition = rect.Location
            Dim innerRectWidth = rect.Width - 2
            Dim innerRectHeight = rect.Height - 2
            Write("┏"c)
            For i = 1 To innerRectWidth
                Write("━"c)
            Next
            Write("┓"c)
            For y = rect.Y + 1 To rect.Y + innerRectHeight - 1
                SetCursorPosition(rect.X, y)
                Write("┃"c)
                SetCursorPosition(rect.X + innerRectWidth + 1, y)
                Write("┃"c)
            Next
            SetCursorPosition(rect.X, rect.Y + innerRectHeight)
            Write("┗"c)
            For i = 1 To innerRectWidth
                Write("━"c)
            Next
            Write("┛"c)
        Next
        For Each sprite In Sprites
            CursorPosition = sprite.Point
            Write(sprite.C)
        Next
    End Sub

    Sub Main()
        OutputEncoding = Text.Encoding.UTF8
        Rectangles.Add(New Rectangle(0, 0, 3, 3))
        Sprites.Add((New Point(1, 1), "x"c))
        Redraw()
    End Sub
End Module
