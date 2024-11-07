using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Synthesis.Util.Cell
{
    /// <summary>
    /// Helpers for working with Cell records
    /// </summary>
    public static class CellUtil
    {
        /// <summary>
        /// Checks if a cell is part of a worldspace
        /// </summary>
        /// <param name="cellContext">The cell to check</param>
        /// <returns>True if the cell is a worldspace cell</returns>
        public static bool IsWorldspaceCell(this IModContext<ICellGetter> cellContext) =>
            cellContext.TryGetParent<IWorldspaceGetter>(out _);

        /// <summary>
        /// Retrieves all interior cells from a mod
        /// </summary>
        /// <param name="mod">The mod to retrieve records from</param>
        /// <returns>Cell records not from worldspaces</returns>
        public static IEnumerable<ICellGetter> GetInteriorCells(this ISkyrimModGetter mod) =>
            mod
                .Cells.Records.SelectMany(cellBlock => cellBlock.SubBlocks)
                .SelectMany(subBlock => subBlock.Cells);

        /// <summary>
        /// Retrieves all exterior worldspace cells from a mod
        /// </summary>
        /// <param name="mod">The mod to retrieve records from</param>
        /// <returns>The cell records from worldspaces</returns>
        public static IEnumerable<ICellGetter> GetWorldspaceCells(this ISkyrimModGetter mod) =>
            mod
                .Worldspaces.Records.SelectMany(worldspace => worldspace.SubCells)
                .SelectMany(worldspaceBlock => worldspaceBlock.Items)
                .SelectMany(worldspaceSubBlock => worldspaceSubBlock.Items);

        /// <summary>
        /// Retrieves *all* cells from a mod, both interior and exterior worldspace cells
        /// </summary>
        /// <param name="mod">The mod to retrieve records from</param>
        /// <returns>All cell records from the mod</returns>
        public static IEnumerable<ICellGetter> GetAllCells(this ISkyrimModGetter mod) =>
            mod.GetInteriorCells().Concat(mod.GetWorldspaceCells());
    }
}
