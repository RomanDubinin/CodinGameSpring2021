using System;
using System.Collections.Generic;

namespace CodinGameSpring2021
{
    class Player
    {
        private static readonly int FinalizeCost = 4;
        static void Main(string[] args)
        {
            string[] inputs;
            int numberOfCells = int.Parse(Console.ReadLine()); // 37
            var cells = new List<Cell>(numberOfCells);
            for (int i = 0; i < numberOfCells; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int index = int.Parse(inputs[0]); // 0 is the center cell, the next cells spiral outwards
                int richness = int.Parse(inputs[1]); // 0 if the cell is unusable, 1-3 for usable cells
                int neigh0 = int.Parse(inputs[2]); // the index of the neighbouring cell for each direction
                int neigh1 = int.Parse(inputs[3]);
                int neigh2 = int.Parse(inputs[4]);
                int neigh3 = int.Parse(inputs[5]);
                int neigh4 = int.Parse(inputs[6]);
                int neigh5 = int.Parse(inputs[7]);
                cells.Add(new Cell
                {
                    CellIndex = index,
                    Richness = richness
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

                var map = new Map(cells, trees);

                var finalDay = 23;
                if (day == finalDay)
                {
                    var treeToComplete = map.GetNextTreeToComplete();
                    if (treeToComplete != null)
                        Console.WriteLine($"COMPLETE {treeToComplete.CellIndex} nutrients: {nutrients}");
                    else
                        Console.WriteLine(GrowSeedCompleteAction(map, finalDay, day, sun));
                }
                else
                {
                    var action = GrowSeedCompleteAction(map, finalDay, day, sun);
                    Console.WriteLine(action);
                }

                // GROW cellIdx | SEED sourceIdx targetIdx | COMPLETE cellIdx | WAIT <message>

                // Console.WriteLine($"nutrients: {nutrients}");
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
            }
        }

        private static string GrowSeedCompleteAction(Map map, int finalDay, int day, int sunPoints)
        {
            var daysLeft = finalDay - day;

            var grownTreesCount = map.GetGrownTreesCount();
            var lowResourcesForSeed = (grownTreesCount+1)*FinalizeCost > sunPoints && daysLeft <= 7 || daysLeft <= 5;
            var lowResourcesForGrow = (grownTreesCount+1)*FinalizeCost > sunPoints && daysLeft <= 4;

            //todo seed only if less than 6 seed already on field
            var (tree, cellToSeed) = map.GetCellToSeed();
            if (cellToSeed != null && !lowResourcesForSeed)
                return $"SEED {tree.CellIndex} {cellToSeed}";

            var highCellsFilled = map.IsHighCellsFilled();
            if (highCellsFilled)
            {
                var treeToComplete = map.GetNextTreeToCompleteFromHighCell();
                if (treeToComplete != null)
                    return $"COMPLETE {treeToComplete.CellIndex}";
            }

            var treeToGrow = map.GetNextTreeToGrow(daysLeft);
            if (treeToGrow != null && !lowResourcesForGrow)
                return $"GROW {treeToGrow.CellIndex} {(lowResourcesForSeed ? "lowForSeed" : "")}";

            return $"WAIT {(lowResourcesForGrow ? "lowForGrow" : "")}";
        }
    }
}