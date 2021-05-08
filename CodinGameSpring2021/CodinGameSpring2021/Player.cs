using System;
using System.Collections.Generic;

namespace CodinGameSpring2021
{
    class Player
    {
        static void Main(string[] args)
        {
            string[] inputs;
            int numberOfCells = int.Parse(Console.ReadLine()); // 37
            var cells = new List<Cell>(numberOfCells);
            for (int i = 0; i < numberOfCells; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                cells.Add(new Cell
                {
                    CellIndex = int.Parse(inputs[0]),
                    Richness = int.Parse(inputs[1]),
                    Neigh0 = int.Parse(inputs[2]),
                    Neigh1 = int.Parse(inputs[3]),
                    Neigh2 = int.Parse(inputs[4]),
                    Neigh3 = int.Parse(inputs[5]),
                    Neigh4 = int.Parse(inputs[6]),
                    Neigh5 = int.Parse(inputs[7])
                });
            }

            while (true)
            {
                int day = int.Parse(Console.ReadLine()); // the game lasts 24 days: 0-23
                int nutrients = int.Parse(Console.ReadLine()); // the base score you gain from the next COMPLETE action
                inputs = Console.ReadLine().Split(' ');
                int sun = int.Parse(inputs[0]); // your sun points
                int score = int.Parse(inputs[1]); // your current score
                inputs = Console.ReadLine().Split(' ');
                int oppSun = int.Parse(inputs[0]); // opponent's sun points
                int oppScore = int.Parse(inputs[1]); // opponent's score
                bool oppIsWaiting = inputs[2] != "0"; // whether your opponent is asleep until the next day

                int numberOfTrees = int.Parse(Console.ReadLine()); // the current amount of trees
                var trees = new List<Tree>(numberOfTrees);
                for (int i = 0; i < numberOfTrees; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int cellIndex = int.Parse(inputs[0]); // location of this tree
                    int size = int.Parse(inputs[1]); // size of this tree: 0-3
                    bool isMine = inputs[2] != "0"; // 1 if this is your tree
                    bool isDormant = inputs[3] != "0"; // 1 if this tree is dormant
                    trees.Add(new Tree
                    {
                        CellIndex = cellIndex,
                        Size = size,
                        IsMine = isMine,
                        IsDormant = isDormant
                    });
                }
                int numberOfPossibleMoves = int.Parse(Console.ReadLine());
                for (int i = 0; i < numberOfPossibleMoves; i++)
                {
                    string possibleMove = Console.ReadLine();
                }

                var strategy = new Strategy(cells, trees, day);
                var action = strategy.GetNextAction(sun);
                Console.WriteLine(action);

                // GROW cellIdx | SEED sourceIdx targetIdx | COMPLETE cellIdx | WAIT <message>

                // Console.WriteLine($"nutrients: {nutrients}");
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
            }
        }

    }
}