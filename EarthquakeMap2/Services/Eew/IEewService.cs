using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EewLibrary;

namespace EarthquakeMap2.Services.Eew;

internal interface IEewService<T> where T : EEW
{
    public Task Start();
    public void UpdateTime(DateTime? dateTime);

    public event EventHandler<EewUpdatedEventArgs<T>> EewUpdatedForFirst;
    public event EventHandler<EewUpdatedEventArgs<T>> EewUpdatedForContinue;
}
