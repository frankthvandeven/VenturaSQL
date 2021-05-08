using System.Collections.ObjectModel;
using System.Collections.Specialized;
using VenturaSQL;

namespace VenturaSQLStudio {
    public class ParameterCollection : ObservableCollection<ParameterItem>
    {
        private Project _owningproject;

        public ParameterCollection(Project owningproject)
        {
            _owningproject = owningproject;
        }

        public ParameterCollection Clone()
        {
            ParameterCollection temp = new ParameterCollection(_owningproject);

            foreach (ParameterItem item in this.Items)
                temp.Add(item.Clone());

            return temp;
        }

        public VenturaSqlSchema AsVenturaSqlSchema()
        {
            ColumnArrayBuilder builder = new ColumnArrayBuilder();

            foreach (ParameterItem item in this)
            {
                VenturaSqlColumn column = item.AsVenturaSqlColumn();
                builder.Add(column);
            }

            return new VenturaSqlSchema(builder);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            _owningproject?.SetModified();

            base.OnCollectionChanged(e);
        }
    }
}





#if DO_NOT_COMPILE

/// <summary>
/// Update the parameter list using the scanned_items list.
/// </summary>
public void UpdateCollection(List<string> scanned_items)
{
    // Disable items not found in scanned_items from Parameters list.
    // and also Enable items that are found!
    for (int x = this.Count - 1; x >= 0; x--)
    {
        string variable_name = this[x].Name;

        // Look up the current parameter item in the new list that was just scanned.
        string found_in_scanneditems = scanned_items.Find(a => a.ToLower() == variable_name.ToLower());

        if (found_in_scanneditems != null)
            this[x].Enabled = true;
        else
            this[x].Enabled = false;
    }

    // Add items to Parameters list.
    foreach (string variable_name in scanned_items)
    {
        ParameterItem found_in_parameterlist = this.FirstOrDefault(a => a.Name.ToLower() == variable_name.ToLower());

        if (found_in_parameterlist == null) // We need to append it.
        {
            ParameterItem item = new ParameterItem(_owningproject);
            item.Name = variable_name;
            item.Enabled = true;
            item.ProviderType = "Int";
            item.Input = true;
            item.Output = false;
            item.DesignValue = "";

            this.Add(item);
        }
    }

}

public int DisabledCount()
{
    int count = 0;

    foreach (ParameterItem item in this)
    {
        if (item.Enabled == false)
            count++;
    }

    return count;
}

public void RemoveAllDisabled()
{
    for (int x = this.Count - 1; x >= 0; x--)
    {
        if (this[x].Enabled == false)
            this.RemoveAt(x);
    }
}

public List<ParameterItem> GetEnabledParameters()
{
    List<ParameterItem> list = new List<ParameterItem>();

    foreach (ParameterItem item in this)
    {
        if (item.Enabled == true)
            list.Add(item);
    }

    return list;
}
#endif