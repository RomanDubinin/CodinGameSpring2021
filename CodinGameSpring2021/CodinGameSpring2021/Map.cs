using System.Collections.Generic;
using System.Linq;

namespace CodinGameSpring2021
{
    public class Map
    {
        private List<Cell> Cells { get; set; }
        private List<Tree> Trees { get; set; }

        public Map(List<Cell> cells, List<Tree> trees)
        {
            Cells = cells;
            Trees = trees;
        }

        public Tree GetNextTreeToComplete()
        {
            var internalCells = Combine();
            return internalCells
                   .Where(x => x.Tree != null)
                   .Where(x => x.Tree.IsMine && !x.Tree.IsDormant && x.Tree.Size == 3)
                   .OrderByDescending(x => x.Richness)
                   .ThenByDescending(x => x.Tree.Size)
                   .Select(x => x.Tree)
                   .FirstOrDefault();
        }

        public Tree GetNextTreeToGrow(int daysToEnd)
        {
            var internalCells = Combine();
            return internalCells
                   .Where(x => x.Tree != null)
                   .Where(x => x.Tree.IsMine && !x.Tree.IsDormant && x.Tree.Size < 3 && 3 - x.Tree.Size < daysToEnd)
                   .OrderByDescending(x => x.Richness)
                   .ThenByDescending(x => x.Tree.Size)
                   .Select(x => x.Tree)
                   .FirstOrDefault();
        }

        private List<InternalCell> Combine()
        {
            return Cells.Select(x => new InternalCell
                        {
                            CellIndex = x.CellIndex,
                            Richness = x.Richness,
                            Tree = Trees.FirstOrDefault(t => t.CellIndex == x.CellIndex)
                        })
                        .ToList();
        }

        private class InternalCell
        {
            public int CellIndex { get; set; }
            public int Richness { get; set; }
            public Tree Tree { get; set; }
        }
    }
}