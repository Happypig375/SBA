Imports System.Console
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
    Structure Sprite
        Public Sub New(display As Char, color As ConsoleColor) ' Consoles don't support surrogate pairs
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
            SetCursorPosition(value.X, value.Y)
        End Set
    End Property

    ReadOnly Happy As New Sprite("☺", White)
    ReadOnly Sad As New Sprite("☹", White)
    ReadOnly Sun As New Sprite("☼", Yellow)
    ReadOnly Evil As New Sprite("☼", Yellow)

    ReadOnly Rectangles As New List(Of Rectangle)
    ReadOnly Sprites As New List(Of (Point As Point, Sprite As Sprite))

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
            ForegroundColor = sprite.Sprite.Color
            Write(sprite.Sprite.Display)
        Next
    End Sub

    Sub Main()
        OutputEncoding = Text.Encoding.Unicode
        WindowWidth = 40
        WindowHeight = 20
        Rectangles.Add(New Rectangle(New Point(0, 0), 2, 2))
        Sprites.Add((New Point(3, 3), Happy))
        Sprites.Add((New Point(3, 4), Sad))
        Sprites.Add((New Point(3, 6), Sun))
        Redraw()
        Clear()
        Write("
// Unicode 1.1
☺ Smiling Face
☹ Frowning Face
☠ Skull and Crossbones
❣ Heavy Heart Exclamation
❤ Red Heart
✌ Victory Hand
☝ Index Pointing Up
✍ Writing Hand
♨ Hot Springs
✈ Airplane
⌛ Hourglass Done
⌚ Watch
☀ Sun
☁ Cloud
☂ Umbrella
❄ Snowflake
☃ Snowman
☄ Comet
♠ Spade Suit
♥ Heart Suit
♦ Diamond Suit
♣ Club Suit
♟ Chess Pawn
☎ Telephone
⌨ Keyboard
✉ Envelope
✏ Pencil
✒ Black Nib
✂ Scissors
☢ Radioactive
☣ Biohazard
↗ Up-Right Arrow
➡ Right Arrow
↘ Down-Right Arrow
↙ Down-Left Arrow
↖ Up-Left Arrow
↕ Up-Down Arrow
↔ Left-Right Arrow
↩ Right Arrow Curving Left
↪ Left Arrow Curving Right
✡ Star of David
☸ Wheel of Dharma
☯ Yin Yang
✝ Latin Cross
☦ Orthodox Cross
☪ Star and Crescent
☮ Peace Symbol
♈ Aries
♉ Taurus
♊ Gemini
♋ Cancer
♌ Leo
♍ Virgo
♎ Libra
♏ Scorpio
♐ Sagittarius
♑ Capricorn
♒ Aquarius
♓ Pisces
▶ Play Button
◀ Reverse Button
♀ Female Sign
♂ Male Sign
☑ Ballot Box With Check
✔ Heavy Check Mark
✖ Heavy Multiplication X
✳ Eight-Spoked Asterisk
✴ Eight-Pointed Star
❇ Sparkle
‼ Double Exclamation Mark
〰 Wavy Dash
© Copyright
® Registered
™ Trade Mark
Ⓜ Circled M
㊗ Japanese Congratulations Button
㊙ Japanese Secret Button
▪ Black Small Square
▫ White Small Square
☜ White Left Pointing Index
♅ Uranus
♪ Eighth Note
♜ Black Chess Rook
☌ Conjunction
♘ White Chess Knight
☛ Black Right Pointing Index
♞ Black Chess Knight
☵ Trigram for Water
☒ Ballot Box with X
♛ Black Chess Queen
♢ White Diamond Suit
✎ Lower Right Pencil
‍ Zero Width Joiner
♡ White Heart Suit
☼ White Sun with Rays
☴ Trigram for Wind
♆ Neptune
☲ Trigram for Fire
☇ Lightning
♇ Pluto
☏ White Telephone
5 Digit Five
2 Digit Two
☨ Cross of Lorraine
☧ Chi Rho
☤ Caduceus
☥ Ankh
♭ Music Flat Sign
☭ Hammer and Sickle
☽ First Quarter Moon
☾ Last Quarter Moon
❥ Rotated Heavy Black Heart Bullet
☍ Opposition
☋ Descending Node
☊ Ascending Node
☬ Adi Shakti
♧ White Club Suit
☉ Sun
# Number Sign
☞ White Right Pointing Index
☶ Trigram for Mountain
♁ Earth
♤ White Spade Suit
☷ Trigram for Earth
✐ Upper Right Pencil
9 Digit Nine
8 Digit Eight
♮ Music Natural Sign
4 Digit Four
1 Digit One
♖ White Chess Rook
★ Black Star
♝ Black Chess Bishop
3 Digit Three
* Asterisk
☰ Trigram for Heaven
☫ Farsi Symbol
♫ Beamed Eighth Notes
7 Digit Seven
♙ White Chess Pawn
0 Digit Zero
♃ Jupiter
☚ Black Left Pointing Index
♬ Beamed Sixteenth Notes
☩ Cross of Jerusalem
♄ Saturn
☓ Saltire
♯ Music Sharp Sign
☟ White Down Pointing Index
☈ Thunderstorm
☻ Black Smiling Face
☱ Trigram for Lake
♕ White Chess Queen
☳ Trigram for Thunder
6 Digit Six
♔ White Chess King
♩ Quarter Note
♚ Black Chess King
♗ White Chess Bishop
☡ Caution Sign
☐ Ballot Box

// Unicode 3.0
⁉ Exclamation Question Mark
#️⃣ Keycap Number Sign
*️⃣ Keycap Asterisk
0️⃣ Keycap Digit Zero
1️⃣ Keycap Digit One
2️⃣ Keycap Digit Two
3️⃣ Keycap Digit Three
4️⃣ Keycap Digit Four
5️⃣ Keycap Digit Five
6️⃣ Keycap Digit Six
7️⃣ Keycap Digit Seven
8️⃣ Keycap Digit Eight
9️⃣ Keycap Digit Nine
ℹ Information
♱ East Syriac Cross
♰ West Syriac Cross
☙ Reversed Rotated Floral Heart Bullet
⃣ Combining Enclosing Keycap

// Unicode 3.2
⤴ Right Arrow Curving Up
⤵ Right Arrow Curving Down
♻ Recycling Symbol
〽 Part Alternation Mark
◼ Black Medium Square
◻ White Medium Square
◾ Black Medium-Small Square
◽ White Medium-Small Square
☖ White Shogi Piece
♷ Recycling Symbol for Type-5 Plastics
⚁ Die Face-2
⚄ Die Face-5
⚆ White Circle with Dot Right
⚈ Black Circle with White Dot Right
♼ Recycled Paper Symbol
☗ Black Shogi Piece
♵ Recycling Symbol for Type-3 Plastics
⚉ Black Circle with Two White Dots
⚀ Die Face-1
⚇ White Circle with Two Dots
♹ Recycling Symbol for Type-7 Plastics
♲ Universal Recycling Symbol
♸ Recycling Symbol for Type-6 Plastics
️ Variation Selector-16
⚂ Die Face-3
♺ Recycling Symbol for Generic Materials
♴ Recycling Symbol for Type-2 Plastics
⚅ Die Face-6
♳ Recycling Symbol for Type-1 Plastics
♽ Partially-Recycled Paper Symbol
⚃ Die Face-4
♶ Recycling Symbol for Type-4 Plastics

// Unicode 4.0
☕ Hot Beverage
☔ Umbrella With Rain Drops
⚡ High Voltage
⚠ Warning
⬆ Up Arrow
⬇ Down Arrow
⬅ Left Arrow
⏏ Eject Button
⚏ Digram for Greater Yin
⚋ Monogram for Yin
⚎ Digram for Lesser Yang
⚑ Black Flag
⚊ Monogram for Yang
⚍ Digram for Lesser Yin
⚐ White Flag
⚌ Digram for Greater Yang

// Unicode 4.1
☘ Shamrock
⚓ Anchor
⚒ Hammer and Pick
⚔ Crossed Swords
⚙ Gear
⚖ Balance Scale
⚗ Alembic
⚰ Coffin
⚱ Funeral Urn
♿ Wheelchair Symbol
⚛ Atom Symbol
⚕ Medical Symbol
♾ Infinity
⚜ Fleur-de-lis
⚫ Black Circle
⚪ White Circle
⚩ Horizontal Male with Stroke Sign
⚭ Marriage Symbol
⚢ Doubled Female Sign
⚥ Male and Female Sign
⚘ Flower
⚤ Interlocked Female and Male Sign
⚦ Male with Stroke Sign
⚨ Vertical Male with Stroke Sign
⚣ Doubled Male Sign
⚬ Medium Small White Circle
⚮ Divorce Symbol
⚚ Staff of Hermes
⚯ Unmarried Partnership Symbol
⚧️ Transgender Symbol

// Unicode 5.0
⚲ Neuter

// Unicode 5.1
🀜🀜🀜
⭐ Star
🀄 Mahjong Red Dragon
⬛ Black Large Square
⬜ White Large Square
🀫 Mahjong Tile Back
🀅 Mahjong Tile Green Dragon
🀞 Mahjong Tile Six of Circles
🀝 Mahjong Tile Five of Circles
⛁ White Draughts King
🀑 Mahjong Tile Two of Bamboos
🀛 Mahjong Tile Three of Circles
🀌 Mahjong Tile Six of Characters
🀍 Mahjong Tile Seven of Characters
🀠 Mahjong Tile Eight of Circles
⚶ Vesta
⚼ Sesquiquadrate
🀕 Mahjong Tile Six of Bamboos
🀓 Mahjong Tile Four of Bamboos
🀘 Mahjong Tile Nine of Bamboos
⛃ Black Draughts King
⚸ Black Moon Lilith
🀏 Mahjong Tile Nine of Characters
⚴ Pallas
⚹ Sextile
🀇 Mahjong Tile One of Characters
⚳ Ceres
⚵ Juno
🀦 Mahjong Tile Spring
🀩 Mahjong Tile Winter
🀢 Mahjong Tile Plum
🀙 Mahjong Tile One of Circles
⚻ Quincunx
🀗 Mahjong Tile Eight of Bamboos
🀃 Mahjong Tile North Wind
🀔 Mahjong Tile Five of Bamboos
🀒 Mahjong Tile Three of Bamboos
🀆 Mahjong Tile White Dragon
🀤 Mahjong Tile Bamboo
🀖 Mahjong Tile Seven of Bamboos
🀣 Mahjong Tile Orchid
🀈 Mahjong Tile Two of Characters
🀟 Mahjong Tile Seven of Circles
🀎 Mahjong Tile Eight of Characters
🀡 Mahjong Tile Nine of Circles
⚷ Chiron
⛀ White Draughts Man
🀥 Mahjong Tile Chrysanthemum
🀉 Mahjong Tile Three of Characters
🀂 Mahjong Tile West Wind
🀊 Mahjong Tile Four of Characters
⚝ Outlined White Star
🀨 Mahjong Tile Autumn
⚺ Semisextile
🀐 Mahjong Tile One of Bamboos
🀁 Mahjong Tile South Wind
🀪 Mahjong Tile Joker
🀜 Mahjong Tile Four of Circles
🀚 Mahjong Tile Two of Circles
߷ NKo Symbol Gbakurunen
🀋 Mahjong Tile Five of Characters
🀀 Mahjong Tile East Wind
⛂ Black Draughts Man
🀧 Mahjong Tile Summer

// Unicode 5.2
⛷ Skier
⛹ Person Bouncing Ball
⛰ Mountain
⛪ Church
⛩ Shinto Shrine
⛲ Fountain
⛺ Tent
⛽ Fuel Pump
⛵ Sailboat
⛴ Ferry
⛅ Sun Behind Cloud
⛈ Cloud With Lightning and Rain
⛱ Umbrella on Ground
⛄ Snowman Without Snow
⚽ Soccer Ball
⚾ Baseball
⛳ Flag in Hole
⛸ Ice Skate
⛑ Rescue Worker’s Helmet
⛏ Pick
⛓ Chains
⛔ No Entry
⭕ Heavy Large Circle
❗ Exclamation Mark
🅿 P Button
🈯 Japanese Reserved Button
🈚 Japanese Free of Charge Button
⛟ Black Truck
⛙ White Left Lane Merge
⛞ Falling Diagonal In White Circle In Black Square
⛮ Gear with Handles
⛶ Square Four Corners
⛯ Map Symbol for Lighthouse
⛜ Left Closed Entry
⛡ Restricted Left Entry-2
⛿ White Flag with Horizontal Middle Black Stripe
⛣ Heavy Circle with Stroke and Two Dots Above
⛊ Turned Black Shogi Piece
⛐ Car Sliding
⛾ Cup On Black Square
⛉ Turned White Shogi Piece
⛚ Drive Slow Sign
⛘ Black Left Lane Merge
⛠ Restricted Left Entry-1
⛆ Rain
⛝ Squared Saltire
⛌ Crossing Lanes
⛕ Alternate One-Way Left Way Traffic
⛬ Historic Site
⛍ Disabled Car
⛫ Castle
⛖ Black Two-Way Left Way Traffic
⚞ Three Lines Converging Right
⛨ Black Cross On Shield
⚟ Three Lines Converging Left
⛻ Japanese Bank Symbol
⛋ White Diamond In Square
⛒ Circled Crossing Lanes
⛛ Heavy White Down-Pointing Triangle
⛭ Gear Without Hub
⛇ Black Snowman
⛼ Headstone Graveyard Symbol
⚿ Squared Key
⛗ White Two-Way Left Way Traffic

// Unicode 6.0
😃 Grinning Face With Big Eyes
😄 Grinning Face With Smiling Eyes
😁 Beaming Face With Smiling Eyes
😆 Grinning Squinting Face
😅 Grinning Face With Sweat
😂 Face With Tears of Joy
😉 Winking Face
😊 Smiling Face With Smiling Eyes
😇 Smiling Face With Halo
😍 Smiling Face With Heart-Eyes
😘 Face Blowing a Kiss
😚 Kissing Face With Closed Eyes
😋 Face Savoring Food
😜 Winking Face With Tongue
😝 Squinting Face With Tongue
😐 Neutral Face
😶 Face Without Mouth
😏 Smirking Face
😒 Unamused Face
😌 Relieved Face
😔 Pensive Face
😪 Sleepy Face
😷 Face With Medical Mask
😵 Dizzy Face
😎 Smiling Face With Sunglasses
😲 Astonished Face
😳 Flushed Face
😨 Fearful Face
😰 Anxious Face With Sweat
😥 Sad but Relieved Face
😢 Crying Face
😭 Loudly Crying Face
😱 Face Screaming in Fear
😖 Confounded Face
😣 Persevering Face
😞 Disappointed Face
😓 Downcast Face With Sweat
😩 Weary Face
😫 Tired Face
😤 Face With Steam From Nose
😡 Pouting Face
😠 Angry Face
😈 Smiling Face With Horns
👿 Angry Face With Horns
💀 Skull
💩 Pile of Poo
👹 Ogre
👺 Goblin
👻 Ghost
👽 Alien
👾 Alien Monster
😺 Grinning Cat Face
😸 Grinning Cat Face With Smiling Eyes
😹 Cat Face With Tears of Joy
😻 Smiling Cat Face With Heart-Eyes
😼 Cat Face With Wry Smile
😽 Kissing Cat Face
🙀 Weary Cat Face
😿 Crying Cat Face
😾 Pouting Cat Face
🙈 See-No-Evil Monkey
🙉 Hear-No-Evil Monkey
🙊 Speak-No-Evil Monkey
💋 Kiss Mark
💌 Love Letter
💘 Heart With Arrow
💝 Heart With Ribbon
💖 Sparkling Heart
💗 Growing Heart
💓 Beating Heart
💞 Revolving Hearts
💕 Two Hearts
💟 Heart Decoration
💔 Broken Heart
💛 Yellow Heart
💚 Green Heart
💙 Blue Heart
💜 Purple Heart
💯 Hundred Points
💢 Anger Symbol
💥 Collision
💫 Dizzy
💦 Sweat Droplets
💨 Dashing Away
💣 Bomb
💬 Speech Balloon
💭 Thought Balloon
💤 Zzz
👋 Waving Hand
✋ Raised Hand
👌 OK Hand
👈 Backhand Index Pointing Left
👉 Backhand Index Pointing Right
👆 Backhand Index Pointing Up
👇 Backhand Index Pointing Down
👍 Thumbs Up
👎 Thumbs Down
✊ Raised Fist
👊 Oncoming Fist
👏 Clapping Hands
🙌 Raising Hands
👐 Open Hands
🙏 Folded Hands
💅 Nail Polish
💪 Flexed Biceps
👂 Ear
👃 Nose
👀 Eyes
👅 Tongue
👄 Mouth
👶 Baby
👦 Boy
👧 Girl
👱 Person: Blond Hair
👨 Man
👩 Woman
👴 Old Man
👵 Old Woman
🙍 Person Frowning
🙎 Person Pouting
🙅 Person Gesturing No
🙆 Person Gesturing OK
💁 Person Tipping Hand
🙋 Person Raising Hand
🙇 Person Bowing
👮 Police Officer
💂 Guard
👷 Construction Worker
👸 Princess
👳 Person Wearing Turban
👲 Man With Chinese Cap
👰 Bride With Veil
👼 Baby Angel
🎅 Santa Claus
💆 Person Getting Massage
💇 Person Getting Haircut
🚶 Person Walking
🏃 Person Running
💃 Woman Dancing
👯 People With Bunny Ears
🏇 Horse Racing
🏂 Snowboarder
🏄 Person Surfing
🚣 Person Rowing Boat
🏊 Person Swimming
🚴 Person Biking
🚵 Person Mountain Biking
🛀 Person Taking Bath
👭 Women Holding Hands
👫 Woman and Man Holding Hands
👬 Men Holding Hands
💏 Kiss
💑 Couple With Heart
👪 Family
👤 Bust in Silhouette
👥 Busts in Silhouette
👣 Footprints
🐵 Monkey Face
🐒 Monkey
🐶 Dog Face
🐕 Dog
🐩 Poodle
🐺 Wolf Face
🐱 Cat Face
🐈 Cat
🐯 Tiger Face
🐅 Tiger
🐆 Leopard
🐴 Horse Face
🐎 Horse
🐮 Cow Face
🐂 Ox
🐃 Water Buffalo
🐄 Cow
🐷 Pig Face
🐖 Pig
🐗 Boar
🐽 Pig Nose
🐏 Ram
🐑 Ewe
🐐 Goat
🐪 Camel
🐫 Two-Hump Camel
🐘 Elephant
🐭 Mouse Face
🐁 Mouse
🐀 Rat
🐹 Hamster Face
🐰 Rabbit Face
🐇 Rabbit
🐻 Bear Face
🐨 Koala
🐼 Panda Face
🐾 Paw Prints
🐔 Chicken
🐓 Rooster
🐣 Hatching Chick
🐤 Baby Chick
🐥 Front-Facing Baby Chick
🐦 Bird
🐧 Penguin
🐸 Frog Face
🐊 Crocodile
🐢 Turtle
🐍 Snake
🐲 Dragon Face
🐉 Dragon
🐳 Spouting Whale
🐋 Whale
🐬 Dolphin
🐟 Fish
🐠 Tropical Fish
🐡 Blowfish
🐙 Octopus
🐚 Spiral Shell
🐌 Snail
🐛 Bug
🐜 Ant
🐝 Honeybee
🐞 Lady Beetle
💐 Bouquet
🌸 Cherry Blossom
💮 White Flower
🌹 Rose
🌺 Hibiscus
🌻 Sunflower
🌼 Blossom
🌷 Tulip
🌱 Seedling
🌲 Evergreen Tree
🌳 Deciduous Tree
🌴 Palm Tree
🌵 Cactus
🌾 Sheaf of Rice
🌿 Herb
🍀 Four Leaf Clover
🍁 Maple Leaf
🍂 Fallen Leaf
🍃 Leaf Fluttering in Wind
🍇 Grapes
🍈 Melon
🍉 Watermelon
🍊 Tangerine
🍋 Lemon
🍌 Banana
🍍 Pineapple
🍎 Red Apple
🍏 Green Apple
🍐 Pear
🍑 Peach
🍒 Cherries
🍓 Strawberry
🍅 Tomato
🍆 Eggplant
🌽 Ear of Corn
🍄 Mushroom
🌰 Chestnut
🍞 Bread
🍖 Meat on Bone
🍗 Poultry Leg
🍔 Hamburger
🍟 French Fries
🍕 Pizza
🍳 Cooking
🍲 Pot of Food
🍱 Bento Box
🍘 Rice Cracker
🍙 Rice Ball
🍚 Cooked Rice
🍛 Curry Rice
🍜 Steaming Bowl
🍝 Spaghetti
🍠 Roasted Sweet Potato
🍢 Oden
🍣 Sushi
🍤 Fried Shrimp
🍥 Fish Cake With Swirl
🍡 Dango
🍦 Soft Ice Cream
🍧 Shaved Ice
🍨 Ice Cream
🍩 Doughnut
🍪 Cookie
🎂 Birthday Cake
🍰 Shortcake
🍫 Chocolate Bar
🍬 Candy
🍭 Lollipop
🍮 Custard
🍯 Honey Pot
🍼 Baby Bottle
🍵 Teacup Without Handle
🍶 Sake
🍷 Wine Glass
🍸 Cocktail Glass
🍹 Tropical Drink
🍺 Beer Mug
🍻 Clinking Beer Mugs
🍴 Fork and Knife
🔪 Kitchen Knife
🌍 Globe Showing Europe-Africa
🌎 Globe Showing Americas
🌏 Globe Showing Asia-Australia
🌐 Globe With Meridians
🗾 Map of Japan
🌋 Volcano
🗻 Mount Fuji
🏠 House
🏡 House With Garden
🏢 Office Building
🏣 Japanese Post Office
🏤 Post Office
🏥 Hospital
🏦 Bank
🏨 Hotel
🏩 Love Hotel
🏪 Convenience Store
🏫 School
🏬 Department Store
🏭 Factory
🏯 Japanese Castle
🏰 Castle
💒 Wedding
🗼 Tokyo Tower
🗽 Statue of Liberty
🌁 Foggy
🌃 Night With Stars
🌄 Sunrise Over Mountains
🌅 Sunrise
🌆 Cityscape at Dusk
🌇 Sunset
🌉 Bridge at Night
🎠 Carousel Horse
🎡 Ferris Wheel
🎢 Roller Coaster
💈 Barber Pole
🎪 Circus Tent
🚂 Locomotive
🚃 Railway Car
🚄 High-Speed Train
🚅 Bullet Train
🚆 Train
🚇 Metro
🚈 Light Rail
🚉 Station
🚊 Tram
🚝 Monorail
🚞 Mountain Railway
🚋 Tram Car
🚌 Bus
🚍 Oncoming Bus
🚎 Trolleybus
🚐 Minibus
🚑 Ambulance
🚒 Fire Engine
🚓 Police Car
🚔 Oncoming Police Car
🚕 Taxi
🚖 Oncoming Taxi
🚗 Automobile
🚘 Oncoming Automobile
🚙 Sport Utility Vehicle
🚚 Delivery Truck
🚛 Articulated Lorry
🚜 Tractor
🚲 Bicycle
🚏 Bus Stop
🚨 Police Car Light
🚥 Horizontal Traffic Light
🚦 Vertical Traffic Light
🚧 Construction
🚤 Speedboat
🚢 Ship
💺 Seat
🚁 Helicopter
🚟 Suspension Railway
🚠 Mountain Cableway
🚡 Aerial Tramway
🚀 Rocket
⏳ Hourglass Not Done
⏰ Alarm Clock
⏱ Stopwatch
⏲ Timer Clock
🕛 Twelve O’Clock
🕧 Twelve-Thirty
🕐 One O’Clock
🕜 One-Thirty
🕑 Two O’Clock
🕝 Two-Thirty
🕒 Three O’Clock
🕞 Three-Thirty
🕓 Four O’Clock
🕟 Four-Thirty
🕔 Five O’Clock
🕠 Five-Thirty
🕕 Six O’Clock
🕡 Six-Thirty
🕖 Seven O’Clock
🕢 Seven-Thirty
🕗 Eight O’Clock
🕣 Eight-Thirty
🕘 Nine O’Clock
🕤 Nine-Thirty
🕙 Ten O’Clock
🕥 Ten-Thirty
🕚 Eleven O’Clock
🕦 Eleven-Thirty
🌑 New Moon
🌒 Waxing Crescent Moon
🌓 First Quarter Moon
🌔 Waxing Gibbous Moon
🌕 Full Moon
🌖 Waning Gibbous Moon
🌗 Last Quarter Moon
🌘 Waning Crescent Moon
🌙 Crescent Moon
🌚 New Moon Face
🌛 First Quarter Moon Face
🌜 Last Quarter Moon Face
🌝 Full Moon Face
🌞 Sun With Face
🌟 Glowing Star
🌠 Shooting Star
🌌 Milky Way
🌀 Cyclone
🌈 Rainbow
🌂 Closed Umbrella
🔥 Fire
💧 Droplet
🌊 Water Wave
🎃 Jack-O-Lantern
🎄 Christmas Tree
🎆 Fireworks
🎇 Sparkler
✨ Sparkles
🎈 Balloon
🎉 Party Popper
🎊 Confetti Ball
🎋 Tanabata Tree
🎍 Pine Decoration
🎎 Japanese Dolls
🎏 Carp Streamer
🎐 Wind Chime
🎑 Moon Viewing Ceremony
🎀 Ribbon
🎁 Wrapped Gift
🎫 Ticket
🏆 Trophy
🏀 Basketball
🏈 American Football
🏉 Rugby Football
🎾 Tennis
🎳 Bowling
🎣 Fishing Pole
🎽 Running Shirt
🎿 Skis
🎯 Direct Hit
🎱 Pool 8 Ball
🔮 Crystal Ball
🎮 Video Game
🎰 Slot Machine
🎲 Game Die
🃏 Joker
🎴 Flower Playing Cards
🎭 Performing Arts
🎨 Artist Palette
👓 Glasses
👔 Necktie
👕 T-Shirt
👖 Jeans
👗 Dress
👘 Kimono
👙 Bikini
👚 Woman’s Clothes
👛 Purse
👜 Handbag
👝 Clutch Bag
🎒 Backpack
👞 Man’s Shoe
👟 Running Shoe
👠 High-Heeled Shoe
👡 Woman’s Sandal
👢 Woman’s Boot
👑 Crown
👒 Woman’s Hat
🎩 Top Hat
🎓 Graduation Cap
💄 Lipstick
💍 Ring
💎 Gem Stone
🔇 Muted Speaker
🔈 Speaker Low Volume
🔉 Speaker Medium Volume
🔊 Speaker High Volume
📢 Loudspeaker
📣 Megaphone
📯 Postal Horn
🔔 Bell
🔕 Bell With Slash
🎼 Musical Score
🎵 Musical Note
🎶 Musical Notes
🎤 Microphone
🎧 Headphone
📻 Radio
🎷 Saxophone
🎸 Guitar
🎹 Musical Keyboard
🎺 Trumpet
🎻 Violin
📱 Mobile Phone
📲 Mobile Phone With Arrow
📞 Telephone Receiver
📟 Pager
📠 Fax Machine
🔋 Battery
🔌 Electric Plug
💻 Laptop Computer
💽 Computer Disk
💾 Floppy Disk
💿 Optical Disk
📀 DVD
🎥 Movie Camera
🎬 Clapper Board
📺 Television
📷 Camera
📹 Video Camera
📼 Videocassette
🔍 Magnifying Glass Tilted Left
🔎 Magnifying Glass Tilted Right
💡 Light Bulb
🔦 Flashlight
🏮 Red Paper Lantern
📔 Notebook With Decorative Cover
📕 Closed Book
📖 Open Book
📗 Green Book
📘 Blue Book
📙 Orange Book
📚 Books
📓 Notebook
📒 Ledger
📃 Page With Curl
📜 Scroll
📄 Page Facing Up
📰 Newspaper
📑 Bookmark Tabs
🔖 Bookmark
💰 Money Bag
💴 Yen Banknote
💵 Dollar Banknote
💶 Euro Banknote
💷 Pound Banknote
💸 Money With Wings
💳 Credit Card
💹 Chart Increasing With Yen
💱 Currency Exchange
💲 Heavy Dollar Sign
📧 E-Mail
📨 Incoming Envelope
📩 Envelope With Arrow
📤 Outbox Tray
📥 Inbox Tray
📦 Package
📫 Closed Mailbox With Raised Flag
📪 Closed Mailbox With Lowered Flag
📬 Open Mailbox With Raised Flag
📭 Open Mailbox With Lowered Flag
📮 Postbox
📝 Memo
💼 Briefcase
📁 File Folder
📂 Open File Folder
📅 Calendar
📆 Tear-Off Calendar
📇 Card Index
📈 Chart Increasing
📉 Chart Decreasing
📊 Bar Chart
📋 Clipboard
📌 Pushpin
📍 Round Pushpin
📎 Paperclip
📏 Straight Ruler
📐 Triangular Ruler
🔒 Locked
🔓 Unlocked
🔏 Locked With Pen
🔐 Locked With Key
🔑 Key
🔨 Hammer
🔫 Pistol
🔧 Wrench
🔩 Nut and Bolt
🔗 Link
🔬 Microscope
🔭 Telescope
📡 Satellite Antenna
💉 Syringe
💊 Pill
🚪 Door
🚽 Toilet
🚿 Shower
🛁 Bathtub
🚬 Cigarette
🗿 Moai
🏧 ATM Sign
🚮 Litter in Bin Sign
🚰 Potable Water
🚹 Men’s Room
🚺 Women’s Room
🚻 Restroom
🚼 Baby Symbol
🚾 Water Closet
🛂 Passport Control
🛃 Customs
🛄 Baggage Claim
🛅 Left Luggage
🚸 Children Crossing
🚫 Prohibited
🚳 No Bicycles
🚭 No Smoking
🚯 No Littering
🚱 Non-Potable Water
🚷 No Pedestrians
📵 No Mobile Phones
🔞 No One Under Eighteen
🔃 Clockwise Vertical Arrows
🔄 Counterclockwise Arrows Button
🔙 Back Arrow
🔚 End Arrow
🔛 On! Arrow
🔜 Soon Arrow
🔝 Top Arrow
🔯 Dotted Six-Pointed Star
⛎ Ophiuchus
🔀 Shuffle Tracks Button
🔁 Repeat Button
🔂 Repeat Single Button
⏩ Fast-Forward Button
⏭ Next Track Button
⏯ Play or Pause Button
⏪ Fast Reverse Button
⏮ Last Track Button
🔼 Upwards Button
⏫ Fast Up Button
🔽 Downwards Button
⏬ Fast Down Button
🎦 Cinema
🔅 Dim Button
🔆 Bright Button
📶 Antenna Bars
📳 Vibration Mode
📴 Mobile Phone Off
🔱 Trident Emblem
📛 Name Badge
🔰 Japanese Symbol for Beginner
✅ White Heavy Check Mark
❌ Cross Mark
❎ Cross Mark Button
➕ Heavy Plus Sign
➖ Heavy Minus Sign
➗ Heavy Division Sign
➰ Curly Loop
➿ Double Curly Loop
❓ Question Mark
❔ White Question Mark
❕ White Exclamation Mark
🔟 Keycap: 10
🔠 Input Latin Uppercase
🔡 Input Latin Lowercase
🔢 Input Numbers
🔣 Input Symbols
🔤 Input Latin Letters
🅰 A Button (Blood Type)
🆎 AB Button (Blood Type)
🅱 B Button (Blood Type)
🆑 CL Button
🆒 Cool Button
🆓 Free Button
🆔 ID Button
🆕 New Button
🆖 NG Button
🅾 O Button (Blood Type)
🆗 OK Button
🆘 SOS Button
🆙 Up! Button
🆚 Vs Button
🈁 Japanese Here Button
🈂 Japanese Service Charge Button
🈷 Japanese Monthly Amount Button
🈶 Japanese Not Free of Charge Button
🉐 Japanese Bargain Button
🈹 Japanese Discount Button
🈲 Japanese Prohibited Button
🉑 Japanese Acceptable Button
🈸 Japanese Application Button
🈴 Japanese Passing Grade Button
🈳 Japanese Vacancy Button
🈺 Japanese Open for Business Button
🈵 Japanese No Vacancy Button
🔴 Red Circle
🔵 Blue Circle
🔶 Large Orange Diamond
🔷 Large Blue Diamond
🔸 Small Orange Diamond
🔹 Small Blue Diamond
🔺 Red Triangle Pointed Up
🔻 Red Triangle Pointed Down
💠 Diamond With a Dot
🔘 Radio Button
🔳 White Square Button
🔲 Black Square Button
🏁 Chequered Flag
🚩 Triangular Flag
🎌 Crossed Flags
🇦 Regional Indicator Symbol Letter A
🇧 Regional Indicator Symbol Letter B
🇨 Regional Indicator Symbol Letter C
🇩 Regional Indicator Symbol Letter D
🇪 Regional Indicator Symbol Letter E
🇫 Regional Indicator Symbol Letter F
🇬 Regional Indicator Symbol Letter G
🇭 Regional Indicator Symbol Letter H
🇮 Regional Indicator Symbol Letter I
🇯 Regional Indicator Symbol Letter J
🇰 Regional Indicator Symbol Letter K
🇱 Regional Indicator Symbol Letter L
🇲 Regional Indicator Symbol Letter M
🇳 Regional Indicator Symbol Letter N
🇴 Regional Indicator Symbol Letter O
🇵 Regional Indicator Symbol Letter P
🇶 Regional Indicator Symbol Letter Q
🇷 Regional Indicator Symbol Letter R
🇸 Regional Indicator Symbol Letter S
🇹 Regional Indicator Symbol Letter T
🇺 Regional Indicator Symbol Letter U
🇻 Regional Indicator Symbol Letter V
🇼 Regional Indicator Symbol Letter W
🇽 Regional Indicator Symbol Letter X
🇾 Regional Indicator Symbol Letter Y
🇿 Regional Indicator Symbol Letter Z
⛧ Inverted Pentagram
⛢ Astronomical Symbol for Uranus
⛥ Right-Handed Interlaced Pentagram
⛦ Left-Handed Interlaced Pentagram
⛤ Pentagram")
        ReadKey(True)
    End Sub
End Module
