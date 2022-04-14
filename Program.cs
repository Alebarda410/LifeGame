var random = new Random();

Console.Write("Введитете высоту поля: ");
var rowCount = CheckData(Console.ReadLine(), 168);

Console.Write("Введитете ширину поля: ");
var columnCount = CheckData(Console.ReadLine(), 630);

Console.Write("Введитете плотность жизни (от 1 до 10): ");
var denLife = CheckData(Console.ReadLine(), 5);

Console.Write("Отрисовывать процесс игры (y/n)?: ");
var showState = Console.ReadLine()!.Contains('n');

Console.Clear();
Console.CursorVisible = false;
Console.SetCursorPosition(0, 0);

var currentGen = new byte[rowCount][];
var nextGen = new byte[rowCount][];
var strOut = new char[columnCount];

for (var i = 0; i < rowCount; i++)
{
    currentGen[i] = new byte[columnCount];
    nextGen[i] = new byte[columnCount];
    for (var j = 0; j < columnCount; j++)
    {
        currentGen[i][j] = random.Next(1, 11) >= denLife ? (byte) 1 : (byte) 0;
    }
}

ParallelLife();

void ParallelLife()
{
    var countLife = 0;
    var curGen = 0;
    var tempCountLife = new Queue<int>();
    var torJ = columnCount - 1;
    var torI = rowCount - 1;
    var cpuCount = Environment.ProcessorCount;
    while (true)
    {
        countLife = 0;

        Parallel.For(0, rowCount, new ParallelOptions {MaxDegreeOfParallelism = cpuCount}, i =>
        {
            for (var j = 0; j < columnCount; j++)
            {
                int jRight;
                int jLeft;
                if (j == 0)
                {
                    jLeft = torJ;
                    jRight = j + 1;
                }
                else if (j == torJ)
                {
                    jLeft = j - 1;
                    jRight = 0;
                }
                else
                {
                    jLeft = j - 1;
                    jRight = j + 1;
                }


                int iTop;
                int iBottom;
                if (i == 0)
                {
                    iTop = torI;
                    iBottom = i + 1;
                }
                else if (i == torI)
                {
                    iTop = i - 1;
                    iBottom = 0;
                }
                else
                {
                    iTop = i - 1;
                    iBottom = i + 1;
                }

                var temp = 0;
                if (currentGen[i][j] == 0)
                {
                    CntOnes();
                    if (temp == 3)
                        nextGen[i][j] = 1;
                }

                else
                {
                    Interlocked.Increment(ref countLife);
                    CntOnes();
                    if (temp is < 2 or > 3)
                        nextGen[i][j] = 0;
                }

                void CntOnes()
                {
                    if (currentGen[iTop][jLeft] == 1) temp++;
                    if (currentGen[iTop][j] == 1) temp++;
                    if (currentGen[iTop][jRight] == 1) temp++;
                    if (currentGen[i][jLeft] == 1) temp++;
                    if (currentGen[i][jRight] == 1) temp++;
                    if (currentGen[iBottom][jLeft] == 1) temp++;
                    if (currentGen[iBottom][j] == 1) temp++;
                    if (currentGen[iBottom][jRight] == 1) temp++;
                }
            }
        });
        tempCountLife.Enqueue(countLife);

        // если текущее число живых повторяется больше 10 раз за 30 поколений, то конец игры
        if (tempCountLife.Count(i => i == countLife) > 10)
        {
            if (showState) Console.WriteLine($"Поколение {curGen} произошло попадание в петлю, конец игры!");
            else Console.Title = $"Поколение {curGen} произошло попадание в петлю, конец игры!";
            
            Console.ReadLine();
            break;
        }

        // потдерживаем хранение информации только о 30 поколениях
        if (tempCountLife.Count > 30)
        {
            tempCountLife.Dequeue();
        }

        for (var i = 0; i < currentGen.Length; i++)
            nextGen[i].CopyTo(currentGen[i], 0);

        curGen++;
        if (showState)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"Поколение {curGen}, живых {countLife}");
        }
        else
        {
            ShowInfo(curGen, countLife);
        }
    }
}

void ShowInfo(int curGen, int countLife)
{
    Console.Title = $"Поколение {curGen}, живых {countLife}";

    for (var i = 0; i < rowCount; i++)
    {
        for (var j = 0; j < columnCount; j++)
        {
            if (nextGen[i][j] == 1)
            {
                strOut[j] = '@';
            }
            else
            {
                strOut[j] = ' ';
            }
        }

        Console.WriteLine(strOut);
    }

    Console.SetCursorPosition(0, 0);
}

int CheckData(string? input, int defValue)
{
    return int.TryParse(input, out var number) ? number : defValue;
}