﻿Imports System.Console
Imports System.ConsoleColor

Module SunnysBigAdventure
    Structure Point
        Public Sub New(x As Integer, y As Integer)
            Me.X = x
            Me.Y = y
        End Sub
        Public ReadOnly Property X As Integer
        Public ReadOnly Property Y As Integer
        Public Overrides Function ToString() As String
            Return $"({X}, {Y})"
        End Function
    End Structure
    Structure Rectangle
        Public Sub New(top As Integer, left As Integer, width As Integer, height As Integer)
            Me.New(New Point(top, left), width, height)
        End Sub
        Public Sub New(topLeft As Point, bottomRight As Point)
            Me.New(topLeft, bottomRight.X - topLeft.X + 1, bottomRight.Y - topLeft.Y + 1)
        End Sub
        Public Sub New(topLeft As Point, width As Integer, height As Integer)
            Me.TopLeft = topLeft
            Me.Width = width
            Me.Height = height
        End Sub
        Public ReadOnly Property TopLeft As Point
        Public ReadOnly Property Width As Integer
        Public ReadOnly Property Height As Integer
        Public ReadOnly Property Left As Integer
            Get
                Return TopLeft.X
            End Get
        End Property
        Public ReadOnly Property Right As Integer
            Get
                Return TopLeft.X + Width - 1
            End Get
        End Property
        Public ReadOnly Property Top As Integer
            Get
                Return TopLeft.Y
            End Get
        End Property
        Public ReadOnly Property Bottom As Integer
            Get
                Return TopLeft.Y + Height - 1
            End Get
        End Property
        Public Overrides Function ToString() As String
            Return $"({Left}, {Top}) to ({Right}, {Bottom})"
        End Function
    End Structure

    Property CursorPosition As Point
        Get
            Return New Point(CursorLeft, CursorTop)
        End Get
        Set(value As Point)
            SetCursorPosition(value.X, value.Y)
        End Set
    End Property

    ReadOnly Rectangles As New List(Of Rectangle)
    ReadOnly Sprites As New List(Of (Point As Point, Display As String, Color As ConsoleColor))

    Sub Redraw()
        ResetColor()
        For Each rect In Rectangles
            CursorPosition = rect.TopLeft
            Write("┏"c)
            For i = 1 To rect.Width - 2
                Write("━"c)
            Next
            Write("┓"c)
            For y = 1 To rect.Height - 2
                SetCursorPosition(rect.Left, y)
                Write("┃"c)
                SetCursorPosition(rect.Right, y)
                Write("┃"c)
            Next
            SetCursorPosition(rect.Left, rect.Bottom)
            Write("┗"c)
            For i = 1 To rect.Width - 2
                Write("━"c)
            Next
            Write("┛"c)
        Next
        For Each sprite In Sprites
            CursorPosition = sprite.Point
            ForegroundColor = sprite.Color
            Write(sprite.Display)
        Next
    End Sub

    Sub Main()
        OutputEncoding = Text.Encoding.UTF8
        WindowWidth = 64
        WindowHeight = 48
        Rectangles.Add(New Rectangle(New Point(0, 0), 2, 2))
        Sprites.Add((New Point(3, 3), "☺", White))
        Sprites.Add((New Point(3, 4), "☹", White))
        Sprites.Add((New Point(3, 5), "😃", White))
        Sprites.Add((New Point(3, 6), "☼", White))
        Redraw()
        ReadKey(True)
    End Sub
End Module
