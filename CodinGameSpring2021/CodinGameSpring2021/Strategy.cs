using System;
using System.Collections.Generic;
using System.Linq;

namespace CodinGameSpring2021
{
    public class Strategy
    {
        private readonly int daysInCycle = 6;
        private readonly int finalizeCost = 4;
        private readonly int finalDay = 23;
        private readonly int day;

        private Dictionary<int, InternalCell> CellsDict { get; }
        private List<InternalCell> InternalCells { get => CellsDict.Select(x => x.Value).ToList(); }
        private List<Tree> Trees { get; }

        public Strategy(List<Cell> cells, List<Tree> trees, int day)
        {
            this.day = day;
            Trees = trees;
            CellsDict = Combine(cells, trees, day);
        }

        public string GetNextAction(int sunPoints)
        {
            if (day == finalDay)
            {
                var treeToComplete = GetNextTreeToComplete();
                if (treeToComplete != null)
                    return $"COMPLETE {treeToComplete.CellIndex}";

                return GrowSeedCompleteAction(finalDay, day, sunPoints);
            }

            return GrowSeedCompleteAction(finalDay, day, sunPoints);
        }

        private string GrowSeedCompleteAction(int finalDay, int day, int sunPoints)
        {
            var daysLeft = finalDay - day;

            var grownTreesCount = GetGrownTreesCount();
            var lowResourcesForSeed = (grownTreesCount+1)*finalizeCost > sunPoints && daysLeft <= 7 || daysLeft <= 5;
            var lowResourcesForGrow = (grownTreesCount+1)*finalizeCost > sunPoints && daysLeft <= 4;
            var seedCount = GetSeedCount();

            var (tree, cellToSeed) = GetCellToSeed();
            if (cellToSeed != null && !lowResourcesForSeed && seedCount < 4)
                return $"SEED {tree.CellIndex} {cellToSeed}";

            var highCellsFilled = IsHighCellsFilled();
            if (highCellsFilled)
            {
                var treeToComplete = GetNextTreeToCompleteFromHighCell();
                if (treeToComplete != null)
                    return $"COMPLETE {treeToComplete.CellIndex}";
            }

            var treeToGrow = GetNextTreeToGrow(daysLeft);
            if (treeToGrow != null && !lowResourcesForGrow)
                return $"GROW {treeToGrow.CellIndex} {(lowResourcesForSeed ? "lowForSeed" : "")}";

            return $"WAIT {(lowResourcesForGrow ? "lowForGrow" : "")}";
        }

        private int GetSeedCount()
        {
            return Trees.Count(x => x.IsMine && x.Size == 0);
        }

        private int GetGrownTreesCount()
        {
            return Trees.Count(x => x.IsMine && x.Size == 3);
        }


        private Tree GetNextTreeToComplete()
        {
            return InternalCells
                   .Where(x => x.Tree != null)
                   .Where(x => x.Tree.IsMine && !x.Tree.IsDormant && x.Tree.Size == 3)
                   .OrderByDescending(x => x.Richness)
                   .ThenBy(x => x.ShadowSize >= x.Tree.Size ? 2 : 1)
                   .Select(x => x.Tree)
                   .FirstOrDefault();
        }

        private Tree GetNextTreeToCompleteFromHighCell()
        {
            return InternalCells
                   .Where(x => x.Tree != null && x.Richness == 3)
                   .Where(x => x.Tree.IsMine && !x.Tree.IsDormant && x.Tree.Size == 3)
                   .OrderBy(x => x.ShadowSize >= x.Tree.Size ? 2 : 1)
                   .Select(x => x.Tree)
                   .FirstOrDefault();
        }

        private Tree GetNextTreeToGrow(int daysToEnd)
        {
            return InternalCells
                   .Where(x => x.Tree != null)
                   .Where(x => x.Tree.IsMine && !x.Tree.IsDormant && x.Tree.Size < 3 && 3 - x.Tree.Size <= daysToEnd)
                   .OrderByDescending(x => x.Richness)
                   .ThenByDescending(x => x.Tree.Size)
                   .Select(x => x.Tree)
                   .FirstOrDefault();
        }

        public bool IsHighCellsFilled()
        {
            var allFilled = InternalCells
                            .Where(x => x.Richness == 3)
                            .All(x => x.Tree != null);
            var withMyTrees = InternalCells
                              .Where(x => x.Richness == 3)
                              .Where(x => x.Tree?.IsMine ?? false)
                              .ToArray();
            return allFilled && withMyTrees.Count(x => x.Tree.Size == 3) > withMyTrees.Length - 2;
        }

        private (Tree Tree, int? CellToSeed) GetCellToSeed()
        {
            var cellsFreeToSeed = Trees
                                  .Where(x => x.IsMine && !x.IsDormant)
                                  .SelectMany(x => GetNeighboursTowardCenter(x).Select(y => (Tree: x, CellToSeed: y)))
                                  .Select(x => (Tree: x.Tree, CellToSeed: CellsDict[x.CellToSeed]))
                                  .Where(x => x.CellToSeed.Tree == null)
                                  .ToArray();

            var highCellsFreeToSeed = cellsFreeToSeed.Where(x => x.CellToSeed.Richness == 3);
            var allHighCells = InternalCells.Where(x => x.Richness == 3 && x.Tree == null);

            var mediumCellsFreeToSeed = cellsFreeToSeed.Where(x => x.CellToSeed.Richness == 2);
            var allMediumCells = InternalCells.Where(x => x.Richness == 2 && x.Tree == null);

            if (highCellsFreeToSeed.Count() != 0)
            {
                var first = highCellsFreeToSeed.First();
                return (first.Tree, first.CellToSeed.CellIndex);
            }
            if (allHighCells.Count() != 0)
            {
                return (null, null);
            }
            if (mediumCellsFreeToSeed.Count() != 0)
            {
                var first = mediumCellsFreeToSeed.First();
                return (first.Tree, first.CellToSeed.CellIndex);
            }
            if (allMediumCells.Count() != 0)
            {
                return (null, null);
            }

            return (null, null);
        }

        private IEnumerable<int> GetNeighboursTowardCenter(Tree tree)
        {
            var neighbours1 = NeighboursTowardsCenter[tree.CellIndex];
            var neighbours2 = neighbours1.Concat(neighbours1.SelectMany(x => NeighboursTowardsCenter[x])).ToArray();
            var neighbours3 = neighbours2.Concat(neighbours2.SelectMany(x => NeighboursTowardsCenter[x])).ToArray();
            return tree.Size switch
            {
                1 => neighbours1,
                2 => neighbours2,
                3 => neighbours3,
                _ => Array.Empty<int>()
            };
        }

        private Dictionary<int, InternalCell> Combine(List<Cell> cells, List<Tree> trees, int day)
        {
            var internalCells = cells.Select(x => new InternalCell
                                     {
                                         CellIndex = x.CellIndex,
                                         Richness = x.Richness,
                                         Tree = trees.FirstOrDefault(t => t.CellIndex == x.CellIndex),
                                         Neigh0 = x.Neigh0,
                                         Neigh1 = x.Neigh1,
                                         Neigh2 = x.Neigh2,
                                         Neigh3 = x.Neigh3,
                                         Neigh4 = x.Neigh4,
                                         Neigh5 = x.Neigh5
                                     })
                                     .ToDictionary(x => x.CellIndex, x => x);

            foreach (var (k, cell) in internalCells)
            {
                if (cell.Tree != null)
                {
                    foreach (var underShadowNeighbour in GetUnderShadowNeighbours(internalCells, cell, day+1))
                    {
                        var underShadow = internalCells[underShadowNeighbour];
                        underShadow.ShadowSize = cell.Tree.Size;
                    }
                }
            }

            return internalCells;
        }

        private int[] GetUnderShadowNeighbours(Dictionary<int, InternalCell> cells, InternalCell cell, int nextDay)
        {
            var nearestNeighIndex = cell.Neighbours[nextDay % daysInCycle];
            if (nearestNeighIndex == -1 || cell.Tree.Size == 0)
                return Array.Empty<int>();

            var secondNeighIndex = cells[nearestNeighIndex].Neighbours[nextDay % daysInCycle];
            if (secondNeighIndex == -1 || cell.Tree.Size == 1)
                return new[] {nearestNeighIndex};

            var thirdNeighIndex = cells[secondNeighIndex].Neighbours[nextDay % daysInCycle];
            if (thirdNeighIndex == -1 || cell.Tree.Size == 2)
                return new[] {nearestNeighIndex, secondNeighIndex};

            return new[] {nearestNeighIndex, secondNeighIndex, thirdNeighIndex};
        }

        private Dictionary<int, int[]> NeighboursTowardsCenter = new Dictionary<int, int[]>
        {
            {0, new[] {1, 2, 3, 4, 5, 6}},
            {1, new[] {0, 2, 6}},
            {2, new[] {0, 1, 3}},
            {3, new[] {0, 4, 2}},
            {4, new[] {0, 3, 5}},
            {5, new[] {0, 4, 6}},
            {6, new[] {0, 5, 1}},
            {7, new[] {1}},
            {8, new[] {1, 2}},
            {9, new[] {2}},
            {10, new[] {2, 3}},
            {11, new[] {3}},
            {12, new[] {3, 4}},
            {13, new[] {4}},
            {14, new[] {4, 5}},
            {15, new[] {5}},
            {16, new[] {5, 6}},
            {17, new[] {6}},
            {18, new[] {1, 6}},
            {19, new[] {7}},
            {20, new[] {7, 8}},
            {21, new[] {8, 9}},
            {22, new[] {9}},
            {23, new[] {9, 10}},
            {24, new[] {10, 11}},
            {25, new[] {11}},
            {26, new[] {11, 12}},
            {27, new[] {12, 13}},
            {28, new[] {13}},
            {29, new[] {13, 14}},
            {30, new[] {14, 15}},
            {31, new[] {15}},
            {32, new[] {15, 16}},
            {33, new[] {16, 17}},
            {34, new[] {17}},
            {35, new[] {17, 18}},
            {36, new[] {18, 7}},
        };

        private class InternalCell
        {
            public int CellIndex { get; set; }
            public int Richness { get; set; }
            public Tree Tree { get; set; }
            public int ShadowSize { get; set; }

            public int Neigh0 { get; set; }
            public int Neigh1 { get; set; }
            public int Neigh2 { get; set; }
            public int Neigh3 { get; set; }
            public int Neigh4 { get; set; }
            public int Neigh5 { get; set; }

            public int[] Neighbours { get => new[] {Neigh0, Neigh1, Neigh2, Neigh3, Neigh4, Neigh5}; }
        }
    }
}