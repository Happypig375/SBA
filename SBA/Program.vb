Imports System.Console
Imports System.Collections.ObjectModel
Imports Unicode = System.Text.UnicodeEncoding
Imports SBA

Module SunnysBigAdventure
#Region "Foundation"
    Structure Point
        Public Sub New(left As Integer, top As Integer)
            Me.Left = left
            Me.Top = top
        End Sub
        Public ReadOnly Property Left As Integer
        Public ReadOnly Property Top As Integer
        Public Overrides Function ToString() As String
            Return $"({Left}, {Top})"
        End Function
    End Structure
    Structure Rectangle
        Public Sub New(top As Integer, left As Integer, width As Integer, height As Integer)
            Me.New(New Point(top, left), width, height)
        End Sub
        Public Sub New(topLeft As Point, bottomRight As Point)
            Me.New(topLeft, bottomRight.Left - topLeft.Left + 1, bottomRight.Top - topLeft.Top + 1)
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
                Return TopLeft.Left
            End Get
        End Property
        Public ReadOnly Property Right As Integer
            Get
                Return TopLeft.Left + Width - 1
            End Get
        End Property
        Public ReadOnly Property Top As Integer
            Get
                Return TopLeft.Top
            End Get
        End Property
        Public ReadOnly Property Bottom As Integer
            Get
                Return TopLeft.Top + Height - 1
            End Get
        End Property
        Function Contains(point As Point) As Boolean
            Return Left <= point.Left And point.Left <= Right And Top <= point.Top And point.Top <= Bottom
        End Function
        ReadOnly Property SafeBounds As Rectangle ' Prevent overwriting adjacent sprites
            Get
                Return New Rectangle(Left - 1, Top, 3, 1)
            End Get
        End Property
        Public Overrides Function ToString() As String
            Return $"({Left}, {Top}) to ({Right}, {Bottom})"
        End Function
    End Structure
    Structure Sprite
        Public Sub New(display As Char, Optional color As ConsoleColor = ConsoleColor.White) ' Consoles don't support surrogate pairs
            Me.Display = display
            Me.Color = color
        End Sub
        Public ReadOnly Property Display As Char
        Public ReadOnly Property Color As ConsoleColor
    End Structure
    Property CursorPosition As Point
        Get
            Return New Point(CursorLeft, CursorTop)
        End Get
        Set(value As Point)
            SetCursorPosition(value.Left, value.Top)
        End Set
    End Property
    Sub IfHasValue(Of T As Structure)(nullable As T?, f As Action(Of T))
        If nullable.HasValue Then f(nullable.GetValueOrDefault())
    End Sub
    Function IfHasValue(Of T As Structure, TReturn)(nullable As T?, f As Func(Of T, TReturn), defaultValue As TReturn) As TReturn
        Return If(nullable.HasValue, f(nullable.GetValueOrDefault()), defaultValue)
    End Function
    Function ReadKey(timeout As TimeSpan) As ConsoleKey?
        If KeyAvailable Then Return Console.ReadKey(True).Key
        Dim beginWait = Date.Now
        While Not KeyAvailable And Date.Now.Subtract(beginWait) < timeout
            Threading.Thread.Sleep(100)
            If KeyAvailable Then Return Console.ReadKey(True).Key
        End While
        Return Nothing
    End Function
    Sub WriteAt(position As Point?, sprite As Sprite)
        If position.HasValue Then
            CursorPosition = position.GetValueOrDefault()
            ForegroundColor = sprite.Color
            Write(sprite.Display)
        End If
    End Sub
    Interface IEntity
        Function Contains(point As Point) As Boolean
    End Interface
    Interface IMobileEntity
        Inherits IEntity
        Sub GoUp()
        Sub GoDown()
        Sub GoLeft()
        Sub GoRight()
    End Interface
    Class SpriteEntity
        Implements IMobileEntity
        Public Sub New(sprite As Sprite)
            _sprite = sprite
        End Sub
        Dim _position As Point?
        Dim _sprite As Sprite
        Public Function Contains(point As Point) As Boolean Implements IEntity.Contains
            Return IfHasValue(_position, Function(pos) New Rectangle(point, 1, 1).SafeBounds.Contains(point), False)
        End Function
        Sub Draw(newPosition As Point?)
            WriteAt(_position, Empty_)
            WriteAt(newPosition, _sprite)
        End Sub
        Public Property Sprite As Sprite
            Get
                Return _sprite
            End Get
            Set(value As Sprite)
                _sprite = value
                Draw(_position)
            End Set
        End Property
        Public Property Position As Point?
            Get
                Return _position
            End Get
            Set(value As Point?)
                If value IsNot Nothing Then
                    For Each entity In Entities
                        If Me IsNot entity AndAlso entity.Contains(value.GetValueOrDefault()) Then Return
                    Next
                End If
                Draw(value)
                _position = value
            End Set
        End Property
        Public Sub GoUp() Implements IMobileEntity.GoUp
            IfHasValue(Position, Sub(point) Position = New Point(point.Left, Math.Max(point.Top - 1, 0)))
        End Sub
        Public Sub GoDown() Implements IMobileEntity.GoDown
            IfHasValue(Position, Sub(point) Position = New Point(point.Left, Math.Min(BufferHeight, point.Top + 1)))
        End Sub
        Public Sub GoLeft() Implements IMobileEntity.GoLeft
            IfHasValue(Position, Sub(point) Position = New Point(Math.Max(point.Left - 1, 0), point.Top))
        End Sub
        Public Sub GoRight() Implements IMobileEntity.GoRight
            IfHasValue(Position, Sub(point) Position = New Point(Math.Min(point.Left + 1, BufferWidth), point.Top))
        End Sub
    End Class
    Class RectangleEntity
        Implements IEntity
        Public Property Rectangle As Rectangle
        Sub New(rect As Rectangle)
            Rectangle = rect
            Draw()
        End Sub
        Public Function Contains(point As Point) As Boolean Implements IEntity.Contains
            Return Rectangle.Contains(point)
        End Function
        Sub Draw(Optional horizontal As Char = "━"c, Optional vertical As Char = "┃"c,
                 Optional topLeft As Char = "┏"c, Optional topRight As Char = "┓"c,
                 Optional bottomLeft As Char = "┗"c, Optional bottomRight As Char = "┛"c)
            CursorPosition = Rectangle.TopLeft
            Write(topLeft)
            For i = 1 To Rectangle.Width - 2
                Write(horizontal)
            Next
            Write(topRight)
            For y = 1 To Rectangle.Height - 2
                SetCursorPosition(Rectangle.Left, y)
                Write(vertical)
                SetCursorPosition(Rectangle.Right, y)
                Write(vertical)
            Next
            SetCursorPosition(Rectangle.Left, Rectangle.Bottom)
            Write(bottomLeft)
            For i = 1 To Rectangle.Width - 2
                Write(bottomRight)
            Next
            Write(bottomRight)
        End Sub
    End Class
    Class TextEntity
        Implements IEntity
        Public Property Text As String
        Public Property Position As Point?
        Sub New(text As String)
            Me.Text = text
        End Sub
        Public Function Contains(point As Point) As Boolean Implements IEntity.Contains
            Return IfHasValue(Position, Function(pos) New Rectangle(pos, Text.Length, 1).SafeBounds.Contains(point), False)
        End Function
    End Class
#End Region
#Region "Entities"
    'Unicode: 
    '1.1☺☹☠❣❤✌☝✍♨✈⌛⌚☀☁☂❄☃☄♠♥♦♣♟☎⌨✉✏✒✂☢☣↗➡↘↙↖↕↔↩↪✡☸☯✝☦☪☮♈♉♊♋♌♍♎♏♐♑♒♓▶◀♀♂☑✔✖✳✴❇‼〰©®™Ⓜ
    '1.1㊗㊙▪▫☜♅♪♜☌♘☛♞☵☒♛♢✎‍♡☼☴♆☲☇♇☏☨☧☤☥♭☭☽☾❥☍☋☊☬♧☉#☞☶♁♤☷✐♮♖★♝*☰☫♫♙♃☚♬☩♄☓♯☟☈☻☱♕☳♔♩♚♗☡☐
    '3.0⁉♱♰☙
    '3.2⤴⤵♻〽◼◻◾◽☖♷⚁⚄⚆⚈♼☗♵⚉⚀⚇♹♲♸⚂♺♴⚅♳♽⚃♶
    '4.0☕☔⚡⚠⬆⬇⬅⏏⚏⚋⚎⚑⚊⚍⚐⚌
    '4.1☘⚓⚒⚔⚙⚖⚗⚰⚱♿⚛⚕♾⚜⚫⚪⚩⚭⚢⚥⚘⚤⚦⚨⚣⚬⚮⚚⚯⚧
    '5.0⚲
    '5.1⭐⬛⬜⛁⚶⚼⛃⚸⚴⚹⚳⚵⚻⚷⛀⚝⚺⛂
    '5.2⛷⛹⛰⛪⛩⛲⛺⛽⛵⛴⛅⛈⛱⛄⚽⚾⛳⛸⛑’⛏⛓⛔⭕❗⛟⛙⛞⛮⛶⛯⛜⛡⛿⛣⛊⛐⛾⛉⛚⛘⛠⛆⛝⛌⛕⛬⛍⛫⛖⚞⛨⚟⛻⛋⛒⛛⛭⛇⛼⚿⛗
    '6.0✋✊⏳⏰⏱⏲✨⛎⏩⏭⏯⏪⏮⏫⏬✅❌❎➕➖➗➰➿❓❔❕⛧⛢⛤ // Right-Handed interlaced pentagram: ⛥ Left-Handed interlaced pentagram: ⛦
    '7.0⏸⏹⏺
    '10.₿
    Const Empty = " "c
    ReadOnly Empty_ As New Sprite(Empty)

    ReadOnly Sunny_ As New Sprite("☺"c)
    ReadOnly Sunny_Angry As New Sprite("☹"c, ConsoleColor.Red)
    ReadOnly Sunny As New SpriteEntity(Sunny_)

    ReadOnly Sun_ As New Sprite("☼"c, ConsoleColor.Yellow)
    ReadOnly Sun As New SpriteEntity(Sun_)

    ReadOnly Horsey_ As New Sprite("♘"c, ConsoleColor.Magenta)
    ReadOnly Horsey_Dead As New Sprite("♞"c, ConsoleColor.DarkMagenta)
    ReadOnly Horsey As New SpriteEntity(Horsey_)

    ReadOnly Rect As New RectangleEntity(New Rectangle(0, 0, 2, 2))

    Dim ActiveEntity As SpriteEntity = Sunny
    ReadOnly Entities As New List(Of IEntity)
#End Region

    Sub Main()
        If LargestWindowWidth < 48 Or LargestWindowHeight < 10 Then
            WriteLine("ERROR: Please decrease font size")
            Return
        End If
        OutputEncoding = New Unicode()
        WindowWidth = 48
        WindowHeight = 10
        CursorVisible = False
        Sunny.Position = New Point(3, 3)
        Sun.Position = New Point(3, 6)
        Horsey.Position = New Point(3, 8)
        While True
            Dim key = ReadKey(TimeSpan.FromSeconds(1))
            Select Case key
                Case ConsoleKey.LeftArrow : ActiveEntity.GoLeft()
                Case ConsoleKey.RightArrow : ActiveEntity.GoRight()
                Case ConsoleKey.UpArrow : ActiveEntity.GoUp()
                Case ConsoleKey.DownArrow : ActiveEntity.GoDown()
                Case Else
            End Select
            Debug.WriteLine(key)
        End While
    End Sub
End Module