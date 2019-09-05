Imports System.Console
Imports System.Collections.ObjectModel
Imports Unicode = System.Text.UnicodeEncoding
Imports SBA

Module SunnysBigAdventure
#Region "Structures"
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
        Function SafeContains(point As Point) As Boolean
            Dim Near = Function(obj As Integer, border As Integer) border - 1 <= obj AndAlso obj <= border + 1
            Return Near(Left, point.Left) And Near(Right, point.Left) And Near(Top, point.Top) And Near(Bottom, point.Top)
        End Function
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
#End Region
#Region "Helpers"
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
#End Region
#Region "Entity Classes"
    MustInherit Class Entity
        Sub New(entities As ICollection(Of Entity))
            entities.Add(Me)
        End Sub
        MustOverride Function Contains(point As Point) As Boolean
        Protected MustOverride Property InnerPosition As Point?
        Public Property Position As Point?
            Get
                Return InnerPosition
            End Get
            Set(value As Point?)
                If value IsNot Nothing Then
                    For Each entity In CurrentRegion.Entities
                        If Me IsNot entity AndAlso entity.Contains(value.GetValueOrDefault()) Then Return
                    Next
                End If
                RedrawAt(value)
                InnerPosition = value
            End Set
        End Property
        Protected MustOverride Sub RedrawAt(newPosition As Point?)
        Public Sub GoUp()
            IfHasValue(Position, Sub(point) Position = New Point(point.Left, Math.Max(point.Top - 1, 0)))
        End Sub
        Public Sub GoDown()
            IfHasValue(Position, Sub(point) Position = New Point(point.Left, Math.Min(BufferHeight, point.Top + 1)))
        End Sub
        Public Sub GoLeft()
            IfHasValue(Position, Sub(point) Position = New Point(Math.Max(point.Left - 1, 0), point.Top))
        End Sub
        Public Sub GoRight()
            IfHasValue(Position, Sub(point) Position = New Point(Math.Min(point.Left + 1, BufferWidth), point.Top))
        End Sub
    End Class
    Class SpriteEntity
        Inherits Entity
        Public Sub New(entities As ICollection(Of Entity), sprite As Sprite)
            MyBase.New(entities)
            _sprite = sprite
        End Sub
        Protected Overrides Property InnerPosition As Point?
        Dim _sprite As Sprite
        Public Overrides Function Contains(point As Point) As Boolean
            Return IfHasValue(Position, Function(pos) New Rectangle(point, 1, 1).SafeContains(point), False)
        End Function
        Protected Overrides Sub RedrawAt(newPosition As Point?)
            RedrawAt(newPosition, _sprite)
        End Sub
        Protected Overloads Sub RedrawAt(newPosition As Point?, newSprite As Sprite)
            WriteAt(Position, Empty_)
            WriteAt(newPosition, newSprite)
        End Sub
        Public Property Sprite As Sprite
            Get
                Return _sprite
            End Get
            Set(value As Sprite)
                RedrawAt(Position, value)
                _sprite = value
            End Set
        End Property
    End Class
    Class RectangleEntity
        Inherits Entity
        Public Sub New(entities As ICollection(Of Entity), rect As Rectangle)
            MyBase.New(entities)
            Rectangle = rect
        End Sub
        Dim _rectangle As Rectangle?
        Public Property Rectangle As Rectangle?
            Get
                Return _rectangle
            End Get
            Set(value As Rectangle?)
                RedrawAt(value)
                _rectangle = value
            End Set
        End Property
        Public Overrides Function Contains(point As Point) As Boolean
            Return IfHasValue(Rectangle, Function(rect) rect.SafeContains(point), False)
        End Function
        Protected Overrides Property InnerPosition As Point?
            Get
                Return Rectangle?.TopLeft
            End Get
            Set(value As Point?)
                Rectangle = IfHasValue(value, Function(point) IfHasValue(Rectangle,
                        Function(rect) New Rectangle(point, rect.Width, rect.Height),
                        New Rectangle(point, 1, 1)), New Rectangle?())
            End Set
        End Property
        Protected Overrides Sub RedrawAt(newPosition As Point?)
        End Sub
        Overloads Sub RedrawAt(newRect As Rectangle?)
            Draw(Rectangle, Empty, Empty, Empty, Empty, Empty, Empty)
            Draw(newRect, "━"c, "┃"c, "┏"c, "┓"c, "┗"c, "┛"c)
        End Sub
        Sub Draw(rectangle As Rectangle?, horizontal As Char, vertical As Char, topLeft As Char, topRight As Char, bottomLeft As Char, bottomRight As Char)
            IfHasValue(rectangle,
                       Sub(rect)
                           CursorPosition = rect.TopLeft
                           Write(topLeft)
                           For i = 1 To rect.Width - 2
                               Write(horizontal)
                           Next
                           Write(topRight)
                           For y = 1 To rect.Height - 2
                               SetCursorPosition(rect.Left, y)
                               Write(vertical)
                               SetCursorPosition(rect.Right, y)
                               Write(vertical)
                           Next
                           SetCursorPosition(rect.Left, rect.Bottom)
                           Write(bottomLeft)
                           For i = 1 To rect.Width - 2
                               Write(bottomRight)
                           Next
                           Write(bottomRight)
                       End Sub)
        End Sub
    End Class
    Class TextEntity
        Inherits Entity
        Public Sub New(entities As ICollection(Of Entity), text As String)
            MyBase.New(entities)
            Me.Text = text
        End Sub
        Protected Overrides Property InnerPosition As Point?
        Dim _text As String
        Public Property Text As String
            Get
                Return _text
            End Get
            Set(value As String)
                RedrawAt(Position, value)
                _text = value
            End Set
        End Property
        Public Overrides Function Contains(point As Point) As Boolean
            Return IfHasValue(Position, Function(pos) New Rectangle(pos, Text.Length, 1).SafeContains(point), False)
        End Function
        Protected Overrides Sub RedrawAt(newPosition As Point?)
            RedrawAt(newPosition, Text)
        End Sub
        Protected Overloads Sub RedrawAt(newPosition As Point?, newText As String)
            IfHasValue(Position, Sub(point)
                                     ResetColor()
                                     CursorPosition = point
                                     For i = 1 To Text.Length
                                         Write(Empty)
                                     Next
                                 End Sub)
            IfHasValue(newPosition, Sub(point)
                                        ResetColor()
                                        CursorPosition = point
                                        Write(newText)
                                    End Sub)
        End Sub
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
    ReadOnly GlobalEntities As New List(Of Entity)
    Public ReadOnly Sunny_ As New Sprite("☺"c)
    Public ReadOnly Sunny_Angry As New Sprite("☹"c, ConsoleColor.Red)
    Public ReadOnly Sunny As New SpriteEntity(GlobalEntities, Sunny_)

    Public ReadOnly Sun_ As New Sprite("☼"c, ConsoleColor.Yellow)
    Public ReadOnly Sun As New SpriteEntity(GlobalEntities, Sun_)

    Public ReadOnly Horsey_ As New Sprite("♘"c, ConsoleColor.Magenta)
    Public ReadOnly Horsey_Dead As New Sprite("♞"c, ConsoleColor.DarkMagenta)
    Public ReadOnly Horsey As New SpriteEntity(GlobalEntities, Horsey_)

    Public ActiveEntity As SpriteEntity = Sunny
#End Region
#Region "Regions"
    Dim CurrentRegion As Region = New Region1()
    MustInherit Class Region
        Sub Go(region As Func(Of Region))
            If region IsNot Nothing Then
                For Each entity In WriteEntities
                    entity.Position = Nothing
                Next
                CurrentRegion = region()
            End If
        End Sub
        Public Sub GoLeft()
            Go(Left)
        End Sub
        Public Sub GoRight()
            Go(Right)
        End Sub
        Protected MustOverride ReadOnly Property Left As Func(Of Region)
        Protected MustOverride ReadOnly Property Right As Func(Of Region)
        Protected ReadOnly WriteEntities As New List(Of Entity)(GlobalEntities)
        Public ReadOnly Entities As New ReadOnlyCollection(Of Entity)(WriteEntities)
    End Class
    Class Region1
        Inherits Region
        Sub New()

        End Sub
        Public ReadOnly Rect As New RectangleEntity(WriteEntities, New Rectangle(0, 0, 2, 2))
        Public ReadOnly SBA As New TextEntity(WriteEntities, "SBA")
        Protected Overrides ReadOnly Property Left As Func(Of Region) = Nothing
        Protected Overrides ReadOnly Property Right As Func(Of Region) = Function() New Region1()
    End Class
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