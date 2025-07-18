﻿using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Models;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Input;

namespace LokqlDx.ViewModels;
/// <summary>
/// This is a copy of the Avalonia FlatTreeDataGridSource that allows dynamic
/// columns
/// </summary>
/// <remarks>
/// The only difference between this and the official
/// version is that it contains a constructor that allows
/// the columns to be specified
/// </remarks>
public class MyFlatTreeDataGridSource<TModel> : NotifyingBase,
    ITreeDataGridSource<TModel>,
    IDisposable
    where TModel : class
{
    private IComparer<TModel>? _comparer;
    private bool _isSelectionSet;
    private IEnumerable<TModel> _items;
    private TreeDataGridItemsSourceView<TModel> _itemsView;
    private AnonymousSortableRows<TModel>? _rows;
    private ITreeDataGridSelection? _selection;

    public MyFlatTreeDataGridSource(IEnumerable<TModel> items, ColumnList<TModel> columns)
    {
        _items = items;
        _itemsView = TreeDataGridItemsSourceView<TModel>.GetOrCreate(items);
        Columns = columns;
    }

    public ColumnList<TModel> Columns { get; }

    public ITreeDataGridCellSelectionModel<TModel>? CellSelection =>
        Selection as ITreeDataGridCellSelectionModel<TModel>;

    public ITreeDataGridRowSelectionModel<TModel>? RowSelection => Selection as ITreeDataGridRowSelectionModel<TModel>;

    public void Dispose()
    {
        _rows?.Dispose();
        GC.SuppressFinalize(this);
    }

    public IRows Rows => _rows ??= CreateRows();
    IColumns ITreeDataGridSource.Columns => Columns;

    public IEnumerable<TModel> Items
    {
        get => _items;
        set
        {
            if (_items != value)
            {
                _items = value;
                _itemsView = TreeDataGridItemsSourceView<TModel>.GetOrCreate(value);
                _rows?.SetItems(_itemsView);
                if (_selection is object)
                    _selection.Source = value;
                RaisePropertyChanged();
            }
        }
    }

    public ITreeDataGridSelection? Selection
    {
        get
        {
            if (_selection == null && !_isSelectionSet)
                _selection = new TreeDataGridRowSelectionModel<TModel>(this);
            return _selection;
        }
        set
        {
            if (_selection != value)
            {
                if (value?.Source != _items)
                    throw new InvalidOperationException("Selection source must be set to Items.");
                _selection = value;
                _isSelectionSet = true;
                RaisePropertyChanged();
            }
        }
    }

    IEnumerable<object> ITreeDataGridSource.Items => Items;
    public bool IsHierarchical => false;
    public bool IsSorted => _comparer is not null;

    public event Action? Sorted;

    void ITreeDataGridSource.DragDropRows(
        ITreeDataGridSource source,
        IEnumerable<IndexPath> indexes,
        IndexPath targetIndex,
        TreeDataGridRowDropPosition position,
        DragDropEffects effects)
    {
        if (effects != DragDropEffects.Move)
            throw new NotSupportedException("Only move is currently supported for drag/drop.");
        if (IsSorted)
            throw new NotSupportedException("Drag/drop is not supported on sorted data.");
        if (position == TreeDataGridRowDropPosition.Inside)
            throw new ArgumentException("Invalid drop position.", nameof(position));
        if (indexes.Any(x => x.Count != 1))
            throw new ArgumentException("Invalid source index.", nameof(indexes));
        if (targetIndex.Count != 1)
            throw new ArgumentException("Invalid target index.", nameof(targetIndex));
        if (_items is not IList<TModel> items)
            throw new InvalidOperationException("Items does not implement IList<T>.");

        if (position == TreeDataGridRowDropPosition.None)
            return;

        var ti = targetIndex[0];

        if (position == TreeDataGridRowDropPosition.After)
            ++ti;

        var sourceItems = new List<TModel>();

        foreach (var src in indexes.OrderByDescending(x => x))
        {
            var i = src[0];
            sourceItems.Add(items[i]);
            items.RemoveAt(i);

            if (i < ti)
                --ti;
        }

        for (var si = sourceItems.Count - 1; si >= 0; --si) items.Insert(ti++, sourceItems[si]);
    }

    bool ITreeDataGridSource.SortBy(IColumn? column, ListSortDirection direction)
    {
        if (column is IColumn<TModel> typedColumn)
        {
            if (!Columns.Contains(typedColumn))
                return true;

            var comparer = typedColumn.GetComparison(direction);

            if (comparer is not null)
            {
                _comparer = comparer is not null ? new FuncComparer<TModel>(comparer) : null;
                _rows?.Sort(_comparer);
                Sorted?.Invoke();
                foreach (var c in Columns)
                    c.SortDirection = c == column ? direction : null;
            }

            return true;
        }

        return false;
    }

    IEnumerable<object> ITreeDataGridSource.GetModelChildren(object model) => Enumerable.Empty<object>();

    private AnonymousSortableRows<TModel> CreateRows() => new(_itemsView, _comparer);

    internal class FuncComparer<T> : IComparer<T>
    {
        private readonly Comparison<T?> _func;

        public FuncComparer(Comparison<T?> func)
        {
            _func = func;
        }

        public int Compare(T? x, T? y) => _func(x, y);
    }
}
