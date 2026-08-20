using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleBrowser.Models
{
    public class HistoryItemModel
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DisplayTitle => Title.Length > 30 ? $"{Title[..30]}..." : Title;
        public DateTime DateVisited { get; set; } = DateTime.MinValue;
    }

    // Base node for TreeView items
    public interface ITreeNode
    {
        string Title { get; }
        IEnumerable<ITreeNode>? Children { get; }
    }

    // Leaf node: Individual history item
    public class HistoryItemNode(HistoryItemModel model) : ITreeNode
    {
        public HistoryItemModel Model { get; } = model;
        public string Title => Model.DisplayTitle;
        public IEnumerable<ITreeNode>? Children => null;
    }

    // Parent node: Group header by Date
    public class DateGroupNode(DateTime date, IEnumerable<HistoryItemModel> items) : ITreeNode
    {
        public DateTime Date { get; } = date;
        public string Title => Date.Equals(DateTime.Today) ? "Today" : Date.ToString("ddd, MMM d, yyyy");
        public IEnumerable<ITreeNode> Children { get; } = items.Select(item => new HistoryItemNode(item)).ToList();
    }
}
