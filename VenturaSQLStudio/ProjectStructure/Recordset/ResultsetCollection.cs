using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio {
    public class ResultsetCollection : ObservableCollection<ResultsetItem>
    {
        private Project _owningproject;

        public ResultsetCollection(Project owningproject)
        {
            _owningproject = owningproject;
        }

        public ResultsetCollection Clone()
        {
            ResultsetCollection temp = new ResultsetCollection(_owningproject);

            foreach (ResultsetItem item in this.Items)
                temp.Add(item.Clone());

            return temp;
        }

        /// <summary>
        /// Set the length of the collection of ResultsetItems.
        /// </summary>
        public void SetResultsetsLength(QueryInfo queryinfo)
        {
            void action()
            {
                int query_resultset_count = queryinfo.ResultSets.Count;

                if (this.Count < query_resultset_count)
                {
                    for (int i = this.Count; i < query_resultset_count; i++)
                    {
                        ResultsetItem item = new ResultsetItem(_owningproject, this.Count + 1, $"Resultset{i + 1}");
                        this.Add(item);
                    }
                }


                for (int i = 0; i < this.Count; i++)
                {
                    if (i < query_resultset_count)
                    {
                        this[i].Enabled = true;
                        this[i].GenerateReferencedTablesList(queryinfo.ResultSets[i]);
                    }
                    else
                    {
                        this[i].Enabled = false;
                        this[i].GenerateReferencedTablesList(null);
                    }
                }
            };

            Application.Current.Dispatcher.Invoke(action);
        }

        public void ResetResultsetNames()
        {
            for (int i = 0; i < this.Count; i++)
            {
                ResultsetItem item = this[i];
                item.ResultsetName = item.GetDefaultName();
            }
        }


        public int DisabledCount()
        {
            int count = 0;

            foreach (ResultsetItem item in this)
            {
                if (item.Enabled == false)
                    count++;
            }

            return count;
        }

        public void RealDeleteDisabled()
        {
            for (int i = this.Count - 1; i >= 0; i--)
                if (this[i].Enabled == false)
                    Application.Current.Dispatcher.Invoke(() => this.RemoveAt(i));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            _owningproject?.SetModified();

            base.OnCollectionChanged(e);
        }
    } // class
} // namespace
