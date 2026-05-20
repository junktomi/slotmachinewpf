# Félkarú rabló - C# WPF minta

Ez egy egyszerű WPF slot machine minta.

## Főbb jellemzők
- 3 tárcsa, mindegyiken 3 látható szimbólum
- a középső sor kerül kiértékelésre
- induló egyenleg: 100 kredit
- egy pörgetés ára: 10 kredit
- 3 egyforma: 50 kredit
- 2 egyforma: 20 kredit
- animált pörgetés, a tárcsák külön-külön állnak meg
- nyerési statisztika
- `SymbolsOnReel` konstanssal egyetlen helyen állítható, hány szimbólum legyen egy tárcsán

## A fontos beállítás
A `MainWindow.xaml.cs` fájlban ezt az egy értéket kell átírni:

```csharp
private const int SymbolsOnReel = 8;
```

Példák:
- `4` -> teszteléshez, gyorsabban jön nyerés
- `10` vagy `12` -> realisztikusabb működés

## Képek használata
A projekt jelenleg **emoji fallbackkel is fut**, ezért akkor is demonstrálható, ha még nincsenek bemásolva a képek.

Ha valódi PNG képeket szeretnél használni, másold be ezeket az `Images` mappába pontosan ezekkel a nevekkel:

- `cherry.png`
- `lemon.png`
- `grapes.png`
- `watermelon.png`
- `apple.png`

## Javasolt képfelhasználás
A mellékelt válaszban megadott források alapján OpenMoji vagy Wikimedia Commons alapú gyümölcsképeket érdemes használni.

## Fordítás / futtatás
Visual Studio-ban nyisd meg a `SlotMachineWpf.csproj` fájlt, majd indítsd el a projektet.
