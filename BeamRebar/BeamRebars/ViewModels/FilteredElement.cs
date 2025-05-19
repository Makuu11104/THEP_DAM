
namespace BeamRebar.BeamRebars.ViewModels
{
    internal class FilteredElement
    {
        private Document doc;

        public FilteredElement(Document doc)
        {
            this.doc = doc;
        }

        public object OfClass(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var collector = new FilteredElementCollector(doc);
            return collector.OfClass(type).ToElements();
        }
    }
}