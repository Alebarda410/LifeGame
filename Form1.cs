using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeGame
{
    public partial class Form1 : Form
    {
        private readonly Random _random = new Random();
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _cG;
        private byte[][] _currentGen, _nextGen;

        public Form1()
        {
            InitializeComponent();
        }

        private void CreateRandomField_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel) return;

            var pathToFile = saveFileDialog1.FileName;
            _stopwatch.Restart();

            var columnCount = Convert.ToInt32(xField.Text);
            var rowCount = Convert.ToInt32(yField.Text);
            var denLife = 11 - Convert.ToInt32(den.Text);
            using (var stream = new BinaryWriter(File.Open(pathToFile, FileMode.Create)))
            {
                stream.Write(columnCount);
                stream.Write(rowCount);

                for (var i = 0; i < rowCount; i++)
                {
                    for (var j = 0; j < columnCount; j++)
                    {
                        stream.Write(_random.Next(1, 11) >= denLife ? (byte)1 : (byte)0);
                    }
                }
            }
            richTextBox1.AppendText($"Карта создана и сохранена  - {_stopwatch.ElapsedMilliseconds / 1000.0} сек\n");
        }

        private void ChooseFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;

            var pathToFile = openFileDialog1.FileName;
            _stopwatch.Restart();

            using (var reader = new BinaryReader(File.Open(pathToFile, FileMode.Open)))
            {
                var columnCount = reader.ReadInt32();
                var rowCount = reader.ReadInt32();
                _currentGen = new byte[rowCount][];
                _nextGen = new byte[rowCount][];

                for (var i = 0; i < rowCount; i++)
                {
                    _currentGen[i] = reader.ReadBytes(columnCount);
                    _nextGen[i] = new byte[columnCount];
                }
                for (var i = 0; i < _currentGen.Length; i++)
                    _currentGen[i].CopyTo(_nextGen[i], 0);
            }
            richTextBox1.AppendText($"Карта загружена - {_stopwatch.ElapsedMilliseconds / 1000.0} сек\n");
        }

        public void ParallelLife()
        {
            var countLife = 0;
            var tempCountLife = 0;
            var torJ = _currentGen[0].Length - 1;
            var torI = _currentGen.Length - 1;

            for (var k = 0; k < _cG; k++)
            {
                countLife = 0;
                _stopwatch.Restart();

                Parallel.For(0, _currentGen.Length, i =>
                {
                    for (var j = 0; j < _currentGen[i].Length; j++)
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

                        int temp;
                        if (_currentGen[i][j] == 0)
                        {
                            temp = 0;
                            if (_currentGen[iTop][jLeft] == 1) temp++;
                            if (_currentGen[iTop][j] == 1) temp++;
                            if (_currentGen[iTop][jRight] == 1) temp++;
                            if (_currentGen[i][jLeft] == 1) temp++;
                            if (temp > 3) continue;
                            if (_currentGen[i][jRight] == 1) temp++;
                            if (_currentGen[iBottom][jLeft] == 1) temp++;
                            if (temp == 0) continue;
                            if (_currentGen[iBottom][j] == 1) temp++;
                            if (_currentGen[iBottom][jRight] == 1) temp++;
                            if (temp == 3)
                                _nextGen[i][j] = 1;
                        }

                        if (_currentGen[i][j] == 1)
                        {
                            Interlocked.Increment(ref countLife);
                            temp = 0;
                            if (_currentGen[iTop][jLeft] == 1) temp++;
                            if (_currentGen[iTop][j] == 1) temp++;
                            if (_currentGen[iTop][jRight] == 1) temp++;
                            if (_currentGen[i][jLeft] == 1) temp++;
                            if (_currentGen[i][jRight] == 1) temp++;
                            if (_currentGen[iBottom][jLeft] == 1) temp++;
                            if (_currentGen[iBottom][j] == 1) temp++;
                            if (_currentGen[iBottom][jRight] == 1) temp++;
                            if (temp < 2 || temp > 3) 
                                _nextGen[i][j] = 0;
                        }
                    }
                });
                if (tempCountLife == countLife)
                {
                    Invoke(new Action(() =>
                        richTextBox1.AppendText(
                            "Произошло попадание в петлю, конец игры!\n")));
                    break;
                }
                tempCountLife = countLife;
                for (var i = 0; i < _currentGen.Length; i++)
                    _nextGen[i].CopyTo(_currentGen[i], 0);

                Invoke(new Action(() =>
                    richTextBox1.AppendText(
                        $"Поколение {k + 1}, живых {countLife} - {_stopwatch.ElapsedMilliseconds / 1000.0} сек\n")));
            }
        }

        public void OneCoreLife()
        {
            var countLife = 0;
            var tempCountLife = 0;

            var torJ = _currentGen[0].Length - 1;
            var torI = _currentGen.Length - 1;

            for (var k = 0; k < _cG; k++)
            {
                countLife = 0;
                _stopwatch.Restart();
                for (var i = 0; i < _currentGen.Length; i++)
                {
                    for (var j = 0; j < _currentGen[i].Length; j++)
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

                        int temp;
                        if (_currentGen[i][j] == 0)
                        {
                            temp = 0;
                            if (_currentGen[iTop][jLeft] == 1) temp++;
                            if (_currentGen[iTop][j] == 1) temp++;
                            if (_currentGen[iTop][jRight] == 1) temp++;
                            if (_currentGen[i][jLeft] == 1) temp++;
                            if (temp > 3) continue;
                            if (_currentGen[i][jRight] == 1) temp++;
                            if (_currentGen[iBottom][jLeft] == 1) temp++;
                            if (temp == 0) continue;
                            if (_currentGen[iBottom][j] == 1) temp++;
                            if (_currentGen[iBottom][jRight] == 1) temp++;
                            if (temp == 3)
                                _nextGen[i][j] = 1;
                        }

                        if (_currentGen[i][j] == 1)
                        {
                            countLife++;
                            temp = 0;
                            if (_currentGen[iTop][jLeft] == 1) temp++;
                            if (_currentGen[iTop][j] == 1) temp++;
                            if (_currentGen[iTop][jRight] == 1) temp++;
                            if (_currentGen[i][jLeft] == 1) temp++;
                            if (_currentGen[i][jRight] == 1) temp++;
                            if (_currentGen[iBottom][jLeft] == 1) temp++;
                            if (_currentGen[iBottom][j] == 1) temp++;
                            if (_currentGen[iBottom][jRight] == 1) temp++;
                            if (temp < 2 || temp > 3)
                                _nextGen[i][j] = 0;
                        }
                    }
                }
                if (tempCountLife == countLife)
                {
                    Invoke(new Action(() =>
                        richTextBox1.AppendText(
                            "Произошло попадание в петлю, конец игры!\n")));
                    break;
                }
                tempCountLife = countLife;

                for (var i = 0; i < _currentGen.Length; i++)
                    _nextGen[i].CopyTo(_currentGen[i], 0);

                Invoke(new Action(() =>
                    richTextBox1.AppendText(
                        $"Поколение {k + 1}, живых {countLife} - {_stopwatch.ElapsedMilliseconds / 1000.0} сек\n")));
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            _cG = Convert.ToInt32(countGen.Text);
            var mThread1 = new Thread(OneCoreLife)
            {
                Priority = ThreadPriority.Highest
            };
            var mThread2 = new Thread(ParallelLife)
            {
                Priority = ThreadPriority.Highest
            };
            if (checkParal.SelectedIndex == 0) mThread2.Start();
            else mThread1.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkParal.SelectedIndex = 1;
        }
    }
}
